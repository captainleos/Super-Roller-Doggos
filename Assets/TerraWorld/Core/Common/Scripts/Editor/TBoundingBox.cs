/*
using UnityEngine;

namespace TerraUnity.Edittime
{
    [ExecuteAlways]
    public class TBoundingBox //: MonoBehaviour
    {
        //public static TTerraWorldGraph worldGraph;
        private static string boundingBoxName = "Bounding Box";
        public static GameObject boundingBox;
        private static int scaleX, scaleZ, offsetX, offsetZ;
        //private static TTerraWorldGraph terraWorldGraph;
        private const float everestHeight = 8848f;
        private static float minBoxSize = 10;

        public static void CreateBoundingBox ()
        {
            if (boundingBox == null)
            {
                TResourcesManager.LoadBoundingBoxResources();
                boundingBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boundingBox.name = boundingBoxName;
                boundingBox.hideFlags = HideFlags.HideAndDontSave;
                boundingBox.GetComponent<MeshRenderer>().sharedMaterial = TResourcesManager.boundingBoxMaterial;
                boundingBox.GetComponent<BoxCollider>().enabled = false;
            }
        }
        public static void UpdateBoundingBox ()
        {
            TArea area = TTerraWorld.Area;
            if (THelpersUI.ActiveNode.areaBounds.top > TTerraWorld.Area._top) THelpersUI.ActiveNode.areaBounds.top = area._top-0.00001;
            if (THelpersUI.ActiveNode.areaBounds.left < TTerraWorld.Area._left) THelpersUI.ActiveNode.areaBounds.left = TTerraWorld.Area._left+0.00001;
            if (THelpersUI.ActiveNode.areaBounds.bottom < TTerraWorld.Area._bottom) THelpersUI.ActiveNode.areaBounds.bottom = TTerraWorld.Area._bottom+0.00001;
            if (THelpersUI.ActiveNode.areaBounds.right > TTerraWorld.Area._right) THelpersUI.ActiveNode.areaBounds.right = TTerraWorld.Area._right-0.00001;
            if (THelpersUI.ActiveNode.areaBounds.minElevation < (TAreaPreview.MinElevation-50)) THelpersUI.ActiveNode.areaBounds.minElevation = TAreaPreview.MinElevation-50;
            if (THelpersUI.ActiveNode.areaBounds.maxElevation > (TAreaPreview.MaxElevation+50)) THelpersUI.ActiveNode.areaBounds.maxElevation = TAreaPreview.MaxElevation+50;

            TArea cropArea = new TArea(THelpersUI.ActiveNode.areaBounds.top, THelpersUI.ActiveNode.areaBounds.left, THelpersUI.ActiveNode.areaBounds.bottom, THelpersUI.ActiveNode.areaBounds.right);
            int widthInMeters = (int)(TAreaPreview.areaWidthMeters * TTerraWorldGraph.scaleFactor);
            int lengthInMeters = (int)(TAreaPreview.areaLengthMeters * TTerraWorldGraph.scaleFactor);
            TUtils.GeoToPixelCrop(TTerraWorld.Area, cropArea, widthInMeters, lengthInMeters, out scaleX, out scaleZ, out offsetX, out offsetZ);
            float centerPositionY = THelpersUI.ActiveNode.areaBounds.minElevation + (THelpersUI.ActiveNode.areaBounds.maxElevation - THelpersUI.ActiveNode.areaBounds.minElevation ) / 2f;

            if (boundingBox == null) CreateBoundingBox();

            boundingBox.transform.localScale = new Vector3
            (
                scaleX,
                THelpersUI.ActiveNode.areaBounds.maxElevation - THelpersUI.ActiveNode.areaBounds.minElevation ,
                scaleZ
            );

            boundingBox.transform.position = new Vector3
            (
                offsetX + (scaleX / 2) - (widthInMeters / 2f),
                centerPositionY+500,
                offsetZ + (scaleZ / 2) - (lengthInMeters / 2f)
            );



            boundingBox.SetActive(true);
            //SetBounds();

        }

        public static void SetBounds()
        {
            if (boundingBox == null)
                return;
            int widthInMeters = (int)(TTerraWorld.Area._areaSizeLon  * 1000 * TTerraWorldGraph.scaleFactor);
            int lengthInMeters = (int)(TTerraWorld.Area._areaSizeLat * 1000 * TTerraWorldGraph.scaleFactor);

            //Limit bounding box within the world boundaries

            if (Mathf.Abs(boundingBox.transform.position.x) > (widthInMeters / 2f) - minBoxSize)
            {
                if(boundingBox.transform.position.x < 0)
                    boundingBox.transform.position = new Vector3(-(widthInMeters / 2f) + (minBoxSize * 2), boundingBox.transform.position.y, boundingBox.transform.position.z);
                else
                    boundingBox.transform.position = new Vector3((widthInMeters / 2f) - (minBoxSize * 2), boundingBox.transform.position.y, boundingBox.transform.position.z);
            
                return;
            }

            if (Mathf.Abs(boundingBox.transform.position.z) > lengthInMeters - minBoxSize)
            {
                if (boundingBox.transform.position.z < 0)
                    boundingBox.transform.position = new Vector3(boundingBox.transform.position.x, boundingBox.transform.position.y, -lengthInMeters + (minBoxSize * 2));
                else
                    boundingBox.transform.position = new Vector3(boundingBox.transform.position.x, boundingBox.transform.position.y, lengthInMeters - (minBoxSize * 2));

                return;
            }

            if (boundingBox.transform.localScale.x <= minBoxSize)
                boundingBox.transform.localScale = new Vector3(minBoxSize, boundingBox.transform.localScale.y, boundingBox.transform.localScale.z);

            if (boundingBox.transform.localScale.y <= minBoxSize)
                boundingBox.transform.localScale = new Vector3(boundingBox.transform.localScale.x, minBoxSize, boundingBox.transform.localScale.z);

            if (boundingBox.transform.localScale.z <= minBoxSize)
                boundingBox.transform.localScale = new Vector3(boundingBox.transform.localScale.x, boundingBox.transform.localScale.y, minBoxSize);

            if (Mathf.Abs(boundingBox.transform.position.x * 2) + boundingBox.transform.localScale.x > widthInMeters)
            {
                boundingBox.transform.localScale = new Vector3
                (
                    widthInMeters - Mathf.Abs(boundingBox.transform.position.x * 2),
                    boundingBox.transform.localScale.y,
                    boundingBox.transform.localScale.z
                );
            }

            if (Mathf.Abs(boundingBox.transform.position.z * 2) + boundingBox.transform.localScale.z > lengthInMeters)
            {
                boundingBox.transform.localScale = new Vector3
                (
                    boundingBox.transform.localScale.x,
                    boundingBox.transform.localScale.y,
                    lengthInMeters - Mathf.Abs(boundingBox.transform.position.z * 2)
                );
            }

            float positionTop = boundingBox.transform.position.z + (boundingBox.transform.localScale.z / 2f) + (widthInMeters / 2f);
            float positionBot = boundingBox.transform.position.z - (boundingBox.transform.localScale.z / 2f) + (widthInMeters / 2f);
            float positionLft = boundingBox.transform.position.x - (boundingBox.transform.localScale.x / 2f) + (lengthInMeters / 2f);
            float positionRgt = boundingBox.transform.position.x + (boundingBox.transform.localScale.x / 2f) + (lengthInMeters / 2f);
            TGlobalPoint coordTopLft = TUtils.PixelToLatLon(TTerraWorld.Area, widthInMeters, lengthInMeters, positionLft, positionTop);
            TGlobalPoint coordBotRgt = TUtils.PixelToLatLon(TTerraWorld.Area, widthInMeters, lengthInMeters, positionRgt, positionBot);

            THelpersUI.ActiveNode.areaBounds.top = coordTopLft.latitude;
            THelpersUI.ActiveNode.areaBounds.left = coordTopLft.longitude;
            THelpersUI.ActiveNode.areaBounds.bottom = coordBotRgt.latitude;
            THelpersUI.ActiveNode.areaBounds.right = coordBotRgt.longitude;
           // THelpersUI.ActiveNode.areaBounds.minElevation = (boundingBox.transform.position.y -500 - (boundingBox.transform.localScale.y / 2));
           // THelpersUI.ActiveNode.areaBounds.maxElevation = boundingBox.transform.position.y -500 + (boundingBox.transform.localScale.y / 2);

            if(THelpersUI.ActiveNode != null)
            {
                boundingBox.SetActive(true);
            }
        }

        public static void ResetBoundingBox ()
        {
            if (UnityEditor.EditorUtility.DisplayDialog("RESET BOUNDS", "Are you sure you want to reset bounding box?", "No", "Yes")) return;
            THelpersUI.ActiveNode.ResetAreaBound();
            UpdateBoundingBox();
        }

        private void Update()
        {
         //   if(THelpersUI.ActiveNode != null) SetBounds();
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
*/

