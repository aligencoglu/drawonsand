using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utils;

public class ShapeDetector : MonoBehaviour
{
    public MouseGetter mouseGetter;
    public int lineSampleFrequency = 5;

    private Tuple<Vector3, float> lastCircle = null;
    private Vector3[] lastLine = { Vector3.zero, Vector3.zero };
    private Vector3[] lastTri = { Vector3.zero, Vector3.zero, Vector3.zero };
    private Vector3[] lastRect = { Vector3.zero, Vector3.zero };
    Stroke currStroke = null;
    int i = -1;

    private void OnDrawGizmos()
    {
        const float scale = 10;
        if (currStroke != null)
        {
            Gizmos.color = UnityEngine.Color.gray;
            Vector3 lastPoint = Vector3.negativeInfinity;
            foreach(Vector3 point in currStroke.points)
            {
                Vector3 scaledPoint = point * scale;
                Gizmos.DrawSphere(scaledPoint, 0.1f);
                if (lastPoint != Vector3.negativeInfinity)
                {
                    Gizmos.DrawLine(lastPoint * scale, scaledPoint);
                }
                lastPoint = point;
            }

            bool flipColor = false;
            Gizmos.color = (flipColor) ? UnityEngine.Color.magenta : UnityEngine.Color.red;
            List<int> deviations = ShapeDetection.Lines(currStroke);

            int lastIndex = 0;
            for (int i = 0; i < deviations.Count; i++)
            {
                int currIndex = deviations[i];
                Vector3 from = currStroke.points[lastIndex] * scale;
                Vector3 to = currStroke.points[currIndex] * scale;
                Gizmos.DrawLine(from, to);
                lastIndex = currIndex;
                flipColor = !flipColor;
            }
            Gizmos.DrawLine(currStroke.points[lastIndex], currStroke.points[currStroke.pointAmt - 1]);
            
        }

        Gizmos.color = UnityEngine.Color.blue;
        if (lastCircle != null)
        {
            GizmosExtensions.DrawWireCircle(lastCircle.Item1 * scale, lastCircle.Item2 * scale, 20, Quaternion.LookRotation(Vector3.up));
        }

        if (lastLine != null)
        {
            Gizmos.DrawLine(lastLine[0] * scale, lastLine[1] * scale);
        }

        if (lastTri != null)
        {
            Vector3 corner1 = lastTri[0] * scale;
            Vector3 corner2 = lastTri[1] * scale;
            Vector3 corner3 = lastTri[2] * scale;
            Gizmos.DrawLine(corner1, corner2);
            Gizmos.DrawLine(corner2, corner3);
            Gizmos.DrawLine(corner3, corner1);
        }

        if (lastRect != null)
        {
            Vector3 corner1 = lastRect[0] * scale;
            Vector3 corner3 = lastRect[1] * scale;
            Vector3 corner2 = new Vector3(corner1.x, corner3.y, 0);
            Vector3 corner4 = new Vector3(corner3.x, corner1.y, 0);
            Gizmos.DrawLine(corner1, corner2);
            Gizmos.DrawLine(corner2, corner3);
            Gizmos.DrawLine(corner3, corner4);
            Gizmos.DrawLine(corner4, corner1);
        }

        
        
    }
    
    void Update()
    {

        // create stroke
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
                lastCircle = ShapeDetection.EstimateCircle(currStroke);
            } 
            else if (currStroke.Detect(ShapeDetection.Line))
            {
                Debug.Log("Line");
                lastLine = ShapeDetection.EstimateLine(currStroke);
            }
            else if (currStroke.Detect(ShapeDetection.Triangle))
            {
                Debug.Log("Triangle");
                lastTri = ShapeDetection.EstimateTriangle(currStroke);
            } 
            else if (currStroke.Detect(ShapeDetection.Rectangle))
            {
                Debug.Log("Rectangle");
                lastRect = ShapeDetection.EstimateRectangle(currStroke);
            }
            
            
        }
        //Debug.Log(i);
    }
    
    
}
