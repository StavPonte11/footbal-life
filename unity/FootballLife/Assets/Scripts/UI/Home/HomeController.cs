#nullable enable
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using FootballLife.Unity.Core.Camera;
using FootballLife.Unity.Core.Environment;
using FootballLife.Unity.Core.Gameplay;
using FootballLife.Unity.Core.SceneManagement;
using FootballLife.Unity.UI.Finances;
using FootballLife.Unity.UI.Phone;
using FootballLife.Unity.UI.Shop;

namespace FootballLife.Unity.UI.Home
{
    /// <summary>
    /// Presentation and interaction controller for the 3D Apartment HUD (#P4-001, #P4-002, #P4-006, #P4-007).
    /// Manages vitals display, hotspot navigation buttons, sleep & gym training execution,
    /// lifestyle property upgrades, finances dashboard, and lifestyle boutique.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class HomeController : MonoBehaviour
    {
        private UIDocument? _uiDoc;
        private HomeInteractionController? _interactionCtrl;
        private HomeCameraRig? _cameraRig;
        private PhoneOSController? _phoneController;
        private FinancesController? _financesController;
        private LifestyleShopController? _shopController;
        private VisualElement? _financesOverlay;
        private VisualElement? _shopOverlay;

        // HUD Elements
        private Label? _labelPropName;
        private Label? _labelTierBadge;
        private Label? _labelLocation;
        private Label? _labelEnergyPercent;
        private VisualElement? _energyBarFill;
        private Label? _labelBalance;

        // Bottom Dock Buttons
        private Button? _btnNavOverview;
        private Button? _btnNavBed;
        private Button? _btnNavGym;
        private Button? _btnNavPhone;
        private Button? _btnNavFinances;
        private Button? _btnNavShop;
        private Button? _btnNavDoor;
        private Button? _btnHomeUpgrade;

        // Toast
        private VisualElement? _cardToast;
        private Label? _labelToastTitle;
        private Label? _labelToastMessage;
        private Coroutine? _toastRoutine;

        // Upgrade Modal
        private VisualElement? _cardUpgradeModal;
        private Label? _labelUpgradeTitle;
        private Label? _labelUpgradeDesc;
        private Label? _labelUpgradePrice;
        private Label? _labelUpgradeUpkeep;
        private Label? _labelUpgradeRestBonus;
        private Label? _labelUpgradeGymBonus;
        private Button? _btnConfirmUpgrade;
        private Button? _btnCloseUpgrade;

        private void Awake()
        {
            _uiDoc = GetComponent<UIDocument>();
            ResolveSceneReferences();
        }

        private void OnEnable()
        {
            ResolveSceneReferences();
            BindUI();
            HookInteractionEvents();
            RefreshVitalsUI();
        }

        private void OnDisable()
        {
            UnhookInteractionEvents();
            UnbindButtons();
        }

        private void ResolveSceneReferences()
        {
            if (_interactionCtrl == null) _interactionCtrl = FindAnyObjectByType<HomeInteractionController>();
            if (_cameraRig == null) _cameraRig = FindAnyObjectByType<HomeCameraRig>();

            // Auto-initialize mock SimulationBridge if testing Home scene directly without Bootstrap
            if (SimulationBridge.Instance == null)
            {
                var bridgeGo = new GameObject("SimulationBridge (Editor Home Preview)");
                var previewBridge = bridgeGo.AddComponent<SimulationBridge>();
                previewBridge.StartNewCareer(
                    playerName: "Marcus Vance",
                    nationality: "England",
                    position: FootballLife.Domain.Position.ST,
                    preferredFoot: Foot.Right,
                    startingClubName: "Northfield Town"
                );
                if (previewBridge.CurrentSave != null)
                {
                    previewBridge.CurrentSave.LifestyleTier = (int)LifestyleTier.Comfortable;
                    previewBridge.CurrentSave.BankBalance = 45000;
                    previewBridge.CurrentSave.WeeklyWage = 1200;
                    previewBridge.CurrentSave.Energy = 72;
                }
                Debug.Log("[HomeController] Auto-initialized mock SimulationBridge for direct Home scene preview.");
            }
        }

        private void BindUI()
        {
            if (_uiDoc == null) return;
            var root = _uiDoc.rootVisualElement;
            if (root == null) return;

            // Top Bar
            _labelPropName = root.Q<Label>("label-home-property-name");
            _labelTierBadge = root.Q<Label>("label-home-tier-badge");
            _labelLocation = root.Q<Label>("label-home-location");
            _labelEnergyPercent = root.Q<Label>("label-home-energy-percent");
            _energyBarFill = root.Q<VisualElement>("home-energy-bar-fill");
            _labelBalance = root.Q<Label>("label-home-balance");

            // Buttons
            _btnNavOverview = root.Q<Button>("btn-nav-overview");
            _btnNavBed = root.Q<Button>("btn-nav-bed");
            _btnNavGym = root.Q<Button>("btn-nav-gym");
            _btnNavPhone = root.Q<Button>("btn-nav-phone");
            _btnNavFinances = root.Q<Button>("btn-nav-finances");
            _btnNavShop = root.Q<Button>("btn-nav-shop");
            _btnNavDoor = root.Q<Button>("btn-nav-door");
            _btnHomeUpgrade = root.Q<Button>("btn-home-upgrade");

            // Toast
            _cardToast = root.Q<VisualElement>("card-home-toast");
            _labelToastTitle = root.Q<Label>("label-toast-title");
            _labelToastMessage = root.Q<Label>("label-toast-message");

            // Upgrade Modal
            _cardUpgradeModal = root.Q<VisualElement>("card-home-upgrade-modal");
            _labelUpgradeTitle = root.Q<Label>("label-upgrade-target-title");
            _labelUpgradeDesc = root.Q<Label>("label-upgrade-target-desc");
            _labelUpgradePrice = root.Q<Label>("label-upgrade-price");
            _labelUpgradeUpkeep = root.Q<Label>("label-upgrade-upkeep");
            _labelUpgradeRestBonus = root.Q<Label>("label-upgrade-rest-bonus");
            _labelUpgradeGymBonus = root.Q<Label>("label-upgrade-gym-bonus");
            _btnConfirmUpgrade = root.Q<Button>("btn-confirm-upgrade");
            _btnCloseUpgrade = root.Q<Button>("btn-close-upgrade");

            // Wire clicks
            if (_btnNavOverview != null) _btnNavOverview.clicked += OnOverviewClicked;
            if (_btnNavBed != null) _btnNavBed.clicked += OnBedClicked;
            if (_btnNavGym != null) _btnNavGym.clicked += OnGymClicked;
            if (_btnNavPhone != null) _btnNavPhone.clicked += OnPhoneClicked;
            if (_btnNavFinances != null) _btnNavFinances.clicked += OnFinancesClicked;
            if (_btnNavShop != null) _btnNavShop.clicked += OnShopClicked;
            if (_btnNavDoor != null) _btnNavDoor.clicked += OnDoorClicked;
            if (_btnHomeUpgrade != null) _btnHomeUpgrade.clicked += OnUpgradeClicked;
            if (_btnConfirmUpgrade != null) _btnConfirmUpgrade.clicked += OnConfirmUpgradeClicked;
            if (_btnCloseUpgrade != null) _btnCloseUpgrade.clicked += OnCloseUpgradeClicked;

            // Finances Overlay
            _financesOverlay = root.Q<VisualElement>("finances-instance");
            if (_financesOverlay != null)
            {
                _financesOverlay.style.position = UnityEngine.UIElements.Position.Absolute;
                _financesOverlay.style.top = 0;
                _financesOverlay.style.left = 0;
                _financesOverlay.style.right = 0;
                _financesOverlay.style.bottom = 0;
                _financesOverlay.style.display = DisplayStyle.None;
                _financesController = new FinancesController(_financesOverlay, onBack: HideFinances);
            }

            // Shop Overlay
            _shopOverlay = root.Q<VisualElement>("shop-instance");
            if (_shopOverlay != null)
            {
                _shopOverlay.style.position = UnityEngine.UIElements.Position.Absolute;
                _shopOverlay.style.top = 0;
                _shopOverlay.style.left = 0;
                _shopOverlay.style.right = 0;
                _shopOverlay.style.bottom = 0;
                _shopOverlay.style.display = DisplayStyle.None;
                _shopController = new LifestyleShopController(_shopOverlay, onBack: HideShop);
            }

            if (_phoneController == null)
            {
                _phoneController = gameObject.GetComponent<PhoneOSController>() ?? gameObject.AddComponent<PhoneOSController>();
            }
            _phoneController.BindRoot(root);
            _phoneController.OnPhoneClosed += () =>
            {
                RefreshVitalsUI();
                _interactionCtrl?.SelectZone(HomeZoneType.Overview);
            };
        }

        private void UnbindButtons()
        {
            if (_btnNavOverview != null) _btnNavOverview.clicked -= OnOverviewClicked;
            if (_btnNavBed != null) _btnNavBed.clicked -= OnBedClicked;
            if (_btnNavGym != null) _btnNavGym.clicked -= OnGymClicked;
            if (_btnNavPhone != null) _btnNavPhone.clicked -= OnPhoneClicked;
            if (_btnNavFinances != null) _btnNavFinances.clicked -= OnFinancesClicked;
            if (_btnNavShop != null) _btnNavShop.clicked -= OnShopClicked;
            if (_btnNavDoor != null) _btnNavDoor.clicked -= OnDoorClicked;
            if (_btnHomeUpgrade != null) _btnHomeUpgrade.clicked -= OnUpgradeClicked;
            if (_btnConfirmUpgrade != null) _btnConfirmUpgrade.clicked -= OnConfirmUpgradeClicked;
            if (_btnCloseUpgrade != null) _btnCloseUpgrade.clicked -= OnCloseUpgradeClicked;
        }

        private void HookInteractionEvents()
        {
            if (_interactionCtrl != null)
            {
                _interactionCtrl.OnBedInteracted += HandleBedInteracted;
                _interactionCtrl.OnGymInteracted += HandleGymInteracted;
                _interactionCtrl.OnDoorExitRequested += HandleDoorExit;
                _interactionCtrl.OnPhoneRequested += HandlePhoneRequested;
            }
        }

        private void UnhookInteractionEvents()
        {
            if (_interactionCtrl != null)
            {
                _interactionCtrl.OnBedInteracted -= HandleBedInteracted;
                _interactionCtrl.OnGymInteracted -= HandleGymInteracted;
                _interactionCtrl.OnDoorExitRequested -= HandleDoorExit;
                _interactionCtrl.OnPhoneRequested -= HandlePhoneRequested;
            }
        }

        public void RefreshVitalsUI()
        {
            var save = SimulationBridge.Instance?.CurrentSave;
            var tier = save != null ? (LifestyleTier)save.LifestyleTier : LifestyleTier.Comfortable;
            var prop = HomePropertyCatalog.GetProperty(tier);

            if (_labelPropName != null) _labelPropName.text = prop.Name;
            if (_labelTierBadge != null) _labelTierBadge.text = $"Tier {(int)tier} · {tier}";
            if (_labelLocation != null) _labelLocation.text = $"{prop.Location} · £{prop.WeeklyUpkeep:N0}/wk upkeep";

            int energy = save?.Energy ?? 78;
            if (_energyBarFill != null) _energyBarFill.style.width = Length.Percent(energy);
            if (_labelEnergyPercent != null) _labelEnergyPercent.text = $"{energy}%";

            int balance = save?.BankBalance ?? 15400;
            if (_labelBalance != null) _labelBalance.text = $"£{balance:N0}";
        }

        // ── Button Handlers ───────────────────────────────────────────────────
        private void OnOverviewClicked()
        {
            _interactionCtrl?.SelectZone(HomeZoneType.Overview);
        }

        private void OnBedClicked()
        {
            _interactionCtrl?.SelectZone(HomeZoneType.Bed);
            HandleBedInteracted();
        }

        private void OnGymClicked()
        {
            _interactionCtrl?.SelectZone(HomeZoneType.Gym);
            HandleGymInteracted();
        }

        private void OnPhoneClicked()
        {
            _interactionCtrl?.SelectZone(HomeZoneType.Phone);
            _phoneController?.OpenPhone();
        }

        private void OnFinancesClicked()
        {
            if (_financesOverlay != null && _financesController != null)
            {
                _financesController.Refresh();
                _financesOverlay.style.display = DisplayStyle.Flex;
            }
        }

        private void HideFinances()
        {
            if (_financesOverlay != null)
                _financesOverlay.style.display = DisplayStyle.None;
            RefreshVitalsUI();
        }

        private void OnShopClicked()
        {
            if (_shopOverlay != null && _shopController != null)
            {
                _shopController.Refresh();
                _shopOverlay.style.display = DisplayStyle.Flex;
            }
        }

        private void HideShop()
        {
            if (_shopOverlay != null)
                _shopOverlay.style.display = DisplayStyle.None;
            RefreshVitalsUI();
        }

        private void OnDoorClicked()
        {
            _interactionCtrl?.ExitToClub();
        }

        private void OnUpgradeClicked()
        {
            var save = SimulationBridge.Instance?.CurrentSave;
            var currentTier = save != null ? (LifestyleTier)save.LifestyleTier : LifestyleTier.Comfortable;
            var nextTier = currentTier < LifestyleTier.Superstar ? (LifestyleTier)((int)currentTier + 1) : currentTier;
            var nextProp = HomePropertyCatalog.GetProperty(nextTier);

            if (_cardUpgradeModal != null) _cardUpgradeModal.style.display = DisplayStyle.Flex;
            if (_labelUpgradeTitle != null) _labelUpgradeTitle.text = nextProp.Name;
            if (_labelUpgradeDesc != null) _labelUpgradeDesc.text = nextProp.Description;
            if (_labelUpgradePrice != null) _labelUpgradePrice.text = $"£{nextProp.PurchaseCost:N0}";
            if (_labelUpgradeUpkeep != null) _labelUpgradeUpkeep.text = $"£{nextProp.WeeklyUpkeep:N0}/wk";
            if (_labelUpgradeRestBonus != null) _labelUpgradeRestBonus.text = $"+{Mathf.RoundToInt((nextProp.RestRecoveryMultiplier - 1f) * 100)}% Speed";
            if (_labelUpgradeGymBonus != null) _labelUpgradeGymBonus.text = $"+{Mathf.RoundToInt((nextProp.GymWorkoutMultiplier - 1f) * 100)}% Physical XP";
        }

        private void OnCloseUpgradeClicked()
        {
            if (_cardUpgradeModal != null) _cardUpgradeModal.style.display = DisplayStyle.None;
        }

        private void OnConfirmUpgradeClicked()
        {
            var save = SimulationBridge.Instance?.CurrentSave;
            if (save == null) return;

            var currentTier = (LifestyleTier)save.LifestyleTier;
            var nextTier = currentTier < LifestyleTier.Superstar ? (LifestyleTier)((int)currentTier + 1) : currentTier;

            var result = HomeSystem.UpgradeHome(save, nextTier);
            if (result.Success)
            {
                OnCloseUpgradeClicked();
                RefreshVitalsUI();
                ShowToast("⭐ Lifestyle Upgraded!", result.Message);

                // Re-skin the 3D apartment environment dynamically
                var envRoot = GameObject.Find("[ENVIRONMENT]");
                if (envRoot != null)
                {
                    ApartmentBuilder.RebuildForTier(envRoot.transform, nextTier);
                }
            }
            else
            {
                ShowToast("❌ Upgrade Failed", result.Message);
            }
        }

        // ── Event Handlers ────────────────────────────────────────────────────
        private void HandleBedInteracted()
        {
            var bridge = SimulationBridge.Instance;
            var save = bridge?.CurrentSave;
            var tier = save != null ? (LifestyleTier)save.LifestyleTier : LifestyleTier.Comfortable;
            int currentEnergy = save?.Energy ?? 70;

            float wellnessBonus = 0f;
            if (save?.OwnedLifestyleItemIds != null)
            {
                var (_, energyBonus, _) = LifestyleShopSystem.CalculateTotalPerks(save.OwnedLifestyleItemIds);
                wellnessBonus = energyBonus;
            }

            var result = HomeSystem.CalculateSleepRecovery(tier, currentEnergy, hoursSlept: 8, additionalRecoveryBonus: wellnessBonus);
            if (save != null)
            {
                save.Energy = result.RecoveredEnergy;
                save.Morale = Math.Min(100, save.Morale + 2);
            }

            RefreshVitalsUI();
            string perkNote = wellnessBonus > 0f ? $" (includes +{wellnessBonus * 100:0}% wellness gear perk)" : "";
            ShowToast("🛌 Rest Completed", $"Deep sleep in your home bed recovered +{result.EnergyGained}% Energy! ({result.TierMultiplier:F2}x efficiency{perkNote})");
        }

        private void HandleGymInteracted()
        {
            var bridge = SimulationBridge.Instance;
            var save = bridge?.CurrentSave;
            var tier = save != null ? (LifestyleTier)save.LifestyleTier : LifestyleTier.Comfortable;
            int currentEnergy = save?.Energy ?? 70;

            var result = HomeSystem.ExecuteHomeWorkout(tier, currentEnergy, WorkoutIntensity.Moderate);
            if (result.Success && save != null)
            {
                save.Energy = result.RemainingEnergy;
                save.Stamina = (byte)Math.Min(99, save.Stamina + (result.StaminaXpGained > 25 ? 1 : 0));
                save.Strength = (byte)Math.Min(99, save.Strength + (result.StrengthXpGained > 25 ? 1 : 0));
            }

            RefreshVitalsUI();
            if (result.Success)
            {
                ShowToast("🏋️ Home Workout Completed", $"+{result.StaminaXpGained} Stamina XP · +{result.StrengthXpGained} Strength XP ({result.TierMultiplier:F2}x gym tier bonus)");
            }
            else
            {
                ShowToast("⚠️ Too Fatigued", result.Message);
            }
        }

        private void HandlePhoneRequested()
        {
            _phoneController?.OpenPhone();
        }

        private void HandleDoorExit()
        {
            ShowToast("🚪 Departed Home", "Returning to Career Hub...");
            StartCoroutine(LoadCareerHubRoutine());
        }

        private IEnumerator LoadCareerHubRoutine()
        {
            yield return new WaitForSeconds(0.4f);
            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadCareerHub();
            }
            else
            {
                SceneManager.LoadScene("CareerHub");
            }
        }

        public void ShowToast(string title, string message)
        {
            if (_cardToast == null) return;
            if (_labelToastTitle != null) _labelToastTitle.text = title;
            if (_labelToastMessage != null) _labelToastMessage.text = message;

            _cardToast.style.display = DisplayStyle.Flex;

            if (_toastRoutine != null) StopCoroutine(_toastRoutine);
            _toastRoutine = StartCoroutine(HideToastRoutine(3.5f));
        }

        private IEnumerator HideToastRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_cardToast != null) _cardToast.style.display = DisplayStyle.None;
        }
    }
}
