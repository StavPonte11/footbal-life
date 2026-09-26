#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;
using FootballLife.Core.Audio;
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
        public event Action<string>? OnInternationalCallUp;
        public event Action<string, string>? OnManagerChanged;
        public event Action<string>? OnSponsorshipSigned;
        public event Action<RetirementDecision>? OnPlayerRetired;
        public event Action<HallOfFameEntry>? OnHallOfFameInducted;

        // Localization & Onboarding events (#P6-005, #P6-007)
        public event Action<GameLanguage>? OnLanguageChanged;
        public event Action<OnboardingState>? OnTutorialStateChanged;

        // Telemetry, Cloud Save & Monetization events (#P6-003, #P6-006, #P6-008)
        public event Action<TelemetryEvent>? OnTelemetryEventTracked;
        public event Action<CloudSyncResult>? OnCloudSyncCompleted;
        public event Action<PurchaseResult>? OnMonetizationPurchased;

        private readonly SponsorshipSystem _sponsorshipSystem = new();
        private readonly RetirementSystem _retirementSystem = new();
        private readonly LegacySystem _legacySystem = new();
        private readonly LocalizationService _localizationService = LocalizationService.Instance;
        private readonly OnboardingSystem _onboardingSystem = new();
        private readonly TelemetryService _telemetryService = TelemetryService.Instance;
        private readonly CloudSaveSyncService _cloudSaveSyncService = new();
        private readonly ICloudSaveProvider _cloudSaveProvider = new EmulatedCloudSaveProvider();
        private readonly MonetizationService _monetizationService = new();
        private readonly CrashDiagnosticService _crashDiagnosticService;

        private OnboardingState _onboardingState = OnboardingState.Initial;

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

            _localizationService.OnLanguageChanged += lang => OnLanguageChanged?.Invoke(lang);
            TryLoadLocalizationCatalogs();

            // Initialize crash diagnostics (#P7-702)
            string crashDir = System.IO.Path.Combine(Application.persistentDataPath, "crash_reports");
            _crashDiagnosticService = new CrashDiagnosticService(maxBreadcrumbs: 30, crashDirectory: crashDir);
            Application.logMessageReceivedThreaded += HandleUnityLogCallback;

            // Flush any offline crash reports from previous session
            int flushed = _crashDiagnosticService.FlushPendingCrashReports(report =>
            {
                _telemetryService.Track(TelemetryEventType.CrashReported, new Dictionary<string, object>
                {
                    ["crash_id"] = report.CrashId,
                    ["exception_type"] = report.ExceptionType,
                    ["is_fatal"] = report.IsFatal,
                    ["context"] = report.Context ?? "Unknown"
                });
            });
            if (flushed > 0)
            {
                Debug.Log($"[SimulationBridge] Flushed {flushed} pending crash report(s) from previous session.");
            }

            _telemetryService.Track(TelemetryEventType.SessionStart, new Dictionary<string, object>
            {
                ["platform"] = Application.platform.ToString(),
                ["unity_version"] = Application.unityVersion
            });
        }

        private void OnDestroy()
        {
            Application.logMessageReceivedThreaded -= HandleUnityLogCallback;
        }

        /// <summary>
        /// Unity log callback routed to CrashDiagnosticService for unhandled exceptions
        /// and error-level log messages (#P7-702).
        /// </summary>
        private void HandleUnityLogCallback(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                var report = _crashDiagnosticService.CaptureException(
                    new Exception(logString),
                    context: "UnityLogCallback",
                    isFatal: true,
                    deviceMemoryMb: SystemInfo.systemMemorySize,
                    batteryLevel: SystemInfo.batteryLevel);

                _telemetryService.Track(TelemetryEventType.CrashReported, new Dictionary<string, object>
                {
                    ["crash_id"] = report.CrashId,
                    ["exception_type"] = report.ExceptionType,
                    ["is_fatal"] = true,
                    ["context"] = "UnityLogCallback"
                });
            }
            else if (type == LogType.Error)
            {
                _crashDiagnosticService.AddBreadcrumb(
                    DiagnosticBreadcrumbCategory.System,
                    $"Unity Error: {logString}");
            }
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

            // Initialize rookie onboarding state (#P6-007)
            _onboardingState = OnboardingState.Initial;
            _onboardingSystem.SyncToSaveData(_onboardingState, _currentSave);

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

            _onboardingState = _onboardingSystem.LoadFromSaveData(_currentSave);
            if (!string.IsNullOrEmpty(_currentSave.PreferredLanguage))
            {
                _localizationService.SetLanguageByCode(_currentSave.PreferredLanguage);
            }

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
                _crashDiagnosticService.AddBreadcrumb(
                    DiagnosticBreadcrumbCategory.Persistence,
                    $"AutoSave S{_currentSave.CurrentSeason}W{_currentSave.CurrentWeek}");
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

            _crashDiagnosticService.AddBreadcrumb(
                DiagnosticBreadcrumbCategory.Simulation,
                $"AdvanceDay S{_currentSave.CurrentSeason}W{_currentSave.CurrentWeek}D{_currentDayOfWeek}");

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

                // Weekly finances: Wage deposit minus living expenses + Sponsorships
                int wage = _currentSave.WeeklyWage;
                int livingCost = _currentSave.LifestyleTier * 120;
                int sponsorshipIncome = ProcessWeeklySponsorshipPayouts();
                int netDeposit = wage - livingCost;
                _currentSave.BankBalance += netDeposit;
                _currentSave.LifetimeEarnings += Math.Max(0, wage);
                AudioManager.Instance?.PlayUIWageChime();

                // Energy bonus from active sponsorships
                int energyBonus = _sponsorshipSystem.CalculateTotalWeeklyEnergyBonus(GetActiveSponsorships());
                if (energyBonus > 0)
                {
                    _currentSave.Energy = Math.Min(100, _currentSave.Energy + energyBonus);
                }

                status = $"Week {_currentSave.CurrentWeek} complete! Net income: £{(netDeposit + sponsorshipIncome):N0}.";
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

            _crashDiagnosticService.AddBreadcrumb(
                DiagnosticBreadcrumbCategory.Match,
                $"MatchEnd {outcomeStr} {homeScore}-{awayScore} Rating:{matchRating:F2}");

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

        public SocialActivityResult ExecuteSocialActivity(SocialActivity activity, bool nearMatchday = false)
        {
            if (_currentSave == null)
            {
                return new SocialActivityResult { Success = false, Message = "No active career." };
            }

            var date = new DateOnly(2026, 9, 25);
            var result = SocialActivitySystem.ExecuteActivity(_currentSave, activity, date, nearMatchday);

            if (result.Success)
            {
                OnStatusLog?.Invoke(result.Message);
                PublishDaySnapshot(result.Message);
                AutoSave();
            }

            return result;
        }

        public PressConference GetPendingPressConference(MatchResult? recentMatch = null)
        {
            if (_currentSave == null)
            {
                EnsureMockSaveForTesting();
            }

            return PressConferenceSystem.GeneratePressConference(_currentSave!, recentMatch);
        }

        public PressAnswerResult AnswerPressQuestion(PressQuestion question, PressResponseChoice choice)
        {
            if (_currentSave == null)
            {
                return new PressAnswerResult { Success = false, Message = "No active career." };
            }

            var result = PressConferenceSystem.AnswerQuestion(_currentSave, question, choice);
            if (result.Success)
            {
                OnStatusLog?.Invoke(result.Message);
                PublishDaySnapshot(result.Message);
                AutoSave();
            }

            return result;
        }

        public List<MediaArticle> GetMediaArticles(MatchResult? recentMatch = null)
        {
            if (_currentSave == null)
            {
                EnsureMockSaveForTesting();
            }

            return MediaFeedSystem.GenerateArticles(_currentSave!, recentMatch);
        }

        private WorldState? _world;

        public WorldState GetOrCreateWorld()
        {
            if (_world != null) return _world;

            var season = new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()), Array.Empty<Matchday>(), 1);
            var world = WorldState.CreateEmpty(season);

            var l1 = League.Create("Premier League", "ENG", 1, 20, 38, 0, 3);
            var l2 = League.Create("Championship", "ENG", 2, 24, 46, 3, 3);
            var l3 = League.Create("League One", "ENG", 3, 24, 46, 3, 4);
            var l4 = League.Create("League Two", "ENG", 4, 24, 46, 4, 2);

            world = world.WithLeague(l1).WithLeague(l2).WithLeague(l3).WithLeague(l4);

            var clubs = new List<Club>
            {
                Club.Create("North London Red", "NLR", l1.Id, 86, new ClubFinances(250000000m, 180000m), 5, TacticalIdentity.Possession),
                Club.Create("Eastland City", "EAC", l1.Id, 89, new ClubFinances(300000000m, 220000m), 5, TacticalIdentity.Possession),
                Club.Create("West London Blue", "WLB", l1.Id, 82, new ClubFinances(200000000m, 160000m), 4, TacticalIdentity.HighPress),
                Club.Create("Birmingham Claret", "BMC", l1.Id, 78, new ClubFinances(120000000m, 95000m), 4, TacticalIdentity.HighPress),

                Club.Create("Yorkshire White", "YKW", l2.Id, 66, new ClubFinances(40000000m, 45000m), 3, TacticalIdentity.HighPress),
                Club.Create("South Yorkshire Red", "SYR", l2.Id, 63, new ClubFinances(35000000m, 40000m), 3, TacticalIdentity.Direct),
                Club.Create("Avon City", "AVC", l2.Id, 56, new ClubFinances(20000000m, 28000m), 3, TacticalIdentity.Possession),
                Club.Create("Northfield Town", "NOR", l2.Id, 54, new ClubFinances(18000000m, 24000m), 2, TacticalIdentity.Counter),

                Club.Create("South Coast Blues", "SCB", l3.Id, 50, new ClubFinances(12000000m, 14000m), 2, TacticalIdentity.Direct),
                Club.Create("Derbyshire Rams", "DBR", l3.Id, 48, new ClubFinances(10000000m, 12000m), 2, TacticalIdentity.Possession),
                Club.Create("Lancashire White", "LNW", l3.Id, 46, new ClubFinances(8000000m, 9500m), 2, TacticalIdentity.HighPress),
                Club.Create("Thames Valley Royals", "TVR", l3.Id, 44, new ClubFinances(7000000m, 8000m), 2, TacticalIdentity.Counter),

                Club.Create("Red Dragons", "RDG", l4.Id, 42, new ClubFinances(6000000m, 7000m), 2, TacticalIdentity.Direct),
                Club.Create("Cheshire Town", "CHT", l4.Id, 40, new ClubFinances(4500000m, 5000m), 1, TacticalIdentity.Possession),
                Club.Create("Riverway FC", "RWY", l4.Id, 37, new ClubFinances(3500000m, 3800m), 1, TacticalIdentity.HighPress),
                Club.Create("Crown Valley", "CRV", l4.Id, 34, new ClubFinances(2500000m, 2500m), 1, TacticalIdentity.Counter)
            };

            foreach (var club in clubs)
            {
                world = world.WithClub(club);
            }

            world = WorldSimulationSystem.ReplenishSquads(world, _simRandom);
            _world = world;
            return _world;
        }

        public TransferBiddingWar GetTransferBiddingWar(bool isTransferRequested = false)
        {
            if (_currentSave == null) EnsureMockSaveForTesting();

            var world = GetOrCreateWorld();
            var pos = Enum.TryParse<Position>(_currentSave!.PrimaryPosition, out var p) ? p : Position.ST;
            var player = new Player(_currentSave.PlayerId, _currentSave.PlayerName, _currentSave.Nationality, new DateOnly(2005, 1, 1), Foot.Right, pos);

            var abilities = new PlayerAbilities(
                pace: _currentSave.Pace,
                acceleration: _currentSave.Acceleration,
                stamina: _currentSave.Stamina,
                strength: _currentSave.Strength,
                agility: _currentSave.Agility,
                passing: _currentSave.Passing,
                shooting: _currentSave.Shooting,
                dribbling: _currentSave.Dribbling,
                crossing: _currentSave.Crossing,
                firstTouch: _currentSave.FirstTouch,
                tackling: _currentSave.Tackling,
                vision: _currentSave.Vision,
                composure: _currentSave.Composure,
                positioning: _currentSave.Positioning,
                decisionMaking: _currentSave.DecisionMaking);

            var currentClub = world.Clubs.Values.FirstOrDefault(c => c.Name.Equals(_currentSave.ClubName, StringComparison.OrdinalIgnoreCase))
                ?? world.Clubs.Values.First();

            var careerState = new PlayerCareerState(
                clubId: currentClub.Id,
                status: SquadStatus.Starter,
                managerTrust: _currentSave.ManagerTrust,
                weeklySalary: _currentSave.WeeklyWage,
                marketValue: _currentSave.MarketValue > 0 ? _currentSave.MarketValue : 1500000m,
                reputation: _currentSave.OverallRating * 0.7f);

            var date = new DateOnly(2026, 8, 15);
            return TransferMarketSystem.GenerateBiddingWar(player, abilities, careerState, world, _simRandom, date, _currentSave.CurrentWeek, isTransferRequested);
        }

        public TransferRequestResult RequestTransferListing(string reason)
        {
            if (_currentSave == null) EnsureMockSaveForTesting();

            var world = GetOrCreateWorld();
            var currentClub = world.Clubs.Values.FirstOrDefault(c => c.Name.Equals(_currentSave!.ClubName, StringComparison.OrdinalIgnoreCase))
                ?? world.Clubs.Values.First();

            var pos = Enum.TryParse<Position>(_currentSave!.PrimaryPosition, out var p) ? p : Position.ST;
            var player = new Player(_currentSave.PlayerId, _currentSave.PlayerName, _currentSave.Nationality, new DateOnly(2005, 1, 1), Foot.Right, pos);

            var careerState = new PlayerCareerState(
                clubId: currentClub.Id,
                status: _currentSave.ManagerTrust > 60 ? SquadStatus.KeyPlayer : SquadStatus.Starter,
                managerTrust: _currentSave.ManagerTrust,
                weeklySalary: _currentSave.WeeklyWage,
                marketValue: _currentSave.MarketValue > 0 ? _currentSave.MarketValue : 1500000m,
                reputation: _currentSave.OverallRating * 0.7f);

            var date = new DateOnly(2026, 8, 15);
            var result = TransferMarketSystem.RequestTransferListing(player, careerState, currentClub, reason, _simRandom, date);

            _currentSave.ManagerTrust = Math.Max(0, _currentSave.ManagerTrust + (int)result.ManagerTrustDelta);
            OnStatusLog?.Invoke(result.ManagerResponse);
            AutoSave();

            return result;
        }

        public bool AcceptTransferBid(ClubBid bid)
        {
            if (_currentSave == null || bid == null) return false;

            _currentSave.ClubName = bid.BiddingClubName;
            _currentSave.WeeklyWage = (int)bid.OfferedWeeklyWage;
            _currentSave.BankBalance += (int)bid.SigningBonus;
            _currentSave.MarketValue = (int)bid.TransferFee;
            _currentSave.ManagerTrust = 65; // Fresh start with new manager

            string msg = $"Transferred to {bid.BiddingClubName}! Wage: £{bid.OfferedWeeklyWage:N0}/wk. Signing bonus £{bid.SigningBonus:N0} received.";
            OnStatusLog?.Invoke(msg);
            PublishDaySnapshot(msg);
            AutoSave();

            return true;
        }

        public WorldSeasonResolution AdvanceSeasonWithWorldProgression()
        {
            var world = GetOrCreateWorld();
            int seasonYear = _currentSave?.CurrentSeason ?? 1;

            var (worldWithResolvedLeagues, resolution) = WorldSimulationSystem.ResolveLeagueSeason(world, seasonYear, _simRandom);
            var (finalWorld, retiredCount) = WorldSimulationSystem.AgeAndDevelopNpcPlayers(worldWithResolvedLeagues, _currentSave?.PlayerId, new DateOnly(2027, 6, 1), _simRandom);
            finalWorld = WorldSimulationSystem.ReplenishSquads(finalWorld, _simRandom);

            _world = finalWorld;

            string msg = $"Season {seasonYear} concluded! {resolution.PromotedClubIds.Count} clubs promoted, {retiredCount} veterans retired.";
            OnStatusLog?.Invoke(msg);
            PublishDaySnapshot(msg);
            AutoSave();

            return resolution;
        }

        public (int Caps, int Goals, int Assists, string NationalTeamName) GetPlayerInternationalStats()
        {
            if (_currentSave == null) EnsureMockSaveForTesting();
            return (_currentSave!.InternationalCaps, _currentSave.InternationalGoals, _currentSave.InternationalAssists, _currentSave.Nationality);
        }

        public bool ProcessInternationalWindow(string tournamentName = "World Cup Qualifier")
        {
            if (_currentSave == null) EnsureMockSaveForTesting();

            var world = GetOrCreateWorld();
            var nationality = _currentSave!.Nationality;

            var nationalTeam = world.NationalTeams.Values.FirstOrDefault(t =>
                string.Equals(t.CountryName, nationality, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.CountryCode, nationality, StringComparison.OrdinalIgnoreCase));

            if (nationalTeam == null)
            {
                string code = nationality.Length >= 3 ? nationality.Substring(0, 3).ToUpperInvariant() : "NAT";
                nationalTeam = NationalTeam.Create(nationality, code, fifaRanking: 15, confederation: "UEFA", managerName: $"{nationality} Coach");
                world = world.WithNationalTeam(nationalTeam);
                _world = world;
            }

            var pos = Enum.TryParse<Position>(_currentSave.PrimaryPosition, out var p) ? p : Position.ST;
            var player = new Player(_currentSave.PlayerId, _currentSave.PlayerName, _currentSave.Nationality, new DateOnly(2005, 1, 1), Foot.Right, pos);

            if (!world.Players.ContainsKey(player.Id))
            {
                var abilities = PlayerAbilities.CreateUniform((byte)_currentSave.OverallRating);
                var state = PlayerState.Default with { Form = _currentSave.Form, Fatigue = 100 - _currentSave.Energy, Confidence = 60f };
                var currentClub = world.Clubs.Values.FirstOrDefault(c => c.Name.Equals(_currentSave.ClubName, StringComparison.OrdinalIgnoreCase)) ?? world.Clubs.Values.First();
                var career = new PlayerCareerState(currentClub.Id, SquadStatus.Starter, _currentSave.ManagerTrust, _currentSave.WeeklyWage, _currentSave.MarketValue, _currentSave.OverallRating);
                world = world.WithPlayer(player, abilities, state, career);
                _world = world;
            }

            var (updatedWorld, callUp) = InternationalSystem.ProcessPlayerCallUp(
                player.Id, nationalTeam.Id, tournamentName, new DateOnly(2026, 9, 1), world, _simRandom);

            _world = updatedWorld;

            if (callUp.Status == CallUpStatus.CalledUp)
            {
                _currentSave.InternationalCaps += callUp.CapsEarned;
                _currentSave.InternationalGoals += callUp.GoalsScored;
                _currentSave.Energy = Math.Max(10, _currentSave.Energy - 20);

                string msg = $"Called up for {nationalTeam.CountryName}! Earned {callUp.CapsEarned} cap, scored {callUp.GoalsScored} goals in {tournamentName}.";
                OnInternationalCallUp?.Invoke(msg);
                OnStatusLog?.Invoke(msg);
                PublishDaySnapshot(msg);
                AutoSave();
                return true;
            }

            return false;
        }

        public ContinentalCompetition? GetActiveContinentalCompetition()
        {
            var world = GetOrCreateWorld();
            return world.ActiveContinentalCompetition;
        }

        public (WorldSeasonResolution SeasonResolution, ContinentalCompetition? ContinentalTournament) AdvanceSeasonWithContinental()
        {
            var seasonResolution = AdvanceSeasonWithWorldProgression();
            var world = GetOrCreateWorld();

            var qualifiedClubs = ContinentalCompetitionSystem.QualifyClubs(
                world, seasonResolution.LeagueResolutions, slotsPerTopLeague: 4, totalSlots: 32);

            var (finalWorld, completedTournament) = ContinentalCompetitionSystem.SimulateFullTournament(
                qualifiedClubs, world, _simRandom, "Champions Cup");

            _world = finalWorld;

            string winnerName = finalWorld.Clubs.TryGetValue(completedTournament.WinnerId ?? Guid.Empty, out var wClub)
                ? wClub.Name
                : "Unknown";

            string msg = $"Champions Cup concluded! Winner: {winnerName} (£20,000,000 prize awarded).";
            OnStatusLog?.Invoke(msg);
            PublishDaySnapshot(msg);
            AutoSave();

            return (seasonResolution, completedTournament);
        }

        public ManagerChangeEvent? CheckAndProcessManagerChange(ClubSeasonOutcome outcome)
        {
            if (_currentSave == null) EnsureMockSaveForTesting();
            var world = GetOrCreateWorld();

            var currentClub = world.Clubs.Values.FirstOrDefault(c => c.Name.Equals(_currentSave!.ClubName, StringComparison.OrdinalIgnoreCase));
            if (currentClub == null) return null;

            float sackProb = ManagerChangeSystem.EvaluateManagerPerformance(currentClub, outcome, world);

            if (_simRandom.NextFloat(0f, 1f) < sackProb)
            {
                var (updatedWorld, changeEvent) = ManagerChangeSystem.TriggerManagerChange(
                    currentClub, ManagerChangeReason.Sacked, world, _simRandom, new DateOnly(2027, 6, 15), _currentSave.PlayerId);

                _world = updatedWorld;
                _currentSave.ManagerTrust = 50;

                var newManager = updatedWorld.Managers[changeEvent.NewManagerId];
                string msg = $"New Manager appointed at {currentClub.Name}: {newManager.Name}! Tactical philosophy: {newManager.TacticalStyle}. Manager trust reset to 50.";
                OnManagerChanged?.Invoke(currentClub.Name, newManager.Name);
                OnStatusLog?.Invoke(msg);
                PublishDaySnapshot(msg);
                AutoSave();

                return changeEvent;
            }

            return null;
        }

        // ─── Milestone 5.3: Endorsements, Retirement & Legacy ─────────────────

        public IReadOnlyList<SponsorshipDeal> GetAvailableSponsorshipOffers()
        {
            if (_currentSave == null) EnsureMockSaveForTesting();
            var world = GetOrCreateWorld();
            return _sponsorshipSystem.GetAvailableOffers(_currentSave!.OverallRating, world.ActiveSponsorships, _simRandom);
        }

        public IReadOnlyList<ActiveSponsorship> GetActiveSponsorships()
        {
            var world = GetOrCreateWorld();
            return world.ActiveSponsorships;
        }

        public (bool Success, string Message) SignSponsorshipDeal(SponsorshipDeal deal)
        {
            if (_currentSave == null) EnsureMockSaveForTesting();
            var world = GetOrCreateWorld();

            var (activeDeal, signingBonus, error) = _sponsorshipSystem.AcceptDeal(
                deal, _currentSave!.OverallRating, world.ActiveSponsorships);

            if (error != null)
            {
                return (false, error);
            }

            _world = world.WithAddedSponsorship(activeDeal!);
            _currentSave.BankBalance += signingBonus;
            if (!_currentSave.ActiveSponsorshipIds.Contains(deal.Id))
            {
                _currentSave.ActiveSponsorshipIds.Add(deal.Id);
            }

            string msg = $"Signed commercial endorsement with {deal.BrandName}! Received signing bonus £{signingBonus:N0}. Weekly payout: £{deal.WeeklyPayout:N0}/wk.";
            OnSponsorshipSigned?.Invoke(msg);
            OnStatusLog?.Invoke(msg);
            PublishDaySnapshot(msg);
            AutoSave();

            return (true, msg);
        }

        public int ProcessWeeklySponsorshipPayouts()
        {
            if (_currentSave == null) return 0;
            var world = GetOrCreateWorld();

            var (totalPayout, updated, expired) = _sponsorshipSystem.ProcessWeeklyPayouts(world.ActiveSponsorships);
            _world = world.WithActiveSponsorships(updated);

            if (totalPayout > 0)
            {
                _currentSave.BankBalance += totalPayout;
                _currentSave.LifetimeEarnings += totalPayout;
            }

            foreach (var exp in expired)
            {
                OnStatusLog?.Invoke($"Commercial endorsement with {exp.BrandName} has completed.");
            }

            return totalPayout;
        }

        public bool IsEligibleForRetirement()
        {
            int age = 20 + (_currentSave?.CurrentSeason ?? 1);
            return _retirementSystem.IsEligibleForRetirement(age);
        }

        public RetirementDecision? GetPlayerRetirementDecision()
        {
            var world = GetOrCreateWorld();
            return world.PlayerRetirement;
        }

        public RetirementDecision RetirePlayer(RetirementReason reason, PostPlayingRole chosenRole)
        {
            if (_currentSave == null) EnsureMockSaveForTesting();
            var world = GetOrCreateWorld();

            int age = 20 + _currentSave!.CurrentSeason;
            var pos = Enum.TryParse<Position>(_currentSave.PrimaryPosition, out var p) ? p : Position.ST;
            var player = new Player(_currentSave.PlayerId, _currentSave.PlayerName, _currentSave.Nationality, new DateOnly(2005, 1, 1), Foot.Right, pos);

            var decision = _retirementSystem.RetirePlayer(player, _currentSave.CurrentSeason, age, reason, chosenRole);
            _currentSave.IsRetired = true;
            _currentSave.RetirementAge = age;
            _currentSave.RetirementSeason = _currentSave.CurrentSeason;
            _currentSave.RetirementReason = reason.ToString();
            _currentSave.PostPlayingRole = chosenRole.ToString();

            // Evaluate Legacy upon retirement
            var legacy = CalculateCurrentCareerLegacy();
            _world = world.WithPlayerRetirement(decision).WithPlayerLegacy(legacy);

            if (legacy.IsHallOfFameInductee)
            {
                var entry = _legacySystem.InductIntoHallOfFame(player, legacy, chosenRole, 2026 + _currentSave.CurrentSeason);
                _world = _world.WithHallOfFameEntry(entry);
                OnHallOfFameInducted?.Invoke(entry);
            }

            OnPlayerRetired?.Invoke(decision);
            OnStatusLog?.Invoke(decision.Statement);
            PublishDaySnapshot(decision.Statement);
            AutoSave();

            return decision;
        }

        public CareerLegacy CalculateCurrentCareerLegacy()
        {
            if (_currentSave == null) EnsureMockSaveForTesting();

            int apps = _currentSave!.TotalAppearances;
            int goals = _currentSave.TotalGoals;
            int assists = _currentSave.TotalAssists;
            int caps = _currentSave.InternationalCaps;
            int intGoals = _currentSave.InternationalGoals;
            int leagueTitles = _currentSave.TotalTrophies;
            int contTitles = 0;
            int cups = 0;
            int intTrophies = 0;
            int peakOvr = _currentSave.OverallRating;
            long earnings = _currentSave.LifetimeEarnings > 0 ? _currentSave.LifetimeEarnings : _currentSave.BankBalance;

            int score = _legacySystem.CalculateCareerScore(
                apps, goals, assists, cleanSheets: 0, caps, intGoals,
                leagueTitles, contTitles, cups, intTrophies, peakOvr, earnings);

            var grade = _legacySystem.DetermineGrade(score);

            var legacy = new CareerLegacy(
                apps, goals, assists, lifetimeCleanSheets: 0, caps, intGoals,
                leagueTitles, contTitles, cups, intTrophies, earnings,
                peakOvr, _currentSave.CurrentSeason, score, grade,
                isHallOfFameInductee: false);

            bool hof = _legacySystem.IsEligibleForHallOfFame(legacy);
            legacy = legacy with { IsHallOfFameInductee = hof };

            _currentSave.CareerScore = score;
            _currentSave.LegacyGrade = grade.ToString();
            _currentSave.IsHallOfFameInductee = hof;

            return legacy;
        }

        public IReadOnlyList<HallOfFameEntry> GetHallOfFameEntries()
        {
            var world = GetOrCreateWorld();
            return world.HallOfFame;
        }

        // ── Localization & Onboarding API (#P6-005, #P6-007) ───────────────────
        public LocalizationService Localization => _localizationService;
        public OnboardingSystem Onboarding => _onboardingSystem;
        public OnboardingState OnboardingState => _onboardingState;

        /// <summary>
        /// Translates a key using active language with optional format arguments.
        /// </summary>
        public string T(string key, params object[] args) => _localizationService.GetText(key, args);

        /// <summary>
        /// Sets active game language and saves preference into active profile.
        /// </summary>
        public void SetLanguage(GameLanguage lang)
        {
            _localizationService.CurrentLanguage = lang;
            if (_currentSave != null)
            {
                _currentSave.PreferredLanguage = LanguageInfo.FromLanguage(lang).Code;
                AutoSave();
            }
        }

        /// <summary>
        /// Advances the onboarding tutorial by marking the given step completed.
        /// </summary>
        public void AdvanceTutorialStep(OnboardingStep step)
        {
            _onboardingState = _onboardingSystem.AdvanceStep(_onboardingState, step);
            if (_currentSave != null)
            {
                _onboardingSystem.SyncToSaveData(_onboardingState, _currentSave);
                var def = _onboardingSystem.GetStepDefinition(step);
                int energy = _currentSave.Energy;
                int form = _currentSave.Form;
                int trust = _currentSave.ManagerTrust;
                int balance = _currentSave.BankBalance;
                _onboardingSystem.ApplyReward(def.Reward, ref energy, ref form, ref trust, ref balance);
                _currentSave.Energy = energy;
                _currentSave.Form = form;
                _currentSave.ManagerTrust = trust;
                _currentSave.BankBalance = balance;
                AutoSave();
            }
            OnTutorialStateChanged?.Invoke(_onboardingState);
        }

        /// <summary>
        /// Skips the rookie tutorial flow and unlocks all game features immediately.
        /// </summary>
        public void SkipTutorial()
        {
            _onboardingState = _onboardingSystem.SkipTutorial(_onboardingState);
            if (_currentSave != null)
            {
                _onboardingSystem.SyncToSaveData(_onboardingState, _currentSave);
                AutoSave();
            }
            OnTutorialStateChanged?.Invoke(_onboardingState);
        }

        /// <summary>
        /// Checks whether a feature is unlocked under the current onboarding state.
        /// </summary>
        public bool IsFeatureUnlocked(string featureKey)
        {
            return _onboardingSystem.IsFeatureUnlocked(_onboardingState, featureKey);
        }

        /// <summary>
        /// Replays the FTUE tutorial from the beginning (#P7-601).
        /// Resets the onboarding state to the Welcome step, persists to save data,
        /// and fires OnTutorialStateChanged so the UI re-presents the first step.
        /// </summary>
        public void ReplayTutorial()
        {
            _onboardingState = _onboardingSystem.ResetForReplay();
            if (_currentSave != null)
            {
                _onboardingSystem.SyncToSaveData(_onboardingState, _currentSave);
                AutoSave();
            }
            OnTutorialStateChanged?.Invoke(_onboardingState);
        }


        // ── Telemetry, Cloud Save & Monetization API (#P6-003, #P6-006, #P6-008) ──
        public TelemetryService Telemetry => _telemetryService;
        public CloudSaveSyncService CloudSync => _cloudSaveSyncService;
        public ICloudSaveProvider CloudProvider => _cloudSaveProvider;
        public MonetizationService Monetization => _monetizationService;

        public void TrackTelemetry(TelemetryEventType type, Dictionary<string, object>? parameters = null)
        {
            _telemetryService.Track(type, parameters);
            OnTelemetryEventTracked?.Invoke(TelemetryEvent.Create(type, _telemetryService.SessionId, parameters));
        }

        public async System.Threading.Tasks.Task<CloudSyncResult> SyncCloudSaveAsync(int slotIndex = 0, ConflictResolutionStrategy strategy = ConflictResolutionStrategy.KeepNewest)
        {
            if (_currentSave == null) EnsureMockSaveForTesting();
            string slotKey = $"slot_{slotIndex}";
            string localJson = _currentSave!.ToJson();
            var result = await _cloudSaveSyncService.SyncSlotAsync(slotKey, localJson, _currentSave, _cloudSaveProvider, strategy);
            if (result.Status == CloudSyncStatus.ConflictResolved && result.ResolvedJson != null && result.ResolvedJson != localJson)
            {
                _currentSave = CareerSaveData.FromJson(result.ResolvedJson);
                SaveManager.SaveCareer(slotIndex, _currentSave);
                PublishDaySnapshot($"Cloud save synced (Conflict resolved: {result.Message})");
            }
            TrackTelemetry(TelemetryEventType.CloudSyncCompleted, new Dictionary<string, object>
            {
                ["slot"] = slotKey,
                ["status"] = result.Status.ToString()
            });
            OnCloudSyncCompleted?.Invoke(result);
            return result;
        }

        public PurchaseResult PurchaseProduct(string productId, string transactionId = "")
        {
            if (_currentSave == null) EnsureMockSaveForTesting();
            var result = _monetizationService.PurchaseProduct(productId, _currentSave!, transactionId);
            if (result.Status == PurchaseStatus.Success)
            {
                AutoSave();
                TrackTelemetry(TelemetryEventType.MonetizationPurchased, new Dictionary<string, object>
                {
                    ["product_id"] = productId,
                    ["price_usd"] = result.Product?.PriceUsd ?? 0m
                });
                PublishDaySnapshot($"Unlocked {result.Product?.Title}!");
            }
            OnMonetizationPurchased?.Invoke(result);
            return result;
        }

        public bool UseRewindToken()
        {
            if (_currentSave == null) return false;
            bool success = _monetizationService.ConsumeRewindToken(_currentSave);
            if (success)
            {
                AutoSave();
                PublishDaySnapshot($"Used Career Rewind Token ({_currentSave.CareerRewindTokens} remaining)");
                TrackTelemetry(TelemetryEventType.EconomyTransaction, new Dictionary<string, object>
                {
                    ["item"] = "career_rewind_token",
                    ["action"] = "consume",
                    ["remaining"] = _currentSave.CareerRewindTokens
                });
            }
            return success;
        }

        private void TryLoadLocalizationCatalogs()
        {
            try
            {
                string streamingDir = System.IO.Path.Combine(Application.streamingAssetsPath, "localization");
                if (System.IO.Directory.Exists(streamingDir))
                {
                    _localizationService.LoadFromDirectory(streamingDir);
                    return;
                }

                string contentDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../../content/data/localization"));
                if (System.IO.Directory.Exists(contentDir))
                {
                    _localizationService.LoadFromDirectory(contentDir);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SimulationBridge] Could not load external localization catalogs: {ex.Message}");
            }
        }

        private void EnsureMockSaveForTesting()
        {
            if (_currentSave == null)
            {
                StartNewCareer("Young Prospect", "England", Position.ST, Foot.Right, "Northfield Town");
            }
        }
    }
}
