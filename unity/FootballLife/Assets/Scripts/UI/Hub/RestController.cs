using FootballLife.Unity.Core.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Hub
{
    /// <summary>
    /// Binds RestView.uxml — light rest or physio selection, funds guard.
    /// Calls SimulationBridge.PerformRest on confirm.
    /// </summary>
    public class RestController
    {
        private readonly Label  _labelEnergyDisplay;
        private readonly VisualElement _cardLightRest;
        private readonly VisualElement _cardPhysio;
        private readonly Label  _labelFundsWarning;
        private readonly Button _btnConfirm;
        private readonly Button _btnCancel;

        private bool _usePhysio = false;
        private SimulationBridge? _bridge;
        private System.Action? _onConfirmed;
        private System.Action? _onCancelled;

        public RestController(VisualElement root, System.Action? onConfirmed, System.Action? onCancelled)
        {
            _onConfirmed = onConfirmed;
            _onCancelled = onCancelled;

            _labelEnergyDisplay = root.Q<Label>("label-energy-display");
            _cardLightRest      = root.Q<VisualElement>("card-light-rest");
            _cardPhysio         = root.Q<VisualElement>("card-physio");
            _labelFundsWarning  = root.Q<Label>("label-funds-warning");
            _btnConfirm = root.Q<Button>("btn-confirm-rest");
            _btnCancel  = root.Q<Button>("btn-cancel");

            _cardLightRest.RegisterCallback<ClickEvent>(_ => SelectOption(false));
            _cardPhysio.RegisterCallback<ClickEvent>(_   => SelectOption(true));

            _btnConfirm.clicked += OnConfirmClicked;
            _btnCancel.clicked  += () => _onCancelled?.Invoke();

            // Default: light rest selected
            SelectOption(false);
        }

        public void Bind(SimulationBridge bridge)
        {
            _bridge = bridge;
            int energy = _bridge.CurrentSave?.Energy ?? 0;
            _labelEnergyDisplay.text = $"⚡ {energy}";
            ValidateFunds();
        }

        private void SelectOption(bool usePhysio)
        {
            _usePhysio = usePhysio;

            // Card border highlights
            SetCardSelected(_cardLightRest, !usePhysio);
            SetCardSelected(_cardPhysio,     usePhysio);

            _btnConfirm.text = usePhysio ? "Book Physio (£150)" : "Take Light Rest";

            ValidateFunds();
        }

        private void ValidateFunds()
        {
            if (!_usePhysio) { _labelFundsWarning.style.display = DisplayStyle.None; return; }

            int balance = _bridge?.CurrentSave?.BankBalance ?? 0;
            bool canAfford = balance >= 150;
            _labelFundsWarning.style.display = canAfford ? DisplayStyle.None : DisplayStyle.Flex;
            _btnConfirm.SetEnabled(canAfford);
        }

        private void OnConfirmClicked()
        {
            _bridge?.PerformRest(_usePhysio ? "Physio" : "Light");
            // Deduct physio cost from save (bridge handles logic via PerformRest)
            if (_usePhysio && _bridge?.CurrentSave != null)
                _bridge.CurrentSave.BankBalance -= 150;
            _onConfirmed?.Invoke();
        }

        private static void SetCardSelected(VisualElement card, bool selected)
        {
            if (card == null) return;
            // Swap card class for selection highlight
            if (selected)
            {
                card.RemoveFromClassList("card");
                card.AddToClassList("card-elevated");
            }
            else
            {
                card.RemoveFromClassList("card-elevated");
                card.AddToClassList("card");
            }
        }
    }
}
