using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


public class MeshSync : MonoBehaviour
{

    public static void UpdateVerticesReceived(byte[] msg)
    {
    }
    public static void UpdateTransformReceived(byte[] msg)
    {
        byte namesize = msg[1];

        char[] goName = new char[namesize];
        for (int i = 2; i < 2 + namesize; i++)
        {
            goName[i - 2] = (char)msg[i];
        }

        GameObject vObj = GameObject.Find(new string(goName));
        if (vObj == null) return;

        BinaryUtilities.DeSerializeTransformOnObject(ref msg, 2 + namesize, vObj);
    }

    public static void AddLine(byte[] msg) 
    {
        byte Mode = msg[1];

        byte namesize = msg[2];
        char[] goName = new char[namesize];
        for (int i = 3; i < 3 + namesize; i++)
        {
            goName[i - 3] = (char)msg[i];
        }
        GameObject vObj = GameObject.Find(new string(goName));
        if (vObj == null) { Debug.Log("Object was Null!"); return; }
        Paint p = vObj.GetComponent<Paint>();
        if ( p == null) { Debug.Log("Paint Scritp was Null!"); return; }
        Vector3 pos = BinaryUtilities.BytesToVector3(ref msg, 3 + namesize);
        if ( Mode == 0) 
        {
            p.CreateNewLine(pos);
        }
        else if ( Mode == 1 )
        {
            p.AddPointToCurrentLine(pos);
        }
       
    }

    public static void UpdateMeshCut(byte[] msg)
    {
      
    }

}
