using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtilities : MonoBehaviour
{
    public static Vector3 GetCenterOfObjects(GameObject[] gos)
    {
        float totalX = 0f;
        float totalY = 0f;
        float totalZ = 0f;
        foreach (GameObject go in gos)
        {
            totalX += go.transform.position.x;
            totalY += go.transform.position.y;
            totalZ += go.transform.position.z;
        }
        float centerX = totalX / gos.Length;
        float centerY = totalY / gos.Length;
        float centerZ = totalZ / gos.Length;

        return new Vector3(centerX, centerY, centerZ);
    }

    public static Vector3[] GetCirclePoints(int points, float radius, Vector3 center)
    {
        Vector3[] v = new Vector3[points];
        float slice = 2 * Mathf.PI / points;
        for (int i = 0; i < points; i++)
        {

            float angle = slice * i;
            float newX = (center.x + radius * Mathf.Cos(angle));
            float newZ = (center.z + radius * Mathf.Sin(angle));
            v[i] = new Vector3(newX, center.y, newZ);
        }
        return v;
    }
    public static int closestMultiple(int number, int multiple)
    {
        if (multiple > number)
            return multiple;

        number = number + multiple / 2;
        number = number - (number % multiple);
        return number;
    }
    public static int nearestmultiple(int numToRound, int multiple, bool flr)
    {
        if (multiple == 0)
            return numToRound;

        int remainder = numToRound % multiple;
        if (remainder == 0)
            return numToRound;

        if (!flr)
            return numToRound + multiple - remainder;
        else
            return numToRound - remainder;
    }
    public static Vector3 RoundEulerAngle(Vector3 euler)
    {
        int nx = MathUtilities.nearestmultiple((int)euler.x, 360, true);
        int ny = MathUtilities.nearestmultiple((int)euler.y, 360, true);
        int nz = MathUtilities.nearestmultiple((int)euler.z, 360, true);

        return new Vector3(euler.x - nx, euler.y - ny, euler.z - nz);

    }
    public static float GetSumOfVectorsDistance(Vector3[] A, Vector3[] B)
    {
        float r = 0f;
        for (int i = 0; i < 24; i++)
        {
            r += Mathf.Abs(Vector3.Distance(A[i], B[i]));
        }
        return r;
    }
    public static Vector3 GetOffsetFromVectors(Vector3 a, Vector3 b)
    {
        return a - b;
    }
    public static Vector3 GetOffsetFromObject(GameObject a, GameObject b)
    {
        return a.transform.position - b.transform.position;
    }
    public static Vector3[] GetPositionAndEulerInFrontOfPlayer(float distance) 
    {
        // Here we change some stuff
        Vector3 pos = Camera.main.transform.root.transform.position + Camera.main.transform.root.transform.forward * distance;
        pos = new Vector3(pos.x, pos.y, pos.z); // zeroing y axis
        Vector3 euler = new Vector3(0, Camera.main.transform.root.eulerAngles.y, 0);
        return new Vector3[2] { pos, euler };
    }
    public static float GetMaxSpaceTakenByBounds(Bounds b)
    {
        float max = float.MinValue;
        if (b.size.x > max)
        {
            max = b.size.x;
        }
        if (b.size.y > max)
        {
            max = b.size.y;
        }
        if (b.size.z > max)
        {
            max = b.size.z;
        }
        return max;
    }

    public static bool BoxIntersectsBox(Vector3 boxCentreA, Vector3 boxSizeA, Vector3 boxCentreB, Vector3 boxSizeB, Quaternion AINV)
    {
        // check if any points are inside first Box
        Vector3[] pts = new Vector3[8];
        
        // Get edges
        pts[0] = boxCentreB + new Vector3(-boxSizeB.x / 2, -boxSizeB.y / 2, -boxSizeB.z / 2);
        pts[1] = boxCentreB + new Vector3(boxSizeB.x / 2, -boxSizeB.y / 2, -boxSizeB.z / 2);
        pts[2] = boxCentreB + new Vector3(boxSizeB.x / 2, -boxSizeB.y / 2, boxSizeB.z / 2);
        pts[3] = boxCentreB + new Vector3(boxSizeB.x / 2, boxSizeB.y / 2, boxSizeB.z / 2);
        pts[4] = boxCentreB + new Vector3(-boxSizeB.x / 2, -boxSizeB.y / 2, boxSizeB.z / 2);
        pts[5] = boxCentreB + new Vector3(boxSizeB.x / 2, boxSizeB.y / 2, -boxSizeB.z / 2);
        pts[6] = boxCentreB + new Vector3(-boxSizeB.x / 2, boxSizeB.y / 2, boxSizeB.z / 2);
        pts[7] = boxCentreB + new Vector3(-boxSizeB.x / 2, boxSizeB.y / 2, -boxSizeB.z / 2);

        // Transform matrix point 
        for (int i = 0; i < 8; i++)
        {
            // Should I inv ?
            Vector3 p = AINV * pts[i];
        }

        // Check if each pts are inside 

        return false;
    }
    public static bool EllipsIntersectsBox(Vector3 boxCentreA, Vector3 boxSizeA, Vector3 boxCentreB, Vector3 boxSizeB)
    {
        return false;
    }
    public static bool CylinderIntersectsBox(Vector3 boxCentreA, Vector3 boxSizeA, Vector3 boxCentreB, Vector3 boxSizeB)
    {
        // Cehck old C#
        return false;
    }
    public static bool SphereIntersectsBox(Vector3 sphereCentre, float sphereRadius, Vector3 boxCentre, Vector3 boxSize)
    {
        float closestX = Mathf.Clamp(sphereCentre.x, boxCentre.x - boxSize.x / 2, boxCentre.x + boxSize.x / 2);
        float closestY = Mathf.Clamp(sphereCentre.y, boxCentre.y - boxSize.y / 2, boxCentre.y + boxSize.y / 2);
        float closestZ = Mathf.Clamp(sphereCentre.z, boxCentre.z - boxSize.z / 2, boxCentre.z + boxSize.z / 2);

        float dx = closestX - sphereCentre.x;
        float dy = closestY - sphereCentre.y;
        float dz = closestZ - sphereCentre.z;

        float sqrDstToBox = dx * dx + dy * dy + dz * dz;
        return sqrDstToBox < sphereRadius * sphereRadius;
    }
    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        //Get a direction from the pivot to the point
        Vector3 dir = point - pivot;
        //Rotate vector around pivot
        dir = rotation * dir;
        //Calc the rotated vector
        point = dir + pivot;
        //Return calculated vector
        return point;
    }
}
