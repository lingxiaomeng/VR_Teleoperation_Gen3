using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Vision;
using RosMessageTypes.Manipulation;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
public class ObjectPoseSub : MonoBehaviour
{
    // Start is called before the first frame update
    ROSConnection m_ROS;
    TFSystem tf;


    [SerializeField]
    Transform camera_optical_link;

    [SerializeField]
    Transform camera_base_link;

    [SerializeField]
    Transform robot_base_link;

    public bool hide_all = false;
    public bool get_tf = false;

    string tf_srv = "/lookupTF";

    static Dictionary<long, string> object_id2name = new Dictionary<long, string> { {9,"AlphabetSoup"      },
                                                                            {10,"Ketchup"          },
                                                                            {11,"Pineapple"        },
                                                                            {12,"BBQSauce"         },
                                                                            {13,"MacaroniAndCheese"},
                                                                            {14,"Popcorn"          },
                                                                            {15,"Butter"           },
                                                                            {16,"Mayo"             },
                                                                            {17,"Raisins"          },
                                                                            {18,"Cherries"         },
                                                                            {19,"Milk"             },
                                                                            {20,"SaladDressing"    },
                                                                            {21,"ChocolatePudding" },
                                                                            {22,"Mushrooms"        },
                                                                            {23,"Spaghetti"        },
                                                                            {24,"Cookies"          },
                                                                            {25,"Mustard"          },
                                                                            {26,"TomatoSauce"      },
                                                                            {27,"Corn"             },
                                                                            {28,"OrangeJuice"      },
                                                                            {29,"Tuna"             },
                                                                            {30,"CreamCheese"      },
                                                                            {31,"Parmesan"         },
                                                                            {32,"Yogurt"           },
                                                                            {33,"GranolaBars"      },
                                                                            {34,"Peaches"          },
                                                                            {35,"GreenBeans"       },
                                                                            {36,"PeasAndCarrots"   } };

    void Start()
    {
        m_ROS = ROSConnection.GetOrCreateInstance();
        m_ROS.Subscribe<Detection3DArrayMsg>("/dope/detected_objects",ObjectPosesUpdate);
        m_ROS.RegisterRosService<TFtransformRequest, TFtransformResponse>(tf_srv);
        lookupTF("base_link", "l515_color_optical_frame");


        HashSet<string> unused_objects = new HashSet<string>(object_id2name.Values);

        foreach (string object_name in unused_objects)
        {
            GameObject real_object = GameObject.Find(object_name);
            GameObject virtual_object = GameObject.Find(object_name + "_virtual");
            if (real_object != null)
            {
                Debug.Log("hidden" + object_name);
                // real_object.transform.SetPositionAndRotation(new Vector3(0, -10, 0), new Quaternion(0, 0, 0, 1));
                real_object.GetComponent<KeepPosition>().keep_aligned = true;
                virtual_object.GetComponent<Rigidbody>().useGravity = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
    void tf_callback(TFtransformResponse response)
    {
        Debug.Log("tf received");
        if (response.error.error == 0)
        {
            if(response.target_frame == "base_link" && response.source_frame == "l515_color_optical_frame")
            {
                Vector3 camera_color_position = response.transform.translation.From<FLU>();
                Quaternion camera_color_rotation = response.transform.rotation.From<FLU>();
                Matrix4x4 t_base_camera = new Matrix4x4();
                t_base_camera.SetTRS(camera_color_position, camera_color_rotation, new Vector3(1,1,1));
                Matrix4x4 new_t = robot_base_link.localToWorldMatrix * t_base_camera * camera_optical_link.localToWorldMatrix.inverse * camera_base_link.localToWorldMatrix;
                camera_base_link.SetPositionAndRotation(new_t.GetPosition(),new_t.rotation);
    
                get_tf = true;
            }
        }
        else
        {
            Debug.Log(string.Format("tf Call back {0}", response.error.error));
        }
        
    }

    void lookupTF(string target_frame, string source_frame)
    {
        TFtransformRequest request = new TFtransformRequest();
        request.source_frame = source_frame;
        request.target_frame = target_frame;
        m_ROS.SendServiceMessage<TFtransformResponse>(tf_srv, request, tf_callback);
    }

    public void SetHideAll(bool hide_all)
    {
        this.hide_all = hide_all;
    }
    
    void ObjectPosesUpdate(Detection3DArrayMsg msg)
    {


       //ssss Debug.Log("new Detection");
        HashSet<string> unused_objects = new HashSet<string>(object_id2name.Values);
        if (!hide_all)
        {
            for (var i = 0; i < msg.detections.Length; i++)
            {
                Detection3DMsg object_detection = msg.detections[i];


                for (var j = 0; j < object_detection.results.Length; j++)
                {
                    ObjectHypothesisWithPoseMsg result = object_detection.results[j];
                    //float x = (float)result.pose.pose.position.x;
                    //float y = (float)result.pose.pose.position.y;
                    //float z = (float)result.pose.pose.position.z;

                    //float ox = (float)result.pose.pose.orientation.x;
                    //float oy = (float)result.pose.pose.orientation.y;
                    //float oz = (float)result.pose.pose.orientation.z;
                    //float ow = (float)result.pose.pose.orientation.w;
                    //Debug.Log(string.Format("{0}th Detection {1}th result: position {2} id {3}", i, j, result.pose, result.id));
                    string object_name = object_id2name[result.id];
                    unused_objects.Remove(object_name);
                    GameObject real_object = GameObject.Find(object_name);
                    //Debug.Log("find:" + object_name);
                    real_object.GetComponent<KeepPosition>().setID(result.id);
                    real_object.GetComponent<KeepPosition>().setObjectName(object_name);
                    //Vector3 object_position_relative = new Vector3(-y, z, -x);
                    Vector3 object_position_relative = result.pose.pose.position.From<FLU>();
                    //Quaternion objcet_oriention_relative = new Quaternion(oy, -oz, -ox, ow);
                    Quaternion objcet_oriention_relative = result.pose.pose.orientation.From<FLU>();
                    Vector3 object_position_abs = camera_optical_link.TransformPoint(object_position_relative);
                    Quaternion tmp = new Quaternion(0.5f, -0.5f, 0.5f, -0.5f);
                    //Debug.Log(object_position_abs);
                    Quaternion objcet_oriention_abs = camera_optical_link.rotation * objcet_oriention_relative * tmp;
                    real_object.transform.SetPositionAndRotation(object_position_abs, objcet_oriention_abs);

                }

            }
        }
        foreach (string object_name in unused_objects)
        {
            GameObject real_object = GameObject.Find(object_name);
            GameObject virtual_object = GameObject.Find(object_name + "_virtual");
            if (real_object != null)
            {
                //Debug.Log("hidden" + object_name);
                real_object.transform.SetPositionAndRotation(new Vector3(real_object.transform.position.x, -10, real_object.transform.position.z), new Quaternion(0, 0, 0, 1));
                real_object.GetComponent<KeepPosition>().keep_aligned = true;
                virtual_object.GetComponent<Rigidbody>().useGravity = false;
            }
        }
    }
}

//header:
//seq: 0
//        stamp:
//secs: 0
//          nsecs: 0
//        frame_id: ''
//      height: 0
//      width: 0
//      fields:[]
//is_bigendian: False
//point_step: 0
//      row_step: 0
//      data:[]
//is_dense: False
//-
//header: 
//      seq: 0
//      stamp:
//secs: 0
//        nsecs: 0
//      frame_id: ''
//    results:
//-
//  id: 20
//        score: 0.41980862617492676
//        pose:
//pose:
//position:
//x: 0.08724413568442575
//              y: -0.06792586288493294
//              z: 0.7625914126737721
//            orientation:
//x: -0.3009118587585009
//              y: 0.5411228754569142
//              z: -0.4596837398577745
//              w: 0.6366544951746709
//          covariance:[0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]
//    bbox:
//center:
//position:
//x: 0.08724413568442575
//          y: -0.06792586288493294
//          z: 0.7625914126737721
//        orientation:
//x: -0.3009118587585009
//          y: 0.5411228754569142
//          z: -0.4596837398577745
//          w: 0.6366544951746709
//      size:
//x: 0.05320600032806397
//        y: 0.024230999946594237
//        z: 0.10359000205993653
//    source_cloud:
//header:
//seq: 0
//        stamp:
//secs: 0
//          nsecs: 0
//        frame_id: ''
//      height: 0
//      width: 0
//      fields:[]
//is_bigendian: False
//point_step: 0
//      row_step: 0
//      data:[]
//is_dense: False
//-- -