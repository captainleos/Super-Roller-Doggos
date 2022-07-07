using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteInEditMode]
    public class MeshTools : MonoBehaviour
    {

        [HideInInspector] public Vector3[] verts;
        [HideInInspector] public bool meshDerty = false;
        private bool editMode = false;

#if UNITY_EDITOR
        public bool EditMode
        {
            get
            {
                return editMode;
            }
            set
            {
                editMode = value;
                if (EditMode) GenerateGizmoes();
                else DistroyGizmoes();
            }
        }

        void OnDisable()
        {
            DistroyGizmoes();
        }

        void Update()
        {
            if (meshDerty) RegenerateMesh();
        }

        void GenerateGizmoes()
        {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            verts = mesh.vertices;

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 vertPos = transform.TransformPoint(verts[i]);
                GameObject handle = new GameObject("Vertex " + i.ToString());
                handle.transform.position = vertPos;
                handle.transform.parent = transform;
                MeshToolsGizmo editMeshGizmo = handle.AddComponent<MeshToolsGizmo>();
                editMeshGizmo._parent = this;
                editMeshGizmo._vertexIndex = i;
            }
        }

        void DistroyGizmoes()
        {
            Transform[] children = GetComponentsInChildren<Transform>();

            foreach (Transform Child in children)
            {
                if (Child.gameObject != this.gameObject)
                {
                    MeshToolsGizmo editMeshGizmo = Child.gameObject.GetComponent<MeshToolsGizmo>();
                    if (editMeshGizmo != null) DestroyImmediate(Child.gameObject);
                }
            }
        }

        void RegenerateMesh()
        {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.vertices = verts;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
#endif
    }
}

