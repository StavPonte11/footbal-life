using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents a football manager whose preferred formation, tactical style,
    /// patience, and trust decay dynamics govern matchday squad selection and player development.
    /// </summary>
    public sealed record Manager
    {
        public const float MinDecayRate = 0.0f;
        public const float MaxDecayRate = 1.0f;
        public const float MinTolerance = 0.0f;
        public const float MaxTolerance = 1.0f;
        public const int MinReputation = 1;
        public const int MaxReputation = 100;

        private readonly float _trustDecayRate;
        private readonly float _toleranceThreshold;
        private readonly int _reputationRating;

        /// <summary>
        /// Unique persistent identifier of the manager.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Full display name of the manager.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Club the manager is currently employed by, or null if unattached.
        /// </summary>
        public Guid? ClubId { get; init; }

        /// <summary>
        /// Primary tactical system preferred by this manager.
        /// </summary>
        public Formation PreferredFormation { get; init; }

        /// <summary>
        /// Overarching gameplay philosophy.
        /// </summary>
        public TacticalIdentity TacticalStyle { get; init; }

        /// <summary>
        /// Rate at which player manager-trust degrades per benched match or poor session [0.0, 1.0].
        /// </summary>
        public float TrustDecayRate
        {
            get => _trustDecayRate;
            init
            {
                if (float.IsNaN(value) || value < MinDecayRate || value > MaxDecayRate)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Trust decay rate must be in [{MinDecayRate}, {MaxDecayRate}]. Actual: {value}");
                }
                _trustDecayRate = value;
            }
        }

        /// <summary>
        /// Normalized trust floor [0.0, 1.0] below which the manager drops the player from the starting XI.
        /// (e.g. 0.40 means a player whose trust is below 40% is benched or dropped).
        /// </summary>
        public float ToleranceThreshold
        {
            get => _toleranceThreshold;
            init
            {
                if (float.IsNaN(value) || value < MinTolerance || value > MaxTolerance)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Tolerance threshold must be in [{MinTolerance}, {MaxTolerance}]. Actual: {value}");
                }
                _toleranceThreshold = value;
            }
        }

        /// <summary>
        /// Managerial reputation and prestige rating [1, 100].
        /// </summary>
        public int ReputationRating
        {
            get => _reputationRating;
            init
            {
                if (value < MinReputation || value > MaxReputation)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Reputation rating must be in [{MinReputation}, {MaxReputation}]. Actual: {value}");
                }
                _reputationRating = value;
            }
        }

        public Manager(
            Guid id,
            string name,
            Formation preferredFormation,
            TacticalIdentity tacticalStyle,
            float trustDecayRate,
            float toleranceThreshold,
            int reputationRating = 50,
            Guid? clubId = null)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Manager ID cannot be an empty Guid.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Manager name cannot be null or whitespace.", nameof(name));
            }

            if (float.IsNaN(trustDecayRate) || trustDecayRate < MinDecayRate || trustDecayRate > MaxDecayRate)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(trustDecayRate),
                    $"Trust decay rate must be in [{MinDecayRate}, {MaxDecayRate}]. Actual: {trustDecayRate}");
            }

            if (float.IsNaN(toleranceThreshold) || toleranceThreshold < MinTolerance || toleranceThreshold > MaxTolerance)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(toleranceThreshold),
                    $"Tolerance threshold must be in [{MinTolerance}, {MaxTolerance}]. Actual: {toleranceThreshold}");
            }

            if (reputationRating < MinReputation || reputationRating > MaxReputation)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reputationRating),
                    $"Reputation rating must be in [{MinReputation}, {MaxReputation}]. Actual: {reputationRating}");
            }

            Id = id;
            Name = name.Trim();
            ClubId = clubId;
            PreferredFormation = preferredFormation;
            TacticalStyle = tacticalStyle;
            _trustDecayRate = trustDecayRate;
            _toleranceThreshold = toleranceThreshold;
            _reputationRating = reputationRating;
        }

        /// <summary>
        /// Factory method to create a new manager with an automatically generated GUID.
        /// </summary>
        public static Manager Create(
            string name,
            Formation preferredFormation,
            TacticalIdentity tacticalStyle,
            float trustDecayRate = 0.05f,
            float toleranceThreshold = 0.40f,
            int reputationRating = 50,
            Guid? clubId = null)
        {
            return new Manager(
                Guid.NewGuid(),
                name,
                preferredFormation,
                tacticalStyle,
                trustDecayRate,
                toleranceThreshold,
                reputationRating,
                clubId);
        }

        /// <summary>
        /// Computes the new trust value after a given number of benched matches or negative events.
        /// Scales by the manager's trust decay rate.
        /// </summary>
        /// <param name="currentTrust">Current trust score in [0.0, 100.0].</param>
        /// <param name="matchesBenched">Consecutive matches excluded from starting lineup.</param>
        /// <returns>Updated trust score bounded in [0.0, 100.0].</returns>
        public float CalculateDecayedTrust(float currentTrust, int matchesBenched = 1)
        {
            if (matchesBenched <= 0) return PlayerCareerState.ClampTrust(currentTrust);

            float decayAmount = currentTrust * _trustDecayRate * matchesBenched;
            float newTrust = currentTrust - decayAmount;
            return PlayerCareerState.ClampTrust(newTrust);
        }

        /// <summary>
        /// Evaluates whether a player's trust score falls below the manager's tolerance floor for starting.
        /// </summary>
        /// <param name="playerTrust">Player's manager trust score [0.0, 100.0].</param>
        /// <returns>True if the player should be dropped from the starting lineup.</returns>
        public bool ShouldDropFromStartingXI(float playerTrust)
        {
            float normalizedTrust = playerTrust / 100f;
            return normalizedTrust < _toleranceThreshold;
        }

        /// <summary>
        /// Assigns or unassigns the manager from a club, returning a new immutable Manager record.
        /// </summary>
        public Manager WithClub(Guid? clubId)
        {
            return this with { ClubId = clubId };
        }
    }
}
