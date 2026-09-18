namespace FootballLife.Domain
{
    /// <summary>
    /// Represents football match situation contexts for attribute relevance evaluation.
    /// </summary>
    public enum SituationType
    {
        OpenPlay = 0,
        CounterAttack = 1,
        DefensiveTransition = 2,
        SetPiece = 3,
        Penalty = 4,
        OneOnOne = 5
    }
}
