using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class OnExit : MonoBehaviour
{
    void OnApplicationQuit()
    {
        if (NetStream._imMaster)
        {
            WorldUpdater.CreateNewBlockSave();
        }
        foreach ( P2SHARE.DL dl in P2SHARE._dlls)
        {
            string fPath = dl.filePath;
            P2SHARE._dlls.Remove(dl);
            File.Delete(fPath);
        }

        BitMemory.SaveMemories();
    }
}
