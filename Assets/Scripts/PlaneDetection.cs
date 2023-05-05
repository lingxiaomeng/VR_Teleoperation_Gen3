using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.PlaneDetection;

public class PlaneDetection : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform camera_color_frame;
    public GameObject planeObj;
    public Material material;
    Vector3[] vertices = new Vector3[4];
    public int numVertices = 4;
    int[] triangles=new int[] { 0,1,2,2,3,0,0,3,2,2,1,0};

    string plane_detection_srv = "/get_plane_corners";
    ROSConnection m_ROS;

    public static T AddOrGetComponent<T>(GameObject gameObject) where T : Component
    {
        return gameObject.TryGetComponent<T>(out var outComponent)
            ? outComponent
            : gameObject.AddComponent<T>();
    }

    void Start()
    {
        m_ROS = ROSConnection.GetOrCreateInstance();
        m_ROS.RegisterRosService<PlaneDetectionRequest, PlaneDetectionResponse>(plane_detection_srv);
        DetectPlane();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DetectPlane()
    {
        PlaneDetectionRequest request = new PlaneDetectionRequest();

        m_ROS.SendServiceMessage<PlaneDetectionResponse>(plane_detection_srv, request, plane_detection_callback);
    }

    void plane_detection_callback(PlaneDetectionResponse response)
    {
        Debug.Log("plane_detection_callback");

        Debug.Log(response);
        MeshFilter meshFilter = AddOrGetComponent<MeshFilter>(planeObj);

        Vector3 corner1 = response.corner1.From<FLU>();
        Vector3 corner2 = response.corner2.From<FLU>();
        Vector3 corner3 = response.corner3.From<FLU>();
        Vector3 corner4 = response.corner4.From<FLU>();
        vertices[0] = camera_color_frame.TransformPoint(corner1);
        vertices[1] = camera_color_frame.TransformPoint(corner2); ;
        vertices[2] = camera_color_frame.TransformPoint(corner3); ;
        vertices[3] = camera_color_frame.TransformPoint(corner4); ;

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;
        
        // Set vertices and triangles
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Calculate normals and UV coordinates (optional)
        mesh.RecalculateNormals();
        Vector2[] uv = new Vector2[numVertices];
        for (int i = 0; i < numVertices; i++)
        {
            uv[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
        mesh.uv = uv;

        // Add MeshRenderer component and assign material
        MeshRenderer meshRenderer = AddOrGetComponent<MeshRenderer>(planeObj);

        meshRenderer.material = material;

        // Add MeshCollider component and set mesh
        MeshCollider meshCollider = AddOrGetComponent<MeshCollider>(planeObj);

        meshCollider.sharedMesh = mesh;

        // Set transform
        planeObj.transform.position = new Vector3(0, 0, 0);
        planeObj.transform.rotation = Quaternion.identity;
        planeObj.transform.localScale = new Vector3(1, 1, 1);
    }
}


//corner1:
//x: -0.333516
//  y: 0.213302
//  z: 0.453852
//corner2:
//x: 0.317034
//  y: 0.181443
//  z: 0.47907
//corner3:
//x: 0.281787
//  y: -0.143034
//  z: 0.974727
//corner4:
//x: -0.368763
//  y: -0.111175
//  z: 0.949509