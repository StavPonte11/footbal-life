using System;
using System.Collections.Generic;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Stateless simulation system governing interpersonal dynamics, affinity decay from neglect,
    /// dialogue and social interaction outcomes, and transfer disruptions.
    /// </summary>
    public static class RelationshipSystem
    {
        public const float DefaultWeeklyDecay = 0.5f;

        /// <summary>
        /// Applies a single week's worth of affinity decay to a relationship from neglect.
        /// Respects the familial affinity floor (20.0) for direct family members.
        /// </summary>
        public static Relationship ApplyWeeklyDecay(Relationship rel, float decayAmount = DefaultWeeklyDecay)
        {
            if (rel is null) throw new ArgumentNullException(nameof(rel));
            return rel.WithDecay(decayAmount);
        }

        /// <summary>
        /// Evaluates elapsed time since the last interaction and applies cumulative weekly decay.
        /// </summary>
        public static Relationship ApplyDecayOverTime(Relationship rel, DateOnly currentDate, float decayPerWeek = DefaultWeeklyDecay)
        {
            if (rel is null) throw new ArgumentNullException(nameof(rel));

            int daysSince = currentDate.DayNumber - rel.LastInteraction.DayNumber;
            if (daysSince < 7) return rel;

            int weeks = daysSince / 7;
            float totalDecay = weeks * decayPerWeek;

            return rel.WithDecay(totalDecay);
        }

        /// <summary>
        /// Records an interactive event (phone call, dinner, shared match, argument) between the player and an individual.
        /// Mutates affinity and trust, updates the interaction date, and logs to shared narrative history.
        /// </summary>
        public static Relationship RecordInteraction(
            Relationship rel,
            DateOnly date,
            float affinityDelta,
            float trustDelta,
            string context)
        {
            if (rel is null) throw new ArgumentNullException(nameof(rel));
            if (string.IsNullOrWhiteSpace(context)) throw new ArgumentException("Interaction context cannot be empty.", nameof(context));

            return rel.WithInteraction(date, affinityDelta, trustDelta, context);
        }

        /// <summary>
        /// Adjusts interpersonal relationships following a club transfer.
        /// Teammates and managers experience social distance, while family and partners adapt to relocation.
        /// </summary>
        public static IReadOnlyList<Relationship> ApplyClubTransfer(
            IReadOnlyList<Relationship> relationships,
            Guid previousClubId,
            Guid newClubId,
            DateOnly transferDate)
        {
            if (relationships is null) throw new ArgumentNullException(nameof(relationships));

            var updated = new List<Relationship>(relationships.Count);

            for (int i = 0; i < relationships.Count; i++)
            {
                var rel = relationships[i];

                switch (rel.Type)
                {
                    case RelationshipType.Teammate:
                        // Teammates from former club drift unless very close friends
                        if (rel.Affinity >= 75f)
                        {
                            updated.Add(rel.WithInteraction(
                                transferDate,
                                affinityDelta: 0f,
                                trustDelta: 2f,
                                historyEntry: "Stayed in close contact after club transfer."));
                        }
                        else
                        {
                            updated.Add(rel.WithInteraction(
                                transferDate,
                                affinityDelta: -5f,
                                trustDelta: 0f,
                                historyEntry: "Drifted apart following transfer to new club."));
                        }
                        break;

                    case RelationshipType.Manager:
                        // Former manager relationship cools down
                        updated.Add(rel.WithInteraction(
                            transferDate,
                            affinityDelta: -10f,
                            trustDelta: -5f,
                            historyEntry: "Transferred away from manager's squad."));
                        break;

                    case RelationshipType.Partner:
                        // Partner reaction depends on existing emotional foundation
                        if (rel.Affinity < 50f)
                        {
                            updated.Add(rel.WithInteraction(
                                transferDate,
                                affinityDelta: -6f,
                                trustDelta: -3f,
                                historyEntry: "Relocation and transfer stress caused emotional friction."));
                        }
                        else
                        {
                            updated.Add(rel.WithInteraction(
                                transferDate,
                                affinityDelta: 4f,
                                trustDelta: 5f,
                                historyEntry: "Supported relocation enthusiastically to back your career move."));
                        }
                        break;

                    default:
                        // Parents, siblings, friends, and agents remain supportive of career advancement
                        updated.Add(rel.WithInteraction(
                            transferDate,
                            affinityDelta: 2f,
                            trustDelta: 2f,
                            historyEntry: "Celebrated exciting new club transfer milestone."));
                        break;
                }
            }

            return updated.AsReadOnly();
        }
    }
}
