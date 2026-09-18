namespace FootballLife.Domain
{
    /// <summary>
    /// Identifies the 15 core football skills and attributes of a player.
    /// </summary>
    public enum AttributeName
    {
        // Physical (5)
        Pace = 0,
        Acceleration = 1,
        Stamina = 2,
        Strength = 3,
        Agility = 4,

        // Technical (6)
        Passing = 5,
        Shooting = 6,
        Dribbling = 7,
        Crossing = 8,
        FirstTouch = 9,
        Tackling = 10,

        // Mental (4)
        Vision = 11,
        Composure = 12,
        Positioning = 13,
        DecisionMaking = 14
    }
}
