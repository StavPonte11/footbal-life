using System;
using System.Linq;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class SocialSystemTests
    {
        private CareerSaveData CreateSampleSave(int balance = 5000, int energy = 80, int morale = 70)
        {
            return new CareerSaveData
            {
                PlayerId = Guid.NewGuid(),
                PlayerName = "Marcus Vance",
                ClubName = "Northfield Town",
                CurrentSeason = 1,
                CurrentWeek = 3,
                BankBalance = balance,
                Energy = energy,
                Morale = morale,
                ManagerTrust = 50
            };
        }

        private Relationship CreateSampleRelationship(RelationshipType type, float affinity = 60f, float trust = 60f)
        {
            return new Relationship(
                id: Guid.NewGuid(),
                playerId: Guid.NewGuid(),
                name: "Test Contact",
                type: type,
                affinity: affinity,
                trust: trust,
                lastInteraction: new DateOnly(2026, 8, 1));
        }

        [Fact]
        public void CallCatchUp_Succeeds_WhenEnergyIsSufficient()
        {
            var save = CreateSampleSave(energy: 50, morale: 65);
            var rel = CreateSampleRelationship(RelationshipType.Partner, affinity: 70f, trust: 70f);

            var result = RelationshipSystem.ExecuteSocialAction(rel, SocialActionType.CallCatchUp, save);

            Assert.True(result.Success);
            Assert.Equal(45, save.Energy);
            Assert.Equal(68, save.Morale);
            Assert.True(result.UpdatedRelationship.Affinity > 70f);
            Assert.True(result.UpdatedRelationship.Trust > 70f);
            Assert.Contains("Caught up on a warm phone call.", result.UpdatedRelationship.SharedHistory.Last());
        }

        [Fact]
        public void CallCatchUp_Fails_WhenEnergyIsDepleted()
        {
            var save = CreateSampleSave(energy: 3);
            var rel = CreateSampleRelationship(RelationshipType.Partner);

            var result = RelationshipSystem.ExecuteSocialAction(rel, SocialActionType.CallCatchUp, save);

            Assert.False(result.Success);
            Assert.Contains("exhausted", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(3, save.Energy);
        }

        [Fact]
        public void SendGift_DeductsFunds_AndBoostsAffinity()
        {
            var save = CreateSampleSave(balance: 1000);
            var rel = CreateSampleRelationship(RelationshipType.Partner, affinity: 75f);

            var result = RelationshipSystem.ExecuteSocialAction(rel, SocialActionType.SendGift, save, customGiftAmount: 200m);

            Assert.True(result.Success);
            Assert.Equal(800, save.BankBalance);
            Assert.Equal(200, result.MoneyCost);
            Assert.Equal(0, result.EnergyCost);
            Assert.True(result.UpdatedRelationship.Affinity > 75f);
        }

        [Fact]
        public void SendGift_Fails_WhenBankBalanceInsufficient()
        {
            var save = CreateSampleSave(balance: 50);
            var rel = CreateSampleRelationship(RelationshipType.Partner);

            var result = RelationshipSystem.ExecuteSocialAction(rel, SocialActionType.SendGift, save, customGiftAmount: 350m);

            Assert.False(result.Success);
            Assert.Contains("Insufficient funds", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(50, save.BankBalance);
        }

        [Fact]
        public void DinnerHangOut_DeductsBothEnergyAndMoney_BoostsMoraleSignificantly()
        {
            var save = CreateSampleSave(balance: 2000, energy: 60, morale: 70);
            var rel = CreateSampleRelationship(RelationshipType.Friend, affinity: 65f);

            var result = RelationshipSystem.ExecuteSocialAction(rel, SocialActionType.DinnerHangOut, save);

            Assert.True(result.Success);
            Assert.Equal(45, save.Energy);
            Assert.Equal(1750, save.BankBalance);
            Assert.Equal(78, save.Morale);
            Assert.True(result.UpdatedRelationship.Affinity >= 77f);
        }

        [Fact]
        public void TalkTactics_Fails_ForNonManagerOrTeammate()
        {
            var save = CreateSampleSave(energy: 70);
            var rel = CreateSampleRelationship(RelationshipType.Partner);

            var result = RelationshipSystem.ExecuteSocialAction(rel, SocialActionType.TalkTactics, save);

            Assert.False(result.Success);
            Assert.Contains("manager or teammates", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void TalkTactics_Succeeds_ForManager_AndBoostsManagerTrust()
        {
            var save = CreateSampleSave(energy: 70);
            save.ManagerTrust = 55;
            var rel = CreateSampleRelationship(RelationshipType.Manager, affinity: 60f, trust: 60f);

            var result = RelationshipSystem.ExecuteSocialAction(rel, SocialActionType.TalkTactics, save);

            Assert.True(result.Success);
            Assert.Equal(62, save.Energy);
            Assert.True(save.ManagerTrust > 55);
            Assert.True(result.UpdatedRelationship.Trust >= 70f);
        }

        [Fact]
        public void PhoneSystem_CreateDefaultContacts_ReturnsDiverseCircle()
        {
            var contacts = PhoneSystem.CreateDefaultContacts(Guid.NewGuid());

            Assert.NotNull(contacts);
            Assert.True(contacts.Count >= 4);
            Assert.Contains(contacts, c => c.Type == RelationshipType.Partner);
            Assert.Contains(contacts, c => c.Type == RelationshipType.Manager);
            Assert.Contains(contacts, c => c.Type == RelationshipType.Agent);
            Assert.Contains(contacts, c => c.Type == RelationshipType.Teammate);
        }

        [Fact]
        public void PhoneSystem_GenerateMessages_PopulatesRealisticThreadsWithDialogueChoices()
        {
            var save = CreateSampleSave();
            var contacts = PhoneSystem.CreateDefaultContacts(save.PlayerId);

            var messages = PhoneSystem.GenerateMessages(save, contacts);

            Assert.NotEmpty(messages);
            foreach (var msg in messages)
            {
                Assert.False(string.IsNullOrWhiteSpace(msg.SenderName));
                Assert.False(string.IsNullOrWhiteSpace(msg.MessageText));
                Assert.NotEmpty(msg.Choices);
            }
        }

        [Fact]
        public void PhoneSystem_ProcessMessageReply_UpdatesSaveAndRelationship()
        {
            var save = CreateSampleSave(energy: 80);
            var contacts = PhoneSystem.CreateDefaultContacts(save.PlayerId);
            var messages = PhoneSystem.GenerateMessages(save, contacts);

            var firstMsg = messages.First();
            var rel = contacts.First(c => c.Id == firstMsg.SenderId);
            float initialAffinity = rel.Affinity;

            string replyText = PhoneSystem.ProcessMessageReply(firstMsg, 0, save, rel, out var updatedRel);

            Assert.False(string.IsNullOrWhiteSpace(replyText));
            Assert.True(firstMsg.IsRead);
            Assert.Equal(0, firstMsg.SelectedChoiceIndex);
            Assert.NotNull(updatedRel);
            Assert.True(updatedRel!.Affinity >= initialAffinity);
        }

        [Fact]
        public void PhoneSystem_GenerateSocialPosts_And_NewsArticles_ReturnsPopulatedFeeds()
        {
            var save = CreateSampleSave();

            var posts = PhoneSystem.GenerateSocialPosts(save);
            var news = PhoneSystem.GenerateNewsArticles(save);

            Assert.NotEmpty(posts);
            Assert.True(posts.Count >= 3);
            Assert.Contains(posts, p => p.LikesCount > 0);

            Assert.NotEmpty(news);
            Assert.Contains(news, n => n.IsNewsArticle);

            // Test toggling like on FootyGram post
            var firstPost = posts[0];
            int initialLikes = firstPost.LikesCount;
            firstPost.ToggleLike();
            Assert.True(firstPost.IsLikedByPlayer);
            Assert.Equal(initialLikes + 1, firstPost.LikesCount);

            firstPost.ToggleLike();
            Assert.False(firstPost.IsLikedByPlayer);
            Assert.Equal(initialLikes, firstPost.LikesCount);
        }
    }
}
