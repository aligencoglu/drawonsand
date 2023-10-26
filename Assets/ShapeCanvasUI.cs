using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShapeCanvasUI : MonoBehaviour
{
    public ShapeDetector shapeDetector;
    public Material canvasMaterial;
    public bool clear = false;

    public void setCanvasShaderSettings(ShapeHolder.ShapeType shapeType, Vector2[] points)
    {

        Debug.Log("Invoked with shape type " + shapeType);
        Vector4 input1 = Vector4.zero;
        Vector4 input2 = Vector4.zero;

        switch (shapeType)
        {
            case ShapeHolder.ShapeType.Circle:
                input1.w = 0;
                Vector2 center = points[0];
                float radius = points[1][0];
                input1.x = center.x;
                input1.y = center.y;
                input1.z = radius;
                break;
            case ShapeHolder.ShapeType.Line:
                input1.w = 1;
                Vector2 start = points[0];
                Vector2 end = points[1];
                input1.x = start.x;
                input1.y = start.y;
                input1.z = end.x;
                input2.x = end.y;
                break;
            case ShapeHolder.ShapeType.Triangle:
                input1.w = 2;
                Vector2 p1 = points[0];
                Vector2 p2 = points[1];
                Vector2 p3 = points[2];
                input1.x = p1.x;
                input1.y = p1.y;
                input1.z = p2.x;
                input2.x = p2.y;
                input2.y = p3.x;
                input2.z = p3.y;
                break;
            case ShapeHolder.ShapeType.Rectangle:
                input1.w = 3;
                Vector2 upperLeft = points[0];
                Vector2 lowerRight = points[1];
                input1.x = upperLeft.x;
                input1.y = upperLeft.y;
                input1.z = lowerRight.x;
                input2.x = lowerRight.y;
                break;
        }
        Debug.Log("Sending data: " + input1 + "; " + input2);
        canvasMaterial.SetVector("_Input", input1);
        canvasMaterial.SetVector("_Input2", input2);
        canvasMaterial.SetFloat("_Clear", clear ? 1 : 0);
    }
}
