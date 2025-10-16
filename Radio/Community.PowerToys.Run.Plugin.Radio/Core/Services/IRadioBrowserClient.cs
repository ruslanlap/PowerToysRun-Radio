using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Community.PowerToys.Run.Plugin.Radio.Core.Models;

namespace Community.PowerToys.Run.Plugin.Radio.Core.Services
{
    /// <summary>
    /// Client for the Radio Browser API.
    /// </summary>
    public interface IRadioBrowserClient : IDisposable
    {
        /// <summary>
        /// Searches for radio stations matching the query.
        /// </summary>
        Task<List<RadioStation>> SearchStationsAsync(
            SearchQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets top stations by votes.
        /// </summary>
        Task<List<RadioStation>> GetTopStationsAsync(
            int limit = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets stations by country code.
        /// </summary>
        Task<List<RadioStation>> GetStationsByCountryAsync(
            string countryCode,
            CancellationToken cancellationToken = default);
    }
}
