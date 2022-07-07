using System.Collections.Generic;

namespace TerraUnity.Edittime
{
    public class TLinearObject
    {
        public long id;
        public string name;
        public List<TGlobalPoint> Points;

        public TLinearObject() : base()
        {
            Points = new List<TGlobalPoint>();
        }

        public TGlobalPoint Center()
        {
            int count = Points.Count;
            TGlobalPoint pivot = new TGlobalPoint();

            int i = (int)(count * 1.0f / 2);
            pivot.latitude += Points[i].latitude;
            pivot.longitude += Points[i].longitude;

            return pivot;
        }
    }
}

