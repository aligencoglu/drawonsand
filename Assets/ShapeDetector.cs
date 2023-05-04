using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class ShapeDetector : MonoBehaviour
{
    public MouseGetter mouseGetter;
    public Texture ballTex;
    Stroke currStroke = null;
    int i = -1;
    public int lineSampleFrequency = 5;
    List<float> circleMeanAngleData = new List<float>();
    List<float> circleAngleDevData = new List<float>();



    // Start is called before the first frame update
    void Start()
    {
        

    }

    private void OnDrawGizmos()
    {
        if (currStroke != null)
        {
            Vector3 lastPoint = Vector3.negativeInfinity;
            foreach(Vector3 point in currStroke.points)
            {
                Gizmos.DrawSphere(point * 10, 0.1f);
                if (lastPoint != Vector3.negativeInfinity)
                {
                    Gizmos.DrawLine(lastPoint * 10, point * 10);
                }
                lastPoint = point;
            }
        }
        
    }


    /*
    void Update()
    {
        currStroke = new Stroke();
        currStroke.addPoint(new Vector3(0, 0, 0));
        currStroke.addPoint(new Vector3(0, 0.1f, 0));
        currStroke.addPoint(new Vector3(0.1f, 0.1f, 0));
        currStroke.addPoint(new Vector3(0.1f, 0, 0));
        currStroke.addPoint(new Vector3(0, 0, 0));
        var angleStats = currStroke.getAngleStats();
        var lineStats = currStroke.getLengthStats();
        Debug.Log("Angle Mean & StdDev: " + angleStats[0] + " " + angleStats[1]);
        Debug.Log("Line Length: " + currStroke.length);
    }
    */

    // Update is called once per frame
    
    void Update()
    {
        Vector3 mouseScreenPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            currStroke = new Stroke(mouseScreenPos);
            i = 0;
        }
        if (!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0) && !currStroke.sameAsLastPoint(mouseScreenPos) && i != -1)
        {
            if (i == 0)
                currStroke.addPoint(mouseScreenPos);
            i = (i + 1) % lineSampleFrequency;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            i = -1;
            if (currStroke.isClosed())
            {
                //Debug.Log("Closed!");
                //Debug.Log("Angle Sum: " + currStroke.internalAngleSum);
                
            } else
            {
                Debug.Log("Open!");
            }

            var angleStats = currStroke.getAngleStats();
            var lineStats = currStroke.getLengthStats();
            Debug.Log("Angle Mean & StdDev: " + angleStats[0] + " " + angleStats[1]);
            float intAngle = (currStroke.pointAmt - 4) / (float)(currStroke.pointAmt - 2) * 180.0f;
            Debug.Log("Angle Mean for Perfect Sphere: " + intAngle);
            Debug.Log("Line Mean & StdDev: " + lineStats[0] + " " + lineStats[1]);


            //currStroke.isSmallPolygon();
            
            if (currStroke.isCircle())
            {
                Debug.Log("Circle");
            } 
            else if (currStroke.isLine())
            {
                Debug.Log("Line");
            }
            
        }
        //Debug.Log(i);
    }
    
    
}
