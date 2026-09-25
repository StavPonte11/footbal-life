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
        public string CharacterBadge { get; }
        public IReadOnlyList<string> ChoiceEffects { get; }

        public LifeEventSnapshot(
            Guid eventId,
            string title,
            string description,
            string category,
            IReadOnlyList<string> choices,
            string characterBadge = "📋 Club Liaison",
            IReadOnlyList<string>? choiceEffects = null)
        {
            EventId = eventId;
            Title = title;
            Description = description;
            Category = category;
            Choices = choices;
            CharacterBadge = characterBadge;
            ChoiceEffects = choiceEffects ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Snapshot payload for end-of-season summary screen (#P2-018).
    /// </summary>
    public readonly struct SeasonSummarySnapshot
    {
        public int Season { get; }
        public string ClubName { get; }
        public string Division { get; }
        public string LeaguePosition { get; }
        public int TotalAppearances { get; }
        public int TotalGoals { get; }
        public int TotalAssists { get; }
        public double AverageRating { get; }
        public int TotalWagesEarned { get; }
        public int TotalExpenses { get; }
        public int NetSavings { get; }
        public string TrophyAchievement { get; }

        public SeasonSummarySnapshot(
            int season,
            string clubName,
            string division,
            string leaguePosition,
            int totalAppearances,
            int totalGoals,
            int totalAssists,
            double averageRating,
            int totalWagesEarned,
            int totalExpenses,
            int netSavings,
            string trophyAchievement)
        {
            Season = season;
            ClubName = clubName;
            Division = division;
            LeaguePosition = leaguePosition;
            TotalAppearances = totalAppearances;
            TotalGoals = totalGoals;
            TotalAssists = totalAssists;
            AverageRating = averageRating;
            TotalWagesEarned = totalWagesEarned;
            TotalExpenses = totalExpenses;
            NetSavings = netSavings;
            TrophyAchievement = trophyAchievement;
        }
    }

    /// <summary>
    /// Snapshot payload for annual attribute development visualization (#P2-019).
    /// </summary>
    public readonly struct AttributeGrowthSnapshot
    {
        public int StartOvr { get; }
        public int EndOvr { get; }
        public int PotentialRating { get; }
        public int Age { get; }
        public string Phase { get; }
        public int PaceDelta { get; }
        public int ShootingDelta { get; }
        public int PassingDelta { get; }
        public int DribblingDelta { get; }
        public int StaminaDelta { get; }
        public int VisionDelta { get; }

        public AttributeGrowthSnapshot(
            int startOvr,
            int endOvr,
            int potentialRating,
            int age,
            string phase,
            int paceDelta,
            int shootingDelta,
            int passingDelta,
            int dribblingDelta,
            int staminaDelta,
            int visionDelta)
        {
            StartOvr = startOvr;
            EndOvr = endOvr;
            PotentialRating = potentialRating;
            Age = age;
            Phase = phase;
            PaceDelta = paceDelta;
            ShootingDelta = shootingDelta;
            PassingDelta = passingDelta;
            DribblingDelta = dribblingDelta;
            StaminaDelta = staminaDelta;
            VisionDelta = visionDelta;
        }
    }

    /// <summary>
    /// Snapshot payload for transfer window offers and contract extensions (#P2-020).
    /// </summary>
    public readonly struct TransferOfferSnapshot
    {
        public string OfferId { get; }
        public string ClubName { get; }
        public string Division { get; }
        public int OfferedWeeklyWage { get; }
        public int SigningBonus { get; }
        public int TransferFee { get; }
        public string SquadRole { get; }
        public int PrestigeStars { get; }
        public bool IsRenewal { get; }

        public TransferOfferSnapshot(
            string offerId,
            string clubName,
            string division,
            int offeredWeeklyWage,
            int signingBonus,
            int transferFee,
            string squadRole,
            int prestigeStars,
            bool isRenewal)
        {
            OfferId = offerId;
            ClubName = clubName;
            Division = division;
            OfferedWeeklyWage = offeredWeeklyWage;
            SigningBonus = signingBonus;
            TransferFee = transferFee;
            SquadRole = squadRole;
            PrestigeStars = prestigeStars;
            IsRenewal = isRenewal;
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
        public MatchOpportunitySnapshot? CurrentMatchOpportunity { get; set; }

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
                var matchOpp = new MatchOpportunitySnapshot(
                    opponentName: "Westford United",
                    competition: "Division 4",
                    isHome: true,
                    squadRole: _currentSave.SquadRole,
                    targetScoreDiff: 1);
                CurrentMatchOpportunity = matchOpp;
                OnMatchOpportunity?.Invoke(matchOpp);
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
                if (_simRandom.NextFloat(0f, 1f) < 0.20f)
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

        /// <summary>
        /// Resolves a selected life event choice, applying state consequences and saving.
        /// </summary>
        public void ResolveLifeEventChoice(LifeEventSnapshot snap, int choiceIndex, string choiceText, int moneyDelta = 0, int moraleDelta = 0, int trustDelta = 0, int energyDelta = 0)
        {
            if (_currentSave == null) return;

            if (moneyDelta != 0)
            {
                _currentSave.BankBalance += moneyDelta;
            }

            if (moraleDelta != 0)
            {
                _currentSave.Morale = Math.Clamp(_currentSave.Morale + moraleDelta, 0, 100);
            }

            if (trustDelta != 0)
            {
                _currentSave.ManagerTrust = Math.Clamp(_currentSave.ManagerTrust + trustDelta, 0, 100);
            }

            if (energyDelta != 0)
            {
                _currentSave.Energy = Math.Clamp(_currentSave.Energy + energyDelta, 0, 100);
            }

            string feedback = $"Resolved '{snap.Title}': {choiceText}";
            OnStatusLog?.Invoke(feedback);
            PublishDaySnapshot(feedback);
        }

        /// <summary>
        /// Triggers a rich life event dilemma snapshot for testing or gameplay invocation.
        /// </summary>
        public void TriggerDilemma(int dilemmaIndex = 0)
        {
            var dilemmas = GetDefaultDilemmas();
            int idx = Math.Clamp(dilemmaIndex, 0, dilemmas.Count - 1);
            OnLifeEventOccurred?.Invoke(dilemmas[idx]);
        }

        public static IReadOnlyList<LifeEventSnapshot> GetDefaultDilemmas() => new[]
        {
            new LifeEventSnapshot(
                Guid.NewGuid(),
                "Commercial Endorsement Deal",
                "A prominent luxury streetwear label wants you as the headline ambassador for their spring collection. The compensation is exceptional, but requires attending weekend photo sessions during crucial match preparation.",
                "Commercial",
                new[] { "Accept Lucrative Deal", "Decline to Stay Focused", "Negotiate Reduced Hours" },
                "👔 Agent David Sterling",
                new[] { "+£3,500 Bank • +8 Morale • -4 Energy", "+10 Manager Trust • +5 Energy", "+£1,500 Bank • +4 Morale" }),

            new LifeEventSnapshot(
                Guid.NewGuid(),
                "Late Night Out with Teammates",
                "After an impressive league performance, team captain Liam Walker invites you out to a private VIP lounge in town to celebrate with the squad.",
                "Social",
                new[] { "Join Full Squad Celebration", "One Drink & Head Home Early", "Politely Decline & Get Sleep" },
                "⚡ Liam Walker (Vice Captain)",
                new[] { "+15 Morale • -20 Energy • -8 Trust", "+6 Morale • -6 Energy", "+12 Energy • +6 Manager Trust" }),

            new LifeEventSnapshot(
                Guid.NewGuid(),
                "Sensationalist Press Rumour",
                "A sensationalist tabloid runs a front-page rumor claiming you are unhappy with your role and angling for an emergency transfer.",
                "Media",
                new[] { "Publicly Denounce Rumours", "No Comment / Stay Silent", "Feed Transfer Speculation" },
                "📰 The Daily Pitch Gossip",
                new[] { "+15 Manager Trust • +5 Morale", "No Stat Impact", "+12 Morale • -15 Manager Trust" }),

            new LifeEventSnapshot(
                Guid.NewGuid(),
                "Manager Tactical Disagreement",
                "Head Coach Elena Rostova pulls you aside in film analysis, pointing out tactical lapses in your defensive pressing work rate.",
                "Club",
                new[] { "Accept Criticism & Promise Extra Runs", "Argue You Need Attacking Freedom", "Ask for Private 1-on-1 Mentoring" },
                "📋 Elena Rostova (Head Coach)",
                new[] { "+12 Manager Trust • -5 Morale", "+8 Morale • -12 Manager Trust", "+8 Trust • +5 Positioning XP" })
        };

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

        /// <summary>
        /// Records match results, updates player statistics, applies condition changes, and auto-saves.
        /// </summary>
        public void RecordMatchResult(
            int playerGoals,
            int playerAssists,
            double matchRating,
            int homeScore,
            int awayScore,
            int managerTrustDelta = 5,
            int formDelta = 4,
            int moraleDelta = 5,
            int energyCost = 25)
        {
            if (_currentSave == null) return;

            _currentSave.TotalAppearances++;
            _currentSave.TotalGoals += playerGoals;
            _currentSave.TotalAssists += playerAssists;

            if (_currentSave.TotalAppearances == 1)
            {
                _currentSave.AverageRating = Math.Round(matchRating, 2);
            }
            else
            {
                double prevTotal = _currentSave.AverageRating * (_currentSave.TotalAppearances - 1);
                _currentSave.AverageRating = Math.Round((prevTotal + matchRating) / _currentSave.TotalAppearances, 2);
            }

            _currentSave.Energy = Math.Max(10, _currentSave.Energy - energyCost);
            _currentSave.Form = Math.Clamp(_currentSave.Form + formDelta, 0, 100);
            _currentSave.Morale = Math.Clamp(_currentSave.Morale + moraleDelta, 0, 100);
            _currentSave.ManagerTrust = Math.Clamp(_currentSave.ManagerTrust + managerTrustDelta, 0, 100);

            string outcomeStr = homeScore > awayScore ? "Won" : (homeScore == awayScore ? "Drew" : "Lost");
            string logMsg = $"Match complete! {outcomeStr} ({homeScore}-{awayScore}). Rating: {matchRating:F2}. Goals: {playerGoals}.";
            OnStatusLog?.Invoke(logMsg);

            AutoSave();
            PublishDaySnapshot(logMsg);
        }

        // ── Milestone 2.6 Off-Season & Transfers ──────────────────────────────

        public SeasonSummarySnapshot GetSeasonSummaryData()
        {
            if (_currentSave == null)
            {
                return new SeasonSummarySnapshot(
                    season: 1,
                    clubName: "Northfield Town",
                    division: "Division 4",
                    leaguePosition: "1st (Champions & Promoted)",
                    totalAppearances: 34,
                    totalGoals: 18,
                    totalAssists: 9,
                    averageRating: 7.45,
                    totalWagesEarned: 38000,
                    totalExpenses: 4560,
                    netSavings: 33440,
                    trophyAchievement: "Division 4 Championship Trophy 🏆");
            }

            int season = _currentSave.CurrentSeason;
            string club = !string.IsNullOrEmpty(_currentSave.ClubName) ? _currentSave.ClubName : "Northfield Town";
            string div = "Division 4";
            int apps = Math.Max(1, _currentSave.TotalAppearances);
            int goals = _currentSave.TotalGoals;
            int assists = _currentSave.TotalAssists;
            double rating = _currentSave.AverageRating > 0 ? _currentSave.AverageRating : 7.20;

            string pos = goals >= 15 ? "1st (Champions & Promoted)" : (goals >= 8 ? "2nd (Automatic Promotion)" : "4th (Playoff Contender)");
            string trophy = goals >= 15 ? $"{div} Champions Trophy 🏆" : (goals >= 8 ? $"{div} Promotion Medal 🥈" : "Top Scorer Award 👟");

            int wages = _currentSave.WeeklyWage * 38;
            int expenses = _currentSave.LifestyleTier * 120 * 38;
            int net = wages - expenses;

            return new SeasonSummarySnapshot(
                season: season,
                clubName: club,
                division: div,
                leaguePosition: pos,
                totalAppearances: apps,
                totalGoals: goals,
                totalAssists: assists,
                averageRating: rating,
                totalWagesEarned: wages,
                totalExpenses: expenses,
                netSavings: net,
                trophyAchievement: trophy);
        }

        public AttributeGrowthSnapshot GetAttributeGrowthData()
        {
            int ovr = _currentSave != null ? _currentSave.OverallRating : 62;
            int startOvr = Math.Max(50, ovr - 3);
            int age = 18 + (_currentSave?.CurrentSeason ?? 1) - 1;
            string phase = age < 23 ? "⚡ Rapid Youth Development (2.0x Multiplier)" : "📈 Peak Development Plateau";

            return new AttributeGrowthSnapshot(
                startOvr: startOvr,
                endOvr: ovr,
                potentialRating: 78,
                age: age,
                phase: phase,
                paceDelta: 2,
                shootingDelta: 3,
                passingDelta: 2,
                dribblingDelta: 2,
                staminaDelta: 1,
                visionDelta: 2);
        }

        public IReadOnlyList<TransferOfferSnapshot> GetTransferOffers()
        {
            string currentClub = _currentSave != null && !string.IsNullOrEmpty(_currentSave.ClubName)
                ? _currentSave.ClubName
                : "Northfield Town";
            int curWage = _currentSave != null ? _currentSave.WeeklyWage : 1000;

            return new List<TransferOfferSnapshot>
            {
                new TransferOfferSnapshot(
                    offerId: "renewal_1",
                    clubName: currentClub,
                    division: "Division 3 (Promoted)",
                    offeredWeeklyWage: (int)(curWage * 1.5m),
                    signingBonus: 10000,
                    transferFee: 0,
                    squadRole: "Key Player / Star",
                    prestigeStars: 3,
                    isRenewal: true),
                new TransferOfferSnapshot(
                    offerId: "transfer_1",
                    clubName: "Southport Athletic",
                    division: "League Two",
                    offeredWeeklyWage: (int)(curWage * 2.2m),
                    signingBonus: 25000,
                    transferFee: 450000,
                    squadRole: "First Team Regular",
                    prestigeStars: 3,
                    isRenewal: false),
                new TransferOfferSnapshot(
                    offerId: "transfer_2",
                    clubName: "Bristol Rovers",
                    division: "League One",
                    offeredWeeklyWage: (int)(curWage * 3.5m),
                    signingBonus: 50000,
                    transferFee: 1200000,
                    squadRole: "Rotation / High Prospect",
                    prestigeStars: 4,
                    isRenewal: false)
            };
        }

        public void AcceptTransferOffer(TransferOfferSnapshot offer)
        {
            if (_currentSave == null) return;

            _currentSave.ClubName = offer.ClubName;
            _currentSave.WeeklyWage = offer.OfferedWeeklyWage;
            _currentSave.SquadRole = offer.SquadRole;
            _currentSave.ContractEndYear = 2026 + _currentSave.CurrentSeason + 2;
            _currentSave.BankBalance += offer.SigningBonus;

            if (offer.IsRenewal)
            {
                _currentSave.ManagerTrust = Math.Min(100, _currentSave.ManagerTrust + 15);
            }
            else
            {
                _currentSave.ManagerTrust = 55; // Fresh clean slate at new club
            }

            string log = offer.IsRenewal
                ? $"Signed contract extension with {offer.ClubName}! New wage: £{offer.OfferedWeeklyWage:N0}/wk."
                : $"Transferred to {offer.ClubName}! Wage: £{offer.OfferedWeeklyWage:N0}/wk. Bonus: £{offer.SigningBonus:N0}.";
            OnStatusLog?.Invoke(log);

            AutoSave();
            PublishDaySnapshot(log);
        }

        public void AdvanceToNextSeason()
        {
            if (_currentSave == null) return;

            _currentSave.CurrentSeason++;
            _currentSave.CurrentWeek = 1;
            _currentDayOfWeek = 1;
            _currentSave.Energy = 100; // Fresh pre-season condition
            _currentSave.Form = 65;

            string log = $"Season {_currentSave.CurrentSeason} has begun! Welcome back to training.";
            OnSeasonAdvanced?.Invoke(_currentSave.CurrentSeason);
            OnWeekAdvanced?.Invoke(1);
            OnStatusLog?.Invoke(log);

            AutoSave();
            PublishDaySnapshot(log);
        }
    }
}
