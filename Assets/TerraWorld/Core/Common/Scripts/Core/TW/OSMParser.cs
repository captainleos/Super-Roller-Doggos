#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace TerraUnity.Edittime
{
    public class SideIndex
    {
        public int areaIndex;
        public PointType pointType;

        public SideIndex()
        {
            this.areaIndex = -1;
            this.pointType = PointType.InSide;
        }

        public SideIndex(int areaIndex, PointType pointType)
        {
            this.areaIndex = areaIndex;
            this.pointType = pointType;
        }
    }

    public class IntersectionNodes
    {
        public TGlobalPoint inPoint;
        public TGlobalPoint outPoint;
        public int inPointID;
        public int outPointID;
        public IntersectionNodes(bool initialize)
        {
            this.inPoint = new TGlobalPoint(-1000, -1000);
            this.outPoint = new TGlobalPoint(-1000, -1000);
            this.inPointID = -1;
            this.outPointID = -1;
        }
    }

    public class OSMParser
    {
        private static XmlDocument doc = null;
        public static LatLonNode[] nodes;
        public static Way[] ways;
        public static Relation[] relations;
        private static List<long> nodeIDs;
        private static List<long> wayIDs;
        private static List<long> relationIDs;
        public static double isInitialized = 0;

        public static List<T2DObject> areaBiomes;
        //public static TAreaBounds sAreaBounds;
        public static TArea sAreaBounds;
        public static List<T2DPoint> areaPoints;
        public static List<TLine> areaLines;
        public static double areaLengthHorizental;
        public static double areaLengthVertical;
        public static double partArealengthHorizental;
        public static double partArealengthVertical;
        public static bool firstAreaPointPassedFlag;
        public static int AreaBorderPointsCount;
        public static double areaPointId;

        static List<TGlobalPoint> intersectionPointX = new List<TGlobalPoint>();
        static List<TGlobalPoint> intersectionPointX1 = new List<TGlobalPoint>();
        static List<TGlobalPoint> intersectionPointX2 = new List<TGlobalPoint>();
        static List<IntersectionNodes> intersectionNodes = new List<IntersectionNodes>();

        public static double CalcDistanceTwoPoints(T2DPoint p1, T2DPoint p2)
        {
            double dx = p2.x - p1.x;
            double dy = p2.y - p1.y;
            double dis = (float)Math.Sqrt(dx * dx + dy * dy);

            return dis;
        }

        public static double CalcDistanceTwoPoints(float x1, float y1, float x2, float y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            double dis = (float)Math.Sqrt(dx * dx + dy * dy);

            return dis;
        }

        public static void CreateAreaPointsAndLines()
        {
            if (areaPoints.Count == 0)
            {
                double areaWidth = TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLon * 1000d;
                double areaHeight = TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLat * 1000d;
                double everyNthMeters = 100;
                AreaBorderPointsCount = (int)((areaWidth + areaHeight) / 2d / everyNthMeters);
                if (AreaBorderPointsCount < 100) AreaBorderPointsCount = 100;
                firstAreaPointPassedFlag = false;
                areaLengthHorizental = sAreaBounds._right - sAreaBounds._left;
                areaLengthVertical = sAreaBounds._top - sAreaBounds._bottom;
                partArealengthHorizental = areaLengthHorizental / (AreaBorderPointsCount - 1);
                partArealengthVertical = areaLengthVertical / (AreaBorderPointsCount - 1);
                areaPoints.Clear();
                areaLines.Clear();

                // Right Border Points
                for (int j = 0; j < AreaBorderPointsCount - 1; j++)
                {
                    areaPoints.Add(new T2DPoint(sAreaBounds._right, sAreaBounds._top - j * partArealengthVertical));
                    if (areaPoints.Count > 1) areaLines.Add(new TLine(areaPoints[areaPoints.Count - 2], areaPoints[areaPoints.Count - 1]));
                }

                // Bottom Border Points
                for (int j = 0; j < AreaBorderPointsCount - 1; j++)
                {
                    areaPoints.Add(new T2DPoint(sAreaBounds._right - j * partArealengthHorizental, sAreaBounds._bottom));
                    areaLines.Add(new TLine(areaPoints[areaPoints.Count - 2], areaPoints[areaPoints.Count - 1]));
                }

                // Left Border Points
                for (int j = 0; j < AreaBorderPointsCount - 1; j++)
                {
                    areaPoints.Add(new T2DPoint(sAreaBounds._left, sAreaBounds._bottom + j * partArealengthVertical));
                    areaLines.Add(new TLine(areaPoints[areaPoints.Count - 2], areaPoints[areaPoints.Count - 1]));
                }

                // Top Border Points
                for (int j = 0; j < AreaBorderPointsCount - 1; j++)
                {
                    areaPoints.Add(new T2DPoint(sAreaBounds._left + j * partArealengthHorizental, sAreaBounds._top));
                    areaLines.Add(new TLine(areaPoints[areaPoints.Count - 2], areaPoints[areaPoints.Count - 1]));
                }

                // Close Area
                areaPoints.Add(areaPoints[0]);
                areaLines.Add(new TLine(areaPoints[areaPoints.Count - 2], areaPoints[areaPoints.Count - 1]));
            }
        }

        public static void ParseOSMFile(string fileName)
        {
            doc.Load(new XmlTextReader(fileName));
            ParseOSM(doc);
        }

        public static void ParseOSM(XmlDocument doc)
        {
            int count;

            XmlNodeList elemList = doc.GetElementsByTagName("node");
            XmlNodeList wayList = doc.GetElementsByTagName("way");
            XmlNodeList relationsList = doc.GetElementsByTagName("relation");

            nodes = new LatLonNode[elemList.Count];
            ways = new Way[wayList.Count];
            relations = new Relation[relationsList.Count];

            count = 0;
            foreach (XmlNode attr in elemList)
            {
                nodes[count] = new LatLonNode(long.Parse(attr.Attributes["id"].InnerText), double.Parse(attr.Attributes["lat"].InnerText), double.Parse(attr.Attributes["lon"].InnerText));
                count++;
            }

            count = 0;
            foreach (XmlNode node in wayList)
            {
                XmlNodeList wayNodes = node.ChildNodes;
                ways[count] = new Way(long.Parse(node.Attributes["id"].InnerText));

                foreach (XmlNode nd in wayNodes)
                {
                    //if (nd.Attributes[0].Name == "ref")
                    if (nd.Name == "nd")
                        ways[count].wNodes.Add(long.Parse(nd.Attributes["ref"].InnerText));
                    else if (nd.Name == "tag")
                        ways[count]._tags.Add(nd.Attributes["k"].InnerText, nd.Attributes["v"].InnerText);
                }
                count++;
            }

            count = 0;
            foreach (XmlNode node in relationsList)
            {
                XmlNodeList relationNodes = node.ChildNodes;
                relations[count] = new Relation(long.Parse(node.Attributes["id"].InnerText));

                foreach (XmlNode nd in relationNodes)
                {
                    if (nd.Name == "member")
                    {
                        Member member = new Member();
                        member.type = nd.Attributes["type"].InnerText;
                        member.nodeRef = long.Parse(nd.Attributes["ref"].InnerText);
                        member.role = nd.Attributes["role"].InnerText;

                        relations[count].members.Add(member);
                    }
                    else if (nd.Name == "tag")
                        relations[count].tags.Add(nd.Attributes["k"].InnerText, nd.Attributes["v"].InnerText);
                }
                count++;
            }
        }

        public static void Initialize(XmlDocument landcoverData, TArea fullArea)
        {
            if (isInitialized != landcoverData.GetHashCode())
            {
                if (fullArea != null) sAreaBounds = fullArea;
                ParseOSM(landcoverData);
                nodeIDs = new List<long>();
                wayIDs = new List<long>();
                relationIDs = new List<long>();
                areaPoints = new List<T2DPoint>();
                areaLines = new List<TLine>();
                for (int i = 0; i < nodes.Length; i++) nodeIDs.Add(nodes[i].id);
                nodeIDs.Sort();
                for (int i = 0; i < ways.Length; i++) wayIDs.Add(ways[i].id);
                wayIDs.Sort();
                for (int i = 0; i < relations.Length; i++) relationIDs.Add(relations[i].id);
                relationIDs.Sort();
                isInitialized = landcoverData.GetHashCode();
                areaPointId = 0;
            }

            CreateAreaPointsAndLines();
        }

        // Landcover Outputs
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        //public static void ExtractLakes(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TAreaBounds areaBounds, TArea fullArea)
        //{
        //    Initialize(LandcoverData, fullArea);
        //    UpdateWaterEntitiesList(ref closedAreaBiomes, (TLandcoverKey.water).ToString(), (TLandcoverValue.lake).ToString());
        //}
        //
        //public static void ExtractOceans(XmlDocument LandcoverData, ref List<T2DObject> oceanAreaBiomes, TAreaBounds areaBounds, TArea fullArea)
        //{
        //    Initialize(LandcoverData, fullArea);
        //      //      UpdateOpenEntitiesList(ref oceanAreaBiomes, (TLandcoverKey.natural).ToString(), (TLandcoverValue.coastline).ToString());
        //    UpdateOpenEntitiesList(ref oceanAreaBiomes, (TLandcoverKey.water).ToString(), (TLandcoverValue.sea).ToString());
        //}
        //
        //public static void ExtractForest(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TAreaBounds areaBounds, TArea fullArea)
        //{
        //    Initialize(LandcoverData, fullArea);
        //    UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.landuse).ToString(), (TLandcoverValue.forest).ToString());
        //}
        //
        //public static void ExtractWood(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TAreaBounds areaBounds, TArea fullArea)
        //{
        //    Initialize(LandcoverData, fullArea);
        //    UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.natural).ToString(), (TLandcoverValue.wood).ToString());
        //}
        //
        //public static void ExtractMeadow(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TAreaBounds areaBounds, TArea fullArea)
        //{
        //    Initialize(LandcoverData, fullArea);
        //    UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.landuse).ToString(), (TLandcoverValue.meadow).ToString());
        //}
        //
        //public static void ExtractOrchard(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TAreaBounds areaBounds, TArea fullArea)
        //{
        //    Initialize(LandcoverData, fullArea);
        //    UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.landuse).ToString(), (TLandcoverValue.orchard).ToString());
        //}
        //
        //public static void ExtractGrass(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TAreaBounds areaBounds, TArea fullArea)
        //{
        //    Initialize(LandcoverData, fullArea);
        //    UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.landuse).ToString(), (TLandcoverValue.grass).ToString());
        //}
        //
        //public static void ExtractGreenField(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TAreaBounds areaBounds, TArea fullArea)
        //{
        //    Initialize(LandcoverData, fullArea);
        //    UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.landuse).ToString(), (TLandcoverValue.greenfield).ToString());
        //}
        //
        //public static void ExtractRivers(XmlDocument LandcoverData, ref List<TLinearObject> LinearBiomes, TAreaBounds areaBounds, TArea fullArea)
        //{
        //    Initialize(LandcoverData, fullArea);
        //    UpdateOpenWaysEntitiesList(ref LinearBiomes, (TLandcoverKey.waterway).ToString(), (TLandcoverValue.river).ToString());
        //    ExtractOpenRelationBiomes(LandcoverData, ref LinearBiomes, TLandcoverKey.water, TLandcoverValue.river);
        //}
        //
        //public static void ExtractWetland(XmlDocument LandcoverData, ref List<T2DObject> wetlandAreaBiomes, TAreaBounds areaBounds, TArea fullArea)
        //{
        //    Initialize(LandcoverData, fullArea);
        //    UpdateClosedEntitiesList(ref wetlandAreaBiomes, (TLandcoverKey.natural).ToString(), (TLandcoverValue.wetland).ToString());
        //}
        //
        //public static void ExtractBeach(XmlDocument LandcoverData, ref List<T2DObject> beachAreaBiomes, TAreaBounds areaBounds, TArea fullArea)
        //{
        //    Initialize(LandcoverData, fullArea);
        //    UpdateClosedEntitiesList(ref beachAreaBiomes, (TLandcoverKey.natural).ToString(), (TLandcoverValue.beach).ToString());
        //}
        //
        //public static void ExtractBay(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TAreaBounds areaBounds, TArea fullArea)
        //{
        //    Initialize(LandcoverData, fullArea);
        //    UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.natural).ToString(), (TLandcoverValue.bay).ToString());
        //}

        public static void ExtractLakes(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TArea fullArea)
        {
            Initialize(LandcoverData, fullArea);
            //  UpdateWaterEntitiesList(ref closedAreaBiomes, (TLandcoverKey.water).ToString(), (TLandcoverValue.lake).ToString());
            //    UpdateOpenEntitiesList(ref closedAreaBiomes, (TLandcoverKey.water).ToString(), (TLandcoverValue.lake).ToString());

            // UpdateOpenEntitiesList(ref closedAreaBiomes, (TLandcoverKey.water).ToString(), (TLandcoverValue.lake).ToString());
            UpdateLakesEntitiesList(ref closedAreaBiomes, (TLandcoverValue.lake).ToString());
        }

        public static void ExtractOceans(XmlDocument LandcoverData, ref List<T2DObject> oceanAreaBiomes, TArea fullArea)
        {
            Initialize(LandcoverData, fullArea);
            List<T2DObject> temp = new List<T2DObject>();
            //            UpdateOpenEntitiesList(ref oceanAreaBiomes, (TLandcoverKey.natural).ToString(), (TLandcoverValue.coastline).ToString());
            UpdateOpenEntitiesList(ref oceanAreaBiomes, (TLandcoverKey.water).ToString(), (TLandcoverValue.sea).ToString());
            // UpdateLakesEntitiesList(ref temp, (TLandcoverValue.lake).ToString());
            //AddOuterVertexForLakeToEntityList(temp, ref oceanAreaBiomes, (TLandcoverValue.lake).ToString());
            //UpdateOpenEntitiesList(ref oceanAreaBiomes, (TLandcoverKey.water).ToString(), (TLandcoverValue.lake).ToString());
        }

        public static void ExtractForest(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TArea fullArea)
        {
            Initialize(LandcoverData, fullArea);
            UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.landuse).ToString(), (TLandcoverValue.forest).ToString());
        }

        public static void ExtractWood(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TArea fullArea)
        {
            Initialize(LandcoverData, fullArea);
            UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.natural).ToString(), (TLandcoverValue.wood).ToString());
        }

        public static void ExtractMeadow(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TArea fullArea)
        {
            Initialize(LandcoverData, fullArea);
            UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.landuse).ToString(), (TLandcoverValue.meadow).ToString());
        }

        public static void ExtractOrchard(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TArea fullArea)
        {
            Initialize(LandcoverData, fullArea);
            UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.landuse).ToString(), (TLandcoverValue.orchard).ToString());
        }

        public static void ExtractGrass(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TArea fullArea)
        {
            Initialize(LandcoverData, fullArea);
            UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.landuse).ToString(), (TLandcoverValue.grass).ToString());
        }

        public static void ExtractGreenField(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TArea fullArea)
        {
            Initialize(LandcoverData, fullArea);
            UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.landuse).ToString(), (TLandcoverValue.greenfield).ToString());
        }

        public static void ExtractRivers(XmlDocument LandcoverData, ref List<TLinearObject> LinearBiomes, TArea fullArea)
        {
            Initialize(LandcoverData, fullArea);
            //   UpdateEntitiesOfType(ref LinearBiomes, (TLandcoverValue.river).ToString());
            UpdateOpenWaysEntitiesList(ref LinearBiomes, (TLandcoverKey.waterway).ToString(), (TLandcoverValue.river).ToString());
            ExtractOpenRelationBiomes(LandcoverData, ref LinearBiomes, TLandcoverKey.water, TLandcoverValue.river);
           // UpdateOpenWaysEntitiesList(ref LinearBiomes, (TLandcoverKey.waterway).ToString(), (TLandcoverValue.stream).ToString());
           // ExtractOpenRelationBiomes(LandcoverData, ref LinearBiomes, TLandcoverKey.water, TLandcoverValue.stream);
        }

        public static void ExtractWetland(XmlDocument LandcoverData, ref List<T2DObject> wetlandAreaBiomes, TArea fullArea)
        {
            Initialize(LandcoverData, fullArea);
            UpdateClosedEntitiesList(ref wetlandAreaBiomes, (TLandcoverKey.natural).ToString(), (TLandcoverValue.wetland).ToString());
        }

        public static void ExtractBeach(XmlDocument LandcoverData, ref List<T2DObject> beachAreaBiomes, TArea fullArea)
        {
            Initialize(LandcoverData, fullArea);
            UpdateClosedEntitiesList(ref beachAreaBiomes, (TLandcoverKey.natural).ToString(), (TLandcoverValue.beach).ToString());
        }

        public static void ExtractBay(XmlDocument LandcoverData, ref List<T2DObject> closedAreaBiomes, TArea fullArea)
        {
            Initialize(LandcoverData, fullArea);
            UpdateClosedEntitiesList(ref closedAreaBiomes, (TLandcoverKey.natural).ToString(), (TLandcoverValue.bay).ToString());
        }

        public static void ExtractOpenRelationBiomes(XmlDocument LandcoverData, ref List<TLinearObject> LinearBiomes, TLandcoverKey landcoverKey, TLandcoverValue landcoverValue)
        {
            List<long> Ids;
            List<string> names;
            List<TGlobalPoint[]> Points = GetOpenRelationsCenterPoints(landcoverKey.ToString(), landcoverValue.ToString(), out Ids, out names);

            for (int i = 0; i < Points.Count; i++)
            {
                TLinearObject BiomePath = new TLinearObject();
                BiomePath.name = names[i];
                BiomePath.id = Ids[i];

                for (int j = 0; j < Points[i].Length; j++)
                    BiomePath.Points.Add(Points[i][j]);

                LinearBiomes.Add(BiomePath);
            }
        }

        private static void UpdateWaterEntitiesList(ref List<T2DObject> closedWaysEntity, string type, string subType)
        {
            List<T2DObject> waysEntity = new List<T2DObject>();

            if (closedWaysEntity == null)
                closedWaysEntity = new List<T2DObject>();

            CreateWaterWaysEntity(ref waysEntity, type, subType);


            ConnectClosedWaysEntity(waysEntity, ref closedWaysEntity, subType);


        }

        private static void UpdateEntitiesOfType(ref List<TLinearObject> rels, string subtype)
        {
            if (rels == null) rels = new List<TLinearObject>();

            string strLake = (TLandcoverValue.lake).ToString();
            string strNatural = (TLandcoverKey.natural).ToString();
            string strWater = (TLandcoverKey.water).ToString();
            string strWaterWay = (TLandcoverKey.waterway).ToString();
            string strRiver = (TLandcoverValue.river).ToString();
            string strRiverBank = (TLandcoverValue.river).ToString();
            string strSea = (TLandcoverValue.sea).ToString();

            for (int i = 0; i < relations.Length; i++)
            {
                Relation relation = relations[i];

                if (IsType(relation.key(strWater), strRiver) || IsType(relation.key(strWater), strRiverBank) || (IsType(relation.key(strNatural), strWater) &&
                    !IsType(relation.key(strWater), strSea) && !IsType(relation.key(strWater), strLake)))
                {
                    //  TDebug.LogInfoToUnityUI("riveeerr:   " + getRelationName(relation));
                }
            }
        }

        private static void UpdateLakesEntitiesList(ref List<T2DObject> closedWaysEntity, string subType)
        {
            if (closedWaysEntity == null) closedWaysEntity = new List<T2DObject>();

            string strLake = (TLandcoverValue.lake).ToString();
            string strNatural = (TLandcoverKey.natural).ToString();
            string strWater = (TLandcoverKey.water).ToString();
            string strWaterWay = (TLandcoverKey.waterway).ToString();
            string strRiver = (TLandcoverValue.river).ToString();
            string strRiverBank = (TLandcoverValue.river).ToString();
            string strSea = (TLandcoverValue.sea).ToString();

            for (int i = 0; i < relations.Length; i++)
            {
                Relation relation = relations[i];

                if (IsType(relation.key(strWater), strLake) || (IsType(relation.key(strNatural), strWater) &&
                    !IsType(relation.key(strWater), strSea) && !IsType(relation.key(strWater), strRiver) &&
                    !IsType(relation.key(strWaterWay), strRiver) && !IsType(relation.key(strWaterWay), strRiverBank)))
                {

                    T2DObject entity = RelationEntity2T2DObject(relation);

                    if (entity != null && entity.AroundPoints.Count >= 4)

                    {

                        entity.property = GetPolygonProperty(entity.AroundPoints, subType);
                        if (entity.name == "")
                        {
                            entity.name = subType;
                            entity.name += "_" + entity.id;
                        }
                        if (intersectionNodes.Count > 0)
                        {
                            PrepareForAddingCorners(ref entity);

                        }
                        intersectionNodes = new List<IntersectionNodes>();
                        //  TDebug.LogInfoToUnityUI("added:   " + entity.name);
                        closedWaysEntity.Add(entity);

                    }

                }
            }

            for (int i = 0; i < ways.Length; i++)
            {
                Way way = ways[i];

                if (IsType(way.key(strWater), strLake) ||
                    (IsType(way.key("natural"), "water") && !IsType(way.key("water"), "river") &&
                    !IsType(way.key("waterway"), "river") && !IsType(way.key("waterway"), "riverbank")))
                {
                    T2DObject entity = WayEntity2T2DObject(way);
                    if (entity != null && entity.AroundPoints.Count >= 4)

                    {
                        entity.property = GetPolygonProperty(entity.AroundPoints, subType);
                        if (entity.name == "")
                        {
                            entity.name = subType;
                            entity.name += "_" + entity.id;
                        }
                        //    TDebug.LogInfoToUnityUI("added:   " + entity.name);
                        // entity = AddExtraPoints(entity);
                        if (intersectionNodes.Count > 0)
                        {
                            PrepareForAddingCorners(ref entity);
                        }
                        intersectionNodes = new List<IntersectionNodes>();
                        closedWaysEntity.Add(entity);

                    }
                }
            }
            // AddOuterToEntityList(closedWaysEntity, ref openWaysEntity, subType);

        }

        private static void PrepareForAddingCorners(ref T2DObject entity)
        {
            int index1 = 0;
            int index2 = 0;
            int insertIndex = 0;
            bool isFirstInsertedCloserToX1 = false;

            //   TDebug.LogInfoToUnityUI("order of nodes:    "+intersectionNodes.Count);
            for (int i = 0; i < intersectionNodes.Count; i++)
            {
                if (intersectionNodes[i].inPointID == -1)
                {
                    intersectionNodes[i].inPoint = intersectionNodes[i].outPoint;
                }
                if (intersectionNodes[i].outPointID == -1)
                {
                    intersectionNodes[i].outPoint = intersectionNodes[i].inPoint;
                }

            }
            List<int> indexesIn = new List<int>();
            indexesIn.Add(GetClosestPoint(intersectionNodes[0].inPoint, entity.AroundPoints));

            List<int> indexesOut = new List<int>();
            indexesOut.Add(GetClosestPoint(intersectionNodes[0].outPoint, entity.AroundPoints));
            int intersectionCount = 0;
            if (intersectionNodes.Count > 1)
            {
                for (int k = 1; k < intersectionNodes.Count; k++)
                {
                    //                    TDebug.LogInfoToUnityUI("x1:   " + k + "   " + entity.AroundPoints.IndexOf(intersectionNodes[k].inPoint) + "   x2:   " + entity.AroundPoints.IndexOf(intersectionNodes[k].outPoint));
                    if (Mathf.Abs(entity.AroundPoints.IndexOf(intersectionNodes[k].inPoint) - entity.AroundPoints.IndexOf(intersectionNodes[k - 1].outPoint)) == 1
                        && (entity.AroundPoints.IndexOf(intersectionNodes[k].outPoint) != -1))
                    {
                        indexesOut[intersectionCount] = (entity.AroundPoints.IndexOf(intersectionNodes[k].outPoint));

                    }
                    else
                    {
                        intersectionCount++;
                        indexesIn.Add(entity.AroundPoints.IndexOf(intersectionNodes[k].inPoint));
                        indexesOut.Add(entity.AroundPoints.IndexOf(intersectionNodes[k].outPoint));
                    }

                }
            }
            //for (int i = 0; i < indexesIn.Count; i++)
            //{
            //    TDebug.LogInfoToUnityUI(entity.name + "   " + i + "   " + indexesIn[i] + "   " + indexesOut[i]);
            //}



            // TDebug.LogInfoToUnityUI("end  order of nodes:    ");

            for (int k = 0; k < indexesIn.Count; k++)
            {
                //  if (intersectionNodes[k].inPointID == -1 || intersectionNodes[k].outPointID == -1)
                //    continue;

                index2 = indexesOut[k];// entity.AroundPoints.IndexOf(intersectionNodes[k].outPoint);// GetClosestPoint(intersectionPointX[1], entity.AroundPoints);
                index1 = indexesIn[k];// entity.AroundPoints.IndexOf(intersectionNodes[k].inPoint);// GetClosestPoint(intersectionPointX[0], entity.AroundPoints);
                if (index1 == -1 || index2 == -1 || (index1 == index2))
                    continue;
                //  TDebug.LogInfoToUnityUI(k + "   " +entity.name+"   "+ index1 + "   " + index2 + "   " + entity.AroundPoints.Count);
                //orientation: node ha be tartibi chide shode and ke az  0 ta index1 jahate barakse aab hast (
                //  agar index 1 0 bashe iani hame hamjahat hastand va agar index 1 ieki munde be axar bashad iaani
                //hame baraaks hastand (dar in surat baiad jahate xalaf ro hesaab kard va baraks kard)
                int majority_orientation = 0;
                if (index1 < index2)
                    majority_orientation = 1;

                else
                    majority_orientation = 2;


                if (index1 > index2)
                {
                    int temp = index2;
                    index2 = index1;
                    index1 = temp;
                }
                insertIndex = index2;
                if ((index1 != 0 && index2 != 0 && index2 - index1 > 1))
                {
                    TGlobalPoint temp = entity.AroundPoints[index1];
                    entity.AroundPoints.RemoveAt(index1);
                    entity.AroundPoints.Insert(0, temp);


                    index1 = 0;
                    // index2 = 1;
                    insertIndex = 0;
                    isFirstInsertedCloserToX1 = true;


                }
                else if ((index1 == 0 && index2 != entity.AroundPoints.Count - 1))
                {
                    TGlobalPoint temp = entity.AroundPoints[index2];
                    entity.AroundPoints.RemoveAt(index2);
                    entity.AroundPoints.Add(temp);
                    index2 = entity.AroundPoints.Count - 1;

                    isFirstInsertedCloserToX1 = true;
                    insertIndex = 0;
                }
                else if (index1 == 0 && index2 == entity.AroundPoints.Count - 1)
                {
                    isFirstInsertedCloserToX1 = true;
                    insertIndex = 0;
                }

                //      TDebug.LogInfoToUnityUI("callliiinnnggggg    " + entity.name + "   " + index1 + "   " + index2 + "   " + entity.AroundPoints.Count + "   " + intersectionPointX1.Count + "   " + intersectionPointX2.Count);

                entity.AroundPoints = CheckAndAddCornerPoints(entity.AroundPoints, entity.AroundPoints[index1], entity.AroundPoints[index2], insertIndex, isFirstInsertedCloserToX1, majority_orientation);
            }

            intersectionPointX1 = new List<TGlobalPoint>();
            intersectionPointX2 = new List<TGlobalPoint>();
            intersectionPointX = new List<TGlobalPoint>();

        }



        private static List<TGlobalPoint> CheckAndAddCornerPoints(List<TGlobalPoint> entity, TGlobalPoint x1, TGlobalPoint x2, int insertIndex, bool isFirstInsertedCloserToX1, int lakeOrientation)
        {
            double dist = GetDistanceBetweenPoints(x1.latitude, x1.longitude, x2.latitude, x2.longitude) * 2;
            //int maxIndex = -1;

            List<TGlobalPoint> cornerTestPolygon = new List<TGlobalPoint>();
            cornerTestPolygon.Add(x1);
            cornerTestPolygon.Add(x2);

            List<TGlobalPoint> pointsToAdd = new List<TGlobalPoint>();



            pointsToAdd = new List<TGlobalPoint>();

            if (x1.longitude != x2.longitude && x1.latitude != x2.latitude)
            {
                if (orientation(new TGlobalPoint(sAreaBounds._bottom, sAreaBounds._left), x1, x2) == lakeOrientation)

                {
                    // TDebug.LogInfoToUnityUI("bottomleft:   " + orientation(x1, new TGlobalPoint(sAreaBounds._bottom, sAreaBounds._left), x2));

                    pointsToAdd.Add(new TGlobalPoint(sAreaBounds._bottom, sAreaBounds._left,
                                                              0,
                                                              x1.pointType,
                                                              0));


                }

                if (orientation(new TGlobalPoint(sAreaBounds._bottom, sAreaBounds._right), x1, x2) == lakeOrientation)
                {
                    //   TDebug.LogInfoToUnityUI("bottomright:   " + orientation(x1, new TGlobalPoint(sAreaBounds._bottom, sAreaBounds._right), x2));
                    pointsToAdd.Add(new TGlobalPoint(sAreaBounds._bottom, sAreaBounds._right,
                                                                  3,
                                                                  x1.pointType,
                                                                  0));


                }
                //  TDebug.LogInfoToUnityUI("top right ore:  " + orientation(new TGlobalPoint(sAreaBounds._top, sAreaBounds._right), x1, x2));
                if (orientation(new TGlobalPoint(sAreaBounds._top, sAreaBounds._right), x1, x2) == lakeOrientation)
                {
                    //   TDebug.LogInfoToUnityUI("topright:   " + orientation(x1, new TGlobalPoint(sAreaBounds._top, sAreaBounds._right), x2));
                    pointsToAdd.Add(new TGlobalPoint(sAreaBounds._top, sAreaBounds._right,
                                                                  2,
                                                                  x1.pointType,
                                                                  0));

                }
                //   TDebug.LogInfoToUnityUI("top left ore:  " + orientation(new TGlobalPoint(sAreaBounds._top, sAreaBounds._left), x1, x2));
                if (orientation(new TGlobalPoint(sAreaBounds._top, sAreaBounds._left), x1, x2) == lakeOrientation)
                {
                    //   TDebug.LogInfoToUnityUI("top left ore:  " + orientation(new TGlobalPoint(sAreaBounds._top, sAreaBounds._left), x1, x2));
                    pointsToAdd.Add(new TGlobalPoint(sAreaBounds._top, sAreaBounds._left,
                                                              1,
                                                              x1.pointType,
                                                              0));


                }


            }



            int count = pointsToAdd.Count;
            //   TDebug.LogInfoToUnityUI("points:  " + count);
            if (count > 1)
            {
                bool intersection = ((isFirstInsertedCloserToX1 && DoPointsInterect(x1, pointsToAdd[0], pointsToAdd[count - 1], x2)) ||
                    (isFirstInsertedCloserToX1 == false && DoPointsInterect(x1, pointsToAdd[count - 1], pointsToAdd[0], x2)));


                if (intersection == true)
                {
                    //   TDebug.LogInfoToUnityUI("intersect");
                    for (int k = count - 1; k >= 0; k--)
                    {

                        entity.Insert(insertIndex, pointsToAdd[k]);

                    }
                }
                else
                {
                    //    TDebug.LogInfoToUnityUI("not intersect");
                    for (int k = 0; k < count; k++)
                    {

                        entity.Insert(insertIndex, pointsToAdd[k]);

                    }
                }
            }
            else
            {
                for (int k = 0; k < count; k++)
                {

                    entity.Insert(insertIndex, pointsToAdd[k]);

                }
            }


            return entity;
        }

        private static bool DoPointsInterect(TGlobalPoint p1, TGlobalPoint q1, TGlobalPoint p2, TGlobalPoint q2)
        {
            int o1 = orientation(p1, q1, p2);
            int o2 = orientation(p1, q1, q2);
            int o3 = orientation(p2, q2, p1);
            int o4 = orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && onSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && onSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && onSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && onSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }

        private static bool onSegment(TGlobalPoint p, TGlobalPoint q, TGlobalPoint r)
        {
            if (q.longitude <= Math.Max(p.longitude, r.longitude) && q.longitude >= Math.Min(p.longitude, r.longitude) &&
                q.latitude <= Math.Max(p.latitude, r.latitude) && q.latitude >= Math.Min(p.latitude, r.latitude))
                return true;

            return false;
        }
        private static int orientation(TGlobalPoint p, TGlobalPoint q, TGlobalPoint r)
        {

            double val = (((q.latitude - p.latitude) * (r.longitude - q.longitude) -
                      (q.longitude - p.longitude) * (r.latitude - q.latitude)));

            if (val == 0) return 0;  // colinear 

            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }



        private static int GetClosestPoint(TGlobalPoint reference, List<TGlobalPoint> points)
        {
            double minDist = 100000000000;
            int index = -1;
            double d;
            for (int i = 0; i < points.Count; i++)
            {
                //                TDebug.LogInfoToUnityUI(reference.latitude + "   " + reference.longitude + "   " + points[i].latitude + "   " + points[i].longitude);
                d = GetDistanceBetweenPoints(reference.latitude, reference.longitude, points[i].latitude, points[i].longitude);

                if (d < minDist)
                {
                    minDist = d;
                    index = i;

                }
            }


            return index;
        }



        private static double GetDistanceBetweenPoints(double lat1, double lon1, double lat2, double lon2)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
                dist = Math.Acos(dist);
                dist = rad2deg(dist);
                dist = dist * 60 * 1.1515;

                dist = dist * 1.609344;

                return (dist);
            }
        }

        private static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }


        private static void UpdateClosedEntitiesList(ref List<T2DObject> closedWaysEntity, string type, string subType)
        {

            List<T2DObject> waysEntity = new List<T2DObject>();
            List<T2DObject> connectedWaysEntity = new List<T2DObject>();

            if (closedWaysEntity == null)
                closedWaysEntity = new List<T2DObject>();

            CreateClosedWaysEntity(ref waysEntity, type, subType);
            ConnectClosedWaysEntity(waysEntity, ref closedWaysEntity, subType);

            //areaBiomes = new List<T2DObject>();
            //for (int i = 0; i < closedWaysEntity.Count; i++)
            //    areaBiomes.Add(closedWaysEntity[i]);
        }

        private static void UpdateOpenEntitiesList(ref List<T2DObject> openWaysEntity, string type, string subType)
        {
            List<T2DObject> waysEntity = new List<T2DObject>();
            List<T2DObject> connectedWaysEntity = new List<T2DObject>();

            if (openWaysEntity == null)
                openWaysEntity = new List<T2DObject>();

            CreateOpenWaysEntity(ref waysEntity, type, subType);
            ConnectOpenWaysEntity(waysEntity, ref connectedWaysEntity, subType);
            AddOuterToEntityList(connectedWaysEntity, ref openWaysEntity, subType);

            //areaBiomes = new List<T2DObject>();
            //for (int i = 0; i < openWaysEntity.Count; i++)
            //    areaBiomes.Add(openWaysEntity[i]);
        }

        private static void AddOuterToEntityList(List<T2DObject> waysEntity, ref List<T2DObject> openWaysEntity, string subType)
        {
            bool innerExistFlag = false;
            bool outerExistFlag = false;

            if (subType == (TLandcoverValue.sea).ToString())
            {
                for (int i = 0; i < waysEntity.Count; i++)
                {
                    openWaysEntity.Add(waysEntity[i]);

                    if (waysEntity[i].property == Property.Outer)
                        outerExistFlag = true;
                    else if (waysEntity[i].property == Property.Inner)
                        innerExistFlag = true;
                }

                if (outerExistFlag == false && innerExistFlag == true)
                {
                    int index;
                    List<TGlobalPoint> aroundPoints = new List<TGlobalPoint>();

                    for (int i = 0; i < 4; i++)
                    {
                        index = i * (AreaBorderPointsCount - 1);
                        aroundPoints.Add(new TGlobalPoint(areaPoints[index].y, areaPoints[index].x, index));
                    }

                    AddWaysEntity(aroundPoints, ref openWaysEntity, 0, (subType + "_" + aroundPoints.Count).ToString(), Property.Outer);
                }
            }
            else
            {
                for (int i = 0; i < waysEntity.Count; i++)
                    openWaysEntity.Add(waysEntity[i]);
            }
        }



        private static void CreateWaterWaysEntity(ref List<T2DObject> waysEntity, string type, string subType)
        {
            int index;
            Way way = new Way();
            Relation relation = new Relation();
            List<long> wayId = new List<long>();
            string strNatural = (TLandcoverKey.natural).ToString();
            string strWater = (TLandcoverKey.water).ToString();
            string strWaterWay = (TLandcoverKey.waterway).ToString();
            string strRiver = (TLandcoverValue.river).ToString();
            string strRiverBank = (TLandcoverValue.river).ToString();
            string strSea = (TLandcoverValue.sea).ToString();

            for (int i = 0; i < relations.Length; i++)
            {
                relation = relations[i];

                if (IsType(relation.key(type), subType) || (IsType(relation.key(strNatural), strWater) &&
                    !IsType(relation.key(strWater), strSea) && !IsType(relation.key(strWater), strRiver) &&
                    !IsType(relation.key(strWaterWay), strRiver) && !IsType(relation.key(strWaterWay), strRiverBank)))
                {
                    T2DObject entity = new T2DObject();
                    entity.name = getRelationName(relation);

                    for (int j = 0; j < relation.members.Count; j++)
                    {
                        Member member = relation.members[j];
                        index = wayIDs.BinarySearch(member.nodeRef);

                        if (index >= 0)
                        {
                            way = ways[index];
                            wayId.Add(way.id);
                            //AddEntityPoints(way, ref entity);
                            AddWayPoints(way, ref waysEntity, entity.name, PolygonConType.Closed);
                        }
                    }
                    //waysEntity.Add(entity);
                }
            }

            wayId.Sort();

            for (int i = 0; i < ways.Length; i++)
            {
                way = ways[i];

                if (IsType(way.key(type), subType) ||
                    (IsType(way.key("natural"), "water") && !IsType(way.key("water"), "river") &&
                    !IsType(way.key("waterway"), "river") && !IsType(way.key("waterway"), "riverbank")))
                {
                    if (wayId.BinarySearch(way.id) < 0)
                    {
                        T2DObject entity = new T2DObject();
                        entity.name = getWayName(way);
                        wayId.Add(way.id);
                        //AddEntityPoints(way, ref entity);
                        //waysEntity.Add(entity);
                        AddWayPoints(way, ref waysEntity, entity.name, PolygonConType.Closed);
                    }
                }
            }
        }

        private static T2DObject WayEntity2T2DObject(Way way)
        {
            T2DObject result = new T2DObject();
            result.id = way.id;
            result.name = getWayName(way);

            for (int k = 0; k < way.wNodes.Count; k++)
            {
                int index = nodeIDs.BinarySearch(way.wNodes[k]);

                if (index >= 0)
                {
                    TGlobalPoint point = new TGlobalPoint((double)nodes[index].lat, (double)nodes[index].lon, nodes[index].id);
                    result.AroundPoints.Add(point);
                }
            }

            return result;

        }

        private static T2DObject RelationEntity2T2DObject(Relation relation)
        {
            T2DObject result = new T2DObject();
            result.id = relation.id;
            result.name = getRelationName(relation);

            for (int j = 0; j < relation.members.Count; j++)
            {

                Member member = relation.members[j];

                if (member.type == "way")
                {

                    int index = wayIDs.BinarySearch(member.nodeRef);
                    if (index >= 0)
                    {
                        Way way = ways[index];

                        if (String.IsNullOrEmpty(member.role) || member.role == "outer")
                        {

                            List<T2DObject> areaPart = new List<T2DObject>();
                            AddWayPoints(way, ref areaPart, getWayName(way), PolygonConType.Closed);
                            // result.AddToAroundpoints(areaPart.AroundPoints);
                            for (int i = 0; i < areaPart.Count; i++)
                            {

                                for (int m = 0; m < areaPart[i].AroundPoints.Count; m++)
                                {
                                    result.AroundPoints.Add(areaPart[i].AroundPoints[m]);/*   new TGlobalPoint(areaPart[i].AroundPoints[m].latitude,
                                                                          areaPart[i].AroundPoints[m].longitude,
                                                                          areaPart[i].AroundPoints[m].id,
                                                                          areaPart[i].AroundPoints[m].pointType,
                                                                          areaPart[i].AroundPoints[m].areaIndex));*/
                                }
                            }

                        }
                        else if (member.type == "way" && member.role == "inner")
                        {
                            T2DObject hole = WayEntity2T2DObject(way);
                            result.Holes.Add(hole);
                        }
                    }
                }

                if (member.type == "relation")
                {
                    int index = relationIDs.BinarySearch(member.nodeRef);
                    if (index >= 0)
                    {
                        Relation _relation = relations[index];
                        T2DObject areaPart = RelationEntity2T2DObject(_relation);
                        // result.AddToAroundpoints(areaPart.AroundPoints);
                        for (int m = 0; m < areaPart.AroundPoints.Count; m++)
                        {
                            result.AroundPoints.Add(areaPart.AroundPoints[m]);/* new TGlobalPoint(areaPart.AroundPoints[m].latitude,
                                                                  areaPart.AroundPoints[m].longitude,
                                                                  areaPart.AroundPoints[m].id,
                                                                  areaPart.AroundPoints[m].pointType,
                                                                  areaPart.AroundPoints[m].areaIndex));*/
                        }
                        for (int i = 0; i < areaPart.Holes.Count; i++)
                        {
                            result.Holes.Add(areaPart.Holes[i]);
                        }

                    }
                }
            }
            return result;
        }

        private static string getWayName(Way way)
        {
            string name = "";

            way._tags.TryGetValue("name:en", out name);
            if (name == null)
                way._tags.TryGetValue("name", out name);

            return name;
        }

        private static string getRelationName(Relation relation)
        {
            string name = "";

            relation.tags.TryGetValue("name:en", out name);
            if (name == null)
                relation.tags.TryGetValue("name", out name);

            return name;
        }

        private static void CreateClosedWaysEntity(ref List<T2DObject> waysEntity, string type, string subType)
        {
            int index;
            string name;
            Way way = new Way();
            Relation relation = new Relation();
            List<long> wayId = new List<long>();

            for (int i = 0; i < relations.Length; i++)
            {
                relation = relations[i];

                if (IsType(relation.key(type), subType))
                {
                    name = getRelationName(relation);
                    for (int j = 0; j < relation.members.Count; j++)
                    {
                        Member member = relation.members[j];
                        index = wayIDs.BinarySearch(member.nodeRef);
                        if (index >= 0)
                        {
                            way = ways[index];
                            wayId.Add(way.id);
                            AddWayPoints(way, ref waysEntity, name, PolygonConType.Closed);
                        }
                    }
                }
            }

            wayId.Sort();

            for (int i = 0; i < ways.Length; i++)
            {
                way = ways[i];

                if (IsType(way.key(type), subType))
                {
                    if (wayId.BinarySearch(way.id) < 0)
                    {
                        name = getWayName(way);
                        wayId.Add(way.id);
                        AddWayPoints(way, ref waysEntity, name, PolygonConType.Closed);
                    }
                }
            }
        }

        private static void CreateOpenWaysEntity(ref List<T2DObject> waysEntity, string type, string subType)
        {
            int index;
            string name;
            Way way = new Way();
            Relation relation = new Relation();
            List<long> wayId = new List<long>();

            for (int i = 0; i < relations.Length; i++)
            {
                relation = relations[i];

                if (IsType(relation.key(type), subType))
                {
                    name = getRelationName(relation);

                    for (int j = 0; j < relation.members.Count; j++)
                    {
                        Member member = relation.members[j];
                        index = wayIDs.BinarySearch(member.nodeRef);

                        if (index >= 0)
                        {
                            way = ways[index];
                            wayId.Add(way.id);
                            AddWayPoints(way, ref waysEntity, name, PolygonConType.Open);
                        }
                    }
                }
            }

            wayId.Sort();

            if (type == (TLandcoverKey.water).ToString() && subType == (TLandcoverValue.sea).ToString())
            {
                type = (TLandcoverKey.natural).ToString();
                subType = (TLandcoverValue.coastline).ToString();
            }

            for (int i = 0; i < ways.Length; i++)
            {
                way = ways[i];

                if (IsType(way.key(type), subType))
                {
                    if (wayId.BinarySearch(way.id) < 0)
                    {
                        name = getWayName(way);
                        AddWayPoints(way, ref waysEntity, name, PolygonConType.Open);
                    }
                }
            }
        }

        private static void AddWayPoints(Way way, ref List<T2DObject> waysEntity, string name, PolygonConType polygonConType)
        {
            bool intersectFlag = false;
            int prevIndex, index, indexAreaIntersect;
            List<TGlobalPoint> aroundPoints = new List<TGlobalPoint>();
            T2DPoint areaIntersectPoint = new T2DPoint();
            TGlobalPoint x1 = new TGlobalPoint();
            TGlobalPoint x2 = new TGlobalPoint();
            //int index1 = 0;
            //int index2 = 0;
            prevIndex = -1;
            intersectFlag = false;
            int icount = 0;

            for (int k = 0; k < way.wNodes.Count; k++)
            {
                index = nodeIDs.BinarySearch(way.wNodes[k]);

                if (index >= 0)
                {
                    TGlobalPoint point = new TGlobalPoint((double)nodes[index].lat, (double)nodes[index].lon, nodes[index].id,
                                                                                                            PointType.InSide, -1);
                    if (k > 0)
                    {
                        if (prevIndex < 0)
                        {
                            intersectFlag = true;
                            indexAreaIntersect = FindIntersectionOfWayPointsToAreaBound(point, ref areaIntersectPoint);
                            x1 = new TGlobalPoint(areaIntersectPoint.y, areaIntersectPoint.x, ++areaPointId,
                                                                                     PointType.InputSide, indexAreaIntersect); ;
                            //     TDebug.LogInfoToUnityUI("x1 node:   " + aroundPoints.Count+"    "+intersectionNodes.Count);
                            /*  if(intersectionPointX1.Count==0)
                              {
                                  intersectionPointX1.Add(x1);
                              }
                            */
                            if (intersectionNodes.Count > 0)
                            {
                                if (intersectionNodes.Last().inPointID == -1)
                                {
                                    intersectionNodes.Last().inPointID = aroundPoints.Count;
                                    intersectionNodes.Last().inPoint = x1;
                                }

                                else
                                {
                                    IntersectionNodes temp = new IntersectionNodes(true);
                                    temp.inPoint = x1;
                                    temp.inPointID = aroundPoints.Count;
                                    temp.outPointID = -1;
                                    intersectionNodes.Add(temp);
                                }


                            }
                            else
                            {
                                IntersectionNodes temp = new IntersectionNodes(true);
                                temp.inPoint = x1;
                                temp.inPointID = aroundPoints.Count;
                                temp.outPointID = -1;
                                intersectionNodes.Add(temp);
                            }


                            aroundPoints.Add(x1);



                        }
                    }

                    aroundPoints.Add(point);
                }
                else // index < 0
                {
                    if (k > 0)
                    {
                        if (prevIndex >= 0)
                        {
                            intersectFlag = true;
                            TGlobalPoint point = new TGlobalPoint((double)nodes[prevIndex].lat, (double)nodes[prevIndex].lon,
                                                                                        nodes[prevIndex].id, PointType.InSide, -1);
                            indexAreaIntersect = FindIntersectionOfWayPointsToAreaBound(point, ref areaIntersectPoint);
                            x2 = new TGlobalPoint(areaIntersectPoint.y, areaIntersectPoint.x, ++areaPointId,
                                                                                        PointType.OutputSide, indexAreaIntersect);
                            //   TDebug.LogInfoToUnityUI("x2 node:   " + aroundPoints.Count+"    "+ intersectionNodes.Count);
                            icount++;

                            aroundPoints.Add(x2);
                            if (intersectionNodes.Count > 0)
                            {
                                // TDebug.LogInfoToUnityUI("last in x2:   " + aroundPoints.Count+"   "+ intersectionNodes[intersectionNodes.Count-1].outPointID + "   " + intersectionNodes[intersectionNodes.Count - 1].inPointID);
                                if ((intersectionNodes.Last().outPointID != -1 && intersectionNodes.Last().inPointID != -1))
                                {

                                    IntersectionNodes temp = new IntersectionNodes(true);
                                    temp.outPoint = x2;
                                    temp.outPointID = aroundPoints.Count;
                                    temp.inPointID = -1;
                                    intersectionNodes.Add(temp);
                                    //   TDebug.LogInfoToUnityUI("x2 node added:   " + intersectionNodes.Last().outPointID + "   " + intersectionNodes.Last().inPointID);
                                }

                                else if (intersectionNodes.Last().outPointID == -1 || intersectionNodes.Last().inPointID == -1)
                                {
                                    intersectionNodes.Last().outPoint = x2;
                                    intersectionNodes.Last().outPointID = aroundPoints.Count;
                                }

                            }
                            else
                            {
                                IntersectionNodes temp = new IntersectionNodes(true);
                                temp.outPoint = x2;
                                temp.outPointID = aroundPoints.Count;
                                temp.inPointID = -1;
                                intersectionNodes.Add(temp);
                            }




                        }
                    }
                }

                prevIndex = index;
            }


            if (aroundPoints.Count > 1)
            {
                if (intersectFlag == true && polygonConType == PolygonConType.Open)
                    GetEntityFromAroundPoint(aroundPoints, ref waysEntity, way.id, name);
                else
                {





                    AddWaysEntity(aroundPoints, ref waysEntity, way.id, name);

                }
            }
        }


        private static void GetEntityFromAroundPoint(List<TGlobalPoint> aroundPoints, ref List<T2DObject> waysEntity, long id, string name)
        {

            int i, j;
            List<TGlobalPoint> arrangedAroundPoints = new List<TGlobalPoint>();
            List<SideIndex> indexSide = new List<SideIndex>();

            for (i = 0; i < aroundPoints.Count; i++)
            {
                if (aroundPoints[i].pointType == PointType.InputSide)
                    indexSide.Add(new SideIndex(i, PointType.InputSide));
                else if (aroundPoints[i].pointType == PointType.OutputSide)
                    indexSide.Add(new SideIndex(i, PointType.OutputSide));
            }

            if (indexSide.Count > 1)
            {
                if (indexSide[0].pointType == PointType.OutputSide)
                {
                    indexSide.Add(indexSide[0]);
                    indexSide.RemoveAt(0);
                }
            }

            for (i = 0; i < indexSide.Count - 1; i += 2)
            {
                arrangedAroundPoints.Clear();

                if (indexSide[i].areaIndex < indexSide[i + 1].areaIndex)
                {
                    for (j = indexSide[i].areaIndex; j <= indexSide[i + 1].areaIndex; j++)
                        arrangedAroundPoints.Add(aroundPoints[j]);
                }
                else
                {
                    for (j = indexSide[i].areaIndex; j < aroundPoints.Count; j++)
                        arrangedAroundPoints.Add(aroundPoints[j]);

                    if (aroundPoints[0].id != aroundPoints[aroundPoints.Count - 1].id)
                    {
                        AddWaysEntity(arrangedAroundPoints, ref waysEntity, id, name);
                        arrangedAroundPoints.Clear();
                    }

                    for (j = 0; j <= indexSide[i + 1].areaIndex; j++)
                        arrangedAroundPoints.Add(aroundPoints[j]);
                }

                AddWaysEntity(arrangedAroundPoints, ref waysEntity, id, name);
            }

            if (indexSide.Count % 2 != 0)
            {
                arrangedAroundPoints.Clear();

                if (indexSide[indexSide.Count - 1].pointType == PointType.InputSide)
                {
                    for (j = indexSide[indexSide.Count - 1].areaIndex; j < aroundPoints.Count; j++)
                        arrangedAroundPoints.Add(aroundPoints[j]);
                }
                else //if (indexSide[indexSide.Count - 1].pointType == PointType.OutputSide)
                {
                    for (j = 0; j <= indexSide[indexSide.Count - 1].areaIndex; j++)
                        arrangedAroundPoints.Add(aroundPoints[j]);
                }

                AddWaysEntity(arrangedAroundPoints, ref waysEntity, id, name);
            }
        }

        private static void AddWaysEntity(List<TGlobalPoint> aroundPoints, ref List<T2DObject> waysEntity, long id, string name,
                                                                                                  Property property = Property.None)
        {
            T2DObject t2DObject = new T2DObject();

            for (int j = 0; j < aroundPoints.Count; j++)
                t2DObject.AroundPoints.Add(aroundPoints[j]);

            t2DObject.id = id;
            if (name == null)
                t2DObject.name = "";
            else
                t2DObject.name = name;

            t2DObject.property = property;
            waysEntity.Add(t2DObject);
        }

        private static void ConnectOpenWaysEntity(List<T2DObject> waysEntity, ref List<T2DObject> connectedWaysEntity, string subType)
        {
            int indexWayPoint, firstAreaIndex, inputAreaIndex, outputAreaIndex;
            double id;
            T2DPoint areaIntersectPoint = new T2DPoint();

            while (waysEntity.Count > 0)
            {
                T2DObject polygon = new T2DObject();
                polygon.name = "";
                indexWayPoint = FindFirstWayEntity(waysEntity);
                firstAreaIndex = -1;
                inputAreaIndex = -1;
                outputAreaIndex = -1;

                while (indexWayPoint >= 0)
                {
                    for (int j = 0; j < waysEntity[indexWayPoint].AroundPoints.Count; j++)
                    {
                        TGlobalPoint point = new TGlobalPoint(waysEntity[indexWayPoint].AroundPoints[j].latitude,
                                                              waysEntity[indexWayPoint].AroundPoints[j].longitude,
                                                              waysEntity[indexWayPoint].AroundPoints[j].id,
                                                              waysEntity[indexWayPoint].AroundPoints[j].pointType,
                                                              waysEntity[indexWayPoint].AroundPoints[j].areaIndex);

                        if (point.pointType == PointType.InputSide)
                        {
                            inputAreaIndex = (int)waysEntity[indexWayPoint].AroundPoints[j].areaIndex;
                            if (inputAreaIndex >= 0 && firstAreaIndex == -1)
                                firstAreaIndex = inputAreaIndex;

                            AddAreaPointsToOpenEntityStart(ref polygon.AroundPoints, outputAreaIndex, inputAreaIndex);
                        }
                        else if (point.pointType == PointType.OutputSide)
                        {
                            outputAreaIndex = (int)waysEntity[indexWayPoint].AroundPoints[j].areaIndex;
                        }
                        polygon.AroundPoints.Add(point);
                    }
                    polygon.id = waysEntity[indexWayPoint].id;
                    if (waysEntity[indexWayPoint].name != "")
                        polygon.name = waysEntity[indexWayPoint].name;
                    waysEntity.RemoveAt(indexWayPoint);
                    id = polygon.AroundPoints[polygon.AroundPoints.Count - 1].id;
                    indexWayPoint = FindNextWayEntity(waysEntity, id, outputAreaIndex, firstAreaIndex, PolygonConType.Open);
                    if (indexWayPoint < 0)
                        break;
                }
                AddAreaPointsToOpenEntityStart(ref polygon.AroundPoints, outputAreaIndex, firstAreaIndex);

                if (polygon.AroundPoints.Count >= 4)
                {
                    polygon.property = GetPolygonProperty(polygon.AroundPoints, subType);
                    if (polygon.name == "")
                        polygon.name = subType;
                    polygon.name += "_" + polygon.id;
                    connectedWaysEntity.Add(polygon);
                }
            }
        }

        private static void ConnectClosedWaysEntity(List<T2DObject> waysEntity, ref List<T2DObject> connectedWaysEntity, string subType)

        {
            int indexWayPoint, firstAreaIndex, outputAreaIndex;
            double id;
            T2DPoint areaIntersectPoint = new T2DPoint();
            T2DObject prevPolygon = new T2DObject();
            Dictionary<string, T2DObject> currentLakes = new Dictionary<string, T2DObject>();

            while (waysEntity.Count > 0)
            {
                T2DObject polygon = new T2DObject();
                polygon.name = "";
                indexWayPoint = FindFirstWayEntity(waysEntity);
                firstAreaIndex = -1;
                outputAreaIndex = -1;

                while (indexWayPoint >= 0)
                {
                    if (waysEntity[indexWayPoint].name != "")
                        polygon.name = waysEntity[indexWayPoint].name;//use the name to look up if existing
                    if (currentLakes.ContainsKey(polygon.name))
                    {
                        polygon.AroundPoints = currentLakes[polygon.name].AroundPoints;
                    }






                    for (int j = 0; j < waysEntity[indexWayPoint].AroundPoints.Count; j++)
                    {
                        polygon.AroundPoints.Add(new TGlobalPoint(waysEntity[indexWayPoint].AroundPoints[j].latitude,
                                                              waysEntity[indexWayPoint].AroundPoints[j].longitude,
                                                              waysEntity[indexWayPoint].AroundPoints[j].id,
                                                              waysEntity[indexWayPoint].AroundPoints[j].pointType,
                                                              waysEntity[indexWayPoint].AroundPoints[j].areaIndex));
                    }
                    polygon.id = waysEntity[indexWayPoint].id;


                    waysEntity.RemoveAt(indexWayPoint);
                    id = polygon.AroundPoints[polygon.AroundPoints.Count - 1].id;
                    indexWayPoint = FindNextWayEntity(waysEntity, id, outputAreaIndex, firstAreaIndex, PolygonConType.Closed);

                    if (indexWayPoint < 0)
                        break;

                }

                if (polygon.AroundPoints.Count >= 4)
                {


                    polygon.property = GetPolygonProperty(polygon.AroundPoints, subType);
                    if (currentLakes.ContainsKey(polygon.name) == false)
                    {
                        if (polygon.name == "")
                        {
                            //????
                        }
                        else
                        {
                            currentLakes.Add(polygon.name, polygon);
                        }
                    }
                    else
                    {
                        currentLakes[polygon.name].AroundPoints = polygon.AroundPoints;
                    }
                    if (polygon.name == "")
                    {
                        polygon.name = subType;
                        continue;
                    }
                    polygon.name += "_" + polygon.id;


                    // connectedWaysEntity.Add(polygon);


                }
            }
            foreach (T2DObject polygon in currentLakes.Values)
            {

                connectedWaysEntity.Add(polygon);
            }
        }

        private static void ChangePolygonRotationCCWToCW(ref List<TGlobalPoint> aroundPoints)
        {
            if (FindPolygonRotation(aroundPoints) == PolygonRotType.CCW)
                ChangePolygonRotationToCW(ref aroundPoints);
        }

        private static void ChangePolygonRotationToCW(ref List<TGlobalPoint> aroundPoints)
        {
            aroundPoints.Reverse();
            for (int i = 0; i < aroundPoints.Count; i++)
            {
                if (aroundPoints[i].pointType == PointType.InputSide || aroundPoints[i].pointType == PointType.OutputSide)
                {
                    TGlobalPoint point = new TGlobalPoint();
                    point = aroundPoints[i];
                    if (point.pointType == PointType.InputSide)
                        point.pointType = PointType.OutputSide;
                    else
                        point.pointType = PointType.InputSide;

                    aroundPoints[i] = point;
                }
            }
        }

        private static void CorrectPolygonInputOutput(ref List<TGlobalPoint> aroundPoints)
        {
            int endIndex = aroundPoints.Count - 1;
            int areaIndex;
            TGlobalPoint endPoint = aroundPoints[aroundPoints.Count - 1];

            if (aroundPoints[0].pointType == PointType.InputSide && endPoint.pointType == PointType.OutputSide)
            {
                if (Math.Abs(aroundPoints[0].areaIndex - endPoint.areaIndex) <= 5 && aroundPoints[0].areaIndex < endPoint.areaIndex)
                {
                    areaIndex = aroundPoints[0].areaIndex;
                    aroundPoints.RemoveAt(endIndex);
                    aroundPoints.Add(new TGlobalPoint(areaLines[areaIndex].p1.y, areaLines[areaIndex].p1.x, endPoint.id,
                                                      endPoint.pointType, areaIndex));
                }
            }
        }

        private static int FindNextWayEntity(List<T2DObject> waysEntity, double endId, int startAreaIndex, int firstAreaIndex,
                                                                                                           PolygonConType polygonConType)
        {
            int nextShapeIndex = -1;
            int endAreaIndex;
            int areaIndex = -1;

            for (int i = 0; i < waysEntity.Count; i++)
            {
                if (waysEntity[i].AroundPoints[0].id == endId)
                {
                    nextShapeIndex = i;
                    break;
                }
            }
            if (polygonConType == PolygonConType.Open && nextShapeIndex < 0)
            {
                if (startAreaIndex != firstAreaIndex)
                {
                    if (startAreaIndex < firstAreaIndex)
                        endAreaIndex = firstAreaIndex;
                    else
                        endAreaIndex = areaLines.Count - 1;

                    for (int i = startAreaIndex; i <= endAreaIndex; i++)
                    {
                        for (int j = 0; j < waysEntity.Count; j++)
                        {
                            if (waysEntity[j].AroundPoints[0].pointType == PointType.InputSide && waysEntity[j].AroundPoints[0].areaIndex == i)
                            {
                                areaIndex = i;
                                nextShapeIndex = j;
                                i = areaLines.Count;
                                break;
                            }
                        }
                    }
                    if (nextShapeIndex < 0 && firstAreaIndex < startAreaIndex)
                    {
                        for (int i = 0; i <= firstAreaIndex; i++)
                        {
                            for (int j = 0; j < waysEntity.Count; j++)
                            {
                                if (waysEntity[j].AroundPoints[0].pointType == PointType.InputSide && waysEntity[j].AroundPoints[0].areaIndex == i)
                                {
                                    areaIndex = i;
                                    nextShapeIndex = j;
                                    i = startAreaIndex;
                                    break;
                                }
                            }
                        }
                    }
                    if (areaIndex == firstAreaIndex)
                        nextShapeIndex = -1;
                }
            }

            return nextShapeIndex;
        }

        private static void AddAreaPointsToOpenEntityStart(ref List<TGlobalPoint> aroundPoints, int startAreaIndex, int endAreaIndex)
        {
            if (startAreaIndex >= 0 && endAreaIndex >= 0)
            {
                if (startAreaIndex < endAreaIndex)
                {
                    AddAreaPointsToOpenEntityEnd(ref aroundPoints, startAreaIndex, endAreaIndex);
                }
                else if (startAreaIndex > endAreaIndex)
                {
                    AddAreaPointsToOpenEntityEnd(ref aroundPoints, startAreaIndex, areaLines.Count);
                    aroundPoints.Add(new TGlobalPoint(areaPoints[0].y, areaPoints[0].x, 0));
                    AddAreaPointsToOpenEntityEnd(ref aroundPoints, 0, endAreaIndex);
                }
            }
        }

        private static void AddAreaPointsToOpenEntityEnd(ref List<TGlobalPoint> aroundPoints, int startAreaIndex, int endAreaIndex)
        {
            if (startAreaIndex >= 0 && endAreaIndex >= 0)
            {
                TLine line0 = areaLines[startAreaIndex];
                for (int i = startAreaIndex; i < endAreaIndex; i++)
                {
                    //                    if (Math.Abs(line0.alpha - areaLines[i].alpha) >= angleDiff)
                    //                    {
                    aroundPoints.Add(new TGlobalPoint(areaLines[i].p2.y, areaLines[i].p2.x, i, PointType.OnSide, i));
                    line0 = areaLines[i];
                    //                    }
                }
            }
        }

        private static int FindFirstWayEntity(List<T2DObject> WayEntity)
        {
            int index = -1;
            int endIndex = -1;
            int indexCoastline = 0;

            if (WayEntity.Count > 1)
            {
                for (int i = 0; i < WayEntity.Count; i++)
                {
                    index = -1;
                    for (int j = 0; j < WayEntity.Count; j++)
                    {
                        endIndex = WayEntity[j].AroundPoints.Count - 1;
                        if (WayEntity[i].AroundPoints[0].id == WayEntity[j].AroundPoints[endIndex].id)
                        {
                            index = j;
                            break;
                        }
                    }
                    if (index == -1)
                    {
                        indexCoastline = i;
                        break;
                    }
                }
            }

            return indexCoastline;
        }

        private static int FindIntersectionOfWayPointsToAreaBound(TGlobalPoint point, ref T2DPoint areaIntersectPoint)
        {

            int j;
            int areaLineIndex = -1;
            double dis = 0;
            double minDistance = 0;
            T2DPoint point0;

            point0 = new T2DPoint(point.longitude, point.latitude);

            minDistance = double.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                j = i * AreaBorderPointsCount;
                dis = areaLines[j].CalcDistancePointFromLine(point0);
                if (dis < minDistance)
                    minDistance = dis;
            }

            areaLineIndex = -1;
            for (int i = 0; i < areaLines.Count; i++)
            {
                if (areaLines[i].CalcPerpendicularIntersection(point0, ref areaIntersectPoint, minDistance))
                {
                    areaLineIndex = i;
                    break;
                }
            }

            return areaLineIndex;
        }

        private static void UpdateOpenWaysEntitiesList(ref List<TLinearObject> openWaysEntity, string type, string subType)
        {
            int index;
            Way way = new Way();

            if (openWaysEntity == null) openWaysEntity = new List<TLinearObject>();

            for (int i = 0; i < ways.Length; i++)
            {
                way = ways[i];
                string name;
                if (!way._tags.TryGetValue("name", out name)) continue;
                if (IsType(way.key(type), subType) && way.wNodes[0] != way.wNodes[way.wNodes.Count - 1])
                {
                    List<TGlobalPoint> wayPoints = new List<TGlobalPoint>();

                    for (int j = 0; j < way.wNodes.Count; j++)
                    {
                        index = nodeIDs.BinarySearch(way.wNodes[j]);
                        if (index >= 0)
                        {
                            TGlobalPoint _point = new TGlobalPoint();
                            _point.latitude = (float)nodes[index].lat;
                            _point.longitude = (float)nodes[index].lon;
                            _point.id = nodes[index].id;
                            wayPoints.Add(_point);
                        }
                    }
                    //    TDebug.LogInfoToUnityUI("river:   " + name + "   " + wayPoints.Count);
                    if (wayPoints.Count > 0)
                    {
                        TLinearObject linearObject = new TLinearObject();
                        linearObject.Points = wayPoints;
                        linearObject.id = way.id;
                        linearObject.name = name;
                        openWaysEntity.Add(linearObject);
                    }

                }
            }
        }

        private static List<TGlobalPoint[]> GetOpenRelationsCenterPoints(string type, string subType, out List<long> Ids, out List<string> names)
        {
            List<TGlobalPoint[]> Points = new List<TGlobalPoint[]>();
            Ids = new List<long>();
            names = new List<string>();
            Way way = new Way();
            Relation relation = new Relation();
            int index;

            for (int i = 0; i < relations.Length; i++)
            {
                relation = relations[i];
                string name;
                if (relation.tags.TryGetValue("name", out name))
                {
                    names.Add(name);
                }
                else
                    continue;

                if (IsType(relation.key(type), subType))
                {
                    List<TGlobalPoint> wayPoints = new List<TGlobalPoint>();

                    for (int j = 0; j < relation.members.Count; j++)
                    {
                        Member member = relation.members[j];

                        if (member.type != "way" || member.role == "outer" || member.role == "inner") continue;
                        // if (member.type != "way" ) continue;

                        index = wayIDs.BinarySearch(member.nodeRef);
                        if (index >= 0)
                        {
                            way = ways[index];
                            for (int x = 0; x < way.wNodes.Count; x++)
                            {
                                long wayNodeID = way.wNodes[x];

                                index = nodeIDs.BinarySearch(wayNodeID);
                                if (index >= 0)
                                {
                                    TGlobalPoint _point = new TGlobalPoint();
                                    _point.latitude = (float)nodes[index].lat;
                                    _point.longitude = (float)nodes[index].lon;
                                    _point.id = nodes[index].id;

                                    wayPoints.Add(_point);
                                }
                            }
                        }
                    }
                    if (wayPoints.Count > 0 && wayPoints[0].id != wayPoints[wayPoints.Count - 1].id)
                    {
                        Points.Add(wayPoints.ToArray());
                        Ids.Add(relation.id);

                    }
                }


            }

            return Points;
        }

        private static bool IsType(string type, string subType = "")
        {
            bool isTypeFlag = false;

            if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(subType))
            {
                if (type == subType)
                    isTypeFlag = true;
            }

            return isTypeFlag;
        }

        private static bool IsNameInclude(string name, string subType)
        {
            bool isTypeFlag = false;

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(subType))
            {
                if (name.ToLower().Contains(subType))
                    isTypeFlag = true;
            }

            return isTypeFlag;
        }

        private static PolygonConType FindPolygonConnection(List<TGlobalPoint> aroundPoints)
        {
            PolygonConType polygonConType = PolygonConType.None;
            int endIndex;

            if (aroundPoints.Count >= 3)
            {
                endIndex = aroundPoints.Count - 1;
                if (aroundPoints[0].longitude != aroundPoints[endIndex].longitude || aroundPoints[0].latitude != aroundPoints[endIndex].latitude)
                {
                    polygonConType = PolygonConType.Open;
                }
                else
                    polygonConType = PolygonConType.Closed;
            }

            return polygonConType;
        }

        private static Property GetPolygonProperty(List<TGlobalPoint> aroundPoints, string subType)
        {
            Property polygonProperty = Property.None;
            PolygonConType polygonConType = PolygonConType.None;

            if (subType == (TLandcoverValue.sea).ToString())
            {
                polygonConType = FindPolygonConnection(aroundPoints);
                if (polygonConType == PolygonConType.Open)
                    polygonProperty = Property.Outer;
                else if (polygonConType == PolygonConType.Closed)
                    polygonProperty = Property.Inner;
            }
            else
            {
                polygonProperty = Property.Outer;
            }

            return polygonProperty;
        }

        private static PolygonRotType FindPolygonRotation(List<TGlobalPoint> aroundPoints)
        {
            PolygonRotType polygonRotType = PolygonRotType.CCW;
            double V1x, V1y, V2x, V2y;
            double sum = 0;
            int endIndex = aroundPoints.Count - 1;

            if (aroundPoints.Count >= 3)
            {
                V1x = aroundPoints[1].longitude - aroundPoints[0].longitude;
                V1y = aroundPoints[1].latitude - aroundPoints[0].latitude;
                for (int i = 1; i < endIndex; i++)
                {
                    V2x = aroundPoints[i + 1].longitude - aroundPoints[i].longitude;
                    V2y = aroundPoints[i + 1].latitude - aroundPoints[i].latitude;
                    sum += V1x * V2y - V1y * V2x;
                    V1x = V2x;
                    V1y = V2y;
                }

                if (FindPolygonConnection(aroundPoints) == PolygonConType.Open)
                {
                    V2x = aroundPoints[0].longitude - aroundPoints[endIndex].longitude;
                    V2y = aroundPoints[0].latitude - aroundPoints[endIndex].latitude;
                    sum += V1x * V2y - V1y * V2x;
                }
                if (sum < 0)
                    polygonRotType = PolygonRotType.CW;
            }

            return polygonRotType;
        }

        private static void ClosePolygon(ref List<TGlobalPoint> aroundPoints)
        {
            int endIndex = aroundPoints.Count - 1;

            if (aroundPoints[0].longitude != aroundPoints[endIndex].longitude || aroundPoints[0].latitude != aroundPoints[endIndex].latitude)
            {
                aroundPoints.Add(aroundPoints[0]);
            }
        }
    }
}

#endif
/*
        private static int FindNextWayEntity(List<T2DObject> waysEntity, double endId)
        {
            int nextShapeIndex = -1;

            for (int i = 0; i < waysEntity.Count; i++)
            {
                if (waysEntity[i].AroundPoints[0].id == endId)
                {
                    nextShapeIndex = i;
                    break;
                }
            }

            return nextShapeIndex;
        }
        private static void ConnectClosedWaysEntity(List<T2DObject> waysEntity, ref List<T2DObject> connectedWaysEntity, string subType)
        {
            int indexWayPoint;
            double id;

            while (waysEntity.Count > 0)
            {
                T2DObject polygon = new T2DObject();
                indexWayPoint = FindFirstWayEntity(waysEntity);
                while (indexWayPoint >= 0)
                {
                    for (int j = 0; j < waysEntity[indexWayPoint].AroundPoints.Count; j++)
                    {
                        polygon.AroundPoints.Add(new TGlobalPoint(waysEntity[indexWayPoint].AroundPoints[j].latitude,
                                                              waysEntity[indexWayPoint].AroundPoints[j].longitude,
                                                              waysEntity[indexWayPoint].AroundPoints[j].id,
                                                              waysEntity[indexWayPoint].AroundPoints[j].pointType,
                                                              waysEntity[indexWayPoint].AroundPoints[j].areaIndex));
                    }
                    polygon.id = waysEntity[indexWayPoint].id;
                    polygon.name = waysEntity[indexWayPoint].name;
                    waysEntity.RemoveAt(indexWayPoint);
                    id = polygon.AroundPoints[polygon.AroundPoints.Count - 1].id;
                    indexWayPoint = FindNextWayEntity(waysEntity, id);
                    if (indexWayPoint < 0)
                        break;
                }
                if (polygon.AroundPoints.Count >= 4)
                {
                    polygon.property = GetPolygonProperty(polygon.AroundPoints, subType);
                    connectedWaysEntity.Add(polygon);
                }
            }
        }

        private static void AddAreaPointsToWaysEntity(List<T2DObject> waysEntity, ref List<T2DObject> connectedWaysEntity, string subType)
        {
            int indexWayPoint, firstAreaIndex, inputAreaIndex, outputAreaIndex;
            double id;
            T2DPoint areaIntersectPoint = new T2DPoint();

            while (waysEntity.Count > 0)
            {
                T2DObject polygon = new T2DObject();
                indexWayPoint = 0;
                firstAreaIndex = -1;
                inputAreaIndex = -1;
                outputAreaIndex = -1;
                while (indexWayPoint >= 0)
                {
                    for (int j = 0; j < waysEntity[indexWayPoint].AroundPoints.Count; j++)
                    {
                        TGlobalPoint point = new TGlobalPoint(waysEntity[indexWayPoint].AroundPoints[j].latitude,
                                                              waysEntity[indexWayPoint].AroundPoints[j].longitude,
                                                              waysEntity[indexWayPoint].AroundPoints[j].id,
                                                              waysEntity[indexWayPoint].AroundPoints[j].pointType,
                                                              waysEntity[indexWayPoint].AroundPoints[j].areaIndex);

                        if (point.pointType == PointType.InputSide)
                        {
                            inputAreaIndex = (int)waysEntity[indexWayPoint].AroundPoints[j].areaIndex;
                            if (inputAreaIndex >= 0 && firstAreaIndex == -1)
                                firstAreaIndex = inputAreaIndex;

                            AddAreaPointsToOpenEntityStart(ref polygon.AroundPoints, outputAreaIndex, inputAreaIndex);
                        }
                        else if (point.pointType == PointType.OutputSide)
                        {
                            outputAreaIndex = (int)waysEntity[indexWayPoint].AroundPoints[j].areaIndex;
                        }
                        polygon.AroundPoints.Add(point);
                    }
                    polygon.id = waysEntity[indexWayPoint].id;
                    polygon.name = waysEntity[indexWayPoint].name;
                    waysEntity.RemoveAt(indexWayPoint);
                    id = polygon.AroundPoints[polygon.AroundPoints.Count - 1].id;
                    indexWayPoint = FindNextWayEntity(waysEntity, id, outputAreaIndex, firstAreaIndex);
                    if (indexWayPoint < 0)
                        break;
                }
                AddAreaPointsToOpenEntityStart(ref polygon.AroundPoints, outputAreaIndex, firstAreaIndex);

                if (polygon.AroundPoints.Count >= 4)
                {
                    polygon.property = GetPolygonProperty(polygon.AroundPoints, subType);
                    connectedWaysEntity.Add(polygon);
                }
            }
        }
private static void ConnectClosedWaysEntity(List<T2DObject> waysEntity, ref List<T2DObject> connectedWaysEntity, string subType)
        {
            int indexWayPoint, firstAreaIndex, inputAreaIndex, outputAreaIndex, areaIndex;
            double id;
            T2DPoint areaIntersectPoint = new T2DPoint();

            while (waysEntity.Count > 0)
            {
                T2DObject polygon = new T2DObject();
                indexWayPoint = FindFirstWayEntity(waysEntity);
                firstAreaIndex = -1;
                areaIndex = -1;
                while (indexWayPoint >= 0)
                {
                    inputAreaIndex = -1;
                    outputAreaIndex = -1;
                    for (int j = 0; j < waysEntity[indexWayPoint].AroundPoints.Count; j++)
                    {
                        TGlobalPoint point = new TGlobalPoint(waysEntity[indexWayPoint].AroundPoints[j].latitude,
                                                              waysEntity[indexWayPoint].AroundPoints[j].longitude,
                                                              waysEntity[indexWayPoint].AroundPoints[j].id,
                                                              waysEntity[indexWayPoint].AroundPoints[j].pointType,
                                                              waysEntity[indexWayPoint].AroundPoints[j].areaIndex);

                        if (point.pointType == PointType.InputSide)
                        {
                            inputAreaIndex = (int)waysEntity[indexWayPoint].AroundPoints[j].areaIndex;
                            if (inputAreaIndex >= 0 && firstAreaIndex == -1)
                                firstAreaIndex = inputAreaIndex;

                            point.areaIndex = inputAreaIndex;
                        }
                        else if (point.pointType == PointType.OutputSide)
                        {
                            outputAreaIndex = (int)waysEntity[indexWayPoint].AroundPoints[j].areaIndex;
                            point.areaIndex = outputAreaIndex;
                        }
                        if (polygon.AroundPoints.Count > 0)
                        {
                            if (point.id != polygon.AroundPoints[polygon.AroundPoints.Count - 1].id)
                                polygon.AroundPoints.Add(point);
                        }
                        else
                        {
                            polygon.AroundPoints.Add(point);
                        }
                    }
                    polygon.id = waysEntity[indexWayPoint].id;
                    polygon.name = waysEntity[indexWayPoint].name;
                    waysEntity.RemoveAt(indexWayPoint);
                    id = polygon.AroundPoints[polygon.AroundPoints.Count - 1].id;
                    indexWayPoint = FindNextWayEntity(waysEntity, id, outputAreaIndex, firstAreaIndex);
                    if (indexWayPoint >= 0 && firstAreaIndex >= 0 && areaIndex == firstAreaIndex)
                        break;
                }
                if (polygon.AroundPoints.Count >= 4)
                {
                    polygon.property = GetPolygonProperty(polygon.AroundPoints, subType);
                    connectedWaysEntity.Add(polygon);
                }
            }
        }

        private static void AddAreaPointsToClosedEntityStart(ref List<TGlobalPoint> aroundPoints, int startAreaIndex, int endAreaIndex,
                                                                                        PolygonRotType polygonRotType = PolygonRotType.CW)
        {
            if (startAreaIndex >= 0 && endAreaIndex >= 0)
            {
                if (polygonRotType == PolygonRotType.CW)
                {
                    if (startAreaIndex < endAreaIndex)
                    {
                        AddAreaPointsToClosedEntityEnd(ref aroundPoints, startAreaIndex, endAreaIndex, polygonRotType);
                    }
                    else if (startAreaIndex > endAreaIndex)
                    {
                        AddAreaPointsToClosedEntityEnd(ref aroundPoints, startAreaIndex, areaLines.Count, polygonRotType);
                        aroundPoints.Add(new TGlobalPoint(areaPoints[0].y, areaPoints[0].x, 0));
                        AddAreaPointsToClosedEntityEnd(ref aroundPoints, 0, endAreaIndex, polygonRotType);
                    }
                }
                else if (polygonRotType == PolygonRotType.CCW)
                {
                    if (startAreaIndex > endAreaIndex)
                    {
                        AddAreaPointsToClosedEntityEnd(ref aroundPoints, startAreaIndex, endAreaIndex, polygonRotType);
                    }
                    else if (startAreaIndex < endAreaIndex)
                    {
                        AddAreaPointsToClosedEntityEnd(ref aroundPoints, startAreaIndex, 0, polygonRotType);
                        aroundPoints.Add(new TGlobalPoint(areaPoints[0].y, areaPoints[0].x, 0));
                        AddAreaPointsToClosedEntityEnd(ref aroundPoints, areaLines.Count - 1, endAreaIndex, polygonRotType);
                    }
                }
            }
        }

        private static void AddAreaPointsToClosedEntityEnd(ref List<TGlobalPoint> aroundPoints, int startAreaIndex, int endAreaIndex,
                                                                                       PolygonRotType polygonRotType = PolygonRotType.CW)
        {
            if (startAreaIndex >= 0 && endAreaIndex >= 0)
            {
                TLine line0 = areaLines[startAreaIndex];
                if (polygonRotType == PolygonRotType.CW)
                {
                    for (int i = startAreaIndex; i < endAreaIndex; i++)
                    {
                        if (Math.Abs(line0.alpha - areaLines[i].alpha) >= angleDiff)
                        {
                            aroundPoints.Add(new TGlobalPoint(areaLines[i].p2.y, areaLines[i].p2.x, i, PointType.OnSide, i));
                            line0 = areaLines[i];
                        }
                    }
                }
                else if (polygonRotType == PolygonRotType.CCW)
                {
                    for (int i = startAreaIndex - 1; i > endAreaIndex; i--)
                    {
                        if (Math.Abs(line0.alpha - areaLines[i].alpha) >= angleDiff)
                        {
                            aroundPoints.Add(new TGlobalPoint(areaLines[i].p2.y, areaLines[i].p2.x, i, PointType.OnSide, i));
                            line0 = areaLines[i];
                        }
                    }
                }
            }
        }
        private static void ClosePolygon(ref List<TGlobalPoint> aroundPoints)
        {
            int endIndex = aroundPoints.Count - 1;

            if (aroundPoints[0].longitude != aroundPoints[endIndex].longitude || aroundPoints[0].latitude != aroundPoints[endIndex].latitude)
            {
                aroundPoints.Add(aroundPoints[0]);
            }
        }
        private static void AddAreaPointsToClosedWaysEntity(List<T2DObject> connectedWaysEntity, ref List<T2DObject> waysEntity, string subType)
        {
            int startAreaIndex = -1;
            int endAreaIndex = -1;
            int firstAreaIndex = -1;

            PolygonRotType polygonRotType;
            T2DPoint areaIntersectPoint = new T2DPoint();

            for (int i = 0; i < connectedWaysEntity.Count; i++)
            {
                T2DObject polygon = new T2DObject();
                polygonRotType = FindPolygonRotation(connectedWaysEntity[i].AroundPoints);
                startAreaIndex = -1;
                endAreaIndex = -1;
                firstAreaIndex = -1;
                for (int j = 0; j < connectedWaysEntity[i].AroundPoints.Count; j++)
                {
                    TGlobalPoint point = new TGlobalPoint(connectedWaysEntity[i].AroundPoints[j].latitude,
                                                          connectedWaysEntity[i].AroundPoints[j].longitude,
                                                          connectedWaysEntity[i].AroundPoints[j].id,
                                                          connectedWaysEntity[i].AroundPoints[j].pointType,
                                                          connectedWaysEntity[i].AroundPoints[j].areaIndex);

                    if (point.pointType == PointType.InputSide)
                    {
                        endAreaIndex = (int)connectedWaysEntity[i].AroundPoints[j].areaIndex;
                        if (endAreaIndex >= 0 && firstAreaIndex == -1)
                            firstAreaIndex = endAreaIndex;

                        AddAreaPointsToClosedEntityStart(ref polygon.AroundPoints, startAreaIndex, endAreaIndex, polygonRotType);
                    }
                    else if (point.pointType == PointType.OutputSide)
                    {
                        startAreaIndex = (int)connectedWaysEntity[i].AroundPoints[j].areaIndex;
                    }
                    polygon.AroundPoints.Add(point);
                }
                AddAreaPointsToClosedEntityStart(ref polygon.AroundPoints, startAreaIndex, firstAreaIndex, polygonRotType);

                if (polygon.AroundPoints.Count >= 4)
                {
                    polygon.id = connectedWaysEntity[i].id;
                    polygon.name = connectedWaysEntity[i].name;
                    polygon.property = GetPolygonProperty(polygon.AroundPoints, subType);
                    waysEntity.Add(polygon);
                }
            }
        }
        private static void AddClosedWayPoints(Way way, ref List<T2DObject> waysEntity, string name = "")
        {
            int prevIndex, index, indexAreaIntersect;
            List<TGlobalPoint> aroundPoints = new List<TGlobalPoint>();
            T2DPoint areaIntersectPoint = new T2DPoint();

            if(way.wNodes[0] == way.wNodes[way.wNodes.Count - 1])
            {
                prevIndex = -1;
                for (int k = 0; k < way.wNodes.Count; k++)
                {
                    index = nodeIDs.BinarySearch(way.wNodes[k]);
                    if (index >= 0)
                    {
                        TGlobalPoint point = new TGlobalPoint((double)nodes[index].lat, (double)nodes[index].lon, nodes[index].id,
                                                                                                              PointType.InSide, -1);
                        if (k > 0)
                        {
                            if (prevIndex < 0)
                            {
                                indexAreaIntersect = FindIntersectionOfWayPointsToAreaBound(point, ref areaIntersectPoint);
                                aroundPoints.Add(new TGlobalPoint(areaIntersectPoint.y, areaIntersectPoint.x, ++areaPointId,
                                                                                            PointType.InputSide, indexAreaIntersect));
                            }
                        }
                        aroundPoints.Add(point);
                    }
                    else // index < 0
                    {
                        if (k > 0)
                        {
                            if (prevIndex >= 0)
                            {
                                TGlobalPoint point = new TGlobalPoint((double)nodes[prevIndex].lat, (double)nodes[prevIndex].lon,
                                                                                         nodes[prevIndex].id, PointType.InSide, -1);
                                indexAreaIntersect = FindIntersectionOfWayPointsToAreaBound(point, ref areaIntersectPoint);
                                aroundPoints.Add(new TGlobalPoint(areaIntersectPoint.y, areaIntersectPoint.x, ++areaPointId,
                                                                                           PointType.OutputSide, indexAreaIntersect));
                            }
                        }
                    }
                    prevIndex = index;
                }
                if (aroundPoints.Count > 1)
                {
                    GetEntityFromAroundPoint(aroundPoints, ref waysEntity, way.id, name);
                }
            }
        }
    private static void AddAreaPointsToOpenWaysEntity(List<T2DObject> connectedWaysEntity, ref List<T2DObject> completeWaysEntity, string subType)
    {
        int inputAreaIndex = -1;
        int outputAreaIndex = -1;
        int firstAreaIndex = -1;

        T2DPoint areaIntersectPoint = new T2DPoint();

        for (int i = 0; i < connectedWaysEntity.Count; i++)
        {
            T2DObject polygon = new T2DObject();
            inputAreaIndex = -1;
            outputAreaIndex = -1;
            firstAreaIndex = -1;
            for (int j = 0; j < connectedWaysEntity[i].AroundPoints.Count; j++)
            {
                TGlobalPoint point = new TGlobalPoint(connectedWaysEntity[i].AroundPoints[j].latitude,
                                                      connectedWaysEntity[i].AroundPoints[j].longitude,
                                                      connectedWaysEntity[i].AroundPoints[j].id,
                                                      connectedWaysEntity[i].AroundPoints[j].pointType,
                                                      connectedWaysEntity[i].AroundPoints[j].areaIndex);

                if (point.pointType == PointType.InputSide)
                {
                    inputAreaIndex = (int)connectedWaysEntity[i].AroundPoints[j].areaIndex;
                    if (inputAreaIndex >= 0 && firstAreaIndex == -1)
                        firstAreaIndex = inputAreaIndex;

                    AddAreaPointsToOpenEntityStart(ref polygon.AroundPoints, outputAreaIndex, inputAreaIndex);
                }
                else if (point.pointType == PointType.OutputSide)
                {
                    outputAreaIndex = (int)connectedWaysEntity[i].AroundPoints[j].areaIndex;
                }
                polygon.AroundPoints.Add(point);
            }
            AddAreaPointsToOpenEntityStart(ref polygon.AroundPoints, outputAreaIndex, firstAreaIndex);

            if (polygon.AroundPoints.Count >= 4)
            {
                polygon.id = connectedWaysEntity[i].id;
                polygon.name = connectedWaysEntity[i].name;
                polygon.property = GetPolygonProperty(polygon.AroundPoints, subType);
                completeWaysEntity.Add(polygon);
            }
        }
    }*/
/*
public static void DeformByMask(ref TMap map, TMask mask, float depth, bool applyMaskData, TNode parentNode,
                                                                         int EdgeSize, T2DObject polygon = null)
{
if (mask == null) return;

float[,] heightmap = map._refTerrain.Heightmap.heightsData;
int width = heightmap.GetLength(0);
int length = heightmap.GetLength(1);

if (EdgeSize < 0 || EdgeSize > (width / 3)) throw new Exception("Edge Size Error!");

float MinValue = 0;
float MaxValue = 0;
float AvgValue = 0;
double correctionFactor = (mask.Width - 1) * 1.0d / mask.Width;
//if (flat) GetminValue(ref heightmap, mask , out MinValue, out MaxValue, out AvgValue);

T2DPoint pixelCenterPoint = null;
float meterPerPixel = 0f;
if (polygon != null)
{
    meterPerPixel = TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLat * 1000f / width;
    polygon.Center();
    System.Numerics.Vector2 centerPointNormalized = map.GetLatLongNormalizedPositionN(polygon.center);
    pixelCenterPoint = new T2DPoint(centerPointNormalized.Y * width, centerPointNormalized.X * length);
}

for (int i = 1 + EdgeSize; i < width - EdgeSize; i++)
{
    for (int j = 1 + EdgeSize; j < length - EdgeSize; j++)
    {
        double normalI = (float)(i) * correctionFactor / width;
        double normalJ = (float)(j) * correctionFactor / length;
        bool allInMask = mask.CheckNormal(normalJ, normalI);
        if (!allInMask) continue;

        normalI = (float)(i - EdgeSize) * correctionFactor / width;
        normalJ = (float)(j + EdgeSize) * correctionFactor / length;
        allInMask = allInMask && mask.CheckNormal(normalJ, normalI);
        if (!allInMask) continue;

        normalI = (float)(i) * correctionFactor / width;
        normalJ = (float)(j + EdgeSize) * correctionFactor / length;
        allInMask = allInMask && mask.CheckNormal(normalJ, normalI);
        if (!allInMask) continue;

        normalI = (float)(i + EdgeSize) * correctionFactor / width;
        normalJ = (float)(j + EdgeSize) * correctionFactor / length;
        allInMask = allInMask && mask.CheckNormal(normalJ, normalI);
        if (!allInMask) continue;

        normalI = (float)(i - EdgeSize) * correctionFactor / width;
        normalJ = (float)(j) * 1.0d / length;
        allInMask = allInMask && mask.CheckNormal(normalJ, normalI);
        if (!allInMask) continue;

        normalI = (float)(i + EdgeSize) * correctionFactor / width;
        normalJ = (float)(j) * correctionFactor / length;
        allInMask = allInMask && mask.CheckNormal(normalJ, normalI);
        if (!allInMask) continue;

        normalI = (float)(i - EdgeSize) * correctionFactor / width;
        normalJ = (float)(j - EdgeSize) * correctionFactor / length;
        allInMask = allInMask && mask.CheckNormal(normalJ, normalI);
        if (!allInMask) continue;

        normalI = (float)(i) * correctionFactor / width;
        normalJ = (float)(j - EdgeSize) * correctionFactor / length;
        allInMask = allInMask && mask.CheckNormal(normalJ, normalI);
        if (!allInMask) continue;

        normalI = (float)(i + EdgeSize) * correctionFactor / width;
        normalJ = (float)(j - EdgeSize) * correctionFactor / length;
        allInMask = allInMask && mask.CheckNormal(normalJ, normalI);

        if (!allInMask)
            continue;

        if (allInMask)
        {
            if (polygon != null)
            {
                float depth2 = 0;
                if (pixelCenterPoint.x >= 0 && pixelCenterPoint.y >= 0)
                {
                    TLine line;
                    line = new TLine(j, i, pixelCenterPoint.x, pixelCenterPoint.y);

                    float distanceToCenter = (float)line.CalcDistanceTwoPoints(line.p1, line.p2) * meterPerPixel;
                    float deformFactor = OSMParser.options.DeformFactor;
                    float depth1 = (float)distanceToCenter * deformFactor;
                    if (depth1 < depth)
                        depth2 = depth - depth1;
                    else
                        depth2 = 0;
                }
            }

            if (applyMaskData)
                //heightmap[i, j] = MinValue - depth ;
                heightmap[i, j] = mask.GetValue(normalJ, normalI) - depth2;
            else
                heightmap[i, j] = heightmap[i, j] - depth2;
        }

        if (parentNode != null)
            parentNode._progress = (float)(i + width * j) / ((length + 1) * (j + 1));
    }
}
}
*/
#endif

