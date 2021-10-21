using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Utils : MonoBehaviour
{

    // binary utilitaries 
    public static void AddBytesToList(ref List<byte> list, byte[] bytes)
    {
        foreach (byte b in bytes) { list.Add(b); }
    }

    public static Vector3 BytesToVector3(ref byte[] bin, int offset)
    {
        if (bin.Length <= offset + 12)
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

    // IO utilitaries
    public static void AppendBytesToFile(string _filePath, byte[] bytes)
    {

        using (FileStream f = new FileStream(_filePath, FileMode.Append))
        {
            f.Write(bytes, 0, bytes.Length);
        }

    }
    public static byte[] ReadBytesInFiles(string _filePath, long offset, int length)
    {
        byte[] result = new byte[length];
        using (Stream stream = File.Open(_filePath, FileMode.Open))
        {
            //stream.Position = offset;
            stream.Seek(offset, SeekOrigin.Begin); // probably better ???
            //stream.Write(bytes, 0, bytes.Length);
            stream.Read(result, 0, length);
        }
        return result;
    }
    // misc utilitaries
    public static uint GetTimeStamp()
    {
        return (uint)(DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1))).TotalSeconds;
    }

    // object hierarchy utilitaries 

    public static GameObject FindGameObjectChild(GameObject fParent, string name)
    {


        List<GameObject> allchilds = new List<GameObject>();
        GetChildsFromParent(fParent, ref allchilds); // recursive loop

        foreach (GameObject go in allchilds)
        {
            if (go.name == name)
            {
                return go.gameObject;
            }

        }
        return null;

    }

    public static void GetChildsFromParent(GameObject Parent, ref List<GameObject> aChild)
    {

        aChild.Add(Parent.gameObject);
        for (int a = 0; a < Parent.transform.childCount; a++)
        {

            aChild.Add(Parent.transform.GetChild(a).gameObject);
            if (Parent.transform.GetChild(a).transform.childCount > 0)
            {
                GetChildsFromParent(Parent.transform.GetChild(a).gameObject, ref aChild); // recursive loop
            }
        }
    }


    // math utilitaries 

    public static int nearestmultiple(int numToRound, int multiple, bool flr)
    {
        if (multiple == 0)
            return numToRound;

        int remainder = numToRound % multiple;
        if (remainder == 0)
            return numToRound;

        if (!flr)
            return numToRound + multiple - remainder;
        else
            return numToRound - remainder;
    }

    public static Vector3 GetOffsetFromVectors(Vector3 a, Vector3 b)
    {
        return a - b;
    }
    public static Vector3 GetOffsetFromObject(GameObject a, GameObject b)
    {
        return a.transform.position - b.transform.position;
    }

    // crypto utilitaries
    public static string GetUniqueName()
    {
        double ts = (double)(DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1))).TotalMilliseconds;
        string hashname = SHAToHex(ComputeSHA(BitConverter.GetBytes(ts)), false);
        return hashname;
        // hashame will always be equal to 
    }
    public static byte[] ComputeSHA(byte[] msg)
    {

        SHA256 sha = SHA256.Create();
        byte[] result = sha.ComputeHash(msg);
        return result;
    }
    public static string SHAToHex(byte[] bytes, bool upperCase)
    {
        StringBuilder result = new StringBuilder(bytes.Length * 2);

        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

        return result.ToString();
    }


    // debug utilitaries

    public static void PrintInfo(string s)
    {
        GameObject.Find("DEBUG").GetComponent<UnityEngine.UI.Text>().text = s;
    }

}
