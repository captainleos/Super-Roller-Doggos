using System.Numerics;
using System;

namespace TerraUnity.Edittime
{
    public class TPoint
    {
        private float x;
        private float y;
        private float z;
        public float X { get => x; set { if (value > 1) x = 1; else if (value < 0) x = 0; else x = value; } }
        public float Y { get => y; set { if (value > 1) y = 1; else if (value < 0) y = 0; else y = value; } }
        public float Z { get => z; set { if (value > 1) z = 1; else if (value < 0) z = 0; else z = value; } }
    }

    public class TPointObject
    {
        public TGlobalPoint GeoPosition;
        public Vector3 rotation;
        public Vector3 scale;
        private int _id;
        private static int Counter;

        public TPointObject()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            _id = (rand.Next() + Counter++);
        }

        public int Id { get => _id; }
    }
}

