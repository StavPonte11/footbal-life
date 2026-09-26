using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Classification of stadium architectural scale and facilities based on club prestige and league tier.
    /// </summary>
    public enum StadiumReputationTier
    {
        /// <summary>
        /// Lower-league / grassroots club (Reputation 1–40):
        /// Low open-air terracing, perimeter railings, simple bench dugouts, basic floodlight poles.
        /// </summary>
        Grassroots = 0,

        /// <summary>
        /// Mid-tier established club (Reputation 41–74):
        /// Enclosed 4-stand ground, corrugated cantilever roofs, full team dugouts with technical boxes, elevated floodlight gantries.
        /// </summary>
        MidTier = 1,

        /// <summary>
        /// Elite contender / international arena (Reputation 75–100):
        /// Grand double-tier bowl, executive suite ribbon, sweeping architectural roof trusses, sunken luxury dugouts, 4 monumental corner floodlight towers.
        /// </summary>
        Elite = 2
    }

    /// <summary>
    /// Dynamic state of spectator engagement and excitement during match highlights.
    /// </summary>
    public enum CrowdExcitementState
    {
        /// <summary>Ambient stadium murmur and supporter background chanting.</summary>
        Murmur = 0,

        /// <summary>Rising anticipation as attack enters final third / shooting opportunity.</summary>
        Anticipation = 1,

        /// <summary>Explosive celebration and celebratory wave when a goal is registered.</summary>
        Roar = 2,

        /// <summary>Collective recoil and gasp following woodwork hit, narrow miss, or reflex save.</summary>
        Gasp = 3
    }

    /// <summary>
    /// Floodlight architectural style corresponding to stadium tier.
    /// </summary>
    public enum FloodlightStyle
    {
        PoleMasts = 0,
        GantryTowers = 1,
        CornerArenaTowers = 2
    }

    /// <summary>
    /// Architectural specification for procedural stadium generation.
    /// </summary>
    public sealed record StadiumTierConfig
    {
        public StadiumReputationTier Tier { get; init; }
        public float StandHeightMeters { get; init; }
        public int TierCount { get; init; }
        public int RowSteps { get; init; }
        public float StandDepthMeters { get; init; }
        public bool HasCantileverRoof { get; init; }
        public bool HasExecutiveBoxes { get; init; }
        public FloodlightStyle LightingStyle { get; init; }
        public int SpectatorDensityRows { get; init; }

        public StadiumTierConfig(
            StadiumReputationTier tier,
            float standHeightMeters,
            int tierCount,
            int rowSteps,
            float standDepthMeters,
            bool hasCantileverRoof,
            bool hasExecutiveBoxes,
            FloodlightStyle lightingStyle,
            int spectatorDensityRows)
        {
            Tier = tier;
            StandHeightMeters = standHeightMeters;
            TierCount = tierCount;
            RowSteps = rowSteps;
            StandDepthMeters = standDepthMeters;
            HasCantileverRoof = hasCantileverRoof;
            HasExecutiveBoxes = hasExecutiveBoxes;
            LightingStyle = lightingStyle;
            SpectatorDensityRows = spectatorDensityRows;
        }
    }

    /// <summary>
    /// Pure domain math and configuration utilities for stadium atmosphere, crowd states, and match visual parameters.
    /// </summary>
    public static class StadiumAtmosphereUtility
    {
        public const int GrassrootsMaxReputation = 40;
        public const int MidTierMaxReputation = 74;

        /// <summary>
        /// Evaluates the stadium tier from a club's institutional reputation rating [1, 100].
        /// </summary>
        public static StadiumReputationTier GetTierFromReputation(int reputationRating)
        {
            if (reputationRating <= GrassrootsMaxReputation)
            {
                return StadiumReputationTier.Grassroots;
            }
            if (reputationRating <= MidTierMaxReputation)
            {
                return StadiumReputationTier.MidTier;
            }
            return StadiumReputationTier.Elite;
        }

        /// <summary>
        /// Evaluates the stadium tier from spectator capacity when club reputation is not directly specified.
        /// </summary>
        public static StadiumReputationTier GetTierFromCapacity(int capacity)
        {
            if (capacity < 12000)
            {
                return StadiumReputationTier.Grassroots;
            }
            if (capacity < 45000)
            {
                return StadiumReputationTier.MidTier;
            }
            return StadiumReputationTier.Elite;
        }

        /// <summary>
        /// Returns the architectural configuration parameters for a given stadium tier.
        /// </summary>
        public static StadiumTierConfig GetTierConfig(StadiumReputationTier tier)
        {
            return tier switch
            {
                StadiumReputationTier.Grassroots => new StadiumTierConfig(
                    tier: StadiumReputationTier.Grassroots,
                    standHeightMeters: 4.5f,
                    tierCount: 1,
                    rowSteps: 5,
                    standDepthMeters: 6.0f,
                    hasCantileverRoof: false,
                    hasExecutiveBoxes: false,
                    lightingStyle: FloodlightStyle.PoleMasts,
                    spectatorDensityRows: 4),

                StadiumReputationTier.MidTier => new StadiumTierConfig(
                    tier: StadiumReputationTier.MidTier,
                    standHeightMeters: 11.0f,
                    tierCount: 1,
                    rowSteps: 12,
                    standDepthMeters: 14.0f,
                    hasCantileverRoof: true,
                    hasExecutiveBoxes: false,
                    lightingStyle: FloodlightStyle.GantryTowers,
                    spectatorDensityRows: 10),

                StadiumReputationTier.Elite or _ => new StadiumTierConfig(
                    tier: StadiumReputationTier.Elite,
                    standHeightMeters: 22.0f,
                    tierCount: 2,
                    rowSteps: 20,
                    standDepthMeters: 22.0f,
                    hasCantileverRoof: true,
                    hasExecutiveBoxes: true,
                    lightingStyle: FloodlightStyle.CornerArenaTowers,
                    spectatorDensityRows: 18)
            };
        }

        /// <summary>
        /// Computes the damped sine displacement for goal-net ripple physics.
        /// </summary>
        /// <param name="shotSpeedKmh">Ball impact speed in km/h.</param>
        /// <param name="elapsedSeconds">Time in seconds since impact.</param>
        /// <param name="frequency">Vibration oscillation frequency in Hz.</param>
        /// <param name="dampingRate">Decay coefficient per second.</param>
        /// <returns>Normal displacement offset in meters.</returns>
        public static float ComputeNetRipple(float shotSpeedKmh, float elapsedSeconds, float frequency = 14f, float dampingRate = 3.2f)
        {
            if (elapsedSeconds < 0f || elapsedSeconds > 2.0f)
            {
                return 0f;
            }

            // Normal speed scaled impulse [0.05m to 0.35m amplitude]
            float initialAmplitude = Math.Clamp(shotSpeedKmh / 120.0f * 0.28f, 0.06f, 0.35f);
            float decay = (float)Math.Exp(-dampingRate * elapsedSeconds);
            float wave = (float)Math.Sin(frequency * elapsedSeconds * Math.PI * 2.0);

            return initialAmplitude * decay * wave;
        }

        /// <summary>
        /// Computes turf particle count for kick or slide impact based on power [0.0, 1.0].
        /// </summary>
        public static int ComputeTurfParticleCount(float power01, bool isSlide = false)
        {
            float clampedPower = Math.Clamp(power01, 0f, 1f);
            if (isSlide)
            {
                return (int)(15 + (clampedPower * 25)); // 15 to 40 particles for slide tackle
            }
            return (int)(8 + (clampedPower * 22));    // 8 to 30 particles for power strike
        }

        /// <summary>
        /// Computes dynamic ball trail width scaled by speed (m/s).
        /// </summary>
        public static float ComputeBallTrailWidth(float speedMps, float baseWidth = 0.12f, float maxWidth = 0.28f)
        {
            if (speedMps <= 1.0f)
            {
                return 0f;
            }
            float t = Math.Clamp((speedMps - 1f) / 30f, 0f, 1f);
            return baseWidth + ((maxWidth - baseWidth) * t);
        }
    }
}
