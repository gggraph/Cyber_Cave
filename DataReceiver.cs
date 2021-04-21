using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.IO;
using System.Linq;

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
        if (Application.persistentDataPath == @"C:/Users/gaelg/AppData/LocalLow/DefaultCompany/Cyber_Cave")
        {
            _imaServer = true;
        }
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

            uint cts = BitConverter.ToUInt32(ReadBytesInFiles(s, 0, 4), 0);
            if (cts > lastts)
            {
                lastts = cts;
            }
        }
        // send the message // header is 4 
        List<byte> msg = new List<byte>();
        msg.Add(4);
        msg = AddBytesToList(msg, BitConverter.GetBytes(lastts));
        SendData(ListToByteArray(msg));
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
    public void InstantiateCube(byte[] msg)
    {
        uint byteOffset = (uint)1;
        float posX = BitConverter.ToSingle(msg, (int)byteOffset);
        float posY = BitConverter.ToSingle(msg, (int)byteOffset + 4);
        float posZ = BitConverter.ToSingle(msg, (int)byteOffset + 8);
        byteOffset += 12;
        char[] name = new char[64];
        for (int i = 0; i < 64; i++)
        {
            name[i] = (char)msg[byteOffset];
            byteOffset++;
        }

        GameObject inst = GameObject.CreatePrimitive(PrimitiveType.Cube);
        inst.transform.position = new Vector3(posX, posY, posZ);
        inst.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        inst.AddComponent<ModifMesh>();
        inst.GetComponent<ModifMesh>().ForceStart();
        inst.tag = "3DMESH";
        inst.name = new string(name);
    }
    public void UpdateVerticesReceived(byte[] msg)
    {
        /*
          -_-_-_-_-_-_-_-data packet structure-_-_-_-_-_-_
                              header
                              taille du char arr du nom de go
                              nom du gameobject (this)

                              index du vertices
                              pos x
                              pos y 
                              pos z

       */
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
           // GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = "no rigidbody";
        }

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
    public void UpdateWorldSave(byte[] msg)
    {
        /*
         maj data structure : 
           -> byte header (6)  (1 o ) 
         -> timestamp (4o)
         -> files offset counter ( 4o ) 
         -> files length counter ( 4o ) 
         -> long current packeddata offset (8 o) 
         -> long packeddata length (8 o ) 
         -> packeddata chunk  ( ?? o )
        */
        // download and construct block save file 
        if (_imaServer) // si je suis le serveur retourner
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
        AppendBytesToFile(nfpath, blockdata);

        if (pc + blockdata.Length >= pl && fc == fl - 1) // fl - 1 ???
        {
            //GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = "update received !";
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
            byte messagelength = ReadBytesInFiles(filePath, byteOffset, 1)[0];
            byte[] msg = ReadBytesInFiles(filePath, byteOffset + 1, messagelength);
            ReceiveData(msg, new PhotonMessageInfo()); // seems ok to me ???
            byteOffset += messagelength + 1; // adding the byte to byte Offset
            //GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = "updating world !";
        }
        //GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = fileLength.ToString();
    }
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

        switch (header)
        {
            case 1: UpdateVerticesReceived(data); Debug.Log("updating vertices"); break;
            case 2: UpdatePositionReceived(data); Debug.Log("updating position"); break;
            case 3: InstantiateCube(data); Debug.Log("creating cubes"); break;
            case 4: ProcessUpdateDemand(data, info); break;
            case 5: UpdateMeshCut(data); Debug.Log("cutting mesh"); break; // cut meshes
            case 6: UpdateWorldSave(data); break;
        }
    }

    // process data for gameobject 




}
