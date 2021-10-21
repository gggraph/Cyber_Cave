using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
public class DataReceiver : MonoBehaviour
{

    public bool _isMine = false;
    public string _nickname = "Gael";
    /* just for a local servor test */
    public bool _imaServer = false;
    public string separator_token = "/";


    // Start is called before the first frame update
    void Start()
    {

        //  GenerateNewPairKey();

        /*
         Auth is automatized : 
            every one have the PrivateKey
            every one have the Public Key of Every one

        When we log, 
        we sign the timestamp. 

        
         */

        /*
        if (Application.persistentDataPath == @"C:/Users/gaelg/AppData/LocalLow/DefaultCompany/Cyber_Cave")
        {
            _imaServer = true;
        }*/

        _imaServer = true;

        // _imaserver will be true ... separator token has to be change if on windows ... 
        if (GetComponent<PhotonView>().IsMine)
        {
            _isMine = true;
            // Invoke("SendData", 1f);
        }
        if (_isMine)
        {
            if (!_imaServer)
            {
                //GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = "asking update ";
                Invoke("UpdateCheck", 3f); // just waiting ...
            }
            else
            {
                /*
                if (!Directory.Exists(Application.persistentDataPath + separator_token + "save"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + separator_token + "save");
                }
                DirectoryInfo df = new DirectoryInfo(Application.persistentDataPath + separator_token + "save");
                FileInfo[] files = df.GetFiles().OrderBy(p => p.CreationTimeUtc).ToArray(); // during this time also sort file by names and process update 

                foreach (FileInfo f in files)
                {
                    string s = f.FullName;
                    UpdateWorld(s); // :-)

                }
                */
            }

        }
    }

  
    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // e.g. store this gameobject as this player's charater in Player.TagObject
        info.Sender.TagObject = this.gameObject;

    }

    public void SendData(byte[] data) // send it to everyone
    {
        GetComponent<PhotonView>().RPC("ReceiveData", RpcTarget.Others, data);

    }
    public void SendDataTest()
    {
        // we can use photonview id to sign our message
        Debug.Log("called");
        GetComponent<PhotonView>().RPC("ReceiveData", RpcTarget.Others, new byte[60]); // or rpc target all

    }


    public static List<byte[]> unprocessed_msg = new List<byte[]>();

    public void ProcessUpdateDemand(byte[] msg, PhotonMessageInfo info)
    {
        /*
         data structure :
         ->header (1 o ) 
         ->the last timestamp the client has ( 4 o)


        then what we do : 
        -> just get all update file sup to this timestamp 
        -> prepare unprocessed msg to create a new block save file if unproccessed is not empty 
        -> send all of those needed files ( idk what is the max data for rpc but need to know ) 
        -> clear unprocess msg

         */
        // only for the server :) 
        if (!_imaServer)
            return;

        Debug.Log("Starting update for client demand ... ");

        //GetComponent<PhotonView>().RPC("ReceiveData", RpcTarget., msg); nned to change rpc target to the client ID
        uint clientlastts = BitConverter.ToUInt32(msg, 1); // jump the header and get last ts ...
        Debug.Log("need file from " + clientlastts + " sec unix");
        // create directory if needed lol :')
        if (!Directory.Exists(Application.persistentDataPath + separator_token + "save"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + separator_token + "save");
            Debug.Log("creating save directory...");
        }
        else
        {
            Debug.Log("directory existing ...");
        }
        string directorypath = Application.persistentDataPath + separator_token + "save";
        //just get all update file sup to this timestamp 
        string[] files = Directory.GetFiles(Application.persistentDataPath + separator_token + "save");
        List<string> fNeeded = new List<string>();
        foreach (string s in files)
        {
            // just read in first bytes ( block save first bytes will be timestamp, next series of network message rougly write to bytes ) 
            uint cts = BitConverter.ToUInt32(ReadBytesInFiles(s, 0, 4), 0);
            if (cts > clientlastts)
            {
                fNeeded.Add(s); //  adding files to send ... :) 
                Debug.Log(s + " required...");
            }

        }
        // now processing unp msg ... ( so create a new block save file ... ) 
        if (unprocessed_msg.Count > 0)
        {
            Debug.Log("starting creating new block save file...");
            List<byte> blockdata = new List<byte>();
            uint cts = GetTimeStamp();
            // we load and rewrite every byte roughly to ram ... some sytem of regulation will be needed in receivedata to avoid ram overflow
            blockdata = AddBytesToList(blockdata, BitConverter.GetBytes(cts));
            foreach (byte[] dts in unprocessed_msg)
            {
                // add the length of msg bytes ??? // should never going more than 256 ??? i guess ????
                blockdata.Add((byte)dts.Length); // should never go more than 255 length ... 
                blockdata = AddBytesToList(blockdata, dts);
            }
            string npath = Application.persistentDataPath + separator_token + "save" + separator_token + cts.ToString();
            File.WriteAllBytes(npath, ListToByteArray(blockdata));
            // we could do some lpz compression like in sc4 save for more efficiency 
            fNeeded.Add(npath);
            Debug.Log("clearing unprocessed _message");
            unprocessed_msg = new List<byte[]>();

            Debug.Log("New block save file created :  " + npath);
        }
        else
        {
            Debug.Log("wtf unproc is equal to 0 ...");
        }
        // send maj to client ( 64000 bytes is the max rpc size  ) 
        // byte header is 6 . 
        /*
         maj data structure : 
         -> byte header (6)  (1 o ) 
         -> timestamp (4o)
         -> files offset counter ( 4o ) 
         -> files length counter ( 4o ) 
         -> long current packeddata offset (8 o) 
         -> long packeddata length (8 o ) 
         -> packeddata chunk  ( ?? o )
         then foreach 
         */
        uint filecounter = 0;
        foreach (string s in fNeeded)
        {

            long flength = new FileInfo(s).Length; // it returns 0 olmg im pretty sure
            Debug.Log("sending file " + flength + "bytes ");
            // send it every 64000 chunk 

            long byteOffset = 0;

            while (byteOffset < flength)
            {
                bool needbreak = false;
                List<byte> lcformat = new List<byte>();
                lcformat.Add(6); // header 
                lcformat = AddBytesToList(lcformat, ReadBytesInFiles(s, 0, 4));  // timestamp 
                lcformat = AddBytesToList(lcformat, BitConverter.GetBytes(filecounter)); // files offset list
                lcformat = AddBytesToList(lcformat, BitConverter.GetBytes((uint)fNeeded.Count)); // files length
                lcformat = AddBytesToList(lcformat, BitConverter.GetBytes(byteOffset)); // offset 8 o
                lcformat = AddBytesToList(lcformat, BitConverter.GetBytes(flength));  //  length  8 o
                if (byteOffset + 64000 < flength)
                {
                    lcformat = AddBytesToList(lcformat, ReadBytesInFiles(s, byteOffset, 64000));
                }
                else
                {
                    long b2r = flength - byteOffset;
                    lcformat = AddBytesToList(lcformat, ReadBytesInFiles(s, byteOffset, (int)b2r));
                    needbreak = true;
                }
                // send back the packed data to the client
                GetComponent<PhotonView>().RPC("ReceiveData", info.Sender, ListToByteArray(lcformat)); // its probably an ECHO ok need to check ...
                Debug.Log("[update progress] Sending file " + filecounter + "/" + fNeeded.Count + " : " + byteOffset + "/" + flength + " bytes ...");
                if (needbreak) { break; }
                byteOffset += 64000;
            }


            filecounter++;
        }
        Debug.Log("Update send... ");
    }

    public static List<string> Unprocessed_Savefiles = new List<string>();
   
   
    public static List<byte> AddBytesToList(List<byte> list, byte[] bytes)
    {
        foreach (byte b in bytes) { list.Add(b); }
        return list;
    }
    public static byte[] ListToByteArray(List<byte> list)
    {
        byte[] result = new byte[list.Count];
        for (int i = 0; i < list.Count; i++) { result[i] = list[i]; }
        return result;
    }

    public uint GetTimeStamp()
    {
        return (uint)(DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1))).TotalSeconds;
    }
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

    [PunRPC]
    public void ReceiveData(byte[] data, PhotonMessageInfo info)
    {
        // archive will be on the xr for the moment
        // archive stuff maj etc. 
        // only if it is the server 

        Debug.Log("data received = " + data.Length);
        // get the header 
        if (data.Length == 0)
            return;
        byte header = data[0];
        // header 0 is call from clien tog get update 

        if (_imaServer && header != 4) // header 4 will be a client calling for update, so dont add it to unprocess msg
        {
            unprocessed_msg.Add(data);
            Debug.Log("adding unproc_msg #" + unprocessed_msg.Count);
        }
        if ( header == 4)
        {
            ProcessUpdateDemand(data, info);
        }

    }

    // process data for gameobject 


    public  void GenerateNewPairKey() // better to use offline & on another device. 
    {
        if (File.Exists(Application.persistentDataPath + separator_token + "privateKey") || File.Exists(Application.persistentDataPath + separator_token + "publicKey"))
        {
            Debug.Log("Already existing RSA key files has been found in app folder. Please move them or rename them. RSA Key Gen has been aborted");
            return;
        }
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
        byte[] _privateKey = rsa.ExportCspBlob(true);
        byte[] _publicKey = rsa.ExportCspBlob(false);
        File.WriteAllBytes(Application.persistentDataPath + separator_token + "privateKey", rsa.ExportCspBlob(true));
        File.WriteAllBytes(Application.persistentDataPath + separator_token + "publicKey", rsa.ExportCspBlob(false));
        rsa.Clear();
        Debug.Log("RSA DONE.");

    }

    public static bool VerifyTransactionDataSignature(byte[] signature)
    {
        return true;
        /*
        // 4 bytes (sign) and pukey (532 ) 
        if (dataBytes.Length != 1100) { return false; }

        byte[] mPKey = new byte[532];
        byte[] msg = new byte[588];
        byte[] signature = new byte[512];
        for (int i = 0; i < 532; i++) { mPKey[i] = dataBytes[i]; }
        for (int i = 0; i < 588; i++) { msg[i] = dataBytes[i]; }
        for (int i = 588; i < 1100; i++) { signature[i - 588] = dataBytes[i]; }

        RSACryptoServiceProvider _MyPuRsa = new RSACryptoServiceProvider();
        try
        {
            _MyPuRsa.ImportCspBlob(mPKey);
            bool success = _MyPuRsa.VerifyData(msg, CryptoConfig.MapNameToOID("SHA256"), signature);
            if (success)
            {
                _MyPuRsa.Clear();
                return true;
            }
            else
            {
                _MyPuRsa.Clear();
                return false;
            }


        }
        catch (CryptographicException e)
        {
            Print(e.Message);
            _MyPuRsa.Clear();
            return false;
        }
        */
    }



}
