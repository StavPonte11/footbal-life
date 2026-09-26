using System;
using System.IO;
using System.Linq;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class WorldDataLoaderTests
    {
        private static string GetContentDataDir()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "content", "data"));
        }

        [Fact]
        public void WorldDataLoader_LoadFromActualFiles_SucceedsWithAllClubsAndLeagues()
        {
            string dataDir = GetContentDataDir();
            if (!Directory.Exists(dataDir)) return; // Skip if run in environment without content dir

            var result = WorldDataLoader.LoadFromDirectory(dataDir);

            Assert.True(result.IsSuccess, $"Load failed: {string.Join("; ", result.Errors.Select(e => e.ToString()))}");
            Assert.NotNull(result.WorldState);
            Assert.True(result.WorldState.Leagues.Count >= 10, $"Expected >= 10 leagues, found {result.WorldState.Leagues.Count}");
            Assert.True(result.WorldState.Clubs.Count >= 50, $"Expected >= 50 clubs, found {result.WorldState.Clubs.Count}");

            // Verify specific club data integrity
            var nlr = result.WorldState.Clubs.Values.FirstOrDefault(c => c.ShortName == "NLR");
            Assert.NotNull(nlr);
            Assert.Equal("North London Red", nlr.Name);
            Assert.Equal("Ashburton Grove", nlr.Stadium.Name);
            Assert.Equal(88, nlr.ReputationRating);
        }

        [Fact]
        public void WorldDataLoader_DuplicateLeagueId_ReturnsValidationError()
        {
            string leaguesJson = """
            {
              "leagues": [
                { "id": "e1000000-0000-0000-0000-000000000001", "name": "League 1", "countryCode": "ENG", "tier": 1, "clubCount": 20, "matchdaysPerSeason": 38 },
                { "id": "e1000000-0000-0000-0000-000000000001", "name": "League 2", "countryCode": "ENG", "tier": 2, "clubCount": 20, "matchdaysPerSeason": 38 }
              ]
            }
            """;
            string clubsJson = """{ "clubs": [] }""";

            var result = WorldDataLoader.Load(clubsJson, leaguesJson);

            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Message.Contains("Duplicate league ID"));
        }

        [Fact]
        public void WorldDataLoader_DuplicateClubId_ReturnsValidationError()
        {
            string leaguesJson = """
            {
              "leagues": [
                { "id": "e1000000-0000-0000-0000-000000000001", "name": "League 1", "countryCode": "ENG", "tier": 1, "clubCount": 20, "matchdaysPerSeason": 38 }
              ]
            }
            """;
            string clubsJson = """
            {
              "clubs": [
                { "id": "c1000000-0000-0000-0000-000000000001", "name": "Club 1", "shortName": "C1", "leagueId": "e1000000-0000-0000-0000-000000000001", "reputationRating": 80, "facilityRating": 4, "tacticalStyle": "Possession" },
                { "id": "c1000000-0000-0000-0000-000000000001", "name": "Club 2", "shortName": "C2", "leagueId": "e1000000-0000-0000-0000-000000000001", "reputationRating": 75, "facilityRating": 3, "tacticalStyle": "Counter" }
              ]
            }
            """;

            var result = WorldDataLoader.Load(clubsJson, leaguesJson);

            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Message.Contains("Duplicate club ID"));
        }

        [Fact]
        public void WorldDataLoader_ClubReferencingMissingLeague_ReturnsValidationError()
        {
            string leaguesJson = """{ "leagues": [] }""";
            string clubsJson = """
            {
              "clubs": [
                { "id": "c1000000-0000-0000-0000-000000000001", "name": "Club 1", "shortName": "C1", "leagueId": "e9999999-9999-9999-9999-999999999999", "reputationRating": 80, "facilityRating": 4, "tacticalStyle": "Possession" }
              ]
            }
            """;

            var result = WorldDataLoader.Load(clubsJson, leaguesJson);

            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Message.Contains("does not exist in leagues dataset"));
        }

        [Fact]
        public void WorldDataLoader_InvalidReputation_ReturnsValidationError()
        {
            string leaguesJson = """
            {
              "leagues": [
                { "id": "e1000000-0000-0000-0000-000000000001", "name": "League 1", "countryCode": "ENG", "tier": 1, "clubCount": 20, "matchdaysPerSeason": 38 }
              ]
            }
            """;
            string clubsJson = """
            {
              "clubs": [
                { "id": "c1000000-0000-0000-0000-000000000001", "name": "Club 1", "shortName": "C1", "leagueId": "e1000000-0000-0000-0000-000000000001", "reputationRating": 150, "facilityRating": 4, "tacticalStyle": "Possession" }
              ]
            }
            """;

            var result = WorldDataLoader.Load(clubsJson, leaguesJson);

            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Message.Contains("Reputation rating"));
        }

        [Fact]
        public void WorldDataLoader_MalformedJson_ReturnsValidationError()
        {
            var result = WorldDataLoader.Load("not a json", "{ \"leagues\": [] }");
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Message.Contains("Failed to parse JSON"));
        }
    }
}
