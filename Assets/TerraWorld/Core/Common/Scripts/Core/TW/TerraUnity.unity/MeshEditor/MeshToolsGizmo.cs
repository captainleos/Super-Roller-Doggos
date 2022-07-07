#if UNITY_EDITOR
using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteInEditMode]
    public class MeshToolsGizmo : MonoBehaviour
    {
        private static float CURRENT_SIZE = 3f;
        [HideInInspector] public MeshTools _parent;
        [HideInInspector] public int _vertexIndex;
        private Vector3 lastPosition;

        void Update()
        {
            if (lastPosition != transform.localPosition)
            {
                _parent.verts[_vertexIndex] = transform.localPosition;
                _parent.meshDerty = true;
                lastPosition = transform.localPosition;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, CURRENT_SIZE);
        }
    }
}
#endif

