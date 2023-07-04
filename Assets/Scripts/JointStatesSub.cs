using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Sensor;
using UnityEngine;

//name:
//-finger_joint
//- joint_1
//- joint_2
//- joint_3
//- joint_4
//- joint_5
//- joint_6
//- joint_7
//position:[0.7999324528255034, 6.621527446792896e-05, 0.26023301644624475, 3.140075439345331, -2.270119695092509, 0.00035816745351091583, 0.9596189165653657, 1.5709311144747353]

public class JointStatesSub : MonoBehaviour
{
    // Start is called before the first frame update
    ROSConnection m_ROS;
    const int k_NumRobotJoints = 7;

    public static readonly string[] LinkNames =
        { "base_link/shoulder_link", "/half_arm_1_link", "/half_arm_2_link", "/forearm_link", "/spherical_wrist_1_link", "/spherical_wrist_2_link", "/bracelet_link" };

    public static Transform base_link_transform;

    public static float finger_value = 0;

    [SerializeField]
    GameObject my_gen3;

    ArticulationBody[] m_JointArticulationBodies;
    ArticulationBody[] m_GrasperArticulationBodies;
    void Start()
    {

        m_ROS = ROSConnection.GetOrCreateInstance();
        m_ROS.Subscribe<JointStateMsg>("/my_gen3/joint_states", JointStatesUpdate);
        //double[] jointstates ={ 0.7999324528255034, 6.621527446792896e-05, 0.26023301644624475, 3.140075439345331, -2.270119695092509, 0.00035816745351091583, 0.9596189165653657, 1.5709311144747353 };
        m_JointArticulationBodies = new ArticulationBody[k_NumRobotJoints];
        m_GrasperArticulationBodies = new ArticulationBody[6];
        base_link_transform = my_gen3.transform.Find("base_link");

        var linkName = string.Empty;
        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            linkName += LinkNames[i];
            m_JointArticulationBodies[i] = my_gen3.transform.Find(linkName).GetComponent<ArticulationBody>();
        }
        var finger_joint                = linkName + "/end_effector_link/robotiq_arg2f_base_link/left_outer_knuckle";
        var left_inner_knuckle_joint    = linkName + "/end_effector_link/robotiq_arg2f_base_link/left_inner_knuckle";
        var left_inner_finger_joint     = linkName + "/end_effector_link/robotiq_arg2f_base_link/left_outer_knuckle/left_outer_finger/left_inner_finger";
        var right_outer_knuckle_joint   = linkName + "/end_effector_link/robotiq_arg2f_base_link/right_outer_knuckle";
        var right_inner_knuckle_joint   = linkName + "/end_effector_link/robotiq_arg2f_base_link/right_inner_knuckle";
        var right_inner_finger_joint    = linkName + "/end_effector_link/robotiq_arg2f_base_link/right_outer_knuckle/right_outer_finger/right_inner_finger";
        m_GrasperArticulationBodies[0] = my_gen3.transform.Find(finger_joint).GetComponent<ArticulationBody>();
        m_GrasperArticulationBodies[1] = my_gen3.transform.Find(left_inner_knuckle_joint).GetComponent<ArticulationBody>();
        m_GrasperArticulationBodies[2] = my_gen3.transform.Find(left_inner_finger_joint).GetComponent<ArticulationBody>();
        m_GrasperArticulationBodies[3] = my_gen3.transform.Find(right_outer_knuckle_joint).GetComponent<ArticulationBody>();
        m_GrasperArticulationBodies[4] = my_gen3.transform.Find(right_inner_knuckle_joint).GetComponent<ArticulationBody>();
        m_GrasperArticulationBodies[5] = my_gen3.transform.Find(right_inner_finger_joint).GetComponent<ArticulationBody>();
    }

    // Update is called once per frame
    void JointStatesUpdate(JointStateMsg jointState)
    {
        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            var joint1XDrive = m_JointArticulationBodies[i].xDrive;
            //Debug.Log((float)jointstates[i + 1] * Mathf.Rad2Deg);
            joint1XDrive.target = (float)jointState.position[i] * Mathf.Rad2Deg;
            joint1XDrive.stiffness = 10000;
            joint1XDrive.damping = 100;
            joint1XDrive.forceLimit = 1000;
            m_JointArticulationBodies[i].xDrive = joint1XDrive;
            
        }
        finger_value = (float)(jointState.position[7] / 0.8);
        for (var i = 0; i < 6; i++)
        {
            var joint1XDrive = m_GrasperArticulationBodies[i].xDrive;
            //Debug.Log((float)jointstates[i + 1] * Mathf.Rad2Deg);
            joint1XDrive.target = (float)jointState.position[i+7] * Mathf.Rad2Deg;
            joint1XDrive.stiffness = 10000;
            joint1XDrive.damping = 100;
            joint1XDrive.forceLimit = 1000;
            m_GrasperArticulationBodies[i].xDrive = joint1XDrive;
        }
        
    }
}
