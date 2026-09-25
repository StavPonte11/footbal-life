namespace FootballLife.Domain
{
    /// <summary>
    /// Type of interpersonal social action available in the Relationship Hub and Phone OS (#P4-003, #P4-004).
    /// </summary>
    public enum SocialActionType
    {
        /// <summary>
        /// Quick catch up phone call. Costs minor energy, restores morale, boosts affinity.
        /// </summary>
        CallCatchUp,

        /// <summary>
        /// Send a thoughtful or luxury gift. Costs money depending on relationship, boosts affinity and trust.
        /// </summary>
        SendGift,

        /// <summary>
        /// Go out for dinner or spend quality leisure time together. Costs energy and money, strong affinity boost.
        /// </summary>
        DinnerHangOut,

        /// <summary>
        /// In-depth tactical discussion. Available for Manager and Teammates; boosts tactical understanding and trust.
        /// </summary>
        TalkTactics
    }
}
