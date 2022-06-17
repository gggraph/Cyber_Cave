using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Paintable_OLD : MonoBehaviour
{
    // @ Add this script to any gameObject that you want to be paintable... 
    // 
    public List<TubeRenderer> Lines = new List<TubeRenderer>();
    Material m;
    public Bounds bounds;
    public void Start()
    {
        m = MaterialUtilities.GetMaterialFromResourcesPool(1);
    }
    public void CreateNewLine(Vector3 p, Material m, Color c, bool _useColor = false, float radius = 0.015f, int sides = 36)
    {
        GameObject i = new GameObject();
        i.transform.parent = this.gameObject.transform;
        TubeRenderer tbrdr = i.AddComponent<TubeRenderer>();
        tbrdr.SetSidesNumber(sides);
        tbrdr.SetRadius1(radius);
        if (_useColor)
            m.color = c;
        tbrdr.SetMaterial(m);
        Lines.Add(tbrdr);
        //tbrdr.SetPositions(new Vector3[1] { p });
        tbrdr.AddNewPositionFromWoorldCoordinate(p);

    }

    public bool AddPointToCurrentLine(Vector3 p, float segdist = 0.005f)
    {
        if (Lines.Count > 0)
        {
            TubeRenderer tbrdr = Lines[Lines.Count - 1];
            if (Vector3.Distance(tbrdr.GetPositionAsWorldCoordinate(tbrdr.GetPositionsCount() - 1), p) > segdist)
            {
                tbrdr.AddNewPositionFromWoorldCoordinate(p);
                Debug.Log("Adding new Line at " + p);
                return true;
            }
        }
        return false;
    }
    bool _painting = false;
    public bool DebugPaint()
    {
        if (!Input.GetMouseButton(0))
        {
            _painting = false;
            return false;
        }

        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            _painting = false;
            return false;
        }
        if (hit.collider.gameObject != this.gameObject)
        {
            _painting = false;
            return false;
        }
        Vector3 fendPos = hit.collider.ClosestPoint(hit.point);
        if (Vector3.Distance(fendPos, hit.point) > 0.08f)
        {
            _painting = false;
            return false; ; // add some distance penality to avoid painting if not closed to mesh triangles
        }
        if (!_painting)
        {
            _painting = true;
            CreateNewLine(fendPos, m, Color.black, true, 0.015f);
            return true;
        }
        else
        {

            if (AddPointToCurrentLine(fendPos))
            {
            }
        }
        return false;
    }
    public void CalculateBoundaries ()
    {
        bounds = ObjectUtilities.GetBoundsOfGroupOfMesh(this.gameObject);
    }
    private void Update()
    {
        DebugPaint();
        // some debug to test stuff
        if (Input.GetKeyDown(KeyCode.P) && Lines.Count > 0 )
        {
           // XYPlotting.TrySendAllLinesFromPaintableObject(this);
        }
    }

    // @ Static Net methods 
    public static void SyncNewLineOnObject(GameObject paintableObject, GameObject painterObject, Vector3 pos)
    {
        List<byte> data = new List<byte>();
        data.Add(21);
        data.Add(0); // Create New Line
        data.Add((byte)paintableObject.name.Length);
        foreach (char c in paintableObject.name.ToCharArray())
            data.Add((byte)c);
        data.Add((byte)painterObject.name.Length);
        foreach (char c in painterObject.name.ToCharArray())
            data.Add((byte)c);

        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(pos));
        NetUtilities.SendDataToAll(data.ToArray());
    }
    public static void SyncAddLineOnObject(GameObject paintableObject, Vector3 pos)
    {
        List<byte> data = new List<byte>();
        data.Add(21);
        data.Add(1); // Add new line
        data.Add((byte)paintableObject.name.Length);
        foreach (char c in paintableObject.name.ToCharArray())
            data.Add((byte)c);
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(pos));
        NetUtilities.SendDataToAll(data.ToArray());
    }

    // @ static Net application
    public static void ProccessLine(byte[] msg)
    {
        byte Mode = msg[1];
        byte paintablenamesize = msg[2];
        char[] paintableName = new char[paintablenamesize];
        for (int i = 3; i < 3 + paintablenamesize; i++)
        {
            paintableName[i - 3] = (char)msg[i];
        }
        GameObject paintableObject = GameObject.Find(new string(paintableName));
        if (paintableObject == null) { Debug.Log("Object was Null!"); return; }
        Paintable_OLD paintable = paintableObject.GetComponent<Paintable_OLD>();
        if (paintable == null) { Debug.Log("Paintable Scritp was Null!"); return; }
        
        if (Mode == 0)
        {
            //Vector3 pos = BinaryUtilities.BytesToVector3(ref msg, 3 + paintablenamesize);
            byte painternamesize = msg[3 + paintablenamesize];
            char[] paintername = new char[painternamesize];
            for (int i = 4 + paintablenamesize; i < 4 + paintablenamesize + painternamesize; i++)
            {
                paintername[i - (4 + paintablenamesize)] = (char)msg[i];
            }
            GameObject painterObject = GameObject.Find(new string(paintername));
            if (painterObject == null) { Debug.Log("Object was Null!"); return; }
            Painter_OLD painter = painterObject.GetComponent<Painter_OLD>();
            if (painter == null) { Debug.Log("Painter Scritp was Null!"); return; }
            Vector3 pos = BinaryUtilities.BytesToVector3(ref msg, 4 + paintablenamesize+painternamesize);
            paintable.CreateNewLine(pos, painter.BrushMaterial, painter.color, painter._useColor, painter.radius);
        }
        else if (Mode == 1)
        {
            Vector3 pos = BinaryUtilities.BytesToVector3(ref msg, 3 + paintablenamesize);
            paintable.AddPointToCurrentLine(pos);
        }

    }
}
