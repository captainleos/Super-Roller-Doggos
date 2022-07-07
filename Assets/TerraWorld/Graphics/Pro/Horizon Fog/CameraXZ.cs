using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class CameraXZ : MonoBehaviour
    {
        private GameObject currentCam;

        private void Start()
        {
            currentCam = Camera.main?.gameObject;
        }

        void Update()
        {
#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView != null)
            {
                Camera temp = UnityEditor.SceneView.lastActiveSceneView.camera;
                if (temp.name.Equals("SceneCamera")) currentCam = temp.gameObject;
                else currentCam = null;
            }
#endif
            if (currentCam == null) return;
            transform.position = new Vector3(currentCam.transform.position.x, transform.position.y, currentCam.transform.position.z);
        }
    }
}

