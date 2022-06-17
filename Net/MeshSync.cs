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

    public static void OnGrabStartReceived(byte[] msg)
    {
        // just the flag and the game Object name ... 
        byte namesize = msg[1];

        char[] goName = new char[namesize];
        for (int i = 2; i < 2 + namesize; i++)
        {
            goName[i - 2] = (char)msg[i];
        }

        GameObject vObj = GameObject.Find(new string(goName));
        if (vObj == null) return;
        ObjectUtilities.DisablePhysicsOnObject(vObj);
        vObj.GetComponent<Grabbable>()._grabbed = true;
    }
    public static void OnGrabEndReceived (byte[] msg)
    {
        byte namesize = msg[1];

        char[] goName = new char[namesize];
        for (int i = 2; i < 2 + namesize; i++)
        {
            goName[i - 2] = (char)msg[i];
        }

        GameObject vObj = GameObject.Find(new string(goName));
        if (vObj == null) return;
        ObjectUtilities.EnablePhysicsOnObject(vObj);
        BinaryUtilities.DeSerializeTransformOnObject(ref msg, 2 + namesize, vObj);
        // +36 
        Vector3 linearVelocity = BinaryUtilities.BytesToVector3(ref msg, 2 + namesize + 36);
        Vector3 angularVelocity = BinaryUtilities.BytesToVector3(ref msg, 2 + namesize + 48);
        vObj.GetComponent<Rigidbody>().velocity = linearVelocity;
        vObj.GetComponent<Rigidbody>().angularVelocity = angularVelocity;
        vObj.GetComponent<Grabbable>()._grabbed = false;

    }

    

    public static void UpdateMeshCut(byte[] msg)
    {
      
    }

}
