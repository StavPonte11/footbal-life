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
    ///
    /// Milestone 7.6 (#P7-601) additions:
    /// - Goal-framing banner: shows the step's GoalMilestoneKey as a motivational target
    ///   (e.g. "Target: Reach Starting XI — get Manager Trust above 60").
    /// - Skip confirmation dialog: warns the player, shows unclaimed reward summary, and
    ///   provides Keep Going / Skip &amp; Claim Rewards options.
    /// - Tutorial replay capability (triggered externally from ProfileController).
    /// - All buttons meet 44×44px minimum via .btn-ergonomic / .btn class CSS tokens (#P7-603).
    /// </summary>
    public class TutorialOverlayController : MonoBehaviour
    {
        public event Action<OnboardingStep>? OnActionClicked;
        public event Action? OnSkipConfirmed;
        public event Action? OnSkipCancelled;

        // ── Main overlay elements ────────────────────────────────────────────
        private VisualElement? _root;
        private VisualElement? _tutorialCard;
        private Label? _counterLabel;
        private Label? _titleLabel;
        private Label? _descLabel;
        private VisualElement? _rewardContainer;
        private Label? _rewardLabel;
        private Button? _actionButton;
        private Button? _skipButton;

        // Goal-framing banner (#P7-601)
        private VisualElement? _goalContainer;
        private Label? _goalIconLabel;
        private Label? _goalTextLabel;

        // Skip confirmation dialog (#P7-601)
        private VisualElement? _skipConfirmDialog;
        private Label? _skipTitleLabel;
        private Label? _skipBodyLabel;
        private Label? _skipRewardSummary;
        private Button? _skipCancelButton;
        private Button? _skipConfirmButton;

        // ── State ────────────────────────────────────────────────────────────
        private OnboardingStep _currentStep;
        private OnboardingStepDefinition? _currentDef;
        private int _currentStepIndex = 1;
        private int _totalSteps = 7;

        private readonly OnboardingSystem _onboardingSystem = new OnboardingSystem();

        // ─────────────────────────────────────────────────────────────────────

        public void BindVisualElements(VisualElement root)
        {
            _root = root.Q<VisualElement>("tutorial-overlay-root") ?? root;

            // Main card
            _tutorialCard = _root.Q<VisualElement>("tutorial-card");
            _counterLabel = _root.Q<Label>("label-tutorial-step-counter");
            _titleLabel = _root.Q<Label>("label-tutorial-title");
            _descLabel = _root.Q<Label>("label-tutorial-desc");
            _rewardContainer = _root.Q<VisualElement>("container-tutorial-reward");
            _rewardLabel = _root.Q<Label>("label-tutorial-reward-text");
            _actionButton = _root.Q<Button>("btn-tutorial-action");
            _skipButton = _root.Q<Button>("btn-tutorial-skip");

            // Goal-framing banner
            _goalContainer = _root.Q<VisualElement>("container-tutorial-goal");
            _goalIconLabel = _root.Q<Label>("label-tutorial-goal-icon");
            _goalTextLabel = _root.Q<Label>("label-tutorial-goal-text");

            // Skip confirmation dialog
            _skipConfirmDialog = _root.Q<VisualElement>("skip-confirm-dialog");
            _skipTitleLabel = _root.Q<Label>("label-skip-title");
            _skipBodyLabel = _root.Q<Label>("label-skip-body");
            _skipRewardSummary = _root.Q<Label>("label-skip-reward-summary");
            _skipCancelButton = _root.Q<Button>("btn-skip-cancel");
            _skipConfirmButton = _root.Q<Button>("btn-skip-confirm");

            // Wire buttons
            if (_actionButton != null)
            {
                _actionButton.clicked -= HandleActionClicked;
                _actionButton.clicked += HandleActionClicked;
            }

            if (_skipButton != null)
            {
                _skipButton.clicked -= HandleSkipButtonClicked;
                _skipButton.clicked += HandleSkipButtonClicked;
            }

            if (_skipCancelButton != null)
            {
                _skipCancelButton.clicked -= HandleSkipCancelled;
                _skipCancelButton.clicked += HandleSkipCancelled;
            }

            if (_skipConfirmButton != null)
            {
                _skipConfirmButton.clicked -= HandleSkipConfirmed;
                _skipConfirmButton.clicked += HandleSkipConfirmed;
            }

            // Language change refresh
            var bridge = SimulationBridge.Instance;
            if (bridge != null)
            {
                bridge.OnLanguageChanged -= HandleLanguageChanged;
                bridge.OnLanguageChanged += HandleLanguageChanged;
            }

            // Initially hide skip dialog
            SetSkipDialogVisible(false);
        }

        private void OnDestroy()
        {
            if (_actionButton != null) _actionButton.clicked -= HandleActionClicked;
            if (_skipButton != null) _skipButton.clicked -= HandleSkipButtonClicked;
            if (_skipCancelButton != null) _skipCancelButton.clicked -= HandleSkipCancelled;
            if (_skipConfirmButton != null) _skipConfirmButton.clicked -= HandleSkipConfirmed;

            var bridge = SimulationBridge.Instance;
            if (bridge != null)
            {
                bridge.OnLanguageChanged -= HandleLanguageChanged;
            }
        }

        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Shows a tutorial step and populates all text, reward, and goal framing.</summary>
        public void ShowStep(
            OnboardingStep step,
            OnboardingStepDefinition def,
            int stepIndex,
            int totalSteps)
        {
            _currentStep = step;
            _currentDef = def;
            _currentStepIndex = stepIndex;
            _totalSteps = totalSteps;

            SetSkipDialogVisible(false);
            RefreshText();

            if (_root != null)
                _root.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (_root != null)
                _root.style.display = DisplayStyle.None;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────

        private void RefreshText()
        {
            if (_currentDef == null) return;

            var bridge = SimulationBridge.Instance;

            // Step Counter
            if (_counterLabel != null)
            {
                string counterFormat = bridge != null
                    ? bridge.T("tutorial.step_counter", _currentStepIndex, _totalSteps)
                    : $"Step {_currentStepIndex} of {_totalSteps}";
                _counterLabel.text = counterFormat;
            }

            // Title & Description
            if (_titleLabel != null)
                _titleLabel.text = bridge != null ? bridge.T(_currentDef.TitleKey) : _currentDef.TitleKey;

            if (_descLabel != null)
                _descLabel.text = bridge != null ? bridge.T(_currentDef.DescriptionKey) : _currentDef.DescriptionKey;

            // Action Button
            if (_actionButton != null)
                _actionButton.text = bridge != null ? bridge.T(_currentDef.ActionPromptKey) : "Continue →";

            // Skip Button
            if (_skipButton != null)
                _skipButton.text = bridge != null ? bridge.T("tutorial.skip_button") : "Skip Tutorial";

            // Starter Reward Badge
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

            // Goal-Framing Banner (#P7-601)
            // Show when the step has an associated goal milestone key.
            if (_goalContainer != null)
            {
                bool hasGoal = !string.IsNullOrEmpty(_currentDef.GoalMilestoneKey);
                _goalContainer.style.display = hasGoal ? DisplayStyle.Flex : DisplayStyle.None;

                if (hasGoal && _goalTextLabel != null)
                {
                    string goalText = bridge != null
                        ? bridge.T(_currentDef.GoalMilestoneKey!)
                        : GetFallbackGoalText(_currentStep);
                    _goalTextLabel.text = goalText;
                }
            }
        }

        /// <summary>
        /// Fallback goal texts when no localization bridge is available.
        /// Provides concrete motivational targets per step (#P7-601).
        /// </summary>
        private static string GetFallbackGoalText(OnboardingStep step) => step switch
        {
            OnboardingStep.Welcome           => "Target: Start your career and sign with a club",
            OnboardingStep.CharacterCreation => "Target: Choose your position and playing style",
            OnboardingStep.ClubSigning       => "Target: Sign a contract and join your first squad",
            OnboardingStep.FirstTraining     => "Target: Complete training to build your fitness",
            OnboardingStep.FirstMatchDebut   => "Target: Make an impact — aim for a 7.0+ match rating",
            OnboardingStep.HomeApartment     => "Target: Rest and recover your Energy above 80%",
            OnboardingStep.SmartphoneIntro   => "Target: Check your messages and manage your media image",
            _                                => "Target: Reach Starting XI — get Manager Trust above 60"
        };

        private void SetSkipDialogVisible(bool visible)
        {
            if (_skipConfirmDialog != null)
                _skipConfirmDialog.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

            if (_tutorialCard != null)
                _tutorialCard.style.display = visible ? DisplayStyle.None : DisplayStyle.Flex;
        }

        /// <summary>
        /// Populates the skip dialog reward summary with all unclaimed starter rewards.
        /// </summary>
        private void PopulateSkipRewardSummary(OnboardingState state)
        {
            if (_skipRewardSummary == null) return;

            var unclaimed = _onboardingSystem.GetAllUnclaimedRewards(state);
            var parts = new System.Collections.Generic.List<string>();

            if (unclaimed.XpBonus > 0) parts.Add($"+{unclaimed.XpBonus} XP");
            if (unclaimed.EnergyBonus > 0) parts.Add($"+{unclaimed.EnergyBonus} Energy");
            if (unclaimed.FormBonus > 0) parts.Add($"+{unclaimed.FormBonus} Form");
            if (unclaimed.ManagerTrustBonus > 0) parts.Add($"+{unclaimed.ManagerTrustBonus} Trust");
            if (unclaimed.CashBonus > 0) parts.Add($"+£{unclaimed.CashBonus}");

            string summary = parts.Count > 0
                ? "Your starters: " + string.Join(" · ", parts)
                : "No pending rewards";

            _skipRewardSummary.text = summary;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Event handlers
        // ─────────────────────────────────────────────────────────────────────

        private void HandleActionClicked()
        {
            OnActionClicked?.Invoke(_currentStep);
        }

        private void HandleSkipButtonClicked()
        {
            // Populate unclaimed rewards from current bridge state
            var bridge = SimulationBridge.Instance;
            if (bridge?.CurrentSave != null)
            {
                var onboardingSystem = new OnboardingSystem();
                var currentState = onboardingSystem.LoadFromSaveData(bridge.CurrentSave);
                PopulateSkipRewardSummary(currentState);
            }

            SetSkipDialogVisible(true);
        }

        private void HandleSkipCancelled()
        {
            SetSkipDialogVisible(false);
            OnSkipCancelled?.Invoke();
        }

        private void HandleSkipConfirmed()
        {
            SetSkipDialogVisible(false);
            Hide();
            OnSkipConfirmed?.Invoke();
        }

        private void HandleLanguageChanged(GameLanguage newLang)
        {
            RefreshText();
        }
    }
}
