using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class ManagerTests
    {
        private readonly Guid _testManagerId = Guid.NewGuid();
        private readonly Guid _testClubId = Guid.NewGuid();

        [Fact]
        public void Manager_CreatedWithValidData_InitializesPropertiesCorrectly()
        {
            var manager = new Manager(
                _testManagerId,
                "Mikel Arteta",
                Formation.F433,
                TacticalIdentity.Possession,
                trustDecayRate: 0.08f,
                toleranceThreshold: 0.45f,
                reputationRating: 84,
                clubId: _testClubId);

            Assert.Equal(_testManagerId, manager.Id);
            Assert.Equal("Mikel Arteta", manager.Name);
            Assert.Equal(Formation.F433, manager.PreferredFormation);
            Assert.Equal(TacticalIdentity.Possession, manager.TacticalStyle);
            Assert.Equal(0.08f, manager.TrustDecayRate);
            Assert.Equal(0.45f, manager.ToleranceThreshold);
            Assert.Equal(84, manager.ReputationRating);
            Assert.Equal(_testClubId, manager.ClubId);
        }

        [Fact]
        public void Manager_CreateFactory_GeneratesUniqueIdAndDefaults()
        {
            var m1 = Manager.Create("Pep Guardiola", Formation.F433, TacticalIdentity.Possession);
            var m2 = Manager.Create("Jurgen Klopp", Formation.F433, TacticalIdentity.HighPress);

            Assert.NotEqual(Guid.Empty, m1.Id);
            Assert.NotEqual(Guid.Empty, m2.Id);
            Assert.NotEqual(m1.Id, m2.Id);
            Assert.Null(m1.ClubId);
            Assert.Equal(0.05f, m1.TrustDecayRate);
            Assert.Equal(0.40f, m1.ToleranceThreshold);
            Assert.Equal(50, m1.ReputationRating);
        }

        [Fact]
        public void Manager_WithEmptyId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>("id", () =>
                new Manager(Guid.Empty, "Manager", Formation.F442, TacticalIdentity.Direct, 0.05f, 0.40f));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Manager_WithNullOrWhitespaceName_ThrowsArgumentException(string? invalidName)
        {
            Assert.Throws<ArgumentException>("name", () =>
                new Manager(_testManagerId, invalidName!, Formation.F442, TacticalIdentity.Direct, 0.05f, 0.40f));
        }

        [Theory]
        [InlineData(-0.01f)]
        [InlineData(1.05f)]
        [InlineData(float.NaN)]
        public void Manager_TrustDecayOutOfBounds_ThrowsArgumentOutOfRangeException(float invalidDecay)
        {
            Assert.Throws<ArgumentOutOfRangeException>("trustDecayRate", () =>
                new Manager(_testManagerId, "Name", Formation.F442, TacticalIdentity.Direct, invalidDecay, 0.40f));
        }

        [Theory]
        [InlineData(-0.01f)]
        [InlineData(1.05f)]
        [InlineData(float.NaN)]
        public void Manager_ToleranceOutOfBounds_ThrowsArgumentOutOfRangeException(float invalidTolerance)
        {
            Assert.Throws<ArgumentOutOfRangeException>("toleranceThreshold", () =>
                new Manager(_testManagerId, "Name", Formation.F442, TacticalIdentity.Direct, 0.05f, invalidTolerance));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        [InlineData(101)]
        public void Manager_ReputationOutOfBounds_ThrowsArgumentOutOfRangeException(int invalidRep)
        {
            Assert.Throws<ArgumentOutOfRangeException>("reputationRating", () =>
                new Manager(_testManagerId, "Name", Formation.F442, TacticalIdentity.Direct, 0.05f, 0.40f, invalidRep));
        }

        // ─── Trust Decay Calculations ─────────────────────────────────────────

        [Fact]
        public void Manager_CalculateDecayedTrust_ReducesTrustProportionally()
        {
            var manager = Manager.Create("Coach", Formation.F4231, TacticalIdentity.Counter, trustDecayRate: 0.10f);

            // Starting trust: 80. Decay per match = 80 * 0.10 = 8.
            float updated = manager.CalculateDecayedTrust(80f, matchesBenched: 1);
            Assert.Equal(72f, updated, precision: 2);

            // 2 matches benched = 80 - (80 * 0.10 * 2) = 64
            float updated2 = manager.CalculateDecayedTrust(80f, matchesBenched: 2);
            Assert.Equal(64f, updated2, precision: 2);
        }

        [Fact]
        public void Manager_CalculateDecayedTrust_ClampsAtMinimum()
        {
            var manager = Manager.Create("Harsh Coach", Formation.F442, TacticalIdentity.Direct, trustDecayRate: 0.50f);
            float updated = manager.CalculateDecayedTrust(20f, matchesBenched: 5);

            Assert.Equal(0f, updated);
        }

        [Fact]
        public void Manager_CalculateDecayedTrust_ZeroOrNegativeMatches_DoesNotDecay()
        {
            var manager = Manager.Create("Coach", Formation.F442, TacticalIdentity.Direct, trustDecayRate: 0.10f);

            Assert.Equal(80f, manager.CalculateDecayedTrust(80f, 0));
            Assert.Equal(80f, manager.CalculateDecayedTrust(80f, -2));
        }

        // ─── Squad Selection Threshold ────────────────────────────────────────

        [Fact]
        public void Manager_ShouldDropFromStartingXI_CorrectlyEvaluatesAgainstTolerance()
        {
            // Tolerance threshold = 0.40 (i.e. trust < 40/100 should be dropped)
            var manager = Manager.Create("Coach", Formation.F433, TacticalIdentity.Possession, toleranceThreshold: 0.40f);

            Assert.True(manager.ShouldDropFromStartingXI(35f));
            Assert.True(manager.ShouldDropFromStartingXI(39.9f));
            Assert.False(manager.ShouldDropFromStartingXI(40f));
            Assert.False(manager.ShouldDropFromStartingXI(75f));
        }

        // ─── Immutability ─────────────────────────────────────────────────────

        [Fact]
        public void Manager_WithClub_UpdatesClubIdImmutably()
        {
            var manager = Manager.Create("Coach", Formation.F433, TacticalIdentity.Possession);
            var newClubId = Guid.NewGuid();

            var employed = manager.WithClub(newClubId);
            var unemployed = employed.WithClub(null);

            Assert.Null(manager.ClubId);
            Assert.Equal(newClubId, employed.ClubId);
            Assert.Null(unemployed.ClubId);
        }

        [Fact]
        public void Formation_AllFiveFormationsAreDefined()
        {
            var formations = Enum.GetValues(typeof(Formation));
            Assert.Equal(5, formations.Length);
        }
    }
}
