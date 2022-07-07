using System;
using System.Numerics;

namespace TerraUnity.Edittime
{
    public class TImageLayer : TLayer
    {
        private Vector2 tiling = Vector2.One;
        public Vector2 Tiling { get => tiling; set => tiling = value; }

        private Vector2 tilingOffset = Vector2.Zero;
        public Vector2 TilingOffset { get => tilingOffset; set => tilingOffset = value; }

        private Vector4 specular = new Vector4(0, 0, 0, 1);
        public Vector4 Specular { get => specular; set => specular = value; }

        private float metallic = 0f;
        public float Metallic
        {
            get { return metallic; }
            set
            {
                float _min = 0f; float _max = 1f;
                if ((value > _max) || (value < _min))
                    throw new Exception("Value must be between " + _min + " & " + _max);
                else
                    metallic = value;
            }
        }

        private float smoothness = 0f;
        public float Smoothness
        {
            get { return smoothness; }
            set
            {
                float _min = 0f; float _max = 1f;
                if ((value > _max) || (value < _min))
                    throw new Exception("Value must be between " + _min + " & " + _max);
                else
                    smoothness = value;
            }
        }

        private float normalScale = 1;
        public float NormalScale
        {
            get { return normalScale; }
            set
            {
                float _min = 0f; float _max = 10f;
                if ((value > _max) || (value < _min))
                    throw new Exception("Value must be between " + _min + " & " + _max);
                else
                    normalScale = value;
            }
        }

      //  private bool isColorMap = false;
      //  public bool IsColorMap { get => isColorMap; set => isColorMap = value; }

    }
}

