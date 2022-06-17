using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BinaryUtilities : MonoBehaviour
{
    // binary utilitaries 
    public static void AddBytesToList(ref List<byte> list, byte[] bytes)
    {
        foreach (byte b in bytes) { list.Add(b); }
    }

    public static Vector3 BytesToVector3(ref byte[] bin, int offset)
    {
        if (bin.Length < offset + 12)
            return new Vector3(0, 0, 0);

        float x = BitConverter.ToSingle(bin, offset);
        float y = BitConverter.ToSingle(bin, offset + 4);
        float z = BitConverter.ToSingle(bin, offset + 8);
        return new Vector3(x, y, z);
    }

    public static byte[] Vector3Tobytes(Vector3 v)
    {
        List<byte> result = new List<byte>();
        AddBytesToList(ref result, BitConverter.GetBytes(v.x));
        AddBytesToList(ref result, BitConverter.GetBytes(v.y));
        AddBytesToList(ref result, BitConverter.GetBytes(v.z));
        return result.ToArray();

    }
    public static Vector3 TransformDataToPosition(ref byte[] data, int off)
    {
        float pX = BitConverter.ToSingle(data, off + 0);
        float pY = BitConverter.ToSingle(data, off + 4);
        float pZ = BitConverter.ToSingle(data, off + 8);
        return new Vector3(pX, pY, pZ);

    }
    public static byte[] SerializeTransform(Transform t)
    {
        List<byte> result = new List<byte>();
        AddBytesToList(ref result, BitConverter.GetBytes(t.position.x));
        AddBytesToList(ref result, BitConverter.GetBytes(t.position.y));
        AddBytesToList(ref result, BitConverter.GetBytes(t.position.z));
        AddBytesToList(ref result, BitConverter.GetBytes(t.eulerAngles.x));
        AddBytesToList(ref result, BitConverter.GetBytes(t.eulerAngles.y));
        AddBytesToList(ref result, BitConverter.GetBytes(t.eulerAngles.z));
        AddBytesToList(ref result, BitConverter.GetBytes(t.localScale.x));
        AddBytesToList(ref result, BitConverter.GetBytes(t.localScale.y));
        AddBytesToList(ref result, BitConverter.GetBytes(t.localScale.z));
        return result.ToArray();
    }

    public static void DeSerializeTransformOnObject(ref byte[] data, int off, GameObject g)
    {
        float pX = BitConverter.ToSingle(data, off + 0);
        float pY = BitConverter.ToSingle(data, off + 4);
        float pZ = BitConverter.ToSingle(data, off + 8);
        float rotX = BitConverter.ToSingle(data, off + 12);
        float rotY = BitConverter.ToSingle(data, off + 16);
        float rotZ = BitConverter.ToSingle(data, off + 20);
        float scX = BitConverter.ToSingle(data, off + 24);
        float scY = BitConverter.ToSingle(data, off + 28);
        float scZ = BitConverter.ToSingle(data, off + 32);

        g.transform.position = new Vector3(pX, pY, pZ);
        g.transform.eulerAngles = new Vector3(rotX, rotY, rotZ);
        g.transform.localScale = new Vector3(scX, scY, scZ);


    }

    public static void SetBit(ref byte b, int p, byte value)
    {

        byte mask = (byte)(1 << p);
        // ((n & ~mask) | (b << p))
        byte lb = b;
        b = (byte)((lb & ~mask) | (value << p));


    }
    public static bool IsBitSet(byte b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }
}
