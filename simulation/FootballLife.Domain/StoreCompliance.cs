using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Privacy consent categories governed under GDPR, CCPA, and COPPA (#P7-802).
    /// </summary>
    public enum PrivacyConsentCategory
    {
        Analytics = 0,
        CrashReporting = 1,
        CloudSave = 2,
        PersonalizedAds = 3
    }

    /// <summary>
    /// Status of a privacy consent request.
    /// </summary>
    public enum ConsentStatus
    {
        NotDetermined = 0,
        Granted = 1,
        Denied = 2
    }

    /// <summary>
    /// Record of a consent decision with timestamp and legal policy version.
    /// </summary>
    public sealed record PrivacyConsentRecord(
        PrivacyConsentCategory Category,
        ConsentStatus Status,
        DateTime DecidedAtUtc,
        string PolicyVersion
    );

    /// <summary>
    /// State of user privacy preferences, age verification, and regulatory consent (#P7-802).
    /// </summary>
    public sealed class PrivacyConsentState
    {
        public const string CurrentPolicyVersion = "2026.1-v1.0";
        public const int CoppaAgeThreshold = 13;
        public const int GdprMinorAgeThreshold = 16;

        public int PlayerAgeYears { get; set; } = 18;
        public bool IsMinor => PlayerAgeYears < GdprMinorAgeThreshold;
        public bool IsChildUnderCoppa => PlayerAgeYears < CoppaAgeThreshold;
        public bool AgeVerified { get; set; } = false;
        public DateTime LastConsentPromptUtc { get; set; } = DateTime.MinValue;

        public Dictionary<PrivacyConsentCategory, ConsentStatus> Consents { get; set; } =
            new Dictionary<PrivacyConsentCategory, ConsentStatus>
            {
                [PrivacyConsentCategory.Analytics] = ConsentStatus.NotDetermined,
                [PrivacyConsentCategory.CrashReporting] = ConsentStatus.NotDetermined,
                [PrivacyConsentCategory.CloudSave] = ConsentStatus.Granted, // Core utility
                [PrivacyConsentCategory.PersonalizedAds] = ConsentStatus.Denied // No ad ID tracking
            };

        public bool HasConsent(PrivacyConsentCategory category)
        {
            // Strict minor protection: minors under COPPA or GDPR cannot opt into analytics or ads
            if (IsMinor && (category == PrivacyConsentCategory.Analytics || category == PrivacyConsentCategory.PersonalizedAds))
            {
                return false;
            }

            if (Consents.TryGetValue(category, out var status))
            {
                return status == ConsentStatus.Granted;
            }

            return false;
        }

        public void SetConsent(PrivacyConsentCategory category, bool granted)
        {
            if (IsMinor && (category == PrivacyConsentCategory.Analytics || category == PrivacyConsentCategory.PersonalizedAds))
            {
                Consents[category] = ConsentStatus.Denied;
                return;
            }

            Consents[category] = granted ? ConsentStatus.Granted : ConsentStatus.Denied;
            LastConsentPromptUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Legal and store policy compliance metadata for in-app monetization offerings (#P7-801).
    /// </summary>
    public sealed record ProductComplianceInfo(
        string ProductId,
        string Title,
        bool IsDeterministic,
        bool HasRandomizedRewards,
        bool IsLootBox,
        string OddsDisclosure,
        bool CompliesWithBelgianGamingAct,
        bool CompliesWithDutchGamblingAct,
        string StoreCategory,
        string RefundCategory
    );

    /// <summary>
    /// Comprehensive audit report of store catalog compliance (#P7-801).
    /// </summary>
    public sealed record StoreComplianceReport(
        int TotalProducts,
        int DeterministicProducts,
        int LootBoxProducts,
        bool GlobalStoreCompliant,
        bool BelgianComplianceApproved,
        bool DutchComplianceApproved,
        DateTime AuditTimestampUtc,
        IReadOnlyList<ProductComplianceInfo> Products
    );

    /// <summary>
    /// Content rating evaluation and metadata for Apple, Google Play, and IARC (#P7-803).
    /// </summary>
    public sealed record AgeRatingProfile(
        string RatingSystem,
        string AgeTier,
        string DescriptorSummary,
        bool HasViolence,
        bool HasGambling,
        bool HasSimulatedGambling,
        bool HasLootBoxes,
        bool HasPaidOdds,
        bool HasInAppPurchases,
        bool CollectsPii,
        bool SharesLocation
    );
}
