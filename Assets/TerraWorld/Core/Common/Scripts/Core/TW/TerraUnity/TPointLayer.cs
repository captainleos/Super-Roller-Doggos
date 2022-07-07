#if UNITY_EDITOR
using System;
using System.Numerics;

namespace TerraUnity.Edittime
{
    public struct ObjectBounds
    {
        public Vector3 center;
        public Vector3 extents;
        public Vector3 min;
        public Vector3 max;
        public Vector3 size;
    }

    public class TPointLayer : TObjectiveLayer
    {
        public int SeedNo = 10000;
        public bool rotation90Degrees = false;
        public bool lockYRotation = false;
        public bool getSurfaceAngle = false;
        private int minRotationRange;
        private int maxRotationRange;

        public int MinRotationRange
        {
            get { return minRotationRange; }
            set
            {
                if (value < 0)
                    throw new Exception("Value must be between 0 & Max Rotation Range");
                else
                    minRotationRange = value;
            }
        }

        public int MaxRotationRange
        {
            get { return maxRotationRange; }
            set
            {
                if (value > 359)
                    throw new Exception("Value must be between Min Rotation Range & 359");
                else
                    maxRotationRange = value;
            }
        }

        private Vector3 scaleMultiplier;
        public Vector3 ScaleMultiplier { get => scaleMultiplier; set => scaleMultiplier = value; }

        private float positionVariation = 0f;
        public float PositionVariation
        {
            get { return positionVariation; }
            set
            {
                int _min = 0; int _max = 100;

                if (value > _max || value < _min)
                    throw new Exception("Value must be between " + _min + " & " + _max);
                else
                    positionVariation = value;
            }
        }

        private float minScale;
        private float maxScale;

        public float MaxScale
        {
            get { return maxScale; }
            set
            {
                if (value > 20)
                    throw new Exception("Value must be between Min. Scale Range & Max. range");
                else
                    maxScale = value;
            }
        }

        public float MinScale
        {
            get { return minScale; }
            set
            {
                if (value < 0.1f)
                    throw new Exception("Value must be between 0.1 & Max. Scale" );
                else
                    minScale = value;
            }
        }

        public TPointLayer()
        {

        }
    }
}
#endif

