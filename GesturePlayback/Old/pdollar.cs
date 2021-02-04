using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class pdollar : MonoBehaviour
{

    public class dPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int ID { get; set; }

        public dPoint(float x, float y, int Id)
        {
            this.X = x;
            this.Y = y;
            this.ID = Id;
        }
    }
    public class Result // constructor
    {
        public string Name { get; set; }
        public float Score { get; set; }
        public Result(string name, float score)
        {
            this.Name = name;
            this.Score = score;

        }

    }

    public class Geometry
    {
        /// <summary>
        /// Computes the Squared Euclidean Distance between two dPoints in 2D
        /// </summary>
        public static float SqrEuclideanDistance(dPoint a, dPoint b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
        }

        /// <summary>
        /// Computes the Euclidean Distance between two dPoints in 2D
        /// </summary>
        public static float EuclideanDistance(dPoint a, dPoint b)
        {
            return (float)Math.Sqrt(SqrEuclideanDistance(a, b));

        }
    }

    public class PointCloud
    {
        public dPoint Origin = new dPoint(0, 0, 0); // do we really need
        public List<dPoint> Points = null;            // gesture dPoints (normalized)
        public string Name = "";                 // gesture class
        private const int SAMPLING_RESOLUTION = 32;

        public PointCloud(List<dPoint> dPoints, string gestureName = "")
        {
            this.Name = gestureName;
            this.Points = Resample(dPoints, SAMPLING_RESOLUTION); // this is originally 32
            this.Points = Scale(this.Points);
            this.Points = TranslateTo(this.Points, Origin);

        }
        public List<dPoint> Resample(List<dPoint> points, int n)
        {
            float I = PathLength(points) / (n - 1); // interval length
            float D = 0.0f;
            List<dPoint> newpoints = new List<dPoint> { points[0] }; //< will initialize list with size 1 ... points[0]
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].ID == points[i - 1].ID)
                {
                    float d = Geometry.EuclideanDistance(points[i - 1], points[i]);
                    if ((D + d) >= I)
                    {
                        var qx = points[i - 1].X + ((I - D) / d) * (points[i].X - points[i - 1].X);
                        var qy = points[i - 1].Y + ((I - D) / d) * (points[i].Y - points[i - 1].Y);
                        var q = new dPoint(qx, qy, points[i].ID);
                        newpoints.Add(q); // append new point 'q'
                        points.Insert(i, q);
                        //points.splice(i, 0, q); // insert 'q' at position i in points s.t. 'q' will be the next i
                        D = 0.0f;
                    }
                    else D += d;
                }
            }
            if (newpoints.Count == n - 1) // sometimes we fall a rounding-error short of adding the last point, so add it if so
                newpoints.Add(new dPoint(points[points.Count - 1].X, points[points.Count - 1].Y, points[points.Count - 1].ID));
            return newpoints;
        }


        /*
        public List<dPoint> Resample(List<dPoint> dPoints, int n)
        {
            List<dPoint> newdPoints = new dPoint[n];
            newdPoints[0] = new dPoint(dPoints[0].X, dPoints[0].Y, dPoints[0].StrokeID);
            int numdPoints = 1;

            float I = PathLength(dPoints) / (n - 1); // computes interval length
            float D = 0;
            for (int i = 1; i < dPoints.Length; i++)
            {
                if (dPoints[i].StrokeID == dPoints[i - 1].StrokeID)
                {
                    float d = Geometry.EuclideanDistance(dPoints[i - 1], dPoints[i]);
                    if (D + d >= I)
                    {
                        dPoint firstdPoint = dPoints[i - 1];
                        while (D + d >= I)
                        {
                            // add interpolated dPoint
                            float t = Math.Min(Math.Max((I - D) / d, 0.0f), 1.0f);
                            if (float.IsNaN(t)) t = 0.5f;
                            newdPoints[numdPoints++] = new dPoint(
                                (1.0f - t) * firstdPoint.X + t * dPoints[i].X,
                                (1.0f - t) * firstdPoint.Y + t * dPoints[i].Y,
                                dPoints[i].StrokeID
                            );

                            // update partial length
                            d = D + d - I;
                            D = 0;
                            firstdPoint = newdPoints[numdPoints - 1];
                        }
                        D = d;
                    }
                    else D += d;
                }
            }

            if (numdPoints == n - 1) // sometimes we fall a rounding-error short of adding the last dPoint, so add it if so
                newdPoints[numdPoints++] = new dPoint(dPoints[dPoints.Length - 1].X, dPoints[dPoints.Length - 1].Y, dPoints[dPoints.Length - 1].StrokeID);
            return newdPoints;
        }
        */


        private float PathLength(List<dPoint> points) // length traversed by a point path
        {
            float d = 0.0f;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].ID == points[i - 1].ID)
                    d += Geometry.EuclideanDistance(points[i - 1], points[i]);
            }
            return d;
        }
        private List<dPoint> Scale(List<dPoint> points)
        {
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            for (var i = 0; i < points.Count; i++)
            {
                minX = Math.Min(minX, points[i].X);
                minY = Math.Min(minY, points[i].Y);
                maxX = Math.Max(maxX, points[i].X);
                maxY = Math.Max(maxY, points[i].Y);
            }
            float size = Math.Max(maxX - minX, maxY - minY);
            List<dPoint> newpoints = new List<dPoint>();//new dPoint[points.Count];
            for (var i = 0; i < points.Count; i++)
            {
                var qx = (points[i].X - minX) / size;
                var qy = (points[i].Y - minY) / size;
                newpoints.Add(new dPoint(qx, qy, points[i].ID));  // this is a push !!! 
            }
            return newpoints;
        }

        private List<dPoint> TranslateTo(List<dPoint> points, dPoint pt) // translates points' centroid to pt
        {
            var c = Centroid(points);
            var newpoints = new List<dPoint>();//new dPoint[points.Count];
            for (var i = 0; i < points.Count; i++)
            {
                var qx = points[i].X + pt.X - c.X;
                var qy = points[i].Y + pt.Y - c.Y;
                newpoints.Add(new dPoint(qx, qy, points[i].ID));
            }
            return newpoints;
        }
        private dPoint Centroid(List<dPoint> points)
        {
            float x = 0.0f, y = 0.0f;
            for (var i = 0; i < points.Count; i++)
            {
                x += points[i].X;
                y += points[i].Y;
            }
            x /= points.Count;
            y /= points.Count;
            return new dPoint(x, y, 0);
        }

    }
    public class PDollarRecognizer
    {
        public List<PointCloud> PointClouds = new List<PointCloud>();
        public void Init()
        {
            PointClouds = new List<PointCloud>();
        }
        public Result Recognize(List<dPoint> points)
        {
            var candidate = new PointCloud(points, "");

            var u = -1;
            var b = float.MaxValue;
            for (var i = 0; i < this.PointClouds.Count; i++) // for each point-cloud template
            {
                var d = GreedyCloudMatch(candidate.Points, this.PointClouds[i]);
                if (d < b)
                {
                    b = d; // best (least) distance
                    u = i; // point-cloud index
                }
            }

            return (u == -1) ? new Result("No match.", 0.0f) : new Result(this.PointClouds[u].Name, (float)(b > 1.0 ? 1.0 / b : 1.0));
        }
        public Result Compare(List<dPoint> points, int PointCloudIndex)
        {
           
            var candidate = new PointCloud(points, "");
            var u = -1;
            var b = float.MaxValue;

            var d = GreedyCloudMatch(candidate.Points, this.PointClouds[PointCloudIndex]);
            if (d < b)
            {
                b = d; // best (least) distance
                u = PointCloudIndex; // point-cloud index
            }

            return (u == -1) ? new Result("No match.", 0.0f) : new Result(this.PointClouds[u].Name, (float)(b > 1.0 ? 1.0 / b : 1.0));
        }
        

        private static float GreedyCloudMatch(List<dPoint> points, PointCloud P)
        {
            var e = 0.50;
            var mff = Math.Floor(Math.Pow(points.Count, 1.0 - e));
            int step = (int)mff;
            var min = float.MaxValue;
            for (int i = 0; i < points.Count; i += step)
            {
                var d1 = CloudDistance(points, P.Points, i);
                var d2 = CloudDistance(P.Points, points, i);
                min = Math.Min(min, Math.Min(d1, d2)); // min3
            }
            return min;
        }

        private static float CloudDistance(List<dPoint> pts1, List<dPoint> pts2, int start)
        {
            bool[] matched = new bool[pts1.Count]; // pts1.length == pts2.length
            for (var k = 0; k < pts1.Count; k++)
            {
                matched[k] = false;

            }

            float sum = 0f;
            var i = start;
            do
            {
                var index = -1;
                var min = float.MaxValue;
                for (var j = 0; j < matched.Length; j++)
                {
                    if (!matched[j])
                    {
                        var d = Geometry.EuclideanDistance(pts1[i], pts2[j]);
                        if (d < min)
                        {
                            min = d;
                            index = j;
                        }
                    }
                }
                if (index == -1) { return sum; }// this is a secure...
                matched[index] = true; // this create issue....
                var weight = 1 - ((i - start + pts1.Count) % pts1.Count) / pts1.Count;
                sum += weight * min;
                i = (i + 1) % pts1.Count;
            } while (i != start);
            return sum;
        }


    }
    private double round(float n, double d) // round 'n' to 'd' decimals
    {
        d = Math.Pow(10, d);
        return Math.Round(n * d) / d;
    }

    public PDollarRecognizer pdoll;

    public object[] RecognizeGesture(List<Point> Points)
    {
        List<dPoint> dPoints = new List<dPoint>();
        for (int i = 0; i < Points.Count; i++)
        {
            dPoints.Add(new dPoint(Points[i].X, Points[i].Y, 0));
        }
        Result r = pdoll.Recognize(dPoints); // seems really more related...
        double fc = round(r.Score, 2);
        object[] result = new object[3];
        result[0] = r.Name;
        result[1] = fc;
        result[2] = false;
        if (fc > 0.24f) { result[2] = true; }
        return result;

    }
    public double[] GetScoreMatchingFromPointSeries(List<List<Point>> PointsSeries)
    {
        // Points Series should be the size of Training Set in Order 
        List<List<dPoint>> dPoints = new List<List<dPoint>>();
        foreach ( List<Point> lp in PointsSeries) 
        {
            List<dPoint> ldp = new List<dPoint>(); 
            foreach(Point p in lp) 
            {
                ldp.Add(new dPoint(p.X, p.Y, 0));
            
            }
            dPoints.Add(ldp); 
        }

        double[] Scores = new double[dPoints.Count]; 
        for (int i = 0; i < dPoints.Count; i++) 
        {
            Result r = pdoll.Compare(dPoints[i], i);
            double fc = round(r.Score, 2);
            Scores[i] = fc; 
        }
        return Scores; 

    }

    public void AddGesture(List<float> PXS, List<float> PYS, string name) // add gesture to the training set + his name...
    {
        List<dPoint> dPoints = new List<dPoint>();
        for (int i = 0; i < PXS.Count; i++)
        {
            dPoints.Add(new dPoint(PXS[i], PYS[i], 0));
        }
        pdoll.PointClouds.Add(new PointCloud(dPoints, name));
    }

    public void InitGestures()
    {
        pdoll = new PDollarRecognizer();
    }



}
