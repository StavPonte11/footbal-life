using FootballLife.Domain;

namespace FootballLife.Unity.Core.Audio
{

    /// <summary>
    /// Identifiers for all distinct in-game sound effects, ambient cues, and UI audio.
    /// </summary>
    public enum SoundId
    {
        KickSoft,
        KickHard,
        WoodworkHit,
        NetRipple,
        WhistleStart,
        WhistleGoal,
        WhistleEnd,
        CrowdMurmur,
        CrowdRoar,
        CrowdGasp,
        UIButtonClick,
        UITabSwitch,
        UIWageChime,
        UIAlertPrompt,
        MenuMusic
    }

    /// <summary>
    /// Types of referee whistles during a football match situation.
    /// </summary>
    public enum WhistleType
    {
        SituationStart,
        GoalScored,
        FullTime
    }
}
