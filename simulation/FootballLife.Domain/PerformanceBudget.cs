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
}
