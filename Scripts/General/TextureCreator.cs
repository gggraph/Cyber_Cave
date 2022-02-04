using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureCreator : MonoBehaviour
{
    public void Start()
    {
        SetDefaultIfNoTexture();
    }
    public void SetDefaultIfNoTexture()
    {
        if (GetComponent<Renderer>().material.mainTexture)
            return;

        GetComponent<Renderer>().material = MaterialUtilities.GetMaterialFromResourcesPool(2);
        Texture2D tex = new Texture2D(100, 100);
        GetComponent<Renderer>().material.mainTexture = tex;
        Destroy(this);
    }
}
