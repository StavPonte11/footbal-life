using System;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.OffSeason
{
    /// <summary>
    /// Controller for AttributeGrowthView.uxml (#P2-019).
    /// Presents annual progression deltas, potential ceiling rating, and age-development curve feedback.
    /// </summary>
    public class AttributeGrowthController
    {
        private readonly VisualElement _root;
        private readonly Action _onProceedToTransfers;
        private readonly Action _onBackToSummary;

        // UI Labels
        private readonly Label _labelBadge;
        private readonly Label _valPreviousOvr;
        private readonly Label _valDeltaOvr;
        private readonly Label _valCurrentOvr;
        private readonly Label _valCeiling;
        private readonly Label _labelPhase;

        private readonly Label _valDeltaShooting;
        private readonly Label _valDeltaPassing;
        private readonly Label _valDeltaDribbling;
        private readonly Label _valDeltaPace;
        private readonly Label _valDeltaStamina;
        private readonly Label _valDeltaVision;

        // Buttons
        private readonly Button _btnTransfers;
        private readonly Button _btnBack;

        public AttributeGrowthController(
            VisualElement root,
            Action onProceedToTransfers,
            Action onBackToSummary)
        {
            _root = root;
            _onProceedToTransfers = onProceedToTransfers;
            _onBackToSummary = onBackToSummary;

            _labelBadge      = root.Q<Label>("label-growth-badge");
            _valPreviousOvr  = root.Q<Label>("val-ovr-previous");
            _valDeltaOvr     = root.Q<Label>("val-ovr-delta");
            _valCurrentOvr   = root.Q<Label>("val-ovr-current");
            _valCeiling      = root.Q<Label>("val-ovr-ceiling");
            _labelPhase      = root.Q<Label>("label-growth-phase");

            _valDeltaShooting = root.Q<Label>("val-delta-shooting");
            _valDeltaPassing  = root.Q<Label>("val-delta-passing");
            _valDeltaDribbling = root.Q<Label>("val-delta-dribbling");
            _valDeltaPace     = root.Q<Label>("val-delta-pace");
            _valDeltaStamina  = root.Q<Label>("val-delta-stamina");
            _valDeltaVision   = root.Q<Label>("val-delta-vision");

            _btnTransfers = root.Q<Button>("btn-growth-transfers");
            _btnBack      = root.Q<Button>("btn-growth-back");

            if (_btnTransfers != null)
                _btnTransfers.clicked += () => _onProceedToTransfers?.Invoke();

            if (_btnBack != null)
                _btnBack.clicked += () => _onBackToSummary?.Invoke();
        }

        public void Bind(AttributeGrowthSnapshot growth, CareerSaveData? save)
        {
            int delta = growth.EndOvr - growth.StartOvr;
            string deltaSign = delta >= 0 ? $"+{delta}" : $"{delta}";

            if (_labelBadge != null)
                _labelBadge.text = $"{growth.StartOvr} → {growth.EndOvr} ({deltaSign} OVR)";

            if (_valPreviousOvr != null)
                _valPreviousOvr.text = growth.StartOvr.ToString();

            if (_valDeltaOvr != null)
                _valDeltaOvr.text = deltaSign;

            if (_valCurrentOvr != null)
                _valCurrentOvr.text = growth.EndOvr.ToString();

            if (_valCeiling != null)
                _valCeiling.text = growth.PotentialRating.ToString();

            if (_labelPhase != null)
                _labelPhase.text = growth.Phase;

            // Attribute deltas
            if (_valDeltaShooting != null)
            {
                int currentVal = save != null ? save.Shooting : 62;
                _valDeltaShooting.text = $"+{growth.ShootingDelta} (Now {currentVal})";
            }

            if (_valDeltaPassing != null)
            {
                int currentVal = save != null ? save.Passing : 60;
                _valDeltaPassing.text = $"+{growth.PassingDelta} (Now {currentVal})";
            }

            if (_valDeltaDribbling != null)
            {
                int currentVal = save != null ? save.Dribbling : 62;
                _valDeltaDribbling.text = $"+{growth.DribblingDelta} (Now {currentVal})";
            }

            if (_valDeltaPace != null)
            {
                int currentVal = save != null ? save.Pace : 64;
                _valDeltaPace.text = $"+{growth.PaceDelta} (Now {currentVal})";
            }

            if (_valDeltaStamina != null)
            {
                int currentVal = save != null ? save.Stamina : 61;
                _valDeltaStamina.text = $"+{growth.StaminaDelta} (Now {currentVal})";
            }

            if (_valDeltaVision != null)
            {
                int currentVal = save != null ? save.Vision : 57;
                _valDeltaVision.text = $"+{growth.VisionDelta} (Now {currentVal})";
            }
        }
    }
}
