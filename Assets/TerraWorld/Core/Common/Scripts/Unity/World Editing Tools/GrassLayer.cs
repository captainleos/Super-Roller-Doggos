using UnityEngine;
using Mewlist.MassiveGrass;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class GrassLayer : WorldToolsParams
    {
        public bool active = true;
        //[SerializeField]
        public MassiveGrassProfile MGP;

        private MassiveGrass _massiveGrass;
        public MassiveGrass massiveGrass { get => transform.parent.GetComponent<MassiveGrass>(); }

        private Terrain _terrain  = null;

        public void UpdateLayer()
        {
#if UNITY_EDITOR
            //massiveGrass = transform.parent.GetComponent<MassiveGrass>();
            if (massiveGrass != null) massiveGrass.Refresh();
#endif
        }

        private Terrain GetTerrain ()
        {
            Transform parent = transform;

            while (parent != null)
            {
                if (parent.GetComponent<Terrain>() != null) return parent.GetComponent<Terrain>();
                parent = parent.transform.parent;
            }

            return null;
        }

        public Terrain Terrain
        {
            get
            {
                if (_terrain == null) _terrain = GetTerrain();
                return _terrain;
            }
        }
    }
}

