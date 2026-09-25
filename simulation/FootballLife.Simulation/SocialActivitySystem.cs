using System;
using System.Linq;
using FootballLife.Domain;
using FootballLife.Simulation.Persistence;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Result structure for an executed social activity outing (#P4-008).
    /// </summary>
    public sealed record SocialActivityResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public int EnergySpent { get; init; }
        public decimal MoneySpent { get; init; }
        public float MoraleGained { get; init; }
        public float TeamAffinityGained { get; init; }
        public int PrestigeGained { get; init; }
        public bool IncurredManagerDisapproval { get; init; }
        public float ManagerTrustPenalty { get; init; }
    }

    /// <summary>
    /// Pure C# stateless simulation system managing off-pitch social outings,
    /// night life leisure, team bonding, energy/morale trade-offs, and manager risk (#P4-008).
    /// </summary>
    public static class SocialActivitySystem
    {
        /// <summary>
        /// Checks whether the player has sufficient energy and funds to participate in the activity.
        /// </summary>
        public static bool CanAffordActivity(CareerSaveData save, SocialActivity activity, out string reason)
        {
            if (save == null)
            {
                reason = "Save data is null.";
                return false;
            }

            if (activity == null)
            {
                reason = "Activity is null.";
                return false;
            }

            if (save.Energy < activity.EnergyCost)
            {
                reason = $"Too exhausted! Requires {activity.EnergyCost}% energy (you have {save.Energy}%).";
                return false;
            }

            if (save.BankBalance < (int)activity.FinancialCost)
            {
                reason = $"Insufficient funds! Costs £{activity.FinancialCost:N0} (balance: £{save.BankBalance:N0}).";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Executes the chosen social activity deterministically, deducting energy and money,
        /// boosting morale and relationships, and checking for manager disapproval if near matchday.
        /// </summary>
        public static SocialActivityResult ExecuteActivity(
            CareerSaveData save,
            SocialActivity activity,
            DateOnly date,
            bool nearMatchday = false,
            float? forcedRandom = null)
        {
            if (!CanAffordActivity(save, activity, out string errorReason))
            {
                return new SocialActivityResult
                {
                    Success = false,
                    Message = errorReason
                };
            }

            // Deduct energy & financial funds
            save.Energy = Math.Max(0, save.Energy - activity.EnergyCost);
            save.BankBalance = Math.Max(0, save.BankBalance - (int)activity.FinancialCost);

            // Boost morale (0-100)
            float oldMorale = save.Morale;
            save.Morale = Math.Clamp((int)Math.Round(save.Morale + activity.MoraleBoost), 0, 100);
            float actualMoraleGain = save.Morale - oldMorale;

            // Boost team affinity across teammates
            if (activity.TeamAffinityBoost > 0f && save.Relationships != null)
            {
                foreach (var rel in save.Relationships.Where(r => r.RelationshipType.Equals("Teammate", StringComparison.OrdinalIgnoreCase)))
                {
                    rel.Affinity = Math.Clamp(rel.Affinity + (int)Math.Round(activity.TeamAffinityBoost), 0, 100);
                }
            }

            // Boost Fan Popularity / Media Reputation if glamorous or philanthropic
            if (activity.Category == SocialActivityCategory.Philanthropy)
            {
                save.FanPopularity = Math.Clamp(save.FanPopularity + 6, 0, 100);
                save.MediaReputation = Math.Clamp(save.MediaReputation + 5, 0, 100);
            }
            else if (activity.Category == SocialActivityCategory.Glamour)
            {
                save.FanPopularity = Math.Clamp(save.FanPopularity + 4, 0, 100);
            }

            // Check risk of manager disapproval if near matchday
            bool managerDisapproved = false;
            float managerPenalty = 0f;

            if (nearMatchday && activity.ManagerTrustRisk > 0f)
            {
                float roll = forcedRandom ?? new SimulationRandom(save.MasterSeed + save.CurrentWeek * 17 + activity.EnergyCost).NextFloat(0f, 1f);
                if (roll < activity.ManagerTrustRisk)
                {
                    managerDisapproved = true;
                    managerPenalty = 12f;
                    save.ManagerTrust = Math.Max(0, save.ManagerTrust - (int)managerPenalty);
                }
            }

            string outcomeMsg = managerDisapproved
                ? $"{activity.IconEmoji} Enjoyed {activity.Name}, but the manager heard you were out late right before matchday! (-{managerPenalty:0} Trust)"
                : $"{activity.IconEmoji} Great outing at {activity.Name}! (+{actualMoraleGain:0} Morale)";

            return new SocialActivityResult
            {
                Success = true,
                Message = outcomeMsg,
                EnergySpent = activity.EnergyCost,
                MoneySpent = activity.FinancialCost,
                MoraleGained = actualMoraleGain,
                TeamAffinityGained = activity.TeamAffinityBoost,
                PrestigeGained = activity.PrestigeBoost,
                IncurredManagerDisapproval = managerDisapproved,
                ManagerTrustPenalty = managerPenalty
            };
        }
    }
}
