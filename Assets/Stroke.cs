using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Stroke
    ///
    /// Represents a single stroke drawn by the player.
    ///
{
    public Stroke()
    {
        this.points = new List<Vector3>();
        initValues();
    }

    public Stroke(List<Vector3> points)
    {
        this.points = points;
        initValues();
    }

    public Stroke(Vector3[] points)
    {
        this.points = new List<Vector3>(points);
        initValues();
    }

    public List<Vector3> points { get; private set; } // points making up this stroke
    private int pointAmt; // amount of points
    public float length { get; private set; } // total length of the stroke
    private float lineMeanLength; // mean length between individual points
    private float lineSqDistFromMeanLength; // sum of the squared distances of the lengths between individual points from the mean length between individual points


    public void addPoint(Vector3 newPoint)
        ///
        /// Add a new point to the stroke.
        /// Also updates internal tracked values.
        ///
    {
        points.Add(newPoint);

        pointAmt++;

        if (pointAmt > 1)
        {
            float newLength = (newPoint - points[pointAmt - 2]).magnitude;
            float delta = newLength - lineMeanLength;
            lineMeanLength = delta / pointAmt;
            float delta2 = newLength - lineMeanLength;
            lineSqDistFromMeanLength += delta * delta2;
        } else
        {
            length = 0;
            lineMeanLength = 0;
            lineSqDistFromMeanLength = 0;
        }
    }

    private void initValues()
        ///
        /// Initialize internal values when initializing object.
        ///
    {
        // get point amt
        pointAmt = points.Count;

        // set other values
        if (pointAmt <= 1)
        {
            this.length = 0;
            this.lineMeanLength = 0;
            this.lineSqDistFromMeanLength = 0;
        } else
        {
            // get total length
            float length = 0;
            for (int i = 1; i < points.Count; i++)
            {
                length += Vector3.Magnitude(points[i] - points[i - 1]);
            }
            this.length = length;

            // get mean length between points
            lineMeanLength = length / this.pointAmt;

            // get sum of squared distances from mean
            for (int i = 1; i < points.Count; i++)
            {
                lineSqDistFromMeanLength += Mathf.Pow(Vector3.Magnitude(points[i] - points[i - 1]) - lineMeanLength, 2);
            }

        }
    }

    public float[] getLineStats()
    ///
    /// Return a tuple where the first value is the mean of the lengths between points making up this stroke, and the second value is the standard deviation
    /// Adapted from Welford's online algorithm at https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Online_algorithm
    ///
    {
        // get stddev of length between points
        float[] lineStats = {0, 0};
        if (pointAmt < 2) return lineStats;

        // set mean
        lineStats[0] = lineMeanLength;

        // set std dev
        lineStats[1] = Mathf.Sqrt(lineSqDistFromMeanLength / pointAmt);

        return lineStats;

    }

    public bool isClosed()
        ///
        /// Returns whether or not this stroke is a closed loop (i.e. the endpoints are sufficiently close together).
        /// TODO: Get better heuristics for how close is close enough/too close/too far.
        ///
    {
        if (pointAmt <= 2) return false;

        return (points[pointAmt - 1] - points[0]).magnitude < 0.1;
    }
}
