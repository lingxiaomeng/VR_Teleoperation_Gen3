using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Manipulation;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class GlobalFlags : MonoBehaviour
{
    // Start is called before the first frame update

    public static bool can_move_virtual = false;
    private static ManipulationSequenceRequest manipulationRequest;
    public static List<ManipulationMsg> manipulationMsgs;
    ROSConnection ros;

    string manipulation_srv_name = "manipulation";

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<ManipulationSequenceRequest, ManipulationSequenceResponse>(manipulation_srv_name);
        manipulationRequest = new ManipulationSequenceRequest();
        manipulationMsgs = new List<ManipulationMsg>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CanMoveVirtual()
    {
        can_move_virtual = true;

        GameObject[] virtual_objects = GameObject.FindGameObjectsWithTag("virtual_object");
        for(var i = 0; i < virtual_objects.Length; i++)
        {
            Rigidbody virtual_body = virtual_objects[i].GetComponent<Rigidbody>();
            //virtual_body.useGravity = false;
        }
        

        Debug.Log("Button Pressed: CanMoveVirtual");
    }

    public void Refresh_Position()
    {
        Debug.Log("Refresh_Position");

        manipulationMsgs.Clear();
        GameObject[] objects = GameObject.FindGameObjectsWithTag("object");
        for (var i = 0; i < objects.Length; i++)
        {
            KeepPosition keepPosition = objects[i].GetComponent<KeepPosition>();
            keepPosition.DisableGravity();
            keepPosition.keep_aligned = true;
            Debug.Log(keepPosition.keep_aligned);
        }
    }

    public void StartSim()
    {
        GameObject[] virtual_objects = GameObject.FindGameObjectsWithTag("virtual_object");
        for (var i = 0; i < virtual_objects.Length; i++)
        {
            Rigidbody virtual_body = virtual_objects[i].GetComponent<Rigidbody>();
            virtual_body.useGravity = true;
        }
        Debug.Log("Button Pressed: StartSim");
    }
   
    public void Call_manipulation_service()
    {
        manipulationRequest.manipulation_sequence = manipulationMsgs.ToArray();
        ros.SendServiceMessage<ManipulationSequenceResponse>(manipulation_srv_name, manipulationRequest, Manipulation_callback);
        Debug.LogWarning("Button Pressed: Call service");
    }

    void Manipulation_callback(ManipulationSequenceResponse response)
    {
        Refresh_Position();
        Debug.Log(string.Format("Service Call back {0}",response.result));
    }


}
