using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paint : MonoBehaviour
{
    MeshModifier Modifier;
    bool _painting = false;
    public List<LineRenderer> Lines = new List<LineRenderer>();
    
    void Start()
    {
        Modifier = GetComponent<MeshModifier>();
    }

    public void CreateNewLine(Vector3 p)
    {
        GameObject i = new GameObject();
        i.transform.parent = this.gameObject.transform;
        Lines.Add(i.AddComponent<LineRenderer>());
        LineRenderer lnrdr = Lines[Lines.Count - 1];
        lnrdr.alignment = LineAlignment.TransformZ;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
                   new GradientColorKey[] { new GradientColorKey(PaintingTool.BrushColor, 1.0f), new GradientColorKey(PaintingTool.BrushColor, 1.0f) },
                   new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 1.0f), new GradientAlphaKey(1.0f, 1.0f) }
               );
        lnrdr.colorGradient = gradient;
        lnrdr.material = MaterialUtilities.GetMaterialFromResourcesPool(2);
        lnrdr.startWidth = PaintingTool.BrushSize;
        lnrdr.endWidth = PaintingTool.BrushSize;
        lnrdr.positionCount = 1;
        lnrdr.SetPosition(0, p);
        Debug.Log("Creating new Line at " + p);
    }
    public bool AddPointToCurrentLine(Vector3 p, float segdist = 0.005f) 
    {
        if (Lines.Count > 0)
        {
            LineRenderer lnrdr = Lines[Lines.Count - 1];
            if (Vector3.Distance(lnrdr.GetPosition(lnrdr.positionCount - 1), p) > segdist)
            {
                lnrdr.positionCount++;
                lnrdr.SetPosition(lnrdr.positionCount - 1, p);
                Debug.Log("Adding new Line at " + p);
                return true;
            }
        }
        return false;
    }

   

    bool TryPaintWithHand(GameObject Hand, float segdist = 0.005f) 
    {
        if (ToolMod.value != 2)
        {
            _painting = false;
            return false;
        }
           
        // get distance from camera player
        if (Vector3.Distance(Hand.transform.position, this.transform.position) > 0.7f)
        {
           // Debug.Log("Too much distance. ");
            _painting = false;
            return false;
        }
        RaycastHit hit = HandRecognition.IsObjectTouchedByHand(Hand, this.gameObject, 0.02f);
        if (hit.collider == null)
        {
            // Debug.Log("Not touching. ");
            _painting = false;
            return false;
        }

        if (!GetComponent<MeshCollider>())
        {
             Debug.Log("No Mesh Collider. ");
            _painting = false;
            return false;
        }
        if (!GetComponent<MeshCollider>().convex)
        {
            Debug.Log("Mesh Collider Not Convex ");
            _painting = false;
            return false;
        }

        Vector3 fendPos = hit.collider.ClosestPoint(Hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position);

        if (Vector3.Distance(fendPos, Hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position) > 0.08f)
        {
            _painting = false;
            Debug.Log("EndPos is too distant");
            return false; ; // add some distance penality to avoid painting if not closed to mesh triangles
        }
        if ( !_painting) 
        {
            _painting = true;
            CreateNewLine(fendPos);
            PaintingTool.SyncNewLineOnObject(this.gameObject, fendPos);
        }
        else 
        {
            if (AddPointToCurrentLine(fendPos))
            {
                PaintingTool.SyncAddLineOnObject(this.gameObject, fendPos);
            }
        }
        return false;

    }

    bool TryPainDebug(float segdist = 0.005f) 
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
            return false ;
        }
        if ( hit.collider.gameObject != this.gameObject) 
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
            CreateNewLine(fendPos);
            PaintingTool.SyncNewLineOnObject(this.gameObject, fendPos);
            return true;
        }
        else
        {

            if (AddPointToCurrentLine(fendPos))
            {
                PaintingTool.SyncAddLineOnObject(this.gameObject, fendPos);
            }
        }
        return false;
    }
    void Update()
    {
        if (GameStatus._selectorIsOpen)
            return;
       // TryPainDebug();
        TryPaintWithHand(Modifier.RightHand);
    }
}
