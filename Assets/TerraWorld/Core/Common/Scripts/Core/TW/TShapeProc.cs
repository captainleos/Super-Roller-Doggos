#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Numerics;

namespace TerraUnity.Edittime
{
    public enum LineSlopeType
    {
        Normal = 0,
        Vertical,
        Horizontal,
    }

    public enum LineDirection
    {
        FirstPoint = 0,
        MiddlePoint,
        SecondPoint,
    }

    public enum LinePosition
    {
        Positive = 0,
        Negative,
        Neutral,
    }

    public enum AddMode
    {
        Add = 0,
        Insert
    }

    public class T2DIndex
    {
        public int index1;
        public int index2;

        public T2DIndex()
        {
            this.index1 = 0;
            this.index2 = 0;
        }
        public T2DIndex(int index1, int index2)
        {
            this.index1 = index1;
            this.index2 = index2;
        }
    }

    public class TPolygonMask
    {
        public List<Vector3> aroundPoints = null;
        public Property property;
        public double minHeight;

        public TPolygonMask()
        {
            this.aroundPoints = new List<Vector3>();
            this.property = Property.None;
            this.minHeight = 0;
        }
    }

    public class T2DPoint                 
    {
        public double x;
        public double y;

        public T2DPoint()
        {
            this.x = 0;
            this.y = 0;
        }

        public T2DPoint( double x, double y )
        {
            this.x = x;
            this.y = y;
        }
    }

    public class T2DShape
    {
        public T2DObject t2DObject;
        public int startAreaIndex;
        public int endAreaIndex;

        public T2DShape()
        {
            this.t2DObject = new T2DObject();
            this.startAreaIndex = -1;
            this.endAreaIndex = -1;
        }
    }

    public class TLine                  //  y = slope * x + wor
    {
        public T2DPoint p1;             //  Point1
        public T2DPoint p2;             //  Point2
        public double slope;            //  Slope
        public double alpha;            //  Angle of Slope
        public LinePosition dirX;       //  Line Direction Horizontal
        public LinePosition dirY;       //  Line Direction Vertical
        public double lor;              //  Lenght of origin ( y = 0 => x = -wor / slope )
        public double wor;              //  Width  of origin ( x = 0 => y = wor )
        public double sqrt;             //  SQRT( a * a + b * b )
        public double disPoints;        //  SQRT( (p1.x - p2.x)^2 + (p1.y - p2.y)^2 )
        public LineSlopeType lineSlopeType;

        public TLine()
        {
            p1 = new T2DPoint(1, 2);
            p2 = new T2DPoint(3, 4);
        }

        public TLine(T2DPoint p1, T2DPoint p2) : this()
        {
            this.p1.x = p1.x;
            this.p1.y = p1.y;
            this.p2.x = p2.x;
            this.p2.y = p2.y;

            this.CalcLineSlopeType();
            this.CalcLine();
        }

        public TLine(double x1, double y1, double slope) : this()
        {
            this.p1.x = x1;
            this.p1.y = y1;
            this.slope = slope;
            this.alpha = Math.Atan(this.slope);
            this.p2.x = p1.x + 100;
            this.p2.y = p1.y + p1.x * this.slope;

            this.CalcLineSlopeType();
            this.CalcLine();
        }

        public TLine( double x1, double y1, double x2, double y2) : this()
        {
            this.p1.x = x1;
            this.p1.y = y1;
            this.p2.x = x2;
            this.p2.y = y2;

            this.CalcLineSlopeType();
            this.CalcLine();
        }

        public void CalcLineSlopeType()
        {
            if (p1.x == p2.x)
                lineSlopeType = LineSlopeType.Vertical;

            else if (p1.y == p2.y)
                lineSlopeType = LineSlopeType.Horizontal;

            else
                lineSlopeType = LineSlopeType.Normal;
        }

        public void CalcLine()
        {
            if (lineSlopeType == LineSlopeType.Normal)
            {
                this.slope = (p2.y - p1.y) / (p2.x - p1.x);
                this.alpha = Math.Atan(this.slope);
                this.wor = p1.y - this.slope * p1.x;
                this.lor = -this.wor / this.slope;
                this.sqrt = Math.Sqrt(this.slope * this.slope + 1);
                if (this.p1.x < this.p2.x)
                    this.dirX = LinePosition.Positive;
                else
                    this.dirX = LinePosition.Negative;

                if (this.p1.y < this.p2.y)
                    this.dirY = LinePosition.Positive;
                else
                    this.dirY = LinePosition.Negative;
            }
            else if (lineSlopeType == LineSlopeType.Horizontal)
            {
                this.slope = 0;
                this.alpha = 0;
                this.wor = p1.y;
                this.lor = double.MaxValue;
                this.sqrt = 1f;
                this.dirY = LinePosition.Neutral;
                if (this.p1.x < this.p2.x)
                    this.dirX = LinePosition.Positive;
                else
                    this.dirX = LinePosition.Negative;
            }
            else if (lineSlopeType == LineSlopeType.Vertical)
            {
                this.slope = double.MaxValue;
                this.wor = double.MaxValue;
                this.lor = this.p1.x;
                this.sqrt = 0f;
                this.dirX = LinePosition.Neutral;
                if (this.p1.y < this.p2.y)
                {
                    this.dirY = LinePosition.Positive;
                    this.alpha = Math.PI / 2f;
                }
                else
                {
                    this.dirY = LinePosition.Negative;
                    this.alpha = -Math.PI / 2f;
                }
            }
            this.disPoints = this.CalcDistanceTwoPoints(this.p1, this.p2);
        }

        public double CalcY(double x)
        {
            double y = 0;

            if (lineSlopeType == LineSlopeType.Normal)
                y = this.slope * x + this.wor;
            else
                y = this.wor;

            return y;
        }

        public double CalcLengthOfLine()
        {
            return this.CalcDistanceTwoPoints(this.p1, this.p2);
        }

        public T2DPoint CalcPointWithLength( double length )
        {
            T2DPoint point = new T2DPoint();

            if (lineSlopeType == LineSlopeType.Normal)
            {
                point.x = this.p1.x + length * Math.Sin(Math.Atan(this.slope));
                point.y = this.slope * point.x + this.wor;
            }
            else if (lineSlopeType == LineSlopeType.Horizontal)
            {
                if (this.dirX == LinePosition.Positive)
                    point.x = this.p1.x + length;
                else
                    point.x = this.p1.x - length;

                point.y = this.wor;
            }
            else if (lineSlopeType == LineSlopeType.Vertical)
            {
                point.x = this.lor;
                if(this.dirY == LinePosition.Positive)
                    point.y = this.p1.y + length;
                else
                    point.y = this.p1.y - length;
            }

            return point;
        }

        public double CalcDistancePointFromLine(T2DPoint p)
        {
            double dis = 0;

            if (lineSlopeType == LineSlopeType.Normal)
            {
                dis = Math.Abs(this.slope * p.x - p.y + this.wor) / this.sqrt;
            }
            else if (lineSlopeType == LineSlopeType.Horizontal)
            {
                dis = Math.Abs(this.wor - p.y);
            }
            else if (lineSlopeType == LineSlopeType.Vertical)
            {
                dis = Math.Abs(this.lor - p.x);
            }

            return dis;
        }

        public double CalcDistanceTwoPoints( T2DPoint p1, T2DPoint p2 )
        {
            double dis = (double)Math.Sqrt((p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y));

            return dis;
        }

        public bool CalcIntersection(TLine line, ref List<T2DPoint> intersectPoints)
        {
            bool hasIntersect = false;
            T2DPoint point1 = new T2DPoint();

            if (this.slope != line.slope)
            {
                if (this.lineSlopeType == LineSlopeType.Normal)
                {
                    if (line.lineSlopeType == LineSlopeType.Vertical)
                    {
                        point1.x = line.p1.x;
                        point1.y = this.slope * line.p1.x + this.wor;
                    }
                    else
                    {
                        point1.x = (line.wor - this.wor) / (this.slope - line.slope);
                        point1.y = (this.slope * line.wor - line.slope * this.wor) / (this.slope - line.slope);
                    }
                }
                else if (this.lineSlopeType == LineSlopeType.Horizontal)
                {
                    if (line.lineSlopeType == LineSlopeType.Vertical)
                    {
                        point1.x = line.lor;
                        point1.y = this.wor;
                    }
                    else
                    {
                        point1.x = (this.wor - line.wor) / line.slope;
                        point1.y = this.wor;
                    }
                }
                else if (this.lineSlopeType == LineSlopeType.Vertical)
                {
                    if (line.lineSlopeType == LineSlopeType.Horizontal)
                    {
                        point1.x = this.lor;
                        point1.y = line.wor;
                    }
                    else
                    {
                        point1.x = this.lor;
                        point1.y = line.slope * this.lor + line.wor;
                    }
                }
                if (this.PointInterLine(point1) && line.PointInterLine(point1))
                {
                    hasIntersect = true;
                    intersectPoints.Add(point1);
                }
            }
            else
            {
                if (this.lineSlopeType == LineSlopeType.Normal)
                {
                    if( (this.slope * this.p1.x + this.wor) == (line.slope * this.p1.x + line.wor) )
                    {
                        intersectPoints.Add(line.p1);
                        intersectPoints.Add(line.p2);
                        hasIntersect = true;
                    }
                }
                else if (this.lineSlopeType == LineSlopeType.Horizontal)
                {
                    if(this.wor == line.wor)
                    {
                        intersectPoints.Add(line.p1);
                        intersectPoints.Add(line.p2);
                        hasIntersect = true;
                    }
                }
                else if (this.lineSlopeType == LineSlopeType.Vertical)
                {
                    if (this.lor == line.lor)
                    {
                        intersectPoints.Add(line.p1);
                        intersectPoints.Add(line.p2);
                        hasIntersect = true;
                    }
                }
            }
            if (hasIntersect == false)
            {
                if( intersectPoints.Count > 0 )
                    intersectPoints.Clear();
            }

            return hasIntersect;
        }

        public bool CalcPerpendicularIntersection(T2DPoint point, ref T2DPoint pointIntersect, double minDistance)
        {
            bool hasIntersect = false;

            if (this.lineSlopeType == LineSlopeType.Normal)
            {
                pointIntersect.x = (point.x + this.slope * (point.y - this.wor)) / (this.slope * this.slope + 1);
                pointIntersect.y = this.slope * pointIntersect.x + this.wor;
            }
            else if (this.lineSlopeType == LineSlopeType.Horizontal)
            {
                pointIntersect.x = point.x;
                pointIntersect.y = this.wor;
            }
            else if (this.lineSlopeType == LineSlopeType.Vertical)
            {
                pointIntersect.x = this.lor;
                pointIntersect.y = point.y;
            }

            if (this.PointInterLine(pointIntersect))
            {
                if(this.CalcDistanceTwoPoints(point, pointIntersect) <= minDistance )
                    hasIntersect = true;
            }

            return hasIntersect;
        }

        public bool PointInterLine(T2DPoint point)
        {
            bool interLineFlag = false;
            bool interLineXFlag;

            if (this.lineSlopeType == LineSlopeType.Normal)
            {
                interLineXFlag = false;
                if (this.dirX == LinePosition.Positive)
                {
                    if (point.x >= this.p1.x && point.x <= this.p2.x)
                        interLineXFlag = true;
                }
                else if (this.dirX == LinePosition.Negative)
                {
                    if (point.x >= this.p2.x && point.x <= this.p1.x)
                        interLineXFlag = true;
                }
                if (interLineXFlag == true)
                {
                    if (this.dirY == LinePosition.Positive)
                    {
                        if (point.y >= this.p1.y && point.y <= this.p2.y)
                            interLineFlag = true;
                    }
                    else if (this.dirY == LinePosition.Negative)
                    {
                        if (point.y >= this.p2.y && point.y <= this.p1.y)
                            interLineFlag = true;
                    }
                }
            }
            else if (this.lineSlopeType == LineSlopeType.Horizontal)
            {
                if( this.dirX == LinePosition.Positive )
                {
                    if( point.x >= this.p1.x && point.x <= this.p2.x )
                        interLineFlag = true;
                }
                else if (this.dirX == LinePosition.Negative)
                {
                    if (point.x >= this.p2.x && point.x <= this.p1.x)
                        interLineFlag = true;
                }
            }
            else if (this.lineSlopeType == LineSlopeType.Vertical)
            {
                if (this.dirY == LinePosition.Positive)
                {
                    if (point.y >= this.p1.y && point.y <= this.p2.y)
                        interLineFlag = true;
                }
                else if (this.dirY == LinePosition.Negative)
                {
                    if (point.y >= this.p2.y && point.y <= this.p1.y)
                        interLineFlag = true;
                }
            }

            return interLineFlag;
        }
    }
}
#endif
#endif

