using System;
using System.Collections.Generic;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# stateless simulation system managing the player's personal economy.
    /// Handles salary credits, match bonus calculations, lifestyle living expenses, and debt tracking.
    /// Operates deterministically with zero garbage collection allocations in core loops.
    /// </summary>
    public static class EconomySystem
    {
        /// <summary>
        /// Credits the contracted weekly wage to the player's bank account.
        /// </summary>
        /// <param name="account">Current finance account state.</param>
        /// <param name="weeklySalary">Contracted weekly wage (must be >= 0).</param>
        /// <param name="date">Transaction calendar date.</param>
        /// <returns>New <see cref="FinanceAccount"/> reflecting the credited salary.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="account"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="weeklySalary"/> is negative.</exception>
        public static FinanceAccount ApplyWeeklySalary(FinanceAccount account, decimal weeklySalary, DateOnly date)
        {
            if (account is null) throw new ArgumentNullException(nameof(account));
            if (weeklySalary < 0m)
                throw new ArgumentOutOfRangeException(nameof(weeklySalary), "Weekly salary cannot be negative.");

            var tx = new FinanceTransaction(
                Guid.NewGuid(),
                date,
                TransactionType.Salary,
                weeklySalary,
                $"Weekly wage credit: £{weeklySalary:N0}");

            return account.WithTransaction(tx);
        }

        /// <summary>
        /// Evaluates and credits earned performance bonuses from a completed match.
        /// Appearance, goal, assist, and clean sheet bonuses are aggregated into a single ledger transaction.
        /// If no bonuses were earned, the account is returned untouched without allocating empty transactions.
        /// </summary>
        /// <param name="account">Current finance account.</param>
        /// <param name="bonuses">Contractual performance incentives.</param>
        /// <param name="result">Match result summary containing player statistics.</param>
        /// <param name="position">Player's active position on the pitch.</param>
        /// <param name="date">Match calendar date.</param>
        /// <param name="playerWasHome">True if player's club was the home team.</param>
        /// <param name="playerId">Optional player GUID to filter individual match events.</param>
        /// <returns>Updated <see cref="FinanceAccount"/> with bonuses credited, or the original instance if zero earned.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
        public static FinanceAccount ApplyMatchBonuses(
            FinanceAccount account,
            ContractBonuses bonuses,
            MatchResult result,
            Position position,
            DateOnly date,
            bool playerWasHome = true,
            Guid playerId = default)
        {
            if (account is null) throw new ArgumentNullException(nameof(account));
            if (bonuses is null) throw new ArgumentNullException(nameof(bonuses));
            if (result is null) throw new ArgumentNullException(nameof(result));

            int appearances = result.PlayerMinutesPlayed > 0 ? 1 : 0;
            if (appearances == 0)
            {
                // Player did not feature in the match — zero bonuses eligible
                return account;
            }

            // Count player goals and assists from events
            int goals = 0;
            int assists = 0;
            if (result.Events != null && result.Events.Count > 0)
            {
                for (int i = 0; i < result.Events.Count; i++)
                {
                    var ev = result.Events[i];
                    bool matchesPlayer = playerId == Guid.Empty || ev.PlayerId == playerId;
                    if (matchesPlayer)
                    {
                        if (ev.Type == MatchEventType.Goal || ev.Type == MatchEventType.PenaltyScored)
                        {
                            goals++;
                        }
                        else if (ev.Type == MatchEventType.Assist)
                        {
                            assists++;
                        }
                    }
                }
            }

            // Fallback to top-level booleans if events were empty or not matched
            if (goals == 0 && result.PlayerScored) goals = 1;
            if (assists == 0 && result.PlayerAssisted) assists = 1;

            // Clean sheet eligibility: defensive position (GK, CB, FB), played >= 60 mins, opposition scored 0
            bool isDefensivePosition = position == Position.GK || position == Position.CB || position == Position.FB;
            int oppositionGoals = playerWasHome ? result.AwayScore : result.HomeScore;
            bool cleanSheet = isDefensivePosition && result.PlayerMinutesPlayed >= 60 && oppositionGoals == 0;

            decimal totalBonus = bonuses.CalculateTotal(appearances, goals, assists, cleanSheet);
            if (totalBonus <= 0m)
            {
                return account;
            }

            string description =
                $"Match bonus: Appearance £{appearances * bonuses.AppearanceBonus:N0}, " +
                $"Goals ({goals}) £{goals * bonuses.GoalBonus:N0}, " +
                $"Assists ({assists}) £{assists * bonuses.AssistBonus:N0}, " +
                $"Clean Sheet £{(cleanSheet ? bonuses.CleanSheetBonus : 0m):N0}";

            var tx = new FinanceTransaction(
                Guid.NewGuid(),
                date,
                TransactionType.MatchBonus,
                totalBonus,
                description);

            return account.WithTransaction(tx);
        }

        /// <summary>
        /// Returns the standard weekly cost associated with a lifestyle living tier.
        /// </summary>
        public static decimal GetLifestyleWeeklyCost(LifestyleTier tier) => tier switch
        {
            LifestyleTier.Modest => 150m,
            LifestyleTier.Comfortable => 500m,
            LifestyleTier.Luxurious => 2000m,
            LifestyleTier.Extravagant => 8000m,
            LifestyleTier.Superstar => 25000m,
            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
        };

        /// <summary>
        /// Deducts weekly living expenses according to the player's chosen lifestyle tier.
        /// Supports overdrafts, allowing negative balances (debt) when expenses exceed available funds.
        /// </summary>
        /// <param name="account">Current finance account.</param>
        /// <param name="tier">Living lifestyle tier.</param>
        /// <param name="date">Transaction calendar date.</param>
        /// <returns>Updated <see cref="FinanceAccount"/> with lifestyle expenses deducted.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="account"/> is null.</exception>
        public static FinanceAccount ApplyLifestyleExpenses(FinanceAccount account, LifestyleTier tier, DateOnly date)
        {
            if (account is null) throw new ArgumentNullException(nameof(account));

            decimal cost = GetLifestyleWeeklyCost(tier);
            var tx = new FinanceTransaction(
                Guid.NewGuid(),
                date,
                TransactionType.LifestyleExpense,
                -cost,
                $"Lifestyle expenses ({tier}): £{cost:N0}");

            return account.WithTransaction(tx);
        }
    }
}
