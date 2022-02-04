using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using System.IO;
using System.Linq;
public class WorldUpdater : MonoBehaviour
{
    /*-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ SPECIFIC TO MASTER -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_*/
    public static byte[] RegFlags = new byte[256]
    {
        0, 1, 1, 1, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 1, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0

    };

    // List of msg to create blocks save if needed
    public static List<byte[]> _unregistered_msg = new List<byte[]>();
    public static void OnWorldUpdateRequest(byte[] msg, PhotonMessageInfo info)
    {
       
        /*
         data structure :
         [4] 
         ->header (1 o ) 
         ->the last timestamp the client has ( 4 o)
         */
        uint clientlastts = BitConverter.ToUInt32(msg, 1); 

        if (!Directory.Exists(Application.persistentDataPath + "/save"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/save");
            Debug.Log("creating save directory...");
        }
        string logpath = Application.persistentDataPath + "/save/chksmlog.txt";
        if (!File.Exists(logpath))
        {
            Debug.Log("Creating save log");
            File.WriteAllBytes(logpath, new byte[0]);
        }
        string[] lines = File.ReadAllLines(logpath);

        List<string> fNeeded = new List<string>();
        foreach (string s in lines)
        {
            // just read in first bytes ( block save first bytes will be timestamp, next series of network message rougly write to bytes ) 
            if ( s.Length >0 )
            {
                uint cts = BitConverter.ToUInt32(IOUtilities.ReadBytesInFiles(s, 0, 4), 0);
                if (cts > clientlastts)
                {
                    fNeeded.Add(s); //  adding files to send ... :) 
                }
            }
        }

        string nblockpath = CreateNewBlockSave();
        if (nblockpath!=null)
        {
            fNeeded.Add(nblockpath);
            //add it also to log
            List<string> chkl = lines.ToList();
            chkl.Add(nblockpath);
            File.WriteAllLines(logpath, chkl.ToArray());
        }

        List<byte> data = new List<byte>();
        data.Add(5);
        // OK Hope file list will never reach 64ko
        foreach ( string s in fNeeded)
        {
            byte[] fts = IOUtilities.ReadBytesInFiles(s, 0, 4); // get the timestamp
            FileInfo fi = new FileInfo(s);
            byte[] chksm = CryptoUtilities.HexToSHA(fi.Name);
            BinaryUtilities.AddBytesToList(ref data, fts);
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes((int)fi.Length));
            BinaryUtilities.AddBytesToList(ref data, chksm);
        }

        NetUtilities.SendDataToSpecific(data.ToArray(), info.Sender);


    }
    
    // create new block and return the path...
    public static string CreateNewBlockSave()
    {
        if (_unregistered_msg.Count > 0)
        {
            Debug.Log("starting creating new block save file...");
            List<byte> blockdata = new List<byte>();
            uint cts = Utilities.GetTimeStamp();
            // we load and rewrite every byte roughly to ram ... some sytem of regulation will be needed in receivedata to avoid ram overflow
            BinaryUtilities.AddBytesToList(ref blockdata, BitConverter.GetBytes(cts));
            foreach (byte[] dts in _unregistered_msg)
            {
                // add the length of msg bytes ??? // should never going more than 256 ??? i guess ????
                blockdata.Add((byte)dts.Length); // should never go more than 255 length ... 
                BinaryUtilities.AddBytesToList(ref blockdata, dts);
            }
            // compute the checksum
            byte[] checksum = CryptoUtilities.ComputeSHA(blockdata.ToArray());
            string npath = Application.persistentDataPath + "/save/" + CryptoUtilities.SHAToHex(checksum);
            File.WriteAllBytes(npath, blockdata.ToArray());
            // we could do some lpz compression like in sc4 save for more efficiency 
            Debug.Log("clearing unprocessed _message");
            _unregistered_msg = new List<byte[]>();
            Debug.Log("New block save file created :  " + npath);

            return npath;
           
        }
        else
        {
            Debug.Log("wtf unproc is equal to 0 ...");
        }
        
        return null;
    }

    /*-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ SPECIFIC TO CLIENT -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_*/

    public static void LoadBlockFiles(string[] files)
    {
        
        List<Tuple<uint, string>> sortedFiles = new List<Tuple<uint, string>>();

        foreach (string f in files)
        {
            uint fts = BitConverter.ToUInt32(IOUtilities.ReadBytesInFiles(f, 0, 4), 0);
            sortedFiles.Add(new Tuple<uint, string>(fts, f));
        }
        if (sortedFiles.Count == 0)
            return;

        sortedFiles.Sort((x, y) => y.Item1.CompareTo(x.Item1));
        
        foreach (Tuple<uint, string> t in sortedFiles)
        {
            LoadBlockSave(t.Item2);
        }

    }
    public static void LoadWorld()
    {

        if (!Directory.Exists(Application.persistentDataPath + "/save"))
        {
            Debug.Log("Creating save directory");
            Directory.CreateDirectory(Application.persistentDataPath + "/save");
        }
        string logpath = Application.persistentDataPath + "/save/chksmlog.txt";
        if (!File.Exists(logpath))
        {
            Debug.Log("Creating save log");
            File.WriteAllBytes(logpath, new byte[0]);
        }
        /*
            Sort all block save by their timestamp... ( first 4 bytes ) 
        */
        string[] lines = File.ReadAllLines(logpath);

        List<Tuple<uint, string>> sortedFiles = new List<Tuple<uint, string>>();

        foreach ( string f in lines)
        {
            if ( f.Length > 0)
            {
                uint fts = BitConverter.ToUInt32(IOUtilities.ReadBytesInFiles(f, 0, 4), 0);
                sortedFiles.Add(new Tuple<uint, string>(fts, f));
            }
          
        }
        if (sortedFiles.Count == 0)
            return;
        sortedFiles.Sort((x, y) => y.Item1.CompareTo(x.Item1));

        foreach (Tuple<uint, string> t in sortedFiles)
        {
            LoadBlockSave(t.Item2);
        }

    }
    public static void LoadBlockSave(string fpath)
    {
        long byteOffset = 4;  // jump ts 
        long fileLength = new FileInfo(fpath).Length;
        while (byteOffset < fileLength)
        {
            byte messagelength = IOUtilities.ReadBytesInFiles(fpath, byteOffset, 1)[0];
            byte[] msg = IOUtilities.ReadBytesInFiles(fpath, byteOffset + 1, messagelength);
            NetUtilities._mNetStream.ReceiveData(msg, new PhotonMessageInfo()); // seems ok to me ???
            byteOffset += messagelength + 1; // adding the byte to byte Offset

        }

    }
    public static void RequestWorldUpdate()
    {
        Debug.Log("Asking for Update");
        if (!Directory.Exists(Application.persistentDataPath + "/save"))
        {
            Debug.Log("Creating save directory");
            Directory.CreateDirectory(Application.persistentDataPath + "/save");
        }
        string logpath = Application.persistentDataPath + "/save/chksmlog.txt";
        if (!File.Exists(logpath))
        {
            Debug.Log("Creating save log");
            File.WriteAllBytes(logpath, new byte[0]);
        }
        string[] lines = File.ReadAllLines(logpath);
        uint lastts = 0;
        foreach ( string s in lines) 
        {
            if ( s.Length > 0)
            {
                uint cts = BitConverter.ToUInt32(IOUtilities.ReadBytesInFiles(s, 0, 4), 0);
                if (cts > lastts)
                {
                    lastts = cts;
                }
            }
           
        }
        Debug.Log("Asking Update from " + Utilities.UnixTimeStampToDateTime(lastts).ToString());
        List<byte> msg = new List<byte>();
        msg.Add(4);
        BinaryUtilities.AddBytesToList(ref msg, BitConverter.GetBytes(lastts));
        NetUtilities.SendDataToMaster(msg.ToArray());
       
    }

    public static void OnBlocksListReceived(byte[] msg, PhotonMessageInfo info)
    {
        // check if info sender is the master ...
        List<string> blockList = new List<string>();
        int off = 1; 
        while ( off < msg.Length)
        {
            // respecively : ts (4b) , file length (4b) , checksum (32b)
            byte[] chksm = new byte[32];
            for (int i = 0; i < 32; i ++)
            {
                chksm[i] = msg[off + 8 + i];
            }
            int flen = BitConverter.ToInt32(msg, off + 4);
            P2SHARE.DL dl = P2SHARE.RequestNewFile(chksm, flen, 2, null); // DO NULL FOR DA MOMENT.
            blockList.Add(dl.filePath);
            off += 40;
        }
        // start coroutine for maj update : 
        NetUtilities._mNetStream.StartCoroutine(NetUtilities._mNetStream.DoMAJCheck(blockList.ToArray()));
    }
}
