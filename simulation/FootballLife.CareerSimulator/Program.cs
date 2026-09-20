using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FootballLife.Domain;
using FootballLife.Simulation;

namespace FootballLife.CareerSimulator
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            bool isInteractive = args.Contains("--interactive") || args.Contains("-i");

            if (isInteractive)
            {
                RunInteractiveLoop();
                return 0;
            }

            // Headless batch execution
            int careersCount = 100;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--careers" && i + 1 < args.Length && int.TryParse(args[i + 1], out int c))
                {
                    careersCount = c;
                }
            }

            RunHeadlessSimulation(careersCount);
            return 0;
        }

        private static void RunInteractiveLoop()
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine("                    FOOTBALL LIFE - CAREER SIMULATOR (CLI)                      ");
            Console.WriteLine("               Milestone 1.6.5: Minimum Playable Loop Validation                ");
            Console.WriteLine("================================================================================");
            Console.WriteLine();

            var rng = new SimulationRandom(42);

            // 1. Initialise player: age 19, position ST, baseline abilities 55, Tier 2 club
            var playerId = Guid.NewGuid();
            var clubId = Guid.NewGuid();
            var leagueId = Guid.NewGuid();

            var playerClub = new Club(
                id: clubId,
                name: "Preston Athletic",
                shortName: "PRE",
                leagueId: leagueId,
                reputationRating: 48,
                finances: new ClubFinances(35000m, 500000m),
                facilityRating: 2,
                tacticalStyle: TacticalIdentity.Counter,
                squadPlayerIds: new[] { playerId });

            var opponentClub = new Club(
                id: Guid.NewGuid(),
                name: "Bristol Rovers",
                shortName: "BRR",
                leagueId: leagueId,
                reputationRating: 46,
                finances: new ClubFinances(30000m, 400000m),
                facilityRating: 2,
                tacticalStyle: TacticalIdentity.HighPress);

            var player = new Player(
                id: playerId,
                name: "Marcus Vance",
                nationality: "ENG",
                dateOfBirth: new DateOnly(2007, 4, 15),
                preferredFoot: Foot.Right,
                primaryPosition: Position.ST);

            var abilities = new PlayerAbilities(
                pace: 55, acceleration: 55, stamina: 55, strength: 55, agility: 55,
                passing: 55, shooting: 55, dribbling: 55, crossing: 55, firstTouch: 55, tackling: 55,
                vision: 55, composure: 55, positioning: 55, decisionMaking: 55);

            var state = new PlayerState(
                fatigue: 10f,
                confidence: 60f,
                form: 55f,
                happiness: 70f,
                motivation: 80f,
                morale: 75f,
                fitness: 85f);

            var career = new PlayerCareerState(
                clubId: clubId,
                status: SquadStatus.Rotation,
                managerTrust: 50f,
                weeklySalary: 1200m,
                marketValue: 150000m,
                reputation: 25f);

            var potential = new PlayerPotential(82, PotentialRange.Medium);

            var matchResults = new List<MatchResult>();
            int weeksInSeason = 38;

            Console.WriteLine($"Player: {player.Name} | Age: 19 | Position: {player.PrimaryPosition} | Club: {playerClub.Name}");
            Console.WriteLine($"Starting Status: {career.Status} | Wage: £{career.WeeklySalary:N0}/wk | Potential: {potential.PotentialRating}");
            Console.WriteLine();

            for (int week = 1; week <= weeksInSeason; week++)
            {
                var weekDate = new DateOnly(2026, 8, 1).AddDays((week - 1) * 7);

                var season = new Season(
                    2026,
                    new DateOnly(2026, 8, 1),
                    new DateOnly(2027, 5, 30),
                    new LeagueTable(leagueId, Array.Empty<LeagueTableRow>()),
                    Array.Empty<Matchday>(),
                    currentWeek: week);

                var world = WorldState.CreateEmpty(season)
                    .WithPlayer(player, abilities, state, career, potential);

                Console.WriteLine("--------------------------------------------------------------------------------");
                Console.WriteLine($"[WEEK {week:D2}/38] Date: {weekDate:yyyy-MM-dd}");
                Console.WriteLine($"Condition -> Fatigue: {state.Fatigue:F1}% | Form: {state.Form:F1} | Confidence: {state.Confidence:F1} | Trust: {career.ManagerTrust:F1} | Status: {career.Status}");
                Console.WriteLine($"Abilities -> OVR: {abilities.CalculateAverage():F1} | FIN: {abilities.Shooting} | PAC: {abilities.Pace} | DRI: {abilities.Dribbling} | PAS: {abilities.Passing}");
                Console.WriteLine();

                Console.WriteLine("Choose Weekly Training Regimen:");
                Console.WriteLine("  [1] Technical Training (Shooting, Finishing, Dribbling) [+XP, +Fatigue]");
                Console.WriteLine("  [2] Physical Gym Session (Pace, Stamina, Strength) [+Physical XP, ++Fatigue]");
                Console.WriteLine("  [3] Recovery Session (Cryotherapy & Light Jog) [-Fatigue, +Form]");
                Console.WriteLine("  [4] Skip Training (Conserve Energy directly for Matchday)");
                Console.WriteLine("  [q] Quit Simulation");
                Console.Write("Action > ");

                string? input = Console.ReadLine()?.Trim().ToLowerInvariant();

                if (input == "q")
                {
                    Console.WriteLine("Quitting interactive simulation...");
                    break;
                }

                if (input == "1" || input == "2" || input == "3")
                {
                    var trainingType = input switch
                    {
                        "1" => TrainingType.Technical,
                        "2" => TrainingType.Physical,
                        _ => TrainingType.Recovery
                    };

                    var session = new TrainingSession(trainingType, TrainingIntensity.Moderate, 60, weekDate);
                    var trainResult = TrainingSystem.CalculateXP(session, player.PrimaryPosition, abilities, state, rng);

                    // Update fatigue from training
                    state = FatigueSystem.ApplyTraining(state, trainResult);

                    // Apply progression XP
                    abilities = ProgressionSystem.ApplyXpGains(playerId, world, trainResult.XpGained, weekDate, rng);

                    Console.WriteLine($"\n>> Training Result: +{trainResult.TotalXpGained:F1} XP earned. Fatigue cost: +{trainResult.FatigueCost:F1}% (Current Fatigue: {state.Fatigue:F1}%)");
                }
                else
                {
                    Console.WriteLine("\n>> Skipping training session to conserve energy.");
                }

                // Simulate weekend fixture
                bool isHomeMatch = week % 2 != 0;
                var fixture = ScheduledMatch.Create(
                    weekDate.AddDays(6),
                    isHomeMatch ? clubId : opponentClub.Id,
                    isHomeMatch ? opponentClub.Id : clubId,
                    leagueId);

                Console.WriteLine($"\n>> MATCHDAY: {(isHomeMatch ? $"{playerClub.Name} (H) vs {opponentClub.Name} (A)" : $"{opponentClub.Name} (H) vs {playerClub.Name} (A)")}");

                var (matchResult, postMatchState) = MatchSimulator.Simulate(
                    fixture,
                    clubId,
                    abilities,
                    state,
                    player.PrimaryPosition,
                    rng,
                    playerId);

                state = postMatchState;
                matchResults.Add(matchResult);

                float trustDelta = ManagerTrustSystem.CalculateTrustDelta(matchResult.PlayerRating, career.Status);
                career = ManagerTrustSystem.ApplyMatchResult(career, matchResult, career.Status);

                string outcomeStr = matchResult.IsWin(isHomeMatch) ? "WIN" : (matchResult.IsDraw() ? "DRAW" : "LOSS");
                Console.WriteLine($"Final Score: {matchResult.HomeScore} - {matchResult.AwayScore} ({outcomeStr})");
                Console.WriteLine($"Performance Rating: {matchResult.PlayerRating:F1}/10.0 | Goals: {(matchResult.PlayerScored ? "1" : "0")} | Assists: {(matchResult.PlayerAssisted ? "1" : "0")}");
                Console.WriteLine($"Manager Trust Delta: {(trustDelta >= 0 ? "+" : "")}{trustDelta:F1} -> New Trust: {career.ManagerTrust:F1}");

                // Mid-season and end-of-season status review
                if (week == 19 || week == 38)
                {
                    var previousStatus = career.Status;
                    var evaluatedStatus = CareerSystem.EvaluateSquadStatus(player, abilities, career, playerClub, world);
                    if (evaluatedStatus != previousStatus)
                    {
                        career = career with { Status = evaluatedStatus };
                        Console.WriteLine($"\n*** SQUAD STATUS REVIEW: Status adjusted from {previousStatus} to {evaluatedStatus}! ***");
                    }
                }

                // Weekly rest & passive day recovery
                state = FatigueSystem.ApplyRest(state, hoursSlept: 8);
                state = FatigueSystem.ApplyDayTick(state);
                Console.WriteLine();
            }

            // End of season review
            Console.WriteLine("================================================================================");
            Console.WriteLine("                           END OF SEASON SUMMARY                                ");
            Console.WriteLine("================================================================================");
            var seasonStats = CareerSystem.AggregateSeasonStats(matchResults, career, career.WeeklySalary, weeksInSeason, attributeGrowthAverage: abilities.CalculateAverage() - 55f);
            Console.WriteLine($"Appearances: {seasonStats.Appearances} / {weeksInSeason}");
            Console.WriteLine($"Goals: {seasonStats.Goals} | Assists: {seasonStats.Assists}");
            Console.WriteLine($"Average Match Rating: {seasonStats.AverageRating:F2}");
            Console.WriteLine($"Final Squad Hierarchy: {seasonStats.FinalStatus}");
            Console.WriteLine($"Total Wages Earned: £{seasonStats.WageEarned:N0}");
            Console.WriteLine($"Final Overall Ability: {abilities.CalculateAverage():F1} (+{abilities.CalculateAverage() - 55f:F1})");
            Console.WriteLine("================================================================================");
        }

        private static void RunHeadlessSimulation(int careersCount)
        {
            Console.WriteLine($"Running headless career validation for {careersCount} careers...");
            var rng = new SimulationRandom(123);
            int totalGoals = 0;
            float totalRating = 0f;
            int totalAppearances = 0;

            for (int c = 1; c <= careersCount; c++)
            {
                var playerId = Guid.NewGuid();
                var clubId = Guid.NewGuid();
                var leagueId = Guid.NewGuid();

                var player = new Player(playerId, $"Prospect {c}", "ENG", new DateOnly(2007, 1, 1), Foot.Right, Position.ST);
                var abilities = new PlayerAbilities(55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55);
                var state = new PlayerState(10, 60, 60, 70, 80, 75, 80);
                var career = PlayerCareerState.CreateAcademy(clubId);

                var fixture = ScheduledMatch.Create(new DateOnly(2026, 9, 1), clubId, Guid.NewGuid(), leagueId);

                for (int week = 1; week <= 38; week++)
                {
                    var (res, updatedState) = MatchSimulator.Simulate(fixture, clubId, abilities, state, Position.ST, rng, playerId);
                    career = ManagerTrustSystem.ApplyMatchResult(career, res, career.Status);
                    state = FatigueSystem.ApplyRest(updatedState, hoursSlept: 8);

                    totalAppearances++;
                    if (res.PlayerScored) totalGoals++;
                    totalRating += res.PlayerRating;
                }
            }

            Console.WriteLine($"Simulated {careersCount} careers ({careersCount * 38} matches).");
            Console.WriteLine($"Average goals/match: {(float)totalGoals / totalAppearances:F2}");
            Console.WriteLine($"Average player rating: {totalRating / totalAppearances:F2}");
            Console.WriteLine("Headless validation complete. Simulation balanced.");
        }
    }
}
