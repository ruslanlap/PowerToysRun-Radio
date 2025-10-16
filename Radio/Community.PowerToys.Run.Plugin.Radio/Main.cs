using ManagedCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wox.Plugin;
using Community.PowerToys.Run.Plugin.Radio.Core.Services;
using Community.PowerToys.Run.Plugin.Radio.Core.Models;
using Community.PowerToys.Run.Plugin.Radio.Logging;

namespace Community.PowerToys.Run.Plugin.Radio
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public class Main : IPlugin, IContextMenu, IDisposable
    {
        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static string PluginID => "0CF5D160821B4F01A24D08E459AF3DC8";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "Radio";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "Search and play radio stations";

        private PluginInitContext Context { get; set; } = null!;

        private string IconPath { get; set; } = string.Empty;

        private bool Disposed { get; set; }

        private IRadioBrowserClient? _radioClient;
        private ILogger? _logger;
        private Storage.FavoriteService? _favorites;

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
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
                        IcoPath = IconPath,
                        Score = 100
                    }
                };
            }

            try
            {
                _logger?.LogInfo($"Query started: '{search}'");
                
                if (_radioClient == null)
                {
                    _logger?.LogError("Radio client is not initialized");
                    return new List<Result>
                    {
                        new Result
                        {
                            Title = "Plugin not initialized",
                            SubTitle = "Please restart PowerToys Run",
                            IcoPath = IconPath,
                            Score = 100
                        }
                    };
                }

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
                            IcoPath = IconPath,
                            Score = 100
                        }
                    };
                }

                // Handle favorites pseudo-query
                if (SearchQuery.Parse(search).IsFavoritesQuery && _favorites != null)
                {
                    var favs = _favorites.GetAll();
                    return favs.Select(Fac).ToList();
                }

                // Show all stations when user explicitly requests broader categories like 'radio ua', 'radio lviv', 'radio jazz'
                return stations.Select(Fac).ToList();
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
                        IcoPath = IconPath,
                        Score = 100
                    }
                };
            }
        }
        private Result Fac(RadioStation station)
        {
            return new Result
            {
                Title = station.Name,
                SubTitle = $"{station.CountryCode} • {station.Tags} • {station.Codec} {station.Bitrate}kbps",
                IcoPath = IconPath,
                ToolTipData = new ToolTipData(
                    station.Name,
                    $"Country: {station.CountryCode}\nTags: {station.Tags}\nCodec: {station.Codec} {station.Bitrate}kbps\nURL: {station.UrlResolved}"
                ),
                Action = _ =>
                {
                    // Default: open in Windows Media Player (or default media player)
                    return Player.PlayerLauncher.PlayInMediaPlayer(station, _logger);
                },
                ContextData = station,
                Score = 100
            };
        }


        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());

            try
            {
                _logger = new RedactedLogger("Radio");
                _favorites = new Storage.FavoriteService(_logger);
                var mirrorService = new MirrorDiscoveryService(_logger);
                _radioClient = new RadioBrowserClient(mirrorService, _logger);
                _logger.LogInfo("Radio plugin initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to initialize plugin: {ex.Message}");
            }
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is RadioStation station)
            {
                var isFav = _favorites?.IsFavorite(station.StationUuid) == true;

                var menus = new List<ContextMenuResult>
                {
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Play in Media Player (Enter)",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xE768",
                        AcceleratorKey = Key.Enter,
                        Action = _ => Player.PlayerLauncher.PlayInMediaPlayer(station, _logger),
                    },
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = isFav ? "Remove from favorites" : "Add to favorites",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = isFav ? "\xE74D" : "\xE734",
                        Action = _ =>
                        {
                            try
                            {
                                if (_favorites != null && !string.IsNullOrEmpty(station.StationUuid))
                                {
                                    if (isFav)
                                        _favorites.Remove(station.StationUuid);
                                    else
                                        _favorites.Add(station);
                                    return true;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError($"Failed to update favorites: {ex.Message}");
                            }
                            return false;
                        },
                    },
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Open in Browser",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xE774",
                        Action = _ => Player.PlayerLauncher.OpenInBrowser(station, _logger),
                    },
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy URL (Ctrl+C)",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xE8C8",
                        AcceleratorKey = Key.C,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ =>
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(station.UrlResolved))
                                {
                                    Clipboard.SetDataObject(station.UrlResolved);
                                    return true;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError($"Failed to copy URL: {ex.Message}");
                            }
                            return false;
                        },
                    }
                };

                return menus;
            }

            return new List<ContextMenuResult>();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            if (_radioClient is IDisposable disposable)
            {
                disposable.Dispose();
            }

            Disposed = true;
        }

        private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/radio.light.png" : "Images/radio.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
    }
}
