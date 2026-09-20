using System;
using System.Collections.Generic;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Stateless pure C# simulation system handling life event condition evaluations,
    /// deterministic weighted selection, and state effect applications.
    /// </summary>
    public static class LifeEventSystem
    {
        /// <summary>
        /// Evaluates whether a given life event's preconditions are satisfied for a player.
        /// </summary>
        public static bool EvaluateConditions(
            LifeEvent ev,
            Player player,
            PlayerState state,
            PlayerCareerState career,
            FinanceAccount? finance,
            WorldState world)
        {
            return EvaluateConditions(ev, player, state, career, finance, world, world.CurrentSeason.StartDate);
        }

        /// <summary>
        /// Evaluates whether a given life event's preconditions are satisfied for a player as of a specific date.
        /// </summary>
        public static bool EvaluateConditions(
            LifeEvent ev,
            Player player,
            PlayerState state,
            PlayerCareerState career,
            FinanceAccount? finance,
            WorldState world,
            DateOnly currentDate)
        {
            if (ev is null) throw new ArgumentNullException(nameof(ev));
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (state is null) throw new ArgumentNullException(nameof(state));
            if (career is null) throw new ArgumentNullException(nameof(career));
            if (world is null) throw new ArgumentNullException(nameof(world));

            // Check event cooldown
            if (world.IsEventOnCooldown(ev.Id, currentDate))
                return false;

            var cond = ev.Conditions;
            if (cond is null) return true;

            // Age evaluation
            if (cond.MinAge.HasValue || cond.MaxAge.HasValue)
            {
                int age = currentDate.Year - player.DateOfBirth.Year;
                if (currentDate < player.DateOfBirth.AddYears(age))
                    age--;

                if (cond.MinAge.HasValue && age < cond.MinAge.Value) return false;
                if (cond.MaxAge.HasValue && age > cond.MaxAge.Value) return false;
            }

            // Fatigue evaluation
            if (cond.MinFatigue.HasValue && state.Fatigue < cond.MinFatigue.Value) return false;
            if (cond.MaxFatigue.HasValue && state.Fatigue > cond.MaxFatigue.Value) return false;

            // Salary evaluation
            if (cond.MinSalary.HasValue && career.WeeklySalary < cond.MinSalary.Value) return false;
            if (cond.MaxSalary.HasValue && career.WeeklySalary > cond.MaxSalary.Value) return false;

            // Manager trust evaluation
            if (cond.MinManagerTrust.HasValue && career.ManagerTrust < cond.MinManagerTrust.Value) return false;
            if (cond.MaxManagerTrust.HasValue && career.ManagerTrust > cond.MaxManagerTrust.Value) return false;

            return true;
        }

        /// <summary>
        /// Selects an eligible life event for the week using deterministic weighted random selection.
        /// Returns null if no events in the pool are currently eligible.
        /// </summary>
        public static LifeEvent? SelectWeeklyEvent(
            IReadOnlyList<LifeEvent> pool,
            Player player,
            WorldState world,
            SimulationRandom rng)
        {
            return SelectWeeklyEvent(pool, player, world, rng, world.CurrentSeason.StartDate);
        }

        /// <summary>
        /// Selects an eligible life event for the week as of a specific date using deterministic weighted random selection.
        /// Returns null if no events in the pool are currently eligible.
        /// </summary>
        public static LifeEvent? SelectWeeklyEvent(
            IReadOnlyList<LifeEvent> pool,
            Player player,
            WorldState world,
            SimulationRandom rng,
            DateOnly currentDate)
        {
            if (pool is null || pool.Count == 0) return null;
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            var state = world.GetState(player.Id);
            var career = world.GetCareerState(player.Id);
            world.TryGetAccount(player.Id, out var finance);

            var eligible = new List<LifeEvent>(pool.Count);
            int totalWeight = 0;

            for (int i = 0; i < pool.Count; i++)
            {
                var ev = pool[i];
                if (EvaluateConditions(ev, player, state, career, finance, world, currentDate))
                {
                    eligible.Add(ev);
                    totalWeight += ev.Weight;
                }
            }

            if (eligible.Count == 0 || totalWeight <= 0)
                return null;

            int roll = rng.NextInt(0, totalWeight);
            int cumulative = 0;

            for (int i = 0; i < eligible.Count; i++)
            {
                cumulative += eligible[i].Weight;
                if (roll < cumulative)
                    return eligible[i];
            }

            return eligible[eligible.Count - 1];
        }

        /// <summary>
        /// Applies the consequences of a selected choice to player state, career state, finances,
        /// and records the event cooldown in the world state.
        /// </summary>
        public static WorldState ApplyChoice(
            LifeEvent ev,
            EventChoice choice,
            Player player,
            WorldState world,
            DateOnly date)
        {
            if (ev is null) throw new ArgumentNullException(nameof(ev));
            if (choice is null) throw new ArgumentNullException(nameof(choice));
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (world is null) throw new ArgumentNullException(nameof(world));

            var state = world.GetState(player.Id);
            var career = world.GetCareerState(player.Id);
            var account = world.Accounts.TryGetValue(player.Id, out var acc) ? acc : FinanceAccount.Create();

            if (choice.Effects != null)
            {
                for (int i = 0; i < choice.Effects.Count; i++)
                {
                    var effect = choice.Effects[i];
                    switch (effect.Target)
                    {
                        case EffectTarget.Happiness:
                            state = state with { Happiness = state.Happiness + effect.Delta };
                            break;

                        case EffectTarget.Fatigue:
                            state = state with { Fatigue = state.Fatigue + effect.Delta };
                            break;

                        case EffectTarget.Confidence:
                            state = state with { Confidence = state.Confidence + effect.Delta };
                            break;

                        case EffectTarget.Morale:
                            state = state with { Morale = state.Morale + effect.Delta };
                            break;

                        case EffectTarget.ManagerTrust:
                            float newTrust = Math.Clamp(
                                career.ManagerTrust + effect.Delta,
                                PlayerCareerState.MinTrust,
                                PlayerCareerState.MaxTrust);
                            career = career with { ManagerTrust = newTrust };
                            break;

                        case EffectTarget.Money:
                            decimal amount = (decimal)effect.Delta;
                            var txType = amount >= 0m ? TransactionType.Investment : TransactionType.Fine;
                            string desc = !string.IsNullOrWhiteSpace(effect.Description)
                                ? effect.Description
                                : (amount >= 0m ? $"Event bonus: {ev.Title}" : $"Event expense: {ev.Title}");
                            var tx = new FinanceTransaction(Guid.NewGuid(), date, txType, amount, desc);
                            account = account.WithTransaction(tx);
                            break;
                    }
                }
            }

            world = world.WithPlayerState(player.Id, state)
                         .WithPlayerCareerState(player.Id, career)
                         .WithPlayerAccount(player.Id, account);

            if (ev.CooldownWeeks > 0)
            {
                world = world.WithEventCooldown(ev.Id, date.AddDays(ev.CooldownWeeks * 7));
            }

            return world;
        }
    }
}
