using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeControl : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject pointcloud;

    public GameObject platonic;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetVideoMode()
    {
        // pointcloud.SetActive(true);
        platonic.SetActive(true);
    }

    public void SetInteractiveMode()
    {
        // pointcloud.SetActive(false);
        platonic.SetActive(false);
    }
}
