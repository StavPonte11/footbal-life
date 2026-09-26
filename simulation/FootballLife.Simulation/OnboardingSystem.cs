using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation.Persistence;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# system managing the First-Time User Experience (FTUE) and progressive onboarding tutorial.
    /// Handles step progression, starter reward distribution, feature gating, and persistence.
    /// </summary>
    public sealed class OnboardingSystem
    {
        private static readonly Dictionary<OnboardingStep, OnboardingStepDefinition> _stepDefinitions =
            new Dictionary<OnboardingStep, OnboardingStepDefinition>
            {
                [OnboardingStep.Welcome] = new OnboardingStepDefinition(
                    Step: OnboardingStep.Welcome,
                    TitleKey: "tutorial.step.welcome.title",
                    DescriptionKey: "tutorial.step.welcome.desc",
                    TargetElementId: "btn-start-career",
                    ActionPromptKey: "tutorial.step.welcome.action",
                    Reward: OnboardingReward.None,
                    GoalMilestoneKey: "tutorial.goal.welcome"
                ),
                [OnboardingStep.CharacterCreation] = new OnboardingStepDefinition(
                    Step: OnboardingStep.CharacterCreation,
                    TitleKey: "tutorial.step.creation.title",
                    DescriptionKey: "tutorial.step.creation.desc",
                    TargetElementId: "btn-confirm-creation",
                    ActionPromptKey: "tutorial.step.creation.action",
                    Reward: new OnboardingReward(FormBonus: 5),
                    GoalMilestoneKey: "tutorial.goal.creation"
                ),
                [OnboardingStep.ClubSigning] = new OnboardingStepDefinition(
                    Step: OnboardingStep.ClubSigning,
                    TitleKey: "tutorial.step.signing.title",
                    DescriptionKey: "tutorial.step.signing.desc",
                    TargetElementId: "btn-sign-contract",
                    ActionPromptKey: "tutorial.step.signing.action",
                    Reward: new OnboardingReward(ManagerTrustBonus: 10, CashBonus: 500),
                    GoalMilestoneKey: "tutorial.goal.signing"
                ),
                [OnboardingStep.FirstTraining] = new OnboardingStepDefinition(
                    Step: OnboardingStep.FirstTraining,
                    TitleKey: "tutorial.step.training.title",
                    DescriptionKey: "tutorial.step.training.desc",
                    TargetElementId: "btn-train",
                    ActionPromptKey: "tutorial.step.training.action",
                    Reward: new OnboardingReward(XpBonus: 50, EnergyBonus: 10),
                    GoalMilestoneKey: "tutorial.goal.training"
                ),
                [OnboardingStep.FirstMatchDebut] = new OnboardingStepDefinition(
                    Step: OnboardingStep.FirstMatchDebut,
                    TitleKey: "tutorial.step.debut.title",
                    DescriptionKey: "tutorial.step.debut.desc",
                    TargetElementId: "btn-match",
                    ActionPromptKey: "tutorial.step.debut.action",
                    Reward: new OnboardingReward(FormBonus: 10, ManagerTrustBonus: 5),
                    GoalMilestoneKey: "tutorial.goal.debut"
                ),
                [OnboardingStep.HomeApartment] = new OnboardingStepDefinition(
                    Step: OnboardingStep.HomeApartment,
                    TitleKey: "tutorial.step.apartment.title",
                    DescriptionKey: "tutorial.step.apartment.desc",
                    TargetElementId: "btn-home",
                    ActionPromptKey: "tutorial.step.apartment.action",
                    Reward: new OnboardingReward(EnergyBonus: 20),
                    GoalMilestoneKey: "tutorial.goal.apartment"
                ),
                [OnboardingStep.SmartphoneIntro] = new OnboardingStepDefinition(
                    Step: OnboardingStep.SmartphoneIntro,
                    TitleKey: "tutorial.step.phone.title",
                    DescriptionKey: "tutorial.step.phone.desc",
                    TargetElementId: "btn-phone",
                    ActionPromptKey: "tutorial.step.phone.action",
                    Reward: OnboardingReward.None,
                    GoalMilestoneKey: "tutorial.goal.phone"
                ),
                [OnboardingStep.Completed] = new OnboardingStepDefinition(
                    Step: OnboardingStep.Completed,
                    TitleKey: "tutorial.step.completed.title",
                    DescriptionKey: "tutorial.step.completed.desc",
                    TargetElementId: "btn-finish-tutorial",
                    ActionPromptKey: "tutorial.step.completed.action",
                    Reward: OnboardingReward.None,
                    GoalMilestoneKey: "tutorial.goal.completed"
                )
            };

        /// <summary>
        /// Retrieves the definition and localization keys for a specific step.
        /// </summary>
        public OnboardingStepDefinition GetStepDefinition(OnboardingStep step)
        {
            if (_stepDefinitions.TryGetValue(step, out var def))
            {
                return def;
            }
            return _stepDefinitions[OnboardingStep.Welcome];
        }

        /// <summary>
        /// Total number of interactive tutorial steps before completion.
        /// </summary>
        public int TotalStepsCount => 7; // Welcome (0) through SmartphoneIntro (6)

        /// <summary>
        /// Returns 1-based display index of current step.
        /// </summary>
        public int GetStepDisplayIndex(OnboardingStep step)
        {
            return Math.Min((int)step + 1, TotalStepsCount);
        }

        /// <summary>
        /// Deterministically advances the onboarding state by completing the given step.
        /// </summary>
        public OnboardingState AdvanceStep(OnboardingState current, OnboardingStep stepToComplete)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (current.IsCompleted || current.IsSkipped) return current;

            var newCompleted = new List<OnboardingStep>(current.CompletedSteps);
            if (!newCompleted.Contains(stepToComplete))
            {
                newCompleted.Add(stepToComplete);
            }

            var nextStep = (OnboardingStep)((int)stepToComplete + 1);
            bool isFinished = nextStep >= OnboardingStep.Completed;

            return new OnboardingState(
                CurrentStep: isFinished ? OnboardingStep.Completed : nextStep,
                CompletedSteps: newCompleted.ToArray(),
                IsCompleted: isFinished,
                IsSkipped: false
            );
        }

        /// <summary>
        /// Immediately skips the remainder of the tutorial.
        /// </summary>
        public OnboardingState SkipTutorial(OnboardingState current)
        {
            return OnboardingState.Skipped;
        }

        /// <summary>
        /// Resets onboarding progress to allow replaying the tutorial flow.
        /// </summary>
        public OnboardingState ResetForReplay()
        {
            return OnboardingState.Replay;
        }

        /// <summary>
        /// Gathers all unclaimed rewards across remaining incomplete tutorial steps,
        /// ensuring players who skip or fast-forward still receive their starter benefits.
        /// </summary>
        public OnboardingReward GetAllUnclaimedRewards(OnboardingState state)
        {
            if (state == null) return OnboardingReward.None;

            int totalXp = 0;
            int totalEnergy = 0;
            int totalForm = 0;
            int totalTrust = 0;
            int totalCash = 0;

            foreach (var kvp in _stepDefinitions)
            {
                if (!state.HasCompletedStep(kvp.Key) && kvp.Value.Reward != null)
                {
                    totalXp += kvp.Value.Reward.XpBonus;
                    totalEnergy += kvp.Value.Reward.EnergyBonus;
                    totalForm += kvp.Value.Reward.FormBonus;
                    totalTrust += kvp.Value.Reward.ManagerTrustBonus;
                    totalCash += kvp.Value.Reward.CashBonus;
                }
            }

            return new OnboardingReward(
                XpBonus: totalXp,
                EnergyBonus: totalEnergy,
                FormBonus: totalForm,
                ManagerTrustBonus: totalTrust,
                CashBonus: totalCash
            );
        }

        /// <summary>
        /// Evaluates whether a particular game feature is currently unlocked.
        /// Non-linear gates guarantee new players are not overwhelmed in session 1.
        /// </summary>
        public bool IsFeatureUnlocked(OnboardingState state, string featureKey)
        {
            if (state == null || state.IsCompleted || state.IsSkipped) return true;

            string normalized = featureKey?.Trim().ToLowerInvariant() ?? string.Empty;

            switch (normalized)
            {
                case "welcome":
                case "creation":
                    return true;

                case "club_signing":
                case "clubs":
                    return state.CurrentStep >= OnboardingStep.ClubSigning;

                case "training":
                    return state.CurrentStep >= OnboardingStep.FirstTraining;

                case "match":
                    return state.CurrentStep >= OnboardingStep.FirstMatchDebut;

                case "home":
                case "rest":
                    return state.CurrentStep >= OnboardingStep.HomeApartment;

                case "phone":
                    return state.CurrentStep >= OnboardingStep.SmartphoneIntro;

                case "transfers":
                case "sponsorship":
                case "shop":
                case "social":
                case "legacy":
                    return state.IsCompleted;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Applies the starter reward of an onboarding step to current career state.
        /// </summary>
        public void ApplyReward(
            OnboardingReward reward,
            ref int energy,
            ref int form,
            ref int managerTrust,
            ref int bankBalance)
        {
            if (reward == null) return;

            energy = Math.Clamp(energy + reward.EnergyBonus, 0, 100);
            form = Math.Clamp(form + reward.FormBonus, 0, 100);
            managerTrust = Math.Clamp(managerTrust + reward.ManagerTrustBonus, 0, 100);
            bankBalance = Math.Max(0, bankBalance + reward.CashBonus);
        }

        /// <summary>
        /// Synchronizes OnboardingState into CareerSaveData persistence.
        /// </summary>
        public void SyncToSaveData(OnboardingState state, CareerSaveData save)
        {
            if (state == null || save == null) return;

            save.IsTutorialCompleted = state.IsCompleted;
            save.IsTutorialSkipped = state.IsSkipped;
            save.CurrentTutorialStep = state.CurrentStep.ToString();

            save.CompletedTutorialSteps.Clear();
            if (state.CompletedSteps != null)
            {
                foreach (var step in state.CompletedSteps)
                {
                    save.CompletedTutorialSteps.Add(step.ToString());
                }
            }
        }

        /// <summary>
        /// Reconstructs OnboardingState from CareerSaveData persistence.
        /// </summary>
        public OnboardingState LoadFromSaveData(CareerSaveData save)
        {
            if (save == null) return OnboardingState.Initial;

            if (save.IsTutorialSkipped) return OnboardingState.Skipped;

            if (save.IsTutorialCompleted)
            {
                return new OnboardingState(
                    CurrentStep: OnboardingStep.Completed,
                    CompletedSteps: new[]
                    {
                        OnboardingStep.Welcome,
                        OnboardingStep.CharacterCreation,
                        OnboardingStep.ClubSigning,
                        OnboardingStep.FirstTraining,
                        OnboardingStep.FirstMatchDebut,
                        OnboardingStep.HomeApartment,
                        OnboardingStep.SmartphoneIntro
                    },
                    IsCompleted: true,
                    IsSkipped: false
                );
            }

            var currentStep = OnboardingStep.Welcome;
            if (Enum.TryParse<OnboardingStep>(save.CurrentTutorialStep, out var parsed))
            {
                currentStep = parsed;
            }

            var completedList = new List<OnboardingStep>();
            if (save.CompletedTutorialSteps != null)
            {
                foreach (var str in save.CompletedTutorialSteps)
                {
                    if (Enum.TryParse<OnboardingStep>(str, out var stepEnum))
                    {
                        completedList.Add(stepEnum);
                    }
                }
            }

            return new OnboardingState(
                CurrentStep: currentStep,
                CompletedSteps: completedList.ToArray(),
                IsCompleted: currentStep == OnboardingStep.Completed,
                IsSkipped: false
            );
        }
    }
}
