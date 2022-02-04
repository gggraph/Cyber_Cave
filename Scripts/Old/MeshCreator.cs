using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCreator : MonoBehaviour
{

    public static GameObject CreateCubeOnXYLine(Vector2 v2A, Vector2 v2B, float Height, float Thickness, int radiusCorrection = 1)
    {

        Vector3 pA = new Vector3(v2A.x, 0, v2A.y); // sure 
        Vector3 pB = new Vector3(v2B.x, 0, v2B.y);
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "cubeline";

        GameObject start = new GameObject();
        start.transform.position = pA;
        GameObject end = new GameObject();
        end.transform.position = pB;

        start.transform.LookAt(pB);
        end.transform.LookAt(pA);
        float distance = Vector3.Distance(start.transform.position, end.transform.position);
        obj.transform.position = start.transform.position + distance / 2 * start.transform.forward;
        obj.transform.rotation = start.transform.rotation;
        obj.transform.eulerAngles = new Vector3(obj.transform.eulerAngles.x, MathUtilities.closestMultiple((int)obj.transform.eulerAngles.y, radiusCorrection), obj.transform.eulerAngles.z);
        obj.transform.localScale = new Vector3(Thickness, Height, distance);
        Destroy(start.gameObject);
        Destroy(end.gameObject);
       
        return obj;

    }

    // not workin ;) 
    public static void CreateQuadOnXYLine(Vector2 vStart, Vector2 vEnd, float H)
    {
        GameObject obj = new GameObject();
        obj.name = "quadline";
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(vEnd.x - vStart.x, 0, 0),
            new Vector3(0, vEnd.y - vStart.y, 0),
            new Vector3(vEnd.x - vStart.x, vEnd.y - vStart.y, 0)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        meshFilter.mesh = mesh;
    }
}
