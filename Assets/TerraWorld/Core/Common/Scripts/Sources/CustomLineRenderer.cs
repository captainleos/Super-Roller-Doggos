using UnityEngine;
using System.Collections.Generic;
using System.Linq;
 

public class CustomLineRenderer : MonoBehaviour
{
	private Mesh _mesh;
    private int[] m_Indices;
    private Vector2[] m_UVs;
    [HideInInspector] public List<Vector3> m_Points;
    public float width = 1.0f;

    public Mesh Mesh { get => _mesh;}

    public void UpdateMesh ()
    {
        if (m_Points == null) return;

        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }
            Vector3[] vertices = _mesh.vertices;
        Vector3[] normals  = _mesh.normals;
        Color[] colors = _mesh.colors;
        Vector3 oldTangent = Vector3.zero;
		
        for (int i = 0; i < m_Points.Count - 1; i++)
        {
			Vector3 faceNormal = Vector3.up;
            Vector3 dir     = (m_Points[i + 1] - m_Points[i]);
            Vector3 tangent = Vector3.Cross(dir, faceNormal).normalized;
            Vector3 offset  = (oldTangent + tangent).normalized * width / 2.0f;
 
            vertices[i * 2]     = m_Points[i] - offset;
            vertices[i * 2 + 1] = m_Points[i] + offset;          
            normals[i * 2]      = normals[i * 2 + 1] = faceNormal;
			
            if (i == m_Points.Count - 2)
            {
                // last two points
                vertices[i * 2 + 2] = m_Points[i + 1] - tangent * width / 2.0f;
                vertices[i * 2 + 3] = m_Points[i + 1] + tangent *width / 2.0f;
                normals[i * 2 + 2]  = normals[i * 2 + 3] = faceNormal;
            }

            oldTangent = tangent;
        }
		
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs = m_UVs;
		List<float> distance = new List<float>();
		
		for (int i = 0 ; i < uvs.Length; i++)
        {	
			if (i < (m_Points.Count - 1))
				distance.Add(Vector3.Distance(m_Points[i], m_Points[i + 1]));
		}
		
		float roadLength = distance.Sum();
		float passedLength = 0.0f;
		
		for (int i = 1 ; i < m_Points.Count; i++)
        {
			uvs[i * 2] = uvs[i * 2 + 1] = new Vector2 ((distance[i - 1] / roadLength) + passedLength, 0.0f);
			passedLength += distance[i - 1] / roadLength;
			uvs[i * 2 + 1].y = 1.0f;
		}

        _mesh.vertices = vertices;
        _mesh.normals = normals;
        _mesh.colors = colors;
        _mesh.uv = uvs;
        _mesh.SetIndices(m_Indices, MeshTopology.Triangles, 0);
        _mesh.SetTriangles(m_Indices, 0);
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = _mesh;
        GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();

        if(GetComponent<MeshCollider>() != null)
        {
            GetComponent<MeshCollider>().sharedMesh = null;
            GetComponent<MeshCollider>().sharedMesh = _mesh;
        }
    }

    public void SetVertexCount (int aCount)
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        aCount = Mathf.Clamp(aCount, 0, 0xFFFF / 2);
        
		if (m_Points.Count > aCount)
			m_Points.RemoveRange(aCount,m_Points.Count-aCount);
       
		while (m_Points.Count < aCount)
            m_Points.Add(new Vector3());

        _mesh.vertices = new Vector3[m_Points.Count * 2];
        _mesh.normals = new Vector3[m_Points.Count * 2];
        _mesh.colors = new UnityEngine.Color[m_Points.Count * 2];
        m_Indices = new int[(m_Points.Count * 2 - 2) * 3];
        m_UVs = new Vector2[m_Points.Count * 2];

        int j = 0;

        for (int i = 0; i < m_Points.Count * 2 - 3; i += 2, j++)
        {
            m_Indices[i * 3] = j * 2;
            m_Indices[i * 3 + 1] = j * 2 + 1;
            m_Indices[i * 3 + 2] = j * 2 + 2;

            m_Indices[i * 3 + 3] = j * 2 + 1;
            m_Indices[i * 3 + 4] = j * 2 + 3;
            m_Indices[i * 3 + 5] = j * 2 + 2;
        }

        for (int i = 0; i < m_Points.Count; i++)
        {
            m_UVs[i * 2]       = m_UVs[i * 2 + 1] = new Vector2((float)i / (m_Points.Count - 1), 0);
            m_UVs[i * 2 + 1].y = 1.0f;
        }
    }
	
    public void SetPosition (int aIndex, Vector3 aPosition)
    {
        if (aIndex < 0 || aIndex >= m_Points.Count) return;
            m_Points[aIndex] = aPosition;
    }
 
    public void SetWidth (float aStartWidth, float aEndWidth)
    {
        if (m_Points == null) return;
        width = aStartWidth;
    }

    public void OnValidate()
    {
        if (m_Points == null || m_Points.Count == 0) return;

        SetWidth(width, width);
        SetVertexCount(m_Points.Count);
        UpdateMesh();
    }
}

