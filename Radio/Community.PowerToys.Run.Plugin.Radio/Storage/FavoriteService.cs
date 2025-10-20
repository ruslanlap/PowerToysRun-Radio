using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Community.PowerToys.Run.Plugin.Radio.Core.Models;
using Community.PowerToys.Run.Plugin.Radio.Logging;

namespace Community.PowerToys.Run.Plugin.Radio.Storage
{
    public sealed class FavoriteService
    {
        private readonly string _filePath;
        private readonly ILogger _logger;
        private readonly object _lock = new();

        public FavoriteService(ILogger logger)
        {
            _logger = logger;
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dir = Path.Combine(appData, "Microsoft", "PowerToys", "PowerToys Run", "Radio");
            Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, "favorites.json");
        }

        public List<RadioStation> GetAll()
        {
            try
            {
                if (!File.Exists(_filePath)) return new List<RadioStation>();
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<RadioStation>>(json) ?? new List<RadioStation>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load favorites", ex);
                return new List<RadioStation>();
            }
        }

        public bool IsFavorite(string stationUuid)
        {
            if (string.IsNullOrWhiteSpace(stationUuid)) return false;
            return GetAll().Any(s => string.Equals(s.StationUuid, stationUuid, StringComparison.OrdinalIgnoreCase));
        }

        public void Add(RadioStation station)
        {
            if (station == null || string.IsNullOrWhiteSpace(station.StationUuid)) return;
            lock (_lock)
            {
                var items = GetAll();
                if (items.Any(s => string.Equals(s.StationUuid, station.StationUuid, StringComparison.OrdinalIgnoreCase))) return;
                items.Add(station);
                Save(items);
            }
        }

        public void Remove(string stationUuid)
        {
            if (string.IsNullOrWhiteSpace(stationUuid)) return;
            lock (_lock)
            {
                var items = GetAll();
                items = items.Where(s => !string.Equals(s.StationUuid, stationUuid, StringComparison.OrdinalIgnoreCase)).ToList();
                Save(items);
            }
        }

        private void Save(List<RadioStation> items)
        {
            try
            {
                var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save favorites", ex);
            }
        }
    }
}
