#nullable enable
using System;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Hub
{
    /// <summary>
    /// Pure presentation controller binding CareerView.uxml to CareerSaveData.
    /// Displays club, squad role, manager trust, contract terms, and statistics.
    /// </summary>
    public class CareerController
    {
        private readonly Button _btnBack;
        private readonly Button _tabCareer;
        private readonly Button _tabProfile;

        private readonly Label _labelOvrBadge;
        private readonly Label _labelClubName;
        private readonly Label _labelPositionFoot;
        private readonly Label _labelSquadRole;
        private readonly Label _labelDivision;
        private readonly Label _labelSeasonWeek;

        private readonly Label _valTrustText;
        private readonly Label _labelTrustStatus;
        private readonly VisualElement _fillTrust;

        private readonly Label _valWage;
        private readonly Label _valContractExpiry;
        private readonly Label _valMarketValue;
        private readonly Label _valLifestyleTier;

        private readonly Label _valAppearances;
        private readonly Label _valGoals;
        private readonly Label _valAssists;
        private readonly Label _valAvgRating;

        private readonly Action _onBackToHub;
        private readonly Action _onOpenProfile;

        public CareerController(
            VisualElement root,
            Action onBackToHub,
            Action onOpenProfile)
        {
            _onBackToHub = onBackToHub;
            _onOpenProfile = onOpenProfile;

            // App Bar & Tabs
            _btnBack = root.Q<Button>("btn-back");
            _tabCareer = root.Q<Button>("tab-career");
            _tabProfile = root.Q<Button>("tab-profile");
            _labelOvrBadge = root.Q<Label>("label-ovr-badge");

            // Club & Role
            _labelClubName = root.Q<Label>("label-club-name");
            _labelPositionFoot = root.Q<Label>("label-position-foot");
            _labelSquadRole = root.Q<Label>("label-squad-role");
            _labelDivision = root.Q<Label>("label-division");
            _labelSeasonWeek = root.Q<Label>("label-season-week");

            // Manager Trust
            _valTrustText = root.Q<Label>("val-trust-text");
            _labelTrustStatus = root.Q<Label>("label-trust-status");
            _fillTrust = root.Q<VisualElement>("fill-trust");

            // Contract & Finances
            _valWage = root.Q<Label>("val-wage");
            _valContractExpiry = root.Q<Label>("val-contract-expiry");
            _valMarketValue = root.Q<Label>("val-market-value");
            _valLifestyleTier = root.Q<Label>("val-lifestyle-tier");

            // Stats
            _valAppearances = root.Q<Label>("val-appearances");
            _valGoals = root.Q<Label>("val-goals");
            _valAssists = root.Q<Label>("val-assists");
            _valAvgRating = root.Q<Label>("val-avg-rating");

            // Wire events
            if (_btnBack != null)
                _btnBack.clicked += () => _onBackToHub?.Invoke();

            if (_tabProfile != null)
                _tabProfile.clicked += () => _onOpenProfile?.Invoke();
        }

        public void Bind(SimulationBridge bridge)
        {
            if (bridge == null || bridge.CurrentSave == null)
                return;

            Bind(bridge.CurrentSave);
        }

        public void Bind(CareerSaveData save)
        {
            if (save == null)
                return;

            if (_labelOvrBadge != null)
                _labelOvrBadge.text = $"{save.OverallRating} OVR";

            if (_labelClubName != null)
                _labelClubName.text = string.IsNullOrEmpty(save.ClubName) ? "Free Agent" : save.ClubName;

            if (_labelPositionFoot != null)
                _labelPositionFoot.text = $"{save.PrimaryPosition} · {save.PreferredFoot} Foot";

            if (_labelSquadRole != null)
                _labelSquadRole.text = string.IsNullOrEmpty(save.SquadRole) ? "Prospect" : save.SquadRole;

            if (_labelDivision != null)
                _labelDivision.text = "Division 4";

            if (_labelSeasonWeek != null)
                _labelSeasonWeek.text = $"Season {save.CurrentSeason} · Week {save.CurrentWeek}";

            // Manager Trust
            int trust = Math.Clamp(save.ManagerTrust, 0, 100);
            if (_valTrustText != null)
                _valTrustText.text = $"{trust} / 100";

            if (_fillTrust != null)
                _fillTrust.style.width = Length.Percent(trust);

            if (_labelTrustStatus != null)
            {
                if (trust < 30)
                    _labelTrustStatus.text = "Status: Reserve Squad (Low Trust)";
                else if (trust < 60)
                    _labelTrustStatus.text = "Status: Squad Rotation";
                else
                    _labelTrustStatus.text = "Status: First Team Starter";
            }

            // Contract & Finances
            if (_valWage != null)
                _valWage.text = $"£{save.WeeklyWage:N0} / wk";

            if (_valContractExpiry != null)
            {
                int yearsLeft = Math.Max(0, save.ContractEndYear - 2025);
                _valContractExpiry.text = $"June {save.ContractEndYear} ({yearsLeft} Years)";
            }

            if (_valMarketValue != null)
                _valMarketValue.text = $"£{save.MarketValue:N0}";

            if (_valLifestyleTier != null)
            {
                string tierDesc = save.LifestyleTier switch
                {
                    1 => "Tier 1 · Modest Flat",
                    2 => "Tier 2 · Suburban House",
                    3 => "Tier 3 · Luxury Penthouse",
                    _ => $"Tier {save.LifestyleTier} · Mansion"
                };
                _valLifestyleTier.text = tierDesc;
            }

            // Career Statistics
            if (_valAppearances != null)
                _valAppearances.text = save.TotalAppearances.ToString();

            if (_valGoals != null)
                _valGoals.text = save.TotalGoals.ToString();

            if (_valAssists != null)
                _valAssists.text = save.TotalAssists.ToString();

            if (_valAvgRating != null)
                _valAvgRating.text = save.AverageRating.ToString("F2");
        }
    }
}
