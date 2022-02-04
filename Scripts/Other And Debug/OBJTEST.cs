using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class OBJTEST : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MaterialUtilities.SetMaterialToObjectsFromPool(this.gameObject, 2);
        ObjectUtilities.CreateCustomColliderFromFirstViableMesh(this.gameObject);
       
        this.gameObject.AddComponent<MeshModifier>();
        /*
        //MaterialUtilities.SetMaterialToObjectsFromPool(this.gameObject, 2);
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject c = transform.GetChild(i).gameObject;
            ObjectUtilities.CreateCustomColliderFromFirstViableMesh(c);
            c.AddComponent<MeshModifier>();
            //MaterialUtilities.SetShaderToObjects(c, Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        }
      */
        
        //MaterialUtilities.SetShaderToObjects(this.gameObject, Shader.Find("Legacy Shaders/Transparent/Diffuse"));
    }
    private void Update()
    {

    }


}
