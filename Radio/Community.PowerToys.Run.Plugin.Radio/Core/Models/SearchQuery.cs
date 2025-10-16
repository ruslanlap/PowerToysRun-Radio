using System;
using System.Linq;

namespace Community.PowerToys.Run.Plugin.Radio.Core.Models
{
    /// <summary>
    /// Represents a parsed search query.
    /// </summary>
    public sealed record SearchQuery
    {
        private static readonly System.Collections.Generic.HashSet<string> COMMON_GENRES = new(StringComparer.OrdinalIgnoreCase)
        {
            "jazz","rock","pop","news","classical","electronic","electro","dance","house","trance","hiphop","hip-hop","rap","rnb","r&b","metal","ambient","chill","chillout","lofi","lo-fi","talk","sports","country","blues","reggae","folk","techno","dubstep","indie","retro","oldies","latin","salsa","kpop","k-pop","jpop","j-pop","anime"
        };

        /// <summary>
        /// The city to search for.
        /// </summary>
        public string City { get; init; } = string.Empty;

        /// <summary>
        /// Optional genre/tag filter.
        /// </summary>
        public string? Genre { get; init; }

        /// <summary>
        /// Special query type for favorites.
        /// </summary>
        public bool IsFavoritesQuery { get; init; }

        /// <summary>
        /// Gets whether this is a valid query (has city or is favorites).
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(City) || IsFavoritesQuery;

        /// <summary>
        /// Static instance for favorites query.
        /// </summary>
        public static SearchQuery Favorites { get; } = new SearchQuery { IsFavoritesQuery = true };

        /// <summary>
        /// Parses a search string into a SearchQuery.
        /// </summary>
        /// <param name="queryString">The query string to parse.</param>
        /// <returns>A SearchQuery object.</returns>
        /// <remarks>
        /// Parsing rules:
        /// - Empty/whitespace: Invalid query
        /// - "favorites" or "fav": Favorites query
        /// - "radio fav" or "radio favorites" also supported (when typed after keyword)
        /// - Single word: City only (unless common genre)
        /// - Two+ words: First word is city, second word is genre
        /// </remarks>
        public static SearchQuery Parse(string queryString)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return new SearchQuery();
            }

            var trimmed = queryString.Trim();

            // Check for favorites keyword
            if (trimmed.Equals("favorites", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("fav", StringComparison.OrdinalIgnoreCase))
            {
                return Favorites;
            }

            // Split into parts
            var parts = trimmed.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return new SearchQuery();
            }

            if (parts.Length == 1)
            {
                var single = parts[0];

                // Country code (UA) or common genre (jazz)
                if (single.Length == 2 && single.All(char.IsLetter))
                {
                    return new SearchQuery { City = single };
                }
                if (COMMON_GENRES.Contains(single))
                {
                    return new SearchQuery { City = string.Empty, Genre = single };
                }

                return new SearchQuery { City = single };
            }

            return new SearchQuery
            {
                City = parts[0],
                Genre = parts[1]
            };
        }

        /// <summary>
        /// Gets a cache key for this query.
        /// </summary>
        public string GetCacheKey()
        {
            if (IsFavoritesQuery)
            {
                return "favorites";
            }

            var city = City.ToLowerInvariant();
            var genre = Genre?.ToLowerInvariant() ?? string.Empty;
            return $"{city}|{genre}";
        }

        /// <summary>
        /// Returns a string representation of the query.
        /// </summary>
        public override string ToString()
        {
            if (IsFavoritesQuery)
            {
                return "Favorites";
            }

            return string.IsNullOrWhiteSpace(Genre) ? City : $"{City} {Genre}";
        }
    }
}
