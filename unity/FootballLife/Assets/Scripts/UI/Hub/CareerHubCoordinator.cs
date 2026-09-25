using FootballLife.Unity.Core.Bridge;
using FootballLife.Unity.UI.Hub;
using FootballLife.Unity.UI.OffSeason;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI
{
    /// <summary>
    /// MonoBehaviour mounted on the CareerHub scene's UIDocument.
    /// Orchestrates: CareerHubView (main screen) ↔ TrainingView ↔ RestView ↔ LifeEventView ↔ CareerView ↔ ProfileView ↔ OffSeason views.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CareerHubCoordinator : MonoBehaviour
    {
        [Header("UXML Assets")]
        [SerializeField] private VisualTreeAsset? _careerHubAsset;
        [SerializeField] private VisualTreeAsset? _trainingAsset;
        [SerializeField] private VisualTreeAsset? _restAsset;
        [SerializeField] private VisualTreeAsset? _lifeEventAsset;
        [SerializeField] private VisualTreeAsset? _careerViewAsset;
        [SerializeField] private VisualTreeAsset? _profileViewAsset;
        [SerializeField] private VisualTreeAsset? _seasonSummaryAsset;
        [SerializeField] private VisualTreeAsset? _attributeGrowthAsset;
        [SerializeField] private VisualTreeAsset? _transferWindowAsset;

        // ── Controllers ───────────────────────────────────────────────────────
        private CareerHubController?       _hubCtrl;
        private TrainingController?        _trainingCtrl;
        private RestController?            _restCtrl;
        private LifeEventController?       _lifeEventCtrl;
        private CareerController?          _careerCtrl;
        private ProfileController?         _profileCtrl;
        private SeasonSummaryController?   _seasonSummaryCtrl;
        private AttributeGrowthController? _attributeGrowthCtrl;
        private TransferWindowController?  _transferWindowCtrl;

        // ── Root panel elements ───────────────────────────────────────────────
        private VisualElement? _hubRoot;
        private VisualElement? _trainingOverlay;
        private VisualElement? _restOverlay;
        private VisualElement? _lifeEventOverlay;
        private VisualElement? _careerOverlay;
        private VisualElement? _profileOverlay;
        private VisualElement? _seasonSummaryOverlay;
        private VisualElement? _attributeGrowthOverlay;
        private VisualElement? _transferWindowOverlay;

        // ── Pending life event ────────────────────────────────────────────────
        private LifeEventSnapshot? _pendingLifeEvent;

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
                Debug.LogError("[CareerHubCoordinator] Missing UIDocument component.");
                return;
            }

            // Auto-initialize mock bridge if testing CareerHub scene directly without Bootstrap
            if (SimulationBridge.Instance == null)
            {
                var bridgeGo = new GameObject("SimulationBridge (Editor Preview)");
                var previewBridge = bridgeGo.AddComponent<SimulationBridge>();
                previewBridge.StartNewCareer(
                    playerName: "Marcus Vance",
                    nationality: "England",
                    position: FootballLife.Domain.Position.ST,
                    preferredFoot: FootballLife.Domain.Foot.Right,
                    startingClubName: "Northfield Town"
                );
                Debug.Log("[CareerHubCoordinator] Auto-initialized mock SimulationBridge for direct scene preview.");
            }

            // Validate assets
            if (_careerHubAsset == null)
            {
                Debug.LogError("[CareerHubCoordinator] _careerHubAsset not assigned in Inspector.");
                return;
            }

            BuildUI();
            BindBridge();

            // Subscribe to life event separately so we can buffer it
            SimulationBridge.Instance.OnLifeEventOccurred += OnLifeEventBuffered;
        }

        private void OnDisable()
        {
            _hubCtrl?.Unbind();
            if (SimulationBridge.Instance != null)
                SimulationBridge.Instance.OnLifeEventOccurred -= OnLifeEventBuffered;
        }

        // ── Build ─────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            var docRoot = _doc!.rootVisualElement;
            docRoot.Clear();

            // ── Career Hub (always visible base layer) ────────────────────────
            _hubRoot = _careerHubAsset!.Instantiate();
            _hubRoot.style.flexGrow = 1;
            docRoot.Add(_hubRoot);

            _hubCtrl = new CareerHubController(
                _hubRoot,
                onOpenTraining:     ShowTraining,
                onOpenRest:         ShowRest,
                onOpenCareer:       OnOpenCareer,
                onOpenMatch:        OnOpenMatch,
                onLifeEventPending: ShowPendingLifeEvent,
                onOpenOffSeason:    ShowSeasonSummary);

            // ── Training overlay ──────────────────────────────────────────────
            if (_trainingAsset != null)
            {
                _trainingOverlay = _trainingAsset.Instantiate();
                _trainingOverlay.style.position  = Position.Absolute;
                _trainingOverlay.style.top    = 0;
                _trainingOverlay.style.left   = 0;
                _trainingOverlay.style.right  = 0;
                _trainingOverlay.style.bottom = 0;
                _trainingOverlay.style.display = DisplayStyle.None;
                docRoot.Add(_trainingOverlay);

                _trainingCtrl = new TrainingController(
                    _trainingOverlay,
                    onConfirmed: HideTraining,
                    onCancelled: HideTraining);
            }

            // ── Rest overlay ──────────────────────────────────────────────────
            if (_restAsset != null)
            {
                _restOverlay = _restAsset.Instantiate();
                _restOverlay.style.position  = Position.Absolute;
                _restOverlay.style.top    = 0;
                _restOverlay.style.left   = 0;
                _restOverlay.style.right  = 0;
                _restOverlay.style.bottom = 0;
                _restOverlay.style.display = DisplayStyle.None;
                docRoot.Add(_restOverlay);

                _restCtrl = new RestController(
                    _restOverlay,
                    onConfirmed: HideRest,
                    onCancelled: HideRest);
            }

            // ── Life Event overlay ────────────────────────────────────────────
            if (_lifeEventAsset != null)
            {
                _lifeEventOverlay = _lifeEventAsset.Instantiate();
                _lifeEventOverlay.style.position  = Position.Absolute;
                _lifeEventOverlay.style.top    = 0;
                _lifeEventOverlay.style.left   = 0;
                _lifeEventOverlay.style.right  = 0;
                _lifeEventOverlay.style.bottom = 0;
                _lifeEventOverlay.style.display = DisplayStyle.None;
                docRoot.Add(_lifeEventOverlay);

                _lifeEventCtrl = new LifeEventController(
                    _lifeEventOverlay,
                    onDismiss: HideLifeEvent);
            }

            // ── Career View overlay ───────────────────────────────────────────
            if (_careerViewAsset != null)
            {
                _careerOverlay = _careerViewAsset.Instantiate();
                _careerOverlay.style.position  = Position.Absolute;
                _careerOverlay.style.top    = 0;
                _careerOverlay.style.left   = 0;
                _careerOverlay.style.right  = 0;
                _careerOverlay.style.bottom = 0;
                _careerOverlay.style.display = DisplayStyle.None;
                docRoot.Add(_careerOverlay);

                _careerCtrl = new CareerController(
                    _careerOverlay,
                    onBackToHub:   ShowHub,
                    onOpenProfile: ShowProfile);
            }

            // ── Profile View overlay ──────────────────────────────────────────
            if (_profileViewAsset != null)
            {
                _profileOverlay = _profileViewAsset.Instantiate();
                _profileOverlay.style.position  = Position.Absolute;
                _profileOverlay.style.top    = 0;
                _profileOverlay.style.left   = 0;
                _profileOverlay.style.right  = 0;
                _profileOverlay.style.bottom = 0;
                _profileOverlay.style.display = DisplayStyle.None;
                docRoot.Add(_profileOverlay);

                _profileCtrl = new ProfileController(
                    _profileOverlay,
                    onBackToHub:  ShowHub,
                    onOpenCareer: ShowCareer);
            }

            // ── Season Summary overlay ────────────────────────────────────────
            if (_seasonSummaryAsset != null)
            {
                _seasonSummaryOverlay = _seasonSummaryAsset.Instantiate();
                _seasonSummaryOverlay.style.position = Position.Absolute;
                _seasonSummaryOverlay.style.top = 0;
                _seasonSummaryOverlay.style.left = 0;
                _seasonSummaryOverlay.style.right = 0;
                _seasonSummaryOverlay.style.bottom = 0;
                _seasonSummaryOverlay.style.display = DisplayStyle.None;
                docRoot.Add(_seasonSummaryOverlay);

                _seasonSummaryCtrl = new SeasonSummaryController(
                    _seasonSummaryOverlay,
                    onViewGrowth:  ShowAttributeGrowth,
                    onReturnToHub: ShowHub);
            }

            // ── Attribute Growth overlay ──────────────────────────────────────
            if (_attributeGrowthAsset != null)
            {
                _attributeGrowthOverlay = _attributeGrowthAsset.Instantiate();
                _attributeGrowthOverlay.style.position = Position.Absolute;
                _attributeGrowthOverlay.style.top = 0;
                _attributeGrowthOverlay.style.left = 0;
                _attributeGrowthOverlay.style.right = 0;
                _attributeGrowthOverlay.style.bottom = 0;
                _attributeGrowthOverlay.style.display = DisplayStyle.None;
                docRoot.Add(_attributeGrowthOverlay);

                _attributeGrowthCtrl = new AttributeGrowthController(
                    _attributeGrowthOverlay,
                    onProceedToTransfers: ShowTransferWindow,
                    onBackToSummary:      ShowSeasonSummary);
            }

            // ── Transfer Window overlay ───────────────────────────────────────
            if (_transferWindowAsset != null)
            {
                _transferWindowOverlay = _transferWindowAsset.Instantiate();
                _transferWindowOverlay.style.position = Position.Absolute;
                _transferWindowOverlay.style.top = 0;
                _transferWindowOverlay.style.left = 0;
                _transferWindowOverlay.style.right = 0;
                _transferWindowOverlay.style.bottom = 0;
                _transferWindowOverlay.style.display = DisplayStyle.None;
                docRoot.Add(_transferWindowOverlay);

                _transferWindowCtrl = new TransferWindowController(
                    _transferWindowOverlay,
                    onAcceptOffer:     OnAcceptTransferOffer,
                    onStartNextSeason: OnStartNextSeason,
                    onBackToGrowth:    ShowAttributeGrowth);
            }
        }

        private void BindBridge()
        {
            var bridge = SimulationBridge.Instance!;
            _hubCtrl?.Bind(bridge);
            _trainingCtrl?.Bind(bridge);
            _restCtrl?.Bind(bridge);
            _careerCtrl?.Bind(bridge);
            _profileCtrl?.Bind(bridge);
        }

        // ── Training overlay ──────────────────────────────────────────────────
        private void ShowTraining()
        {
            // Re-bind in case energy changed since construction
            if (SimulationBridge.Instance != null)
                _trainingCtrl?.Bind(SimulationBridge.Instance);

            if (_trainingOverlay != null)
                _trainingOverlay.style.display = DisplayStyle.Flex;
        }

        private void HideTraining()
        {
            if (_trainingOverlay != null)
                _trainingOverlay.style.display = DisplayStyle.None;

            // Re-bind rest panel too (balance might have changed)
            if (SimulationBridge.Instance != null)
                _restCtrl?.Bind(SimulationBridge.Instance);
        }

        // ── Rest overlay ──────────────────────────────────────────────────────
        private void ShowRest()
        {
            if (SimulationBridge.Instance != null)
                _restCtrl?.Bind(SimulationBridge.Instance);

            if (_restOverlay != null)
                _restOverlay.style.display = DisplayStyle.Flex;
        }

        private void HideRest()
        {
            if (_restOverlay != null)
                _restOverlay.style.display = DisplayStyle.None;
        }

        // ── Life Event overlay ────────────────────────────────────────────────
        private void OnLifeEventBuffered(LifeEventSnapshot snap)
        {
            // Buffer the event; it will be shown when the hub requests it
            _pendingLifeEvent = snap;
        }

        private void ShowPendingLifeEvent()
        {
            if (_pendingLifeEvent == null || _lifeEventCtrl == null || _lifeEventOverlay == null)
                return;

            _lifeEventCtrl.Show(_pendingLifeEvent.Value);
            _lifeEventOverlay.style.display = DisplayStyle.Flex;
            _pendingLifeEvent = null;
        }

        private void HideLifeEvent()
        {
            if (_lifeEventOverlay != null)
                _lifeEventOverlay.style.display = DisplayStyle.None;
        }

        // ── Career & Profile Screens Navigation ───────────────────────────────
        private void OnOpenCareer()
        {
            ShowCareer();
        }

        private void OnOpenMatch()
        {
            if (FootballLife.Unity.Core.SceneManagement.SceneFlowManager.Instance != null)
            {
                FootballLife.Unity.Core.SceneManagement.SceneFlowManager.Instance.LoadMatch();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Match");
            }
        }

        public void ShowCareer()
        {
            if (SimulationBridge.Instance != null)
                _careerCtrl?.Bind(SimulationBridge.Instance);

            if (_profileOverlay != null)
                _profileOverlay.style.display = DisplayStyle.None;

            if (_careerOverlay != null)
                _careerOverlay.style.display = DisplayStyle.Flex;
        }

        public void ShowProfile()
        {
            if (SimulationBridge.Instance != null)
                _profileCtrl?.Bind(SimulationBridge.Instance);

            if (_careerOverlay != null)
                _careerOverlay.style.display = DisplayStyle.None;

            if (_profileOverlay != null)
                _profileOverlay.style.display = DisplayStyle.Flex;
        }

        public void ShowHub()
        {
            HideAllOverlays();
        }

        // ── Milestone 2.6 Off-Season & Transfers Navigation ───────────────────
        public void ShowSeasonSummary()
        {
            if (SimulationBridge.Instance != null)
            {
                var summary = SimulationBridge.Instance.GetSeasonSummaryData();
                _seasonSummaryCtrl?.Bind(summary, SimulationBridge.Instance.CurrentSave);
            }

            HideAllOverlays();
            if (_seasonSummaryOverlay != null)
                _seasonSummaryOverlay.style.display = DisplayStyle.Flex;
        }

        public void ShowAttributeGrowth()
        {
            if (SimulationBridge.Instance != null)
            {
                var growth = SimulationBridge.Instance.GetAttributeGrowthData();
                _attributeGrowthCtrl?.Bind(growth, SimulationBridge.Instance.CurrentSave);
            }

            HideAllOverlays();
            if (_attributeGrowthOverlay != null)
                _attributeGrowthOverlay.style.display = DisplayStyle.Flex;
        }

        public void ShowTransferWindow()
        {
            if (SimulationBridge.Instance != null)
            {
                var offers = SimulationBridge.Instance.GetTransferOffers();
                _transferWindowCtrl?.Bind(offers, SimulationBridge.Instance.CurrentSave);
            }

            HideAllOverlays();
            if (_transferWindowOverlay != null)
                _transferWindowOverlay.style.display = DisplayStyle.Flex;
        }

        private void OnAcceptTransferOffer(TransferOfferSnapshot offer)
        {
            if (SimulationBridge.Instance != null)
            {
                SimulationBridge.Instance.AcceptTransferOffer(offer);
                var offers = SimulationBridge.Instance.GetTransferOffers();
                _transferWindowCtrl?.Bind(offers, SimulationBridge.Instance.CurrentSave);
            }
        }

        private void OnStartNextSeason()
        {
            if (SimulationBridge.Instance != null)
            {
                SimulationBridge.Instance.AdvanceToNextSeason();
            }

            HideAllOverlays();
        }

        private void HideAllOverlays()
        {
            if (_trainingOverlay != null) _trainingOverlay.style.display = DisplayStyle.None;
            if (_restOverlay != null) _restOverlay.style.display = DisplayStyle.None;
            if (_lifeEventOverlay != null) _lifeEventOverlay.style.display = DisplayStyle.None;
            if (_careerOverlay != null) _careerOverlay.style.display = DisplayStyle.None;
            if (_profileOverlay != null) _profileOverlay.style.display = DisplayStyle.None;
            if (_seasonSummaryOverlay != null) _seasonSummaryOverlay.style.display = DisplayStyle.None;
            if (_attributeGrowthOverlay != null) _attributeGrowthOverlay.style.display = DisplayStyle.None;
            if (_transferWindowOverlay != null) _transferWindowOverlay.style.display = DisplayStyle.None;
        }
    }
}
