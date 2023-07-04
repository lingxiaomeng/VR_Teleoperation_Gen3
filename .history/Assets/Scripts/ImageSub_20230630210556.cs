using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Sensor;
using UnityEngine.UI;
using UnityEngine;

public class ImageSub : MonoBehaviour
{

    // Start is called before the first frame update
    [SerializeField]
    string image_topic;
    ROSConnection m_ROS;
    Texture2D texRos;

    RawImage image;
    void Start()
    {
        m_ROS = ROSConnection.GetOrCreateInstance();
        image = GetComponent<RawImage>();
        m_ROS.Subscribe<CompressedImageMsg>(image_topic, image_sub);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void image_sub(CompressedImageMsg img)
    {
       // Debug.Log("get image");
        // stopping the prev output and clearing the texture
        // if (texRos != null) {
        //     display.texture = null;
        //     // texRos.Stop();
        //     texRos = null;
        // } else {
        // RenderTexture rendtextRos = new RenderTexture(640, 480, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
        // rendtextRos.Create();
        // rendtextRos.
        texRos = new Texture2D(1280, 720, TextureFormat.RGB24, false); // , TextureFormat.RGB24
      //  BgrToRgb(img.data);
        //BgrToRgb(img.data);

        texRos.LoadImage(img.data);
        Destroy(image.texture);

        image.texture = texRos;
        //display.texture = texRos;
    }

    public void BgrToRgb(byte[] data)
    {
        for (int i = 0; i < data.Length; i += 3)
        {
            byte dummy = data[i];
            data[i] = data[i + 2];
            data[i + 2] = dummy;
        }
    }

    private void OnDisable()
    {
        m_ROS.Unsubscribe(image_topic);
    }

    private void OnEnable()
    {
        if(image_topic != null)
            Debug.Log(image_sub);
            m_ROS.Subscribe<CompressedImageMsg>(image_topic, image_sub);
    }
}
