using RosMessageTypes.Manipulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


public class KeepPosition : MonoBehaviour
{
    // Start is called before the first frame update
    

    [SerializeField]
    GameObject virtual_object;

    private Rigidbody virtual_body;

    public bool keep_aligned = true;

    private ManipulationMsg request;

    private int state = 0;

    private long id = 0;
    private string object_name;


    void Start()
    {
        virtual_body = virtual_object.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (this.keep_aligned)
        {
            //Debug.Log(virtual_object.transform.position);
            //Debug.Log(this.transform.position);
            //this.transform.GetChild(0).transform.SetPositionAndRotation(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1));
            virtual_object.transform.SetPositionAndRotation(this.transform.position, this.transform.rotation);
        }
        if (this.state ==1) {
            virtual_body.useGravity = true;
            this.state = 2;
            Debug.Log("Falling Start");

        }
        else if(this.state == 2) {
            if (virtual_body.IsSleeping())
            {
                Debug.Log("Falling End");
                request.endpose.position = JointStatesSub.base_link_transform.InverseTransformPoint(virtual_object.transform.position).To<FLU>();
                Quaternion tmp = new Quaternion(0.5f, -0.5f, 0.5f, -0.5f);
                request.endpose.orientation = (Quaternion.Inverse(JointStatesSub.base_link_transform.rotation) * virtual_object.transform.rotation * Quaternion.Inverse(tmp)).To<FLU>();
                request.id = this.id;
                GlobalFlags.manipulationMsgs.Add(request);
                this.state = 0;
                virtual_body.isKinematic = true;
            }
        }
    }

    public void StopMove()
    {
        virtual_body.velocity = new Vector3(0, 0, 0);
        virtual_body.angularVelocity = new Vector3(0, 0, 0);
        virtual_body.useGravity = true;        
        this.state = 1;
        Debug.Log("Stop Move add request Start Falling");
        GlobalFlags.message += string.Format("{0} Stop Move: {1} and start falling\n", get_system_time(), this.object_name);
    }

    

    public void StartMove()
    {
        
        virtual_body.useGravity = false;
        virtual_body.isKinematic = false;
        this.keep_aligned = false;

        if (GlobalFlags.manipulationMsgs.Count > 0) {
            ManipulationMsg lastRequest = GlobalFlags.manipulationMsgs[GlobalFlags.manipulationMsgs.Count - 1];
            if(lastRequest.id == this.id)
            {
                GlobalFlags.manipulationMsgs.RemoveAt(GlobalFlags.manipulationMsgs.Count-1);
            }
        }

        this.request = new ManipulationMsg();
        request.startpose.position = JointStatesSub.base_link_transform.InverseTransformPoint(this.transform.position).To<FLU>();
        request.object_name = this.object_name;
        Debug.Log("Start Move");
        Debug.Log(JointStatesSub.base_link_transform.position);
        Debug.Log(this.transform.position);
        Debug.Log(JointStatesSub.base_link_transform.InverseTransformPoint(this.transform.position));
        Debug.Log(request.startpose.position);
        GlobalFlags.message += string.Format("{0} Start Move: {1}\n", get_system_time(),this.object_name);
        Quaternion tmp = new Quaternion(0.5f, -0.5f, 0.5f, -0.5f);
        request.startpose.orientation = (Quaternion.Inverse(JointStatesSub.base_link_transform.rotation) * this.transform.rotation* Quaternion.Inverse(tmp)).To<FLU>();

        //request.startpose.orientation = this.transform.rotation.To<FLU>();
        
    }

    public void setID(long id)
    {
        this.id = id;
    }

    public void setObjectName(string name)
    {
        this.object_name = name;
    }

    public void DisableGravity()
    {
        virtual_body.useGravity = false;
    }

    private string get_system_time()
    {
        return System.DateTime.Now.ToString("YYYY-MM-DD HH-mm-ss");
    }
}
