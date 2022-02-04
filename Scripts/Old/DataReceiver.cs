//                                  OLD WRAPPER

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Dummiesman;
using System.Threading;

// NETSTREAM stuff
public class DataReceiver : MonoBehaviour
{
   
    public bool _isMine = false;
    public string _nickname;
   
    public bool _imaMaster = false;
    public string separator_token = "/";

    public static byte[] _servKey = new byte[1]
    {
        0
    };
    // we absolutely not need this in fact 

    public void TestDL()
    { // byte length 5984151
        // checksum :602ec769af70aa5b82ec153a911ccc5ffa20c1e1fb5b8224d6ba1ce684fc34ff 
        FileInfo fi = new FileInfo("default\\test.mp3");
        Debug.Log("file length = " + fi.Length);
        byte[] chksm = Utils.DoFileCheckSum("default\\test.mp3");

        string smstr = CryptoUtilities.SHAToHex(chksm, false);
        Debug.Log(smstr);

        // Ask for a specific file 
        P2SHARE.RequestNewFile(chksm, (int)fi.Length, 0);

    }
    void Start()
    {

        // -*------------------- TESTING PI2SHARE --------------------* 
       
        // start DLL threading ...
      
        Invoke("TestDL", 5f);
       
       

        // _imaserver will be true ... separator token has to be change if on windows ... 
        if (GetComponent<PhotonView>().IsMine)
        {
            _isMine = true;
            // Invoke("SendData", 1f);
        }
        if (_isMine)
        {
            // start some threading 
            PhotonView pw = GetComponent<PhotonView>();
            new Thread(() => P2SHARE.ProccessUpload(pw)).Start();
            if (!_imaMaster)
            {
                //Invoke("UpdateCheck", 3f); // Invoke Update
                // will not need it here 
               // Invoke("Auth", 3f); // Invoke Update
            }
            else
            {
              
            }

        }
    }

    void Auth() 
    {
        int authid = -1;
        // [0]  search .ini 
        if ( File.Exists(Application.persistentDataPath + separator_token + "chrauth"))
        {
            authid = BitConverter.ToInt32(File.ReadAllBytes(Application.persistentDataPath + separator_token + "chrauth"), 4);
        }
        // [1]  search .puk file & .prk file
        string[] pukpath = Directory.GetFiles(Application.persistentDataPath, "*.puk", SearchOption.AllDirectories);
        string[] prkpath = Directory.GetFiles(Application.persistentDataPath, "*.prk", SearchOption.AllDirectories);
        
        if (pukpath.Length == 0 || prkpath.Length == 0)
        {
            GenerateNewPairKey();
            pukpath = Directory.GetFiles(Application.persistentDataPath, "*.puk", SearchOption.AllDirectories);
            prkpath = Directory.GetFiles(Application.persistentDataPath, "*.prk", SearchOption.AllDirectories);
        }

        byte[] puk = File.ReadAllBytes(pukpath[0]);
        byte[] prk = File.ReadAllBytes(prkpath[0]);

        // [2] Now build msg 4 server 

        List<byte> msg = new List<byte>();
        msg.Add(7); // first byte is flag : auth 

        byte[] tsbytes = BitConverter.GetBytes(Utilities.GetTimeStamp());
        BinaryUtilities.AddBytesToList(ref msg, tsbytes);
        BinaryUtilities.AddBytesToList(ref msg, SignData(prk));
        if (authid == -1)
        {
            BinaryUtilities.AddBytesToList(ref msg, puk);
        }
        else
        {
            BinaryUtilities.AddBytesToList(ref msg, BitConverter.GetBytes(authid));
        }

        SendData(msg.ToArray()); 
    }


    // the function for the server to verify authentification

    void VerifyAuth(byte[] msg, PhotonMessageInfo info)
    {
        if (!_imaMaster)
            return;
        // [0] Verify if auth id is mentionned or not by defining data length
        // pu key is 532 bytes . sign is 512 bytes. 
        if (msg.Length < 516)
            return ;

        byte[] auth_msg = new byte[516];
        for (int i = 0; i < 516; i++)
            auth_msg[i] = msg[i + 1];

        int auth_id = -1;
        List<byte> data = new List<byte>();

        if ( msg.Length == 1048) 
        {
            // new account needed 
            byte[] puk = new byte[532];
            for (int i = 517; i < msg.Length; i++)
                puk[i - 517] = msg[i];

            if ( !VerifyAuthSignature(auth_msg, puk))
            {
                return;
            }
            // register the account 
            if (!Directory.Exists(Application.persistentDataPath + separator_token + "acc"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + separator_token + "acc");
            }
            // it will be equal to number 
            string[] pukpath = Directory.GetFiles(Application.persistentDataPath + separator_token + "acc", "*.k", SearchOption.AllDirectories);
             auth_id = pukpath.Length;
            File.WriteAllBytes(Application.persistentDataPath + separator_token + "acc" + separator_token + auth_id.ToString() + ".k", puk);

            // send back auth id to the guy. ( byte flag 9 )
            data = new List<byte>();
            data.Add(9);
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(auth_id));
            GetComponent<PhotonView>().RPC("ReceiveData", info.Sender, data.ToArray());

        }
        else if (msg.Length == 521)
        {
            byte[] puk = null;
             auth_id = BitConverter.ToInt32(msg, 517);
            if ( !File.Exists(Application.persistentDataPath + separator_token + "acc" + separator_token + auth_id.ToString() + ".k"))
            {
                return;
            }
            else
            {
                puk = File.ReadAllBytes(Application.persistentDataPath + separator_token + "acc" + separator_token + auth_id.ToString() + ".k");
            }
            if ( puk.Length != 532 || puk == null)
            {
                return;
            }
            if (!VerifyAuthSignature(auth_msg, puk))
            {
                return;
            }
        }
        else
        {
            return;
        }

        data = new List<byte>();
        // >>>>>>>>>>>>>>> Send to this specific client his auth _ id 
        GameObject mask = Instantiate(Resources.Load(auth_id.ToString(), typeof(GameObject))) as GameObject;
        if (!mask)
            return;

        // send to all people

        data.Add(8);
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(auth_id));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(info.Sender.UserId.Length));
        foreach ( char c in info.Sender.UserId.ToCharArray())
        {
            data.Add((byte) c);
        }

        // we are the server we 
        SendData(data.ToArray());
    }

    void UpdateCheck()
    {

        // request last update
        // ->header (1 o ) 
        //->the last timestamp the client has( 4 o)
        uint lastts = 0;
        if (!Directory.Exists(Application.persistentDataPath + separator_token + "save"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + separator_token + "save");
        }
        DirectoryInfo df = new DirectoryInfo(Application.persistentDataPath + separator_token + "save");
        FileInfo[] files = df.GetFiles().OrderBy(p => p.CreationTimeUtc).ToArray(); // during this time also sort file by names and process update 
        // update my world first 

        foreach (FileInfo f in files)
        {
            string s = f.FullName;
            UpdateWorld(s); // :-)

            uint cts = BitConverter.ToUInt32(IOUtilities.ReadBytesInFiles(s, 0, 4), 0);
            if (cts > lastts)
            {
                lastts = cts;
            }
        }
        // send the message // header is 4 
        List<byte> msg = new List<byte>();
        msg.Add(4);
        BinaryUtilities.AddBytesToList(ref msg, BitConverter.GetBytes(lastts));
        SendData(msg.ToArray());
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


    // SYNC DATA
    public void UpdatePositionReceived(byte[] msg)
    {
        byte namesize = msg[1];

        char[] goName = new char[namesize];
        for (int i = 2; i < 2 + namesize; i++)
        {
            goName[i - 2] = (char)msg[i];
        }
        Debug.Log(new string(goName));
        GameObject vObj = GameObject.Find(new string(goName));
        if (vObj == null) return;
        uint byteOffset = (uint)2 + namesize;
        float posX = BitConverter.ToSingle(msg, (int)byteOffset);
        float posY = BitConverter.ToSingle(msg, (int)byteOffset + 4);
        float posZ = BitConverter.ToSingle(msg, (int)byteOffset + 8);
        float rotX = BitConverter.ToSingle(msg, (int)byteOffset + 12);
        float rotY = BitConverter.ToSingle(msg, (int)byteOffset + 16);
        float rotZ = BitConverter.ToSingle(msg, (int)byteOffset + 20);
        vObj.transform.position = new Vector3(posX, posY, posZ);
        vObj.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);
    }
    public void InstantiateMesh(byte[] msg)
    {
        
        byte _primitive = msg[1];
        char[] name = new char[64];
        for (int i = 0; i < 64; i++)
        {
            name[i] = (char)msg[i+2];
        }
        if (_primitive > 0 )
        {
            // is instantiate stuff.
            GameObject inst = null;
            switch ( _primitive)
            {
                case 1:
                    inst = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
            }
            if (inst == null)
                return;
            BinaryUtilities.DeSerializeTransformOnObject(ref msg, 66, inst);
            inst.AddComponent<ModifMesh>();
            inst.GetComponent<ModifMesh>().ForceStart();
            inst.tag = "3DMESH";
            inst.name = new string(name);
            return;
        }
        else
        {
            byte[] chksm = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                chksm[i] = msg[i + 66];
            }
            string sumstr = CryptoUtilities.SHAToHex(chksm, false);
            // do i have it in my obj\\ directory ?
            string objPath = "obj\\" + sumstr;
            if ( File.Exists(objPath) )
            {
                
                GameObject inst = new OBJLoader().Load(objPath);
                if (inst == null)
                {
                    Debug.Log("fail to import OBJ");
                    return;
                }
                BinaryUtilities.DeSerializeTransformOnObject(ref msg, 98, inst);
                inst.AddComponent<ModifMesh>();
                inst.GetComponent<ModifMesh>().ForceStart();
                inst.tag = "3DMESH";
                inst.name = new string(name);
                return;
            }
            else
            {
                // ASK to server a New Upload...
            }
        }
     
    }
    public void UpdateVerticesReceived(byte[] msg)
    {
     
        byte namesize = msg[1];

        char[] goName = new char[namesize];
        for (int i = 2; i < 2 + namesize; i++)
        {
            goName[i - 2] = (char)msg[i];
        }
        Debug.Log(new string(goName));
        GameObject vObj = GameObject.Find(new string(goName));
        if (vObj == null) return;

        uint byteOffset = (uint)2 + namesize;
        while (byteOffset < msg.Length)
        {
            uint index = BitConverter.ToUInt32(msg, (int)byteOffset);
            float posX = BitConverter.ToSingle(msg, (int)byteOffset + 4);
            float posY = BitConverter.ToSingle(msg, (int)byteOffset + 8);
            float posZ = BitConverter.ToSingle(msg, (int)byteOffset + 12);
            // it crash here cayse vertices are not done properly ( so i should wait vertices are done then .. ) 
            vObj.GetComponent<ModifMesh>().vertices[index] = new Vector3(posX, posY, posZ);


            byteOffset += 16;

        }
        vObj.GetComponent<ModifMesh>().mesh.vertices = vObj.GetComponent<ModifMesh>().vertices;
        vObj.GetComponent<ModifMesh>().mesh.RecalculateBounds();

    }

    public void UpdateMeshCut(byte[] msg)
    {
        byte namesize = msg[1];

        char[] goName = new char[namesize];
        for (int i = 2; i < 2 + namesize; i++)
        {
            goName[i - 2] = (char)msg[i];
        }
        Debug.Log(new string(goName)); // ok parfait 
        GameObject vObj = GameObject.Find(new string(goName));
        if (vObj == null) return;
        uint byteOffset = (uint)2 + namesize;
        float posX = BitConverter.ToSingle(msg, (int)byteOffset);
        float posY = BitConverter.ToSingle(msg, (int)byteOffset + 4);
        float posZ = BitConverter.ToSingle(msg, (int)byteOffset + 8);
        float riX = BitConverter.ToSingle(msg, (int)byteOffset + 12);
        float riY = BitConverter.ToSingle(msg, (int)byteOffset + 16);
        float riZ = BitConverter.ToSingle(msg, (int)byteOffset + 20);
        byteOffset += 24;
        // get the name of the pieces 2 
        char[] name = new char[64];
        for (int i = 0; i < 64; i++)
        {
            name[i] = (char)msg[byteOffset];
            byteOffset++;
        }

        string ancient_name = vObj.name;
        GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(vObj, new Vector3(posX, posY, posZ), new Vector3(riX, riY, riZ), null); // material null here. hope it will not get error
        if (!pieces[1].GetComponent<Rigidbody>())
        {
            // will send 2 * 64 bytes ( name of the 2 pieces ) 
            pieces[0].tag = "3DMESH";
            if (!pieces[0].GetComponent<ModifMesh>())
            {
                pieces[0].AddComponent<ModifMesh>();
                pieces[0].GetComponent<ModifMesh>().ForceStart();
            }

            pieces[0].name = ancient_name;


            Mesh mesh = pieces[1].GetComponent<MeshFilter>().mesh;
            MeshCollider mc = pieces[1].AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
            mc.convex = true;

            pieces[1].AddComponent<Rigidbody>();
            // we should add to pieces 1 a 3d meshes tag and modif mesh component ????
            pieces[1].tag = "3DMESH";
            pieces[1].AddComponent<ModifMesh>();
            pieces[1].GetComponent<ModifMesh>().ForceStart();
            pieces[1].name = new string(name);
        }
        else
        {

        }

    }

    public static List<byte[]> unprocessed_msg = new List<byte[]>();

    public void ProcessUpdateDemand(byte[] msg, PhotonMessageInfo info)
    {
       
        // only for the server :) 
        if (!_imaMaster)
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
            uint cts = BitConverter.ToUInt32(IOUtilities.ReadBytesInFiles(s, 0, 4), 0);
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
            uint cts = Utilities.GetTimeStamp();
            // we load and rewrite every byte roughly to ram ... some sytem of regulation will be needed in receivedata to avoid ram overflow
            BinaryUtilities.AddBytesToList(ref blockdata, BitConverter.GetBytes(cts));
            foreach (byte[] dts in unprocessed_msg)
            {
                // add the length of msg bytes ??? // should never going more than 256 ??? i guess ????
                blockdata.Add((byte)dts.Length); // should never go more than 255 length ... 
                BinaryUtilities.AddBytesToList(ref blockdata, dts);
            }
            string npath = Application.persistentDataPath + separator_token + "save" + separator_token + cts.ToString();
            File.WriteAllBytes(npath, blockdata.ToArray());
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
                BinaryUtilities.AddBytesToList(ref lcformat, IOUtilities.ReadBytesInFiles(s, 0, 4));  // timestamp 
                BinaryUtilities.AddBytesToList(ref lcformat, BitConverter.GetBytes(filecounter)); // files offset list
                BinaryUtilities.AddBytesToList(ref lcformat, BitConverter.GetBytes((uint)fNeeded.Count)); // files length
                BinaryUtilities.AddBytesToList(ref lcformat, BitConverter.GetBytes(byteOffset)); // offset 8 o
                BinaryUtilities.AddBytesToList(ref lcformat, BitConverter.GetBytes(flength));  //  length  8 o
                if (byteOffset + 64000 < flength)
                {
                    BinaryUtilities.AddBytesToList(ref lcformat, IOUtilities.ReadBytesInFiles(s, byteOffset, 64000));
                }
                else
                {
                    long b2r = flength - byteOffset;
                    BinaryUtilities.AddBytesToList(ref lcformat, IOUtilities.ReadBytesInFiles(s, byteOffset, (int)b2r));
                    needbreak = true;
                }
                // send back the packed data to the client
                GetComponent<PhotonView>().RPC("ReceiveData", info.Sender, lcformat.ToArray()); // its probably an ECHO ok need to check ...
                Debug.Log("[update progress] Sending file " + filecounter + "/" + fNeeded.Count + " : " + byteOffset + "/" + flength + " bytes ...");
                if (needbreak) { break; }
                byteOffset += 64000;
            }


            filecounter++;
        }
        Debug.Log("Update send... ");
    }

    public static List<string> Unprocessed_Savefiles = new List<string>();
    public void UpdateWorldSave(byte[] msg)
    {
     
        // download and construct block save file 
        if (_imaMaster) // si je suis le serveur retourner
            return;

        string savepath = Application.persistentDataPath + separator_token + "save";
        // will i received it in order ???
        int byteOffset = 1; // jump the header
        uint ts = BitConverter.ToUInt32(msg, byteOffset); byteOffset += 4;
        uint fc = BitConverter.ToUInt32(msg, byteOffset); byteOffset += 4;
        uint fl = BitConverter.ToUInt32(msg, byteOffset); byteOffset += 4;
        long pc = BitConverter.ToInt64(msg, byteOffset); byteOffset += 8;
        long pl = BitConverter.ToInt64(msg, byteOffset); byteOffset += 8;
        byte[] blockdata = new byte[msg.Length - byteOffset];
        for (int i = 0; i < blockdata.Length; i++)
        {
            blockdata[i] = msg[byteOffset];
            byteOffset++;
        }
        string nfpath = savepath + separator_token + ts.ToString();
        if (pc == 0)
        {
            // create a file named ts with ts as first bytes

            // File.WriteAllBytes(nfpath, BitConverter.GetBytes(ts)); we dont need to write ts cause its already send in first packed data ... 
            File.WriteAllBytes(nfpath, new byte[0]);
            Unprocessed_Savefiles.Add(nfpath);
            // add this file to unprocessed save file
        }
        // i need to find a way to to reconstruct the file if its not received in order (by splitting i guess) 
        // il should also need to know if the checksum of file is correct to know it is right order
        Utils.AppendBytesToFile(nfpath, blockdata);

        if (pc + blockdata.Length >= pl && fc == fl - 1) // fl - 1 ???
        {
       
            foreach (string s in Unprocessed_Savefiles)
            {
                UpdateWorld(s);
            }
            Unprocessed_Savefiles = new List<string>(); // empty the list 
        }
    }
    public void UpdateWorld(string filePath)
    {
        // jump first 4  bytes cause its timestamp 
        // then iterate through header length bytes of the message and compute it with receive data 
        long byteOffset = 4;  // jump ts 
        long fileLength = new FileInfo(filePath).Length;
        while (byteOffset < fileLength)
        {
            byte messagelength = IOUtilities.ReadBytesInFiles(filePath, byteOffset, 1)[0];
            byte[] msg = IOUtilities.ReadBytesInFiles(filePath, byteOffset + 1, messagelength);
            ReceiveData(msg, new PhotonMessageInfo()); // seems ok to me ???
            byteOffset += messagelength + 1; // adding the byte to byte Offset
   
        }

    }
   

    public static void  Metamorph ( byte[] data)
    {
        // data.Add(8);
       

        int auth_id = BitConverter.ToInt32(data, 1);
        int uidl = BitConverter.ToInt32(data, 5);
        char[] UserIdn = new char[uidl];

        for (int i = 9;  i< 9+uidl ; i++  ) 
        {
            UserIdn[i - 9] = (char)data[i];
        }

        string useridstr = new string(UserIdn);
        // now get all avatar. get all photonview. Then Permute mask or add Mask.

        GameObject[] allavs = GameObject.FindGameObjectsWithTag("Avatar");
        foreach ( GameObject go in allavs)
        {
            if (go.GetComponent<PhotonView>())
            {
               // go.GetComponent<PhotonView>()
            }
        }

    }

    [PunRPC]
    public void ReceiveData(byte[] data, PhotonMessageInfo info)
    {
        // archive will be on the xr for the moment
        // archive stuff maj etc. 
        // only if it is the server 

        
        // get the header 
        if (data.Length == 0)
            return;
        byte header = data[0];
        Debug.Log("[data received] s:" + data.Length + " f:"+(int)header);
        // header 0 is call from clien tog get update 

        // dont register dl demand and so on 
        if (_imaMaster && header != 4  
            && header != 7 && header !=  8 && header != 9) // need better way to filter the thing
        {
            unprocessed_msg.Add(data);
            Debug.Log("adding unproc_msg #" + unprocessed_msg.Count);
        }

        switch (header)
        {
            case 1: UpdateVerticesReceived(data); Debug.Log("updating vertices"); break;
            case 2: UpdatePositionReceived(data); Debug.Log("updating position"); break;
            case 3: InstantiateMesh(data); Debug.Log("Instantiate"); break;
            case 4: ProcessUpdateDemand(data, info); break;
            case 5: UpdateMeshCut(data); Debug.Log("cutting mesh"); break; // cut meshes
            case 6: UpdateWorldSave(data); break;

            case 7: VerifyAuth(data, info); break; // verify authentification ( server only ) 
            case 8: break; // Metamorph function
            case 9: break; // Update ini file ( get AUTH ID ) 
            case 10: PlaySoundBox(data); break;

             
            case 12: P2SHARE.OnFileRequest(data, info); break;
            case 13: P2SHARE.OnSeederJoin(data, info); break;
            case 14: P2SHARE.OnUploadDirective(data, info); break;
            case 15: P2SHARE.OnDLLError(data, info); break;
            case 16: P2SHARE.OnDataReceived(data, info); break;
            case 17: P2SHARE.OnSeederLeave(data, info); break;
            case 18: P2SHARE.OnRCVPing(data, info); break;
        }
    }

    public void PlaySoundBox(byte[] msg)
    {
        uint sound_id = BitConverter.ToUInt32(msg, 1);
        if (GameObject.Find("SND_" + sound_id.ToString()))
        {
            GameObject.Find("SND_" + sound_id.ToString()).GetComponent<SoundHit>().DO_HIT();
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
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096);
        byte[] _privateKey = rsa.ExportCspBlob(true);
        byte[] _publicKey = rsa.ExportCspBlob(false);
        File.WriteAllBytes(Application.persistentDataPath + separator_token + "key.prk", rsa.ExportCspBlob(true));
        File.WriteAllBytes(Application.persistentDataPath + separator_token + "key.puk", rsa.ExportCspBlob(false));
        rsa.Clear();
        Debug.Log("RSA DONE.");

    }

    public static byte[] SignData(byte[] prkey) 
    {
        RSACryptoServiceProvider _MyPrRsa = new RSACryptoServiceProvider();

        try 
        {
            _MyPrRsa.ImportCspBlob(prkey);
            byte[] UnsignedData = BitConverter.GetBytes(Utilities.GetTimeStamp());
            SHA256 sha = SHA256.Create();
            UnsignedData = sha.ComputeHash(UnsignedData);
            byte[] Signature = _MyPrRsa.SignHash(UnsignedData, CryptoConfig.MapNameToOID("SHA256"));
            _MyPrRsa.Clear();
            return Signature;
        }
        catch ( Exception e) 
        {
            _MyPrRsa.Clear();
            return null;
        }


    }

    public static bool VerifyAuthSignature(byte[] data, byte[] pukey)
    {

        // first 4 bytes is number of second since 2020 unix epoch 
        // next 512 bytes is signature of this number of day 
        if (data.Length != 516)
            return false;

        byte[] msg = new byte[4];

        for (int i = 0; i < 4; i++)
            msg[i] = data[i];

        byte[] sign = new byte[512];

        for (int i = 0; i < 512; i++)
            sign[i] = data[i + 4];

        RSACryptoServiceProvider _MyPuRsa = new RSACryptoServiceProvider();
        try
        {
            _MyPuRsa.ImportCspBlob(pukey);
            bool success = _MyPuRsa.VerifyData(msg, CryptoConfig.MapNameToOID("SHA256"), sign);
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
            Debug.Log(e.Message);
            _MyPuRsa.Clear();
            return false;
        }

     
    }



}
*/