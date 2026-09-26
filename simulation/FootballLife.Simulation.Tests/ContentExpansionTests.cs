using System;
using System.IO;
using System.Linq;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class ContentExpansionTests
    {
        private static string GetContentDataDir()
        {
            string baseDir = AppContext.BaseDirectory;
            string candidate = Path.Combine(baseDir, "..", "..", "..", "..", "..", "content", "data");
            if (Directory.Exists(candidate)) return Path.GetFullPath(candidate);
            candidate = Path.GetFullPath("content/data");
            if (Directory.Exists(candidate)) return candidate;
            return candidate;
        }

        [Fact]
        public void ContentExpansion_EventsJson_ContainsOver100ValidLifeEvents()
        {
            string eventsPath = Path.Combine(GetContentDataDir(), "events.json");
            Assert.True(File.Exists(eventsPath), $"events.json not found at: {eventsPath}");

            var result = LifeEventDataLoader.LoadFromFile(eventsPath);
            Assert.True(result.IsSuccess, $"Failed loading events: {string.Join("; ", result.Errors)}");
            Assert.True(result.Events.Count >= 100, $"Expected >= 100 events, found {result.Events.Count}");

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var categories = new HashSet<EventCategory>();

            foreach (var ev in result.Events)
            {
                Assert.True(ids.Add(ev.Id), $"Duplicate event ID found: {ev.Id}");
                Assert.False(string.IsNullOrWhiteSpace(ev.Title), $"Empty title in event: {ev.Id}");
                Assert.False(string.IsNullOrWhiteSpace(ev.Description), $"Empty description in event: {ev.Id}");
                Assert.True(ev.Choices.Count >= 2, $"Event {ev.Id} must have >= 2 choices");
                Assert.True(ev.Weight > 0, $"Event {ev.Id} must have weight > 0");

                categories.Add(ev.Category);

                foreach (var choice in ev.Choices)
                {
                    Assert.False(string.IsNullOrWhiteSpace(choice.Id), $"Empty choice id in event {ev.Id}");
                    Assert.False(string.IsNullOrWhiteSpace(choice.Text), $"Empty choice text in event {ev.Id}");
                }
            }

            Assert.Contains(EventCategory.LockerRoom, categories);
            Assert.Contains(EventCategory.Media, categories);
            Assert.Contains(EventCategory.Commercial, categories);
            Assert.Contains(EventCategory.Training, categories);
            Assert.Contains(EventCategory.Family, categories);
            Assert.Contains(EventCategory.Lifestyle, categories);
        }

        [Fact]
        public void ContentExpansion_ClubsJson_ContainsOver50ClubsWithValidLeagues()
        {
            string dir = GetContentDataDir();
            var result = WorldDataLoader.LoadFromDirectory(dir);
            Assert.True(result.IsSuccess, $"Failed loading world data: {string.Join("; ", result.Errors.Select(e => e.ToString()))}");
            Assert.NotNull(result.WorldState);

            var clubs = result.WorldState.Clubs.Values.ToList();
            Assert.True(clubs.Count >= 50, $"Expected >= 50 clubs, found {clubs.Count}");

            var leagueIds = result.WorldState.Leagues.Keys.ToHashSet();

            foreach (var club in clubs)
            {
                Assert.True(leagueIds.Contains(club.LeagueId), $"Club '{club.Name}' references missing league '{club.LeagueId}'");
                Assert.InRange(club.ReputationRating, 1, 100);
                Assert.InRange(club.FacilityRating, 1, 5);
                Assert.NotNull(club.Stadium);
                Assert.True(club.Stadium.Capacity >= 5000, $"Club {club.Name} stadium capacity unexpectedly low: {club.Stadium.Capacity}");
                Assert.True(club.Finances.WeeklyWageBudget > 0, $"Club {club.Name} has invalid wage budget");
                Assert.True(club.Finances.TransferBudget > 0, $"Club {club.Name} has invalid transfer budget");
            }
        }

        [Fact]
        public void ContentExpansion_LeaguesJson_ContainsOver10LeaguesAcrossMultipleTiers()
        {
            string dir = GetContentDataDir();
            var result = WorldDataLoader.LoadFromDirectory(dir);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.WorldState);

            var leagues = result.WorldState.Leagues.Values.ToList();
            Assert.True(leagues.Count >= 10, $"Expected >= 10 leagues, found {leagues.Count}");

            var tiers = leagues.Select(l => l.Tier).Distinct().ToList();
            Assert.Contains(1, tiers);
            Assert.Contains(2, tiers);

            var countries = leagues.Select(l => l.CountryCode).Distinct().ToList();
            Assert.True(countries.Count >= 4, $"Expected at least 4 countries represented, found {countries.Count}");
        }
    }
}
