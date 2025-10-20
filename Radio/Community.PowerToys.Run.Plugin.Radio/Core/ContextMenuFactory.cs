using Community.PowerToys.Run.Plugin.Radio.Core.Models;
using Community.PowerToys.Run.Plugin.Radio.Logging;
using Community.PowerToys.Run.Plugin.Radio.Storage;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Radio.Core
{
    public class ContextMenuFactory
    {
        private readonly FavoriteService _favorites;
        private readonly ILogger _logger;

        public ContextMenuFactory(FavoriteService favorites, ILogger logger)
        {
            _favorites = favorites;
            _logger = logger;
        }

        public List<ContextMenuResult> Create(Result selectedResult)
        {
            if (selectedResult.ContextData is RadioStation station)
            {
                var isFav = _favorites?.IsFavorite(station.StationUuid) == true;

                var menus = new List<ContextMenuResult>
                {
                    new ContextMenuResult
                    {
                        PluginName = "Radio",
                        Title = "Play in Media Player (Enter)",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xE768",
                        AcceleratorKey = Key.Enter,
                        Action = _ => Player.PlayerLauncher.PlayInMediaPlayer(station, _logger),
                    },
                    new ContextMenuResult
                    {
                        PluginName = "Radio",
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
                        PluginName = "Radio",
                        Title = "Open in Browser",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xE774",
                        Action = _ => Player.PlayerLauncher.OpenInBrowser(station, _logger),
                    },
                    new ContextMenuResult
                    {
                        PluginName = "Radio",
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
    }
}
