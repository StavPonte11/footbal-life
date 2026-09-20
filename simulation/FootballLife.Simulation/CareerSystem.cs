using System;
using System.Collections.Generic;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Simulation system for long-term career progression, squad status hierarchy evaluation,
    /// and end-of-season statistics aggregation.
    /// </summary>
    public static class CareerSystem
    {
        /// <summary>
        /// Evaluates a player's squad status within a club based on manager trust, relative reputation,
        /// and positional squad competition. Status can only change by at most one tier per evaluation.
        /// </summary>
        public static SquadStatus EvaluateSquadStatus(
            Player player,
            PlayerAbilities abilities,
            PlayerCareerState careerState,
            Club club,
            WorldState world)
        {
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));
            if (careerState is null) throw new ArgumentNullException(nameof(careerState));
            if (club is null) throw new ArgumentNullException(nameof(club));
            if (world is null) throw new ArgumentNullException(nameof(world));

            // Factor 1: Manager Trust (40% weight) [0..100]
            float trustFactor = careerState.ManagerTrust;

            // Factor 2: Player reputation relative to club reputation (30% weight) [0..100]
            float repRatio = (float)careerState.Reputation / Math.Max(1, club.ReputationRating);
            float repFactor = Math.Min(100f, Math.Max(0f, repRatio * 80f));

            // Factor 3: Positional competition within club (30% weight) [0..100]
            float playerOverall = abilities.CalculateAverage();
            int betterPositionPlayers = 0;

            if (club.SquadPlayerIds != null)
            {
                foreach (var teammateId in club.SquadPlayerIds)
                {
                    if (teammateId == player.Id) continue;

                    if (world.Players.TryGetValue(teammateId, out var teammate) &&
                        teammate.PrimaryPosition == player.PrimaryPosition &&
                        world.Abilities.TryGetValue(teammateId, out var teammateAbilities))
                    {
                        if (teammateAbilities.CalculateAverage() > playerOverall)
                        {
                            betterPositionPlayers++;
                        }
                    }
                }
            }

            float competitionFactor = betterPositionPlayers switch
            {
                0 => 100f,
                1 => 70f,
                2 => 40f,
                _ => 15f
            };

            // Composite score [0..100]
            float composite = (trustFactor * 0.40f) + (repFactor * 0.30f) + (competitionFactor * 0.30f);

            SquadStatus targetStatus = composite switch
            {
                >= 81f => SquadStatus.KeyPlayer,
                >= 66f => SquadStatus.Starter,
                >= 46f => SquadStatus.Rotation,
                >= 31f => SquadStatus.Bench,
                >= 16f => SquadStatus.Reserve,
                _ => SquadStatus.Academy
            };

            // Status can only change by at most one tier per evaluation cycle
            int currentLevel = (int)careerState.Status;
            int targetLevel = (int)targetStatus;

            if (targetLevel > currentLevel)
            {
                return (SquadStatus)(currentLevel + 1);
            }
            if (targetLevel < currentLevel)
            {
                return (SquadStatus)(currentLevel - 1);
            }

            return careerState.Status;
        }

        /// <summary>
        /// Aggregates match results, wage earnings, and attribute growth into an immutable annual <see cref="SeasonStats"/> summary.
        /// </summary>
        public static SeasonStats AggregateSeasonStats(
            IReadOnlyList<MatchResult> results,
            PlayerCareerState finalCareerState,
            decimal weeklyWage,
            int weeksInSeason,
            float attributeGrowthAverage = 0f)
        {
            if (results is null) throw new ArgumentNullException(nameof(results));
            if (finalCareerState is null) throw new ArgumentNullException(nameof(finalCareerState));

            int appearances = 0;
            int goals = 0;
            int assists = 0;
            int yellowCards = 0;
            int redCards = 0;
            float ratingSum = 0f;

            for (int i = 0; i < results.Count; i++)
            {
                var match = results[i];
                if (match.PlayerMinutesPlayed > 0)
                {
                    appearances++;
                    ratingSum += match.PlayerRating;

                    if (match.PlayerScored) goals++;
                    if (match.PlayerAssisted) assists++;

                    if (match.Events != null)
                    {
                        for (int j = 0; j < match.Events.Count; j++)
                        {
                            var e = match.Events[j];
                            if (e.Type == MatchEventType.YellowCard) yellowCards++;
                            else if (e.Type == MatchEventType.RedCard) redCards++;
                        }
                    }
                }
            }

            float averageRating = appearances > 0 ? (float)Math.Round(ratingSum / appearances, 2) : 0f;
            decimal wageEarned = weeklyWage * weeksInSeason;

            return new SeasonStats(
                appearances: appearances,
                goals: goals,
                assists: assists,
                averageRating: averageRating,
                yellowCards: yellowCards,
                redCards: redCards,
                wageEarned: wageEarned,
                finalStatus: finalCareerState.Status,
                attributeGrowthAverage: attributeGrowthAverage);
        }
    }
}
