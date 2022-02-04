using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingTool : MonoBehaviour
{
    public static float BrushSize = 0.004f;
    public static Color BrushColor = Color.green;

    public static void SyncNewLineOnObject(GameObject go, Vector3 pos)
    {
        List<byte> data = new List<byte>();
        data.Add(21);
        data.Add(0); // Paint Mode?
        data.Add((byte)go.name.Length);
        foreach (char c in go.name.ToCharArray())
            data.Add((byte)c);
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(pos));
        NetUtilities.SendDataToAll(data.ToArray());
    }
    public static void SyncAddLineOnObject(GameObject go, Vector3 pos)
    {
        List<byte> data = new List<byte>();
        data.Add(21);
        data.Add(1); 
        data.Add((byte)go.name.Length);
        foreach (char c in go.name.ToCharArray())
            data.Add((byte)c);
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(pos));
        NetUtilities.SendDataToAll(data.ToArray());
    }
    
}
