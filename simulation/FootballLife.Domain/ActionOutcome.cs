namespace FootballLife.Domain
{
    /// <summary>
    /// Zero-allocation readonly struct representing the resolution of a player's chosen match action.
    /// Tracks success, outcome categorization, XP generated, confidence shift, and manager trust adjustment.
    /// </summary>
    public readonly struct ActionOutcome
    {
        public bool Success { get; }
        public OutcomeType Type { get; }
        public float XpContribution { get; }
        public float ConfidenceDelta { get; }
        public float ManagerTrustDelta { get; }

        public ActionOutcome(
            bool success,
            OutcomeType type,
            float xpContribution,
            float confidenceDelta,
            float managerTrustDelta)
        {
            Success = success;
            Type = type;
            XpContribution = xpContribution;
            ConfidenceDelta = confidenceDelta;
            ManagerTrustDelta = managerTrustDelta;
        }
    }
}
