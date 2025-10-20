using Community.PowerToys.Run.Plugin.Radio.Core.Models;
using Community.PowerToys.Run.Plugin.Radio.Core.Services;
using Community.PowerToys.Run.Plugin.Radio.Logging;
using Community.PowerToys.Run.Plugin.Radio.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Radio.Core
{
    public class QueryHandler
    {
        private readonly IRadioBrowserClient _radioClient;
        private readonly ILogger _logger;
        private readonly FavoriteService _favorites;

        public QueryHandler(IRadioBrowserClient radioClient, FavoriteService favorites, ILogger logger)
        {
            _radioClient = radioClient;
            _favorites = favorites;
            _logger = logger;
        }

        public List<Result> Query(Query query, string iconPath)
        {
            var search = query.Search?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(search))
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Radio Browser",
                        SubTitle = "Type station name, city, or country to search",
                        IcoPath = iconPath,
                        Score = 100
                    }
                };
            }

            try
            {
                _logger?.LogInfo($"Query started: '{search}'");

                // Use Task.Run to properly handle async calls
                var stations = Task.Run(async () =>
                {
                    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30));
                    var searchQuery = SearchQuery.Parse(search);
                    try
                    {
                        return await _radioClient.SearchStationsAsync(searchQuery, cts.Token);
                    }
                    catch (RadioBrowserException)
                    {
                        _logger?.LogWarning("Retrying search with refreshed mirrors...");
                        return await _radioClient.SearchStationsAsync(searchQuery, CancellationToken.None);
                    }
                }).GetAwaiter().GetResult();

                _logger?.LogInfo($"Query completed: found {stations?.Count ?? 0} stations");

                if (stations == null || stations.Count == 0)
                {
                    return new List<Result>
                    {
                        new Result
                        {
                            Title = "No stations found",
                            SubTitle = $"Try different search terms (e.g., city name, country)",
                            IcoPath = iconPath,
                            Score = 100
                        }
                    };
                }

                // Handle favorites pseudo-query
                if (SearchQuery.Parse(search).IsFavoritesQuery && _favorites != null)
                {
                    var favs = _favorites.GetAll();
                    return favs.Select(station => ResultFactory.Create(station, iconPath)).ToList();
                }

                // Show all stations when user explicitly requests broader categories like 'radio ua', 'radio lviv', 'radio jazz'
                return stations.Select(station => ResultFactory.Create(station, iconPath)).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error searching stations: {ex.Message}", ex);
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Radio Browser Error",
                        SubTitle = $"All API servers failed. Check your internet connection or try again later.",
                        IcoPath = iconPath,
                        Score = 100
                    }
                };
            }
        }
    }
}
