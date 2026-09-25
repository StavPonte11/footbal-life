using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation.Persistence;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Stateless simulation service driving the in-game Smartphone OS (#P4-003, #P4-004):
    /// generating messaging threads, processing dialogue choices, producing FootyGram social feeds,
    /// and publishing Football Daily sports news.
    /// </summary>
    public static class PhoneSystem
    {
        /// <summary>
        /// Seeds a realistic baseline interpersonal contact circle for a young footballer's career.
        /// </summary>
        public static List<Relationship> CreateDefaultContacts(Guid playerId)
        {
            var date = new DateOnly(2026, 8, 1);
            return new List<Relationship>
            {
                new Relationship(
                    id: Guid.NewGuid(),
                    playerId: playerId,
                    name: "Maya Brooks",
                    type: RelationshipType.Partner,
                    affinity: 84f,
                    trust: 80f,
                    lastInteraction: date.AddDays(-2),
                    sharedHistory: new[] { "[2026-07-28] Supported your graduation into the first team squad." }),

                new Relationship(
                    id: Guid.NewGuid(),
                    playerId: playerId,
                    name: "Manager Henderson",
                    type: RelationshipType.Manager,
                    affinity: 62f,
                    trust: 60f,
                    lastInteraction: date.AddDays(-1),
                    sharedHistory: new[] { "[2026-07-30] Laid down tactical standards for the upcoming season." }),

                new Relationship(
                    id: Guid.NewGuid(),
                    playerId: playerId,
                    name: "Liam Vance",
                    type: RelationshipType.Agent,
                    affinity: 75f,
                    trust: 88f,
                    lastInteraction: date.AddDays(-4),
                    sharedHistory: new[] { "[2026-07-25] Negotiated professional terms and initial wage." }),

                new Relationship(
                    id: Guid.NewGuid(),
                    playerId: playerId,
                    name: "Jack Sterling",
                    type: RelationshipType.Teammate,
                    affinity: 72f,
                    trust: 70f,
                    lastInteraction: date.AddDays(-1),
                    sharedHistory: new[] { "[2026-07-31] Team bonding session after pre-season training." }),

                new Relationship(
                    id: Guid.NewGuid(),
                    playerId: playerId,
                    name: "Sarah (Mom)",
                    type: RelationshipType.Parent,
                    affinity: 92f,
                    trust: 95f,
                    lastInteraction: date.AddDays(-3),
                    sharedHistory: new[] { "[2026-07-20] Sent good luck gift for the new campaign." })
            };
        }

        /// <summary>
        /// Generates contextual incoming messages based on career situation and performance.
        /// </summary>
        public static List<PhoneMessage> GenerateMessages(CareerSaveData save, IReadOnlyList<Relationship> contacts)
        {
            if (save is null) throw new ArgumentNullException(nameof(save));
            var messages = new List<PhoneMessage>();

            // 1. Manager message
            var manager = FindContactByType(contacts, RelationshipType.Manager);
            if (manager != null)
            {
                messages.Add(new PhoneMessage(
                    id: Guid.NewGuid(),
                    senderId: manager.Id,
                    senderName: manager.Name,
                    senderType: RelationshipType.Manager,
                    avatarCode: "manager",
                    messageText: "Big match coming up this weekend. Make sure you get quality rest, hydrate, and review the opponent's defensive tape.",
                    timestamp: "09:14 AM",
                    isRead: false,
                    choices: new List<DialogueChoice>
                    {
                        new DialogueChoice("I'll be in peak shape, Boss! Ready to make an impact.", affinityDelta: 2f, trustDelta: 4f, energyCost: 0, "Sounds like the right attitude. See you on the training pitch tomorrow."),
                        new DialogueChoice("Focusing on recovery tonight. Will be 100% prepared.", affinityDelta: 1f, trustDelta: 2f, energyCost: 0, "Good discipline. Don't slack off on nutrition.")
                    }));
            }

            // 2. Partner message
            var partner = FindContactByType(contacts, RelationshipType.Partner);
            if (partner != null)
            {
                messages.Add(new PhoneMessage(
                    id: Guid.NewGuid(),
                    senderId: partner.Id,
                    senderName: partner.Name,
                    senderType: RelationshipType.Partner,
                    avatarCode: "partner",
                    messageText: "Hey love! So proud of how hard you trained today. Are we still getting dinner on Tuesday evening? ❤️",
                    timestamp: "12:30 PM",
                    isRead: false,
                    choices: new List<DialogueChoice>
                    {
                        new DialogueChoice("Definitely! Tuesday night is on me, can't wait.", affinityDelta: 6f, trustDelta: 3f, energyCost: 5, "Yay! Booking that Italian place you love. See you soon! 😘"),
                        new DialogueChoice("Training might run late, but I'll make time for you.", affinityDelta: 3f, trustDelta: 2f, energyCost: 0, "I understand your schedule is crazy. Let's do dessert at home then. ☕")
                    }));
            }

            // 3. Agent message
            var agent = FindContactByType(contacts, RelationshipType.Agent);
            if (agent != null)
            {
                messages.Add(new PhoneMessage(
                    id: Guid.NewGuid(),
                    senderId: agent.Id,
                    senderName: agent.Name,
                    senderType: RelationshipType.Agent,
                    avatarCode: "agent",
                    messageText: "Sponsors are keeping a close watch on your form. Keep your average rating high and we'll secure the boot endorsement deal next month.",
                    timestamp: "02:45 PM",
                    isRead: false,
                    choices: new List<DialogueChoice>
                    {
                        new DialogueChoice("Let's secure it. I'll let my football do the talking.", affinityDelta: 3f, trustDelta: 5f, energyCost: 0, "That's what I love to hear. I'll prep the contract terms."),
                        new DialogueChoice("One match at a time for now. Team results come first.", affinityDelta: 1f, trustDelta: 4f, energyCost: 0, "Professional approach. Good head on your shoulders.")
                    }));
            }

            // 4. Teammate message
            var teammate = FindContactByType(contacts, RelationshipType.Teammate);
            if (teammate != null)
            {
                messages.Add(new PhoneMessage(
                    id: Guid.NewGuid(),
                    senderId: teammate.Id,
                    senderName: teammate.Name,
                    senderType: RelationshipType.Teammate,
                    avatarCode: "teammate",
                    messageText: "That finishing session was unreal today lad! 🔥 Want to do extra free-kick practice after tomorrow's drills?",
                    timestamp: "05:12 PM",
                    isRead: true,
                    choices: new List<DialogueChoice>
                    {
                        new DialogueChoice("Count me in! Top bins only 🎯", affinityDelta: 5f, trustDelta: 3f, energyCost: 6, "Let's go! Keeper won't know what hit him."),
                        new DialogueChoice("Gotta save my legs for matchday mate, but good session!", affinityDelta: 2f, trustDelta: 3f, energyCost: 0, "Fair play, smart thinking. Let's get the 3 points Saturday.")
                    }));
            }

            return messages;
        }

        /// <summary>
        /// Processes a player's chosen dialogue reply in a chat thread.
        /// Deterministically applies affinity/trust adjustments and save state deltas.
        /// </summary>
        public static string ProcessMessageReply(
            PhoneMessage message,
            int choiceIndex,
            CareerSaveData save,
            Relationship? senderRelationship,
            out Relationship? updatedRelationship)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));
            if (save is null) throw new ArgumentNullException(nameof(save));

            updatedRelationship = senderRelationship;

            if (choiceIndex < 0 || choiceIndex >= message.Choices.Count)
            {
                return "Invalid choice selected.";
            }

            var choice = message.Choices[choiceIndex];
            message.SelectedChoiceIndex = choiceIndex;
            message.IsRead = true;

            // Apply energy cost
            save.Energy = Math.Max(0, save.Energy - choice.EnergyCost);
            save.Morale = Math.Min(100, save.Morale + 2);

            // Update relationship if available
            if (senderRelationship != null)
            {
                var date = new DateOnly(2026, 8, 1).AddDays(save.CurrentWeek * 7);
                updatedRelationship = senderRelationship.WithInteraction(
                    date,
                    choice.AffinityDelta,
                    choice.TrustDelta,
                    $"Chat reply: \"{choice.ChoiceText}\"");

                if (senderRelationship.Type == RelationshipType.Manager)
                {
                    save.ManagerTrust = Math.Min(100, save.ManagerTrust + (int)Math.Round(choice.TrustDelta * 0.8f));
                }
            }

            return choice.ReplyText;
        }

        /// <summary>
        /// Generates emergent social feed posts for the FootyGram application.
        /// Reflects the player's goals, club performance, and fan sentiment.
        /// </summary>
        public static List<SocialPost> GenerateSocialPosts(CareerSaveData save)
        {
            string playerTag = save != null ? save.PlayerName.Replace(" ", "") : "MarcusVance";
            string clubName = save != null ? save.ClubName : "Northfield Town";
            int goals = save?.TotalGoals ?? 12;

            return new List<SocialPost>
            {
                new SocialPost(
                    id: Guid.NewGuid(),
                    authorName: "Northfield Fan TV",
                    handle: "@NorthfieldFanTV",
                    avatarIcon: "fan",
                    content: $"Can we talk about #{playerTag}? The movement off the ball in training today was world class. Starting eleven this weekend is non-negotiable! 🔥⚽",
                    likesCount: 1420,
                    commentsCount: 88,
                    timeAgo: "22m ago",
                    tag: "#MatchdayReady"),

                new SocialPost(
                    id: Guid.NewGuid(),
                    authorName: "Jack Sterling",
                    handle: "@jack_sterling8",
                    avatarIcon: "teammate",
                    content: $"Lethal combo in training drills today with @{playerTag.ToLower()}. Chemistry building week by week. We go again! 👊⚡",
                    likesCount: 3840,
                    commentsCount: 142,
                    timeAgo: "1h ago",
                    tag: "#TeamSpirit"),

                new SocialPost(
                    id: Guid.NewGuid(),
                    authorName: "EFL Scouting Hub",
                    handle: "@EFLScouts",
                    avatarIcon: "scout",
                    content: $"📊 PROSPECT WATCH: At just 18 years old, #{playerTag} has already netted {goals} goals this campaign. Pace, positioning, and elite finishing temperament.",
                    likesCount: 5210,
                    commentsCount: 230,
                    timeAgo: "3h ago",
                    tag: "#RisingStar"),

                new SocialPost(
                    id: Guid.NewGuid(),
                    authorName: $"{clubName} Official",
                    handle: $"@{clubName.Replace(" ", "")}FC",
                    avatarIcon: "club",
                    content: $"Locked in for the upcoming fixture. Tactical preparations complete at the training ground. 🏟️🔴⚪",
                    likesCount: 8900,
                    commentsCount: 310,
                    timeAgo: "5h ago",
                    tag: "#MatchdayFocus")
            };
        }

        /// <summary>
        /// Generates sports journalism headlines and news articles for Football Daily.
        /// </summary>
        public static List<SocialPost> GenerateNewsArticles(CareerSaveData save)
        {
            string playerName = save?.PlayerName ?? "Marcus Vance";
            string clubName = save?.ClubName ?? "Northfield Town";

            return new List<SocialPost>
            {
                new SocialPost(
                    id: Guid.NewGuid(),
                    authorName: "The Football Athletic",
                    handle: "TheAthletic.com",
                    avatarIcon: "news",
                    content: $"SCOUTING REPORT: How {playerName} became {clubName}'s most dangerous attacking outlet. An analytical breakdown of off-the-ball runs and finishing composure.",
                    likesCount: 680,
                    commentsCount: 45,
                    timeAgo: "1h ago",
                    tag: "TACTICS",
                    isNewsArticle: true),

                new SocialPost(
                    id: Guid.NewGuid(),
                    authorName: "Sky Sports News",
                    handle: "SkySports.com",
                    avatarIcon: "news",
                    content: $"TRANSFER GOSSIP: Several Championship clubs are reportedly preparing multi-million pound inquiries for {clubName} prodigy {playerName}.",
                    likesCount: 1240,
                    commentsCount: 92,
                    timeAgo: "3h ago",
                    tag: "TRANSFERS",
                    isNewsArticle: true),

                new SocialPost(
                    id: Guid.NewGuid(),
                    authorName: "Daily Football Mail",
                    handle: "DailyMail.co.uk",
                    avatarIcon: "news",
                    content: $"MANAGER'S PRAISE: Boss Henderson commends squad professionalism in pre-match press conference: 'Our young players are setting the standard in training every single day.'",
                    likesCount: 420,
                    commentsCount: 28,
                    timeAgo: "6h ago",
                    tag: "PRESS",
                    isNewsArticle: true)
            };
        }

        private static Relationship? FindContactByType(IReadOnlyList<Relationship> contacts, RelationshipType type)
        {
            for (int i = 0; i < contacts.Count; i++)
            {
                if (contacts[i].Type == type) return contacts[i];
            }
            return null;
        }
    }
}
