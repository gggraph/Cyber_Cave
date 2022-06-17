using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrinterXY : MonoBehaviour
{
    [Header("Setup")]
    public GameObject PaintableObject;
    public GameObject Button;

   
    // Update is called once per frame
    void Update()
    {
        if (!PaintableObject || !Button )
            return;

        TryPrint();
    }
    bool InsideLastFrame = false;
    void TryPrint()
    {
        int result = ObjectUtilities.DoesAnyAvatarFingerInsideObject(Button);
        if (result != -1)
        {
            if (!InsideLastFrame)
            {
                InsideLastFrame = true;
                if (PaintableObject)
                {
                    // Ask to print 
                    SoundMap.FastPlaySoundAtPosition("3dprintingcommand", PaintableObject.transform.position);
                    List<byte> data = new List<byte>();
                    data.Add(81);
                    data.Add((byte)PaintableObject.name.Length);
                    foreach (char c in PaintableObject.name.ToCharArray())
                        data.Add((byte)c);
                    NetUtilities.SendDataToAll(data.ToArray());
                }

            }

        }
        else
            InsideLastFrame = false;
    }
}
