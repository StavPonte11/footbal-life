using System;
using FootballLife.Domain;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using FootballLife.Unity.Core.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using DomainPosition = FootballLife.Domain.Position;
using UIPosition = UnityEngine.UIElements.Position;

namespace FootballLife.Unity.UI.Match
{
    /// <summary>
    /// MonoBehaviour orchestrating the Match scene UI lifecycle:
    /// MatchPreviewView (#P2-015) ↔ MatchGameView (#P2-016) ↔ MatchPostView (#P2-017).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MatchCoordinator : MonoBehaviour
    {
        [Header("UXML Assets")]
        [SerializeField] private VisualTreeAsset? _matchPreviewAsset;
        [SerializeField] private VisualTreeAsset? _matchGameAsset;
        [SerializeField] private VisualTreeAsset? _matchPostAsset;
        [Header("3D Match HUD (#P3-011)")]
        [SerializeField] private MatchHUDController? _hudController;

        // ── Controllers ───────────────────────────────────────────────────────
        private MatchPreviewController? _previewCtrl;
        private MatchGameController?    _gameCtrl;
        private MatchPostController?    _postCtrl;

        // ── Overlays ──────────────────────────────────────────────────────────
        private VisualElement? _previewOverlay;
        private VisualElement? _gameOverlay;
        private VisualElement? _postOverlay;

        private UIDocument? _doc;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (_doc == null)
            {
                _doc = GetComponent<UIDocument>();
            }

            if (_doc == null)
            {
                Debug.LogError("[MatchCoordinator] Missing UIDocument component.");
                return;
            }

            // Auto-initialize mock bridge if testing Match scene directly without Bootstrap
            if (SimulationBridge.Instance == null)
            {
                var bridgeGo = new GameObject("SimulationBridge (Editor Preview)");
                var previewBridge = bridgeGo.AddComponent<SimulationBridge>();
                previewBridge.StartNewCareer(
                    playerName: "Marcus Vance",
                    nationality: "England",
                    position: DomainPosition.ST,
                    preferredFoot: Foot.Right,
                    startingClubName: "Northfield Town"
                );
                Debug.Log("[MatchCoordinator] Auto-initialized mock SimulationBridge for direct Match preview.");
            }

            if (_hudController == null)
            {
                _hudController = FindAnyObjectByType<MatchHUDController>(FindObjectsInactive.Include);
            }
            if (_hudController != null)
            {
                _hudController.OnMatchFinishRequested += HandleHUDMatchFinished;
            }

            BuildUI();
            ShowPreview();
        }

        private void OnDisable()
        {
            if (_hudController != null)
            {
                _hudController.OnMatchFinishRequested -= HandleHUDMatchFinished;
            }
        }

        private void BuildUI()
        {
            var docRoot = _doc!.rootVisualElement;
            docRoot.Clear();

            // ── 1. Match Preview View ─────────────────────────────────────────
            if (_matchPreviewAsset != null)
            {
                _previewOverlay = _matchPreviewAsset.Instantiate();
                _previewOverlay.style.position = UIPosition.Absolute;
                _previewOverlay.style.top = 0;
                _previewOverlay.style.left = 0;
                _previewOverlay.style.right = 0;
                _previewOverlay.style.bottom = 0;
                docRoot.Add(_previewOverlay);

                _previewCtrl = new MatchPreviewController(
                    _previewOverlay,
                    onKickoff: StartMatch,
                    onCancel: ReturnToHub);
            }

            // ── 2. Match Gameplay View ────────────────────────────────────────
            if (_matchGameAsset != null)
            {
                _gameOverlay = _matchGameAsset.Instantiate();
                _gameOverlay.style.position = UIPosition.Absolute;
                _gameOverlay.style.top = 0;
                _gameOverlay.style.left = 0;
                _gameOverlay.style.right = 0;
                _gameOverlay.style.bottom = 0;
                _gameOverlay.style.display = DisplayStyle.None;
                docRoot.Add(_gameOverlay);

                _gameCtrl = new MatchGameController(
                    _gameOverlay,
                    onMatchComplete: ShowPostMatch);
            }

            // ── 3. Post-Match Summary View ────────────────────────────────────
            if (_matchPostAsset != null)
            {
                _postOverlay = _matchPostAsset.Instantiate();
                _postOverlay.style.position = UIPosition.Absolute;
                _postOverlay.style.top = 0;
                _postOverlay.style.left = 0;
                _postOverlay.style.right = 0;
                _postOverlay.style.bottom = 0;
                _postOverlay.style.display = DisplayStyle.None;
                docRoot.Add(_postOverlay);

                _postCtrl = new MatchPostController(
                    _postOverlay,
                    onReturnToHub: ReturnToHub);
            }
        }

        public void ShowPreview()
        {
            var bridge = SimulationBridge.Instance;
            var opp = bridge?.CurrentMatchOpportunity ?? new MatchOpportunitySnapshot(
                opponentName: "Westford United",
                competition: "Division 4",
                isHome: true,
                squadRole: bridge?.CurrentSave?.SquadRole ?? "Starter",
                targetScoreDiff: 1);

            _previewCtrl?.Bind(opp, bridge?.CurrentSave);

            if (_gameOverlay != null) _gameOverlay.style.display = DisplayStyle.None;
            if (_postOverlay != null) _postOverlay.style.display = DisplayStyle.None;
            if (_hudController != null) _hudController.gameObject.SetActive(false);
            if (_previewOverlay != null) _previewOverlay.style.display = DisplayStyle.Flex;
        }

        public void StartMatch()
        {
            var bridge = SimulationBridge.Instance;
            var opp = bridge?.CurrentMatchOpportunity ?? new MatchOpportunitySnapshot(
                opponentName: "Westford United",
                competition: "Division 4",
                isHome: true,
                squadRole: bridge?.CurrentSave?.SquadRole ?? "Starter",
                targetScoreDiff: 1);

            if (_previewOverlay != null) _previewOverlay.style.display = DisplayStyle.None;
            if (_postOverlay != null) _postOverlay.style.display = DisplayStyle.None;

            if (_hudController != null)
            {
                _hudController.gameObject.SetActive(true);
                if (_gameOverlay != null) _gameOverlay.style.display = DisplayStyle.None;

                string home = opp.IsHome ? (bridge?.CurrentSave?.ClubName ?? "Northfield Town") : opp.OpponentName;
                string away = opp.IsHome ? opp.OpponentName : (bridge?.CurrentSave?.ClubName ?? "Northfield Town");
                float stamina = bridge?.CurrentSave != null ? (float)bridge.CurrentSave.Energy : 78f;
                _hudController.SetMatchDetails(home, away, 0, 0, 68, stamina);
            }
            else if (_gameOverlay != null)
            {
                _gameOverlay.style.display = DisplayStyle.Flex;
                _gameCtrl?.StartMatch(opp, bridge?.CurrentSave);
            }
        }

        public void ShowPostMatch(MatchSummaryData summary)
        {
            var bridge = SimulationBridge.Instance;

            if (_hudController != null) _hudController.gameObject.SetActive(false);
            if (_previewOverlay != null) _previewOverlay.style.display = DisplayStyle.None;
            if (_gameOverlay != null) _gameOverlay.style.display = DisplayStyle.None;
            if (_postOverlay != null) _postOverlay.style.display = DisplayStyle.Flex;

            _postCtrl?.Bind(summary, bridge?.CurrentSave, bridge);
        }

        private void HandleHUDMatchFinished(int homeScore, int awayScore)
        {
            var bridge = SimulationBridge.Instance;
            var opp = bridge?.CurrentMatchOpportunity;
            var summary = new MatchSummaryData
            {
                HomeClub = opp?.IsHome == true ? (bridge?.CurrentSave?.ClubName ?? "Northfield Town") : (opp?.OpponentName ?? "Northfield Town"),
                AwayClub = opp?.IsHome == true ? (opp?.OpponentName ?? "Westford United") : (bridge?.CurrentSave?.ClubName ?? "Westford United"),
                HomeScore = homeScore,
                AwayScore = awayScore,
                PlayerGoals = homeScore > 0 ? 1 : 0,
                PlayerAssists = 0,
                KeyActions = 3,
                Errors = 0,
                MatchRating = homeScore > awayScore ? 8.2 : 6.8,
                Competition = opp?.Competition ?? "Division 4",
                IsHome = opp?.IsHome ?? true
            };
            ShowPostMatch(summary);
        }

        public void ReturnToHub()
        {
            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadCareerHub();
            }
            else
            {
                SceneManager.LoadScene("CareerHub");
            }
        }
    }
}
