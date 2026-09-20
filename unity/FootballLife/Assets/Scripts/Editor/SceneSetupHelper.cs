using UnityEditor;
using UnityEngine;
using FootballLife.Unity.Core.SceneManagement;

namespace FootballLife.Unity.Editor
{
    public static class SceneSetupHelper
    {
        [MenuItem("Football Life/Scene/Ensure Standard Hierarchy")]
        public static void SetupCurrentSceneHierarchy()
        {
            SceneFlowManager.EnsureStandardHierarchy();
            Debug.Log("[SceneSetupHelper] Standard scene root hierarchy verified ([MANAGERS], [ENVIRONMENT], [ENTITIES], [CAMERAS], [UI]).");
        }
    }
}
