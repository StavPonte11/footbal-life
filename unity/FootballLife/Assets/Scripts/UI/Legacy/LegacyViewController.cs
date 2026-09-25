#nullable enable
using System;
using System.Linq;
using FootballLife.Domain;
using FootballLife.Unity.Core.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Legacy
{
    /// <summary>
    /// UI Toolkit controller managing Career Legacy, Hall of Fame inductions, and the Retirement Arc.
    /// </summary>
    public sealed class LegacyViewController
    {
        private readonly VisualElement _root;
        private readonly Action _onBack;

        private Button? _btnBack;
        private Label? _labelGradeBadge;

        // Tabs
        private Button? _tabRetro;
        private Button? _tabHof;
        private Button? _tabRetirement;

        private VisualElement? _containerRetro;
        private VisualElement? _containerHof;
        private VisualElement? _containerRetirement;

        // Hero Score & Grade
        private Label? _labelHeroGrade;
        private Label? _labelHeroScore;
        private Label? _labelHofStatus;

        // Stats
        private Label? _statAppearances;
        private Label? _statGoals;
        private Label? _statAssists;
        private Label? _statTrophies;
        private Label? _statCaps;
        private Label? _statEarnings;

        // Hall of Fame
        private Label? _labelPlaqueText;

        // Retirement Controls
        private VisualElement? _sectionActiveRetirement;
        private VisualElement? _sectionAlreadyRetired;
        private Label? _labelRetirementDesc;
        private Label? _labelAlreadyRetiredStatement;

        private Button? _btnReasonPeak;
        private Button? _btnReasonDecline;
        private Button? _btnReasonTrophies;

        private Button? _btnRoleManager;
        private Button? _btnRoleCoach;
        private Button? _btnRolePundit;
        private Button? _btnRoleAmbassador;

        private Button? _btnConfirmRetirement;

        private RetirementReason _selectedReason = RetirementReason.VoluntaryAtPeak;
        private PostPlayingRole _selectedRole = PostPlayingRole.Manager;

        public LegacyViewController(VisualElement root, Action onBack)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _onBack = onBack ?? throw new ArgumentNullException(nameof(onBack));

            QueryElements();
            RegisterCallbacks();
        }

        private void QueryElements()
        {
            _btnBack = _root.Q<Button>("btn-legacy-back");
            _labelGradeBadge = _root.Q<Label>("label-grade-badge");

            _tabRetro = _root.Q<Button>("tab-retro");
            _tabHof = _root.Q<Button>("tab-hof");
            _tabRetirement = _root.Q<Button>("tab-retirement");

            _containerRetro = _root.Q<VisualElement>("container-retro");
            _containerHof = _root.Q<VisualElement>("container-hof");
            _containerRetirement = _root.Q<VisualElement>("container-retirement");

            _labelHeroGrade = _root.Q<Label>("label-hero-grade");
            _labelHeroScore = _root.Q<Label>("label-hero-score");
            _labelHofStatus = _root.Q<Label>("label-hof-status");

            _statAppearances = _root.Q<Label>("stat-appearances");
            _statGoals = _root.Q<Label>("stat-goals");
            _statAssists = _root.Q<Label>("stat-assists");
            _statTrophies = _root.Q<Label>("stat-trophies");
            _statCaps = _root.Q<Label>("stat-caps");
            _statEarnings = _root.Q<Label>("stat-earnings");

            _labelPlaqueText = _root.Q<Label>("label-plaque-text");

            _sectionActiveRetirement = _root.Q<VisualElement>("section-active-retirement-options");
            _sectionAlreadyRetired = _root.Q<VisualElement>("section-already-retired");
            _labelRetirementDesc = _root.Q<Label>("label-retirement-status-desc");
            _labelAlreadyRetiredStatement = _root.Q<Label>("label-already-retired-statement");

            _btnReasonPeak = _root.Q<Button>("btn-reason-peak");
            _btnReasonDecline = _root.Q<Button>("btn-reason-decline");
            _btnReasonTrophies = _root.Q<Button>("btn-reason-trophies");

            _btnRoleManager = _root.Q<Button>("btn-role-manager");
            _btnRoleCoach = _root.Q<Button>("btn-role-coach");
            _btnRolePundit = _root.Q<Button>("btn-role-pundit");
            _btnRoleAmbassador = _root.Q<Button>("btn-role-ambassador");

            _btnConfirmRetirement = _root.Q<Button>("btn-confirm-retirement");
        }

        private void RegisterCallbacks()
        {
            if (_btnBack != null) _btnBack.clicked += _onBack.Invoke;

            if (_tabRetro != null) _tabRetro.clicked += () => SwitchTab(0);
            if (_tabHof != null) _tabHof.clicked += () => SwitchTab(1);
            if (_tabRetirement != null) _tabRetirement.clicked += () => SwitchTab(2);

            // Reason buttons
            if (_btnReasonPeak != null) _btnReasonPeak.clicked += () => SetReason(RetirementReason.VoluntaryAtPeak);
            if (_btnReasonDecline != null) _btnReasonDecline.clicked += () => SetReason(RetirementReason.AgeAndDecline);
            if (_btnReasonTrophies != null) _btnReasonTrophies.clicked += () => SetReason(RetirementReason.TrophyCabinetComplete);

            // Role buttons
            if (_btnRoleManager != null) _btnRoleManager.clicked += () => SetRole(PostPlayingRole.Manager);
            if (_btnRoleCoach != null) _btnRoleCoach.clicked += () => SetRole(PostPlayingRole.AcademyCoach);
            if (_btnRolePundit != null) _btnRolePundit.clicked += () => SetRole(PostPlayingRole.TVPundit);
            if (_btnRoleAmbassador != null) _btnRoleAmbassador.clicked += () => SetRole(PostPlayingRole.ClubAmbassador);

            if (_btnConfirmRetirement != null)
            {
                _btnConfirmRetirement.clicked += OnConfirmRetirement;
            }
        }

        private void SwitchTab(int index)
        {
            if (_containerRetro != null) _containerRetro.style.display = index == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            if (_containerHof != null) _containerHof.style.display = index == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            if (_containerRetirement != null) _containerRetirement.style.display = index == 2 ? DisplayStyle.Flex : DisplayStyle.None;

            UpdateTabButtonClass(_tabRetro, index == 0);
            UpdateTabButtonClass(_tabHof, index == 1);
            UpdateTabButtonClass(_tabRetirement, index == 2);
        }

        private static void UpdateTabButtonClass(Button? button, bool isActive)
        {
            if (button == null) return;
            if (isActive) button.AddToClassList("tab-button-active");
            else button.RemoveFromClassList("tab-button-active");
        }

        private void SetReason(RetirementReason reason)
        {
            _selectedReason = reason;
            UpdateSelectionStyle(_btnReasonPeak, reason == RetirementReason.VoluntaryAtPeak);
            UpdateSelectionStyle(_btnReasonDecline, reason == RetirementReason.AgeAndDecline);
            UpdateSelectionStyle(_btnReasonTrophies, reason == RetirementReason.TrophyCabinetComplete);
        }

        private void SetRole(PostPlayingRole role)
        {
            _selectedRole = role;
            UpdateSelectionStyle(_btnRoleManager, role == PostPlayingRole.Manager);
            UpdateSelectionStyle(_btnRoleCoach, role == PostPlayingRole.AcademyCoach);
            UpdateSelectionStyle(_btnRolePundit, role == PostPlayingRole.TVPundit);
            UpdateSelectionStyle(_btnRoleAmbassador, role == PostPlayingRole.ClubAmbassador);
        }

        private static void UpdateSelectionStyle(Button? button, bool isSelected)
        {
            if (button == null) return;
            if (isSelected)
            {
                button.style.backgroundColor = new Color(0.15f, 0.4f, 0.9f, 0.8f);
                button.style.color = Color.white;
            }
            else
            {
                button.style.backgroundColor = StyleKeyword.Null;
                button.style.color = StyleKeyword.Null;
            }
        }

        public void Refresh()
        {
            var bridge = SimulationBridge.Instance;
            if (bridge == null) return;

            var legacy = bridge.CalculateCurrentCareerLegacy();
            var retirement = bridge.GetPlayerRetirementDecision();
            var hofEntries = bridge.GetHallOfFameEntries();

            // Hero
            if (_labelGradeBadge != null) _labelGradeBadge.text = legacy.Grade.ToString().ToUpperInvariant();
            if (_labelHeroGrade != null) _labelHeroGrade.text = $"CAREER STATUS: {legacy.Grade.ToString().ToUpperInvariant()}";
            if (_labelHeroScore != null) _labelHeroScore.text = $"{legacy.CareerScore} pts";
            if (_labelHofStatus != null)
            {
                _labelHofStatus.text = legacy.IsHallOfFameInductee ? "⭐ Hall of Fame Inductee" : "Hall of Fame Contender";
            }

            // Stats
            if (_statAppearances != null) _statAppearances.text = legacy.LifetimeAppearances.ToString();
            if (_statGoals != null) _statGoals.text = legacy.LifetimeGoals.ToString();
            if (_statAssists != null) _statAssists.text = legacy.LifetimeAssists.ToString();
            if (_statTrophies != null) _statTrophies.text = legacy.TotalTrophies.ToString();
            if (_statCaps != null) _statCaps.text = legacy.InternationalCaps.ToString();
            if (_statEarnings != null) _statEarnings.text = $"£{legacy.LifetimeEarnings:N0}";

            // Hall of Fame Plaque
            var latestEntry = hofEntries.LastOrDefault();
            if (_labelPlaqueText != null)
            {
                if (latestEntry != null)
                {
                    _labelPlaqueText.text = latestEntry.PlaqueText;
                }
                else if (legacy.IsHallOfFameInductee)
                {
                    _labelPlaqueText.text = $"In recognition of legendary achievements (Career Score: {legacy.CareerScore}), this player is eligible for the Hall of Fame upon formal retirement.";
                }
                else
                {
                    _labelPlaqueText.text = "The Hall of Fame is reserved for icons and legends of the sport with extraordinary silverware and longevity.";
                }
            }

            // Retirement Section
            bool isRetired = retirement != null && retirement.IsRetired;
            if (_sectionActiveRetirement != null) _sectionActiveRetirement.style.display = isRetired ? DisplayStyle.None : DisplayStyle.Flex;
            if (_sectionAlreadyRetired != null) _sectionAlreadyRetired.style.display = isRetired ? DisplayStyle.Flex : DisplayStyle.None;

            if (isRetired)
            {
                if (_labelAlreadyRetiredStatement != null)
                {
                    _labelAlreadyRetiredStatement.text = retirement!.Statement;
                }
            }
            else
            {
                bool eligible = bridge.IsEligibleForRetirement();
                if (_btnConfirmRetirement != null)
                {
                    _btnConfirmRetirement.SetEnabled(eligible);
                    _btnConfirmRetirement.text = eligible
                        ? "Hang Up Boots & Announce Retirement"
                        : "Retirement Locked (Available at Age 32+)";
                }
            }

            SetReason(_selectedReason);
            SetRole(_selectedRole);
        }

        private void OnConfirmRetirement()
        {
            var bridge = SimulationBridge.Instance;
            if (bridge == null) return;

            var decision = bridge.RetirePlayer(_selectedReason, _selectedRole);
            Refresh();
            SwitchTab(1); // Switch to Hall of Fame tab to view plaque
        }
    }
}
