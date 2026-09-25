using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Transfer status of a player in the market (#P5-002).
    /// </summary>
    public enum PlayerTransferStatus
    {
        NotForSale,
        TransferListedByClub,
        TransferRequestedByPlayer,
        LoanListed
    }

    /// <summary>
    /// Formal transfer listing record for a player in the market (#P5-002).
    /// </summary>
    public sealed record TransferListing
    {
        public Guid PlayerId { get; init; }
        public Guid ClubId { get; init; }
        public PlayerTransferStatus Status { get; init; }
        public decimal AskingPrice { get; init; }
        public DateOnly ListedDate { get; init; }
        public string Reason { get; init; } = string.Empty;
        public bool IsPlayerInitiated => Status == PlayerTransferStatus.TransferRequestedByPlayer;
    }
}
