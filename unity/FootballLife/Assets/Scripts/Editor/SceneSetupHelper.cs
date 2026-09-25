using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using FootballLife.Unity.Core.SceneManagement;
using FootballLife.Unity.Core.Camera;
using FootballLife.Unity.Core.Environment;
using FootballLife.Unity.Core.Gameplay;

namespace FootballLife.Unity.Editor
{
    /// <summary>
    /// Football Life ▶ Scene Setup ▶ Setup All Scenes
    ///
    /// Call this once to:
    ///   1. Add all scenes to Build Settings in the correct order.
    ///   2. Open Bootstrap → create [MANAGERS] with SceneFlowManager + SimulationBridge.
    ///   3. Open MainMenu  → ensure hierarchy; menu UI (stub label).
    ///   4. Open CareerHub → create UIDocument + CareerHubCoordinator + assign UXML refs.
    ///   5. Open Match     → create empty hierarchy ready for M2.5.
    ///   6. Re-open Bootstrap as the working scene.
    /// </summary>
    public static class FullSceneSetup
    {
        private const string kBootstrap  = "Assets/Scenes/Bootstrap.unity";
        private const string kMainMenu   = "Assets/Scenes/MainMenu.unity";
        private const string kCareerHub  = "Assets/Scenes/CareerHub.unity";
        private const string kMatch      = "Assets/Scenes/Match.unity";

        // UXML asset paths
        private const string kCareerHubUxml   = "Assets/UI/Views/CareerHubView.uxml";
        private const string kTrainingUxml    = "Assets/UI/Views/TrainingView.uxml";
        private const string kRestUxml        = "Assets/UI/Views/RestView.uxml";
        private const string kLifeEventUxml   = "Assets/UI/Views/LifeEventView.uxml";
        private const string kCareerViewUxml  = "Assets/UI/Views/CareerView.uxml";
        private const string kProfileViewUxml = "Assets/UI/Views/ProfileView.uxml";
        private const string kMatchPreviewUxml = "Assets/UI/Views/MatchPreviewView.uxml";
        private const string kMatchGameUxml    = "Assets/UI/Views/MatchGameView.uxml";
        private const string kMatchPostUxml    = "Assets/UI/Views/MatchPostView.uxml";
        private const string kMatchHudUxml     = "Assets/UI/Views/MatchHUDView.uxml";
        private const string kSeasonSummaryUxml    = "Assets/UI/Views/SeasonSummaryView.uxml";
        private const string kAttributeGrowthUxml  = "Assets/UI/Views/AttributeGrowthView.uxml";
        private const string kTransferWindowUxml   = "Assets/UI/Views/TransferWindowView.uxml";
        private const string kPanelSettings   = "Assets/UI/PanelSettings.asset";
        private const string kCreationUxml    = "Assets/UI/Views/PlayerCreationView.uxml";
        private const string kClubUxml        = "Assets/UI/Views/ClubSelectionView.uxml";

        [MenuItem("Football Life/Scene Setup/Setup All Scenes (Run Once)")]
        public static void SetupAllScenes()
        {
            if (!EditorUtility.DisplayDialog(
                    "Football Life — Full Scene Setup",
                    "This will overwrite scene GameObjects in Bootstrap, MainMenu, CareerHub, and Match. " +
                    "Make sure you have committed any unsaved work. Continue?",
                    "Yes, Setup All", "Cancel"))
                return;

            ExecuteFullSetup();

            Debug.Log("[FullSceneSetup] ✅ All scenes configured! Open Bootstrap.unity and press Play to start.");
            EditorUtility.DisplayDialog(
                "Setup Complete",
                "All scenes have been configured.\n\n" +
                "► Open Bootstrap.unity and press ▶ Play to start the game.\n" +
                "► The Bootstrap scene auto-loads MainMenu → Player Creation → CareerHub.",
                "Got it!");
        }

        public static void SetupAllScenesBatchmode()
        {
            ExecuteFullSetup();
            Debug.Log("[FullSceneSetup] ✅ Batchmode setup complete.");
        }

        public static void ExecuteFullSetup()
        {
            SetupBuildSettings();
            SetupBootstrapScene();
            SetupMainMenuScene();
            SetupCareerHubScene();
            SetupMatchScene();
            EditorSceneManager.OpenScene(kBootstrap, OpenSceneMode.Single);
        }

        // ── Build Settings ────────────────────────────────────────────────────
        private static void SetupBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(kBootstrap,  true),
                new EditorBuildSettingsScene(kMainMenu,   true),
                new EditorBuildSettingsScene(kCareerHub,  true),
                new EditorBuildSettingsScene(kMatch,      true),
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("[FullSceneSetup] Build Settings: 4 scenes registered (Bootstrap=0, MainMenu=1, CareerHub=2, Match=3).");
        }

        // ── PanelSettings Helper ──────────────────────────────────────────────
        private static PanelSettings GetOrCreatePanelSettings()
        {
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(kPanelSettings);
            if (panelSettings == null)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                panelSettings.referenceResolution = new Vector2Int(1080, 1920);
                panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
                panelSettings.match = 0.5f;
                AssetDatabase.CreateAsset(panelSettings, kPanelSettings);
                AssetDatabase.SaveAssets();
                Debug.Log($"[FullSceneSetup] Created PanelSettings asset at {kPanelSettings}");
            }
            return panelSettings;
        }

        // ── Bootstrap Scene ───────────────────────────────────────────────────
        private static void SetupBootstrapScene()
        {
            var scene = EditorSceneManager.OpenScene(kBootstrap, OpenSceneMode.Single);
            EnsureStandardHierarchy();

            var managers = GameObject.Find("[MANAGERS]");

            // SceneFlowManager
            if (managers.GetComponentInChildren<SceneFlowManager>() == null)
            {
                var sfmGo = new GameObject("SceneFlowManager");
                sfmGo.transform.SetParent(managers.transform, false);
                sfmGo.AddComponent<SceneFlowManager>();
                Debug.Log("[FullSceneSetup] Bootstrap: Added SceneFlowManager.");
            }

            // SimulationBridge
            var bridgeType = System.Type.GetType("FootballLife.Unity.Core.Bridge.SimulationBridge, FootballLife.Unity.Core");
            if (bridgeType != null && managers.GetComponentInChildren(bridgeType) == null)
            {
                var bridgeGo = new GameObject("SimulationBridge");
                bridgeGo.transform.SetParent(managers.transform, false);
                bridgeGo.AddComponent(bridgeType);
                Debug.Log("[FullSceneSetup] Bootstrap: Added SimulationBridge.");
            }
            else if (bridgeType == null)
            {
                Debug.LogWarning("[FullSceneSetup] Bootstrap: Could not resolve SimulationBridge type — add manually.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ── MainMenu Scene ────────────────────────────────────────────────────
        private static void SetupMainMenuScene()
        {
            var scene = EditorSceneManager.OpenScene(kMainMenu, OpenSceneMode.Single);
            EnsureStandardHierarchy();

            var uiRoot = GameObject.Find("[UI]");
            var panelSettings = GetOrCreatePanelSettings();

            var existingDoc = uiRoot.GetComponentInChildren<UIDocument>();
            if (existingDoc == null)
            {
                var uiDocGo = new GameObject("UIDocument_PlayerCreation");
                uiDocGo.transform.SetParent(uiRoot.transform, false);
                existingDoc = uiDocGo.AddComponent<UIDocument>();
            }

            if (panelSettings != null) existingDoc.panelSettings = panelSettings;

            var creationAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(kCreationUxml);
            if (creationAsset != null) existingDoc.visualTreeAsset = creationAsset;

            var coordType = System.Type.GetType("FootballLife.Unity.UI.Creation.PlayerCreationCoordinator, FootballLife.Unity.UI");
            if (coordType != null && existingDoc.GetComponent(coordType) == null)
            {
                var coord = existingDoc.gameObject.AddComponent(coordType) as MonoBehaviour;
                var so = new SerializedObject(coord);
                SetSerializedRef(so, "_uiDocument", existingDoc);
                SetSerializedAsset(so, "_playerCreationViewAsset", kCreationUxml);
                SetSerializedAsset(so, "_clubSelectionViewAsset",  kClubUxml);
                so.ApplyModifiedProperties();
                Debug.Log("[FullSceneSetup] MainMenu: Added PlayerCreationCoordinator with UXML refs.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ── CareerHub Scene ───────────────────────────────────────────────────
        private static void SetupCareerHubScene()
        {
            var scene = EditorSceneManager.OpenScene(kCareerHub, OpenSceneMode.Single);
            EnsureStandardHierarchy();

            var uiRoot = GameObject.Find("[UI]");
            var panelSettings = GetOrCreatePanelSettings();

            var existingDoc = uiRoot.GetComponentInChildren<UIDocument>();
            if (existingDoc == null)
            {
                var uiDocGo = new GameObject("UIDocument_CareerHub");
                uiDocGo.transform.SetParent(uiRoot.transform, false);
                existingDoc = uiDocGo.AddComponent<UIDocument>();
            }

            if (panelSettings != null) existingDoc.panelSettings = panelSettings;

            var hubAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(kCareerHubUxml);
            if (hubAsset != null) existingDoc.visualTreeAsset = hubAsset;

            var coordType = System.Type.GetType("FootballLife.Unity.UI.CareerHubCoordinator, FootballLife.Unity.UI");
            if (coordType != null)
            {
                var coord = existingDoc.GetComponent(coordType) as MonoBehaviour;
                if (coord == null)
                    coord = existingDoc.gameObject.AddComponent(coordType) as MonoBehaviour;

                var so = new SerializedObject(coord);
                SetSerializedAsset(so, "_careerHubAsset", kCareerHubUxml);
                SetSerializedAsset(so, "_trainingAsset",  kTrainingUxml);
                SetSerializedAsset(so, "_restAsset",      kRestUxml);
                SetSerializedAsset(so, "_lifeEventAsset", kLifeEventUxml);
                SetSerializedAsset(so, "_careerViewAsset", kCareerViewUxml);
                SetSerializedAsset(so, "_profileViewAsset", kProfileViewUxml);
                SetSerializedAsset(so, "_seasonSummaryAsset", kSeasonSummaryUxml);
                SetSerializedAsset(so, "_attributeGrowthAsset", kAttributeGrowthUxml);
                SetSerializedAsset(so, "_transferWindowAsset", kTransferWindowUxml);
                so.ApplyModifiedProperties();
                Debug.Log("[FullSceneSetup] CareerHub: Configured CareerHubCoordinator with all UXML refs.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ── Match Scene ───────────────────────────────────────────────────────
        private static void SetupMatchScene()
        {
            var scene = EditorSceneManager.OpenScene(kMatch, OpenSceneMode.Single);
            EnsureStandardHierarchy();

            // ── [ENVIRONMENT] 3D Stadium & Pitch ──────────────────────────────
            var envRoot = GameObject.Find("[ENVIRONMENT]");
            if (envRoot != null)
            {
                var existingPitch = envRoot.transform.Find("Pitch_Environment");
                if (existingPitch != null)
                {
                    Object.DestroyImmediate(existingPitch.gameObject);
                }
                PitchBuilder.BuildFullPitchEnvironment(envRoot.transform);
                Debug.Log("[FullSceneSetup] Match: Built 3D Pitch Environment.");
            }

            // ── [ENTITIES] Match Ball ──────────────────────────────────────────
            var entitiesRoot = GameObject.Find("[ENTITIES]");
            GameObject? ballGo = null;
            if (entitiesRoot != null)
            {
                var existingBall = entitiesRoot.transform.Find("Match_Ball");
                if (existingBall == null)
                {
                    ballGo = BallController.CreateBallGameObject(entitiesRoot.transform, new Vector3(0f, 0.11f, 15f));
                    Debug.Log("[FullSceneSetup] Match: Created 3D Match Ball.");
                }
                else
                {
                    ballGo = existingBall.gameObject;
                }
            }

            // ── [CAMERAS] Camera & MatchCameraRig ──────────────────────────────
            var cameras = GameObject.Find("[CAMERAS]");
            if (cameras != null)
            {
                var camOnRoot = cameras.GetComponent<UnityEngine.Camera>();
                if (camOnRoot != null)
                {
                    Object.DestroyImmediate(camOnRoot);
                }

                var camTransform = cameras.transform.Find("Main Camera");
                GameObject camGo;
                UnityEngine.Camera cam;
                if (camTransform == null)
                {
                    camGo = new GameObject("Main Camera");
                    camGo.transform.SetParent(cameras.transform, false);
                    cam = camGo.AddComponent<UnityEngine.Camera>();
                    camGo.tag = "MainCamera";
                    Debug.Log("[FullSceneSetup] Match: Added Main Camera as child of [CAMERAS].");
                }
                else
                {
                    camGo = camTransform.gameObject;
                    cam = camGo.GetComponent<UnityEngine.Camera>() ?? camGo.AddComponent<UnityEngine.Camera>();
                }

                var rig = camGo.GetComponent<MatchCameraRig>();
                if (rig == null)
                {
                    rig = camGo.AddComponent<MatchCameraRig>();
                }

                var goalStructure = GameObject.Find("Goal_Structure");
                rig.SetTargets(
                    player: null,
                    ball: ballGo != null ? ballGo.transform : null,
                    goal: goalStructure != null ? goalStructure.transform : null);
                rig.SetMode(MatchCameraRig.CameraMode.ActionAim);
                rig.SnapToTarget();
                Debug.Log("[FullSceneSetup] Match: Configured MatchCameraRig (ActionAim mode).");

                // ── [ENTITIES] Situation Pawns (Striker, Teammate, Defenders, GK) ──
                if (entitiesRoot != null)
                {
                    var ballController = ballGo?.GetComponent<BallController>();
                    SituationPawnPresenter.SetupMatchSituationPawns(
                        entitiesRoot.transform,
                        ballController,
                        rig,
                        goalStructure != null ? goalStructure.transform : null);
                    Debug.Log("[FullSceneSetup] Match: Spawned 3D Situation Pawns.");
                }
            }

            var uiRoot = GameObject.Find("[UI]");
            var panelSettings = GetOrCreatePanelSettings();

            var existingDoc = uiRoot.GetComponentInChildren<UIDocument>();
            if (existingDoc == null)
            {
                var uiDocGo = new GameObject("UIDocument_Match");
                uiDocGo.transform.SetParent(uiRoot.transform, false);
                existingDoc = uiDocGo.AddComponent<UIDocument>();
            }

            if (panelSettings != null) existingDoc.panelSettings = panelSettings;

            var previewAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(kMatchPreviewUxml);
            if (previewAsset != null) existingDoc.visualTreeAsset = previewAsset;

            // ── In-Game 3D Match HUD (#P3-010, #P3-011) ────────────────────────
            var hudTransform = uiRoot.transform.Find("UIDocument_MatchHUD");
            GameObject hudGo;
            UIDocument hudDoc;
            if (hudTransform == null)
            {
                hudGo = new GameObject("UIDocument_MatchHUD");
                hudGo.transform.SetParent(uiRoot.transform, false);
                hudDoc = hudGo.AddComponent<UIDocument>();
            }
            else
            {
                hudGo = hudTransform.gameObject;
                hudDoc = hudGo.GetComponent<UIDocument>() ?? hudGo.AddComponent<UIDocument>();
            }

            if (panelSettings != null) hudDoc.panelSettings = panelSettings;
            var hudAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(kMatchHudUxml);
            if (hudAsset != null) hudDoc.visualTreeAsset = hudAsset;

            var hudCtrlType = System.Type.GetType("FootballLife.Unity.UI.Match.MatchHUDController, FootballLife.Unity.UI");
            MonoBehaviour? hudCtrl = null;
            if (hudCtrlType != null)
            {
                hudCtrl = hudGo.GetComponent(hudCtrlType) as MonoBehaviour;
                if (hudCtrl == null)
                {
                    hudCtrl = hudGo.AddComponent(hudCtrlType) as MonoBehaviour;
                }
            }
            hudGo.SetActive(true);
            Debug.Log("[FullSceneSetup] Match: Configured UIDocument_MatchHUD with MatchHUDController.");

            var coordType = System.Type.GetType("FootballLife.Unity.UI.Match.MatchCoordinator, FootballLife.Unity.UI");
            if (coordType != null)
            {
                var coord = existingDoc.GetComponent(coordType) as MonoBehaviour;
                if (coord == null)
                    coord = existingDoc.gameObject.AddComponent(coordType) as MonoBehaviour;

                var so = new SerializedObject(coord);
                SetSerializedAsset(so, "_matchPreviewAsset", kMatchPreviewUxml);
                SetSerializedAsset(so, "_matchGameAsset",    kMatchGameUxml);
                SetSerializedAsset(so, "_matchPostAsset",    kMatchPostUxml);
                if (hudCtrl != null) SetSerializedRef(so, "_hudController", hudCtrl);
                so.ApplyModifiedProperties();
                Debug.Log("[FullSceneSetup] Match: Configured MatchCoordinator with all UXML & HUD refs.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static void EnsureStandardHierarchy()
        {
            string[] roots = { "[MANAGERS]", "[ENVIRONMENT]", "[ENTITIES]", "[CAMERAS]", "[UI]" };
            foreach (var name in roots)
            {
                if (GameObject.Find(name) == null)
                    new GameObject(name);
            }
        }

        private static void SetSerializedRef(SerializedObject so, string propName, Object obj)
        {
            var prop = so.FindProperty(propName);
            if (prop != null) prop.objectReferenceValue = obj;
        }

        private static void SetSerializedAsset(SerializedObject so, string propName, string assetPath)
        {
            var prop = so.FindProperty(propName);
            if (prop == null) return;
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset != null) prop.objectReferenceValue = asset;
            else Debug.LogWarning($"[FullSceneSetup] Asset not found at '{assetPath}' for property '{propName}'.");
        }
    }
}
