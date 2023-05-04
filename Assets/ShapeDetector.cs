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

    private void OnDrawGizmos()
    {
        if (currStroke != null)
        {
            Vector3 lastPoint = Vector3.negativeInfinity;
            foreach(Vector3 point in currStroke.points)
            {
                Vector3 scaledPoint = point * 10;
                Gizmos.DrawSphere(scaledPoint, 0.1f);
                if (lastPoint != Vector3.negativeInfinity)
                {
                    Gizmos.DrawLine(lastPoint * 10, scaledPoint);
                }
                lastPoint = point;
            }


            Gizmos.color = UnityEngine.Color.red;
            List<int> deviations = ShapeDetection.Lines(currStroke);

            int lastIndex = 0;
            for (int i = 0; i < deviations.Count; i++)
            {
                int currIndex = deviations[i];
                Vector3 from = currStroke.points[lastIndex] * 10;
                Vector3 to = currStroke.points[currIndex] * 10;
                Gizmos.DrawLine(from, to);
                lastIndex = currIndex;
            }
            Gizmos.DrawLine(currStroke.points[lastIndex], currStroke.points[currStroke.pointAmt - 1]);
        }
        
    }
    
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
            
            if (currStroke.Detect(ShapeDetection.Circle))
            {
                Debug.Log("Circle");
            } 
            else if (currStroke.Detect(ShapeDetection.Line))
            {
                Debug.Log("Line");
            }
            else if (currStroke.Detect(ShapeDetection.Triangle))
            {
                Debug.Log("Triangle");
            } 
            else if (currStroke.Detect(ShapeDetection.Rectangle))
            {
                Debug.Log("Rectangle");
            }
            
        }
        //Debug.Log(i);
    }
    
    
}
