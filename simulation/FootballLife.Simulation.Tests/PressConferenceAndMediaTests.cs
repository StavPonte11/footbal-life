using System;
using System.Linq;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public sealed class PressConferenceAndMediaTests
    {
        [Fact]
        public void GeneratePressConference_WinContext_IncludesWinQuestions()
        {
            var save = new CareerSaveData { PlayerName = "Leo Swift", ClubName = "Riverside FC" };
            var winResult = new MatchResult(
                homeScore: 3,
                awayScore: 1,
                events: null,
                playerRating: 8.5f,
                playerMinutesPlayed: 90,
                playerScored: true,
                playerAssisted: false);

            var pc = PressConferenceSystem.GeneratePressConference(save, winResult);

            Assert.NotNull(pc);
            Assert.Contains("Post-Match", pc.Title, StringComparison.OrdinalIgnoreCase);
            Assert.NotEmpty(pc.Questions);
            Assert.Contains(pc.Questions, q => q.QuestionText.Contains("defining performance") || q.QuestionText.Contains("Sensational"));
        }

        [Fact]
        public void GeneratePressConference_LossContext_IncludesAccountabilityQuestions()
        {
            var save = new CareerSaveData { PlayerName = "Leo Swift", ClubName = "Riverside FC" };
            var lossResult = new MatchResult(
                homeScore: 0,
                awayScore: 2,
                events: null,
                playerRating: 5.8f,
                playerMinutesPlayed: 90,
                playerScored: false,
                playerAssisted: false);

            var pc = PressConferenceSystem.GeneratePressConference(save, lossResult);

            Assert.NotNull(pc);
            Assert.NotEmpty(pc.Questions);
            Assert.Contains(pc.Questions, q => q.QuestionText.Contains("defeat") || q.QuestionText.Contains("disjointed"));
        }

        [Fact]
        public void AnswerQuestion_HumbleChoice_BoostsManagerTrustAndTeamMorale()
        {
            var save = new CareerSaveData
            {
                ManagerTrust = 50,
                Morale = 60,
                FanPopularity = 50,
                MediaReputation = 50
            };

            var question = new PressQuestion
            {
                Id = "q_test",
                QuestionText = "How do you reflect on the match?",
                Responses = new[]
                {
                    new PressResponseChoice
                    {
                        Tone = PressTone.Humble,
                        Text = "Credit to the team and manager.",
                        ManagerTrustDelta = +6f,
                        TeammateMoraleDelta = +5f,
                        FanPopularityDelta = +3f,
                        MediaReputationDelta = +4f,
                        ConsequenceSummary = "Shows humility and team spirit."
                    }
                }
            };

            var result = PressConferenceSystem.AnswerQuestion(save, question, question.Responses[0]);

            Assert.True(result.Success);
            Assert.Equal(56, save.ManagerTrust);
            Assert.Equal(65, save.Morale);
            Assert.Equal(53, save.FanPopularity);
            Assert.Equal(54, save.MediaReputation);
        }

        [Fact]
        public void AnswerQuestion_DefiantChoice_PenalizesManagerTrust_BoostsFans()
        {
            var save = new CareerSaveData
            {
                ManagerTrust = 50,
                Morale = 60,
                FanPopularity = 50,
                MediaReputation = 50
            };

            var question = new PressQuestion
            {
                Id = "q_test",
                QuestionText = "Were you frustrated by the substitutions?",
                Responses = new[]
                {
                    new PressResponseChoice
                    {
                        Tone = PressTone.Defiant,
                        Text = "The decision made no sense.",
                        ManagerTrustDelta = -8f,
                        TeammateMoraleDelta = -3f,
                        FanPopularityDelta = +7f,
                        MediaReputationDelta = -5f,
                        ConsequenceSummary = "Controversial outburst."
                    }
                }
            };

            var result = PressConferenceSystem.AnswerQuestion(save, question, question.Responses[0]);

            Assert.True(result.Success);
            Assert.Equal(42, save.ManagerTrust);
            Assert.Equal(57, save.Morale);
            Assert.Equal(57, save.FanPopularity);
            Assert.Equal(45, save.MediaReputation);
        }

        [Fact]
        public void MediaFeedSystem_GeneratesCuratedArticles_WithDynamicMetrics()
        {
            var save = new CareerSaveData
            {
                PlayerName = "Alex Mercer",
                ClubName = "London Athletic",
                OverallRating = 74,
                MarketValue = 250000,
                FanPopularity = 65
            };

            var articles = MediaFeedSystem.GenerateArticles(save);

            Assert.NotNull(articles);
            Assert.True(articles.Count >= 3);
            Assert.Contains(articles, a => a.CategoryTag == "TRANSFER");
            Assert.Contains(articles, a => a.CategoryTag == "LIFESTYLE");
            Assert.All(articles, a => Assert.True(a.LikesCount > 0 && a.ViewsCount > 0));
        }

        [Fact]
        public void BuildLLMPrompt_FormatsValidPrompt()
        {
            var save = new CareerSaveData { PlayerName = "Leo", ClubName = "Arsenal" };
            string prompt = MediaFeedSystem.BuildLLMPrompt(save, "Scored a hat-trick in the derby.");

            Assert.Contains("Leo", prompt);
            Assert.Contains("Arsenal", prompt);
            Assert.Contains("JSON", prompt);
        }
    }
}
