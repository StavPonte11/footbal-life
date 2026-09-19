namespace FootballLife.Domain
{
    /// <summary>
    /// Actions available to a player during a match situation.
    /// </summary>
    public enum MatchAction
    {
        ShortPass = 0,
        LongPass = 1,
        Cross = 2,
        Dribble = 3,
        ThroughBall = 4,
        Shot_Close = 5,
        Shot_Long = 6,
        Header = 7,
        Tackle = 8,
        Interception = 9,
        AerialChallenge = 10,
        GoalkeeperSave = 11,
        ClaimCross = 12,
        DistributionPass = 13,
        Press = 14,
        CutInside = 15
    }
}
