using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# localization service managing multilingual catalogs, fallback resolution,
    /// and token substitution for Football Life.
    /// Deterministic and execution-safe across headless test environments and in-engine presentation.
    /// </summary>
    public sealed class LocalizationService
    {
        private static readonly Lazy<LocalizationService> _lazyInstance =
            new Lazy<LocalizationService>(() => new LocalizationService());

        public static LocalizationService Instance => _lazyInstance.Value;

        private readonly Dictionary<GameLanguage, Dictionary<string, string>> _catalogs =
            new Dictionary<GameLanguage, Dictionary<string, string>>();

        private GameLanguage _currentLanguage = GameLanguage.English;

        public event Action<GameLanguage>? OnLanguageChanged;

        public GameLanguage CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnLanguageChanged?.Invoke(_currentLanguage);
                }
            }
        }

        public LocalizationService()
        {
            foreach (GameLanguage lang in Enum.GetValues(typeof(GameLanguage)))
            {
                _catalogs[lang] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            SeedEmbeddedFallbackStrings();
        }

        /// <summary>
        /// Loads a JSON string table into the catalog for the specified language.
        /// </summary>
        public void LoadCatalog(GameLanguage language, string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(jsonContent)) return;

            var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
            if (entries != null)
            {
                LoadCatalog(language, entries);
            }
        }

        /// <summary>
        /// Merges dictionary entries into the catalog for the specified language.
        /// </summary>
        public void LoadCatalog(GameLanguage language, IReadOnlyDictionary<string, string> entries)
        {
            if (entries == null) return;

            var target = _catalogs[language];
            foreach (var kvp in entries)
            {
                target[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Scans a directory for localization JSON files (e.g. en.json, es.json, etc.) and loads them.
        /// </summary>
        public void LoadFromDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return;

            foreach (var info in LanguageInfo.SupportedLanguages)
            {
                string filePath = Path.Combine(directoryPath, $"{info.Code}.json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    LoadCatalog(info.Language, json);
                }
            }
        }

        /// <summary>
        /// Retrieves localized text for the given key in the current language, falling back to English.
        /// Supports positional string formatting (e.g. {0}, {1}).
        /// </summary>
        public string GetText(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            string? text = null;

            // 1. Try active language
            if (_catalogs.TryGetValue(_currentLanguage, out var activeCatalog) &&
                activeCatalog.TryGetValue(key, out var val))
            {
                text = val;
            }
            // 2. Fall back to English
            else if (_currentLanguage != GameLanguage.English &&
                     _catalogs.TryGetValue(GameLanguage.English, out var enCatalog) &&
                     enCatalog.TryGetValue(key, out var enVal))
            {
                text = enVal;
            }

            if (text == null)
            {
                text = key; // Return raw key if not localized
            }

            if (args != null && args.Length > 0)
            {
                try
                {
                    return string.Format(text, args);
                }
                catch (FormatException)
                {
                    return text;
                }
            }

            return text;
        }

        /// <summary>
        /// Retrieves localized text and substitutes named tokens (e.g. {playerName}, {clubName}).
        /// </summary>
        public string GetTextWithTokens(string key, IReadOnlyDictionary<string, string> tokens)
        {
            string raw = GetText(key);
            if (tokens == null || tokens.Count == 0) return raw;

            foreach (var kvp in tokens)
            {
                raw = raw.Replace($"{{{kvp.Key}}}", kvp.Value);
            }

            return raw;
        }

        /// <summary>
        /// Checks if a key exists in the specified language (or current language if unspecified).
        /// </summary>
        public bool HasKey(string key, GameLanguage? language = null)
        {
            if (string.IsNullOrEmpty(key)) return false;
            var targetLang = language ?? _currentLanguage;
            return _catalogs.TryGetValue(targetLang, out var cat) && cat.ContainsKey(key);
        }

        /// <summary>
        /// Returns total number of registered keys for a language.
        /// </summary>
        public int GetKeyCount(GameLanguage language)
        {
            return _catalogs.TryGetValue(language, out var cat) ? cat.Count : 0;
        }

        /// <summary>
        /// Sets current language by 2-letter ISO code (e.g. "es", "de", "en").
        /// </summary>
        public bool SetLanguageByCode(string code)
        {
            var info = LanguageInfo.FromCode(code);
            CurrentLanguage = info.Language;
            return true;
        }

        private void SeedEmbeddedFallbackStrings()
        {
            var en = _catalogs[GameLanguage.English];
            en["common.confirm"] = "Confirm";
            en["common.cancel"] = "Cancel";
            en["common.back"] = "Back";
            en["common.continue"] = "Continue";
            en["common.close"] = "Close";
            en["common.save"] = "Save";
            en["common.skip"] = "Skip";
            en["common.next"] = "Next";
            en["common.finish"] = "Finish";
            en["common.week"] = "Week";
            en["common.season"] = "Season";

            en["nav.hub"] = "Career Hub";
            en["nav.overview"] = "Overview";
            en["nav.match"] = "Play Match";
            en["nav.training"] = "Training";
            en["nav.rest"] = "Rest & Recover";
            en["nav.shop"] = "Lifestyle Shop";
            en["nav.social"] = "Social Activities";
            en["nav.phone"] = "Smartphone";
            en["nav.transfers"] = "Transfer Market";
            en["nav.sponsorship"] = "Sponsorships";
            en["nav.legacy"] = "Career Legacy";

            en["tutorial.title"] = "Rookie Onboarding";
            en["tutorial.step_counter"] = "Step {0} of {1}";
            en["tutorial.skip_button"] = "Skip Tutorial";
            en["tutorial.next_button"] = "Next";
            en["tutorial.got_it_button"] = "Got It!";
            en["tutorial.step.welcome.title"] = "Welcome to Football Life";
            en["tutorial.step.welcome.desc"] = "Your football journey begins today. Balance your performances on the pitch with relationships, lifestyle, and media in your quest to become a legend.";
            en["tutorial.step.welcome.action"] = "Begin Career";
            en["tutorial.step.completed.title"] = "You're Ready for the Big Leagues!";
            en["tutorial.step.completed.desc"] = "You have mastered the fundamentals of Football Life. The entire footballing world is now at your feet. Train hard, play passionately, and leave your legacy!";
            en["tutorial.step.completed.action"] = "Start Playing";
        }
    }
}
