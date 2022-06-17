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

    public static void AddDefaultComponentsToObjectInstance(GameObject go, bool addPhysics = false)
    {
       // can depend. Need to be tested.
        CreateCustomColliderFromFirstViableMesh(go);
        go.AddComponent<Grabbable>();
        go.AddComponent<Cuttable>();
        go.AddComponent<Paintable>();
        if (addPhysics)
        {
            go.AddComponent<Rigidbody>().useGravity = true;
        }
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
    // Something had to be rethink here cause physics can be applied to a a parent without anthing on it... 
    public static bool DisablePhysicsOnObject(GameObject parent)
    {
        List<GameObject> all = new List<GameObject>();
        all.Add(parent);
        GetChildsFromParent(parent, all);
        foreach ( GameObject go in all)
        {
            if (go.GetComponent<Rigidbody>())
            {
                go.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
        return true;
    }
    public static bool EnablePhysicsOnObject(GameObject parent)
    {
        List<GameObject> all = new List<GameObject>();
        all.Add(parent);
        GetChildsFromParent(parent, all);
        foreach (GameObject go in all)
        {
            if (go.GetComponent<Rigidbody>())
            {
                go.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
        return true;
    }

    public static void ClearObjectVelocity(GameObject go)
    {
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (!rb)
            return;
        rb.angularVelocity = new Vector3();
        rb.velocity = new Vector3();
    }

    public static bool DoesObjectTouchOther_SphereCast(GameObject a, GameObject b,  float radius = 1f)
    {
       
        RaycastHit[] hits;
        Ray ray = new Ray(a.transform.position, new Vector3(0,0,0));
        hits = Physics.SphereCastAll(ray, radius, 0.00001f);

        List<GameObject> allchilds = new List<GameObject>();
        ObjectUtilities.GetChildsFromParent(b, allchilds); // recursive loop
        foreach (GameObject g in allchilds)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.gameObject == g)
                    return true;
            }
        }

        return false;
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

    /*
     [ NOTE ABOUT BOUNDS] 
     some bounds that can be accessed : 
	    renderer.bounds interesting but changing over time ( by point of view) (it 's the bounds of the part of mesh renderered ) 
	    mesh.bounds could be interesting but only local. We have to convert it to world space. 
	    collider.bounds : a really nice bounds that fit mesh bounds in world space but object should have a collider... 
     
     */

    public static Collider[] GetCollidersOfMesh(GameObject parent)
    {
        List<Collider> colliders = new List<Collider>();
        Collider[] cs = parent.GetComponentsInChildren<Collider>();
        if (parent.GetComponent<Collider>())
            colliders.Add(parent.GetComponent<Collider>());
        foreach (Collider c in cs)
        {
            colliders.Add(c);
        }
        return colliders.ToArray();
    }
    public static Bounds GetBoundsOfGroupOfMesh(GameObject obj)
    {
        // This altering bounds i dont know why size is fucked up 
        Bounds bounds = new Bounds();
        List<Collider> colliders = new List<Collider>();
        Collider[] cs = obj.GetComponentsInChildren<Collider>();
        if (obj.GetComponent<Collider>())
            colliders.Add(obj.GetComponent<Collider>());
        foreach (Collider c in cs)
        {
            colliders.Add(c);
        }
        
        if ( colliders.Count > 0)
        {
            // Copy first collider bounds ... 
            Collider c = colliders[0];
            // Because collider is a pointer and we will encapsulate. We should copy and not pass pointer. 
            bounds.center = new Vector3(c.bounds.center.x, c.bounds.center.y, c.bounds.center.z);
            bounds.extents = new Vector3(c.bounds.extents.x, c.bounds.extents.y, c.bounds.extents.z);
            bounds.min = new Vector3(c.bounds.min.x, c.bounds.min.y, c.bounds.min.z);
            bounds.max = new Vector3(c.bounds.max.x, c.bounds.max.y, c.bounds.max.z);
            bounds.size = new Vector3(c.bounds.size.x, c.bounds.size.y, c.bounds.size.z);
            // Encapsulate
            foreach (Collider coll in colliders)
            {
                bounds.Encapsulate(coll.bounds);
            }
        }
        return bounds;
    }

    public static int DoesAnyFingerInsideObject(GameObject colOb)
    {
        if (!colOb)
            return -1;
        if (!colOb.GetComponent<MeshCollider>())
            return -1;

        if (HandUtilities.IsLeftHandTracked())
        {
            for (int i = 0; i < 19; i++)
            {
                Vector3 pos = HandUtilities.LeftHand.GetComponent<OVRSkeleton>().Bones[i].Transform.position;
                if (colOb.GetComponent<MeshCollider>().bounds.Contains(pos))
                    return 0;
            }
            
        }
        if (HandUtilities.IsRightHandTracked())
        {
            for (int i = 0; i < 19; i++)
            {
                Vector3 pos = HandUtilities.RightHand.GetComponent<OVRSkeleton>().Bones[i].Transform.position;
                if (colOb.GetComponent<MeshCollider>().bounds.Contains(pos))
                    return 1;
            }
        }
        return -1;
    }
    public static int DoesAnyAvatarFingerInsideObject(GameObject colOb)
    {
        if (!colOb)
            return -1;
        if (!colOb.GetComponent<MeshCollider>())
            return -1;
        
        GameObject avatar = Camera.main.transform.root.gameObject.GetComponent<AnchorUpdater>().mAvatar;
        if (!avatar)
            return -1;

        // Now Get BodyAnimation in script
        // up to i:18 it is right
        List<GameObject> fingerBones = avatar.GetComponentInChildren<BodyAnimation>().fingerBones;
        for (int i = 0; i < fingerBones.Count; i++)
        {
            if (!fingerBones[i])
                continue;
            Vector3 pos = fingerBones[i].transform.position;
            if (colOb.GetComponent<MeshCollider>().bounds.Contains(pos))
            {
                if (i > 18)
                    return 1;
                else
                    return 0;
            }
                
        }
       
        return -1;
    }
    // @ Return true if any control inside collider
    public static int CheckIfControllerInsideObject(GameObject colOb)
    {
        if (!colOb)
            return -1;
        if (!colOb.GetComponent<MeshCollider>())
            return -1;

        Vector3 pos;
        if (HandUtilities.IsLeftHandTracked())
        {
            pos = HandUtilities.LeftHand.transform.position;
            if (colOb.GetComponent<MeshCollider>().bounds.Contains(pos))
                return 0;
        }
        if (HandUtilities.IsRightHandTracked())
        {
            pos = HandUtilities.RightHand.transform.position;
            if (colOb.GetComponent<MeshCollider>().bounds.Contains(pos))
                return 1;
        }
        if (ControllerData.IsLeftControllerUsed())
        {
            pos = ControllerData.GetLeftControllerPosition();
            if (colOb.GetComponent<MeshCollider>().bounds.Contains(pos))
                return 0;
        }
        if (ControllerData.IsRightControllerUsed())
        {
            pos = ControllerData.GetRightControllerPosition();
            if (colOb.GetComponent<MeshCollider>().bounds.Contains(pos))
                return 1;
        }
        return -1;
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

    public static GameObject CreateDome(float ColorVariationSpeed = 3f)
    {
        GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(dome.GetComponent<SphereCollider>());
        float domesize = 20f;

        dome.transform.position = Camera.main.transform.root.gameObject.transform.position;
        Debug.Log("Main camera object is " + Camera.main.transform.root.gameObject.name + " position is " + Camera.main.transform.root.gameObject.transform.position);
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
        dome.AddComponent<SmoothRandomColor>().Speed = ColorVariationSpeed;
        MaterialUtilities.SetMaterialToObjectsFromPool(dome, 2);
        return dome;
    }

}
