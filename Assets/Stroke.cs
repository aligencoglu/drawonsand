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
        /// Creates an empty stroke
    {
        this.points = new List<Vector3>();
        initValues();
    }

    public Stroke(List<Vector3> points)
        /// Creates a stroke from a list of points
    {
        this.points = points;
        initValues();
    }

    public Stroke(Vector3[] points)
        /// Creates a stroke from an array of points
    {
        this.points = new List<Vector3>(points);
        initValues();
    }

    public Stroke(Vector3 startPoint)
        /// Creates a stroke with a single point
    {
        points = new List<Vector3>();
        points.Add(startPoint);
        initValues();
    }
    
    public List<Vector3> points { get; private set; } // points making up this stroke
    private int pointAmt; // amount of points

    // length info
    public float length { get; private set; } // total length of the stroke
    private float lineMeanLength; // mean length between individual points
    private float lineSqDistFromMeanLength; // sum of the squared distances of the lengths between individual points from the mean length between individual points

    // angle info
    public float internalAngleSum { get; private set; } // sum of internal angles at points
    private float angleMean; // mean of the internal angles at points
    private float angleSqDistFromAngleMean; // sum of the squared distances of the internal angles from the mean internal angle


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
            // update length info
            int lineAmt = pointAmt - 1;
            float newLength = (newPoint - points[pointAmt - 2]).magnitude;
            length += newLength;
            float delta = newLength - lineMeanLength;
            lineMeanLength += delta / lineAmt;
            float delta2 = newLength - lineMeanLength;
            lineSqDistFromMeanLength += delta * delta2;
        } else
        {
            length = 0;
            lineMeanLength = 0;
            lineSqDistFromMeanLength = 0;
        }
        if (pointAmt > 2)
        {
            // update angle info
            int angleAmt = pointAmt - 2;
            Vector3 lastLine = points[pointAmt - 1] - points[pointAmt - 2];
            Vector3 secondLastLine = points[pointAmt - 2] - points[pointAmt - 3];
            float newAngle = Mathf.Abs(Vector3.Angle(secondLastLine, lastLine));
            if (newAngle > 180)
                newAngle = 360 - newAngle;
            internalAngleSum += newAngle;

            float delta = newAngle - angleMean;
            angleMean += delta / angleAmt;
            float delta2 = newAngle - angleMean;
            angleSqDistFromAngleMean += delta * delta2;
        } else
        {
            internalAngleSum = 0;
            angleMean = 0;
            angleSqDistFromAngleMean = 0;
        }
    }

    private void initValues()
        ///
        /// Initialize internal values when initializing object.
        ///
    {
        // get point amt
        pointAmt = points.Count;

        // set length values
        if (pointAmt <= 1)
        {
            this.length = 0;
            this.lineMeanLength = 0;
            this.lineSqDistFromMeanLength = 0;
        } else
        {
            // get line amt for calculations
            int lineAmt = pointAmt - 1;

            // get total length
            float length = 0;
            for (int i = 1; i < points.Count; i++)
            {
                length += Vector3.Magnitude(points[i] - points[i - 1]);
            }
            this.length = length;

            // get mean length between points
            lineMeanLength = length / lineAmt;

            // get sum of squared distances from mean
            for (int i = 1; i < points.Count; i++)
            {
                lineSqDistFromMeanLength += Mathf.Pow(Vector3.Magnitude(points[i] - points[i - 1]) - lineMeanLength, 2);
            }

        }
        
        // set angle values
        if (pointAmt <= 2)
        {
            this.internalAngleSum = 0;
            this.angleMean = 0;
            this.angleSqDistFromAngleMean = 0;
        } else
        {
            // get angle amt for calculations
            int angleAmt = pointAmt - 2;

            // get total angle sum
            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector3 lastLine = points[i+1] - points[i];
                Vector3 secondLastLine = points[i] - points[i-1];
                float newAngle = Mathf.Abs(Vector3.Angle(secondLastLine, lastLine));
                if (newAngle > 180)
                    newAngle = 360 - newAngle;
                internalAngleSum += newAngle;

                angleMean += newAngle;
            }

            // get mean angle at points
            angleMean /= angleAmt;

            // get sum of squared distances of angles from mean angle
            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector3 lastLine = points[i + 1] - points[i];
                Vector3 secondLastLine = points[i] - points[i - 1];
                float newAngle = Mathf.Abs(Vector3.Angle(secondLastLine, lastLine));
                if (newAngle > 180)
                    newAngle = 360 - newAngle;

                angleSqDistFromAngleMean += Mathf.Pow(newAngle - angleMean, 2);
            }


        }
    }

    public float[] getLengthStats()
        ///
        /// Return a tuple where the first value is the mean of the lengths between points making up this stroke, and the second value is the standard deviation
        /// Adapted from Welford's online algorithm at https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Online_algorithm
        ///
    {
        float[] lengthStats = {0, 0};
        if (pointAmt < 2) return lengthStats;

        // set mean
        lengthStats[0] = lineMeanLength;

        // set std dev
        lengthStats[1] = Mathf.Sqrt(lineSqDistFromMeanLength / pointAmt);

        return lengthStats;

    }

    public float[] getAngleStats()
        ///
        /// Return a tuple where the first value is the mean of the lengths between points making up this stroke, and the second value is the standard deviation
        /// Adapted from Welford's online algorithm at https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Online_algorithm
        ///
    {
        float[] angleStats = { 0, 0 };
        if (pointAmt < 3) return angleStats;

        // set mean
        angleStats[0] = angleMean;

        // set std dev
        angleStats[1] = Mathf.Sqrt(angleSqDistFromAngleMean / (pointAmt - 2));

        return angleStats;
    }

    public bool isClosed()
        ///
        /// Return whether or not this stroke is a closed loop (i.e. the endpoints are sufficiently close together).
        /// TODO: Get better heuristics for how close is close enough/too close/too far.
        ///
    {
        if (pointAmt <= 2) return false;

        return (points[pointAmt - 1] - points[0]).magnitude < 0.1;
    }
}
