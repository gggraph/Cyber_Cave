using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class IOUtilities : MonoBehaviour
{
    // IO utilitaries
    public static void AppendBytesToFile(string _filePath, byte[] bytes)
    {

        using (FileStream f = new FileStream(_filePath, FileMode.Append))
        {
            f.Write(bytes, 0, bytes.Length);
        }

    }
    public static void OverWriteBytesInFile(string _filePath, byte[] bytes, int offset)
    {

        /**/
        using (FileStream f = File.OpenWrite(_filePath))
        {
            f.Position = offset;
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
}
