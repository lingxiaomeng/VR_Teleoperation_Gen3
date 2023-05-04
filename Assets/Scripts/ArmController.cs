using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Std;

public class ArmController : MonoBehaviour
{

    public GameObject scalpelObject;

    public GameObject endEffectorObject;

    public GameObject base_link;

    ROSConnection m_ROS;

    private bool follow = false;
    private bool tracking = false;
    // Start is called before the first frame update

    private float last_Scale_value = 0;
    private string follow_topic_name = "gen3_follow";
    private string grasp_topic_name = "gen3_grasp";

    void Start()
    {
        m_ROS = ROSConnection.GetOrCreateInstance();
        m_ROS.RegisterPublisher<PoseMsg>(follow_topic_name);
        m_ROS.RegisterPublisher<Float32Msg>(grasp_topic_name);

    }

    // Update is called once per frame
    void Update()
    {
        if (follow == false && tracking == true)
        {
            follow = true;
            last_Scale_value = scalpelObject.transform.localScale[0];
            Debug.Log("Start Tracking\n");
        }

        if (follow == true && tracking == false)
        {
            follow = false;
            // scalpelObject.transform.rotation = endEffectorObject.transform.rotation;
            // scalpelObject.transform.position = endEffectorObject.transform.position + endEffectorObject.transform.rotation * scalpelOffset;
            PoseMsg pose = new PoseMsg();
            Vector3 position = new Vector3(0, 0, 0);
            pose.position = position.To<FLU>();
            m_ROS.Publish(follow_topic_name, pose);
            Debug.Log("Stop Tracking\n");
        }
        UnityEngine.Quaternion joystick_transform = UnityEngine.Quaternion.Euler(0, 0, 0);
        // Debug.Log(joystick_transform.eulerAngles);

        if (follow)
        {
            if (scalpelObject.transform.localScale[0] != last_Scale_value)
            {
                Float32Msg float32Msg = new Float32Msg();
                float32Msg.data = 1-(scalpelObject.transform.localScale[0]-0.05f)*20;
                m_ROS.Publish(grasp_topic_name, float32Msg);

            }
            else
            {
                PoseMsg pose = new PoseMsg();
                Vector3 position = base_link.transform.InverseTransformPoint(scalpelObject.transform.position);
                Quaternion quaternion = Quaternion.Inverse(base_link.transform.rotation) * scalpelObject.transform.rotation;
                pose.position = position.To<FLU>();
                pose.orientation = quaternion.To<FLU>();
                m_ROS.Publish("gen3_follow", pose);
            }

        }

        if (follow == false && tracking == false)
        {
            scalpelObject.transform.rotation = endEffectorObject.transform.rotation;
            scalpelObject.transform.position = endEffectorObject.transform.position;
            float finger_value = (1-JointStatesSub.finger_value)/20+0.05f;
            scalpelObject.transform.localScale = new Vector3(finger_value, finger_value, finger_value);
        }
    }

    public void SetTracking(bool value)
    {
        tracking = value;
    }
}
