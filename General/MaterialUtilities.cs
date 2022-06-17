using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialUtilities : MonoBehaviour
{
    public static bool SetMaterialToSpecificObjectFromPool(GameObject go, int index)
    {
        if (go.GetComponent<Renderer>())
        {
            go.GetComponent<Renderer>().material = GetMaterialFromResourcesPool(index);
            return true;
        }
        return false;
    }

    public static bool SetShaderToObjects(GameObject parent, Shader shad)
    {
        List<GameObject> all = new List<GameObject>();
        ObjectUtilities.GetChildsFromParent(parent, all);
        all.Add(parent);
        Debug.Log("setting material to " + parent.name);
        foreach (GameObject go in all)
        {
            Renderer rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                if (rend.materials.Length == 0)
                {
                    Material m = GetMaterialFromResourcesPool(2);
                    rend.material = m;
                    rend.materials = new Material[1] { m };
                }
                else
                {
                    foreach (Material m in rend.materials)
                    {
                        m.shader = shad;
                    }
                }
                
               
            }


        }
        return true;
    }
    public static bool SetMaterialToObjectsFromPool(GameObject parent, int index)
    {
        List<GameObject> all = new List<GameObject>();
        ObjectUtilities.GetChildsFromParent(parent, all);
        all.Add(parent);
        Material m = GetMaterialFromResourcesPool(index);
        Debug.Log("setting material to " + parent.name);
        foreach ( GameObject go in all)
        {
            Renderer rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                Debug.Log("setting material to " + go.name);
                rend.material = m;
                rend.materials = new Material[1] { m };
            }
                
            
        }
        return true;
    }
    public static Material GetDefaultMaterial()
    {
        return new Material(Shader.Find("Standard"));
    }
    public static Material GetDefaultColorMaterial()
    {
        return new Material(Shader.Find("Unlit/Color"));
    }
    public static Material GetMaterialFromAssets(string name)
    {
        return Resources.Load("Materials/"+name.ToString(), typeof(Material)) as Material;
    }
    public static Material GetMaterialFromResourcesPool(int index)
    {
        return Resources.Load("Materials/Resources_Pool_" + index.ToString(), typeof(Material)) as Material;
    }
    public static void SetMaterialToOpaqueMode(Material material)
    {
        material.SetOverrideTag("RenderType", "");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }
    public static void SetMaterialToFadeMode_Light(Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
    }
    public static void SetMaterialToFadeMode(Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
    public enum BlendMode
    {
        Opaque,
        Cutout,
        Fade,
        Transparent
    }
    public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = -1;
                break;
            case BlendMode.Cutout:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 2450;
                break;
            case BlendMode.Fade:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 3000;
                break;
            case BlendMode.Transparent:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 3000;
                break;
        }

    }



}
