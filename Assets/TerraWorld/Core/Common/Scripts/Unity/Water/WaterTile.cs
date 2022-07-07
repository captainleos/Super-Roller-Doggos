using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class WaterTile : MonoBehaviour
    {
        public PlanarReflection reflection;
        public WaterBase waterBase;
        private Camera currentCam;

        public void Start()
        {
            AcquireComponents();
        }

        void AcquireComponents()
        {
            if (!reflection)
            {
                if (transform.parent)
                    reflection = transform.parent.GetComponent<PlanarReflection>();
                else
                    reflection = transform.GetComponent<PlanarReflection>();
            }

            if (!waterBase)
            {
                if (transform.parent)
                    waterBase = transform.parent.GetComponent<WaterBase>();
                else
                    waterBase = transform.GetComponent<WaterBase>();
            }
        }
        public void Update()
        {
            if (Application.isEditor)
                AcquireComponents();
        }

        public void OnWillRenderObject ()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                if (!Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView != null)
                {
                    Camera temp = UnityEditor.SceneView.lastActiveSceneView.camera;

                    if (temp.name.Equals("SceneCamera"))
                        currentCam = temp;
                    else
                        currentCam = null;
                }
                else
                    currentCam = Camera.main;
            }
#else
        //    else
                currentCam = Camera.main;
#endif

            if (reflection)
                reflection.WaterTileBeingRendered(transform, currentCam);

            if (waterBase)
                waterBase.WaterTileBeingRendered(transform, currentCam);
        }
    }
}

