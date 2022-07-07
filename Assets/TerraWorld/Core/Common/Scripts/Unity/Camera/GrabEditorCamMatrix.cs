#if UNITY_EDITOR
using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class GrabEditorCamMatrix : MonoBehaviour
    {
        private Camera renderingCamera;

        void Update()
        {
            if (Application.isPlaying || UnityEditor.SceneView.lastActiveSceneView == null) return;

#if UNITY_EDITOR
            Camera temp = UnityEditor.SceneView.lastActiveSceneView.camera;

            if (temp.name.Equals("SceneCamera"))
                renderingCamera = temp;
            else
                renderingCamera = null;

            if (renderingCamera != null)
            {
                transform.position = renderingCamera.transform.position;
                transform.rotation = renderingCamera.transform.rotation;
            }
#endif
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += OnEditorUpdate;
#endif
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= OnEditorUpdate;
#endif
        }

#if UNITY_EDITOR
        protected virtual void OnEditorUpdate()
        {
            Update();
        }
#endif
    }
}
#endif

