namespace FootballLife.Domain
{
    /// <summary>
    /// Categorisation of financial transactions recorded on a player's ledger.
    /// </summary>
    public enum TransactionType
    {
        Salary = 0,
        MatchBonus = 1,
        LifestyleExpense = 2,
        Fine = 3,
        Investment = 4,
        TransferBonus = 5
    }
}
