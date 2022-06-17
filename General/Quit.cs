using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quit : MonoBehaviour
{
    public static void Proccess()
    {
        // Send Ungrabbing message to all grabbed object 

        Grabbable[] gbs = FindObjectsOfType<Grabbable>();
        foreach ( Grabbable g in gbs)
        {
            if (g._userisgrabber && g._grabbed)
                g.ForceReleasing();
            
        }
        // Clear current DLL
        P2SHARE.ForceEndAllDlls();

        // Save Scene
        SceneSaver.SaveSceneAsFile();

        //Quit App cleanely
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif

    }
}
