using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class ManagerChangeSystemTests
    {
        private static Season CreateTestSeason()
        {
            var leagueId = Guid.NewGuid();
            var clubId = Guid.NewGuid();
            var table = LeagueTable.Create(leagueId, new[] { clubId });
            return new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), table, new List<Matchday>());
        }

        private static Club CreateTestClub(string name, int rep = 75, BoardPatience patience = BoardPatience.Balanced)
        {
            var finances = new ClubFinances(weeklyWageBudget: 500_000m, transferBudget: 10_000_000m);
            var board = new ClubBoard("Board", patience, 3);
            return Club.Create(
                name: name,
                shortName: name.Substring(0, Math.Min(3, name.Length)).ToUpperInvariant(),
                leagueId: Guid.NewGuid(),
                reputationRating: rep,
                finances: finances,
                facilityRating: 4,
                tacticalStyle: TacticalIdentity.Possession,
                board: board);
        }

        [Fact]
        public void EvaluateManagerPerformance_ChampionsAndPromoted_ZeroSackProbability()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var club = CreateTestClub("Title Winner", rep: 85);

            var titleOutcome = new ClubSeasonOutcome
            {
                ClubId = club.Id,
                ClubName = club.Name,
                LeagueId = club.LeagueId,
                FinalPosition = 1,
                WonTitle = true
            };

            var promotedOutcome = new ClubSeasonOutcome
            {
                ClubId = club.Id,
                ClubName = club.Name,
                LeagueId = club.LeagueId,
                FinalPosition = 2,
                Promoted = true
            };

            Assert.Equal(0.0f, ManagerChangeSystem.EvaluateManagerPerformance(club, titleOutcome, world));
            Assert.Equal(0.0f, ManagerChangeSystem.EvaluateManagerPerformance(club, promotedOutcome, world));
        }

        [Fact]
        public void EvaluateManagerPerformance_Relegation_HasHighSackRisk()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var club = CreateTestClub("Relegated Club", rep: 65);

            var relegatedOutcome = new ClubSeasonOutcome
            {
                ClubId = club.Id,
                ClubName = club.Name,
                LeagueId = club.LeagueId,
                FinalPosition = 20,
                Relegated = true
            };

            float risk = ManagerChangeSystem.EvaluateManagerPerformance(club, relegatedOutcome, world);
            Assert.True(risk >= 0.75f);
        }

        [Fact]
        public void EvaluateManagerPerformance_BoardPatienceInfluencesRisk()
        {
            var leagueId = Guid.NewGuid();
            var clubImpatient = CreateTestClub("Impatient Club", rep: 80, BoardPatience.Impatient) with { LeagueId = leagueId };
            var clubPatient = CreateTestClub("Patient Club", rep: 80, BoardPatience.Patient) with { LeagueId = leagueId };

            var world = WorldState.CreateEmpty(CreateTestSeason()).WithClubs(new[] { clubImpatient, clubPatient });

            var mediocreOutcome = new ClubSeasonOutcome
            {
                FinalPosition = 15,
                Relegated = false,
                WonTitle = false
            };

            float impatientRisk = ManagerChangeSystem.EvaluateManagerPerformance(
                clubImpatient, mediocreOutcome with { ClubId = clubImpatient.Id, LeagueId = leagueId }, world);

            float patientRisk = ManagerChangeSystem.EvaluateManagerPerformance(
                clubPatient, mediocreOutcome with { ClubId = clubPatient.Id, LeagueId = leagueId }, world);

            Assert.True(impatientRisk > patientRisk);
        }

        [Fact]
        public void GenerateNewManager_CreatesTierAppropriateManager()
        {
            var club = CreateTestClub("Elite Club", rep: 85);
            var rng = new SimulationRandom(42);

            var manager = ManagerChangeSystem.GenerateNewManager(club, rng);

            Assert.NotNull(manager.Name);
            Assert.True(manager.ReputationRating >= 75 && manager.ReputationRating <= 95);
            Assert.Equal(club.Id, manager.ClubId);
            Assert.True(manager.TrustDecayRate >= 0.03f && manager.TrustDecayRate <= 0.08f);
            Assert.True(manager.ToleranceThreshold >= 0.30f && manager.ToleranceThreshold <= 0.50f);
        }

        [Fact]
        public void TriggerManagerChange_AppointsNewManagerAndResetsTrust()
        {
            var club = CreateTestClub("Arsenal", rep: 88);

            var oldMgr = Manager.Create("Old Manager", Formation.F433, TacticalIdentity.HighPress, clubId: club.Id);
            club = club.WithManager(oldMgr.Id);

            var playerId = Guid.NewGuid();
            var player = new Player(playerId, "Bukayo Saka", "England", new DateOnly(2001, 9, 5), Foot.Left, Position.RW);
            var abilities = PlayerAbilities.CreateUniform(85);
            var state = PlayerState.Default with { Form = 70f, Fatigue = 20f, Confidence = 80f };
            var career = new PlayerCareerState(club.Id, SquadStatus.Starter, managerTrust: 95f, 150000m, 120000000m, 85f);
            club = club.WithAddedPlayer(playerId);

            var world = WorldState.CreateEmpty(CreateTestSeason())
                .WithClub(club)
                .WithManager(oldMgr)
                .WithPlayer(player, abilities, state, career);

            var rng = new SimulationRandom(999);
            var date = new DateOnly(2026, 11, 1);

            var (updatedWorld, changeEvent) = ManagerChangeSystem.TriggerManagerChange(
                club, ManagerChangeReason.Sacked, world, rng, date, playerFocusId: playerId);

            Assert.Equal(club.Id, changeEvent.ClubId);
            Assert.Equal(oldMgr.Id, changeEvent.OldManagerId);
            Assert.NotEqual(oldMgr.Id, changeEvent.NewManagerId);
            Assert.Equal(ManagerChangeReason.Sacked, changeEvent.Reason);
            Assert.Equal(50.0f, changeEvent.PlayerTrustReset);

            var updatedClub = updatedWorld.GetClub(club.Id);
            Assert.Equal(changeEvent.NewManagerId, updatedClub.ManagerId);

            var updatedOldMgr = updatedWorld.Managers[oldMgr.Id];
            Assert.Null(updatedOldMgr.ClubId);

            var updatedCareer = updatedWorld.GetCareerState(playerId);
            Assert.Equal(50.0f, updatedCareer.ManagerTrust);

            Assert.Contains(changeEvent, updatedWorld.ManagerChangeHistory);
        }

        [Fact]
        public void ApplyTrustReset_ResetsPlayerTrustDirectly()
        {
            var playerId = Guid.NewGuid();
            var player = new Player(playerId, "Young Player", "England", new DateOnly(2005, 1, 1), Foot.Right, Position.ST);
            var abilities = PlayerAbilities.CreateUniform(60);
            var state = PlayerState.Default;
            var career = new PlayerCareerState(Guid.NewGuid(), SquadStatus.Starter, managerTrust: 15f, 1000m, 50000m, 40f);

            var world = WorldState.CreateEmpty(CreateTestSeason())
                .WithPlayer(player, abilities, state, career);

            var updatedWorld = ManagerChangeSystem.ApplyTrustReset(playerId, world, 50.0f);
            Assert.Equal(50.0f, updatedWorld.GetCareerState(playerId).ManagerTrust);
        }
    }
}
