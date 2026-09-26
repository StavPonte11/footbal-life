using System;
using System.Collections.Generic;
using System.Text.Json;
using FootballLife.Domain;
using FootballLife.Simulation.Persistence;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# privacy consent, GDPR/CCPA compliance, and age-gating service (#P7-802).
    /// Enforces player data rights (Consent, Access/Export, Erasure, and Minor Protection).
    /// </summary>
    public sealed class PrivacyConsentService
    {
        private static readonly Lazy<PrivacyConsentService> _lazyInstance =
            new Lazy<PrivacyConsentService>(() => new PrivacyConsentService());

        public static PrivacyConsentService Instance => _lazyInstance.Value;

        public PrivacyConsentService()
        {
        }

        /// <summary>
        /// Updates the player's age and enforces regulatory age gating (COPPA <13, GDPR-K <16).
        /// If the player is a minor, analytics and profiling are permanently disabled.
        /// </summary>
        public void SetPlayerAge(PrivacyConsentState state, int ageYears)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            state.PlayerAgeYears = Math.Clamp(ageYears, 3, 120);
            state.AgeVerified = true;

            if (state.IsMinor)
            {
                state.Consents[PrivacyConsentCategory.Analytics] = ConsentStatus.Denied;
                state.Consents[PrivacyConsentCategory.PersonalizedAds] = ConsentStatus.Denied;
            }

            SyncWithRuntimeServices(state);
        }

        /// <summary>
        /// Grants consent for a specific category. Automatically blocked if player is a minor.
        /// </summary>
        public bool GrantConsent(PrivacyConsentState state, PrivacyConsentCategory category)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            if (state.IsMinor && (category == PrivacyConsentCategory.Analytics || category == PrivacyConsentCategory.PersonalizedAds))
            {
                state.Consents[category] = ConsentStatus.Denied;
                SyncWithRuntimeServices(state);
                return false;
            }

            state.SetConsent(category, true);
            SyncWithRuntimeServices(state);
            return true;
        }

        /// <summary>
        /// Revokes consent for a specific category and cleans up associated buffers.
        /// </summary>
        public void RevokeConsent(PrivacyConsentState state, PrivacyConsentCategory category)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            state.SetConsent(category, false);

            if (category == PrivacyConsentCategory.Analytics)
            {
                TelemetryService.Instance.Flush();
            }
            else if (category == PrivacyConsentCategory.CrashReporting)
            {
                CrashDiagnosticService.Instance.ClearBreadcrumbs();
                CrashDiagnosticService.Instance.ClearAllStoredReports();
            }

            SyncWithRuntimeServices(state);
        }

        /// <summary>
        /// Synchronizes the player's privacy consent state with the runtime Telemetry and Crash diagnostic services.
        /// </summary>
        public void SyncWithRuntimeServices(PrivacyConsentState state)
        {
            if (state == null) return;

            bool analyticsAllowed = state.HasConsent(PrivacyConsentCategory.Analytics);
            TelemetryService.Instance.IsOptedOut = !analyticsAllowed;

            bool crashAllowed = state.HasConsent(PrivacyConsentCategory.CrashReporting);
            CrashDiagnosticService.Instance.IsOptedOut = !crashAllowed;
        }

        /// <summary>
        /// Generates a GDPR-compliant portable data export (Right of Access, Article 15)
        /// in human-readable JSON format, containing all persisted player state with zero PII.
        /// </summary>
        public string ExportPlayerData(CareerSaveData save, PrivacyConsentState state)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            if (state == null) throw new ArgumentNullException(nameof(state));

            var exportPayload = new
            {
                ExportMetadata = new
                {
                    ExportDateUtc = DateTime.UtcNow,
                    DataSubjectType = "Player",
                    GameTitle = "Football Life",
                    DataPolicyVersion = PrivacyConsentState.CurrentPolicyVersion,
                    PiiCollected = false
                },
                PrivacyStatus = new
                {
                    AgeYears = state.PlayerAgeYears,
                    IsMinor = state.IsMinor,
                    AgeVerified = state.AgeVerified,
                    Consents = state.Consents
                },
                CareerProfile = new
                {
                    save.PlayerName,
                    Position = save.PrimaryPosition,
                    save.Nationality,
                    save.DateOfBirth,
                    save.OverallRating,
                    save.ClubName,
                    save.CurrentSeason,
                    save.CurrentWeek,
                    save.TotalAppearances,
                    save.TotalGoals,
                    save.TotalAssists,
                    save.TotalTrophies,
                    save.LifetimeEarnings
                },
                EntitlementsAndMonetization = new
                {
                    save.CareerRewindTokens,
                    save.OwnedCosmeticIds,
                    RewindTokenDeterministic = true
                },
                Preferences = new
                {
                    save.PreferredLanguage,
                    save.IsTutorialCompleted
                }
            };

            return JsonSerializer.Serialize(exportPayload, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Executes a GDPR Article 17 "Right to Erasure" / Right to be Forgotten request:
        /// wipes diagnostic breadcrumbs, deletes offline crash reports, purges telemetry queues,
        /// and permanently revokes all optional data collection consents.
        /// </summary>
        public void ExecuteDataErasure(CareerSaveData save, PrivacyConsentState state, Action? onComplete = null)
        {
            if (state != null)
            {
                state.Consents[PrivacyConsentCategory.Analytics] = ConsentStatus.Denied;
                state.Consents[PrivacyConsentCategory.CrashReporting] = ConsentStatus.Denied;
                state.Consents[PrivacyConsentCategory.PersonalizedAds] = ConsentStatus.Denied;
            }

            if (save != null)
            {
                save.TelemetryOptOut = true;
            }

            TelemetryService.Instance.IsOptedOut = true;
            TelemetryService.Instance.Flush();

            CrashDiagnosticService.Instance.IsOptedOut = true;
            CrashDiagnosticService.Instance.ClearBreadcrumbs();
            CrashDiagnosticService.Instance.ClearAllStoredReports();

            onComplete?.Invoke();
        }
    }
}
