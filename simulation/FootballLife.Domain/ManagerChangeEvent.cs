using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Reasons why a manager departs or changes at a club.
    /// </summary>
    public enum ManagerChangeReason
    {
        Sacked = 0,
        Resigned = 1,
        ContractExpired = 2,
        Promoted = 3,
        Retired = 4
    }

    /// <summary>
    /// Represents a recorded manager change event at a club.
    /// </summary>
    public sealed record ManagerChangeEvent
    {
        public Guid Id { get; init; }
        public Guid ClubId { get; init; }
        public Guid? OldManagerId { get; init; }
        public Guid NewManagerId { get; init; }
        public ManagerChangeReason Reason { get; init; }
        public DateOnly Date { get; init; }
        public float PlayerTrustReset { get; init; }

        public ManagerChangeEvent(
            Guid id,
            Guid clubId,
            Guid? oldManagerId,
            Guid newManagerId,
            ManagerChangeReason reason,
            DateOnly date,
            float playerTrustReset = 50.0f)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Manager change event ID cannot be empty.", nameof(id));
            if (clubId == Guid.Empty)
                throw new ArgumentException("Club ID cannot be empty.", nameof(clubId));
            if (newManagerId == Guid.Empty)
                throw new ArgumentException("New manager ID cannot be empty.", nameof(newManagerId));

            Id = id;
            ClubId = clubId;
            OldManagerId = oldManagerId;
            NewManagerId = newManagerId;
            Reason = reason;
            Date = date;
            PlayerTrustReset = Math.Clamp(playerTrustReset, 0.0f, 100.0f);
        }

        public static ManagerChangeEvent Create(
            Guid clubId,
            Guid? oldManagerId,
            Guid newManagerId,
            ManagerChangeReason reason,
            DateOnly date,
            float playerTrustReset = 50.0f)
        {
            return new ManagerChangeEvent(
                Guid.NewGuid(), clubId, oldManagerId, newManagerId, reason, date, playerTrustReset);
        }
    }
}
