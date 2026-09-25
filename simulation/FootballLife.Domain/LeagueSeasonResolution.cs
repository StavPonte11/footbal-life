using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Outcome for a single club at the end of a league season (#P5-003).
    /// </summary>
    public sealed record ClubSeasonOutcome
    {
        public Guid ClubId { get; init; }
        public string ClubName { get; init; } = string.Empty;
        public Guid LeagueId { get; init; }
        public int FinalPosition { get; init; }
        public int Points { get; init; }
        public int GoalDifference { get; init; }
        public bool WonTitle { get; init; }
        public bool Promoted { get; init; }
        public bool Relegated { get; init; }
        public bool QualifiedForContinental { get; init; }
    }

    /// <summary>
    /// Summary of an entire league's season resolution including promotions and relegations (#P5-003).
    /// </summary>
    public sealed record LeagueSeasonResolution
    {
        public int SeasonYear { get; init; }
        public Guid LeagueId { get; init; }
        public string LeagueName { get; init; } = string.Empty;
        public int Tier { get; init; }
        public Guid ChampionClubId { get; init; }
        public string ChampionClubName { get; init; } = string.Empty;
        public IReadOnlyList<Guid> PromotedClubIds { get; init; } = Array.Empty<Guid>();
        public IReadOnlyList<Guid> RelegatedClubIds { get; init; } = Array.Empty<Guid>();
        public IReadOnlyList<ClubSeasonOutcome> Standings { get; init; } = Array.Empty<ClubSeasonOutcome>();
    }

    /// <summary>
    /// Complete universe season resolution across all tiers (#P5-001, #P5-003).
    /// </summary>
    public sealed record WorldSeasonResolution
    {
        public int SeasonYear { get; init; }
        public IReadOnlyList<LeagueSeasonResolution> LeagueResolutions { get; init; } = Array.Empty<LeagueSeasonResolution>();
        public IReadOnlyList<Guid> PromotedClubIds { get; init; } = Array.Empty<Guid>();
        public IReadOnlyList<Guid> RelegatedClubIds { get; init; } = Array.Empty<Guid>();
        public int TotalPlayerTransfers { get; init; }
        public int TotalPlayerRetirements { get; init; }
    }
}
