using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System;
using System.Linq;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class PointCloudSubscriber : MonoBehaviour
{

    ROSConnection m_ROS;

    public string point_cloud_topic;
    public PointCloudRenderer pointCloudRenderer;
    public float radius;
    public Transform camera_frame;


    private Vector3[] pcl;
    private Color[] pcl_color;

    string m_XChannel = "x";
    string m_YChannel = "y";
    string m_ZChannel = "z";

    string m_RgbChannel = "rgb";
    // Start is called before the first frame update
    void Start()
    {
        m_ROS = ROSConnection.GetOrCreateInstance();
        m_ROS.Subscribe<PointCloud2Msg>(point_cloud_topic, PointCloud_Callback);
    }

    // Update is called once per frame
    void Update()
    {

        //if (isMessageReceived)
        //{
        //    PointCloudRendering();
        //    isMessageReceived = false;
        //}
    }


    IEnumerator renderPointCloud(PointCloud2Msg message)
    {
        Dictionary<string, int> channelToIdx = new Dictionary<string, int>();
        for (int i = 0; i < message.fields.Length; i++)
        {
            channelToIdx.Add(message.fields[i].name, i);
        }
        pointCloudRenderer.ClearBuffers();
        pointCloudRenderer.SetCapacity((int)(message.data.Length / message.point_step));

        TFFrame frame = TFSystem.instance.GetTransform(message.header);

        Func<int, Color> colorGenerator = (int iPointStep) => Color.white;

        int rgbChannelOffset = (int)message.fields[channelToIdx[m_RgbChannel]].offset;
        colorGenerator = (int iPointStep) => new Color32
        (
            message.data[iPointStep + rgbChannelOffset + 2],
            message.data[iPointStep + rgbChannelOffset + 1],
            message.data[iPointStep + rgbChannelOffset],
            255
        );




        int xChannelOffset = (int)message.fields[channelToIdx[m_XChannel]].offset;
        int yChannelOffset = (int)message.fields[channelToIdx[m_YChannel]].offset;
        int zChannelOffset = (int)message.fields[channelToIdx[m_ZChannel]].offset;
        int sizeChannelOffset = 0;

        int maxI = message.data.Length / (int)message.point_step;

        pcl = new Vector3[maxI];
        pcl_color = new Color[maxI];


        //Debug.Log("start rendering points:");
        yield return null;
        int num = 0;
        int points_perframe = maxI / 25;
        for (int i = 0; i < maxI; i++)
        {
            int iPointStep = i * (int)message.point_step;
            var x = BitConverter.ToSingle(message.data, iPointStep + xChannelOffset);
            var y = BitConverter.ToSingle(message.data, iPointStep + yChannelOffset);
            var z = BitConverter.ToSingle(message.data, iPointStep + zChannelOffset);
            Vector3<FLU> rosPoint = new Vector3<FLU>(x, y, z);
            Vector3 unityPoint = rosPoint.toUnity;
            Vector3 wordPoint = camera_frame.TransformPoint(unityPoint);

            Color color = colorGenerator(iPointStep);

            pcl[i] = unityPoint;
            pcl_color[i] = color;
            pointCloudRenderer.AddPoint2(wordPoint, color, radius);
            if (i>num*points_perframe)
            {
                num++;
                yield return null;
                //Debug.Log("rendering:" + num);
            }
        }
        pointCloudRenderer.new_message = true;
        yield return null;
    }
    void PointCloud_Callback(PointCloud2Msg message) {
        Debug.Log("receive pointcloud");
        StartCoroutine(renderPointCloud(message));
    }

    private void OnDisable()
    {
        m_ROS.Unsubscribe(point_cloud_topic);
    }

    private void OnEnable()
    {
        m_ROS.Subscribe<PointCloud2Msg>(point_cloud_topic, PointCloud_Callback);
    }
}
