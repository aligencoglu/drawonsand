using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour
{
    public Vector3[] controlPoints;

    public int selectedControlPoint;
    [Range(0, 1)] public float speed = 0.1f;

    // Update is called once per frame
    void Update()
    {
        // set control point from input
        if (Input.GetKeyDown(KeyCode.S))
        {
            selectedControlPoint = 0;
        } 
        else if (Input.GetKeyDown(KeyCode.W))
        {
            selectedControlPoint = 1;
        }


        if (!(selectedControlPoint >= 0 && selectedControlPoint < controlPoints.Length))
        {
            selectedControlPoint = 0;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(controlPoints[selectedControlPoint]), speed);
    }
}
