using System;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure simulation system for adjusting manager trust based on match performance,
    /// rating bands, and squad expectation status.
    /// </summary>
    public static class ManagerTrustSystem
    {
        /// <summary>
        /// Updates the player's career state and manager trust based on post-match performance rating.
        /// </summary>
        public static PlayerCareerState ApplyMatchResult(
            PlayerCareerState careerState,
            MatchResult result,
            SquadStatus squadExpectation)
        {
            if (careerState is null) throw new ArgumentNullException(nameof(careerState));
            if (result is null) throw new ArgumentNullException(nameof(result));

            float delta = CalculateTrustDelta(result.PlayerRating, squadExpectation);
            float newTrust = PlayerCareerState.ClampTrust(careerState.ManagerTrust + delta);

            return careerState with
            {
                Status = squadExpectation,
                ManagerTrust = newTrust
            };
        }

        /// <summary>
        /// Calculates the manager trust adjustment delta for a given player rating and squad expectation.
        /// </summary>
        public static float CalculateTrustDelta(float rating, SquadStatus squadExpectation)
        {
            float baseDelta = rating switch
            {
                >= 8.0f => 4.0f,
                >= 6.5f => 1.5f,
                >= 5.0f => 0.0f,
                >= 3.5f => -2.0f,
                _ => -5.0f
            };

            // Amplified penalty for KeyPlayer performing poorly
            if (squadExpectation == SquadStatus.KeyPlayer && rating < 5.0f)
            {
                baseDelta *= 1.5f;
            }

            return baseDelta;
        }
    }
}
