using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class ShapeDetection 
{
    const float LINE_MAX_ANGLE_DEVIATION = 22.5f;
    const float MAX_ALLOWED_STDDEV = 20;
    const float MAX_VARIANCE_FROM_RIGHT_ANGLE = 20;
    const float MAX_VARIANCE_FROM_INTERNAL_ANGLE = 25;
    const int MIN_LINE_INDEX_DISTANCE = 3;



    // --- DETECTING SHAPES ---

    public static bool Circle(Stroke stroke)
    {
        if (!stroke.isClosed()) return false;

        var angleStats = stroke.getAngleStats();
        float intAngle = (stroke.pointAmt - 4) / (float)(stroke.pointAmt - 2) * 180.0f;

        // heuristics for detecting circles
        // low angle standard deviation, and internal angles are near ideal internal angles
        bool cwise = angleStats[0] - intAngle < MAX_VARIANCE_FROM_INTERNAL_ANGLE;
        bool ccwise = 180 - angleStats[0] - intAngle < MAX_VARIANCE_FROM_INTERNAL_ANGLE;
        
        // could also return if it's a clockwise or counterclockwise circle here.

        return angleStats[1] < MAX_ALLOWED_STDDEV && (cwise || ccwise);
    }

    private static bool isLine(float angleMean, float angleStdDev)
    {
        return Mathf.Abs(angleMean - 180) <= LINE_MAX_ANGLE_DEVIATION && angleStdDev < MAX_ALLOWED_STDDEV;
    }

    public static bool Line(Stroke stroke)
    {
        if (stroke.isClosed()) return false;

        var angleStats = stroke.getAngleStats();
        // Debug.Log(Mathf.Abs(angleStats[0] - 180) + ", " + angleStats[1]);
        return isLine(angleStats[0], angleStats[1]);
    }

    public static List<int> Lines(Stroke stroke)
        ///
        /// Return a list of indices of stroke.points where the angle deviates enough
        /// to cut off a line. Anything in between these indices is considered a line.
        ///
    {
        List<int> deviations = new List<int>();
        int last_i = -1;
        for (int i = 1; i < stroke.pointAmt - 1; i++)
        {
            float angle = stroke.angleAt(i);
            if (Mathf.Abs(angle - 180) > LINE_MAX_ANGLE_DEVIATION)
            {
                int lastLineEnd = (deviations.Count == 0) ? 0 : deviations[deviations.Count - 1];

                if (i - lastLineEnd >= MIN_LINE_INDEX_DISTANCE)
                {
                    deviations.Add(i);
                    last_i = i;
                }
            }
        }
        

        return deviations;
    }

    public static bool Polygon(Stroke stroke, int sides) {
        int deviationCount = Lines(stroke).Count;

        Debug.Log((deviationCount + 1) + " sides found");
        return stroke.isClosed() && !Circle(stroke) && deviationCount == sides - 1;
    }

    public static bool Triangle(Stroke stroke)
    {
        return Polygon(stroke, 3);
    }

    public static bool Rectangle(Stroke stroke)
    {
        // TODO: Test this!
        if (!Polygon(stroke, 4)) return false;
        List<int> deviations = Lines(stroke);
        
        for (int i = 0; i < deviations.Count; i++)
        {
            Debug.Log(stroke.angleAt(deviations[i]));
            // check if each angle at a corner is close to 90 degrees, return false if not
            if (Mathf.Abs(stroke.angleAt(deviations[i]) - 90) > MAX_VARIANCE_FROM_RIGHT_ANGLE) return false;
        }
        return true;
        
    }

    // --- ESTIMATING SHAPES ---

    public static Vector3[] EstimateLine(Stroke stroke, int start, int end)
    {
        Vector3[] endpoints = new Vector3[2];
        endpoints[0] = stroke.points[start];
        endpoints[1] = stroke.points[end];

        return endpoints;
    }

    public static Vector3[] EstimateLine(Stroke stroke)
    {
        return EstimateLine(stroke, 0, stroke.pointAmt-1);
    }

    public static Tuple<Vector3, float> EstimateCircle(Stroke stroke)
        ///
        /// Return a tuple of Vector3 and float, representing the center and radius of the circle best fit to the given stroke.
        ///
    {
        if (stroke.pointAmt <= 6)
        {
            Debug.LogError("Stroke too small to estimate circle!");
            return null;
        }

        // strategy: find three points on the stroke equidistant to each other, and find their center of mass.
        // could do this with least squares, but gonna go easier for this one.
        float length = stroke.length;

        float currLen = 0;

        int[] tripoints = new int[3];
        int i_tripoints = 0;

        for (int i = 1; i < stroke.pointAmt; i++)
        {
            currLen += (stroke.points[i] - stroke.points[i - 1]).magnitude;
            if (currLen >= length * 0.33f)
            {
                tripoints[i_tripoints] = i;
                currLen = 0;
                i_tripoints++;
                // is it possible for this to increment i_tripoints beyond 2?
            }
        }

        Vector3 center = (
            stroke.points[tripoints[0]]
            + stroke.points[tripoints[1]]
            + stroke.points[tripoints[2]]
        ) * 0.33f;

        float radius = (
            (stroke.points[tripoints[0]] - center).magnitude
            + (stroke.points[tripoints[1]] - center).magnitude
            + (stroke.points[tripoints[2]] - center).magnitude
            ) * 0.33f;

        return new Tuple<Vector3, float>(center, radius);
    }

    public static Vector3[] EstimateTriangle(Stroke stroke)
    {
        List<int> deviations = Lines(stroke);
        return new Vector3[] { stroke.points[0], stroke.points[deviations[0]], stroke.points[deviations[1]] };
    }

    public static Vector3[] EstimateRectangle(Stroke stroke)
        ///
        /// Return a tuple of Vector3's representing respectively the upper left and lower right corners of the rectangle estimated from the given stroke.
        /// Precondition: Lines(stroke).Count >= 3
        ///
    {

        // TODO: Take into consideration rotated rectangles. Shouldn't be too hard.
        List<int> deviations = Lines(stroke);
        Vector3[] oppositeCorners = new Vector3[2];
        oppositeCorners[0] = stroke.points[0];
        oppositeCorners[1] = stroke.points[deviations[1]];

        return oppositeCorners;

    }
}
