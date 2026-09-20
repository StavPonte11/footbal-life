using System;
using System.Collections.Generic;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class RelationshipDomainTests
    {
        private static readonly DateOnly TestDate = new DateOnly(2026, 8, 1);

        [Fact]
        public void Relationship_ConstructsAndClampsWithinBounds()
        {
            var playerId = Guid.NewGuid();
            var rel = new Relationship(
                id: Guid.NewGuid(),
                playerId: playerId,
                name: "Sarah (Partner)",
                type: RelationshipType.Partner,
                affinity: 120f, // Above 100 -> clamped to 100
                trust: -10f,   // Below 0 -> clamped to 0
                lastInteraction: TestDate);

            Assert.Equal("Sarah (Partner)", rel.Name);
            Assert.Equal(RelationshipType.Partner, rel.Type);
            Assert.Equal(100f, rel.Affinity);
            Assert.Equal(0f, rel.Trust);
            Assert.False(rel.IsFamily);
        }

        [Fact]
        public void Relationship_IsFamily_TrueOnlyForParentAndSibling()
        {
            var pId = Guid.NewGuid();
            var parent = Relationship.Create(pId, "Mom", RelationshipType.Parent);
            var sibling = Relationship.Create(pId, "Brother", RelationshipType.Sibling);
            var friend = Relationship.Create(pId, "Mate", RelationshipType.Friend);
            var teammate = Relationship.Create(pId, "Teammate", RelationshipType.Teammate);
            var agent = Relationship.Create(pId, "Agent", RelationshipType.Agent);

            Assert.True(parent.IsFamily);
            Assert.True(sibling.IsFamily);
            Assert.False(friend.IsFamily);
            Assert.False(teammate.IsFamily);
            Assert.False(agent.IsFamily);
        }

        [Fact]
        public void Relationship_WithInteraction_UpdatesStateAndHistory()
        {
            var pId = Guid.NewGuid();
            var rel = Relationship.Create(pId, "Dave", RelationshipType.Friend, 50f, 50f, TestDate);

            var nextDate = TestDate.AddDays(7);
            var updated = rel.WithInteraction(nextDate, 15f, 10f, "Dinner after Saturday match");

            Assert.Equal(65f, updated.Affinity);
            Assert.Equal(60f, updated.Trust);
            Assert.Equal(nextDate, updated.LastInteraction);
            Assert.Contains(updated.SharedHistory, h => h.Contains("Dinner after Saturday match"));
        }

        [Fact]
        public void Relationship_WithDecay_RespectsFamilyFloor()
        {
            var pId = Guid.NewGuid();
            var parent = Relationship.Create(pId, "Mom", RelationshipType.Parent, 25f, 50f, TestDate);
            var friend = Relationship.Create(pId, "Mate", RelationshipType.Friend, 25f, 50f, TestDate);

            // Large decay of 15 points
            var decayedParent = parent.WithDecay(15f);
            var decayedFriend = friend.WithDecay(15f);

            // Parent should hit the floor of 20f
            Assert.Equal(20f, decayedParent.Affinity);
            // Friend can decay freely below 20f (25 - 15 = 10f)
            Assert.Equal(10f, decayedFriend.Affinity);
        }

        [Fact]
        public void WorldState_RegistersAndRetrievesRelationships()
        {
            var playerId = Guid.NewGuid();
            var rel1 = Relationship.Create(playerId, "Dad", RelationshipType.Parent);
            var rel2 = Relationship.Create(playerId, "Agent", RelationshipType.Agent);

            var season = new Season(
                2026,
                TestDate,
                new DateOnly(2027, 5, 30),
                new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()),
                Array.Empty<Matchday>());

            var world = WorldState.CreateEmpty(season)
                .WithRelationship(rel1)
                .WithRelationship(rel2);

            Assert.Equal(2, world.Relationships.Count);
            Assert.Equal("Dad", world.GetRelationship(rel1.Id).Name);

            var playerRels = world.GetPlayerRelationships(playerId);
            Assert.Equal(2, playerRels.Count);
        }
    }
}
