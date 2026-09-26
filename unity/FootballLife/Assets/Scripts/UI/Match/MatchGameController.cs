#nullable enable
using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Match
{
    public sealed class MatchSummaryData
    {
        public string HomeClub { get; set; } = "Northfield Town";
        public string AwayClub { get; set; } = "Westford United";
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public int PlayerGoals { get; set; }
        public int PlayerAssists { get; set; }
        public int KeyActions { get; set; }
        public int Errors { get; set; }
        public double MatchRating { get; set; }
        public string Competition { get; set; } = "Division 4";
        public bool IsHome { get; set; } = true;
    }

    /// <summary>
    /// Presentation and interactive controller for MatchGameView.uxml (#P2-016).
    /// Manages match scoreboard, minute-by-minute situation progression, player decision-making,
    /// and pure simulation resolution via ActionResolver.
    /// </summary>
    public class MatchGameController
    {
        // ── Queried UI Elements ───────────────────────────────────────────────
        private readonly Label _labelHomeName;
        private readonly Label _labelAwayName;
        private readonly Label _labelHomeScore;
        private readonly Label _labelAwayScore;
        private readonly Label _labelClock;
        private readonly Label _labelCommentary;

        private readonly VisualElement _cardSituation;
        private readonly Label _labelSituationTitle;
        private readonly Label _labelSituationMinute;
        private readonly Label _labelSituationNarrative;
        private readonly Label _labelSituationPressure;
        private readonly Label _labelSituationAdvantage;

        private readonly Button? _btnChoice1;
        private readonly Button? _btnChoice2;
        private readonly Button? _btnChoice3;

        private readonly VisualElement _cardResolution;
        private readonly Label _labelResolutionOutcome;
        private readonly Label _labelResolutionNarrative;
        private readonly Label _labelResolutionRatingDelta;
        private readonly Label _labelResolutionConfidenceDelta;
        private readonly Button? _btnNextMoment;

        private readonly Label _valLiveRating;
        private readonly Label _valLiveGoals;
        private readonly Label _valLiveAssists;

        // ── State & Simulation ────────────────────────────────────────────────
        private readonly Action<MatchSummaryData> _onMatchComplete;
        private readonly SimulationRandom _rng = new SimulationRandom(12345);

        private CareerSaveData? _save;
        private MatchOpportunitySnapshot? _opp;
        private PlayerAbilities _abilities = PlayerAbilities.Default;
        private PlayerState _playerState = PlayerState.Default;

        private int _situationIndex;
        private readonly int[] _situationMinutes = { 18, 42, 67, 85 };
        private MatchSituation? _currentSituation;

        private int _homeScore;
        private int _awayScore;
        private int _playerGoals;
        private int _playerAssists;
        private int _keyActions;
        private int _errors;
        private double _matchRating = 6.00;

        public MatchGameController(
            VisualElement root,
            Action<MatchSummaryData> onMatchComplete)
        {
            _onMatchComplete = onMatchComplete;

            _labelHomeName     = root.Q<Label>("label-game-home-name");
            _labelAwayName     = root.Q<Label>("label-game-away-name");
            _labelHomeScore    = root.Q<Label>("label-game-home-score");
            _labelAwayScore    = root.Q<Label>("label-game-away-score");
            _labelClock        = root.Q<Label>("label-game-clock");
            _labelCommentary   = root.Q<Label>("label-commentary-text");

            _cardSituation          = root.Q<VisualElement>("card-situation");
            _labelSituationTitle    = root.Q<Label>("label-situation-title");
            _labelSituationMinute   = root.Q<Label>("label-situation-minute");
            _labelSituationNarrative = root.Q<Label>("label-situation-narrative");
            _labelSituationPressure = root.Q<Label>("label-situation-pressure");
            _labelSituationAdvantage = root.Q<Label>("label-situation-advantage");

            _btnChoice1 = root.Q<Button>("btn-choice-1");
            _btnChoice2 = root.Q<Button>("btn-choice-2");
            _btnChoice3 = root.Q<Button>("btn-choice-3");

            _cardResolution                = root.Q<VisualElement>("card-resolution");
            _labelResolutionOutcome         = root.Q<Label>("label-resolution-outcome");
            _labelResolutionNarrative       = root.Q<Label>("label-resolution-narrative");
            _labelResolutionRatingDelta     = root.Q<Label>("label-resolution-rating-delta");
            _labelResolutionConfidenceDelta = root.Q<Label>("label-resolution-confidence-delta");
            _btnNextMoment                 = root.Q<Button>("btn-next-moment");

            _valLiveRating  = root.Q<Label>("val-live-rating");
            _valLiveGoals   = root.Q<Label>("val-live-goals");
            _valLiveAssists = root.Q<Label>("val-live-assists");

            if (_btnChoice1 != null)
                _btnChoice1.clicked += () => OnChoiceSelected(0);
            if (_btnChoice2 != null)
                _btnChoice2.clicked += () => OnChoiceSelected(1);
            if (_btnChoice3 != null)
                _btnChoice3.clicked += () => OnChoiceSelected(2);

            if (_btnNextMoment != null)
                _btnNextMoment.clicked += OnNextMoment;
        }

        public void StartMatch(MatchOpportunitySnapshot? opp, CareerSaveData? save)
        {
            _opp = opp;
            _save = save;

            // Initialize scores & stats
            _homeScore = 0;
            _awayScore = 0;
            _playerGoals = 0;
            _playerAssists = 0;
            _keyActions = 0;
            _errors = 0;
            _matchRating = 6.00;
            _situationIndex = 0;

            // Domain state mapping
            if (save != null)
            {
                _abilities = new PlayerAbilities(
                    pace: save.Pace,
                    acceleration: save.Acceleration,
                    stamina: save.Stamina,
                    strength: save.Strength,
                    agility: save.Agility,
                    passing: save.Passing,
                    shooting: save.Shooting,
                    dribbling: save.Dribbling,
                    crossing: save.Crossing,
                    firstTouch: save.FirstTouch,
                    tackling: save.Tackling,
                    vision: save.Vision,
                    composure: save.Composure,
                    positioning: save.Positioning,
                    decisionMaking: save.DecisionMaking);

                _playerState = new PlayerState(
                    fatigue: Math.Max(0, 100 - save.Energy),
                    confidence: 60f,
                    form: save.Form,
                    happiness: 60f,
                    motivation: 60f,
                    morale: save.Morale,
                    fitness: save.Energy);
            }

            string homeTeam = save != null && !string.IsNullOrEmpty(save.ClubName) ? save.ClubName : "Northfield Town";
            string awayTeam = opp.HasValue ? opp.Value.OpponentName : "Westford United";

            if (_labelHomeName != null) _labelHomeName.text = homeTeam;
            if (_labelAwayName != null) _labelAwayName.text = awayTeam;

            UpdateScoreboard(minute: 1);
            PresentSituation(_situationIndex);
        }

        private void PresentSituation(int index)
        {
            if (index >= _situationMinutes.Length)
            {
                FinishMatch();
                return;
            }

            if (index == 0)
            {
                Core.Audio.AudioManager.Instance?.StartCrowdMurmur();
            }
            Core.Audio.AudioManager.Instance?.PlayWhistle(Core.Audio.WhistleType.SituationStart);

            int minute = _situationMinutes[index];
            UpdateScoreboard(minute);

            // Hide resolution card, show situation card
            if (_cardResolution != null) _cardResolution.style.display = DisplayStyle.None;
            if (_cardSituation != null) _cardSituation.style.display = DisplayStyle.Flex;

            // Generate contextual situation based on minute and position
            switch (index)
            {
                case 0:
                    // Early counter-attack
                    _currentSituation = new MatchSituation(
                        SituationType.RunningInBehind,
                        opponentPressure: 4.2f,
                        expectedDifficulty: 0.35f,
                        positionalAdvantage: 0.60f,
                        new[]
                        {
                            new ActionChoice(MatchAction.Shot_Close, riskLevel: 0.35f, expectedValue: 0.70f),
                            new ActionChoice(MatchAction.ThroughBall, riskLevel: 0.20f, expectedValue: 0.60f),
                            new ActionChoice(MatchAction.Dribble, riskLevel: 0.50f, expectedValue: 0.80f)
                        });

                    SetSituationUI(
                        minute,
                        "⚽ Counter-Attack Breakthrough",
                        "You break past the defensive line into the channel. The keeper rushes off his line while your winger provides support on the left!",
                        "Moderate (4.2/10)",
                        "High (+0.60)",
                        "🎯 Finesse Shot (Risk: 35%)",
                        "👟 Square Pass to Teammate (Risk: 20%)",
                        "💨 Dribble Past Goalkeeper (Risk: 50%)");
                    break;

                case 1:
                    // Edge of the box combination
                    _currentSituation = new MatchSituation(
                        SituationType.ReceivingInBox,
                        opponentPressure: 6.5f,
                        expectedDifficulty: 0.45f,
                        positionalAdvantage: 0.30f,
                        new[]
                        {
                            new ActionChoice(MatchAction.Shot_Close, riskLevel: 0.45f, expectedValue: 0.65f),
                            new ActionChoice(MatchAction.ShortPass, riskLevel: 0.15f, expectedValue: 0.50f),
                            new ActionChoice(MatchAction.CutInside, riskLevel: 0.40f, expectedValue: 0.60f)
                        });

                    SetSituationUI(
                        minute,
                        "⚽ Crowded Penalty Box Opportunity",
                        "A cross deflects into your path at the penalty spot! Two defenders converge rapidly to close down your shooting angle.",
                        "High (6.5/10)",
                        "Moderate (+0.30)",
                        "💥 First-Time Volley (Risk: 45%)",
                        "👟 Lay-off Pass to Midfielder (Risk: 15%)",
                        "⚡ Cut Inside to Create Space (Risk: 40%)");
                    break;

                case 2:
                    // Second half long range / transition
                    _currentSituation = new MatchSituation(
                        SituationType.LongShot,
                        opponentPressure: 5.0f,
                        expectedDifficulty: 0.50f,
                        positionalAdvantage: 0.15f,
                        new[]
                        {
                            new ActionChoice(MatchAction.Shot_Long, riskLevel: 0.50f, expectedValue: 0.55f),
                            new ActionChoice(MatchAction.ThroughBall, riskLevel: 0.30f, expectedValue: 0.65f),
                            new ActionChoice(MatchAction.ShortPass, riskLevel: 0.15f, expectedValue: 0.40f)
                        });

                    SetSituationUI(
                        minute,
                        "⚽ Edge of Box Playmaking",
                        "The opponent defense drops deep to protect the lead. You have 3 yards of space outside the 18-yard box.",
                        "Moderate (5.0/10)",
                        "Even (+0.15)",
                        "🚀 Power Shot from Distance (Risk: 50%)",
                        "🎯 Weighted Through-Ball into Box (Risk: 30%)",
                        "🔄 Recycle Possession to Fullback (Risk: 15%)");
                    break;

                default:
                    // Late 85' crucial moment
                    _currentSituation = new MatchSituation(
                        SituationType.RunningInBehind,
                        opponentPressure: 7.0f,
                        expectedDifficulty: 0.55f,
                        positionalAdvantage: 0.45f,
                        new[]
                        {
                            new ActionChoice(MatchAction.Shot_Close, riskLevel: 0.40f, expectedValue: 0.75f),
                            new ActionChoice(MatchAction.Cross, riskLevel: 0.25f, expectedValue: 0.60f),
                            new ActionChoice(MatchAction.Dribble, riskLevel: 0.55f, expectedValue: 0.70f)
                        });

                    SetSituationUI(
                        minute,
                        "⚽ 85' Crucial Match Winner Opportunity",
                        "Injury time approaches! A loose header from the defender falls behind the backline. You surge into the penalty box!",
                        "Intense (7.0/10)",
                        "High (+0.45)",
                        "🔥 Chip the Advancing Keeper (Risk: 40%)",
                        "👟 Low Cross to Far Post (Risk: 25%)",
                        "💨 Round the Keeper (Risk: 55%)");
                    break;
            }

        }

        private void SetSituationUI(
            int minute,
            string title,
            string narrative,
            string pressure,
            string advantage,
            string c1Text,
            string c2Text,
            string c3Text)
        {
            if (_labelSituationMinute != null) _labelSituationMinute.text = $"{minute}&apos;";
            if (_labelSituationTitle != null) _labelSituationTitle.text = title;
            if (_labelSituationNarrative != null) _labelSituationNarrative.text = narrative;
            if (_labelSituationPressure != null) _labelSituationPressure.text = pressure;
            if (_labelSituationAdvantage != null) _labelSituationAdvantage.text = advantage;

            if (_btnChoice1 != null) _btnChoice1.text = c1Text;
            if (_btnChoice2 != null) _btnChoice2.text = c2Text;
            if (_btnChoice3 != null) _btnChoice3.text = c3Text;

            if (_labelCommentary != null)
                _labelCommentary.text = $"{minute}&apos; — {narrative}";
        }

        private void OnChoiceSelected(int choiceIdx)
        {
            if (_currentSituation == null || choiceIdx >= _currentSituation.AvailableChoices.Length)
                return;

            Core.Audio.AudioManager.Instance?.PlayUIClick();

            var choice = _currentSituation.AvailableChoices[choiceIdx];
            var outcome = ActionResolver.Resolve(choice.Action, _currentSituation, _abilities, _playerState, _rng);

            string outcomeHeader;
            string narrativeText;
            double ratingDelta;
            int confidenceDelta = (int)outcome.ConfidenceDelta;

            if (outcome.Success)
            {
                if (outcome.Type == OutcomeType.Goal || choice.Action == MatchAction.Shot_Close || choice.Action == MatchAction.Shot_Long)
                {
                    _playerGoals++;
                    _homeScore++;
                    ratingDelta = 0.85;
                    outcomeHeader = "GOAL SCORED! ⚽";
                    narrativeText = "Sensational execution! You strike the ball cleanly past the goalkeeper and into the back of the net!";
                }
                else if (choice.Action == MatchAction.ThroughBall || choice.Action == MatchAction.Cross || choice.Action == MatchAction.ShortPass)
                {
                    _playerAssists++;
                    _homeScore++;
                    ratingDelta = 0.65;
                    outcomeHeader = "ASSIST! 👟";
                    narrativeText = "Perfect vision! Your pinpoint pass unlocks the defense and your teammate converts with a first-time finish!";
                }
                else
                {
                    _keyActions++;
                    ratingDelta = 0.35;
                    outcomeHeader = "EXCELLENT PLAY! ⭐";
                    narrativeText = "Great technique and awareness! You beat your man and keep momentum on your team's side.";
                }

                Core.Audio.AudioManager.Instance?.TriggerCrowdRoar();
                Core.Audio.AudioManager.Instance?.PlayWhistle(Core.Audio.WhistleType.GoalScored);
            }
            else
            {
                _errors++;
                ratingDelta = -0.30;
                outcomeHeader = "CHANCE MISSED! ❌";
                narrativeText = "The defender read the play well and intercepted before you could cleanly execute your action.";

                Core.Audio.AudioManager.Instance?.TriggerCrowdGasp();

                // Opponent background goal chance on counter
                if (_rng.NextBool(0.35f) && _awayScore == 0)
                {
                    _awayScore++;
                }
            }

            _matchRating = Math.Clamp(Math.Round(_matchRating + ratingDelta, 2), 4.50, 9.90);

            // Update live stats & resolution display
            UpdateScoreboard(_situationMinutes[_situationIndex]);

            if (_labelResolutionOutcome != null)
            {
                _labelResolutionOutcome.text = outcomeHeader;
                _labelResolutionOutcome.style.color = outcome.Success ? new UnityEngine.Color(0.2f, 0.85f, 0.4f) : new UnityEngine.Color(0.95f, 0.3f, 0.3f);
            }

            if (_labelResolutionNarrative != null)
                _labelResolutionNarrative.text = narrativeText;

            if (_labelResolutionRatingDelta != null)
                _labelResolutionRatingDelta.text = $"{(ratingDelta >= 0 ? "+" : "")}{ratingDelta:F2} Match Rating";

            if (_labelResolutionConfidenceDelta != null)
                _labelResolutionConfidenceDelta.text = $"+{confidenceDelta} Confidence";

            if (_btnNextMoment != null)
            {
                bool isLast = _situationIndex >= _situationMinutes.Length - 1;
                _btnNextMoment.text = isLast ? "Full Time Whistle →" : "Next Situation →";
            }

            if (_cardSituation != null) _cardSituation.style.display = DisplayStyle.None;
            if (_cardResolution != null) _cardResolution.style.display = DisplayStyle.Flex;
        }

        private void OnNextMoment()
        {
            _situationIndex++;
            PresentSituation(_situationIndex);
        }

        private void UpdateScoreboard(int minute)
        {
            if (_labelClock != null) _labelClock.text = $"{minute}&apos;";
            if (_labelHomeScore != null) _labelHomeScore.text = _homeScore.ToString();
            if (_labelAwayScore != null) _labelAwayScore.text = _awayScore.ToString();

            if (_valLiveRating != null) _valLiveRating.text = _matchRating.ToString("F2");
            if (_valLiveGoals != null) _valLiveGoals.text = _playerGoals.ToString();
            if (_valLiveAssists != null) _valLiveAssists.text = _playerAssists.ToString();
        }

        private void FinishMatch()
        {
            Core.Audio.AudioManager.Instance?.PlayWhistle(Core.Audio.WhistleType.FullTime);
            Core.Audio.AudioManager.Instance?.StopCrowdMurmur();

            UpdateScoreboard(90);

            string homeTeam = _save != null && !string.IsNullOrEmpty(_save.ClubName) ? _save.ClubName : "Northfield Town";
            string awayTeam = _opp.HasValue ? _opp.Value.OpponentName : "Westford United";
            string comp = _opp.HasValue ? _opp.Value.Competition : "Division 4";
            bool isHome = !_opp.HasValue || _opp.Value.IsHome;

            var summary = new MatchSummaryData
            {
                HomeClub = homeTeam,
                AwayClub = awayTeam,
                HomeScore = _homeScore,
                AwayScore = _awayScore,
                PlayerGoals = _playerGoals,
                PlayerAssists = _playerAssists,
                KeyActions = _keyActions,
                Errors = _errors,
                MatchRating = _matchRating,
                Competition = comp,
                IsHome = isHome
            };

            _onMatchComplete?.Invoke(summary);
        }
    }
}
