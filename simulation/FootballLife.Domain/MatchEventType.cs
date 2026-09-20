namespace FootballLife.Domain
{
    /// <summary>
    /// Event types that can occur during a football match.
    /// </summary>
    public enum MatchEventType
    {
        Goal = 0,
        Assist = 1,
        YellowCard = 2,
        RedCard = 3,
        Substitution = 4,
        Miss = 5,
        Save = 6,
        TackleWon = 7,
        Foul = 8,
        PenaltyScored = 9,
        PenaltyMissed = 10
    }
}
