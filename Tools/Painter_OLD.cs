using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Painter_OLD : MonoBehaviour
{
    //// @ Add this script to any gameObject with collider to paint on Paintable_OLDobject.
    /// When collider touches objects, it adds a new line if necessary 
    /// Size of object collider will determine size of brush. Also you can add a specific texture or a unit color into painter. 

    Collider col;
    List<Paintable_OLD> LastPaintables = new List<Paintable_OLD>();
    // set matrial. default is blank material
    public Material BrushMaterial;
    // use color & apply if needed
    public Color color;
    public bool _useColor = false;
    // Change radius of tubular draw
    public float radius = 0.03f;
  

    private void Start()
    {
        col = GetComponent<Collider>();
        if (BrushMaterial == null)
            BrushMaterial = MaterialUtilities.GetMaterialFromResourcesPool(1);
        color = Color.black;
    }
    bool TryPaintOnObject(Paintable_OLD p)
    {
  

        if (Vector3.Distance(this.transform.position, p.gameObject.transform.position) > 0.7f)
        {
           // return false;
        }
        p.CalculateBoundaries();
        // check if collider is inside ... 
        if (!col.bounds.Intersects(p.bounds))
        {
            Debug.Log("Not intersecting...");
            return false;
        }
            

        Collider[] cols = ObjectUtilities.GetCollidersOfMesh(p.gameObject); 

        foreach ( Collider c in cols)
        {
            if (col.bounds.Intersects(c.bounds))
            {
                Debug.Log("intersecting " + c.gameObject.name);
                // get closest point on mesh 
                Vector3 hitpos = c.ClosestPoint(col.transform.position);
                if (Vector3.Distance(hitpos, col.transform.position) > 0.08f)
                {
                    return false; ; // add some distance penality to avoid painting if not closed to mesh triangles
                }
                if (!DoesLastPaintablesContainsPaintable(p))
                {
                    p.CreateNewLine(hitpos, BrushMaterial, color, _useColor, radius, 36 );
                    Paintable_OLD.SyncNewLineOnObject(p.gameObject, this.gameObject, hitpos);
                    return true;
                }
                else
                {
                    if (p.AddPointToCurrentLine(hitpos))
                    {
                        Paintable_OLD.SyncAddLineOnObject(p.gameObject, hitpos); // here something need to be reworked ...
                    }
                    return true;
                }
            }
        }

        return false;
    }

    private bool DoesLastPaintablesContainsPaintable(Paintable_OLD pt)
    {
        foreach ( Paintable_OLD p in LastPaintables)
        {
            if (p.gameObject == pt.gameObject)
                return true;
        }
        return false;
    }

    private void Update()
    {
        // @ don't do anything if this has no grabbable and is not grabbed by yourself... Set parent and not root object because root change when grabbing. 
        Grabbable g = gameObject.transform.parent.gameObject.GetComponent<Grabbable>();
        if (g == null)
        {
            return;
        }
        if ( !g._grabbed )
        {
            if (!g._userisgrabber)
            {
            }
            return;
        }
        // @ check if grabber is you
      
          

        Paintable_OLD[] Paintables = FindObjectsOfType<Paintable_OLD>();
        List<Paintable_OLD> currentPaintables = new List<Paintable_OLD>();
        foreach (Paintable_OLD pt in Paintables)
        {
            if (TryPaintOnObject(pt))
            {
                currentPaintables.Add(pt);
            }
        }
        // update lastpaintable
        LastPaintables = new List<Paintable_OLD>();
        foreach (Paintable_OLD p in currentPaintables)
            LastPaintables.Add(p);
    }

}
