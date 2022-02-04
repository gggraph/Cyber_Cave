using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class IOManager : MonoBehaviour
{
    public static void CheckFileDirectories()
    {
        // Create all necessary folders which stores data.
        if (!Directory.Exists(Application.persistentDataPath + "/default"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/default");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/mesh"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/mesh");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/sound"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/sound");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/texture"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/texture");
        }

        CheckDirectory(Application.persistentDataPath + "/default");
        CheckDirectory(Application.persistentDataPath + "/mesh");
        CheckDirectory(Application.persistentDataPath + "/sound");
        CheckDirectory(Application.persistentDataPath + "/texture");
    }
    public static void AddFileToChecksumLog(string filePath)
    {
        FileInfo f = new FileInfo(filePath);
        string dirpath = f.DirectoryName;
        if (!File.Exists(dirpath + "/chksmlog.txt"))
        {
            File.WriteAllBytes(dirpath + "/chksmlog.txt", new byte[0]);
        }

        string[] lines = File.ReadAllLines(dirpath + "/chksmlog.txt");
        List<string> chksmlog = lines.ToList();
        chksmlog.Add(f.FullName);
        File.WriteAllLines(dirpath + "/chksmlog.txt", chksmlog.ToArray());

    }


    public static void CheckDirectory(string dirName)
    {
        // check if chksmlog is in there.
        if (!File.Exists(dirName + "/chksmlog.txt"))
        {
            File.WriteAllBytes(dirName + "/chksmlog.txt", new byte[0]);
        }
        string[] lines = File.ReadAllLines(dirName + "/chksmlog.txt");
        List<string> chksmlog = lines.ToList();


        DirectoryInfo dir = new DirectoryInfo(dirName);
        FileInfo[] files = dir.GetFiles();

        foreach ( FileInfo f in files)
        {
            if (!DoesStringExists(f.FullName, chksmlog) && f.Name != "chksmlog.txt")
            {
                // okay now do checksum
                byte[] chksm = CryptoUtilities.DoFileCheckSum(f.FullName);
                string sumstr = CryptoUtilities.SHAToHex(chksm);

                // rename the file 
                string newpath = Path.Combine(f.Directory.FullName, sumstr);
                f.MoveTo(newpath); // this works
                // add the new path to log 
                chksmlog.Add(newpath);
                Debug.Log("proccessing " + f.FullName);
            }
        }
        File.WriteAllLines(dirName + "/chksmlog.txt", chksmlog.ToArray());

    }

    public static bool DoesStringExists(string str, List<string> list)
    {
        foreach ( string s in list)
        {
            if (s == str)
                return true;
        }
        return false;
    }
}
