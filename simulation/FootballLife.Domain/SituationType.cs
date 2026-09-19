namespace FootballLife.Domain
{
    /// <summary>
    /// Represents football match situation contexts for attribute relevance evaluation
    /// and position-specific situation generation.
    /// </summary>
    public enum SituationType
    {
        OpenPlay = 0,
        CounterAttack = 1,
        DefensiveTransition = 2,
        SetPiece = 3,
        Penalty = 4,
        OneOnOne = 5,

        // Position-specific match scenarios (P1-029)
        RunningInBehind = 6,
        ReceivingInBox = 7,
        LongShot = 8,
        Pressing = 9,
        AerialChallenge = 10,
        ReceiveBall1v1 = 11,
        Cross = 12,
        CutInside = 13,
        ReceiveUnderPressure = 14,
        ThroughBall = 15,
        LongPass = 16,
        Interception = 17,
        Tackle = 18,
        DistributionPass = 19,
        PositioningRun = 20,
        BuildUpPass = 21,
        Save = 22,
        ClaimCross = 23,
        Distribution = 24
    }
}

