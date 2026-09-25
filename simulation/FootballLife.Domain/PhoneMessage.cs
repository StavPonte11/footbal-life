using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Interactive dialogue choice that a player can select when replying to an incoming message.
    /// </summary>
    public sealed record DialogueChoice
    {
        public string ChoiceText { get; init; }
        public float AffinityDelta { get; init; }
        public float TrustDelta { get; init; }
        public int EnergyCost { get; init; }
        public string ReplyText { get; init; }

        public DialogueChoice(
            string choiceText,
            float affinityDelta,
            float trustDelta,
            int energyCost,
            string replyText)
        {
            ChoiceText = choiceText;
            AffinityDelta = affinityDelta;
            TrustDelta = trustDelta;
            EnergyCost = energyCost;
            ReplyText = replyText;
        }
    }

    /// <summary>
    /// Represents an instant messaging thread or conversation bubble in the in-game WhatsApp application (#P4-003).
    /// </summary>
    public sealed record PhoneMessage
    {
        public Guid Id { get; init; }
        public Guid SenderId { get; init; }
        public string SenderName { get; init; }
        public RelationshipType SenderType { get; init; }
        public string AvatarCode { get; init; }
        public string MessageText { get; init; }
        public string Timestamp { get; init; }
        public bool IsRead { get; set; }
        public IReadOnlyList<DialogueChoice> Choices { get; init; }
        public int? SelectedChoiceIndex { get; set; }

        public PhoneMessage(
            Guid id,
            Guid senderId,
            string senderName,
            RelationshipType senderType,
            string avatarCode,
            string messageText,
            string timestamp,
            bool isRead = false,
            IReadOnlyList<DialogueChoice>? choices = null,
            int? selectedChoiceIndex = null)
        {
            Id = id;
            SenderId = senderId;
            SenderName = senderName;
            SenderType = senderType;
            AvatarCode = avatarCode;
            MessageText = messageText;
            Timestamp = timestamp;
            IsRead = isRead;
            Choices = choices ?? Array.Empty<DialogueChoice>();
            SelectedChoiceIndex = selectedChoiceIndex;
        }
    }
}
