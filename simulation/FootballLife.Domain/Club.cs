using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents a football club with institutional reputation, league affiliation,
    /// financial standing, facility standard, stadium, visuals, fanbase, ownership, and history.
    /// </summary>
    public sealed record Club
    {
        public const int MinReputation = 1;
        public const int MaxReputation = 100;
        public const int MinFacility = 1;
        public const int MaxFacility = 5;

        private readonly int _reputationRating;
        private readonly int _facilityRating;

        /// <summary>
        /// Unique persistent identifier of the club.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Full display name of the club.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Short abbreviated name (e.g., "ARS", "MCI", "RMA").
        /// </summary>
        public string ShortName { get; init; }

        /// <summary>
        /// Identifier of the league this club currently competes in.
        /// </summary>
        public Guid LeagueId { get; init; }

        /// <summary>
        /// Institutional prestige and world ranking [1, 100].
        /// </summary>
        public int ReputationRating
        {
            get => _reputationRating;
            init
            {
                if (value < MinReputation || value > MaxReputation)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Club reputation must be in [{MinReputation}, {MaxReputation}]. Actual: {value}");
                }
                _reputationRating = value;
            }
        }

        /// <summary>
        /// Current financial status and budget constraints of the club.
        /// </summary>
        public ClubFinances Finances { get; init; }

        /// <summary>
        /// Training and youth academy infrastructure grade [1, 5].
        /// </summary>
        public int FacilityRating
        {
            get => _facilityRating;
            init
            {
                if (value < MinFacility || value > MaxFacility)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Facility rating must be in [{MinFacility}, {MaxFacility}]. Actual: {value}");
                }
                _facilityRating = value;
            }
        }

        /// <summary>
        /// Default tactical style and sporting philosophy of the club.
        /// </summary>
        public TacticalIdentity TacticalStyle { get; init; }

        /// <summary>
        /// Currently appointed first team manager, or null if vacant.
        /// </summary>
        public Guid? ManagerId { get; init; }

        /// <summary>
        /// Home ground and pitch configuration.
        /// </summary>
        public ClubStadium Stadium { get; init; }

        /// <summary>
        /// Visual identity assets and kit color schemes.
        /// </summary>
        public ClubVisuals Visuals { get; init; }

        /// <summary>
        /// Supporter demographic, loyalty, and performance expectation level.
        /// </summary>
        public ClubFanbase Fanbase { get; init; }

        /// <summary>
        /// Owner and board governance profile.
        /// </summary>
        public ClubBoard Board { get; init; }

        /// <summary>
        /// Historical legacy, founding year, and trophy honors.
        /// </summary>
        public ClubHistory History { get; init; }

        /// <summary>
        /// Immutable list of player IDs assigned to the club roster.
        /// </summary>
        public IReadOnlyList<Guid> SquadPlayerIds { get; init; }

        public Club(
            Guid id,
            string name,
            string shortName,
            Guid leagueId,
            int reputationRating,
            ClubFinances finances,
            int facilityRating,
            TacticalIdentity tacticalStyle,
            Guid? managerId = null,
            ClubStadium? stadium = null,
            ClubVisuals? visuals = null,
            ClubFanbase? fanbase = null,
            ClubBoard? board = null,
            ClubHistory? history = null,
            IReadOnlyList<Guid>? squadPlayerIds = null)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Club ID cannot be an empty Guid.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Club name cannot be null or whitespace.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(shortName))
            {
                throw new ArgumentException("Club short name cannot be null or whitespace.", nameof(shortName));
            }

            if (leagueId == Guid.Empty)
            {
                throw new ArgumentException("League ID cannot be an empty Guid.", nameof(leagueId));
            }

            if (reputationRating < MinReputation || reputationRating > MaxReputation)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reputationRating),
                    $"Reputation rating must be in [{MinReputation}, {MaxReputation}]. Actual: {reputationRating}");
            }

            if (finances is null)
            {
                throw new ArgumentNullException(nameof(finances), "Club finances cannot be null.");
            }

            if (facilityRating < MinFacility || facilityRating > MaxFacility)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(facilityRating),
                    $"Facility rating must be in [{MinFacility}, {MaxFacility}]. Actual: {facilityRating}");
            }

            string trimmedName = name.Trim();
            string trimmedShort = shortName.Trim();

            Id = id;
            Name = trimmedName;
            ShortName = trimmedShort;
            LeagueId = leagueId;
            _reputationRating = reputationRating;
            Finances = finances;
            _facilityRating = facilityRating;
            TacticalStyle = tacticalStyle;
            ManagerId = managerId;
            Stadium = stadium ?? ClubStadium.Default(trimmedName);
            Visuals = visuals ?? ClubVisuals.Default(trimmedShort);
            Fanbase = fanbase ?? ClubFanbase.Default(reputationRating);
            Board = board ?? ClubBoard.Default(trimmedName);
            History = history ?? ClubHistory.Default;
            SquadPlayerIds = squadPlayerIds ?? Array.Empty<Guid>();
        }

        /// <summary>
        /// Factory method generating a new Club instance with an automatically generated GUID.
        /// </summary>
        public static Club Create(
            string name,
            string shortName,
            Guid leagueId,
            int reputationRating,
            ClubFinances finances,
            int facilityRating,
            TacticalIdentity tacticalStyle,
            Guid? managerId = null,
            ClubStadium? stadium = null,
            ClubVisuals? visuals = null,
            ClubFanbase? fanbase = null,
            ClubBoard? board = null,
            ClubHistory? history = null,
            IReadOnlyList<Guid>? squadPlayerIds = null)
        {
            return new Club(
                Guid.NewGuid(),
                name,
                shortName,
                leagueId,
                reputationRating,
                finances,
                facilityRating,
                tacticalStyle,
                managerId,
                stadium,
                visuals,
                fanbase,
                board,
                history,
                squadPlayerIds);
        }

        /// <summary>
        /// Adds a player ID to the club's active squad roster, returning a new immutable Club instance.
        /// </summary>
        public Club WithAddedPlayer(Guid playerId)
        {
            if (playerId == Guid.Empty) throw new ArgumentException("Player ID cannot be empty.", nameof(playerId));
            if (SquadPlayerIds.Contains(playerId)) return this;

            var updated = new List<Guid>(SquadPlayerIds) { playerId };
            return this with { SquadPlayerIds = updated.AsReadOnly() };
        }

        /// <summary>
        /// Removes a player ID from the squad roster, returning a new immutable Club instance.
        /// </summary>
        public Club WithRemovedPlayer(Guid playerId)
        {
            if (!SquadPlayerIds.Contains(playerId)) return this;

            var updated = new List<Guid>(SquadPlayerIds);
            updated.Remove(playerId);
            return this with { SquadPlayerIds = updated.AsReadOnly() };
        }

        /// <summary>
        /// Updates the league affiliation of the club, returning a new immutable Club instance.
        /// </summary>
        public Club WithLeague(Guid leagueId)
        {
            return this with { LeagueId = leagueId };
        }
    }
}
