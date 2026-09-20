namespace FootballLife.Domain
{
    /// <summary>
    /// Lifecycle states of an official transfer offer.
    /// </summary>
    public enum TransferOfferStatus
    {
        /// <summary>
        /// Offer is active and awaiting player/club decision.
        /// </summary>
        Pending,

        /// <summary>
        /// Offer has been accepted; transfer executed.
        /// </summary>
        Accepted,

        /// <summary>
        /// Offer was rejected by the player or club.
        /// </summary>
        Rejected,

        /// <summary>
        /// Offer window elapsed without acceptance.
        /// </summary>
        Expired,

        /// <summary>
        /// Offering club cancelled the bid before a decision was finalized.
        /// </summary>
        Withdrawn
    }
}
