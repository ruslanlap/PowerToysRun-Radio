using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Community.PowerToys.Run.Plugin.Radio.Core.Models;
using Community.PowerToys.Run.Plugin.Radio.Logging;

namespace Community.PowerToys.Run.Plugin.Radio.Player
{
    internal static class PlayerLauncher
    {
        public static bool PlayInMediaPlayer(RadioStation station, ILogger? logger)
        {
            try
            {
                if (station == null || string.IsNullOrWhiteSpace(station.UrlResolved)) return false;

                // Create a temporary .m3u to force opening in a media player
                var dir = Path.Combine(Path.GetTempPath(), "PTRadio");
                Directory.CreateDirectory(dir);
                var safeName = SanitizeFileName(string.IsNullOrWhiteSpace(station.Name) ? "station" : station.Name);
                var m3uPath = Path.Combine(dir, safeName + ".m3u");

                var m3u = new StringBuilder();
                m3u.AppendLine("#EXTM3U");
                m3u.AppendLine(station.UrlResolved);
                File.WriteAllText(m3uPath, m3u.ToString());

                logger?.LogInfo($"Launching media player with M3U: {m3uPath}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = m3uPath,
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError("Failed to launch media player", ex);
                return false;
            }
        }

        public static bool OpenInBrowser(RadioStation station, ILogger? logger)
        {
            try
            {
                if (station == null || string.IsNullOrWhiteSpace(station.UrlResolved)) return false;
                logger?.LogInfo($"Opening in browser: {station.UrlResolved}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = station.UrlResolved,
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError("Failed to open in browser", ex);
                return false;
            }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name.Length > 80 ? name[..80] : name;
        }
    }
}
