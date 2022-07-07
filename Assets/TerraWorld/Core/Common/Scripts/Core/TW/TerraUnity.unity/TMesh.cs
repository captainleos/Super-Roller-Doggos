#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using TerraUnity.TriangleNet.Geometry;
using TerraUnity.Utils;

namespace TerraUnity.Edittime
{
    public class PolygonVertices
    {
        public Property property;
        public System.Numerics.Vector3[] vertices = null;

        public PolygonVertices()
        {
        }

        public PolygonVertices(int verticesCount)
        {
            vertices = new System.Numerics.Vector3[verticesCount];
        }
    }

    public class TMesh 
    {
        private Mesh mesh;
        public Mesh Mesh { get => mesh; set => mesh = value; }

        public static Mesh GetMeshObject(string AssetPath, string MeshName)
        {
            Mesh result = null;

            if (!string.IsNullOrEmpty(AssetPath) && File.Exists(AssetPath) && !string.IsNullOrEmpty(MeshName))
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetPath);

                foreach (Object obj in assets)
                {
                    if (obj is Mesh)
                    {
                        Mesh subMesh = (Mesh)obj;
                        if (MeshName == subMesh.name) result = subMesh;
                    }
                }
            }
            else
                result = null;

            return result;
        }

        public static void GetMeshPath(Mesh mesh, ref string AssetPath, ref string MeshName)
        {
            AssetPath = AssetDatabase.GetAssetPath(mesh);
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetPath);

            foreach (UnityEngine.Object obj in assets)
            {
                if (obj is Mesh && obj.Equals(mesh))
                {
                    Mesh subMesh = (Mesh)obj;
                    MeshName = subMesh.name;
                }
            }
        }

        public void GenerateMesh(System.Numerics.Vector3[] vertices)
        {
            TTriangulator tr = new TTriangulator(vertices);
            int[] indices = tr.Triangulate();

            Vector3[] verticesUnity = new Vector3[vertices.Length];

            for(int i = 0; i < vertices.Length; i++)
                verticesUnity[i] = TUtils.CastToUnity(vertices[i]);

            Vector2[] uvs = new Vector2[vertices.Length];

            for (int i = 0; i < uvs.Length; i++)
                uvs[i] = new Vector2(vertices[i].X, vertices[i].Z);

            Mesh = new Mesh();
            Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Mesh.vertices = verticesUnity;
            Mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            Mesh.uv = uvs;
            Mesh.RecalculateNormals();
            Mesh.RecalculateBounds();
        }

        public void GenerateMesh_TriangleNet(List<PolygonVertices> polygonVertices)
        {
            CreateMeshTri CMT = new CreateMeshTri();
            var poly = new Polygon();
            Vertex[] vertexArray;

            for (int i = 0; i < polygonVertices.Count; i++)
            {
                vertexArray = GetPoints(polygonVertices[i].vertices);
                TerraUnity.TriangleNet.Geometry.Contour contour = new TerraUnity.TriangleNet.Geometry.Contour(vertexArray);

                if(polygonVertices[i].property == Property.Outer)
                    poly.Add(contour, false);
                else if (polygonVertices[i].property == Property.Inner)
                    poly.Add(contour, true);
            }

            var m = poly.Triangulate();
            var verts = CMT.ToVector3(m.Vertices);
            var tris = CMT.ToTriangles(m.Triangles);

            Vector2[] uvs = new Vector2[verts.Length];

            for (int i = 0; i < uvs.Length; i++)
                uvs[i] = new Vector2(verts[i].x, verts[i].y);

            Mesh = new Mesh();
            Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Mesh.vertices = verts;
            Mesh.SetIndices(tris, MeshTopology.Triangles, 0);
            Mesh.uv = uvs;
            Mesh.RecalculateNormals();
            Mesh.RecalculateBounds();
        }

        public Vertex[] GetPoints(System.Numerics.Vector3[] vertices)
        {
            var p = new List<Vertex>();

            for (int i = 0; i < vertices.Length; i++)
                p.Add(new Vertex(vertices[i].X, vertices[i].Z));

            return p.ToArray();
        }

        public static void GenerateMesh2(Vector3[] vertices)
        {
            PointF[] m_Points = new PointF[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
                m_Points[i] = new PointF(vertices[i].z, vertices[i].x);

            // Make a Polygon
            TPolygon pgon = new TPolygon(m_Points);

            // Triangulate.
            List<TTriangle> triangles = pgon.Triangulate();

            // Draw the triangles.
            int[] indices = new int[triangles.Count * 3];
        }
    }
}
#endif
#endif

