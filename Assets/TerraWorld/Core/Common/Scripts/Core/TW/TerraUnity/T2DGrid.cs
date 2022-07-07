#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TerraUnity.Utils;

namespace TerraUnity.Edittime
{
    public struct Triangle
    {
        public int Index1;
        public int Index2;
        public int Index3;

        public Triangle(int index1, int index2 , int index3)
        {
            Index1 = index1;
            Index2 = index2;
            Index3 = index3;
        }
    }

    public class T2DGrid 
    {
        private static int Counter;
        private string _ID;
        public string ID { get => _ID; }
        public string name;
        public List<TGlobalPoint> VerticesList;
        public List<Triangle> TrianglesList;

        public T2DGrid()
        {
            VerticesList = new List<TGlobalPoint>();
            TrianglesList = new List<Triangle>();
            Counter++;
            System.Random rand = new System.Random((int)DateTime.Now.Ticks);
            _ID = (rand.Next()+ Counter++).ToString();
        }

        public int IsPointInGrid(TGlobalPoint P1)
        {
            for (int i = 0; i< VerticesList.Count; i++)
                if (P1.latitude == VerticesList[i].latitude && P1.longitude == VerticesList[i].longitude)
                    return i;

            return -1;
        }

        public int IsPointCloseToGrid(TGlobalPoint P1, double minDistance)
        {
            for (int i = 0; i < VerticesList.Count; i++)
                if ( Math.Abs (P1.latitude - VerticesList[i].latitude) <= minDistance && Math.Abs(P1.longitude - VerticesList[i].longitude) <= minDistance)
                    return i;
            return -1;
        }

        public void AddTriangle(TGlobalPoint P1, TGlobalPoint P2, TGlobalPoint P3)
        {
            int Point1Index = -1;
            int Point2Index = -1;
            int Point3Index = -1;

            // Clockwise reordering
            if (TUtils.AngleFrom3PointsInDegrees(P2.latitude, P2.longitude, P1.latitude, P1.longitude, P3.latitude, P3.longitude) < 0 )
            {
                TGlobalPoint temp = P2;
                P2 = P3;
                P3 = temp;
            }

            // Inserting Points in VerticesList
            int startvaule = (VerticesList.Count < 1000) ? 0 : (VerticesList.Count - 1000);

            for (int i = startvaule ; i < VerticesList.Count; i++)
            {
                if ((VerticesList[i].latitude == P1.latitude) && (VerticesList[i].longitude == P1.longitude))
                    Point1Index = i;
                if ((VerticesList[i].latitude == P2.latitude) && (VerticesList[i].longitude == P2.longitude))
                    Point2Index = i;
                if ((VerticesList[i].latitude == P3.latitude) && (VerticesList[i].longitude == P3.longitude))
                    Point3Index = i;
            }

            if (Point1Index == -1) { VerticesList.Add(P1); Point1Index = VerticesList.Count - 1; }
            if (Point2Index == -1) { VerticesList.Add(P2); Point2Index = VerticesList.Count - 1; }
            if (Point3Index == -1) { VerticesList.Add(P3); Point3Index = VerticesList.Count - 1; }

            Triangle triangle = new Triangle(Point1Index, Point2Index, Point3Index);
            TrianglesList.Add(triangle);
        }

        public void AddQuad(TGlobalPoint P1, TGlobalPoint P2, TGlobalPoint P3, TGlobalPoint P4)
        {
            int Point1Index = -1;
            int Point2Index = -1;
            int Point3Index = -1;
            int Point4Index = -1;

            // Clockwise reordering
            if (TUtils.AngleFrom3PointsInDegrees(P2.latitude, P2.longitude, P1.latitude, P1.longitude, P3.latitude, P3.longitude) < 0)
            {
                TGlobalPoint temp = P2;
                P2 = P3;
                P3 = temp;
            }

            if (TUtils.AngleFrom3PointsInDegrees(P3.latitude, P3.longitude, P1.latitude, P1.longitude, P4.latitude, P4.longitude) < 0)
            {
                TGlobalPoint temp = P3;
                P3 = P4;
                P4 = temp;
            }

            int startvaule = (VerticesList.Count < 1000) ? 0 : (VerticesList.Count - 1000);

            // Inserting Points in VerticesList
            for (int i = startvaule; i < VerticesList.Count; i++)
            {
                if ((VerticesList[i].latitude == P1.latitude) && (VerticesList[i].longitude == P1.longitude))
                    Point1Index = i;
                if ((VerticesList[i].latitude == P2.latitude) && (VerticesList[i].longitude == P2.longitude))
                    Point2Index = i;
                if ((VerticesList[i].latitude == P3.latitude) && (VerticesList[i].longitude == P3.longitude))
                    Point3Index = i;
                if ((VerticesList[i].latitude == P4.latitude) && (VerticesList[i].longitude == P4.longitude))
                    Point3Index = i;
            }

            if (Point1Index == -1) { VerticesList.Add(P1); Point1Index = VerticesList.Count - 1; }
            if (Point2Index == -1) { VerticesList.Add(P2); Point2Index = VerticesList.Count - 1; }
            if (Point3Index == -1) { VerticesList.Add(P3); Point3Index = VerticesList.Count - 1; }
            if (Point4Index == -1) { VerticesList.Add(P4); Point4Index = VerticesList.Count - 1; }

            Triangle triangle1 = new Triangle(Point1Index, Point2Index, Point3Index);
            TrianglesList.Add(triangle1);
            Triangle triangle2 = new Triangle(Point1Index, Point3Index, Point4Index);
            TrianglesList.Add(triangle2);
        }

        public int[] GetIndices()
        {
            int[] indices = new int[TrianglesList.Count * 3];

            for (int i = 0; i < TrianglesList.Count; i++)
            {
                indices[i * 3] = TrianglesList[i].Index1;
                indices[i * 3 + 1] = TrianglesList[i].Index2;
                indices[i * 3 + 2] = TrianglesList[i].Index3;
            }

            return indices;
        }

        public TGlobalPoint Center()
        {
            int count = VerticesList.Count;
            TGlobalPoint pivot = new TGlobalPoint();

            for (int i = 0; i < count; i++)
            {
                pivot.latitude += VerticesList[i].latitude;
                pivot.longitude += VerticesList[i].longitude;
            }

            pivot.latitude /= count;
            pivot.longitude /= count;

            return pivot;
        }

        public TArea GetBounds()
        {
            double top = double.MinValue;
            double left = double.MaxValue;
            double bottom = double.MaxValue;
            double right = double.MinValue;
            int count = VerticesList.Count;
            TGlobalPoint node;

            for (int i = 0; i < count; i++)
            {
                node.latitude = VerticesList[i].latitude;
                node.longitude = VerticesList[i].longitude;

                if (top < node.latitude)
                    top = node.latitude;

                if (left > node.longitude)
                    left = node.longitude;

                if (bottom > node.latitude)
                    bottom = node.latitude;

                if (right < node.longitude)
                    right = node.longitude;
            }

            return new TArea(top, left, bottom, right);
        }
    }
}
#endif
#endif

