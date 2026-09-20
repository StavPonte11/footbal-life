namespace FootballLife.Domain
{
    /// <summary>
    /// Discrete resolution outcomes for player match actions.
    /// </summary>
    public enum OutcomeType
    {
        Goal = 0,
        Assist = 1,
        PassCompleted = 2,
        Turnover = 3,
        TackleWon = 4,
        TackleLost = 5,
        InterceptionWon = 6,
        SaveMade = 7,
        ChanceMissed = 8,
        Foul = 9
    }
}
