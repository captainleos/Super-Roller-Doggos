#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Numerics;
using TerraUnity.Utils;

namespace TerraUnity.Edittime
{
    public class THeightmap
    {
        public float[,] heightsData;
        //public int heightmapResolution;

        public THeightmap()
        {
            heightsData = new float[33, 33];
        }

        public THeightmap(float[,] HeightsData)
        {
            heightsData = HeightsData;
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < heightsData.GetLength(0); i++)
                for (int j = 0; j < heightsData.GetLength(1); j++)
                    if (heightsData[i, j] != 0) return false;

            return true;
        }

        public float GetSteepness (double x, double y, float AreaWidth , float AreaLength)
        {
            Vector3 a, b, c;
            Vector3 Normal;

            //-------------
            GetVertexPoints(x, y, out a, out b, out c);
            a.X = a.X * AreaLength;
            a.Y = a.Y * AreaWidth;
            b.X = b.X * AreaLength;
            b.Y = b.Y * AreaWidth;
            c.X = c.X * AreaLength;
            c.Y = c.Y * AreaWidth;

            var dir = Vector3.Cross(b - a, c - a);
            Normal = Vector3.Normalize(dir);

            float result =  (float)Math.Acos(Math.Abs(Normal.Z)) * 180 / (float)(Math.PI);
            if (result > 90 || result < 0) throw new Exception("Sttepness Error");
            return result;
        }

        public bool GetVertexPoints(double x, double y, out Vector3 vertexPoint1, out Vector3 vertexPoint2, out Vector3 vertexPoint3)
        {
            bool result = false;
            int indexX = (int)Math.Floor(x * (heightsData.GetLength(1) - 1));
            int indexY = (int)Math.Floor(y * (heightsData.GetLength(0) - 1));
            double indexYF = y * (heightsData.GetLength(0) - 1);
            double indexXF = x * (heightsData.GetLength(1) - 1);

            if (indexY < 0) indexY = 0;
            if (indexXF < 0) indexXF = 0;
            if (indexX < 0) indexX = 0;
            if (indexYF < 0) indexYF = 0;

            float[] neighbors = new float[4];
            try
            {
                neighbors[0] = heightsData[indexY, indexX];
                neighbors[1] = heightsData[indexY + 1, indexX];
                neighbors[2] = heightsData[indexY, indexX + 1];
                neighbors[3] = heightsData[indexY + 1, indexX + 1];
            }
            catch
            {
                result = false;
                vertexPoint1 = Vector3.Zero;
                vertexPoint2 = Vector3.Zero;
                vertexPoint3 = Vector3.Zero;
                return result;
            }

            Vector3 p1 = new Vector3(indexX / (float)(heightsData.GetLength(0) - 1), (float)indexY / (heightsData.GetLength(1) - 1), neighbors[0]);
            Vector3 p2 = new Vector3(indexX / (float)(heightsData.GetLength(0) - 1), (float)(indexY + 1) / (heightsData.GetLength(1) - 1), neighbors[1]);
            Vector3 p3 = new Vector3((indexX + 1) / (float)(heightsData.GetLength(0) - 1), indexY / (float)(heightsData.GetLength(1) - 1), neighbors[2]);
            Vector3 p4 = new Vector3((indexX + 1) / (float)(heightsData.GetLength(0) - 1), (indexY + 1) / (float)(heightsData.GetLength(1) - 1), neighbors[3]);

            double[] distances = new double[2];
            distances[0] = Math.Sqrt((indexYF - indexY - 1) * (indexYF - indexY - 1) + (indexXF - indexX) * (indexXF - indexX));
            distances[1] = Math.Sqrt((indexYF - indexY) * (indexYF - indexY) + (indexXF - indexX - 1) * (indexXF - indexX - 1));

            // Get proper triangle points in patch
            if (distances[0] < distances[1])
            {
                result = true;
                vertexPoint1 = p1;
                vertexPoint2 = p2;
                vertexPoint3 = p4;
            }
            else
            {
                result = true;
                vertexPoint1 = p1;
                vertexPoint2 = p3;
                vertexPoint3 = p4;
            }

            return result;
        }

        public double GetInterpolatedHeight(double normalX, double normalY)
        {
            Vector3 p1 = Vector3.Zero;
            Vector3 p2 = Vector3.Zero;
            Vector3 p3 = Vector3.Zero;

            return GetInterpolatedHeight(normalX, normalY, out p1, out p2, out p3);
        }

        public double GetInterpolatedHeight(double x, double y, out Vector3 vertexPoint1, out Vector3 vertexPoint2, out Vector3 vertexPoint3)
        {
            double result = 0;
            x = TUtils.Clamp(0d, 1d, x);
            y = TUtils.Clamp(0d, 1d, y);

            int indexX = (int)Math.Floor(x * (heightsData.GetLength(1) - 1));
            int indexY = (int)Math.Floor(y * (heightsData.GetLength(0) - 1));
            double indexYF = y * (heightsData.GetLength(0) - 1);
            double indexXF = x * (heightsData.GetLength(1) - 1);

            if (indexX == indexXF && indexY == indexYF)
            {
                vertexPoint1 = new Vector3((float)x, (float)y, heightsData[indexY, indexX]);
                vertexPoint2 = vertexPoint1;
                vertexPoint3 = vertexPoint1;
                return heightsData[indexY, indexX];
            }

            if (x == 1 || y == 1)
            {
                vertexPoint1 = new Vector3((float)x, (float)y, heightsData[indexY, indexX]);
                vertexPoint2 = vertexPoint1;
                vertexPoint3 = vertexPoint1;
                return heightsData[indexY, indexX];
            }

            if (GetVertexPoints(x, y, out vertexPoint1, out vertexPoint2, out vertexPoint3))
                result = GetZOnPlane(x, y, vertexPoint1, vertexPoint2, vertexPoint3);
            else
                throw new Exception("Not In Range"); ;

            return result;
        }

        public Vector3 GetInterpolatedNormal (double x, double y)
        {
            Vector3 result = Vector3.Zero;
            Vector3 vertexPoint1 = Vector3.Zero;
            Vector3 vertexPoint2 = Vector3.Zero;
            Vector3 vertexPoint3 = Vector3.Zero;

            if (GetVertexPoints(x, y, out vertexPoint1, out vertexPoint2, out vertexPoint3))
                result = GetNOnPlane(x, y, vertexPoint1, vertexPoint2, vertexPoint3, 1, 1 , 1);
            else
                throw new Exception("Not In Range"); ;

            return result;
        }

        public Vector3 GetInterpolatedNormal(double x, double y, float width, float height, float length)
        {
            Vector3 result = Vector3.Zero;
            Vector3 vertexPoint1 = Vector3.Zero;
            Vector3 vertexPoint2 = Vector3.Zero;
            Vector3 vertexPoint3 = Vector3.Zero;

            if (GetVertexPoints(x, y, out vertexPoint1, out vertexPoint2, out vertexPoint3))
                result = GetNOnPlane(x, y, vertexPoint1, vertexPoint2, vertexPoint3, width, height, length);
            else
                throw new Exception("Not In Range"); ;

            return result;
        }

        public float Getsteepness(double x, double y, float width, float height, float length)
        {
            float result = 0;
            Vector3 vertexPoint1 = Vector3.Zero;
            Vector3 vertexPoint2 = Vector3.Zero;
            Vector3 vertexPoint3 = Vector3.Zero;

            if (GetVertexPoints(x, y, out vertexPoint1, out vertexPoint2, out vertexPoint3))
                result = 90 - GetAngleOnPlane(vertexPoint1, vertexPoint2, vertexPoint3, width, height, length) * 90 / (float)Math.PI ;
            else
                throw new Exception("Not In Range"); ;

            return result;
        }

        private double GetZOnPlane (double x, double y, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 v1 = new Vector3(p1.X - p3.X, p1.Y - p3.Y, p1.Z - p3.Z);
            Vector3 v2 = new Vector3(p2.X - p3.X, p2.Y - p3.Y, p2.Z - p3.Z);
            Vector3 abc = new Vector3(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
            double d = abc.X * p3.X + abc.Y * p3.Y + abc.Z * p3.Z;
            double result = (d - abc.X * x - abc.Y * y) / abc.Z;

            return result;
        }

        private Vector3 GetNOnPlane (double x, double y, Vector3 p1, Vector3 p2, Vector3 p3, float width, float height, float length)
        {
            Vector3 abc;
            Vector3 P1 = new Vector3(p1.X * width, p1.Z * height, p1.Y * length);
            Vector3 P2 = new Vector3(p2.X * width, p2.Z * height, p2.Y * length);
            Vector3 P3 = new Vector3(p3.X * width, p3.Z * height, p3.Y * length);

            Vector3 v1 =  new Vector3(P2.X - P1.X, P2.Y - P1.Y, P2.Z - P1.Z);
            Vector3 v2 =  new Vector3(P2.X - P3.X, P2.Y - P3.Y, P2.Z - P3.Z);

            abc = Vector3.Cross(v1, v2);

            double abcSize = Math.Sqrt(abc.X * abc.X + abc.Y * abc.Y + abc.Z * abc.Z);
            abc.X = (float)(abc.X / abcSize);
            abc.Y = (float)(abc.Y / abcSize);
            abc.Z = (float)(abc.Z / abcSize);

            return abc;
        }

        private float GetAngleOnPlane(Vector3 p1, Vector3 p2, Vector3 p3, float width, float height, float length)
        {
            Vector3 abc;
            Vector3 P1 = new Vector3(p1.X * width, p1.Z * height, p1.Y * length);
            Vector3 P2 = new Vector3(p2.X * width, p2.Z * height, p2.Y * length);
            Vector3 P3 = new Vector3(p3.X * width, p3.Z * height, p3.Y * length);

            Vector3 v1 = new Vector3(P2.X - P1.X, P2.Y - P1.Y, P2.Z - P1.Z);
            Vector3 v2 = new Vector3(P2.X - P3.X, P2.Y - P3.Y, P2.Z - P3.Z);

            abc = Vector3.Cross(v1, v2);

            double abcSize = Math.Sqrt(abc.X * abc.X + abc.Y * abc.Y + abc.Z * abc.Z);
            float result = (float) Math.Acos(abc.Y / abcSize);

            return result;
        }
    }
}
#endif
#endif

