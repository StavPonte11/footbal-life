using System;
using UnityEngine;
using FootballLife.Domain;

namespace FootballLife.Unity.Core.Performance
{
    /// <summary>
    /// Manages mobile frame rates, thermal/battery budgeting, and memory cleanup across scene transitions.
    /// Ensures 60 FPS during 3D gameplay and throttles down to 30 FPS during UI navigation or low battery.
    /// Strictly forbids GC collection in active gameplay loops.
    /// </summary>
    public class MobilePerformanceManager : MonoBehaviour
    {
        private static MobilePerformanceManager? _instance;
        public static MobilePerformanceManager Instance => _instance!;

        [Header("Settings")]
        [SerializeField] private QualityTier _currentTier = QualityTier.High;
        [SerializeField] private bool _batterySaverOverride = false;

        private bool _isIn3DGameplay = false;
        private float _batteryCheckTimer = 0f;
        private const float BatteryCheckIntervalSeconds = 30f;

        public QualityTier CurrentTier => _currentTier;
        public bool IsIn3DGameplay => _isIn3DGameplay;
        public bool IsBatterySaverActive => _batterySaverOverride;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePerformanceSettings();
        }

        private void Start()
        {
            ApplyTargetFrameRate();
        }

        private void Update()
        {
            // Periodic battery check every 30 seconds to adjust frame rate without allocating
            _batteryCheckTimer += Time.unscaledDeltaTime;
            if (_batteryCheckTimer >= BatteryCheckIntervalSeconds)
            {
                _batteryCheckTimer = 0f;
                CheckBatteryState();
            }
        }

        /// <summary>
        /// Configures initial screen sleep and physics timestep settings.
        /// </summary>
        private void InitializePerformanceSettings()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            QualitySettings.vSyncCount = 0; // Disable VSync so targetFrameRate takes precedence
            ApplyTargetFrameRate();
        }

        /// <summary>
        /// Sets whether the game is currently rendering an active 3D match situation.
        /// </summary>
        public void SetIn3DGameplay(bool inGameplay)
        {
            _isIn3DGameplay = inGameplay;
            ApplyTargetFrameRate();
        }

        /// <summary>
        /// Explicitly toggles user-selected battery saver mode.
        /// </summary>
        public void SetBatterySaver(bool enabled)
        {
            _batterySaverOverride = enabled;
            ApplyTargetFrameRate();
        }

        /// <summary>
        /// Updates the current graphics/performance quality tier.
        /// </summary>
        public void SetQualityTier(QualityTier tier)
        {
            _currentTier = tier;
            ApplyTargetFrameRate();
        }

        /// <summary>
        /// Calculates and applies the target frame rate based on domain performance budget rules.
        /// </summary>
        public void ApplyTargetFrameRate()
        {
            float batteryLevel = SystemInfo.batteryLevel;
            bool isLowBattery = batteryLevel > 0f && batteryLevel <= PerformanceBudget.LowBatteryThreshold;
            bool batterySaver = _batterySaverOverride || isLowBattery || _currentTier == QualityTier.BatterySaver;

            int targetFps = PerformanceBudget.GetRecommendedFrameRate(batteryLevel, batterySaver, _isIn3DGameplay);
            Application.targetFrameRate = targetFps;
        }

        private void CheckBatteryState()
        {
            float batteryLevel = SystemInfo.batteryLevel;
            if (batteryLevel > 0f && batteryLevel <= PerformanceBudget.LowBatteryThreshold)
            {
                ApplyTargetFrameRate();
            }
        }

        /// <summary>
        /// Explicit memory release intended ONLY for scene transitions, screen unloads, or level completion.
        /// NEVER call this during 60 FPS gameplay situations.
        /// </summary>
        public void PerformSceneBoundaryCleanup()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
    }
}
