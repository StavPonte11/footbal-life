using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Immutable domain record representing a player's financial bank account and ledger history.
    /// Tracks current balance and all historical transactions in chronological order.
    /// Supports overdrafts (negative balances) reflecting debt.
    /// </summary>
    public sealed record FinanceAccount
    {
        public decimal Balance { get; init; }
        public IReadOnlyList<FinanceTransaction> History { get; init; }

        public bool IsInDebt => Balance < 0m;
        public decimal DebtAmount => Balance < 0m ? -Balance : 0m;

        public FinanceAccount(decimal balance, IReadOnlyList<FinanceTransaction>? history = null)
        {
            Balance = balance;
            History = history != null
                ? new List<FinanceTransaction>(history).AsReadOnly()
                : Array.Empty<FinanceTransaction>();
        }

        /// <summary>
        /// Creates a new account with an optional starting balance.
        /// </summary>
        public static FinanceAccount Create(decimal initialBalance = 0m)
        {
            return new FinanceAccount(initialBalance);
        }

        /// <summary>
        /// Returns a new <see cref="FinanceAccount"/> with the transaction applied to the balance and appended to history.
        /// </summary>
        public FinanceAccount WithTransaction(FinanceTransaction transaction)
        {
            if (transaction is null) throw new ArgumentNullException(nameof(transaction));

            var newHistory = new List<FinanceTransaction>(History.Count + 1);
            newHistory.AddRange(History);
            newHistory.Add(transaction);

            return this with
            {
                Balance = Balance + transaction.Amount,
                History = newHistory.AsReadOnly()
            };
        }
    }
}
