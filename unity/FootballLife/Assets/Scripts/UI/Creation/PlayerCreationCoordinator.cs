using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Unity.Core.Bridge;
using FootballLife.Unity.Core.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Creation
{
    /// <summary>
    /// Orchestrates the multi-step Player Creation & Club Selection wizard.
    /// Wires UI events to SimulationBridge and transitions to CareerHub upon contract signing.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class PlayerCreationCoordinator : MonoBehaviour
    {
        [Header("UI Document & Templates")]
        [SerializeField] private UIDocument? _uiDocument;
        [SerializeField] private VisualTreeAsset? _playerCreationViewAsset;
        [SerializeField] private VisualTreeAsset? _clubSelectionViewAsset;

        private PlayerCreationData? _pendingPlayerData;
        private PlayerCreationController? _creationController;
        private ClubSelectionController? _clubController;

        private void Awake()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            _creationController = gameObject.AddComponent<PlayerCreationController>();
            _clubController = gameObject.AddComponent<ClubSelectionController>();

            _creationController.OnPlayerCreated += HandlePlayerCreated;
            _clubController.OnContractSigned += HandleContractSigned;
            _clubController.OnBackRequested += ShowPlayerCreationStep;
        }

        private void Start()
        {
            ShowPlayerCreationStep();
        }

        private void OnDestroy()
        {
            if (_creationController != null) _creationController.OnPlayerCreated -= HandlePlayerCreated;
            if (_clubController != null)
            {
                _clubController.OnContractSigned -= HandleContractSigned;
                _clubController.OnBackRequested -= ShowPlayerCreationStep;
            }
        }

        public void ShowPlayerCreationStep()
        {
            if (_uiDocument == null || _playerCreationViewAsset == null) return;

            _uiDocument.visualTreeAsset = _playerCreationViewAsset;
            _creationController?.BindVisualElements(_uiDocument.rootVisualElement);
        }

        public void ShowClubSelectionStep()
        {
            if (_uiDocument == null || _clubSelectionViewAsset == null) return;

            _uiDocument.visualTreeAsset = _clubSelectionViewAsset;
            _clubController?.BindVisualElements(_uiDocument.rootVisualElement);
        }

        private void HandlePlayerCreated(PlayerCreationData data)
        {
            _pendingPlayerData = data;
            ShowClubSelectionStep();
        }

        private void HandleContractSigned(ClubOfferData offer)
        {
            if (_pendingPlayerData == null) return;

            Debug.Log($"[PlayerCreationCoordinator] Contract Signed with {offer.ClubName}! Initializing simulation career.");

            // 1. Initialize Simulation Session
            var bridge = SimulationBridge.Instance;
            if (bridge != null)
            {
                bridge.StartNewCareer(
                    playerName: _pendingPlayerData.FullName,
                    nationality: _pendingPlayerData.Nationality,
                    position: _pendingPlayerData.PrimaryPosition,
                    preferredFoot: _pendingPlayerData.PreferredFoot,
                    startingClubName: offer.ClubName,
                    seed: 42
                );

                if (bridge.CurrentSave != null)
                {
                    bridge.CurrentSave.WeeklyWage = offer.WeeklyWage;
                    bridge.CurrentSave.SquadRole = offer.SquadRole;
                    bridge.CurrentSave.ManagerTrust = offer.InitialManagerTrust;
                    bridge.AutoSave();
                }
            }

            // 2. Transition smoothly to CareerHub
            var sceneFlow = SceneFlowManager.Instance;
            if (sceneFlow != null)
            {
                sceneFlow.LoadCareerHub();
            }
        }
    }
}
