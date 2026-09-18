namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the player's hierarchy and expected role within the squad.
    /// </summary>
    public enum SquadStatus
    {
        Academy = 0,
        Reserve = 1,
        Bench = 2,
        Rotation = 3,
        Starter = 4,
        KeyPlayer = 5
    }
}
