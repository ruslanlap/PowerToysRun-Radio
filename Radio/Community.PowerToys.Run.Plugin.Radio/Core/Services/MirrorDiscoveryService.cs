using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Community.PowerToys.Run.Plugin.Radio.Logging;

namespace Community.PowerToys.Run.Plugin.Radio.Core.Services
{
    /// <summary>
    /// Discovers Radio Browser API mirrors.
    /// </summary>
    public sealed class MirrorDiscoveryService
    {
        // List of known working mirrors (updated with multiple fallbacks)
        private static readonly List<string> KNOWN_MIRRORS = new List<string>
        {
            "https://api.radio-browser.info", // aggregator
            "https://de1.api.radio-browser.info",
            "https://de2.api.radio-browser.info",
            "https://de3.api.radio-browser.info",
            "https://nl1.api.radio-browser.info",
            "https://nl2.api.radio-browser.info",
            "https://us1.api.radio-browser.info",
            "https://gb1.api.radio-browser.info",
            "https://at1.api.radio-browser.info"
        };

        private readonly ILogger _logger;
        private readonly List<string> _workingMirrors = new List<string>();
        private DateTime _lastDiscovery = DateTime.MinValue;
        private readonly TimeSpan _cacheLifetime = TimeSpan.FromHours(1);
        private readonly object _lock = new object();

        public MirrorDiscoveryService(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the list of available API mirrors.
        /// </summary>
        public async Task<List<string>> GetMirrorsAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_workingMirrors.Any() && DateTime.UtcNow - _lastDiscovery < _cacheLifetime)
                {
                    _logger.LogDebug($"Returning cached mirrors: {_workingMirrors.Count} mirrors");
                    return new List<string>(_workingMirrors);
                }
            }

            try
            {
                _logger.LogInfo("Testing available API mirrors...");
                var workingMirrors = new List<string>();

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    MaxConnectionsPerServer = 10,
                    UseProxy = false,
                    AllowAutoRedirect = true
                };

                using var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
                httpClient.DefaultRequestHeaders.Add("User-Agent", "PTR-Radio/1.0");
                httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                
                foreach (var mirror in KNOWN_MIRRORS)
                {
                    try
                    {
                        _logger.LogDebug($"Testing mirror: {mirror}");
                        var response = await httpClient.GetAsync($"{mirror}/json/stats", cancellationToken);
                        if (response.IsSuccessStatusCode)
                        {
                            workingMirrors.Add(mirror);
                            _logger.LogInfo($"✓ Mirror is working: {mirror}");
                        }
                        else
                        {
                            _logger.LogDebug($"Mirror returned {response.StatusCode}: {mirror}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug($"Mirror failed: {mirror} - {ex.Message}");
                    }
                }

                if (workingMirrors.Any())
                {
                    lock (_lock)
                    {
                        _workingMirrors.Clear();
                        _workingMirrors.AddRange(workingMirrors);
                        _lastDiscovery = DateTime.UtcNow;
                    }

                    _logger.LogInfo($"Found {workingMirrors.Count} working API mirrors");
                    return workingMirrors;
                }
                else
                {
                    _logger.LogWarning("No working mirrors found, using defaults");
                    return new List<string>(KNOWN_MIRRORS);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to test mirrors", ex);
                return new List<string>(KNOWN_MIRRORS);
            }
        }


        /// <summary>
        /// Gets a random mirror from the available list.
        /// </summary>
        public string GetRandomMirror()
        {
            lock (_lock)
            {
                if (!_workingMirrors.Any())
                {
                    _logger.LogDebug("No mirrors cached, using default");
                    return KNOWN_MIRRORS[0];
                }

                var mirror = _workingMirrors[Random.Shared.Next(_workingMirrors.Count)];
                _logger.LogDebug($"Selected random mirror: {mirror}");
                return mirror;
            }
        }
    }
}
