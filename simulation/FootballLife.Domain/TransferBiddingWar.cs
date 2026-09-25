using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Individual club bid within a multi-club transfer bidding war (#P5-002).
    /// </summary>
    public sealed record ClubBid
    {
        public string BidId { get; init; } = Guid.NewGuid().ToString("N")[..8];
        public Guid BiddingClubId { get; init; }
        public string BiddingClubName { get; init; } = string.Empty;
        public int LeagueTier { get; init; }
        public string LeagueName { get; init; } = string.Empty;
        public decimal TransferFee { get; init; }
        public decimal OfferedWeeklyWage { get; init; }
        public decimal SigningBonus { get; init; }
        public SquadRole PromisedSquadRole { get; init; }
        public int ContractLengthYears { get; init; } = 3;
        public int ClubPrestige { get; init; }
        public DateOnly BidDate { get; init; }
    }

    /// <summary>
    /// Multi-club bidding war for a player during an active transfer window (#P5-002).
    /// </summary>
    public sealed record TransferBiddingWar
    {
        public Guid PlayerId { get; init; }
        public string PlayerName { get; init; } = string.Empty;
        public Guid CurrentClubId { get; init; }
        public string CurrentClubName { get; init; } = string.Empty;
        public decimal EstimatedMarketValue { get; init; }
        public int WindowWeek { get; init; }
        public IReadOnlyList<ClubBid> Bids { get; init; } = Array.Empty<ClubBid>();
        public bool HasActiveBids => Bids.Count > 0;
    }
}
