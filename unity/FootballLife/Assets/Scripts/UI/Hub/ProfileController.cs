#nullable enable
using System;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Hub
{
    /// <summary>
    /// Pure presentation controller binding ProfileView.uxml to CareerSaveData.
    /// Strictly separates long-term abilities (Pace, Passing, Vision) from temporary condition (Energy, Form, Morale).
    /// </summary>
    public class ProfileController
    {
        private readonly Button _btnBack;
        private readonly Button _tabCareer;
        private readonly Button _tabProfile;

        private readonly Label _labelOvrBadge;
        private readonly Label _labelAvatarInitial;
        private readonly Label _labelPlayerName;
        private readonly Label _labelPlayerMeta;
        private readonly Label _labelPositionBadge;

        // International Football (#P5-004)
        private readonly Label? _badgeIntlTeam;
        private readonly Label? _valIntlCaps;
        private readonly Label? _valIntlGoals;
        private readonly Label? _valIntlStatus;
        private readonly Button? _btnIntlCallUp;

        // Dynamic Condition
        private readonly Label _valConditionEnergy;
        private readonly VisualElement _fillConditionEnergy;
        private readonly Label _valConditionForm;
        private readonly VisualElement _fillConditionForm;
        private readonly Label _valConditionMorale;
        private readonly VisualElement _fillConditionMorale;
        private readonly Label _valConditionTrust;
        private readonly VisualElement _fillConditionTrust;

        // Physical
        private readonly Label _valPace;
        private readonly VisualElement _fillPace;
        private readonly Label _valAcceleration;
        private readonly VisualElement _fillAcceleration;
        private readonly Label _valStamina;
        private readonly VisualElement _fillStamina;
        private readonly Label _valStrength;
        private readonly VisualElement _fillStrength;
        private readonly Label _valAgility;
        private readonly VisualElement _fillAgility;

        // Technical
        private readonly Label _valShooting;
        private readonly VisualElement _fillShooting;
        private readonly Label _valPassing;
        private readonly VisualElement _fillPassing;
        private readonly Label _valDribbling;
        private readonly VisualElement _fillDribbling;
        private readonly Label _valFirstTouch;
        private readonly VisualElement _fillFirstTouch;
        private readonly Label _valCrossing;
        private readonly VisualElement _fillCrossing;
        private readonly Label _valTackling;
        private readonly VisualElement _fillTackling;

        // Mental
        private readonly Label _valVision;
        private readonly VisualElement _fillVision;
        private readonly Label _valComposure;
        private readonly VisualElement _fillComposure;
        private readonly Label _valPositioning;
        private readonly VisualElement _fillPositioning;
        private readonly Label _valDecisionMaking;
        private readonly VisualElement _fillDecisionMaking;

        private readonly Action _onBackToHub;
        private readonly Action _onOpenCareer;

        public ProfileController(
            VisualElement root,
            Action onBackToHub,
            Action onOpenCareer)
        {
            _onBackToHub = onBackToHub;
            _onOpenCareer = onOpenCareer;

            // App Bar & Tabs
            _btnBack = root.Q<Button>("btn-back");
            _tabCareer = root.Q<Button>("tab-career");
            _tabProfile = root.Q<Button>("tab-profile");
            _labelOvrBadge = root.Q<Label>("label-ovr-badge");

            // Identity Header
            _labelAvatarInitial = root.Q<Label>("label-avatar-initial");
            _labelPlayerName = root.Q<Label>("label-player-name");
            _labelPlayerMeta = root.Q<Label>("label-player-meta");
            _labelPositionBadge = root.Q<Label>("label-position-badge");

            // International
            _badgeIntlTeam = root.Q<Label>("badge-intl-team");
            _valIntlCaps = root.Q<Label>("val-intl-caps");
            _valIntlGoals = root.Q<Label>("val-intl-goals");
            _valIntlStatus = root.Q<Label>("val-intl-status");
            _btnIntlCallUp = root.Q<Button>("btn-intl-callup");

            if (_btnIntlCallUp != null)
            {
                _btnIntlCallUp.clicked += OnCheckInternationalCallUp;
            }

            // Dynamic Condition
            _valConditionEnergy = root.Q<Label>("val-condition-energy");
            _fillConditionEnergy = root.Q<VisualElement>("fill-condition-energy");
            _valConditionForm = root.Q<Label>("val-condition-form");
            _fillConditionForm = root.Q<VisualElement>("fill-condition-form");
            _valConditionMorale = root.Q<Label>("val-condition-morale");
            _fillConditionMorale = root.Q<VisualElement>("fill-condition-morale");
            _valConditionTrust = root.Q<Label>("val-condition-trust");
            _fillConditionTrust = root.Q<VisualElement>("fill-condition-trust");

            // Physical
            _valPace = root.Q<Label>("val-pace");
            _fillPace = root.Q<VisualElement>("fill-pace");
            _valAcceleration = root.Q<Label>("val-acceleration");
            _fillAcceleration = root.Q<VisualElement>("fill-acceleration");
            _valStamina = root.Q<Label>("val-stamina");
            _fillStamina = root.Q<VisualElement>("fill-stamina");
            _valStrength = root.Q<Label>("val-strength");
            _fillStrength = root.Q<VisualElement>("fill-strength");
            _valAgility = root.Q<Label>("val-agility");
            _fillAgility = root.Q<VisualElement>("fill-agility");

            // Technical
            _valShooting = root.Q<Label>("val-shooting");
            _fillShooting = root.Q<VisualElement>("fill-shooting");
            _valPassing = root.Q<Label>("val-passing");
            _fillPassing = root.Q<VisualElement>("fill-passing");
            _valDribbling = root.Q<Label>("val-dribbling");
            _fillDribbling = root.Q<VisualElement>("fill-dribbling");
            _valFirstTouch = root.Q<Label>("val-firsttouch");
            _fillFirstTouch = root.Q<VisualElement>("fill-firsttouch");
            _valCrossing = root.Q<Label>("val-crossing");
            _fillCrossing = root.Q<VisualElement>("fill-crossing");
            _valTackling = root.Q<Label>("val-tackling");
            _fillTackling = root.Q<VisualElement>("fill-tackling");

            // Mental
            _valVision = root.Q<Label>("val-vision");
            _fillVision = root.Q<VisualElement>("fill-vision");
            _valComposure = root.Q<Label>("val-composure");
            _fillComposure = root.Q<VisualElement>("fill-composure");
            _valPositioning = root.Q<Label>("val-positioning");
            _fillPositioning = root.Q<VisualElement>("fill-positioning");
            _valDecisionMaking = root.Q<Label>("val-decisionmaking");
            _fillDecisionMaking = root.Q<VisualElement>("fill-decisionmaking");

            // Wire events
            if (_btnBack != null)
                _btnBack.clicked += () => _onBackToHub?.Invoke();

            if (_tabCareer != null)
                _tabCareer.clicked += () => _onOpenCareer?.Invoke();
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

            // Identity
            if (_labelOvrBadge != null)
                _labelOvrBadge.text = $"{save.OverallRating} OVR";

            if (_labelAvatarInitial != null && !string.IsNullOrEmpty(save.PlayerName))
                _labelAvatarInitial.text = save.PlayerName.Substring(0, 1).ToUpperInvariant();

            if (_labelPlayerName != null)
                _labelPlayerName.text = string.IsNullOrEmpty(save.PlayerName) ? "Marcus Vance" : save.PlayerName;

            if (_labelPlayerMeta != null)
                _labelPlayerMeta.text = $"{save.Nationality} · 17 Years Old · {save.PreferredFoot} Foot";

            if (_labelPositionBadge != null)
                _labelPositionBadge.text = save.PrimaryPosition;

            // Temporary Condition (0-100)
            BindStatMeter(_valConditionEnergy, _fillConditionEnergy, save.Energy, "%");
            BindStatMeter(_valConditionForm, _fillConditionForm, save.Form, "%");
            BindStatMeter(_valConditionMorale, _fillConditionMorale, save.Morale, "%");
            BindStatMeter(_valConditionTrust, _fillConditionTrust, save.ManagerTrust, "%");

            // Physical Abilities (0-100)
            BindStatMeter(_valPace, _fillPace, save.Pace);
            BindStatMeter(_valAcceleration, _fillAcceleration, save.Acceleration);
            BindStatMeter(_valStamina, _fillStamina, save.Stamina);
            BindStatMeter(_valStrength, _fillStrength, save.Strength);
            BindStatMeter(_valAgility, _fillAgility, save.Agility);

            // Technical Abilities (0-100)
            BindStatMeter(_valShooting, _fillShooting, save.Shooting);
            BindStatMeter(_valPassing, _fillPassing, save.Passing);
            BindStatMeter(_valDribbling, _fillDribbling, save.Dribbling);
            BindStatMeter(_valFirstTouch, _fillFirstTouch, save.FirstTouch);
            BindStatMeter(_valCrossing, _fillCrossing, save.Crossing);
            BindStatMeter(_valTackling, _fillTackling, save.Tackling);

            // Mental Abilities (0-100)
            BindStatMeter(_valVision, _fillVision, save.Vision);
            BindStatMeter(_valComposure, _fillComposure, save.Composure);
            BindStatMeter(_valPositioning, _fillPositioning, save.Positioning);
            BindStatMeter(_valDecisionMaking, _fillDecisionMaking, save.DecisionMaking);

            // International Football (#P5-004)
            if (_badgeIntlTeam != null)
                _badgeIntlTeam.text = $"🌍 {save.Nationality}";

            if (_valIntlCaps != null)
                _valIntlCaps.text = save.InternationalCaps.ToString();

            if (_valIntlGoals != null)
                _valIntlGoals.text = save.InternationalGoals.ToString();

            if (_valIntlStatus != null)
            {
                if (save.IsRetiredFromInternational)
                    _valIntlStatus.text = "Retired";
                else if (save.InternationalCaps > 0)
                    _valIntlStatus.text = "Capped";
                else
                    _valIntlStatus.text = "Eligible";
            }
        }

        private void OnCheckInternationalCallUp()
        {
            if (SimulationBridge.Instance != null)
            {
                SimulationBridge.Instance.ProcessInternationalWindow();
                if (SimulationBridge.Instance.CurrentSave != null)
                {
                    Bind(SimulationBridge.Instance.CurrentSave);
                }
            }
        }

        private static void BindStatMeter(Label? label, VisualElement? fill, int value, string suffix = "")
        {
            int clamped = Math.Clamp(value, 0, 100);
            if (label != null)
                label.text = $"{clamped}{suffix}";

            if (fill != null)
                fill.style.width = Length.Percent(clamped);
        }
    }
}
