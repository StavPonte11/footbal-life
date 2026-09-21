using FootballLife.Unity.Core.Bridge;
using FootballLife.Unity.UI.Hub;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI
{
    /// <summary>
    /// MonoBehaviour mounted on the CareerHub scene's UIDocument.
    /// Orchestrates: CareerHubView (main screen) ↔ TrainingView overlay ↔ RestView overlay ↔ LifeEventView overlay.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CareerHubCoordinator : MonoBehaviour
    {
        [Header("UXML Assets")]
        [SerializeField] private VisualTreeAsset? _careerHubAsset;
        [SerializeField] private VisualTreeAsset? _trainingAsset;
        [SerializeField] private VisualTreeAsset? _restAsset;
        [SerializeField] private VisualTreeAsset? _lifeEventAsset;

        // ── Controllers ───────────────────────────────────────────────────────
        private CareerHubController?  _hubCtrl;
        private TrainingController?   _trainingCtrl;
        private RestController?       _restCtrl;
        private LifeEventController?  _lifeEventCtrl;

        // ── Root panel elements ───────────────────────────────────────────────
        private VisualElement? _hubRoot;
        private VisualElement? _trainingOverlay;
        private VisualElement? _restOverlay;
        private VisualElement? _lifeEventOverlay;

        // ── Pending life event ────────────────────────────────────────────────
        private LifeEventSnapshot? _pendingLifeEvent;

        private UIDocument? _doc;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (_doc == null || SimulationBridge.Instance == null)
            {
                Debug.LogError("[CareerHubCoordinator] Missing UIDocument or SimulationBridge.Instance.");
                return;
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
                onLifeEventPending: ShowPendingLifeEvent);

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
        }

        private void BindBridge()
        {
            var bridge = SimulationBridge.Instance!;
            _hubCtrl?.Bind(bridge);
            _trainingCtrl?.Bind(bridge);
            _restCtrl?.Bind(bridge);
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

        // ── Career Screen stub ────────────────────────────────────────────────
        private void OnOpenCareer()
        {
            // Will navigate to Career screen in Milestone 2.4
            Debug.Log("[CareerHubCoordinator] Career screen — coming in Milestone 2.4.");
        }
    }
}
