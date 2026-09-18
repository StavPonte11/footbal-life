using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Loads and validates static content datasets (clubs, leagues) from JSON into a coherent WorldState.
    /// Catches structural and relational errors before the simulation engine starts.
    /// </summary>
    public static class WorldDataLoader
    {
        public static WorldDataLoadResult Load(string clubsJson, string leaguesJson, Season? defaultSeason = null)
        {
            var errors = new List<ValidationError>();
            var leagues = new Dictionary<Guid, League>();
            var clubs = new Dictionary<Guid, Club>();

            // 1. Parse and validate leagues
            try
            {
                using var leaguesDoc = JsonDocument.Parse(leaguesJson);
                var root = leaguesDoc.RootElement;
                if (!root.TryGetProperty("leagues", out var leaguesArr) || leaguesArr.ValueKind != JsonValueKind.Array)
                {
                    errors.Add(new ValidationError("Leagues", "root", "Missing 'leagues' array in JSON root."));
                }
                else
                {
                    foreach (var elem in leaguesArr.EnumerateArray())
                    {
                        ParseLeague(elem, leagues, errors);
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ValidationError("Leagues", "json", $"Failed to parse JSON: {ex.Message}"));
            }

            // 2. Parse and validate clubs
            try
            {
                using var clubsDoc = JsonDocument.Parse(clubsJson);
                var root = clubsDoc.RootElement;
                if (!root.TryGetProperty("clubs", out var clubsArr) || clubsArr.ValueKind != JsonValueKind.Array)
                {
                    errors.Add(new ValidationError("Clubs", "root", "Missing 'clubs' array in JSON root."));
                }
                else
                {
                    foreach (var elem in clubsArr.EnumerateArray())
                    {
                        ParseClub(elem, clubs, leagues, errors);
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ValidationError("Clubs", "json", $"Failed to parse JSON: {ex.Message}"));
            }

            if (errors.Count > 0)
            {
                return WorldDataLoadResult.Failure(errors.AsReadOnly());
            }

            // 3. Construct WorldState
            var season = defaultSeason ?? CreateDefaultSeason(leagues.Keys, clubs.Keys);
            var world = WorldState.CreateEmpty(season);

            foreach (var league in leagues.Values)
            {
                world = world.WithLeague(league);
            }

            foreach (var club in clubs.Values)
            {
                world = world.WithClub(club);
            }

            return WorldDataLoadResult.Success(world);
        }

        public static WorldDataLoadResult LoadFromDirectory(string directoryPath, Season? defaultSeason = null)
        {
            string clubsPath = Path.Combine(directoryPath, "clubs.json");
            string leaguesPath = Path.Combine(directoryPath, "leagues.json");

            if (!File.Exists(clubsPath))
            {
                return WorldDataLoadResult.Failure(new[] { new ValidationError("Clubs", "file", $"File not found: {clubsPath}") });
            }

            if (!File.Exists(leaguesPath))
            {
                return WorldDataLoadResult.Failure(new[] { new ValidationError("Leagues", "file", $"File not found: {leaguesPath}") });
            }

            string clubsJson = File.ReadAllText(clubsPath);
            string leaguesJson = File.ReadAllText(leaguesPath);

            return Load(clubsJson, leaguesJson, defaultSeason);
        }

        private static void ParseLeague(JsonElement elem, Dictionary<Guid, League> leagues, List<ValidationError> errors)
        {
            if (!elem.TryGetProperty("id", out var idProp) || !Guid.TryParse(idProp.GetString(), out var id))
            {
                errors.Add(new ValidationError("League", "id", "Missing or invalid GUID id."));
                return;
            }

            if (leagues.ContainsKey(id))
            {
                errors.Add(new ValidationError("League", "id", "Duplicate league ID detected.", id.ToString()));
                return;
            }

            string name = elem.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "";
            string country = elem.TryGetProperty("countryCode", out var cProp) ? cProp.GetString() ?? "" : "";
            int tier = elem.TryGetProperty("tier", out var tProp) ? tProp.GetInt32() : 1;
            int clubCount = elem.TryGetProperty("clubCount", out var ccProp) ? ccProp.GetInt32() : 20;
            int matchdays = elem.TryGetProperty("matchdaysPerSeason", out var mdProp) ? mdProp.GetInt32() : 38;
            int promotion = elem.TryGetProperty("promotionSlots", out var pProp) ? pProp.GetInt32() : 0;
            int relegation = elem.TryGetProperty("relegationSlots", out var rProp) ? rProp.GetInt32() : 3;

            try
            {
                var league = new League(id, name, country, tier, clubCount, matchdays, promotion, relegation);
                leagues[id] = league;
            }
            catch (Exception ex)
            {
                errors.Add(new ValidationError("League", name, ex.Message));
            }
        }

        private static void ParseClub(
            JsonElement elem,
            Dictionary<Guid, Club> clubs,
            Dictionary<Guid, League> leagues,
            List<ValidationError> errors)
        {
            if (!elem.TryGetProperty("id", out var idProp) || !Guid.TryParse(idProp.GetString(), out var id))
            {
                errors.Add(new ValidationError("Club", "id", "Missing or invalid GUID id."));
                return;
            }

            if (clubs.ContainsKey(id))
            {
                errors.Add(new ValidationError("Club", "id", "Duplicate club ID detected.", id.ToString()));
                return;
            }

            string name = elem.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "";
            string shortName = elem.TryGetProperty("shortName", out var snProp) ? snProp.GetString() ?? "" : "";

            if (!elem.TryGetProperty("leagueId", out var lProp) || !Guid.TryParse(lProp.GetString(), out var leagueId))
            {
                errors.Add(new ValidationError("Club", "leagueId", "Missing or invalid GUID leagueId.", name));
                return;
            }

            if (!leagues.ContainsKey(leagueId))
            {
                errors.Add(new ValidationError("Club", "leagueId", $"Referenced league '{leagueId}' does not exist in leagues dataset.", name));
            }

            int reputation = elem.TryGetProperty("reputationRating", out var repProp) ? repProp.GetInt32() : 50;
            int facility = elem.TryGetProperty("facilityRating", out var facProp) ? facProp.GetInt32() : 3;

            TacticalIdentity tactical = TacticalIdentity.Possession;
            if (elem.TryGetProperty("tacticalStyle", out var tacProp) && !Enum.TryParse(tacProp.GetString(), out tactical))
            {
                errors.Add(new ValidationError("Club", "tacticalStyle", $"Invalid tactical style: {tacProp.GetString()}"));
            }

            ClubFinances finances = new ClubFinances(100_000m, 10_000_000m);
            if (elem.TryGetProperty("finances", out var finElem))
            {
                decimal wage = finElem.TryGetProperty("weeklyWageBudget", out var wProp) ? wProp.GetDecimal() : 0m;
                decimal transfer = finElem.TryGetProperty("transferBudget", out var trProp) ? trProp.GetDecimal() : 0m;
                try
                {
                    finances = new ClubFinances(wage, transfer);
                }
                catch (Exception ex)
                {
                    errors.Add(new ValidationError("ClubFinances", name, ex.Message));
                }
            }

            ClubStadium? stadium = null;
            if (elem.TryGetProperty("stadium", out var stadElem))
            {
                string stadName = stadElem.TryGetProperty("name", out var sn) ? sn.GetString() ?? "" : "";
                int capacity = stadElem.TryGetProperty("capacity", out var cap) ? cap.GetInt32() : 25_000;
                int pitch = stadElem.TryGetProperty("pitchQuality", out var pq) ? pq.GetInt32() : 3;
                try
                {
                    stadium = new ClubStadium(stadName, capacity, pitch);
                }
                catch (Exception ex)
                {
                    errors.Add(new ValidationError("ClubStadium", name, ex.Message));
                }
            }

            ClubVisuals? visuals = null;
            if (elem.TryGetProperty("visuals", out var visElem))
            {
                string logo = visElem.TryGetProperty("logoAssetKey", out var lp) ? lp.GetString() ?? "" : "";
                var home = ParseKit(visElem, "homeKit", "#FFFFFF", "#000000");
                var away = ParseKit(visElem, "awayKit", "#000000", "#FFFFFF");
                visuals = new ClubVisuals(logo, home, away);
            }

            ClubFanbase? fanbase = null;
            if (elem.TryGetProperty("fanbase", out var fanElem))
            {
                int fans = fanElem.TryGetProperty("supporterCount", out var sc) ? sc.GetInt32() : 100_000;
                int loyalty = fanElem.TryGetProperty("loyaltyRating", out var lr) ? lr.GetInt32() : 70;
                int exp = fanElem.TryGetProperty("expectationRating", out var er) ? er.GetInt32() : 70;
                try
                {
                    fanbase = new ClubFanbase(fans, loyalty, exp);
                }
                catch (Exception ex)
                {
                    errors.Add(new ValidationError("ClubFanbase", name, ex.Message));
                }
            }

            ClubBoard? board = null;
            if (elem.TryGetProperty("board", out var boardElem))
            {
                string owner = boardElem.TryGetProperty("ownerName", out var on) ? on.GetString() ?? "" : "";
                BoardPatience patience = BoardPatience.Balanced;
                if (boardElem.TryGetProperty("patience", out var patProp)) Enum.TryParse(patProp.GetString(), out patience);
                int ambition = boardElem.TryGetProperty("financialAmbition", out var fa) ? fa.GetInt32() : 3;
                try
                {
                    board = new ClubBoard(owner, patience, ambition);
                }
                catch (Exception ex)
                {
                    errors.Add(new ValidationError("ClubBoard", name, ex.Message));
                }
            }

            ClubHistory? history = null;
            if (elem.TryGetProperty("history", out var histElem))
            {
                int founded = histElem.TryGetProperty("foundedYear", out var fy) ? fy.GetInt32() : 1900;
                int titles = histElem.TryGetProperty("leagueTitles", out var lt) ? lt.GetInt32() : 0;
                int cups = histElem.TryGetProperty("domesticCups", out var dc) ? dc.GetInt32() : 0;
                int euro = histElem.TryGetProperty("continentalTrophies", out var ct) ? ct.GetInt32() : 0;
                try
                {
                    history = new ClubHistory(founded, titles, cups, euro);
                }
                catch (Exception ex)
                {
                    errors.Add(new ValidationError("ClubHistory", name, ex.Message));
                }
            }

            try
            {
                var club = new Club(
                    id,
                    name,
                    shortName,
                    leagueId,
                    reputation,
                    finances,
                    facility,
                    tactical,
                    managerId: null,
                    stadium: stadium,
                    visuals: visuals,
                    fanbase: fanbase,
                    board: board,
                    history: history);

                clubs[id] = club;
            }
            catch (Exception ex)
            {
                errors.Add(new ValidationError("Club", name, ex.Message));
            }
        }

        private static KitColors ParseKit(JsonElement parent, string propName, string defaultPrimary, string defaultSecondary)
        {
            if (parent.TryGetProperty(propName, out var kitElem))
            {
                string primary = kitElem.TryGetProperty("primaryHex", out var p) ? p.GetString() ?? defaultPrimary : defaultPrimary;
                string secondary = kitElem.TryGetProperty("secondaryHex", out var s) ? s.GetString() ?? defaultSecondary : defaultSecondary;
                string? trim = kitElem.TryGetProperty("trimHex", out var t) ? t.GetString() : null;
                return new KitColors(primary, secondary, trim);
            }
            return new KitColors(defaultPrimary, defaultSecondary);
        }

        private static Season CreateDefaultSeason(IEnumerable<Guid> leagueIds, IEnumerable<Guid> clubIds)
        {
            var leagueId = System.Linq.Enumerable.FirstOrDefault(leagueIds);
            var table = LeagueTable.Create(leagueId, clubIds);
            return new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), table, new List<Matchday>());
        }
    }
}
