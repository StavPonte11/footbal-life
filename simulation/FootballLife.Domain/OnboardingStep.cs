using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Discrete steps in the First-Time User Experience (FTUE) and onboarding tutorial.
    /// </summary>
    public enum OnboardingStep
    {
        Welcome = 0,
        CharacterCreation = 1,
        ClubSigning = 2,
        FirstTraining = 3,
        FirstMatchDebut = 4,
        HomeApartment = 5,
        SmartphoneIntro = 6,
        Completed = 7
    }

    /// <summary>
    /// Starter bonus reward awarded upon completing a tutorial step.
    /// </summary>
    public sealed record OnboardingReward(
        int XpBonus = 0,
        int EnergyBonus = 0,
        int FormBonus = 0,
        int ManagerTrustBonus = 0,
        int CashBonus = 0
    )
    {
        public static readonly OnboardingReward None = new OnboardingReward();
    }

    /// <summary>
    /// Metadata definition describing an onboarding tutorial step and its UI bindings.
    /// </summary>
    public sealed record OnboardingStepDefinition(
        OnboardingStep Step,
        string TitleKey,
        string DescriptionKey,
        string TargetElementId,
        string ActionPromptKey,
        OnboardingReward Reward,
        string? GoalMilestoneKey = null
    );

    /// <summary>
    /// Immutable state of the player's onboarding progress.
    /// </summary>
    public sealed record OnboardingState(
        OnboardingStep CurrentStep,
        IReadOnlyList<OnboardingStep> CompletedSteps,
        bool IsCompleted,
        bool IsSkipped
    )
    {
        public static OnboardingState Initial => new OnboardingState(
            CurrentStep: OnboardingStep.Welcome,
            CompletedSteps: Array.Empty<OnboardingStep>(),
            IsCompleted: false,
            IsSkipped: false
        );

        public static OnboardingState Replay => new OnboardingState(
            CurrentStep: OnboardingStep.Welcome,
            CompletedSteps: Array.Empty<OnboardingStep>(),
            IsCompleted: false,
            IsSkipped: false
        );

        public static OnboardingState Skipped => new OnboardingState(
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
            IsSkipped: true
        );

        public bool HasCompletedStep(OnboardingStep step)
        {
            if (CompletedSteps == null) return false;
            for (int i = 0; i < CompletedSteps.Count; i++)
            {
                if (CompletedSteps[i] == step) return true;
            }
            return false;
        }
    }
}
