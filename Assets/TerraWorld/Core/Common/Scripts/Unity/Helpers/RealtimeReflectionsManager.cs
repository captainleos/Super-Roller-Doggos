using UnityEngine;
using UnityEditor;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class RealtimeReflectionsManager : MonoBehaviour
    {
        private Camera renderingCamera = null;

        void Update()
        {
            if (renderingCamera == null) SetRenderingCamera();
            if (renderingCamera == null) return;
            transform.position = renderingCamera.transform.position;
        }

        private void SetRenderingCamera()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                if (!Application.isPlaying && SceneView.lastActiveSceneView != null)
                {
                    Camera temp = SceneView.lastActiveSceneView.camera;
                    if (temp.name.Equals("SceneCamera")) renderingCamera = temp;
                    else renderingCamera = null;
                }
                else
                    renderingCamera = Camera.main;
            }
            else
#endif
                renderingCamera = Camera.main;
        }
    }
}

