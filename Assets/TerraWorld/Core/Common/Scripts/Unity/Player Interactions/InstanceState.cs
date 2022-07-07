using UnityEngine;

namespace TerraUnity.Runtime
{
    public class InstanceState : MonoBehaviour
    {
        public PlayerInteractions PI;
        public TScatterParams SP;
        public Vector2Int instanceIndex;
        public Matrix4x4 initialMatrix;
        //public bool isRemoved;
        //public bool isUpdated;
        //public bool isDisabled;

        private void OnDestroy()
        {
            DestroyAndClone();
        }

        public Matrix4x4 GetCurrentMatrix ()
        {
            return transform.localToWorldMatrix;
        }

        private void DestroyAndClone ()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
#endif

            if (!Application.isPlaying) return;

            // Remove matrix from patch
            Vector2 instancePos2D = new Vector2(transform.position.x, transform.position.z);
            int index, row, col;

            if (SP.GetPatchesRowCol(instancePos2D, out index, out row, out col))
                SP._patches[index].matrices.Remove(initialMatrix);

            // Clone current instance dummy and set back to instances list
            GameObject go = Instantiate(gameObject);
            InstanceState state = go.GetComponent<InstanceState>();
            PI.instanceObjects[instanceIndex.x, instanceIndex.y] = go;
            PI.instanceStates[instanceIndex.x, instanceIndex.y] = state;
            PI.incomingMatrices[instanceIndex.x].Remove(initialMatrix);
            go.SetActive(false);
        }
    }
}

