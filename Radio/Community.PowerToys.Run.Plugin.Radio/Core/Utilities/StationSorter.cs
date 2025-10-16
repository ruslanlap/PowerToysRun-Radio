using System.Collections.Generic;
using System.Linq;
using Community.PowerToys.Run.Plugin.Radio.Core.Models;

namespace Community.PowerToys.Run.Plugin.Radio.Core.Utilities
{
    /// <summary>
    /// Provides deterministic sorting for radio stations.
    /// </summary>
    public static class StationSorter
    {
        /// <summary>
        /// Sorts stations deterministically by quality and popularity.
        /// </summary>
        /// <param name="stations">The stations to sort.</param>
        /// <returns>Sorted list of stations.</returns>
        /// <remarks>
        /// Sort order:
        /// 1. Working stations first (lastcheckok DESC)
        /// 2. Most popular (clickcount DESC)
        /// 3. Highest quality (bitrate DESC)
        /// 4. Alphabetical (name ASC)
        /// 5. UUID (stationuuid ASC) for determinism
        /// </remarks>
        public static List<RadioStation> Sort(List<RadioStation> stations)
        {
            return stations
                .Where(s => s.IsWorking) // Only working stations
                .OrderByDescending(s => s.LastCheckOk) // Working first
                .ThenByDescending(s => s.ClickCount) // Popular first
                .ThenByDescending(s => s.Bitrate) // High quality first
                .ThenBy(s => s.Name) // Alphabetical
                .ThenBy(s => s.StationUuid) // Deterministic tie-breaker
                .ToList();
        }
    }
}
