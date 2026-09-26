using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Supported localized languages.
    /// </summary>
    public enum GameLanguage
    {
        English = 0,
        Spanish = 1,
        German = 2,
        French = 3,
        Italian = 4
    }

    /// <summary>
    /// Metadata descriptor for a localized language.
    /// </summary>
    public readonly struct LanguageInfo : IEquatable<LanguageInfo>
    {
        public GameLanguage Language { get; }
        public string Code { get; }
        public string DisplayName { get; }
        public string NativeName { get; }
        public string FlagEmoji { get; }

        public LanguageInfo(GameLanguage language, string code, string displayName, string nativeName, string flagEmoji)
        {
            Language = language;
            Code = code ?? throw new ArgumentNullException(nameof(code));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            NativeName = nativeName ?? throw new ArgumentNullException(nameof(nativeName));
            FlagEmoji = flagEmoji ?? string.Empty;
        }

        public static readonly IReadOnlyList<LanguageInfo> SupportedLanguages = new[]
        {
            new LanguageInfo(GameLanguage.English, "en", "English", "English", "🇬🇧"),
            new LanguageInfo(GameLanguage.Spanish, "es", "Spanish", "Español", "🇪🇸"),
            new LanguageInfo(GameLanguage.German, "de", "German", "Deutsch", "🇩🇪"),
            new LanguageInfo(GameLanguage.French, "fr", "French", "Français", "🇫🇷"),
            new LanguageInfo(GameLanguage.Italian, "it", "Italian", "Italiano", "🇮🇹")
        };

        public static LanguageInfo FromLanguage(GameLanguage language)
        {
            foreach (var info in SupportedLanguages)
            {
                if (info.Language == language)
                    return info;
            }
            return SupportedLanguages[0];
        }

        public static LanguageInfo FromCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return SupportedLanguages[0];

            string normalized = code.Trim().ToLowerInvariant();
            if (normalized.Contains("-"))
                normalized = normalized.Split('-')[0];

            foreach (var info in SupportedLanguages)
            {
                if (info.Code.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                    return info;
            }
            return SupportedLanguages[0];
        }

        public bool Equals(LanguageInfo other) => Language == other.Language && Code == other.Code;
        public override bool Equals(object? obj) => obj is LanguageInfo other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Language, Code);
        public override string ToString() => $"{DisplayName} ({NativeName}) [{Code}]";
    }
}
