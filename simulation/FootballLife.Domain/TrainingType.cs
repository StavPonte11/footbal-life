namespace FootballLife.Domain
{
    /// <summary>
    /// Classifies the type of training activity performed in a session.
    /// Determines which attributes receive XP and what the fatigue cost profile is.
    /// </summary>
    public enum TrainingType
    {
        /// <summary>Ball work, passing drills, shooting practice, crossing, first touch. BaseXP = 3.0 per attribute.</summary>
        Technical,

        /// <summary>Running, sprinting, stamina and endurance work, gym-free physical conditioning. BaseXP = 3.0 per attribute.</summary>
        Physical,

        /// <summary>Tactics board, video analysis, decision-making drills. Lower XP gain but lower fatigue. BaseXP = 2.0 per attribute.</summary>
        Mental,

        /// <summary>Position-specific scenarios — targets the 3 highest-weighted attributes for the player's position. BaseXP = 4.0.</summary>
        PositionSpecific,

        /// <summary>Active recovery: stretching, pool work, light jogging. XP = 0, FatigueCost is negative (restores fatigue).</summary>
        Recovery,

        /// <summary>Weight room strength and power training. BaseXP = 2.5 per attribute.</summary>
        Gym,
    }
}
