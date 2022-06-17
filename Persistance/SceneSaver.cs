using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class SceneSaver : MonoBehaviour
{
    public static byte[] SaveSceneAsFile()
    {
        // @ we will get one file which will be worlsave
        ObjectSaver[] savers = FindObjectsOfType<ObjectSaver>();
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(Utilities.GetTimeStamp()));
        data.AddRange(BitConverter.GetBytes(savers.Length));
        foreach (ObjectSaver o in savers)
            data.AddRange(o.GetEntryData());
        File.WriteAllBytes(Application.persistentDataPath + "/worldsave", data.ToArray());

        return data.ToArray();
    }
}
