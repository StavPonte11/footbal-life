using FootballLife.Unity.Core.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Hub
{
    /// <summary>
    /// Binds TrainingView.uxml — category selection, intensity slider, fatigue guard.
    /// Calls SimulationBridge.SelectWeeklyTraining on confirm.
    /// </summary>
    public class TrainingController
    {
        // ── View elements ─────────────────────────────────────────────────────
        private readonly Button _btnIntensityLow;
        private readonly Button _btnIntensityMed;
        private readonly Button _btnIntensityHigh;
        private readonly Label  _labelIntensityDesc;
        private readonly Label  _labelEnergyRemaining;
        private readonly Label  _labelFatigueWarning;
        private readonly Button _btnConfirm;
        private readonly Button _btnCancel;

        private readonly Button _btnCatPhysical;
        private readonly Button _btnCatTechnical;
        private readonly Button _btnCatTactical;
        private readonly Button _btnCatGoalkeeping;

        // ── State ─────────────────────────────────────────────────────────────
        private int _intensityLevel = 2;      // 1=Low, 2=Med, 3=High
        private string _selectedCategory = "Technical";

        private SimulationBridge? _bridge;
        private System.Action? _onConfirmed;
        private System.Action? _onCancelled;

        // Energy cost per intensity
        private static readonly int[] _energyCosts = { 0, 12, 24, 36 };

        // ── Constructor ───────────────────────────────────────────────────────
        public TrainingController(VisualElement root, System.Action? onConfirmed, System.Action? onCancelled)
        {
            _onConfirmed = onConfirmed;
            _onCancelled = onCancelled;

            _btnIntensityLow  = root.Q<Button>("btn-intensity-low");
            _btnIntensityMed  = root.Q<Button>("btn-intensity-med");
            _btnIntensityHigh = root.Q<Button>("btn-intensity-high");
            _labelIntensityDesc    = root.Q<Label>("label-intensity-desc");
            _labelEnergyRemaining  = root.Q<Label>("label-energy-remaining");
            _labelFatigueWarning   = root.Q<Label>("label-fatigue-warning");
            _btnConfirm = root.Q<Button>("btn-confirm-train");
            _btnCancel  = root.Q<Button>("btn-cancel");

            _btnCatPhysical    = root.Q<Button>("btn-cat-physical");
            _btnCatTechnical   = root.Q<Button>("btn-cat-technical");
            _btnCatTactical    = root.Q<Button>("btn-cat-tactical");
            _btnCatGoalkeeping = root.Q<Button>("btn-cat-goalkeeping");

            // Wire intensity
            _btnIntensityLow.clicked  += () => SetIntensity(1);
            _btnIntensityMed.clicked  += () => SetIntensity(2);
            _btnIntensityHigh.clicked += () => SetIntensity(3);

            // Wire category
            _btnCatPhysical.clicked    += () => SelectCategory("Physical",    _btnCatPhysical);
            _btnCatTechnical.clicked   += () => SelectCategory("Technical",   _btnCatTechnical);
            _btnCatTactical.clicked    += () => SelectCategory("Tactical",    _btnCatTactical);
            _btnCatGoalkeeping.clicked += () => SelectCategory("Goalkeeping", _btnCatGoalkeeping);

            _btnConfirm.clicked += OnConfirmClicked;
            _btnCancel.clicked  += () => _onCancelled?.Invoke();

            // Default selection
            SelectCategory("Technical", _btnCatTechnical);
            SetIntensity(2);
        }

        public void Bind(SimulationBridge bridge)
        {
            _bridge = bridge;
            RefreshEnergyDisplay();
        }

        // ── Private ───────────────────────────────────────────────────────────
        private void SetIntensity(int level)
        {
            _intensityLevel = level;

            // Update intensity button styles
            SetButtonSelected(_btnIntensityLow,  level == 1);
            SetButtonSelected(_btnIntensityMed,  level == 2);
            SetButtonSelected(_btnIntensityHigh, level == 3);

            RefreshEnergyDisplay();
        }

        private void SelectCategory(string category, Button btn)
        {
            _selectedCategory = category;
            SetButtonSelected(_btnCatPhysical,    btn == _btnCatPhysical);
            SetButtonSelected(_btnCatTechnical,   btn == _btnCatTechnical);
            SetButtonSelected(_btnCatTactical,    btn == _btnCatTactical);
            SetButtonSelected(_btnCatGoalkeeping, btn == _btnCatGoalkeeping);
        }

        private void RefreshEnergyDisplay()
        {
            int currentEnergy = _bridge?.CurrentSave?.Energy ?? 100;
            int cost = _energyCosts[_intensityLevel];
            int gainForm = _intensityLevel * 3;

            _labelEnergyRemaining.text = $"⚡ {currentEnergy}";
            _labelIntensityDesc.text = $"Energy Cost: -{cost} · Form Gain: +{gainForm}";

            bool tooFatigued = currentEnergy < cost;
            _labelFatigueWarning.style.display = tooFatigued ? DisplayStyle.Flex : DisplayStyle.None;
            _btnConfirm.SetEnabled(!tooFatigued);
        }

        private void OnConfirmClicked()
        {
            _bridge?.SelectWeeklyTraining(_selectedCategory, _intensityLevel);
            _onConfirmed?.Invoke();
        }

        private static void SetButtonSelected(Button btn, bool selected)
        {
            if (btn == null) return;
            if (selected)
            {
                btn.RemoveFromClassList("btn-secondary");
                btn.AddToClassList("btn-primary");
            }
            else
            {
                btn.RemoveFromClassList("btn-primary");
                btn.AddToClassList("btn-secondary");
            }
        }
    }
}
