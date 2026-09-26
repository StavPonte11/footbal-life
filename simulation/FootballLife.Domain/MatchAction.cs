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
        CutInside = 15,

        // Phase 8.1 — Match Engine v2: New Actions
        FreeKick_Direct = 16,
        FreeKick_Cross = 17,
        PenaltyKick = 18,
        SkillMove = 19,
        ChipShot = 20,
        Volley = 21,
        DivingHeader = 22,
        SlidingTackle = 23,
        BlockShot = 24,
        CornerDelivery = 25,
        GoalkeeperRush = 26
    }
}
