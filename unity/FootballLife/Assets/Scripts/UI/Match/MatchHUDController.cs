#nullable enable
using System;
using UnityEngine;
using UnityEngine.UIElements;
using FootballLife.Unity.Core.Gameplay;
using FootballLife.Unity.Core.Camera;

namespace FootballLife.Unity.UI.Match
{
    /// <summary>
    /// In-game Match HUD controller: manages live scoreline, match clock, stamina bar,
    /// contextual action triggers, and the dynamic goal celebration banner overlay.
    /// Zero GC allocations in update loops.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class MatchHUDController : MonoBehaviour
    {
        [Header("Match Vitals")]
        [SerializeField] private string _homeTeam = "Northfield Town";
        [SerializeField] private string _awayTeam = "Westford United";
        [SerializeField] private int _homeScore = 0;
        [SerializeField] private int _awayScore = 0;
        [SerializeField] private int _matchMinute = 68;
        [SerializeField] private float _staminaPercent = 78f;

        private UIDocument? _uiDocument;
        private Label? _labelHomeName;
        private Label? _labelAwayName;
        private Label? _labelHomeScore;
        private Label? _labelAwayScore;
        private Label? _labelClock;
        private VisualElement? _staminaFill;
        private Label? _labelStaminaPercent;
        private Label? _labelActionHint;

        private Button? _btnShoot;
        private Button? _btnPass;
        private Button? _btnReset;

        // Goal Celebration Overlay
        private VisualElement? _cardGoalCelebration;
        private Label? _labelGoalScorer;
        private Label? _labelGoalSpeed;
        private Label? _labelGoalScoreline;
        private Button? _btnGoalContinue;
        private Button? _btnGoalFinish;

        public event Action<int, int>? OnMatchFinishRequested;

        // References
        private BallController? _ball;
        private PlayerPawnController? _striker;
        private PassingInteraction? _passing;
        private MatchCameraRig? _cameraRig;
        private MatchPawn? _teammate;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            ResolveSceneReferences();
        }

        private void OnEnable()
        {
            BindUI();
            GoalTrigger.OnGoalScored += HandleGoalScored;
        }

        private void OnDisable()
        {
            GoalTrigger.OnGoalScored -= HandleGoalScored;
            UnbindButtons();
        }

        private void ResolveSceneReferences()
        {
            if (_ball == null) _ball = FindAnyObjectByType<BallController>();
            if (_cameraRig == null) _cameraRig = FindAnyObjectByType<MatchCameraRig>();
            if (_passing == null) _passing = FindAnyObjectByType<PassingInteraction>();

            var presenter = FindAnyObjectByType<SituationPawnPresenter>();
            if (presenter != null)
            {
                if (_striker == null && presenter.UserPlayer != null)
                    _striker = presenter.UserPlayer.GetComponent<PlayerPawnController>();
                if (_teammate == null)
                    _teammate = presenter.Teammate;
            }
        }

        private void BindUI()
        {
            if (_uiDocument == null) return;
            var root = _uiDocument.rootVisualElement;
            if (root == null) return;

            // Scoreboard elements
            _labelHomeName = root.Q<Label>("label-hud-home-name");
            _labelAwayName = root.Q<Label>("label-hud-away-name");
            _labelHomeScore = root.Q<Label>("label-hud-home-score");
            _labelAwayScore = root.Q<Label>("label-hud-away-score");
            _labelClock = root.Q<Label>("label-hud-clock");
            _staminaFill = root.Q<VisualElement>("stamina-bar-fill");
            _labelStaminaPercent = root.Q<Label>("label-stamina-percent");
            _labelActionHint = root.Q<Label>("label-action-hint");

            // Buttons
            _btnShoot = root.Q<Button>("btn-action-shoot");
            _btnPass = root.Q<Button>("btn-action-pass");
            _btnReset = root.Q<Button>("btn-action-reset");

            // Goal Celebration elements
            _cardGoalCelebration = root.Q<VisualElement>("card-goal-celebration");
            _labelGoalScorer = root.Q<Label>("label-goal-scorer");
            _labelGoalSpeed = root.Q<Label>("label-goal-speed");
            _labelGoalScoreline = root.Q<Label>("label-goal-scoreline");
            _btnGoalContinue = root.Q<Button>("btn-goal-continue");
            _btnGoalFinish = root.Q<Button>("btn-goal-finish");

            // Wire button clicks
            if (_btnShoot != null) _btnShoot.clicked += OnShootClicked;
            if (_btnPass != null) _btnPass.clicked += OnPassClicked;
            if (_btnReset != null) _btnReset.clicked += OnResetClicked;
            if (_btnGoalContinue != null) _btnGoalContinue.clicked += OnGoalContinueClicked;
            if (_btnGoalFinish != null) _btnGoalFinish.clicked += OnGoalFinishClicked;

            UpdateScoreboardUI();
        }

        private void UnbindButtons()
        {
            if (_btnShoot != null) _btnShoot.clicked -= OnShootClicked;
            if (_btnPass != null) _btnPass.clicked -= OnPassClicked;
            if (_btnReset != null) _btnReset.clicked -= OnResetClicked;
            if (_btnGoalContinue != null) _btnGoalContinue.clicked -= OnGoalContinueClicked;
            if (_btnGoalFinish != null) _btnGoalFinish.clicked -= OnGoalFinishClicked;
        }

        private void UpdateScoreboardUI()
        {
            if (_labelHomeName != null) _labelHomeName.text = _homeTeam;
            if (_labelAwayName != null) _labelAwayName.text = _awayTeam;
            if (_labelHomeScore != null) _labelHomeScore.text = _homeScore.ToString();
            if (_labelAwayScore != null) _labelAwayScore.text = _awayScore.ToString();
            if (_labelClock != null) _labelClock.text = $"{_matchMinute}'";

            if (_staminaFill != null) _staminaFill.style.width = Length.Percent(_staminaPercent);
            if (_labelStaminaPercent != null) _labelStaminaPercent.text = $"{Mathf.RoundToInt(_staminaPercent)}%";
        }

        // ── Button Callbacks ──────────────────────────────────────────────────
        private void OnShootClicked()
        {
            ResolveSceneReferences();
            if (_ball == null || _ball.IsKicked || _striker == null) return;

            // Direct power shot toward goal corners
            var shotVel = new Vector3(-1.4f, 4.2f, 25.0f);
            var spin = new Vector3(3.0f, -4.0f, 0f);
            _striker.ExecuteKick(shotVel, spin, _ball);

            if (_cameraRig != null)
            {
                _cameraRig.SetMode(MatchCameraRig.CameraMode.ShotTrack);
            }
        }

        private void OnPassClicked()
        {
            ResolveSceneReferences();
            if (_passing != null && _teammate != null)
            {
                _passing.ExecutePassToTeammate(_teammate);
            }
        }

        private void OnResetClicked()
        {
            ResolveSceneReferences();
            if (_ball != null)
            {
                _ball.ResetToInitialPosition();
            }
            if (_cameraRig != null)
            {
                _cameraRig.SetMode(MatchCameraRig.CameraMode.ActionAim);
                _cameraRig.SnapToTarget();
            }
            if (_cardGoalCelebration != null)
            {
                _cardGoalCelebration.style.display = DisplayStyle.None;
            }
            if (_striker != null)
            {
                _striker.SetState(PawnAnimState.Idle);
            }
        }

        private void OnGoalContinueClicked()
        {
            OnResetClicked();
        }

        private void OnGoalFinishClicked()
        {
            OnMatchFinishRequested?.Invoke(_homeScore, _awayScore);
        }

        // ── Goal Detection & Celebration Presentation ─────────────────────────
        private void HandleGoalScored(GoalScoredEvent evt)
        {
            _homeScore++;
            UpdateScoreboardUI();

            if (_cardGoalCelebration != null)
            {
                _cardGoalCelebration.style.display = DisplayStyle.Flex;
            }

            if (_labelGoalScorer != null)
            {
                _labelGoalScorer.text = $"Marcus Vance {_matchMinute}'";
            }

            if (_labelGoalSpeed != null)
            {
                _labelGoalSpeed.text = $"⚡ Shot Speed: {evt.ShotSpeedKmh:F1} km/h";
            }

            if (_labelGoalScoreline != null)
            {
                _labelGoalScoreline.text = $"{_homeTeam} {_homeScore} - {_awayScore} {_awayTeam}";
            }

            if (_cameraRig != null)
            {
                _cameraRig.SetMode(MatchCameraRig.CameraMode.Celebration);
            }
        }

        public void SetMatchDetails(string home, string away, int homeScore, int awayScore, int minute, float stamina)
        {
            _homeTeam = home;
            _awayTeam = away;
            _homeScore = homeScore;
            _awayScore = awayScore;
            _matchMinute = minute;
            _staminaPercent = stamina;
            UpdateScoreboardUI();
        }
    }
}
