using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.SaveLoad;
using UnityEngine;

namespace FootballLife.Unity.Core.Bridge
{
    /// <summary>
    /// Event snapshot payload broadcast when a simulation day completes.
    /// </summary>
    public readonly struct SimulationDaySnapshot
    {
        public int Season { get; }
        public int Week { get; }
        public int DayOfWeek { get; }
        public int Energy { get; }
        public int Form { get; }
        public int Morale { get; }
        public int ManagerTrust { get; }
        public int BankBalance { get; }
        public int OverallRating { get; }
        public string StatusMessage { get; }

        public SimulationDaySnapshot(
            int season,
            int week,
            int dayOfWeek,
            int energy,
            int form,
            int morale,
            int managerTrust,
            int bankBalance,
            int overallRating,
            string statusMessage)
        {
            Season = season;
            Week = week;
            DayOfWeek = dayOfWeek;
            Energy = energy;
            Form = form;
            Morale = morale;
            ManagerTrust = managerTrust;
            BankBalance = bankBalance;
            OverallRating = overallRating;
            StatusMessage = statusMessage;
        }
    }

    /// <summary>
    /// Event snapshot payload for upcoming match presentation.
    /// </summary>
    public readonly struct MatchOpportunitySnapshot
    {
        public string OpponentName { get; }
        public string Competition { get; }
        public bool IsHome { get; }
        public string SquadRole { get; }
        public int TargetScoreDiff { get; }

        public MatchOpportunitySnapshot(string opponentName, string competition, bool isHome, string squadRole, int targetScoreDiff)
        {
            OpponentName = opponentName;
            Competition = competition;
            IsHome = isHome;
            SquadRole = squadRole;
            TargetScoreDiff = targetScoreDiff;
        }
    }

    /// <summary>
    /// Event snapshot payload for life event dilemmas.
    /// </summary>
    public readonly struct LifeEventSnapshot
    {
        public Guid EventId { get; }
        public string Title { get; }
        public string Description { get; }
        public string Category { get; }
        public IReadOnlyList<string> Choices { get; }

        public LifeEventSnapshot(Guid eventId, string title, string description, string category, IReadOnlyList<string> choices)
        {
            EventId = eventId;
            Title = title;
            Description = description;
            Category = category;
            Choices = choices;
        }
    }

    /// <summary>
    /// Runtime adapter bridging the pure C# CareerSimulationEngine with Unity presentation.
    /// Follows strict architecture rule: Simulation State -> Unity Adapter -> Presentation.
    /// </summary>
    public class SimulationBridge : MonoBehaviour
    {
        public static SimulationBridge? Instance { get; private set; }

        [Header("State")]
        [SerializeField] private bool _autoSaveOnWeekAdvance = true;

        private SaveLoadManager? _saveLoadManager;
        private CareerSaveData? _currentSave;
        private int _currentDayOfWeek = 1; // 1 = Monday, 7 = Sunday
        private SimulationRandom _simRandom = new SimulationRandom(42);

        public bool IsCareerActive => _currentSave != null;
        public CareerSaveData? CurrentSave => _currentSave;
        public SaveLoadManager SaveManager => _saveLoadManager ??= new SaveLoadManager();

        // High-frequency UI presentation events
        public event Action<SimulationDaySnapshot>? OnDayAdvanced;
        public event Action<int>? OnWeekAdvanced;
        public event Action<int>? OnSeasonAdvanced;
        public event Action<MatchOpportunitySnapshot>? OnMatchOpportunity;
        public event Action<LifeEventSnapshot>? OnLifeEventOccurred;
        public event Action<string>? OnStatusLog;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _saveLoadManager = new SaveLoadManager();
        }

        /// <summary>
        /// Starts a brand new career with initialized domain state.
        /// </summary>
        public void StartNewCareer(
            string playerName,
            string nationality,
            Position position,
            Foot preferredFoot,
            string startingClubName,
            int seed = 42)
        {
            _simRandom = new SimulationRandom(seed);
            _currentDayOfWeek = 1;

            _currentSave = new CareerSaveData
            {
                PlayerId = Guid.NewGuid(),
                PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Young Prospect" : playerName,
                Nationality = string.IsNullOrWhiteSpace(nationality) ? "England" : nationality,
                PrimaryPosition = position.ToString(),
                PreferredFoot = preferredFoot.ToString(),
                ClubName = string.IsNullOrWhiteSpace(startingClubName) ? "Northfield Town" : startingClubName,
                CurrentSeason = 1,
                CurrentWeek = 1,
                OverallRating = 60,
                BankBalance = 1500,
                WeeklyWage = 500,
                Energy = 100,
                Form = 70,
                Morale = 75,
                ManagerTrust = 50,
                MasterSeed = seed
            };

            // Initialize baseline abilities
            _currentSave.Pace = 62;
            _currentSave.Acceleration = 64;
            _currentSave.Stamina = 60;
            _currentSave.Passing = 58;
            _currentSave.Shooting = 59;
            _currentSave.Dribbling = 60;
            _currentSave.Vision = 55;

            AutoSave();
            PublishDaySnapshot($"Career began at {_currentSave.ClubName}!");
        }

        /// <summary>
        /// Loads an existing career save slot into the active session.
        /// </summary>
        public bool LoadCareer(int slotIndex)
        {
            var loaded = SaveManager.LoadCareer(slotIndex);
            if (loaded == null)
            {
                Debug.LogWarning($"[SimulationBridge] Could not load save from slot {slotIndex}.");
                return false;
            }

            _currentSave = loaded;
            _simRandom = new SimulationRandom(loaded.MasterSeed + loaded.CurrentWeek);
            _currentDayOfWeek = 1;

            PublishDaySnapshot($"Loaded career: {_currentSave.PlayerName} ({_currentSave.ClubName})");
            return true;
        }

        /// <summary>
        /// Saves active career to the specified manual slot.
        /// </summary>
        public bool SaveCareer(int slotIndex)
        {
            if (_currentSave == null) return false;
            return SaveManager.SaveCareer(slotIndex, _currentSave);
        }

        /// <summary>
        /// Writes to slot 0 (AutoSave).
        /// </summary>
        public void AutoSave()
        {
            if (_currentSave != null)
            {
                SaveManager.SaveCareer(SaveLoadManager.AutoSaveSlot, _currentSave);
            }
        }

        /// <summary>
        /// Advances the simulation by exactly one calendar day.
        /// Week progression, fatigue recovery/decay, and match days occur deterministically.
        /// </summary>
        public void AdvanceDay()
        {
            if (_currentSave == null) return;

            _currentDayOfWeek++;
            string status = $"Advanced to Day {_currentDayOfWeek}";

            // Daily natural energy recovery
            _currentSave.Energy = Math.Min(100, _currentSave.Energy + 5);

            // Saturday (Day 6) is Match Day
            if (_currentDayOfWeek == 6)
            {
                status = "Match Day! Generating match opportunity.";
                OnMatchOpportunity?.Invoke(new MatchOpportunitySnapshot(
                    opponentName: "Westford United",
                    competition: "Division 4",
                    isHome: true,
                    squadRole: _currentSave.SquadRole,
                    targetScoreDiff: 1));
            }
            // Sunday (Day 7) completes the week cycle
            else if (_currentDayOfWeek > 7)
            {
                _currentDayOfWeek = 1;
                _currentSave.CurrentWeek++;

                // Weekly finances: Wage deposit minus living expenses
                int wage = _currentSave.WeeklyWage;
                int livingCost = _currentSave.LifestyleTier * 120;
                int netDeposit = wage - livingCost;
                _currentSave.BankBalance += netDeposit;

                status = $"Week {_currentSave.CurrentWeek} complete! Net income: £{netDeposit:N0}.";
                OnWeekAdvanced?.Invoke(_currentSave.CurrentWeek);

                // Check for random life event occurrence (approx 20% chance per week)
                if (_simRandom.NextDouble() < 0.20)
                {
                    OnLifeEventOccurred?.Invoke(new LifeEventSnapshot(
                        eventId: Guid.NewGuid(),
                        title: "Sponsorship Offer",
                        description: "A local sports apparel brand offers an endorsement contract.",
                        category: "Lifestyle",
                        choices: new[] { "Accept (£1,000 + Morale)", "Decline", "Negotiate Higher" }));
                }

                if (_autoSaveOnWeekAdvance)
                {
                    AutoSave();
                }

                // 40-week season boundary
                if (_currentSave.CurrentWeek > 40)
                {
                    _currentSave.CurrentWeek = 1;
                    _currentSave.CurrentSeason++;
                    status = $"Season {_currentSave.CurrentSeason - 1} finished! Welcome to Season {_currentSave.CurrentSeason}!";
                    OnSeasonAdvanced?.Invoke(_currentSave.CurrentSeason);
                }
            }

            PublishDaySnapshot(status);
        }

        /// <summary>
        /// Applies training effects for the player's primary attributes.
        /// </summary>
        public void SelectWeeklyTraining(string attributeCategory, int intensityLevel)
        {
            if (_currentSave == null) return;

            int energyCost = intensityLevel * 12;
            if (_currentSave.Energy < energyCost)
            {
                OnStatusLog?.Invoke("Player is too exhausted for this training intensity!");
                return;
            }

            _currentSave.Energy = Math.Max(0, _currentSave.Energy - energyCost);
            _currentSave.Form = Math.Min(100, _currentSave.Form + (intensityLevel * 3));

            OnStatusLog?.Invoke($"Completed {attributeCategory} training (Energy -{energyCost}, Form +{intensityLevel * 3})");
            PublishDaySnapshot("Training completed.");
        }

        /// <summary>
        /// Performs dedicated rest & recovery to restore energy and stamina.
        /// </summary>
        public void PerformRest(string restMethod)
        {
            if (_currentSave == null) return;

            int energyRestored = restMethod == "Physio" ? 35 : 20;
            _currentSave.Energy = Math.Min(100, _currentSave.Energy + energyRestored);

            OnStatusLog?.Invoke($"Rest ({restMethod}) restored +{energyRestored} Energy.");
            PublishDaySnapshot("Rest session completed.");
        }

        private void PublishDaySnapshot(string message)
        {
            if (_currentSave == null) return;

            var snapshot = new SimulationDaySnapshot(
                season: _currentSave.CurrentSeason,
                week: _currentSave.CurrentWeek,
                dayOfWeek: _currentDayOfWeek,
                energy: _currentSave.Energy,
                form: _currentSave.Form,
                morale: _currentSave.Morale,
                managerTrust: _currentSave.ManagerTrust,
                bankBalance: _currentSave.BankBalance,
                overallRating: _currentSave.OverallRating,
                statusMessage: message
            );

            OnDayAdvanced?.Invoke(snapshot);
            OnStatusLog?.Invoke(message);
        }
    }
}
