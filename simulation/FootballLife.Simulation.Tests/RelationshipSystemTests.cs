using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class RelationshipSystemTests
    {
        private static readonly DateOnly TestDate = new DateOnly(2026, 8, 1);

        // ─── P1-056: Affinity Decay Tests ─────────────────────────────────────

        [Fact]
        public void Relationship_Affinity_DecaysFromNeglect()
        {
            var pId = Guid.NewGuid();
            var friend = Relationship.Create(pId, "Jack (Mate)", RelationshipType.Friend, initialAffinity: 60f, initialTrust: 50f, initialDate: TestDate);

            var decayed = RelationshipSystem.ApplyWeeklyDecay(friend, 0.5f);

            Assert.Equal(59.5f, decayed.Affinity);
        }

        [Fact]
        public void Relationship_FamilyAffinity_HasMinimumFloor()
        {
            var pId = Guid.NewGuid();
            var parent = Relationship.Create(pId, "Dad", RelationshipType.Parent, initialAffinity: 21f, initialTrust: 70f, initialDate: TestDate);

            // Applying 10 weeks of neglect (5.0 total decay)
            var decayed = parent;
            for (int i = 0; i < 10; i++)
            {
                decayed = RelationshipSystem.ApplyWeeklyDecay(decayed, 0.5f);
            }

            // Must hit the family floor of 20.0, never below
            Assert.Equal(20.0f, decayed.Affinity);
        }

        [Fact]
        public void RelationshipSystem_NonFamily_DecaysToZeroFromLongNeglect()
        {
            var pId = Guid.NewGuid();
            var teammate = Relationship.Create(pId, "Former Teammate", RelationshipType.Teammate, initialAffinity: 5f, initialTrust: 20f, initialDate: TestDate);

            // 20 weeks of neglect (10.0 total decay)
            var decayed = teammate;
            for (int i = 0; i < 20; i++)
            {
                decayed = RelationshipSystem.ApplyWeeklyDecay(decayed, 0.5f);
            }

            // Non-family relationships can decay completely to 0
            Assert.Equal(0.0f, decayed.Affinity);
        }

        [Fact]
        public void RelationshipSystem_ApplyDecayOverTime_CalculatesWeeksAccurately()
        {
            var pId = Guid.NewGuid();
            var friend = Relationship.Create(pId, "School Friend", RelationshipType.Friend, initialAffinity: 70f, initialTrust: 50f, initialDate: TestDate);

            // 28 days later = 4 weeks elapsed -> 4 * 0.5 = 2.0 decay
            var fourWeeksLater = TestDate.AddDays(28);
            var decayed = RelationshipSystem.ApplyDecayOverTime(friend, fourWeeksLater, 0.5f);

            Assert.Equal(68.0f, decayed.Affinity);
        }

        [Fact]
        public void RelationshipSystem_RecentInteraction_NoDecayWithinSameWeek()
        {
            var pId = Guid.NewGuid();
            var partner = Relationship.Create(pId, "Chloe (Partner)", RelationshipType.Partner, initialAffinity: 85f, initialTrust: 90f, initialDate: TestDate);

            // 3 days later -> less than 7 days, no decay applied
            var threeDaysLater = TestDate.AddDays(3);
            var updated = RelationshipSystem.ApplyDecayOverTime(partner, threeDaysLater, 0.5f);

            Assert.Equal(85.0f, updated.Affinity);
        }

        // ─── P1-057: Interaction Events Tests ─────────────────────────────────

        [Fact]
        public void RelationshipSystem_PositiveInteraction_IncreasesAffinityAndAppendsHistory()
        {
            var pId = Guid.NewGuid();
            var friend = Relationship.Create(pId, "Sam", RelationshipType.Friend, initialAffinity: 60f, initialTrust: 50f, initialDate: TestDate);

            var interactionDate = TestDate.AddDays(7);
            var updated = RelationshipSystem.RecordInteraction(friend, interactionDate, 12f, 8f, "Celebrated match win together over dinner");

            Assert.Equal(72f, updated.Affinity);
            Assert.Equal(58f, updated.Trust);
            Assert.Equal(interactionDate, updated.LastInteraction);
            Assert.Contains(updated.SharedHistory, h => h.Contains("Celebrated match win"));
        }

        [Fact]
        public void RelationshipSystem_NegativeInteraction_ReducesTrust()
        {
            var pId = Guid.NewGuid();
            var agent = Relationship.Create(pId, "Agent Smith", RelationshipType.Agent, initialAffinity: 50f, initialTrust: 30f, initialDate: TestDate);

            var interactionDate = TestDate.AddDays(14);
            var updated = RelationshipSystem.RecordInteraction(agent, interactionDate, -15f, -35f, "Dispute over unapproved media interview");

            Assert.Equal(35f, updated.Affinity);
            Assert.Equal(0f, updated.Trust); // Clamped at 0
        }

        // ─── P1-058: Club Transfer Impact Tests ────────────────────────────────

        [Fact]
        public void RelationshipSystem_ClubTransfer_ImpactsFormerTeammatesAndManager()
        {
            var pId = Guid.NewGuid();
            var prevClub = Guid.NewGuid();
            var newClub = Guid.NewGuid();
            var transferDate = new DateOnly(2027, 1, 15);

            var teammate = Relationship.Create(pId, "Regular Teammate", RelationshipType.Teammate, initialAffinity: 55f, initialTrust: 50f, initialDate: TestDate);
            var manager = Relationship.Create(pId, "Former Manager", RelationshipType.Manager, initialAffinity: 60f, initialTrust: 65f, initialDate: TestDate);
            var family = Relationship.Create(pId, "Parents", RelationshipType.Parent, initialAffinity: 85f, initialTrust: 90f, initialDate: TestDate);

            var rels = new[] { teammate, manager, family };
            var updated = RelationshipSystem.ApplyClubTransfer(rels, prevClub, newClub, transferDate);

            // Teammate (< 75 affinity) drifted: -5 affinity
            var updatedTeammate = updated[0];
            Assert.Equal(50f, updatedTeammate.Affinity);
            Assert.Contains(updatedTeammate.SharedHistory, h => h.Contains("Drifted apart following transfer"));

            // Manager cooled down: -10 affinity, -5 trust
            var updatedManager = updated[1];
            Assert.Equal(50f, updatedManager.Affinity);
            Assert.Equal(60f, updatedManager.Trust);
            Assert.Contains(updatedManager.SharedHistory, h => h.Contains("Transferred away from manager's squad"));

            // Family celebrated milestone: +2 affinity, +2 trust
            var updatedFamily = updated[2];
            Assert.Equal(87f, updatedFamily.Affinity);
            Assert.Equal(92f, updatedFamily.Trust);
        }

        [Fact]
        public void RelationshipSystem_ClubTransfer_CloseTeammatesStayInTouch()
        {
            var pId = Guid.NewGuid();
            var transferDate = new DateOnly(2027, 1, 15);

            var bestFriendTeammate = Relationship.Create(pId, "Best Mate Teammate", RelationshipType.Teammate, initialAffinity: 85f, initialTrust: 80f, initialDate: TestDate);
            var updated = RelationshipSystem.ApplyClubTransfer(new[] { bestFriendTeammate }, Guid.NewGuid(), Guid.NewGuid(), transferDate);

            // Close friend (>= 75 affinity) doesn't lose affinity, gains trust (+2)
            var rel = updated[0];
            Assert.Equal(85f, rel.Affinity);
            Assert.Equal(82f, rel.Trust);
            Assert.Contains(rel.SharedHistory, h => h.Contains("Stayed in close contact"));
        }

        [Fact]
        public void RelationshipSystem_ClubTransfer_PartnerReactionsByAffinity()
        {
            var pId = Guid.NewGuid();
            var transferDate = new DateOnly(2027, 1, 15);

            var supportivePartner = Relationship.Create(pId, "Supportive Partner", RelationshipType.Partner, initialAffinity: 80f, initialTrust: 75f, initialDate: TestDate);
            var strainedPartner = Relationship.Create(pId, "Strained Partner", RelationshipType.Partner, initialAffinity: 40f, initialTrust: 35f, initialDate: TestDate);

            var updated = RelationshipSystem.ApplyClubTransfer(new[] { supportivePartner, strainedPartner }, Guid.NewGuid(), Guid.NewGuid(), transferDate);

            // Supportive partner (>= 50): +4 affinity, +5 trust
            Assert.Equal(84f, updated[0].Affinity);
            Assert.Equal(80f, updated[0].Trust);

            // Strained partner (< 50): -6 affinity, -3 trust
            Assert.Equal(34f, updated[1].Affinity);
            Assert.Equal(32f, updated[1].Trust);
        }

        // ─── P1-059: PlayerFactory Integration ────────────────────────────────

        [Fact]
        public void PlayerFactory_CreateCareer_InitializesStarterRelationships()
        {
            var clubId = Guid.NewGuid();
            var leagueId = Guid.NewGuid();
            var club = new Club(clubId, "Leeds Town", "LEE", leagueId, 50, new ClubFinances(25000m, 300000m), 2, TacticalIdentity.HighPress);

            var season = new Season(2026, TestDate, new DateOnly(2027, 5, 30), new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()), Array.Empty<Matchday>());
            var world = WorldState.CreateEmpty(season).WithClub(club);

            var args = new PlayerCreationArgs(
                name: "Liam O'Connor",
                nationality: "IRL",
                dateOfBirth: new DateOnly(2007, 2, 20),
                preferredFoot: Foot.Left,
                primaryPosition: Position.CM,
                startingClubId: clubId,
                startingAbilityBase: 55);

            var rng = new SimulationRandom(999);
            var careerWorld = PlayerFactory.CreateCareer(world, args, rng);

            var player = careerWorld.GetPlayer(args.StartingClubId != Guid.Empty ? careerWorld.Clubs[clubId].SquadPlayerIds[0] : Guid.Empty);
            var playerRels = careerWorld.GetPlayerRelationships(player.Id);

            Assert.NotEmpty(playerRels);
            Assert.Contains(playerRels, r => r.Type == RelationshipType.Parent);
            Assert.Contains(playerRels, r => r.Type == RelationshipType.Agent);
            Assert.Contains(playerRels, r => r.Type == RelationshipType.Manager);
        }
    }
}
