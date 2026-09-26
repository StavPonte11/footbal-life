using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Scalable graphics and simulation performance quality levels for mobile hardware.
    /// </summary>
    public enum QualityTier
    {
        BatterySaver = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    /// <summary>
    /// Performance budget constraints and targets for mobile platforms.
    /// Adheres to zero-GC in hot paths and 60 FPS mobile rendering targets.
    /// </summary>
    public static class PerformanceBudget
    {
        public const int TargetFrameRateGameplay = 60;
        public const int TargetFrameRateBatterySaver = 30;
        public const float MaxFrameTimeMs = 16.66f;
        public const float BatterySaverFrameTimeMs = 33.33f;

        public const int MaxTotalRamMb = 200;
        public const int MaxWorldDataMemoryMb = 10;
        public const float LowBatteryThreshold = 0.20f;

        /// <summary>
        /// Evaluates the recommended target frame rate based on battery level and power saving mode.
        /// </summary>
        public static int GetRecommendedFrameRate(float batteryLevel, bool isBatterySaverActive, bool isIn3DGameplay)
        {
            if (isBatterySaverActive || (batteryLevel > 0f && batteryLevel <= LowBatteryThreshold))
            {
                return TargetFrameRateBatterySaver;
            }

            return isIn3DGameplay ? TargetFrameRateGameplay : TargetFrameRateBatterySaver;
        }
    }

    /// <summary>
    /// Classification of mobile device hardware capability based on RAM, CPU cores, and GPU specs.
    /// </summary>
    public enum DeviceTier
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    /// <summary>
    /// Concrete performance configuration tuned per device tier (#P7-701).
    /// </summary>
    public sealed record DevicePerformanceProfile
    {
        public DeviceTier Tier { get; init; }
        public QualityTier QualityTier { get; init; }
        public int TargetFpsGameplay { get; init; }
        public int TargetFpsUI { get; init; }
        public float RenderScale { get; init; }
        public float CrowdDensityFactor { get; init; }
        public int MaxTurfParticles { get; init; }
        public int MaxNetRippleIterations { get; init; }
        public bool EnableShadowCascades { get; init; }
        public int TargetMaxRamMb { get; init; }

        public DevicePerformanceProfile(
            DeviceTier tier,
            QualityTier qualityTier,
            int targetFpsGameplay,
            int targetFpsUI,
            float renderScale,
            float crowdDensityFactor,
            int maxTurfParticles,
            int maxNetRippleIterations,
            bool enableShadowCascades,
            int targetMaxRamMb)
        {
            Tier = tier;
            QualityTier = qualityTier;
            TargetFpsGameplay = targetFpsGameplay;
            TargetFpsUI = targetFpsUI;
            RenderScale = renderScale;
            CrowdDensityFactor = crowdDensityFactor;
            MaxTurfParticles = maxTurfParticles;
            MaxNetRippleIterations = maxNetRippleIterations;
            EnableShadowCascades = enableShadowCascades;
            TargetMaxRamMb = targetMaxRamMb;
        }

        public static DevicePerformanceProfile GetProfile(DeviceTier tier) => tier switch
        {
            DeviceTier.Low => new DevicePerformanceProfile(
                tier: DeviceTier.Low,
                qualityTier: QualityTier.Low,
                targetFpsGameplay: 30,
                targetFpsUI: 30,
                renderScale: 0.85f,
                crowdDensityFactor: 0.35f,
                maxTurfParticles: 16,
                maxNetRippleIterations: 4,
                enableShadowCascades: false,
                targetMaxRamMb: 150
            ),
            DeviceTier.Medium => new DevicePerformanceProfile(
                tier: DeviceTier.Medium,
                qualityTier: QualityTier.Medium,
                targetFpsGameplay: 60,
                targetFpsUI: 30,
                renderScale: 1.0f,
                crowdDensityFactor: 0.70f,
                maxTurfParticles: 32,
                maxNetRippleIterations: 8,
                enableShadowCascades: true,
                targetMaxRamMb: 180
            ),
            DeviceTier.High or _ => new DevicePerformanceProfile(
                tier: DeviceTier.High,
                qualityTier: QualityTier.High,
                targetFpsGameplay: 60,
                targetFpsUI: 30,
                renderScale: 1.0f,
                crowdDensityFactor: 1.0f,
                maxTurfParticles: 64,
                maxNetRippleIterations: 16,
                enableShadowCascades: true,
                targetMaxRamMb: 200
            )
        };
    }

    /// <summary>
    /// Classifies mobile hardware into Low, Mid, or High device tiers based on specifications (#P7-701).
    /// </summary>
    public static class DeviceTierClassifier
    {
        public static DeviceTier Classify(int systemMemoryMb, int processorCount, int graphicsMemoryMb = 0, bool supportsComputeShaders = true)
        {
            // Low tier: less than 3GB RAM or 4 or fewer CPU cores, or very low VRAM
            if (systemMemoryMb < 3072 || processorCount <= 4 || (graphicsMemoryMb > 0 && graphicsMemoryMb < 768))
            {
                return DeviceTier.Low;
            }

            // High tier: at least 6GB RAM, 8+ CPU cores, compute shaders supported, and >=2GB VRAM
            if (systemMemoryMb >= 6000 && processorCount >= 8 && supportsComputeShaders && (graphicsMemoryMb == 0 || graphicsMemoryMb >= 2048))
            {
                return DeviceTier.High;
            }

            // Mid tier: 3GB to 6GB RAM, 6+ cores
            return DeviceTier.Medium;
        }
    }
}
