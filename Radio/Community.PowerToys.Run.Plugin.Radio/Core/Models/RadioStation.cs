using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Community.PowerToys.Run.Plugin.Radio.Core.Models
{
    /// <summary>
    /// Represents a radio station from the Radio Browser API.
    /// </summary>
    public sealed record RadioStation
    {
        /// <summary>
        /// Unique identifier for the station.
        /// </summary>
        [JsonPropertyName("stationuuid")]
        public string StationUuid { get; init; } = string.Empty;

        /// <summary>
        /// Station name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Resolved stream URL (ready to play).
        /// </summary>
        [JsonPropertyName("url_resolved")]
        public string UrlResolved { get; init; } = string.Empty;

        /// <summary>
        /// Station favicon/logo URL.
        /// </summary>
        [JsonPropertyName("favicon")]
        public string Favicon { get; init; } = string.Empty;

        /// <summary>
        /// Homepage URL.
        /// </summary>
        [JsonPropertyName("homepage")]
        public string Homepage { get; init; } = string.Empty;

        /// <summary>
        /// Audio codec (e.g., MP3, AAC, OGG).
        /// </summary>
        [JsonPropertyName("codec")]
        public string Codec { get; init; } = string.Empty;

        /// <summary>
        /// Bitrate in kbps.
        /// </summary>
        [JsonPropertyName("bitrate")]
        public int Bitrate { get; init; }

        /// <summary>
        /// Country code (e.g., US, GB, JP).
        /// </summary>
        [JsonPropertyName("countrycode")]
        public string CountryCode { get; init; } = string.Empty;

        /// <summary>
        /// Number of times the station has been clicked.
        /// </summary>
        [JsonPropertyName("clickcount")]
        public int ClickCount { get; init; }

        /// <summary>
        /// Whether the station is working (1 = working, 0 = broken).
        /// </summary>
        [JsonPropertyName("lastcheckok")]
        public int LastCheckOk { get; init; }

        /// <summary>
        /// Comma-separated list of tags/genres.
        /// </summary>
        [JsonPropertyName("tags")]
        public string Tags { get; init; } = string.Empty;

        /// <summary>
        /// Gets the display name (fallback to "Unknown Station" if empty).
        /// </summary>
        [JsonIgnore]
        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "Unknown Station" : Name;

        /// <summary>
        /// Gets the display codec in uppercase (fallback to "Unknown").
        /// </summary>
        [JsonIgnore]
        public string DisplayCodec => string.IsNullOrWhiteSpace(Codec) ? "Unknown" : Codec.ToUpperInvariant();

        /// <summary>
        /// Gets whether the station is currently working.
        /// </summary>
        [JsonIgnore]
        public bool IsWorking => LastCheckOk == 1;

        /// <summary>
        /// Gets the tags as an array.
        /// </summary>
        [JsonIgnore]
        public string[] TagList => Tags
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToArray();

        /// <summary>
        /// Gets the truncated name for display (max 60 characters).
        /// </summary>
        [JsonIgnore]
        public string TruncatedName => DisplayName.Length > 60
            ? DisplayName.Substring(0, 57) + "..."
            : DisplayName;
    }
}
