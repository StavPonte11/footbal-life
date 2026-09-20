using System;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class RandomNameGeneratorTests
    {
        [Theory]
        [InlineData("England")]
        [InlineData("France")]
        [InlineData("Germany")]
        [InlineData("Spain")]
        [InlineData("Italy")]
        [InlineData("Brazil")]
        [InlineData("Argentina")]
        [InlineData("Netherlands")]
        [InlineData("Portugal")]
        [InlineData("USA")]
        public void GenerateFullName_ProducesNonEmptyNames_ForSupportedNationalities(string nationality)
        {
            var (first, last) = RandomNameGenerator.GenerateFullName(nationality);

            Assert.False(string.IsNullOrWhiteSpace(first));
            Assert.False(string.IsNullOrWhiteSpace(last));
        }

        [Fact]
        public void GenerateFullName_WithUnknownNationality_FallsBackSafely()
        {
            var (first, last) = RandomNameGenerator.GenerateFullName("Atlantis");

            Assert.False(string.IsNullOrWhiteSpace(first));
            Assert.False(string.IsNullOrWhiteSpace(last));
        }

        [Fact]
        public void GenerateFirstNameAndLastName_WithSeed_IsDeterministic()
        {
            var rng1 = new SimulationRandom(777);
            var rng2 = new SimulationRandom(777);

            string first1 = RandomNameGenerator.GenerateFirstName("France", rng1);
            string first2 = RandomNameGenerator.GenerateFirstName("France", rng2);

            Assert.Equal(first1, first2);
        }
    }
}
