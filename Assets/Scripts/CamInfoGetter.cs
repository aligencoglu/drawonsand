using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamInfoGetter : MonoBehaviour
{
    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode |= DepthTextureMode.Depth;
    }
}
