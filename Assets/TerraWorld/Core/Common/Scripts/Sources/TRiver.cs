#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Xml;

namespace TerraUnity.Edittime
{
    public class TRiver
    {
        string _id;
        string _riverName;

        public Quaternion rotation;
        public List<Vector3> nodePositions;
        public List<string> nodeRef;
        float _widthFactor;
        float _heightFactor;
        //Vector2 _terrainSize;
        Vector4 _mapBounds;

        public TRiver(TLinearObject river, float widthFactor, float heightFactor, Vector2 terrainSize, Vector4 mapBounds, Terrain terrain, TTerrain tTerrainData)
        {
            _id = river.id.ToString();
            nodePositions = new List<Vector3>();
            //_terrainSize = terrainSize;
            _widthFactor = widthFactor;
            _heightFactor = heightFactor;
            _mapBounds = mapBounds;
            this._riverName = river.name;

            addNodes(terrain, tTerrainData, river);
        }

        private void addNodes(Terrain terrain, TTerrain tTerrainData, TLinearObject river)
        {
            //   float lastHeight = float.MaxValue;
            double pixelXNormal = 0;
            double pixelYNormal = 0;
            int pixelX = 0;
            int pixelY = 0;
            //int pixelXold = 0;
            //int pixelYold = 0;

            for (int i = 0; i < river.Points.Count; i++)
            {
                tTerrainData.Map.GetNearestIndex(river.Points[i], out pixelX, out pixelY);
                tTerrainData.Map.GetNormalizedIndex(river.Points[i], out pixelXNormal, out pixelYNormal);
                // if (pixelX != pixelXold || pixelY != pixelYold)
                {
                    double normalizedX = pixelX * 1.0f / (tTerrainData.Map.Heightmap.heightsData.GetLength(0));
                    double normalizedZ = pixelY * 1.0f / (tTerrainData.Map.Heightmap.heightsData.GetLength(1));
                    float worldPosY = terrain.terrainData.GetInterpolatedHeight((float)normalizedX, (float)normalizedZ) + terrain.transform.position.y;

                    float worldPosX = (float)(pixelXNormal * terrain.terrainData.size.x + terrain.transform.position.x);
                    float worldPosZ = (float)(pixelYNormal * terrain.terrainData.size.z + terrain.transform.position.z);

                    for (int u = -1; u < 2; u++)
                    {
                        for (int k = -1; k < 2; k++)
                        {
                            float Y1 = terrain.terrainData.GetHeight((pixelX + u), (pixelY + k)) + terrain.transform.position.y;
                            if (worldPosY > Y1) worldPosY = Y1;
                        }
                    }


                    nodePositions.Add(new Vector3(worldPosX, worldPosY, worldPosZ));
                }
            }
        }

        public List<Vector3> getNodes()
        {
            return nodePositions;
        }

        public Vector3 getNode(int index)
        {
            return nodePositions[index];
        }

        public string id
        {
            get { return this._id; }
        }

        public string riverName
        {
            set { this._riverName = value; }
            get { return this._riverName; }
        }

        public float widthFactor
        {
            get { return this._widthFactor; }
        }

        public float heightFactor
        {
            get { return this._heightFactor; }
        }
    }
}
#endif
#endif

