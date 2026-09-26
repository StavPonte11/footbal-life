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
                    // Rapid Counter-Attack Breakthrough
                    _currentSituation = new MatchSituation(
                        SituationType.CounterAttackRun,
                        opponentPressure: 3.8f,
                        expectedDifficulty: 0.40f,
                        positionalAdvantage: 0.65f,
                        new[]
                        {
                            new ActionChoice(MatchAction.ThroughBall, riskLevel: 0.30f, expectedValue: 0.70f),
                            new ActionChoice(MatchAction.Dribble, riskLevel: 0.45f, expectedValue: 0.75f),
                            new ActionChoice(MatchAction.Shot_Long, riskLevel: 0.55f, expectedValue: 0.60f)
                        });

                    SetSituationUI(
                        minute,
                        "⚡ Counter-Attack Breakout",
                        "Your team intercepts at midfield! You sprint into open green space with only one retreating defender between you and the goal.",
                        "Moderate (3.8/10)",
                        "High (+0.65)",
                        "🎯 Slip Through-Ball to Winger (Risk: 30%)",
                        "💨 Burst Past Defender (Risk: 45%)",
                        "🚀 Early Driven Strike (Risk: 55%)");
                    break;

                case 1:
                    // 22-Yard Direct Free Kick Set Piece
                    _currentSituation = new MatchSituation(
                        SituationType.FreeKick,
                        opponentPressure: 3.5f,
                        expectedDifficulty: 0.65f,
                        positionalAdvantage: 0.35f,
                        new[]
                        {
                            new ActionChoice(MatchAction.FreeKick_Direct, riskLevel: 0.65f, expectedValue: 0.75f),
                            new ActionChoice(MatchAction.FreeKick_Cross, riskLevel: 0.30f, expectedValue: 0.60f),
                            new ActionChoice(MatchAction.ShortPass, riskLevel: 0.15f, expectedValue: 0.45f)
                        });

                    SetSituationUI(
                        minute,
                        "🎯 22-Yard Direct Free Kick",
                        "You've won a dangerous set piece just outside the penalty arc! The defensive wall is set, and the goalkeeper is anticipating your curl.",
                        "Direct Set Piece (3.5/10)",
                        "High (+0.35)",
                        "⚽ Curl Over Wall into Top Corner (Risk: 65%)",
                        "👟 Whip Swerving Cross to Far Post (Risk: 30%)",
                        "🔄 Short Lay-off to Supporting Midfielder (Risk: 15%)");
                    break;

                case 2:
                    // 1v1 Skill Move Take-on in the Box
                    _currentSituation = new MatchSituation(
                        SituationType.Dribbling1v1,
                        opponentPressure: 5.5f,
                        expectedDifficulty: 0.50f,
                        positionalAdvantage: 0.40f,
                        new[]
                        {
                            new ActionChoice(MatchAction.SkillMove, riskLevel: 0.50f, expectedValue: 0.75f),
                            new ActionChoice(MatchAction.Dribble, riskLevel: 0.35f, expectedValue: 0.60f),
                            new ActionChoice(MatchAction.ShortPass, riskLevel: 0.15f, expectedValue: 0.50f)
                        });

                    SetSituationUI(
                        minute,
                        "⚡ 1v1 Penalty Box Take-On",
                        "Isolated against the fullback on the left edge of the box! He's committed his weight—one explosive move opens up a clean shooting lane.",
                        "Direct Pressure (5.5/10)",
                        "Moderate (+0.40)",
                        "✨ Stepover & Burst Inside (Risk: 50%)",
                        "💨 Accelerate Down the Byline (Risk: 35%)",
                        "👟 Cut-back Pass to Top of Box (Risk: 15%)");
                    break;

                default:
                    // Late High-Stakes Penalty Kick or Breakaway
                    _currentSituation = new MatchSituation(
                        SituationType.PenaltyKick,
                        opponentPressure: 7.5f,
                        expectedDifficulty: 0.40f,
                        positionalAdvantage: 0.80f,
                        new[]
                        {
                            new ActionChoice(MatchAction.PenaltyKick, riskLevel: 0.25f, expectedValue: 0.85f)
                        });

                    SetSituationUI(
                        minute,
                        "🔥 88' Penalty Kick — High Drama!",
                        "A blatant handball in the box gives you a match-winning penalty! The entire stadium holds its breath as you step up to the spot.",
                        "Intense Pressure (7.5/10)",
                        "Dominant (+0.80)",
                        "⚽ Pick the Corner & Strike True (Risk: 25%)",
                        "—",
                        "—");
                    break;
            }

            // Reposition 3D pawns, ball, and camera for the new situation
            _situationPresenter?.ApplySituationPreset(_currentSituation.Type, _ball, _cameraRig);
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
            if (_btnChoice2 != null)
            {
                _btnChoice2.text = c2Text;
                _btnChoice2.style.display = c2Text == "—" ? DisplayStyle.None : DisplayStyle.Flex;
            }
            if (_btnChoice3 != null)
            {
                _btnChoice3.text = c3Text;
                _btnChoice3.style.display = c3Text == "—" ? DisplayStyle.None : DisplayStyle.Flex;
            }

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
                if (outcome.Type == OutcomeType.FreeKickGoal)
                {
                    _playerGoals++;
                    _homeScore++;
                    ratingDelta = 1.00;
                    outcomeHeader = "SPECTACULAR FREE KICK GOAL! 🎯⚽";
                    narrativeText = "Incredible curl and dip! The ball bends over the wall and clips the underside of the crossbar into the top corner!";
                }
                else if (outcome.Type == OutcomeType.PenaltyGoal)
                {
                    _playerGoals++;
                    _homeScore++;
                    ratingDelta = 0.85;
                    outcomeHeader = "PENALTY SCORED! 🎯⚽";
                    narrativeText = "Ice cold composure! You send the goalkeeper diving the wrong way and tuck the ball emphatically inside the post!";
                }
                else if (outcome.Type == OutcomeType.HeaderGoal || choice.Action == MatchAction.DivingHeader)
                {
                    _playerGoals++;
                    _homeScore++;
                    ratingDelta = 0.90;
                    outcomeHeader = "BULLET HEADER GOAL! 💥⚽";
                    narrativeText = "Sensational aerial power! You rise above your marker and power a thumping header past the helpless goalkeeper!";
                }
                else if (outcome.Type == OutcomeType.Goal || choice.Action == MatchAction.Shot_Close || choice.Action == MatchAction.Shot_Long || choice.Action == MatchAction.ChipShot)
                {
                    _playerGoals++;
                    _homeScore++;
                    ratingDelta = 0.85;
                    outcomeHeader = "GOAL SCORED! ⚽";
                    narrativeText = "Sensational execution! You strike the ball cleanly past the goalkeeper and into the back of the net!";
                }
                else if (choice.Action == MatchAction.FreeKick_Cross || choice.Action == MatchAction.CornerDelivery)
                {
                    _playerAssists++;
                    _homeScore++;
                    ratingDelta = 0.75;
                    outcomeHeader = "SET PIECE ASSIST! 👟🎯";
                    narrativeText = "Masterful delivery! Your whipped ball finds your teammate perfectly for a thumping header into the net!";
                }
                else if (choice.Action == MatchAction.ThroughBall || choice.Action == MatchAction.Cross || choice.Action == MatchAction.ShortPass)
                {
                    _playerAssists++;
                    _homeScore++;
                    ratingDelta = 0.65;
                    outcomeHeader = "ASSIST! 👟";
                    narrativeText = "Perfect vision! Your pinpoint pass unlocks the defense and your teammate converts with a first-time finish!";
                }
                else if (outcome.Type == OutcomeType.SkillBeatDefender || choice.Action == MatchAction.SkillMove)
                {
                    _keyActions++;
                    ratingDelta = 0.50;
                    outcomeHeader = "DAZZLING SKILL MOVE! ✨";
                    narrativeText = "Electric footwork! A lightning stepover sends the defender tumbling as you burst into open space!";
                }
                else if (outcome.Type == OutcomeType.BlockMade || choice.Action == MatchAction.BlockShot)
                {
                    _keyActions++;
                    ratingDelta = 0.60;
                    outcomeHeader = "HEROIC SHOT BLOCK! 🛡️";
                    narrativeText = "Crucial defensive intervention! You throw your body on the line to block a goal-bound strike!";
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

                if (outcome.Type == OutcomeType.PenaltyMissed || outcome.Type == OutcomeType.PenaltySaved)
                {
                    ratingDelta = -0.55;
                    outcomeHeader = "PENALTY MISSED! ❌";
                    narrativeText = "Agony from the spot! The keeper guesses correctly and parries your penalty away!";
                }
                else if (choice.Action == MatchAction.FreeKick_Direct)
                {
                    ratingDelta = -0.20;
                    outcomeHeader = "FREE KICK OVER! ❌";
                    narrativeText = "Close effort! The ball clears the wall with dip, but just misses the top right corner.";
                }
                else
                {
                    ratingDelta = -0.30;
                    outcomeHeader = "CHANCE MISSED! ❌";
                    narrativeText = "The defender read the play well and intercepted before you could cleanly execute your action.";
                }

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
