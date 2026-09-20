using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents an official transfer and employment offer submitted by a suitor club to a player.
    /// Captures offered weekly wage, squad role, contract duration, transfer fee, and incentive bonuses.
    /// </summary>
    public sealed record TransferOffer
    {
        public const int MinContractYears = 1;
        public const int MaxContractYears = 5;

        private readonly decimal _offeredWage;
        private readonly decimal _transferFee;
        private readonly int _contractYears;
        private readonly decimal _releaseClause;
        private readonly decimal _signingBonus;

        /// <summary>
        /// Unique persistent identifier of this transfer proposal.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Player targeted by the transfer bid.
        /// </summary>
        public Guid PlayerId { get; init; }

        /// <summary>
        /// Prospective acquiring club.
        /// </summary>
        public Guid OfferingClubId { get; init; }

        /// <summary>
        /// Proposed role in the first team hierarchy.
        /// </summary>
        public SquadRole OfferedRole { get; init; }

        /// <summary>
        /// Proposed weekly salary (> 0).
        /// </summary>
        public decimal OfferedWage
        {
            get => _offeredWage;
            init
            {
                if (value <= 0m)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Offered wage must be greater than zero.");
                }
                _offeredWage = value;
            }
        }

        /// <summary>
        /// Transfer compensation fee offered to current club (>= 0; 0 for free agents).
        /// </summary>
        public decimal TransferFee
        {
            get => _transferFee;
            init
            {
                if (value < 0m)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Transfer fee cannot be negative.");
                }
                _transferFee = value;
            }
        }

        /// <summary>
        /// Duration of proposed contract in seasons [1, 5].
        /// </summary>
        public int ContractYears
        {
            get => _contractYears;
            init
            {
                if (value < MinContractYears || value > MaxContractYears)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Contract duration must be between {MinContractYears} and {MaxContractYears} years. Actual: {value}");
                }
                _contractYears = value;
            }
        }

        /// <summary>
        /// Minimum buyout clause in new contract (>= 0; 0 if none).
        /// </summary>
        public decimal ReleaseClause
        {
            get => _releaseClause;
            init
            {
                if (value < 0m)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Release clause cannot be negative.");
                }
                _releaseClause = value;
            }
        }

        /// <summary>
        /// Upfront cash bonus awarded to the player upon contract execution (>= 0).
        /// </summary>
        public decimal SigningBonus
        {
            get => _signingBonus;
            init
            {
                if (value < 0m)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Signing bonus cannot be negative.");
                }
                _signingBonus = value;
            }
        }

        /// <summary>
        /// Date the offer was formally dispatched.
        /// </summary>
        public DateOnly OfferDate { get; init; }

        /// <summary>
        /// Deadline after which the offer lapses (must be >= OfferDate).
        /// </summary>
        public DateOnly ExpiryDate { get; init; }

        /// <summary>
        /// Current lifecycle status of the proposal.
        /// </summary>
        public TransferOfferStatus Status { get; init; }

        public TransferOffer(
            Guid id,
            Guid playerId,
            Guid offeringClubId,
            SquadRole offeredRole,
            decimal offeredWage,
            decimal transferFee,
            int contractYears,
            DateOnly offerDate,
            DateOnly expiryDate,
            decimal releaseClause = 0m,
            decimal signingBonus = 0m,
            TransferOfferStatus status = TransferOfferStatus.Pending)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Offer ID cannot be an empty Guid.", nameof(id));
            }

            if (playerId == Guid.Empty)
            {
                throw new ArgumentException("Player ID cannot be an empty Guid.", nameof(playerId));
            }

            if (offeringClubId == Guid.Empty)
            {
                throw new ArgumentException("Offering club ID cannot be an empty Guid.", nameof(offeringClubId));
            }

            if (offeredWage <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(offeredWage), "Offered wage must be greater than zero.");
            }

            if (transferFee < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(transferFee), "Transfer fee cannot be negative.");
            }

            if (contractYears < MinContractYears || contractYears > MaxContractYears)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(contractYears),
                    $"Contract duration must be between {MinContractYears} and {MaxContractYears} years. Actual: {contractYears}");
            }

            if (releaseClause < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(releaseClause), "Release clause cannot be negative.");
            }

            if (signingBonus < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(signingBonus), "Signing bonus cannot be negative.");
            }

            if (expiryDate < offerDate)
            {
                throw new ArgumentException(
                    $"ExpiryDate ({expiryDate:yyyy-MM-dd}) cannot precede OfferDate ({offerDate:yyyy-MM-dd}).",
                    nameof(expiryDate));
            }

            Id = id;
            PlayerId = playerId;
            OfferingClubId = offeringClubId;
            OfferedRole = offeredRole;
            _offeredWage = offeredWage;
            _transferFee = transferFee;
            _contractYears = contractYears;
            OfferDate = offerDate;
            ExpiryDate = expiryDate;
            _releaseClause = releaseClause;
            _signingBonus = signingBonus;
            Status = status;
        }

        /// <summary>
        /// Factory helper to generate a new <see cref="TransferOffer"/> with a new GUID and pending status.
        /// </summary>
        public static TransferOffer Create(
            Guid playerId,
            Guid offeringClubId,
            SquadRole offeredRole,
            decimal offeredWage,
            decimal transferFee,
            int contractYears,
            DateOnly offerDate,
            DateOnly expiryDate,
            decimal releaseClause = 0m,
            decimal signingBonus = 0m)
        {
            return new TransferOffer(
                Guid.NewGuid(),
                playerId,
                offeringClubId,
                offeredRole,
                offeredWage,
                transferFee,
                contractYears,
                offerDate,
                expiryDate,
                releaseClause,
                signingBonus,
                TransferOfferStatus.Pending);
        }

        /// <summary>
        /// Returns an updated immutable copy with the new <see cref="TransferOfferStatus"/>.
        /// </summary>
        public TransferOffer WithStatus(TransferOfferStatus newStatus)
        {
            return this with { Status = newStatus };
        }

        /// <summary>
        /// Checks if the proposal has elapsed past its expiration date relative to a current calendar date.
        /// </summary>
        public bool IsExpired(DateOnly currentDate) => currentDate > ExpiryDate;
    }
}
