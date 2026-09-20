using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FootballLife.Unity.Core.SceneManagement
{
    /// <summary>
    /// Central orchestrator for asynchronous scene transitions, loading states, and scene lifecycle.
    /// Persistent singleton across scene changes.
    /// </summary>
    public class SceneFlowManager : MonoBehaviour
    {
        public const string SceneBootstrap = "Bootstrap";
        public const string SceneMainMenu = "MainMenu";
        public const string SceneCareerHub = "CareerHub";
        public const string SceneMatch = "Match";

        public static SceneFlowManager? Instance { get; private set; }

        public string CurrentSceneName { get; private set; } = SceneBootstrap;
        public bool IsLoading { get; private set; }

        public event Action<float>? OnLoadingProgress;
        public event Action<string>? OnSceneLoaded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentSceneName = SceneManager.GetActiveScene().name;
        }

        private void Start()
        {
            // If starting from Bootstrap scene, automatically transition to MainMenu
            if (CurrentSceneName == SceneBootstrap || string.IsNullOrEmpty(CurrentSceneName))
            {
                LoadMainMenu();
            }
        }

        public void LoadMainMenu(Action? onComplete = null)
        {
            StartCoroutine(LoadSceneRoutine(SceneMainMenu, onComplete));
        }

        public void LoadCareerHub(Action? onComplete = null)
        {
            StartCoroutine(LoadSceneRoutine(SceneCareerHub, onComplete));
        }

        public void LoadMatch(Action? onComplete = null)
        {
            StartCoroutine(LoadSceneRoutine(SceneMatch, onComplete));
        }

        public void TransitionTo(string targetScene, Action? onComplete = null)
        {
            StartCoroutine(LoadSceneRoutine(targetScene, onComplete));
        }

        private IEnumerator LoadSceneRoutine(string targetScene, Action? onComplete)
        {
            if (IsLoading) yield break;
            IsLoading = true;

            OnLoadingProgress?.Invoke(0.1f);
            yield return null;

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);
            if (asyncOp != null)
            {
                while (!asyncOp.isDone)
                {
                    float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                    OnLoadingProgress?.Invoke(progress);
                    yield return null;
                }
            }

            CurrentSceneName = targetScene;
            IsLoading = false;
            OnLoadingProgress?.Invoke(1.0f);
            OnSceneLoaded?.Invoke(targetScene);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Validates that the active scene hierarchy adheres to the architectural standard:
        /// [MANAGERS], [ENVIRONMENT], [ENTITIES], [CAMERAS], [UI].
        /// </summary>
        public static void EnsureStandardHierarchy()
        {
            string[] requiredRoots = { "[MANAGERS]", "[ENVIRONMENT]", "[ENTITIES]", "[CAMERAS]", "[UI]" };
            foreach (var rootName in requiredRoots)
            {
                var go = GameObject.Find(rootName);
                if (go == null)
                {
                    new GameObject(rootName);
                }
            }
        }
    }
}
