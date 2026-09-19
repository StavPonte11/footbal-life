using System;
using System.Collections.Generic;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Simulates a full 90-minute football match minute-by-minute.
    /// Manages player situations, background match momentum, event logging, performance rating,
    /// and post-match player condition updates.
    /// </summary>
    public static class MatchSimulator
    {
        private const float BackgroundTeamGoalChancePerMinute = 0.013f; // ~1.17 goals per team per 90m
        private static readonly Guid DefaultPlayerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid OpponentTeamId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        /// <summary>
        /// Simulates a scheduled fixture for a player's club.
        /// </summary>
        public static (MatchResult Result, PlayerState UpdatedState) Simulate(
            ScheduledMatch fixture,
            Guid playerClubId,
            PlayerAbilities abilities,
            PlayerState state,
            Position position,
            SimulationRandom rng,
            Guid? playerId = null)
        {
            if (fixture is null) throw new ArgumentNullException(nameof(fixture));
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));
            if (state is null) throw new ArgumentNullException(nameof(state));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            bool playerIsHome = fixture.HomeClubId == playerClubId;
            Guid activePlayerId = playerId ?? DefaultPlayerId;

            var matchState = MatchState.Create(fixture.HomeClubId, fixture.AwayClubId);

            int playerGoals = 0;
            int playerAssists = 0;
            float keyActions = 0f;
            int seriousErrors = 0;
            float confidenceDeltaAccum = 0f;

            // Minute-by-minute simulation loop (1 to 90)
            for (int minute = 1; minute <= 90; minute++)
            {
                matchState = matchState with { Minute = minute };

                // Check for player situation
                var situation = MatchSituationGenerator.GenerateSituation(
                    matchState,
                    position,
                    state,
                    rng,
                    playerIsHome);

                if (situation != null)
                {
                    // Select the choice with highest expected value
                    ActionChoice bestChoice = situation.AvailableChoices[0];
                    for (int i = 1; i < situation.AvailableChoices.Length; i++)
                    {
                        if (situation.AvailableChoices[i].ExpectedValue > bestChoice.ExpectedValue)
                        {
                            bestChoice = situation.AvailableChoices[i];
                        }
                    }

                    var outcome = ActionResolver.Resolve(bestChoice.Action, situation, abilities, state, rng);
                    confidenceDeltaAccum += outcome.ConfidenceDelta;

                    if (outcome.Success)
                    {
                        if (outcome.Type == OutcomeType.Goal)
                        {
                            playerGoals++;
                            matchState = matchState.WithGoal(minute, activePlayerId, null, playerIsHome);
                        }
                        else if (outcome.Type == OutcomeType.SaveMade)
                        {
                            keyActions += 1.0f;
                            matchState = matchState.AddEvent(new MatchEvent(minute, MatchEventType.Save, activePlayerId, "Decisive goalkeeper save"));
                        }
                        else if (outcome.Type == OutcomeType.TackleWon)
                        {
                            keyActions += 0.8f;
                            matchState = matchState.AddEvent(new MatchEvent(minute, MatchEventType.TackleWon, activePlayerId, "Clean tackle won"));
                        }
                        else if (outcome.Type == OutcomeType.InterceptionWon)
                        {
                            keyActions += 0.8f;
                            matchState = matchState.AddEvent(new MatchEvent(minute, MatchEventType.TackleWon, activePlayerId, "Interception"));
                        }
                        else
                        {
                            keyActions += 0.5f;
                        }
                    }
                    else
                    {
                        if (outcome.ManagerTrustDelta <= -3.5f)
                        {
                            seriousErrors++;
                            // 30% chance opposition scores off a catastrophic turnover/error
                            if (rng.NextBool(0.30f))
                            {
                                matchState = matchState.WithGoal(minute, OpponentTeamId, null, !playerIsHome);
                            }
                        }

                        if (outcome.Type == OutcomeType.ChanceMissed)
                        {
                            matchState = matchState.AddEvent(new MatchEvent(minute, MatchEventType.Miss, activePlayerId, "Shot missed target"));
                        }
                    }
                }
                else
                {
                    // Background simulation for non-player actions
                    // Player's team scoring opportunity
                    if (rng.NextBool(BackgroundTeamGoalChancePerMinute))
                    {
                        // Deterministic Guid derived from minute and home/away team
                        byte teamByte = (byte)(playerIsHome ? 1 : 2);
                        Guid teammateId = new Guid(minute, (short)teamByte, (short)1, 0, 0, 0, 0, 0, 0, 0, teamByte);
                        matchState = matchState.WithGoal(minute, teammateId, null, playerIsHome);
                    }

                    // Opponent's team scoring opportunity
                    if (rng.NextBool(BackgroundTeamGoalChancePerMinute))
                    {
                        byte oppByte = (byte)(playerIsHome ? 2 : 1);
                        Guid opponentScorer = new Guid(minute, (short)oppByte, (short)2, 0, 0, 0, 0, 0, 0, 0, oppByte);
                        matchState = matchState.WithGoal(minute, opponentScorer, null, !playerIsHome);
                    }
                }
            }

            matchState = matchState with { IsFinished = true };

            // Compute match performance rating [1.0, 10.0]
            // Base 5.0, +position adjustment (+0.8 for full 90m clean outfield performance), +1.5 per goal, +0.75 per assist, +0.5 per key action, -1.0 per serious error
            float positionBase = (position == Position.CM || position == Position.DM || position == Position.CB || position == Position.FB) ? 0.8f : 0.5f;
            float rawRating = 5.0f + positionBase
                + (playerGoals * 1.5f)
                + (playerAssists * 0.75f)
                + (keyActions * 0.5f)
                - (seriousErrors * 1.0f);

            float playerRating = Math.Min(10.0f, Math.Max(1.0f, (float)Math.Round(rawRating, 1)));

            var result = new MatchResult(
                homeScore: matchState.HomeScore,
                awayScore: matchState.AwayScore,
                events: matchState.EventLog,
                playerRating: playerRating,
                playerMinutesPlayed: 90,
                playerScored: playerGoals > 0,
                playerAssisted: playerAssists > 0);

            // Update player state:
            // 1. Fatigue: Full 90m match application
            var postMatchState = FatigueSystem.ApplyMatch(state, abilities, minutesPlayed: 90, matchIntensity: 1.0f);

            // 2. Form: shifts towards match performance (rating * 10f)
            float targetForm = playerRating * 10f;
            float newForm = (postMatchState.Form * 0.65f) + (targetForm * 0.35f);

            // 3. Confidence: updated by accumulated deltas from in-match actions
            float newConfidence = postMatchState.Confidence + confidenceDeltaAccum;

            var updatedState = postMatchState with
            {
                Form = newForm,
                Confidence = newConfidence
            };

            return (result, updatedState);
        }
    }
}
