using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reducto : MonoBehaviour
{
    public static List<GameObject> miniatures;
    public static List<GameObject> references;
    public static bool _running = false;

    public static bool TryRunReducto(float distance = 5f, float reductio = 30f)
    {
        if (_running)
            return false;

        _running = true;
        GameObject g = new GameObject();
        g.AddComponent<Reducto>().StartCoroutine(
            g.GetComponent<Reducto>().CreateAndRunReductoWorld(distance, reductio)
            );
       
        return true;

    }

    public IEnumerator CreateAndRunReductoWorld (float distance = 5f, float reductio = 30f)
    {
        /*
         Get All Objects in distance. They should have a MeshModifier Component. They should size at least 3f.  
         Attach those objects to a specific center.
         */
        references = new List<GameObject>();
        miniatures = new List<GameObject>();

        MeshModifier[] modifiers = (MeshModifier[])Object.FindObjectsOfType(typeof(MeshModifier));
        Debug.Log("number of modifiers object : " + modifiers.Length);

        foreach (MeshModifier m in modifiers)
        {

            if (   m.maxBoundaries.size.x >= 3f
                || m.maxBoundaries.size.y >= 3f
                || m.maxBoundaries.size.z >= 3f
                )
            {
                if (Vector3.Distance(m.gameObject.transform.position, Camera.main.transform.position) <= distance)
                {
                    references.Add(m.gameObject);
                }
            }


        }

        Debug.Log("number of objects to miniaturize : " + references.Count);

        if (references.Count == 0)
        {
            _running = false;
            yield break;
        }
        
        foreach (GameObject go in references)
        {
            GameObject m = Instantiate(go);
            m.GetComponent<MeshModifier>().SetSyncing(false);
            miniatures.Add(m);
        }

        Vector3 center = MathUtilities.GetCenterOfObjects(references.ToArray());
        // create a plane here at center
        GameObject H_ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        H_ground.transform.position = center;

        // attach all reducto objects at center 
        foreach (GameObject go in miniatures)
        {
            go.transform.parent = H_ground.transform;
        }
        // miniaturize by rescaling ground ....
        H_ground.transform.localScale /= reductio;

        // apply eye following to ground
        EyeFollower ef = H_ground.AddComponent<EyeFollower>();
        ef.SetDistanceFromCamera(0.2f);
        ef.EulerOffset = new Vector3( -40f,0,0);
       
        while ( !HandRecognition.IsPose_OKSign())
        {
            
            for (int i = 0; i < miniatures.Count; i++)
            {
                // Apply Position from offset of HGROUND reapply * reductio from center... 
                Vector3 offset = miniatures[i].transform.position - H_ground.transform.position; // or use relative localPosition
                offset *= reductio;
                references[i].transform.position = center + offset;

                // Apply rotation 
                references[i].transform.eulerAngles = miniatures[i].transform.localEulerAngles; // use local here i guess

                // Apply scale
                references[i].transform.localScale = miniatures[i].transform.localScale;

            }
            yield return new WaitForEndOfFrame();
        }

        // destroy all miniature objects. 
        for (int i = miniatures.Count - 1; i >= 0; i--)
            Destroy(miniatures[i].gameObject);

        _running = false;
        yield break;
    }

   
}
