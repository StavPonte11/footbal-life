using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the finalized outcome of a contract negotiation interaction between a player and club.
    /// </summary>
    public sealed record ContractNegotiationResult
    {
        public ContractNegotiationStatus Status { get; init; }
        public decimal AgreedWage { get; init; }
        public int AgreedYears { get; init; }
        public decimal AgreedSigningBonus { get; init; }
        public string Feedback { get; init; }

        public ContractNegotiationResult(
            ContractNegotiationStatus status,
            decimal agreedWage,
            int agreedYears,
            decimal agreedSigningBonus,
            string feedback)
        {
            Status = status;
            AgreedWage = Math.Max(0m, agreedWage);
            AgreedYears = Math.Max(0, agreedYears);
            AgreedSigningBonus = Math.Max(0m, agreedSigningBonus);
            Feedback = feedback ?? string.Empty;
        }

        public static ContractNegotiationResult Accept(decimal wage, int years, decimal signingBonus, string feedback)
        {
            return new ContractNegotiationResult(
                ContractNegotiationStatus.Accepted,
                wage,
                years,
                signingBonus,
                feedback);
        }

        public static ContractNegotiationResult Counter(decimal counterWage, int counterYears, decimal signingBonus, string feedback)
        {
            return new ContractNegotiationResult(
                ContractNegotiationStatus.CounterOffer,
                counterWage,
                counterYears,
                signingBonus,
                feedback);
        }

        public static ContractNegotiationResult WalkAway(string feedback)
        {
            return new ContractNegotiationResult(
                ContractNegotiationStatus.WalkedAway,
                agreedWage: 0m,
                agreedYears: 0,
                agreedSigningBonus: 0m,
                feedback);
        }
    }
}
