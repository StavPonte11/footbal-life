using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Reason prompting or driving player retirement.
    /// </summary>
    public enum RetirementReason
    {
        AgeAndDecline = 1,
        ContractExpired = 2,
        MajorInjury = 3,
        VoluntaryAtPeak = 4,
        TrophyCabinetComplete = 5
    }

    /// <summary>
    /// Post-playing career role chosen by the retired footballer.
    /// </summary>
    public enum PostPlayingRole
    {
        Manager = 1,
        AcademyCoach = 2,
        TVPundit = 3,
        ClubAmbassador = 4,
        PrivateLife = 5
    }

    /// <summary>
    /// Pure C# domain model capturing the formal retirement of a footballer.
    /// </summary>
    public sealed record RetirementDecision
    {
        public Guid PlayerId { get; init; }
        public bool IsRetired { get; init; }
        public int RetirementAge { get; init; }
        public int RetirementSeason { get; init; }
        public RetirementReason Reason { get; init; }
        public PostPlayingRole ChosenRole { get; init; }
        public string Statement { get; init; }

        public RetirementDecision(
            Guid playerId,
            bool isRetired,
            int retirementAge,
            int retirementSeason,
            RetirementReason reason,
            PostPlayingRole chosenRole,
            string? statement = null)
        {
            if (playerId == Guid.Empty) throw new ArgumentException("PlayerId cannot be empty.", nameof(playerId));
            if (retirementAge <= 0) throw new ArgumentOutOfRangeException(nameof(retirementAge));
            if (retirementSeason <= 0) throw new ArgumentOutOfRangeException(nameof(retirementSeason));

            PlayerId = playerId;
            IsRetired = isRetired;
            RetirementAge = retirementAge;
            RetirementSeason = retirementSeason;
            Reason = reason;
            ChosenRole = chosenRole;
            Statement = statement ?? $"Retired at age {retirementAge} in Season {retirementSeason} due to {reason}, embarking on a new journey as {chosenRole}.";
        }
    }
}
