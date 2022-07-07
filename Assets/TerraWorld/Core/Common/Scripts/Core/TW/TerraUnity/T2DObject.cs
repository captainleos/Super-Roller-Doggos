#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Numerics;

namespace TerraUnity.Edittime
{
    public class T2DObject 
    {
        public long id;
        public Property property = Property.None;
        public string name;
        public List<TGlobalPoint> AroundPoints;
        public List<T2DObject> Holes;
        public TGlobalPoint center;
        public float minHeight = 0;
        public float avgHeight = 0;
        public float maxHeight = 0;

        public T2DObject() 
        {
            this.AroundPoints = new List<TGlobalPoint>();
            this.Holes = new List<T2DObject>();
            this.center = new TGlobalPoint();
            this.id = 0;
            this.name = "T2DObject0";
        }

        public void Center()
        {
            if (this.center.longitude == 0 || this.center.latitude == 0)
            {
                int count = AroundPoints.Count;

                for (int i = 0; i < count; i++)
                {
                    this.center.latitude += AroundPoints[i].latitude;
                    this.center.longitude += AroundPoints[i].longitude;
                }

                this.center.latitude /= count;
                this.center.longitude /= count;
            }
        }

        public TArea GetBounds()
        {
            double top = double.MinValue;
            double left = double.MaxValue;
            double bottom = double.MaxValue;
            double right = double.MinValue;
            TLandcoverProccessor.GetBounds(AroundPoints, out top, out left, out bottom, out right);

            return new TArea(top, left, bottom, right);
        }

        public double GetAreaInM2()
        {
            int i, j;
            double tempArea = 0;

            for (i = 0; i < AroundPoints.Count; i++)
            {
                j = (i + 1) % AroundPoints.Count;

                tempArea += AroundPoints[i].latitude * AroundPoints[j].longitude;
                tempArea -= AroundPoints[i].longitude * AroundPoints[j].latitude;
            }

            tempArea = Math.Abs(tempArea) / 2;

            TArea boxArea = GetBounds();
            double fullArea = (boxArea._top - boxArea._bottom) * (boxArea._right - boxArea._left);
            double area = boxArea._areaSizeLat * boxArea._areaSizeLon * 1000000 * tempArea / fullArea;
            return area;
        }

        public float GetPermimeter()
        {
            float perimeter = 0;
            Vector2 currentPos = Vector2.Zero;
            Vector2 previousPos = Vector2.Zero;

            for (int i = 0; i < AroundPoints.Count; i++)
            {
                float lat = (float)AroundPoints[i].latitude;
                float lon = (float)AroundPoints[i].longitude;
                currentPos = new Vector2(lat, lon);

                if (i > 0)
                    perimeter += Vector2.Distance(currentPos, previousPos);

                previousPos = new Vector2(lat, lon);
            }

            return perimeter;
        }

        public void AddToAroundpoints(TGlobalPoint point)
        {
            AroundPoints.Add(point);
        }

        public void AddToAroundpoints( List<TGlobalPoint> points)
        {
            for (int i = 0; i < points.Count; i++)
                AddToAroundpoints(points[i]);
        }
    }
}
#endif
#endif

