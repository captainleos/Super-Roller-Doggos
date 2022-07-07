using UnityEngine;
using System.Collections.Generic;
using TerraUnity.TriangleNet.Geometry;

namespace TerraUnity.Edittime
{
    public class CreateMeshTri //: MonoBehaviour
    {
        public Vector3[] ToVector3(ICollection<Vertex> vert)
        {
            List<Vertex> vertList = new List<Vertex>();
            foreach (var vt in vert) vertList.Add(vt);

            var v = new Vector3[vert.Count];
            for (int i = 0; i < vertList.Count; i++)
            {
                v[i].x = (float)vertList[i].x;
                v[i].z = (float)vertList[i].y;
            }

            return v;
        }

        public int[] ToTriangles(ICollection<TerraUnity.TriangleNet.Topology.Triangle> tris)
        {
            List<TerraUnity.TriangleNet.Topology.Triangle> triList = new List<TerraUnity.TriangleNet.Topology.Triangle>();
            foreach (var t in tris) triList.Add(t);


            int[] ts = new int[tris.Count * 3];
            for (int i = 0; i < triList.Count; i++)
            {
                TerraUnity.TriangleNet.Topology.Triangle tri = triList[i];
                ts[i * 3 + 2] = tri.vertices[0].ID;
                ts[i * 3 + 1] = tri.vertices[1].ID;
                ts[i * 3 + 0] = tri.vertices[2].ID;
            }

            return ts;
        }
    }
}

