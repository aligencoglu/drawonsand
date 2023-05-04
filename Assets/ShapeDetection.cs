using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShapeDetection 
{
    const float LINE_MAX_ANGLE_DEVIATION = 25;
    const float MAX_ALLOWED_STDDEV = 25;
    public static bool Circle(Stroke stroke)
    {
        if (!stroke.isClosed()) return false;

        var angleStats = stroke.getAngleStats();
        float intAngle = (stroke.pointAmt - 4) / (float)(stroke.pointAmt - 2) * 180.0f;

        // heuristics for detecting circles
        // low angle standard deviation, and internal angles are near ideal internal angles
        bool cwise = angleStats[0] - intAngle < 30;
        bool ccwise = 180 - angleStats[0] - intAngle < 30;

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
        const int MIN_LINE_LENGTH = 3;
        List<int> deviations = new List<int>();

        for (int i = 1; i < stroke.pointAmt - 1; i++)
        {
            float angle = stroke.angleAt(i);
            if (Mathf.Abs(angle - 180) > LINE_MAX_ANGLE_DEVIATION)
            {
                int lastLineEnd = (deviations.Count == 0) ? 0 : deviations[deviations.Count - 1];

                if (i - lastLineEnd >= MIN_LINE_LENGTH)
                    deviations.Add(i);
            }

        }

        return deviations;
    }

    public static bool Polygon(Stroke stroke, int sides) {
        return stroke.isClosed() && !Circle(stroke) && Lines(stroke).Count == sides - 1;
    }

    public static bool Triangle(Stroke stroke)
    {
        return Polygon(stroke, 3);
    }

    public static bool Rectangle(Stroke stroke)
    {
        return Polygon(stroke, 4);
    }





}
