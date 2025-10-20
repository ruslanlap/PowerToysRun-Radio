using Community.PowerToys.Run.Plugin.Radio.Core;
using Community.PowerToys.Run.Plugin.Radio.Core.Models;
using Community.PowerToys.Run.Plugin.Radio.Core.Services;
using Community.PowerToys.Run.Plugin.Radio.Logging;
using ManagedCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wox.Plugin;

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
        private QueryHandler? _queryHandler;
        private ContextMenuFactory? _contextMenuFactory;

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            if (_queryHandler == null)
            {
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
            return _queryHandler.Query(query, IconPath);
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
                _queryHandler = new QueryHandler(_radioClient, _favorites, _logger);
                _contextMenuFactory = new ContextMenuFactory(_favorites, _logger);
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
            if(_contextMenuFactory == null)
            {
                return new List<ContextMenuResult>();
            }
            return _contextMenuFactory.Create(selectedResult);
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

        private void UpdateIconPath(Theme theme)
        {
            IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/radio.light.png" : "Images/radio.dark.png";
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
    }
}
