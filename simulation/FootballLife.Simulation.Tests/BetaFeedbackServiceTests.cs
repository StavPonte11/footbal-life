using System;
using System.IO;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public sealed class BetaFeedbackServiceTests : IDisposable
    {
        private readonly string _testFeedbackDir;
        private readonly BetaFeedbackService _feedbackService;

        public BetaFeedbackServiceTests()
        {
            _testFeedbackDir = Path.Combine(Path.GetTempPath(), "fl_feedback_tests_" + Guid.NewGuid().ToString("N"));
            _feedbackService = new BetaFeedbackService(_testFeedbackDir);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testFeedbackDir))
                {
                    Directory.Delete(_testFeedbackDir, true);
                }
            }
            catch { }
        }

        [Fact]
        public void SubmitFeedback_ValidPayload_SavesToMemoryAndDisk()
        {
            bool eventFired = false;
            _feedbackService.OnFeedbackSubmitted += report => eventFired = true;

            var report = _feedbackService.SubmitFeedback(
                category: FeedbackCategory.MatchEngine,
                satisfactionRating: 5,
                comment: "The curling shots feel incredibly smooth and responsive on mobile!",
                contactEmail: "tester@example.com"
            );

            Assert.True(eventFired);
            Assert.NotNull(report);
            Assert.Equal(FeedbackCategory.MatchEngine, report.Category);
            Assert.Equal(5, report.SatisfactionRating);
            Assert.Equal(1, _feedbackService.QueuedCount);

            // Check disk file
            string expectedFile = Path.Combine(_testFeedbackDir, $"feedback_{report.FeedbackId}.json");
            Assert.True(File.Exists(expectedFile));
            string json = File.ReadAllText(expectedFile);
            Assert.Contains("curling shots", json);
        }

        [Theory]
        [InlineData(0, 1)] // Clamp to 1
        [InlineData(6, 5)] // Clamp to 5
        [InlineData(4, 4)]
        public void SubmitFeedback_ClampsSatisfactionRating(int inputRating, int expectedRating)
        {
            var report = _feedbackService.SubmitFeedback(
                category: FeedbackCategory.UIUX,
                satisfactionRating: inputRating,
                comment: "Testing rating clamping behavior"
            );

            Assert.Equal(expectedRating, report.SatisfactionRating);
        }

        [Fact]
        public void SubmitFeedback_EmptyComment_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _feedbackService.SubmitFeedback(
                    category: FeedbackCategory.Bug,
                    satisfactionRating: 3,
                    comment: "   "
                );
            });
        }

        [Fact]
        public void FlushStoredFeedback_DispatchesAndDeletesDiskFiles()
        {
            _feedbackService.SubmitFeedback(FeedbackCategory.Bug, 2, "Bug report 1");
            _feedbackService.SubmitFeedback(FeedbackCategory.UIUX, 4, "UI feedback 2");

            int dispatched = 0;
            int flushedCount = _feedbackService.FlushStoredFeedback(report =>
            {
                dispatched++;
                Assert.NotNull(report);
            });

            Assert.Equal(2, flushedCount);
            Assert.Equal(2, dispatched);

            // Verify disk directory is empty
            var remainingFiles = Directory.GetFiles(_testFeedbackDir, "feedback_*.json");
            Assert.Empty(remainingFiles);
        }
    }
}
