using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents an emotional, interpersonal bond between a footballer and an individual person.
    /// Tracks affinity, mutual trust, interaction recency, and shared narrative history.
    /// </summary>
    public sealed record Relationship
    {
        public const float MinAffinity = 0f;
        public const float MaxAffinity = 100f;
        public const float MinTrust = 0f;
        public const float MaxTrust = 100f;
        public const float FamilyAffinityFloor = 20f;

        private readonly float _affinity;
        private readonly float _trust;

        public Guid Id { get; init; }
        public Guid PlayerId { get; init; }
        public string Name { get; init; }
        public RelationshipType Type { get; init; }

        /// <summary>
        /// Emotional closeness, warmth, and goodwill [0f, 100f].
        /// </summary>
        public float Affinity
        {
            get => _affinity;
            init => _affinity = Math.Clamp(value, MinAffinity, MaxAffinity);
        }

        /// <summary>
        /// Mutual reliability, confidentiality, and respect [0f, 100f].
        /// </summary>
        public float Trust
        {
            get => _trust;
            init => _trust = Math.Clamp(value, MinTrust, MaxTrust);
        }

        /// <summary>
        /// Calendar date of the most recent meaningful contact or dialogue.
        /// </summary>
        public DateOnly LastInteraction { get; init; }

        /// <summary>
        /// Chronological record of shared events, milestones, and turning points.
        /// </summary>
        public IReadOnlyList<string> SharedHistory { get; init; }

        /// <summary>
        /// Returns true if this relationship is a direct family connection (Parent, Sibling).
        /// </summary>
        public bool IsFamily => Type == RelationshipType.Parent || Type == RelationshipType.Sibling;

        public Relationship(
            Guid id,
            Guid playerId,
            string name,
            RelationshipType type,
            float affinity,
            float trust,
            DateOnly lastInteraction,
            IReadOnlyList<string>? sharedHistory = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Relationship name cannot be empty.", nameof(name));

            Id = id;
            PlayerId = playerId;
            Name = name;
            Type = type;
            _affinity = Math.Clamp(affinity, MinAffinity, MaxAffinity);
            _trust = Math.Clamp(trust, MinTrust, MaxTrust);
            LastInteraction = lastInteraction;
            SharedHistory = sharedHistory ?? Array.Empty<string>();
        }

        /// <summary>
        /// Factory helper to create a new relationship instance.
        /// </summary>
        public static Relationship Create(
            Guid playerId,
            string name,
            RelationshipType type,
            float initialAffinity = 75f,
            float initialTrust = 75f,
            DateOnly? initialDate = null)
        {
            var date = initialDate ?? new DateOnly(2026, 8, 1);
            var history = new[] { $"Relationship established with {name} ({type})." };
            return new Relationship(Guid.NewGuid(), playerId, name, type, initialAffinity, initialTrust, date, history);
        }

        /// <summary>
        /// Applies an interaction effect, updating date, affinity, trust, and appending to shared history.
        /// </summary>
        public Relationship WithInteraction(DateOnly date, float affinityDelta, float trustDelta, string? historyEntry = null)
        {
            var newHistory = new List<string>(SharedHistory);
            if (!string.IsNullOrWhiteSpace(historyEntry))
            {
                newHistory.Add($"[{date:yyyy-MM-dd}] {historyEntry}");
            }

            return this with
            {
                Affinity = Affinity + affinityDelta,
                Trust = Trust + trustDelta,
                LastInteraction = date,
                SharedHistory = newHistory.AsReadOnly()
            };
        }

        /// <summary>
        /// Applies affinity decay from neglect, respecting the familial floor for family members.
        /// </summary>
        public Relationship WithDecay(float decayAmount)
        {
            if (decayAmount <= 0f) return this;

            float minFloor = IsFamily ? FamilyAffinityFloor : MinAffinity;
            float newAffinity = Math.Clamp(Affinity - decayAmount, minFloor, MaxAffinity);

            return this with { Affinity = newAffinity };
        }
    }
}
