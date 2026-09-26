#nullable enable
using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Unity.Core.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Tutorial
{
    /// <summary>
    /// UI Toolkit controller managing the First-Time User Experience (FTUE) tutorial overlay.
    /// Presents contextual step instructions, starter rewards, and handles action triggers with full localization.
    /// </summary>
    public class TutorialOverlayController : MonoBehaviour
    {
        public event Action<OnboardingStep>? OnActionClicked;
        public event Action? OnSkipClicked;

        private VisualElement? _root;
        private Label? _counterLabel;
        private Label? _titleLabel;
        private Label? _descLabel;
        private VisualElement? _rewardContainer;
        private Label? _rewardLabel;
        private Button? _actionButton;
        private Button? _skipButton;

        private OnboardingStep _currentStep;
        private OnboardingStepDefinition? _currentDef;
        private int _currentStepIndex = 1;
        private int _totalSteps = 7;

        public void BindVisualElements(VisualElement root)
        {
            _root = root.Q<VisualElement>("tutorial-overlay-root") ?? root;

            _counterLabel = _root.Q<Label>("label-tutorial-step-counter");
            _titleLabel = _root.Q<Label>("label-tutorial-title");
            _descLabel = _root.Q<Label>("label-tutorial-desc");
            _rewardContainer = _root.Q<VisualElement>("container-tutorial-reward");
            _rewardLabel = _root.Q<Label>("label-tutorial-reward-text");
            _actionButton = _root.Q<Button>("btn-tutorial-action");
            _skipButton = _root.Q<Button>("btn-tutorial-skip");

            if (_actionButton != null)
            {
                _actionButton.clicked -= HandleActionClicked;
                _actionButton.clicked += HandleActionClicked;
            }

            if (_skipButton != null)
            {
                _skipButton.clicked -= HandleSkipClicked;
                _skipButton.clicked += HandleSkipClicked;
            }

            var bridge = SimulationBridge.Instance;
            if (bridge != null)
            {
                bridge.OnLanguageChanged -= HandleLanguageChanged;
                bridge.OnLanguageChanged += HandleLanguageChanged;
            }
        }

        private void OnDestroy()
        {
            if (_actionButton != null) _actionButton.clicked -= HandleActionClicked;
            if (_skipButton != null) _skipButton.clicked -= HandleSkipClicked;

            var bridge = SimulationBridge.Instance;
            if (bridge != null)
            {
                bridge.OnLanguageChanged -= HandleLanguageChanged;
            }
        }

        public void ShowStep(OnboardingStep step, OnboardingStepDefinition def, int stepIndex, int totalSteps)
        {
            _currentStep = step;
            _currentDef = def;
            _currentStepIndex = stepIndex;
            _totalSteps = totalSteps;

            RefreshText();

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        private void RefreshText()
        {
            if (_currentDef == null) return;

            var bridge = SimulationBridge.Instance;

            // Step Counter
            if (_counterLabel != null)
            {
                string counterFormat = bridge != null ? bridge.T("tutorial.step_counter", _currentStepIndex, _totalSteps) : $"Step {_currentStepIndex} of {_totalSteps}";
                _counterLabel.text = counterFormat;
            }

            // Title & Description
            if (_titleLabel != null)
            {
                _titleLabel.text = bridge != null ? bridge.T(_currentDef.TitleKey) : _currentDef.TitleKey;
            }

            if (_descLabel != null)
            {
                _descLabel.text = bridge != null ? bridge.T(_currentDef.DescriptionKey) : _currentDef.DescriptionKey;
            }

            // Action Button Text
            if (_actionButton != null)
            {
                _actionButton.text = bridge != null ? bridge.T(_currentDef.ActionPromptKey) : "Continue →";
            }

            // Skip Button Text
            if (_skipButton != null)
            {
                _skipButton.text = bridge != null ? bridge.T("tutorial.skip_button") : "Skip Tutorial";
            }

            // Rewards
            if (_rewardContainer != null && _rewardLabel != null)
            {
                var r = _currentDef.Reward;
                bool hasReward = (r.XpBonus > 0 || r.EnergyBonus > 0 || r.FormBonus > 0 || r.ManagerTrustBonus > 0 || r.CashBonus > 0);

                _rewardContainer.style.display = hasReward ? DisplayStyle.Flex : DisplayStyle.None;

                if (hasReward)
                {
                    string rewardSummary = "";
                    if (r.XpBonus > 0) rewardSummary += $"+{r.XpBonus} XP ";
                    if (r.EnergyBonus > 0) rewardSummary += $"+{r.EnergyBonus} Energy ";
                    if (r.FormBonus > 0) rewardSummary += $"+{r.FormBonus} Form ";
                    if (r.ManagerTrustBonus > 0) rewardSummary += $"+{r.ManagerTrustBonus} Trust ";
                    if (r.CashBonus > 0) rewardSummary += $"+£{r.CashBonus} ";

                    string rewardPrefix = bridge != null ? bridge.T("tutorial.reward_title") : "Starter Bonus:";
                    _rewardLabel.text = $"{rewardPrefix} {rewardSummary.Trim()}";
                }
            }
        }

        private void HandleActionClicked()
        {
            OnActionClicked?.Invoke(_currentStep);
        }

        private void HandleSkipClicked()
        {
            Hide();
            OnSkipClicked?.Invoke();
        }

        private void HandleLanguageChanged(GameLanguage newLang)
        {
            RefreshText();
        }
    }
}
