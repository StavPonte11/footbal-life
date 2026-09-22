using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using FootballLife.Unity.Core.SceneManagement;

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
        private const string kCareerHubUxml  = "Assets/UI/Views/CareerHubView.uxml";
        private const string kTrainingUxml   = "Assets/UI/Views/TrainingView.uxml";
        private const string kRestUxml       = "Assets/UI/Views/RestView.uxml";
        private const string kLifeEventUxml  = "Assets/UI/Views/LifeEventView.uxml";
        private const string kPanelSettings  = "Assets/UI/PanelSettings.asset";
        private const string kCreationUxml   = "Assets/UI/Views/PlayerCreationView.uxml";
        private const string kClubUxml       = "Assets/UI/Views/ClubSelectionView.uxml";

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
            if (coordType != null && existingDoc.GetComponent(coordType) == null)
            {
                var coord = existingDoc.gameObject.AddComponent(coordType) as MonoBehaviour;
                var so = new SerializedObject(coord);
                SetSerializedAsset(so, "_careerHubAsset", kCareerHubUxml);
                SetSerializedAsset(so, "_trainingAsset",  kTrainingUxml);
                SetSerializedAsset(so, "_restAsset",      kRestUxml);
                SetSerializedAsset(so, "_lifeEventAsset", kLifeEventUxml);
                so.ApplyModifiedProperties();
                Debug.Log("[FullSceneSetup] CareerHub: Added CareerHubCoordinator with UXML refs.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ── Match Scene ───────────────────────────────────────────────────────
        private static void SetupMatchScene()
        {
            var scene = EditorSceneManager.OpenScene(kMatch, OpenSceneMode.Single);
            EnsureStandardHierarchy();

            // Add a Camera to the [CAMERAS] root so the scene isn't black
            var cameras = GameObject.Find("[CAMERAS]");
            if (cameras != null && cameras.GetComponentInChildren<Camera>() == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.transform.SetParent(cameras.transform, false);
                camGo.transform.localPosition = new Vector3(0, 1, -10);
                camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                Debug.Log("[FullSceneSetup] Match: Added Main Camera.");
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
