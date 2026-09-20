namespace FootballLife.Domain
{
    /// <summary>
    /// Outcome status of a contract bargaining session.
    /// </summary>
    public enum ContractNegotiationStatus
    {
        /// <summary>
        /// Club accepted the player's demands.
        /// </summary>
        Accepted,

        /// <summary>
        /// Club returned a counter-proposal compromising between demands and budget.
        /// </summary>
        CounterOffer,

        /// <summary>
        /// Negotiation broke down due to exorbitant demands or lack of alignment; club walked away.
        /// </summary>
        WalkedAway
    }
}
