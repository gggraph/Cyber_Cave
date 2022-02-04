using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolMod : MonoBehaviour
{

    /*
     Tool Mode Value : -> 
     0      -> edit transform ( set normal shader  ) 
     1      -> edit vertices  ( set wirefram shader )
     
     */
    public static int value = 0;
    public static Texture2D temptex;

    public static void SetModeValue(int val)
    {
        value = val;
        ApplySpecificityOfModeValue(value);

    }

    public static void DisableSpecificityOfModeValue(int val)
    {
        
    }
    public static void ApplySpecificityOfModeValue(int val)
    {
        if ( val == 1)
        {
            // editing vertices 
            GameObject[] meshes = GameObject.FindGameObjectsWithTag("3DMESH");
            foreach ( GameObject go in meshes)
            {
                if ( go.GetComponent<SoundHit>() == null)
                {
                    if (go.GetComponent<Renderer>())
                    {
                        Shader shad = Shader.Find(@"UnityLibrary/Effects/Wireframe");
                        go.GetComponent<Renderer>().material.shader = shad;
                    }
                }
            }
        }
        if (val == 0)
        {
            GameObject[] meshes = GameObject.FindGameObjectsWithTag("3DMESH");
            foreach (GameObject go in meshes)
            {
                if (go.GetComponent<SoundHit>() == null)
                {
                    if (go.GetComponent<Renderer>())
                    {
                        Shader shad = Shader.Find(@"Standard");
                        go.GetComponent<Renderer>().material.shader = shad;
                    }
                }
            }
        }
    }
}
