using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class OnboardingSystemTests
    {
        [Fact]
        public void OnboardingSystem_InitialState_IsWelcomeAndNotCompleted()
        {
            var system = new OnboardingSystem();
            var state = OnboardingState.Initial;

            Assert.Equal(OnboardingStep.Welcome, state.CurrentStep);
            Assert.False(state.IsCompleted);
            Assert.False(state.IsSkipped);
            Assert.Empty(state.CompletedSteps);
        }

        [Fact]
        public void OnboardingSystem_SequentialProgression_ReachesCompleted()
        {
            var system = new OnboardingSystem();
            var state = OnboardingState.Initial;

            var steps = new[]
            {
                OnboardingStep.Welcome,
                OnboardingStep.CharacterCreation,
                OnboardingStep.ClubSigning,
                OnboardingStep.FirstTraining,
                OnboardingStep.FirstMatchDebut,
                OnboardingStep.HomeApartment,
                OnboardingStep.SmartphoneIntro
            };

            foreach (var step in steps)
            {
                Assert.False(state.IsCompleted);
                state = system.AdvanceStep(state, step);
                Assert.True(state.HasCompletedStep(step));
            }

            Assert.Equal(OnboardingStep.Completed, state.CurrentStep);
            Assert.True(state.IsCompleted);
            Assert.False(state.IsSkipped);
        }

        [Fact]
        public void OnboardingSystem_FeatureGating_LocksAndUnlocksAppropriately()
        {
            var system = new OnboardingSystem();
            var state = OnboardingState.Initial;

            // Welcome stage: only basic welcome/creation unlocked
            Assert.True(system.IsFeatureUnlocked(state, "welcome"));
            Assert.True(system.IsFeatureUnlocked(state, "creation"));
            Assert.False(system.IsFeatureUnlocked(state, "training"));
            Assert.False(system.IsFeatureUnlocked(state, "match"));
            Assert.False(system.IsFeatureUnlocked(state, "transfers"));
            Assert.False(system.IsFeatureUnlocked(state, "sponsorship"));

            // Advance through CharacterCreation & ClubSigning
            state = system.AdvanceStep(state, OnboardingStep.Welcome);
            state = system.AdvanceStep(state, OnboardingStep.CharacterCreation);
            state = system.AdvanceStep(state, OnboardingStep.ClubSigning);

            // Now at FirstTraining: training unlocked, but match & transfers still locked
            Assert.Equal(OnboardingStep.FirstTraining, state.CurrentStep);
            Assert.True(system.IsFeatureUnlocked(state, "training"));
            Assert.False(system.IsFeatureUnlocked(state, "match"));
            Assert.False(system.IsFeatureUnlocked(state, "transfers"));

            // Complete tutorial
            state = system.SkipTutorial(state);
            Assert.True(state.IsCompleted);
            Assert.True(state.IsSkipped);
            Assert.True(system.IsFeatureUnlocked(state, "transfers"));
            Assert.True(system.IsFeatureUnlocked(state, "sponsorship"));
            Assert.True(system.IsFeatureUnlocked(state, "shop"));
        }

        [Fact]
        public void OnboardingSystem_StarterRewards_ApplyCorrectly()
        {
            var system = new OnboardingSystem();

            int energy = 50;
            int form = 50;
            int trust = 50;
            int balance = 1000;

            var reward = new OnboardingReward(
                EnergyBonus: 20,
                FormBonus: 10,
                ManagerTrustBonus: 15,
                CashBonus: 500
            );

            system.ApplyReward(reward, ref energy, ref form, ref trust, ref balance);

            Assert.Equal(70, energy);
            Assert.Equal(60, form);
            Assert.Equal(65, trust);
            Assert.Equal(1500, balance);
        }

        [Fact]
        public void OnboardingSystem_SaveData_RoundtripsAccurately()
        {
            var system = new OnboardingSystem();
            var state = OnboardingState.Initial;

            // Advance two steps
            state = system.AdvanceStep(state, OnboardingStep.Welcome);
            state = system.AdvanceStep(state, OnboardingStep.CharacterCreation);

            var save = new CareerSaveData();
            system.SyncToSaveData(state, save);

            Assert.Equal(OnboardingStep.ClubSigning.ToString(), save.CurrentTutorialStep);
            Assert.Contains(OnboardingStep.Welcome.ToString(), save.CompletedTutorialSteps);
            Assert.Contains(OnboardingStep.CharacterCreation.ToString(), save.CompletedTutorialSteps);

            // Restore from save
            var restored = system.LoadFromSaveData(save);
            Assert.Equal(OnboardingStep.ClubSigning, restored.CurrentStep);
            Assert.True(restored.HasCompletedStep(OnboardingStep.Welcome));
            Assert.True(restored.HasCompletedStep(OnboardingStep.CharacterCreation));
            Assert.False(restored.HasCompletedStep(OnboardingStep.ClubSigning));
            Assert.False(restored.IsCompleted);
        }

        [Fact]
        public void OnboardingSystem_ResetForReplay_RestoresInitialStep()
        {
            var system = new OnboardingSystem();
            var state = OnboardingState.Skipped;

            Assert.True(state.IsCompleted);
            Assert.True(state.IsSkipped);

            var replayed = system.ResetForReplay();

            Assert.Equal(OnboardingStep.Welcome, replayed.CurrentStep);
            Assert.False(replayed.IsCompleted);
            Assert.False(replayed.IsSkipped);
            Assert.Empty(replayed.CompletedSteps);
        }

        [Fact]
        public void OnboardingSystem_GetAllUnclaimedRewards_AggregatesPendingBonuses()
        {
            var system = new OnboardingSystem();
            var state = OnboardingState.Initial;

            var rewards = system.GetAllUnclaimedRewards(state);

            Assert.True(rewards.CashBonus >= 500);
            Assert.True(rewards.XpBonus >= 50);
            Assert.True(rewards.EnergyBonus >= 30);
            Assert.True(rewards.FormBonus >= 15);
            Assert.True(rewards.ManagerTrustBonus >= 15);
        }
    }
}
