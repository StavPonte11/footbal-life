namespace FootballLife.Domain
{
    /// <summary>
    /// Contractual timeline status relative to legal transfer and Bosman free agent rules.
    /// </summary>
    public enum ContractExpiryStatus
    {
        /// <summary>
        /// Contract is active with over six months of duration remaining.
        /// </summary>
        Active,

        /// <summary>
        /// Contract has 6 months or fewer remaining (&lt;= 26 weeks);
        /// player is legally entitled to enter pre-contract negotiations with outside clubs (Bosman ruling).
        /// </summary>
        BosmanEligible,

        /// <summary>
        /// Contract has elapsed; player is an unattached free agent unless renewed.
        /// </summary>
        Expired
    }
}
