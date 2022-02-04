using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geometra : MonoBehaviour
{
    public List<Vector2> HitPoints = new List<Vector2>();
    public List<Vector2> HitOffsets = new List<Vector2>();
    public List<Vector2[]> VectorizedPoint = new List<Vector2[]>();

    public Vector2 direction;
    public bool _onDraw = false;
    public float AngleTolerance;

    void Start()
    {
        GlobalMenu.OpenGlobalMenu();
        AngleTolerance = 0.6f;
    }

    // In Boot.cs ... 
    
    void DrawVectors()
    {
       // if (ToolMod.value != 3)
        //    return;
        if (!Input.GetMouseButton(0) && HandRecognition.DoesRightFingerTouchingObject_SphereCast(this.gameObject).collider == null )
        {
            if (_onDraw)
            {
                _onDraw = false;
                // add latest hitpoint
                if (VectorizedPoint[VectorizedPoint.Count - 1].Length != 0)
                {
                    VectorizedPoint[VectorizedPoint.Count - 1][1] = HitPoints[HitPoints.Count - 1];
                }

                HitPoints = new List<Vector2>();
                HitOffsets = new List<Vector2>();
            }
            return;
        }

        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            return;
        }
        Renderer rend = hit.transform.GetComponent<Renderer>();
        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null)
        {
            return;
        }
        Texture2D tex = rend.material.mainTexture as Texture2D;
        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= tex.width;
        pixelUV.y *= tex.height;
        HitPoints.Add(new Vector2(pixelUV.x, pixelUV.y));
       // tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.black);
        if ( HitPoints.Count > 1)
        {
            HitOffsets.Add(HitPoints[HitPoints.Count - 1] - HitPoints[HitPoints.Count - 2]);
        }
        if (!_onDraw)
        {
            _onDraw = true;
            direction = new Vector2(0, 0);
            // start new vectorized point
            VectorizedPoint.Add(new Vector2[2]);
            VectorizedPoint[VectorizedPoint.Count - 1][0] = new Vector2(pixelUV.x, pixelUV.y);
           
        }
        else
        {
            // set vector 1 
            VectorizedPoint[VectorizedPoint.Count - 1][1] = new Vector2(pixelUV.x, pixelUV.y);

            // get the last direction 
            Vector2 currentdirection = GetAverageOffsets(10);
            currentdirection = RoundVectorOffset(currentdirection);
            if ( currentdirection != new Vector2(0, 0))
            {
                // compare current direction with direction
                float dist = Vector2.Distance(direction, currentdirection);
                //Debug.Log(dist);
                if (dist > AngleTolerance)
                {
                    direction = currentdirection;
                    DrawBrezenhamLine(ref tex, Color.blue,
                        (int)VectorizedPoint[VectorizedPoint.Count - 1][0].x, (int)VectorizedPoint[VectorizedPoint.Count - 1][0].y,
                        (int)VectorizedPoint[VectorizedPoint.Count - 1][1].x, (int)VectorizedPoint[VectorizedPoint.Count - 1][1].y
                        );
                    // and set red pixel at start and end
                    tex.SetPixel((int)VectorizedPoint[VectorizedPoint.Count - 1][0].x, (int)VectorizedPoint[VectorizedPoint.Count - 1][0].y, Color.red);
                    tex.SetPixel((int)VectorizedPoint[VectorizedPoint.Count - 1][1].x, (int)VectorizedPoint[VectorizedPoint.Count - 1][1].y, Color.red);

                    // Add new vector point 
                    VectorizedPoint.Add(new Vector2[2]);
                    VectorizedPoint[VectorizedPoint.Count - 1][0] = new Vector2(pixelUV.x, pixelUV.y);
                }
                    
            }
        }
        tex.Apply();
    }
    void DrawBrezenhamLine(ref Texture2D tex, Color c, int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = (dx > dy ? dx : -dy) / 2, e2;
        for (; ; )
        {
            tex.SetPixel(x0, y0, c);
            if (x0 == x1 && y0 == y1) break;
            e2 = err;
            if (e2 > -dx) { err -= dy; x0 += sx; }
            if (e2 < dy) { err += dx; y0 += sy; }
        }
    }
    Vector2 RoundVectorOffset(Vector2 offset)
    {
        float reference = 0;
        if (Mathf.Abs(offset.x) > reference)
            reference = Mathf.Abs(offset.x);
        if (Mathf.Abs(offset.y) > reference)
            reference = Mathf.Abs(offset.y);
        if (reference == 0)
            return offset;
        return new Vector2(offset.x / reference, offset.y / reference);
    }
    Vector2 GetAverageOffsets(int frames)
    {
        if (HitOffsets.Count-1 - frames < 0)
            return new Vector2(0, 0);

        Vector2 sum = new Vector2(0, 0);
        for (int i = HitOffsets.Count-1; i>= HitOffsets.Count-1-frames; i--)
        {
            sum += HitOffsets[i];
        }
        sum /= frames;
        return sum;
    }

    void BuildGeometraSpace()
    {

        List<GameObject> space = new List<GameObject>();
        foreach ( Vector2[] v in VectorizedPoint)
        {
            GameObject g = MeshCreator.CreateCubeOnXYLine(v[0], v[1], 3f, 1f, 1);
            g.GetComponent<Renderer>().material.shader = Shader.Find("Custom/ColoredGlass");
            g.GetComponent<Renderer>().material.color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            space.Add(g);
           // g.AddComponent<FadeIn>().Init(3f);
        }
        // get center of all those objects 
        Vector3 center = MathUtilities.GetCenterOfObjects(space.ToArray());
        GameObject p = new GameObject();
        p.transform.position = center;
        foreach ( GameObject go in space)
        {
            go.transform.parent = p.transform;
        }
        p.transform.position = new Vector3(0, 0, 0);
        

    }
    void Update()
    {
        DrawVectors();
        if (Input.GetKeyDown(KeyCode.R) || HandRecognition.IsPose_OKSign())
            BuildGeometraSpace();
    }
    
}
