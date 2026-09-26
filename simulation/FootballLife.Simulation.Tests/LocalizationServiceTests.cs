using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class LocalizationServiceTests
    {
        [Fact]
        public void LocalizationService_ResolvesEnglishAndFallbacks()
        {
            var service = new LocalizationService();
            service.CurrentLanguage = GameLanguage.English;

            // Known embedded key
            string welcome = service.GetText("tutorial.step.welcome.title");
            Assert.Equal("Welcome to Football Life", welcome);

            // Positional formatting
            string counter = service.GetText("tutorial.step_counter", 1, 7);
            Assert.Equal("Step 1 of 7", counter);

            // Missing key returns key itself
            string missing = service.GetText("non.existent.key.xyz");
            Assert.Equal("non.existent.key.xyz", missing);
        }

        [Fact]
        public void LocalizationService_SwitchesLanguagesAndFallsBack()
        {
            var service = new LocalizationService();

            service.LoadCatalog(GameLanguage.Spanish, new Dictionary<string, string>
            {
                ["common.confirm"] = "Confirmar",
                ["common.cancel"] = "Cancelar"
            });

            service.CurrentLanguage = GameLanguage.Spanish;
            Assert.Equal("Confirmar", service.GetText("common.confirm"));
            Assert.Equal("Cancelar", service.GetText("common.cancel"));

            // Key exists in English embedded fallback but not in Spanish custom catalog
            string fallback = service.GetText("tutorial.step.welcome.title");
            Assert.Equal("Welcome to Football Life", fallback);
        }

        [Fact]
        public void LocalizationService_FiresLanguageChangedEvent()
        {
            var service = new LocalizationService();
            GameLanguage detectedLang = GameLanguage.English;
            int eventFiredCount = 0;

            service.OnLanguageChanged += lang =>
            {
                detectedLang = lang;
                eventFiredCount++;
            };

            service.CurrentLanguage = GameLanguage.German;
            Assert.Equal(GameLanguage.German, detectedLang);
            Assert.Equal(1, eventFiredCount);

            // Setting same language should not fire event
            service.CurrentLanguage = GameLanguage.German;
            Assert.Equal(1, eventFiredCount);
        }

        [Fact]
        public void LocalizationService_SubstitutesNamedTokens()
        {
            var service = new LocalizationService();
            service.LoadCatalog(GameLanguage.English, new Dictionary<string, string>
            {
                ["dialogue.welcome_player"] = "Hello {playerName}, welcome to {clubName}!"
            });

            var tokens = new Dictionary<string, string>
            {
                ["playerName"] = "Marcus Vance",
                ["clubName"] = "Arsenal"
            };

            string result = service.GetTextWithTokens("dialogue.welcome_player", tokens);
            Assert.Equal("Hello Marcus Vance, welcome to Arsenal!", result);
        }

        [Fact]
        public void LocalizationService_LoadsCatalogsFromDisk_AndMaintainsParity()
        {
            // Locate content directory
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string contentDir = Path.GetFullPath(Path.Combine(basePath, "../../../../../content/data/localization"));

            Assert.True(Directory.Exists(contentDir), $"Localization directory not found at: {contentDir}");

            var service = new LocalizationService();
            service.LoadFromDirectory(contentDir);

            // Read the raw json files for parity verification
            string enPath = Path.Combine(contentDir, "en.json");
            Assert.True(File.Exists(enPath), "en.json must exist");

            var enCatalog = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(enPath))!;
            Assert.True(enCatalog.Count >= 80, $"Expected >= 80 English keys, got {enCatalog.Count}");

            string[] otherLangs = { "es", "de", "fr", "it" };

            foreach (var langCode in otherLangs)
            {
                string langPath = Path.Combine(contentDir, $"{langCode}.json");
                Assert.True(File.Exists(langPath), $"{langCode}.json must exist");

                var catalog = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(langPath))!;
                Assert.Equal(enCatalog.Count, catalog.Count);

                foreach (var kvp in enCatalog)
                {
                    Assert.True(catalog.ContainsKey(kvp.Key),
                        $"Language '{langCode}' is missing key: '{kvp.Key}'");
                    Assert.False(string.IsNullOrWhiteSpace(catalog[kvp.Key]),
                        $"Language '{langCode}' has empty translation for: '{kvp.Key}'");
                }
            }
        }
    }
}
