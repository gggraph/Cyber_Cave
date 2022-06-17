using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DLlog : MonoBehaviour
{
    

    // Update is called once per frame
    void Update()
    {
        string myLog = "";
        List<P2SHARE.DL> dlls = P2SHARE._dlls;
        if (dlls.Count > 0)
            myLog += "[Current DLL Progress]" + "\n";
        foreach (P2SHARE.DL dl in dlls)
        {
            myLog += "-_-_-_ DLL #" + dl.filePath +" -_-_-_-_-" + "\n";
            myLog += "Size : " + dl.fileSize + " bytes" + "\n";
            if (Utilities.GetTimeStamp() - dl.ini_timestamp > 10)
            {
                myLog += "[WARNING] No packet received from seeders since " + (Utilities.GetTimeStamp() - dl.ini_timestamp) + "seconds." + "\n";
            }
            
            myLog += "[Progress]"; 
            //(float)c.cell_fill_status / (float)dl.fileSize.
            foreach (P2SHARE.UploadCell c in dl._cells)
            {
                myLog += "cell #" + c.o + " : " + ((float)c.cell_fill_status / (float)c.l)*100 + " %" + "\n";
            }
            myLog += "-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-" + "\n";
        }
        this.GetComponent<TextMesh>().text = myLog;
    }
}
