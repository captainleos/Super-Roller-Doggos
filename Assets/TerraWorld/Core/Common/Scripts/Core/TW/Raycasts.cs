using UnityEngine;
using UnityEditor;

namespace TerraUnity.Runtime
{
    public class Raycasts
    {
        private static RaycastHit[] hits = new RaycastHit[10]; //Supports up to 10 collision checks and order detected colliders based on their distance to the ray origin. Change it to your liking if needed!

        public static bool RaycastNonAllocSorted(Ray ray, bool bypassWater, bool underWater, out RaycastHit closestHit, int layerMask = ~0, float maxDistance = Mathf.Infinity, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            closestHit = new RaycastHit();
            float closestDistance = Mathf.Infinity;
            int closestIndex = -1;
            int hitCount = Physics.RaycastNonAlloc(ray, hits, maxDistance, ~0, queryTriggerInteraction);

            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].collider == null) continue;
                int hitLayer = hits[i].transform.gameObject.layer;
                if (LayerMask.LayerToName(hitLayer) == "Water" && underWater) continue;

                if (hits[i].distance < closestDistance)
                {
                    closestDistance = hits[i].distance;
                    closestIndex = i;
                }
            }

            if (closestIndex != -1)
            {
                int hitLayer = hits[closestIndex].transform.gameObject.layer;
                if (LayerMask.LayerToName(hitLayer) == "Water" && bypassWater) return false;

                if (layerMask == (layerMask | (1 << hitLayer)))
                {
                    closestHit = hits[closestIndex];
                    return true;
                }
            }

            return false;
        }

        public static bool RaycastNonAllocSortedFirstHitDistance(Ray ray, bool bypassWater, bool underWater, out RaycastHit closestHit, out float firstHitDistance, int layerMask = ~0, float maxDistance = Mathf.Infinity, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            closestHit = new RaycastHit();
            firstHitDistance = Mathf.Infinity;
            int closestIndex = -1;
            int hitCount = Physics.RaycastNonAlloc(ray, hits, maxDistance, ~0, queryTriggerInteraction);

            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].collider == null) continue;
                int hitLayer = hits[i].transform.gameObject.layer;
                if (LayerMask.LayerToName(hitLayer) == "Water" && underWater) continue;

                if (hits[i].distance < firstHitDistance)
                {
                    firstHitDistance = hits[i].distance;
                    closestIndex = i;
                }
            }

            if (closestIndex != -1)
            {
                int hitLayer = hits[closestIndex].transform.gameObject.layer;
                if (LayerMask.LayerToName(hitLayer) == "Water" && bypassWater) return false;

                if (layerMask == (layerMask | (1 << hitLayer)))
                {
                    closestHit = hits[closestIndex];
                    return true;
                }
            }

            return false;
        }

        public static bool RaycastNonAlloc(Ray ray, out RaycastHit hit, int layerMask = ~0, float maxDistance = Mathf.Infinity, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            hit = new RaycastHit();
            int hitCount = Physics.RaycastNonAlloc(ray, hits, maxDistance, ~0, queryTriggerInteraction);

            if (hitCount > 0)
            {
                hit = hits[0];
                return true;
            }
            else
                return false;
        }

#if UNITY_EDITOR
        public static bool GetMouseWorldPosition(Vector2 mouseScreenPos, out Vector3 mouseWorldPos)
        {
            mouseWorldPos = Vector3.zero;
            float ppp = EditorGUIUtility.pixelsPerPoint;
            mouseScreenPos.x *= ppp;
            mouseScreenPos.y = SceneView.lastActiveSceneView.camera.pixelHeight - mouseScreenPos.y * ppp;
            Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mouseScreenPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                mouseWorldPos = hit.point;
                return true;
            }

            return false;
        }
#endif
    }
}

