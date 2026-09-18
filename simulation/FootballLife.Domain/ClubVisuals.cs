using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Hex-encoded color palette for a kit (primary, secondary, trim).
    /// Used by presentation layer to tint 3D player models and UI elements without engine coupling.
    /// </summary>
    public sealed record KitColors
    {
        public string PrimaryHex { get; init; }
        public string SecondaryHex { get; init; }
        public string TrimHex { get; init; }

        public KitColors(string primaryHex, string secondaryHex, string? trimHex = null)
        {
            if (string.IsNullOrWhiteSpace(primaryHex))
            {
                throw new ArgumentException("Primary hex color cannot be null or whitespace.", nameof(primaryHex));
            }

            if (string.IsNullOrWhiteSpace(secondaryHex))
            {
                throw new ArgumentException("Secondary hex color cannot be null or whitespace.", nameof(secondaryHex));
            }

            PrimaryHex = NormalizeHex(primaryHex);
            SecondaryHex = NormalizeHex(secondaryHex);
            TrimHex = string.IsNullOrWhiteSpace(trimHex) ? "#FFFFFF" : NormalizeHex(trimHex!);
        }

        private static string NormalizeHex(string hex)
        {
            string clean = hex.Trim().ToUpperInvariant();
            return clean.StartsWith("#") ? clean : $"#{clean}";
        }
    }

    /// <summary>
    /// Bundles visual identifiers for a club (badge/logo key, home kit, away kit).
    /// </summary>
    public sealed record ClubVisuals
    {
        /// <summary>
        /// Asset address / sprite lookup identifier for the club badge in presentation layer.
        /// </summary>
        public string LogoAssetKey { get; init; }

        /// <summary>
        /// Color configuration for home match kit.
        /// </summary>
        public KitColors HomeKit { get; init; }

        /// <summary>
        /// Color configuration for away match kit.
        /// </summary>
        public KitColors AwayKit { get; init; }

        public ClubVisuals(string logoAssetKey, KitColors homeKit, KitColors awayKit)
        {
            if (string.IsNullOrWhiteSpace(logoAssetKey))
            {
                throw new ArgumentException("Logo asset key cannot be null or whitespace.", nameof(logoAssetKey));
            }

            LogoAssetKey = logoAssetKey.Trim();
            HomeKit = homeKit ?? throw new ArgumentNullException(nameof(homeKit));
            AwayKit = awayKit ?? throw new ArgumentNullException(nameof(awayKit));
        }

        public static ClubVisuals Default(string shortName) =>
            new ClubVisuals(
                logoAssetKey: $"badges/{shortName.ToLowerInvariant()}",
                homeKit: new KitColors("#1E40AF", "#FFFFFF", "#93C5FD"),
                awayKit: new KitColors("#F3F4F6", "#1F2937", "#9CA3AF")
            );
    }
}
