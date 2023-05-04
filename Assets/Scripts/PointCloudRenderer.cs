using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudRenderer : MonoBehaviour
{

    // Mesh stores the positions and colours of every point in the cloud
    // The renderer and filter are used to display it
    Mesh mesh;
    MeshRenderer meshRenderer;
    MeshFilter mf;

    // The size, positions and colours of each of the pointcloud
    public float pointSize = 0.01f;
    public bool new_message = false;

    List<Vector3> m_Vertices = new List<Vector3>();
    List<Vector3> m_UVRs = new List<Vector3>(); // texture UV and point radius
    List<Color> m_Colors32 = new List<Color>();
    List<int> m_Triangles = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        mf = gameObject.GetComponent<MeshFilter>();
        mesh = new Mesh
        {
            // Use 32 bit integer values for the mesh, allows for stupid amount of vertices (2,147,483,647 I think?)
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
    }

    // Update is called once per frame
    void UpdateMesh()
    {
        if (new_message)
        {
            mesh.Clear();
            mesh.indexFormat = m_Vertices.Count < 65536 ? UnityEngine.Rendering.IndexFormat.UInt16 : UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = m_Vertices.ToArray();
            mesh.colors = m_Colors32.ToArray();
            mesh.SetUVs(0, m_UVRs.ToArray());
            mesh.triangles = m_Triangles.ToArray();
            mf.mesh = mesh;
            new_message = false;
        }
    }
    void UpdateMesh2()
    {
        if (new_message)
        {
            mesh.Clear();
            mesh.vertices = m_Vertices.ToArray();
            mesh.colors = m_Colors32.ToArray();
            int[] indices = new int[m_Vertices.Count];
            for (int i = 0; i < m_Vertices.Count; i++){
                indices[i] = i;
            }
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            mf.mesh = mesh;
            new_message = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = camera_frame.position;
        //transform.rotation = camera_frame.rotation;
        //meshRenderer.material.SetFloat("_PointSize", pointSize);
        UpdateMesh2();
    }

    public void SetCapacity(int numPoints)
    {
        m_Vertices.Capacity = numPoints * 4;
        m_UVRs.Capacity = numPoints * 4;
        m_Colors32.Capacity = numPoints * 4;
    }
    public void AddPoint(Vector3 point, Color32 color, float radius)
    {
        int start = m_Vertices.Count;

        for (int Idx = 0; Idx < 4; ++Idx)
        {
            m_Vertices.Add(point);
            m_Colors32.Add(color);
        }

        m_UVRs.Add(new Vector3(0, 0, radius));
        m_UVRs.Add(new Vector3(0, 1, radius));
        m_UVRs.Add(new Vector3(1, 0, radius));
        m_UVRs.Add(new Vector3(1, 1, radius));

        m_Triangles.Add(start + 0);
        m_Triangles.Add(start + 1);
        m_Triangles.Add(start + 2);
        m_Triangles.Add(start + 3);
        m_Triangles.Add(start + 2);
        m_Triangles.Add(start + 1);
    }
    public void AddPoint2(Vector3 point, Color32 color, float radius)
    {

        m_Vertices.Add(point);
        m_Colors32.Add(color);
    }


    public void ClearBuffers()
    {
        m_Vertices.Clear();
        m_Colors32.Clear();
        m_UVRs.Clear();
        m_Triangles.Clear();
    }
}
