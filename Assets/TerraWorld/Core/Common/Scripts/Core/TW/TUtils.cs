#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using TerraUnity.Edittime;

namespace TerraUnity.Utils
{
    public static class TUtils
    {
#if TERRAWORLD_PRO
        public static void GeoToPixelCrop (TArea baseArea, TArea cropArea, int w, int h, out int newW, out int newH, out int offsetX, out int offsetY)
        {
            newW = (int)((cropArea._right - cropArea._left) * w / (baseArea._right - baseArea._left));
            newH = (int)((cropArea._top - cropArea._bottom) * h / (baseArea._top - baseArea._bottom));
            offsetX = (int)((cropArea._left - baseArea._left) * w / (baseArea._right - baseArea._left));
            offsetY = (int)((cropArea._bottom - baseArea._bottom) * h / (baseArea._top - baseArea._bottom));
        }

        public static TGlobalPoint PixelToLatLon (TArea area, int width, int length, double indexX, double indexY)
        {
            TGlobalPoint result = new TGlobalPoint();

            double normalizedX = InverseLerp(0, width, indexX);
            double normalizedZ = InverseLerp(0, length, indexY);

            result.longitude = area._left + ((area._right - area._left) * normalizedX);
            result.latitude = area._bottom + ((area._top - area._bottom) * normalizedZ);

            return result;
        }

        public static bool PointInPolygon(List<Vector2> polyPoints, Vector2 p)
        {
            int j = polyPoints.Count - 1;
            bool inside = false;
            double x;

            for (int i = 0; i < polyPoints.Count; j = i++)
            {
                if (polyPoints[i].X == p.X && polyPoints[i].Y == p.Y)
                {
                    inside = true;
                    break;
                }
                else
                {
                    if ((polyPoints[i].Y <= p.Y && p.Y < polyPoints[j].Y) || (polyPoints[j].Y <= p.Y && p.Y < polyPoints[i].Y))
                    {
                        x = p.X - ((polyPoints[j].X - polyPoints[i].X) * (p.Y - polyPoints[i].Y) /
                                    (polyPoints[j].Y - polyPoints[i].Y) + polyPoints[i].X);
                        if (x == 0)
                        {
                            inside = true;
                            break;
                        }
                        else if (x < 0)
                        {
                            inside = !inside;
                        }
                    }
                }
            }

            return inside;
        }

        public static bool PointInPolygon(List<TGlobalPoint> polyPoints, TGlobalPoint p)
        {
            int j = polyPoints.Count - 1;
            bool inside = false;
            double x;

            for (int i = 0; i < polyPoints.Count; j = i++)
            {
                if (polyPoints[i].longitude == p.longitude && polyPoints[i].latitude == p.latitude)
                {
                    inside = true;
                    break;
                }
                else
                {
                    if ((polyPoints[i].latitude <= p.latitude && p.latitude < polyPoints[j].latitude) || 
                        (polyPoints[j].latitude <= p.latitude && p.latitude < polyPoints[i].latitude))
                    {
                        x = p.longitude - ((polyPoints[j].longitude - polyPoints[i].longitude) * (p.latitude - polyPoints[i].latitude) / 
                                           (polyPoints[j].latitude - polyPoints[i].latitude) + polyPoints[i].longitude);
                        if (x == 0)
                        {
                            inside = true;
                            break;
                        }
                        else if (x < 0)
                        {
                            inside = !inside;
                        }
                    }
                }
            }

            return inside;
        }

        public static bool PointInPolygon(List<T2DPoint> polyPoints, T2DPoint p)
        {
            int j = polyPoints.Count - 1;
            bool inside = false;
            double x;

            for (int i = 0; i < polyPoints.Count; j = i++)
            {
                if (polyPoints[i].x == p.x && polyPoints[i].y == p.y)
                {
                    inside = true;
                    break;
                }
                else
                {
                    if ((polyPoints[i].y <= p.y && p.y < polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y < polyPoints[i].y))
                    {
                        x = p.x - ( (polyPoints[j].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / 
                                    (polyPoints[j].y - polyPoints[i].y) + polyPoints[i].x );
                        if (x == 0)
                        {
                            inside = true;
                            break;
                        }
                        else if (x < 0)
                        {
                            inside = !inside;
                        }
                    }
                }
            }

            return inside;
        }

        public static bool PointInPolygon(List<Vector3> polyPoints, Vector3 p)
        {
            int j = polyPoints.Count - 1;
            bool inside = false;
            double x;

            for (int i = 0; i < polyPoints.Count; j = i++)
            {
                if (polyPoints[i].X == p.X && polyPoints[i].Z == p.Z)
                {
                    inside = true;
                    break;
                }
                else
                {
                    if ((polyPoints[i].Z <= p.Z && p.Z < polyPoints[j].Z) || (polyPoints[j].Z <= p.Z && p.Z < polyPoints[i].Z))
                    {
                        x = p.X - ((polyPoints[j].X - polyPoints[i].X) * (p.Z - polyPoints[i].Z) /
                                    (polyPoints[j].Z - polyPoints[i].Z) + polyPoints[i].X);
                        if (x == 0)
                        {
                            inside = true;
                            break;
                        }
                        else if (x < 0)
                        {
                            inside = !inside;
                        }
                    }
                }
            }

            return inside;
        }

        public static void PolygonRectangle(List<TGlobalPoint> polyPoints, ref TGlobalPoint pointLB, ref TGlobalPoint pointRT)
        {
            int j = polyPoints.Count - 1;

            pointLB.longitude = double.MaxValue;
            pointLB.latitude = double.MaxValue;
            pointRT.longitude = double.MinValue;
            pointRT.latitude = double.MinValue;

            for (int i = 0; i < polyPoints.Count; j = i++)
            {
                if (polyPoints[i].longitude < pointLB.longitude)
                    pointLB.longitude = polyPoints[i].longitude;

                if (polyPoints[i].longitude > pointRT.longitude)
                    pointRT.longitude = polyPoints[i].longitude;

                if (polyPoints[i].latitude < pointLB.latitude)
                    pointLB.latitude = polyPoints[i].latitude;

                if (polyPoints[i].latitude > pointRT.latitude)
                    pointRT.latitude = polyPoints[i].latitude;
            }
        }

        public static string[] LogicalComparer (string filePath, string fileType)
        {
            string[] names = Directory.GetFiles(filePath, "*" + fileType, SearchOption.AllDirectories);
            ns.NumericComparer ns = new ns.NumericComparer();
            Array.Sort(names, ns);

            return names;
        }

        public static string[] LogicalComparer (string[] names)
        {
            ns.NumericComparer ns = new ns.NumericComparer();
            Array.Sort(names, ns);

            return names;
        }

        public static double Rad2Deg (double radians)
        {
            return radians * (180 / Math.PI);
        }

        public static double Deg2Rad (double degrees)
        {
            return degrees * (Math.PI / 180);
        }
#endif

        public static double InverseLerp (double a, double b, double value)
        {
            return (value - a) / (b - a);
        }

        public static double Clamp(double min, double max, double value)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        public static UnityEngine.Vector2 CastToUnity(Vector2 v)
        {
            return new UnityEngine.Vector2(v.X, v.Y);
        }

        public static Vector2 CastToNumerics(UnityEngine.Vector2 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static UnityEngine.Vector3 CastToUnity(Vector3 v)
        {
            return new UnityEngine.Vector3(v.X, v.Y, v.Z);
        }

        public static UnityEngine.Vector4 CastToUnity(Vector4 v)
        {
            return new UnityEngine.Vector4(v.X, v.Y, v.Z,v.W);
        }

        public static Vector3 CastToNumerics(UnityEngine.Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Vector4 CastToNumerics(UnityEngine.Vector4 v)
        {
            return new Vector4(v.x, v.y, v.z,v.w);
        }

        public static UnityEngine.Quaternion CastToUnity(Quaternion v)
        {
            return new UnityEngine.Quaternion(v.X, v.Y, v.Z, v.W);
        }

        public static UnityEngine.Color CastToUnityColor(Color c)
        {
            return new UnityEngine.Color(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }

        public static Color CastToDrawingColor(UnityEngine.Color c)
        {
            return Color.FromArgb((int)(c.a * 255f), (int)(c.r * 255f), (int)(c.g * 255f), (int)(c.b * 255f));
        }

        public static UnityEngine.Color Vector4ToUnityColor (Vector4 v)
        {
            return new UnityEngine.Color(v.X, v.Y, v.Z, v.W);
        }

        public static Vector4 UnityColorToVector4(UnityEngine.Color c)
        {
            return new Vector4(c.r, c.g, c.b, c.a);
        }

        public static UnityEngine.Color CastToUnityColor(Vector4 v)
        {
            return new UnityEngine.Color(v.X / 255f, v.Y / 255f, v.Z / 255f, v.W / 255f);
        }

        public static Color CastToDrawingColor(UnityEngine.Vector4 v)
        {
            return Color.FromArgb((int)(v.w * 255f), (int)(v.x * 255f), (int)(v.y * 255f), (int)(v.z * 255f));
        }

        public static UnityEngine.Vector4 CastToUnityVector(Color c)
        {
            return new UnityEngine.Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }

        public static Vector4 CastToDrawingVector(UnityEngine.Color c)
        {
            return new Vector4(c.r * 255, c.g * 255, c.b * 255, c.a * 255);
        }

        public static bool IsPowerOfTwo(ulong x)
        {
            return (x & (x - 1)) == 0;
        }

        public static int nearestPowerOfTwo(int n)
        {
            int v = n;

            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++; // next power of 2

            int x = v >> 1; // previous power of 2

            return (v - n) > (n - x) ? x : v;
        }

        public static double AngleFrom3PointsInDegrees(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double a = x2 - x1;
            double b = y2 - y1;
            double c = x3 - x2;
            double d = y3 - y2;

            double atanA = Math.Atan2(a, b);
            double atanB = Math.Atan2(c, d);

            return (atanA - atanB) * (-180 / Math.PI);
            // if Second line is counterclockwise from 1st line angle is 
            // positive, else negative
        }
    }
}
#endif

