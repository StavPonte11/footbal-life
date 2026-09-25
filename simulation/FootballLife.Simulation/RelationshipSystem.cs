using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation.Persistence;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Result payload from performing a lifestyle social interaction (#P4-004).
    /// </summary>
    public readonly record struct SocialActionResult(
        bool Success,
        string Message,
        Relationship UpdatedRelationship,
        int EnergyCost,
        int MoneyCost,
        float AffinityGained,
        float TrustGained,
        int MoraleGained
    );

    /// <summary>
    /// Stateless simulation system governing interpersonal dynamics, affinity decay from neglect,
    /// dialogue and social interaction outcomes, and transfer disruptions.
    /// </summary>
    public static class RelationshipSystem
    {
        public const float DefaultWeeklyDecay = 0.5f;

        /// <summary>
        /// Executes a purposeful social interaction with a person in the player's life.
        /// Deterministically adjusts energy, finances, morale, and relationship affinity/trust.
        /// </summary>
        public static SocialActionResult ExecuteSocialAction(
            Relationship rel,
            SocialActionType action,
            CareerSaveData save,
            decimal customGiftAmount = 0m,
            DateOnly? currentDate = null)
        {
            if (rel is null) throw new ArgumentNullException(nameof(rel));
            if (save is null) throw new ArgumentNullException(nameof(save));

            var date = currentDate ?? new DateOnly(2026, 8, 1).AddDays(save.CurrentWeek * 7);

            switch (action)
            {
                case SocialActionType.CallCatchUp:
                {
                    const int energyCost = 5;
                    if (save.Energy < energyCost)
                    {
                        return new SocialActionResult(false, "Too exhausted to call right now.", rel, 0, 0, 0f, 0f, 0);
                    }

                    save.Energy -= energyCost;
                    save.Morale = Math.Min(100, save.Morale + 3);

                    float affinityDelta = 4.0f;
                    float trustDelta = 2.0f;
                    string note = "Caught up on a warm phone call.";
                    var updated = rel.WithInteraction(date, affinityDelta, trustDelta, note);

                    return new SocialActionResult(true, $"Had a great catch-up call with {rel.Name} (+{affinityDelta:F1} Affinity)", updated, energyCost, 0, affinityDelta, trustDelta, 3);
                }

                case SocialActionType.SendGift:
                {
                    int giftCost = customGiftAmount > 0 ? (int)customGiftAmount : (rel.IsFamily || rel.Type == RelationshipType.Partner ? 350 : 150);
                    if (save.BankBalance < giftCost)
                    {
                        return new SocialActionResult(false, $"Insufficient funds to send this gift (£{giftCost:N0} required).", rel, 0, 0, 0f, 0f, 0);
                    }

                    save.BankBalance -= giftCost;
                    save.Morale = Math.Min(100, save.Morale + 5);

                    float affinityDelta = 8.5f;
                    float trustDelta = 4.0f;
                    string note = $"Sent a thoughtful gift (£{giftCost:N0}).";
                    var updated = rel.WithInteraction(date, affinityDelta, trustDelta, note);

                    return new SocialActionResult(true, $"Sent a gift to {rel.Name} (-£{giftCost:N0}, +{affinityDelta:F1} Affinity)", updated, 0, giftCost, affinityDelta, trustDelta, 5);
                }

                case SocialActionType.DinnerHangOut:
                {
                    const int energyCost = 15;
                    const int dinnerCost = 250;

                    if (save.Energy < energyCost)
                    {
                        return new SocialActionResult(false, "Too tired for an evening dinner out.", rel, 0, 0, 0f, 0f, 0);
                    }

                    if (save.BankBalance < dinnerCost)
                    {
                        return new SocialActionResult(false, $"Need at least £{dinnerCost:N0} for dinner.", rel, 0, 0, 0f, 0f, 0);
                    }

                    save.Energy -= energyCost;
                    save.BankBalance -= dinnerCost;
                    save.Morale = Math.Min(100, save.Morale + 8);

                    float affinityDelta = 12.0f;
                    float trustDelta = 6.0f;
                    string note = "Enjoyed a relaxing dinner and quality time together.";
                    var updated = rel.WithInteraction(date, affinityDelta, trustDelta, note);

                    return new SocialActionResult(true, $"Wonderful dinner with {rel.Name}! (+{affinityDelta:F1} Affinity, +8 Morale)", updated, energyCost, dinnerCost, affinityDelta, trustDelta, 8);
                }

                case SocialActionType.TalkTactics:
                {
                    if (rel.Type != RelationshipType.Manager && rel.Type != RelationshipType.Teammate)
                    {
                        return new SocialActionResult(false, "Can only discuss match tactics with your manager or teammates.", rel, 0, 0, 0f, 0f, 0);
                    }

                    const int energyCost = 8;
                    if (save.Energy < energyCost)
                    {
                        return new SocialActionResult(false, "Too drained to analyze tactics right now.", rel, 0, 0, 0f, 0f, 0);
                    }

                    save.Energy -= energyCost;
                    save.Form = Math.Min(100, save.Form + 2);

                    float affinityDelta = 3.0f;
                    float trustDelta = 10.0f;
                    string note = "Discussed pitch positioning and tactical match plans.";
                    var updated = rel.WithInteraction(date, affinityDelta, trustDelta, note);

                    if (rel.Type == RelationshipType.Manager)
                    {
                        save.ManagerTrust = Math.Min(100, save.ManagerTrust + 4);
                    }

                    return new SocialActionResult(true, $"Productive tactical review with {rel.Name}! (+{trustDelta:F1} Trust, +4 Manager Trust)", updated, energyCost, 0, affinityDelta, trustDelta, 2);
                }

                default:
                    return new SocialActionResult(false, "Unknown social action.", rel, 0, 0, 0f, 0f, 0);
            }
        }

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
