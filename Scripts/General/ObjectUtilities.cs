using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectUtilities : MonoBehaviour
{
    public static GameObject InstantiateGameObjectFromAssets(string path)
    {
        GameObject prefab = Resources.Load(path) as GameObject;
        return UnityEngine.GameObject.Instantiate(prefab);
    }
    public static Texture2D GetTexture2DFromAssets(string path)
    {
        return Resources.Load(path) as Texture2D;
    }

    public static GameObject FindGameObjectChild(GameObject fParent, string name)
    {


        List<GameObject> allchilds = new List<GameObject>();
        GetChildsFromParent(fParent, allchilds); // recursive loop

        foreach (GameObject go in allchilds)
        {
            if (go.name == name)
            {
                return go.gameObject;
            }

        }
        return null;

    }

    public static void GetChildsFromParent(GameObject Parent, List<GameObject> aChild)
    {

        aChild.Add(Parent.gameObject);
        for (int a = 0; a < Parent.transform.childCount; a++)
        {

            aChild.Add(Parent.transform.GetChild(a).gameObject);
            if (Parent.transform.GetChild(a).transform.childCount > 0)
            {
                GetChildsFromParent(Parent.transform.GetChild(a).gameObject, aChild); // recursive loop
            }
        }
    }
    public static bool CreateCustomColliderFromFirstViableMesh(GameObject parent)
    {
        if (parent.GetComponent<Collider>())
        {
            Destroy(parent.GetComponent<Collider>());
        }

        //if ( parent.)
        List<GameObject> all = new List<GameObject>();
        all.Add(parent);
        GetChildsFromParent(parent, all);
        foreach ( GameObject go in all)
        {
            MeshFilter f = go.GetComponent<MeshFilter>();
            if (f != null)
            {
                Mesh m = f.mesh;
                parent.AddComponent<MeshCollider>().sharedMesh = m;
                parent.GetComponent<MeshCollider>().convex = true;
                return true;
               // rend.
            }
        }
        return false;
    }
    public static void RescaleMeshToSize(GameObject obj, float size) 
    {
        Bounds b = GetBoundsOfGroupOfMesh(obj);
        float osize = MathUtilities.GetMaxSpaceTakenByBounds(b);
        float prct = size/ osize;
        obj.transform.localScale = new Vector3(obj.transform.localScale.x * prct, obj.transform.localScale.y * prct, obj.transform.localScale.z * prct);

    }
    public static Bounds GetBoundsOfGroupOfMesh(GameObject obj)
    {
        
        Bounds bounds = new Bounds();
        List<Renderer> renderers = new List<Renderer>();
        Renderer[] rr = obj.GetComponentsInChildren<Renderer>();
        if (obj.GetComponent<Renderer>())
            renderers.Add(obj.GetComponent<Renderer>());
        foreach ( Renderer rend in rr)
        {
            renderers.Add(rend);
        }

        if (renderers.Count > 0)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer.enabled)
                {
                    bounds = renderer.bounds;
                    break;
                }
            }
            //Encapsulate for all renderers
            foreach (Renderer renderer in renderers)
            {
                if (renderer.enabled)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }
        return bounds;
    }

    public static void UnRenderObjectsAround(float radius)
    {

    }
    public static void UnRenderObjects(float radius)
    {

    }
    public static void FadeInObject(GameObject go, float duration = 2f, float maxAlpha = 1f)
    {
        Fade f = go.GetComponent<Fade>();
        if ( f == null)
        {
            go.AddComponent<Fade>().FadeIn(duration, maxAlpha);
            return;
        }
        else
        {
            f.FadeIn(duration, maxAlpha);
            return;
        }
    }
    public static void FadeOutObject(GameObject go, float duration = 2f, float minAlpha = 1f)
    {
        Fade f = go.GetComponent<Fade>();
        if (f == null)
        {
            go.AddComponent<Fade>().FadeOut(duration, minAlpha);
            return;
        }
        else
        {
            f.FadeOut(duration, minAlpha);
            return;
        }
    }
    public static void FadeAndDestroyObject(GameObject go, float duration = 2f)
    {
        Fade f = go.GetComponent<Fade>();
        if (f == null)
        {
            go.AddComponent<Fade>().FadeOutAndDestroy(duration);
            return;
        }
        else
        {
            f.FadeOutAndDestroy(duration);
            return;
        }
    }
    public static void FadeOutObjectsAround(Vector3 position, float radius, float seconds)
    {
        Renderer[] allRend = FindObjectsOfType<Renderer>();
        List<GameObject> candidates = new List<GameObject>();

        Bounds avatarbound = new Bounds(position, new Vector3(radius, radius, radius));
        foreach (Renderer rend in allRend)
        {
            if (!candidates.Contains(rend.gameObject.transform.root.gameObject) && rend.gameObject.transform.root.gameObject != NetUtilities._myAvatar)
            {
                Bounds b = GetBoundsOfGroupOfMesh(rend.transform.root.gameObject);
                if (avatarbound.Intersects(b))
                {
                    candidates.Add(rend.gameObject.transform.root.gameObject);
                   // Debug.Log("Apply fadeout to  " + rend.gameObject.transform.root.gameObject.name);
                }

            }
        }
      
        foreach ( GameObject go in candidates)
        {
            FadeOutObject(go, seconds);
        }
    }
    public static void FadeInObjectsAround(float radius, float seconds)
    {
        Renderer[] allRend = FindObjectsOfType<Renderer>();
        List<GameObject> candidates = new List<GameObject>();
        Bounds avatarbound = new Bounds(Camera.main.transform.position, new Vector3(radius, radius, radius));
        foreach (Renderer rend in allRend)
        {
            if (!candidates.Contains(rend.gameObject.transform.root.gameObject) && rend.gameObject.transform.root.gameObject != NetUtilities._myAvatar)
            {
                if (avatarbound.Intersects(GetBoundsOfGroupOfMesh(rend.transform.root.gameObject)))
                {
                    candidates.Add(rend.gameObject.transform.root.gameObject);
                }
                
            }
        }
        foreach (GameObject go in candidates)
        {
           
            FadeInObject(go, seconds);
        }
    }

    public static GameObject CreateDome()
    {
        GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        float domesize = 20f;

        dome.transform.localPosition = GameObject.Find("OVRCameraRig").transform.position;
        dome.transform.localScale = new Vector3(domesize, domesize, domesize);



        Vector3[] normals = dome.GetComponent<MeshFilter>().mesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        dome.GetComponent<MeshFilter>().sharedMesh.normals = normals;

        int[] triangles = dome.GetComponent<MeshFilter>().sharedMesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int t = triangles[i];
            triangles[i] = triangles[i + 2];
            triangles[i + 2] = t;
        }

        dome.GetComponent<MeshFilter>().sharedMesh.triangles = triangles;

        FadeInObject(dome, 3f, 0.7f);
        dome.AddComponent<SmoothRandomColor>();
        MaterialUtilities.SetMaterialToObjectsFromPool(dome, 2);
        return dome;
    }

}
