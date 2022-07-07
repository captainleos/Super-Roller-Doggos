#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace TerraUnity.Runtime
{
    public class SceneManagement : MonoBehaviour
    {
        public static void MarkSceneDirty()
        {
            if (Application.isPlaying) return;

            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene != null) EditorSceneManager.MarkSceneDirty(currentScene);
        }
    }
}
#endif

