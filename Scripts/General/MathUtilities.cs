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
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * distance;
        pos = new Vector3(pos.x, 0f, pos.z); // zeroing y axis
        Vector3 euler = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
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
}
