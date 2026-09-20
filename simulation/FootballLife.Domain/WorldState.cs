using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents an immutable, coherent snapshot of the entire simulated football universe at a given point in time.
    /// Acts as the single source of truth passed between simulation ticks.
    /// </summary>
    public sealed record WorldState
    {
        public IReadOnlyDictionary<Guid, Club> Clubs { get; init; }
        public IReadOnlyDictionary<Guid, Player> Players { get; init; }
        public IReadOnlyDictionary<Guid, PlayerAbilities> Abilities { get; init; }
        public IReadOnlyDictionary<Guid, PlayerState> States { get; init; }
        public IReadOnlyDictionary<Guid, PlayerCareerState> CareerStates { get; init; }
        public IReadOnlyDictionary<Guid, League> Leagues { get; init; }
        public IReadOnlyDictionary<Guid, Manager> Managers { get; init; }
        public IReadOnlyDictionary<Guid, Contract> Contracts { get; init; }
        /// <summary>
        /// Development ceiling data for all players whose potential has been generated.
        /// Not every player in <see cref="Players"/> is guaranteed to have an entry;
        /// potential is only created at career initialisation or via <see cref="PlayerFactory"/>.
        /// </summary>
        public IReadOnlyDictionary<Guid, PlayerPotential> Potentials { get; init; }
        /// <summary>
        /// Bank accounts and financial transaction history for players.
        /// </summary>
        public IReadOnlyDictionary<Guid, FinanceAccount> Accounts { get; init; }
        public Season CurrentSeason { get; init; }

        public WorldState(
            IReadOnlyDictionary<Guid, Club> clubs,
            IReadOnlyDictionary<Guid, Player> players,
            IReadOnlyDictionary<Guid, PlayerAbilities> abilities,
            IReadOnlyDictionary<Guid, PlayerState> states,
            IReadOnlyDictionary<Guid, PlayerCareerState> careerStates,
            IReadOnlyDictionary<Guid, League> leagues,
            IReadOnlyDictionary<Guid, Manager> managers,
            IReadOnlyDictionary<Guid, Contract> contracts,
            IReadOnlyDictionary<Guid, PlayerPotential> potentials,
            Season currentSeason,
            IReadOnlyDictionary<Guid, FinanceAccount>? accounts = null)
        {
            Clubs = clubs ?? throw new ArgumentNullException(nameof(clubs));
            Players = players ?? throw new ArgumentNullException(nameof(players));
            Abilities = abilities ?? throw new ArgumentNullException(nameof(abilities));
            States = states ?? throw new ArgumentNullException(nameof(states));
            CareerStates = careerStates ?? throw new ArgumentNullException(nameof(careerStates));
            Leagues = leagues ?? throw new ArgumentNullException(nameof(leagues));
            Managers = managers ?? throw new ArgumentNullException(nameof(managers));
            Contracts = contracts ?? throw new ArgumentNullException(nameof(contracts));
            Potentials = potentials ?? throw new ArgumentNullException(nameof(potentials));
            CurrentSeason = currentSeason ?? throw new ArgumentNullException(nameof(currentSeason));
            Accounts = accounts ?? new ReadOnlyDictionary<Guid, FinanceAccount>(new Dictionary<Guid, FinanceAccount>());
        }

        /// <summary>
        /// Creates an empty initial WorldState tied to a season.
        /// </summary>
        public static WorldState CreateEmpty(Season season)
        {
            return new WorldState(
                new ReadOnlyDictionary<Guid, Club>(new Dictionary<Guid, Club>()),
                new ReadOnlyDictionary<Guid, Player>(new Dictionary<Guid, Player>()),
                new ReadOnlyDictionary<Guid, PlayerAbilities>(new Dictionary<Guid, PlayerAbilities>()),
                new ReadOnlyDictionary<Guid, PlayerState>(new Dictionary<Guid, PlayerState>()),
                new ReadOnlyDictionary<Guid, PlayerCareerState>(new Dictionary<Guid, PlayerCareerState>()),
                new ReadOnlyDictionary<Guid, League>(new Dictionary<Guid, League>()),
                new ReadOnlyDictionary<Guid, Manager>(new Dictionary<Guid, Manager>()),
                new ReadOnlyDictionary<Guid, Contract>(new Dictionary<Guid, Contract>()),
                new ReadOnlyDictionary<Guid, PlayerPotential>(new Dictionary<Guid, PlayerPotential>()),
                season,
                new ReadOnlyDictionary<Guid, FinanceAccount>(new Dictionary<Guid, FinanceAccount>()));
        }

        // ─── Query & Lookup Methods ───────────────────────────────────────────

        public Player GetPlayer(Guid id)
        {
            if (Players.TryGetValue(id, out var player)) return player;
            throw new KeyNotFoundException($"Player with ID '{id}' was not found in WorldState.");
        }

        public PlayerAbilities GetAbilities(Guid id)
        {
            if (Abilities.TryGetValue(id, out var abilities)) return abilities;
            throw new KeyNotFoundException($"Abilities for Player '{id}' were not found in WorldState.");
        }

        public PlayerState GetState(Guid id)
        {
            if (States.TryGetValue(id, out var state)) return state;
            throw new KeyNotFoundException($"State for Player '{id}' was not found in WorldState.");
        }

        public PlayerCareerState GetCareerState(Guid id)
        {
            if (CareerStates.TryGetValue(id, out var careerState)) return careerState;
            throw new KeyNotFoundException($"CareerState for Player '{id}' was not found in WorldState.");
        }

        public Club GetClub(Guid id)
        {
            if (Clubs.TryGetValue(id, out var club)) return club;
            throw new KeyNotFoundException($"Club with ID '{id}' was not found in WorldState.");
        }

        public League GetLeague(Guid id)
        {
            if (Leagues.TryGetValue(id, out var league)) return league;
            throw new KeyNotFoundException($"League with ID '{id}' was not found in WorldState.");
        }

        public Manager GetManager(Guid id)
        {
            if (Managers.TryGetValue(id, out var manager)) return manager;
            throw new KeyNotFoundException($"Manager with ID '{id}' was not found in WorldState.");
        }

        public Contract? FindContractForPlayer(Guid playerId)
        {
            return Contracts.Values.FirstOrDefault(c => c.PlayerId == playerId);
        }

        public Contract GetContractForPlayer(Guid playerId)
        {
            var contract = FindContractForPlayer(playerId);
            if (contract is not null) return contract;
            throw new KeyNotFoundException($"No active contract found for player '{playerId}'.");
        }

        /// <summary>
        /// Retrieves the <see cref="PlayerPotential"/> for the given player.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when no potential exists for the player.</exception>
        public PlayerPotential GetPotential(Guid playerId)
        {
            if (Potentials.TryGetValue(playerId, out var potential)) return potential;
            throw new KeyNotFoundException($"Potential for Player '{playerId}' was not found in WorldState.");
        }

        /// <summary>
        /// Retrieves the <see cref="FinanceAccount"/> for the given player.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when no finance account exists for the player.</exception>
        public FinanceAccount GetAccount(Guid playerId)
        {
            if (Accounts.TryGetValue(playerId, out var account)) return account;
            throw new KeyNotFoundException($"FinanceAccount for Player '{playerId}' was not found in WorldState.");
        }

        /// <summary>
        /// Attempts to retrieve the <see cref="FinanceAccount"/> for the given player without throwing.
        /// </summary>
        public bool TryGetAccount(Guid playerId, out FinanceAccount account)
        {
            return Accounts.TryGetValue(playerId, out account);
        }

        // ─── Immutable Mutation Helpers ───────────────────────────────────────

        /// <summary>
        /// Registers or updates a player and all corresponding facets in the snapshot.
        /// Optionally registers a <see cref="PlayerPotential"/> and/or <see cref="FinanceAccount"/> at the same time.
        /// </summary>
        public WorldState WithPlayer(
            Player player,
            PlayerAbilities abilities,
            PlayerState state,
            PlayerCareerState careerState,
            PlayerPotential? potential = null,
            FinanceAccount? account = null)
        {
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));
            if (state is null) throw new ArgumentNullException(nameof(state));
            if (careerState is null) throw new ArgumentNullException(nameof(careerState));

            var newPlayers = new Dictionary<Guid, Player>(Players) { [player.Id] = player };
            var newAbilities = new Dictionary<Guid, PlayerAbilities>(Abilities) { [player.Id] = abilities };
            var newStates = new Dictionary<Guid, PlayerState>(States) { [player.Id] = state };
            var newCareer = new Dictionary<Guid, PlayerCareerState>(CareerStates) { [player.Id] = careerState };

            var result = this with
            {
                Players = new ReadOnlyDictionary<Guid, Player>(newPlayers),
                Abilities = new ReadOnlyDictionary<Guid, PlayerAbilities>(newAbilities),
                States = new ReadOnlyDictionary<Guid, PlayerState>(newStates),
                CareerStates = new ReadOnlyDictionary<Guid, PlayerCareerState>(newCareer)
            };

            if (potential is not null)
                result = result.WithPlayerPotential(player.Id, potential);

            if (account is not null)
                result = result.WithPlayerAccount(player.Id, account);

            return result;
        }

        /// <summary>
        /// Registers or replaces the <see cref="FinanceAccount"/> for a player.
        /// </summary>
        public WorldState WithPlayerAccount(Guid playerId, FinanceAccount account)
        {
            if (account is null) throw new ArgumentNullException(nameof(account));
            var newAccounts = new Dictionary<Guid, FinanceAccount>(Accounts) { [playerId] = account };
            return this with { Accounts = new ReadOnlyDictionary<Guid, FinanceAccount>(newAccounts) };
        }

        /// <summary>
        /// Registers or replaces the <see cref="PlayerPotential"/> for a player.
        /// Does NOT require the player to already exist in <see cref="Players"/>;
        /// potential may be set during factory initialisation before the player record is added.
        /// </summary>
        public WorldState WithPlayerPotential(Guid playerId, PlayerPotential potential)
        {
            if (potential is null) throw new ArgumentNullException(nameof(potential));
            var newPotentials = new Dictionary<Guid, PlayerPotential>(Potentials) { [playerId] = potential };
            return this with { Potentials = new ReadOnlyDictionary<Guid, PlayerPotential>(newPotentials) };
        }

        public WorldState WithPlayerState(Guid playerId, PlayerState state)
        {
            if (!Players.ContainsKey(playerId)) throw new KeyNotFoundException($"Player '{playerId}' not found.");
            if (state is null) throw new ArgumentNullException(nameof(state));

            var newStates = new Dictionary<Guid, PlayerState>(States) { [playerId] = state };
            return this with { States = new ReadOnlyDictionary<Guid, PlayerState>(newStates) };
        }

        public WorldState WithPlayerAbilities(Guid playerId, PlayerAbilities abilities)
        {
            if (!Players.ContainsKey(playerId)) throw new KeyNotFoundException($"Player '{playerId}' not found.");
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));

            var newAbilities = new Dictionary<Guid, PlayerAbilities>(Abilities) { [playerId] = abilities };
            return this with { Abilities = new ReadOnlyDictionary<Guid, PlayerAbilities>(newAbilities) };
        }

        public WorldState WithPlayerCareerState(Guid playerId, PlayerCareerState careerState)
        {
            if (!Players.ContainsKey(playerId)) throw new KeyNotFoundException($"Player '{playerId}' not found.");
            if (careerState is null) throw new ArgumentNullException(nameof(careerState));

            var newCareer = new Dictionary<Guid, PlayerCareerState>(CareerStates) { [playerId] = careerState };
            return this with { CareerStates = new ReadOnlyDictionary<Guid, PlayerCareerState>(newCareer) };
        }

        public WorldState WithClub(Club club)
        {
            if (club is null) throw new ArgumentNullException(nameof(club));
            var newClubs = new Dictionary<Guid, Club>(Clubs) { [club.Id] = club };
            return this with { Clubs = new ReadOnlyDictionary<Guid, Club>(newClubs) };
        }

        public WorldState WithLeague(League league)
        {
            if (league is null) throw new ArgumentNullException(nameof(league));
            var newLeagues = new Dictionary<Guid, League>(Leagues) { [league.Id] = league };
            return this with { Leagues = new ReadOnlyDictionary<Guid, League>(newLeagues) };
        }

        public WorldState WithManager(Manager manager)
        {
            if (manager is null) throw new ArgumentNullException(nameof(manager));
            var newManagers = new Dictionary<Guid, Manager>(Managers) { [manager.Id] = manager };
            return this with { Managers = new ReadOnlyDictionary<Guid, Manager>(newManagers) };
        }

        public WorldState WithContract(Contract contract)
        {
            if (contract is null) throw new ArgumentNullException(nameof(contract));
            var newContracts = new Dictionary<Guid, Contract>(Contracts) { [contract.Id] = contract };
            return this with { Contracts = new ReadOnlyDictionary<Guid, Contract>(newContracts) };
        }

        public WorldState WithSeason(Season season)
        {
            if (season is null) throw new ArgumentNullException(nameof(season));
            return this with { CurrentSeason = season };
        }
    }
}
