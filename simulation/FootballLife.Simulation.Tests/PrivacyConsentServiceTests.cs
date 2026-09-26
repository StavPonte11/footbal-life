using System;
using System.IO;
using FootballLife.Domain;
using FootballLife.Simulation.Persistence;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public sealed class PrivacyConsentServiceTests : IDisposable
    {
        private readonly string _testCrashDir;
        private readonly CrashDiagnosticService _diagnosticService;
        private readonly TelemetryService _telemetryService;
        private readonly PrivacyConsentService _privacyService;

        public PrivacyConsentServiceTests()
        {
            _testCrashDir = Path.Combine(Path.GetTempPath(), "fl_privacy_tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testCrashDir);

            _diagnosticService = new CrashDiagnosticService(maxBreadcrumbs: 20, crashDirectory: _testCrashDir);
            _telemetryService = TelemetryService.Instance;
            _privacyService = PrivacyConsentService.Instance;

            // Reset test state
            _telemetryService.IsOptedOut = false;
            _diagnosticService.IsOptedOut = false;
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testCrashDir))
                {
                    Directory.Delete(_testCrashDir, true);
                }
            }
            catch { }

            _telemetryService.IsOptedOut = false;
            _diagnosticService.IsOptedOut = false;
        }

        [Fact]
        public void DefaultState_HasCoreCloudSaveGranted_AndAnalyticsNotDetermined()
        {
            var state = new PrivacyConsentState();

            Assert.True(state.HasConsent(PrivacyConsentCategory.CloudSave));
            Assert.False(state.HasConsent(PrivacyConsentCategory.Analytics));
            Assert.False(state.HasConsent(PrivacyConsentCategory.PersonalizedAds));
        }

        [Fact]
        public void GrantConsent_Adult_GrantsAnalyticsAndSyncsTelemetry()
        {
            var state = new PrivacyConsentState { PlayerAgeYears = 22 };

            bool granted = _privacyService.GrantConsent(state, PrivacyConsentCategory.Analytics);

            Assert.True(granted);
            Assert.True(state.HasConsent(PrivacyConsentCategory.Analytics));
            Assert.False(_telemetryService.IsOptedOut);
        }

        [Fact]
        public void RevokeConsent_RevokesAnalyticsAndDisablesTelemetry()
        {
            var state = new PrivacyConsentState { PlayerAgeYears = 25 };
            _privacyService.GrantConsent(state, PrivacyConsentCategory.Analytics);
            Assert.False(_telemetryService.IsOptedOut);

            _privacyService.RevokeConsent(state, PrivacyConsentCategory.Analytics);

            Assert.False(state.HasConsent(PrivacyConsentCategory.Analytics));
            Assert.True(_telemetryService.IsOptedOut);
        }

        [Fact]
        public void RevokeConsent_CrashReporting_CleansBreadcrumbsAndDisablesService()
        {
            _diagnosticService.AddBreadcrumb(DiagnosticBreadcrumbCategory.UI, "Button tapped");
            Assert.Single(_diagnosticService.GetRecentBreadcrumbs());

            var state = new PrivacyConsentState { PlayerAgeYears = 20 };
            _privacyService.RevokeConsent(state, PrivacyConsentCategory.CrashReporting);

            // Verify opt-out and cleanup
            Assert.False(state.HasConsent(PrivacyConsentCategory.CrashReporting));
            Assert.True(CrashDiagnosticService.Instance.IsOptedOut);
        }

        [Theory]
        [InlineData(10)] // Under COPPA (<13)
        [InlineData(12)] // Under COPPA (<13)
        [InlineData(14)] // Under GDPR-K (<16)
        [InlineData(15)] // Under GDPR-K (<16)
        public void SetPlayerAge_Minor_BlocksAnalyticsAndForcesOptOut(int age)
        {
            var state = new PrivacyConsentState();

            _privacyService.SetPlayerAge(state, age);

            Assert.True(state.IsMinor);
            Assert.False(state.HasConsent(PrivacyConsentCategory.Analytics));
            Assert.True(_telemetryService.IsOptedOut);

            // Attempting to grant consent as minor must fail
            bool canGrant = _privacyService.GrantConsent(state, PrivacyConsentCategory.Analytics);
            Assert.False(canGrant);
            Assert.False(state.HasConsent(PrivacyConsentCategory.Analytics));
            Assert.True(_telemetryService.IsOptedOut);
        }

        [Fact]
        public void ExportPlayerData_ProducesCompleteNoPiiJson()
        {
            var save = new CareerSaveData
            {
                PlayerName = "Test Striker",
                PrimaryPosition = "ST",
                OverallRating = 72,
                ClubName = "Royal Northfield",
                CareerRewindTokens = 3,
                PreferredLanguage = "es"
            };
            var state = new PrivacyConsentState { PlayerAgeYears = 19 };

            string json = _privacyService.ExportPlayerData(save, state);

            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.Contains("Test Striker", json);
            Assert.Contains("CareerRewindTokens", json);
            Assert.Contains("PiiCollected\": false", json);
            Assert.Contains(PrivacyConsentState.CurrentPolicyVersion, json);
        }

        [Fact]
        public void ExecuteDataErasure_ResetsConsentsAndPurgesDiagnostics()
        {
            var save = new CareerSaveData();
            var state = new PrivacyConsentState { PlayerAgeYears = 30 };
            _privacyService.GrantConsent(state, PrivacyConsentCategory.Analytics);
            _privacyService.GrantConsent(state, PrivacyConsentCategory.CrashReporting);

            bool callbackFired = false;
            _privacyService.ExecuteDataErasure(save, state, () => callbackFired = true);

            Assert.True(callbackFired);
            Assert.False(state.HasConsent(PrivacyConsentCategory.Analytics));
            Assert.False(state.HasConsent(PrivacyConsentCategory.CrashReporting));
            Assert.True(save.TelemetryOptOut);
            Assert.True(_telemetryService.IsOptedOut);
            Assert.True(CrashDiagnosticService.Instance.IsOptedOut);
        }

        [Fact]
        public void CareerSaveData_JsonRoundTrip_PreservesConsentState()
        {
            var save = new CareerSaveData
            {
                PlayerName = "Alex Rivera",
                ConsentState = new PrivacyConsentState
                {
                    PlayerAgeYears = 24,
                    AgeVerified = true
                }
            };
            save.ConsentState.SetConsent(PrivacyConsentCategory.Analytics, true);

            string json = save.ToJson();
            var restored = CareerSaveData.FromJson(json);

            Assert.NotNull(restored.ConsentState);
            Assert.Equal(24, restored.ConsentState.PlayerAgeYears);
            Assert.True(restored.ConsentState.AgeVerified);
            Assert.True(restored.ConsentState.HasConsent(PrivacyConsentCategory.Analytics));
        }
    }
}
