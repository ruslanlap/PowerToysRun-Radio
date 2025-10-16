using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Community.PowerToys.Run.Plugin.Radio.Core.Models;
using Community.PowerToys.Run.Plugin.Radio.Logging;

namespace Community.PowerToys.Run.Plugin.Radio.Core.Services
{
    /// <summary>
    /// HTTP client for the Radio Browser API with improved search.
    /// </summary>
    public sealed class RadioBrowserClient : IRadioBrowserClient, IDisposable
    {
        private const string USER_AGENT = "PTR-Radio/1.0";
        // Some mirrors enforce CORS on browser; on desktop we can use both

        private const int TIMEOUT_SECONDS = 15;
        private static readonly string[] BASE_URLS = new[]
        {
            "https://api.radio-browser.info",
            "https://de1.api.radio-browser.info",
            "https://de2.api.radio-browser.info",
            "https://de3.api.radio-browser.info",
            "https://nl1.api.radio-browser.info",
            "https://nl2.api.radio-browser.info",
            "https://us1.api.radio-browser.info",
            "https://gb1.api.radio-browser.info",
            "https://at1.api.radio-browser.info"
        };

        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly MirrorDiscoveryService _mirrorService;
        private bool _disposed;

        public RadioBrowserClient(MirrorDiscoveryService mirrorService, ILogger _logger)
        {
            this._logger = _logger;
            _mirrorService = mirrorService;

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS)
            };

            _httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            
            _logger.LogInfo("RadioBrowser client initialized");
        }

        public async Task<List<RadioStation>> SearchStationsAsync(
            SearchQuery query,
            CancellationToken cancellationToken = default)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var searchTerm = query.City;
            var genre = query.Genre;
            
            _logger.LogInfo($"Searching for: '{searchTerm}' genre: '{genre}'");

            // Try dynamic mirrors first, then fall back to static list
            var mirrors = await _mirrorService.GetMirrorsAsync(cancellationToken);
            var allBases = mirrors.Concat(BASE_URLS).Distinct().OrderBy(_ => Random.Shared.Next()).ToList();

            foreach (var baseUrl in allBases)
            {
                try
                {
                    var url = BuildSearchUrl(baseUrl, searchTerm, genre, 50);
                    _logger.LogDebug($"Trying: {url}");

                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.ConnectionClose = false;
                    var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                    if ((int)response.StatusCode == 502)
                    {
                        // try json/stations/byname as fallback since some mirrors changed routing
                        var fallbackUrl = $"{baseUrl}/json/stations/byname/{Uri.EscapeDataString(searchTerm)}?hidebroken=true&order=clickcount&reverse=true&limit=50";
                        _logger.LogWarning($"502 on search, trying fallback: {fallbackUrl}");
                        response = await _httpClient.GetAsync(fallbackUrl, cancellationToken);
                    }

                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    var stations = JsonSerializer.Deserialize<List<RadioStation>>(json) ?? new List<RadioStation>();

                    _logger.LogInfo($"✓ Found {stations.Count} stations");
                    return stations;
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning($"Timeout contacting: {baseUrl}");
                }
                catch (Exception ex)
                {
                    if (!NetworkErrors.IsDnsOrGatewayError(ex))
                    {
                        _logger.LogWarning($"Failed: {baseUrl} - {ex.Message}");
                    }
                    else
                    {
                        _logger.LogWarning($"Mirror problem: {baseUrl} - {ex.Message}");
                    }
                }
            }

            throw new RadioBrowserException("All API servers failed");
        }

        public async Task<List<RadioStation>> GetTopStationsAsync(
            int limit = 20,
            CancellationToken cancellationToken = default)
        {
            return await SearchStationsAsync(SearchQuery.Parse("top"),  cancellationToken);
        }

        public async Task<List<RadioStation>> GetStationsByCountryAsync(
            string countryCode,
            CancellationToken cancellationToken = default)
        {
            return await SearchStationsAsync(SearchQuery.Parse(countryCode), cancellationToken);
        }

        private string BuildSearchUrl(string baseUrl, string searchTerm, string? genre, int limit)
        {
            var query = new StringBuilder($"{baseUrl}/json/stations/search?");

            var isCountryCode = !string.IsNullOrWhiteSpace(searchTerm) && searchTerm.Length == 2 && searchTerm.All(char.IsLetter);

            // Build base filter by country or by name, only if we have a search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                if (isCountryCode)
                {
                    query.Append($"countrycode={Uri.EscapeDataString(searchTerm.ToUpper())}");
                }
                else
                {
                    query.Append($"name={Uri.EscapeDataString(searchTerm)}");
                }
            }

            // Always allow extra tag filter when provided (also supports tag-only searches)
            if (!string.IsNullOrWhiteSpace(genre))
            {
                if (!query.ToString().EndsWith("?"))
                {
                    query.Append("&");
                }
                query.Append($"tag={Uri.EscapeDataString(genre)}");
            }

            query.Append("&hidebroken=true");
            query.Append("&order=clickcount");
            query.Append("&reverse=true");
            query.Append($"&limit={limit}");

            return query.ToString();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _httpClient?.Dispose();
            _disposed = true;
        }

        private sealed record ClickResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("ok")]
            public bool Ok { get; init; }

            [System.Text.Json.Serialization.JsonPropertyName("url")]
            public string? Url { get; init; }
        }
    }

    /// <summary>
    /// Exception thrown when Radio Browser API calls fail.
    /// </summary>
    public sealed class RadioBrowserException : Exception
    {
        public RadioBrowserException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }

    internal static class NetworkErrors
    {
        public static bool IsDnsOrGatewayError(Exception ex)
        {
            var msg = ex.Message ?? string.Empty;
            return msg.Contains("No such host is known", StringComparison.OrdinalIgnoreCase)
                   || msg.Contains("502", StringComparison.OrdinalIgnoreCase)
                   || msg.Contains("Bad Gateway", StringComparison.OrdinalIgnoreCase);
        }
    }
}
