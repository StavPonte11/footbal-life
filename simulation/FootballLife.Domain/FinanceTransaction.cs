using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Immutable record representing an itemised financial transaction on a player's ledger.
    /// Positive amounts denote credits (income); negative amounts denote debits (expenses).
    /// </summary>
    public sealed record FinanceTransaction
    {
        public Guid Id { get; init; }
        public DateOnly Date { get; init; }
        public TransactionType Type { get; init; }
        public decimal Amount { get; init; }
        public string Description { get; init; }

        public FinanceTransaction(
            Guid id,
            DateOnly date,
            TransactionType type,
            decimal amount,
            string description)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Transaction ID cannot be an empty Guid.", nameof(id));

            Id = id;
            Date = date;
            Type = type;
            Amount = amount;
            Description = description ?? string.Empty;
        }

        /// <summary>
        /// Factory helper generating a new transaction with a unique GUID.
        /// </summary>
        public static FinanceTransaction Create(
            DateOnly date,
            TransactionType type,
            decimal amount,
            string description)
        {
            return new FinanceTransaction(Guid.NewGuid(), date, type, amount, description);
        }
    }
}
