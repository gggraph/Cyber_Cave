using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.IO;
using System.Threading;

public class NetStream : MonoBehaviour
{
    /*
     On Aplication EXIT:
    [SERVER]
    create a new block save

    [ALL]
    DELETE UNFINISHED DLL FILE... 

    [AJD] 
        UPLOAD 3D MODEL
        UPLOAD D UNE TEXTURE
        UPLOAD D UNEN S
     
     */
    /*
     * 
     *            TODO : 
    L'utilisateur pourra : 
	* Importer des sons, textures et des Meshs     . 
	* Modifier les meshs, le transform de l'object . 
	* Modifier la texture et le shader de l'objet  .
	* Paindre sur l'objet (modifier la texture pixel par pixel) 
	* Placer des meshs primitifs. ou des meshs customisés.
	* Placer des effets de lumières. 
	* Placer des Sound Collider. 
	* Placer des objets de projections de textures. 
	* Attacher des meshs ensemble.
	* 
        Message By flags: 
        [000] NULL message
        [001] Mesh Vertices update
        [002] Mesh Transform update
        [003] Mesh Instantiation
        
        [004] World Update request [master only]
        [005] On save-blocks list received
        [006] Ping Master
        [007] On Master Ping Back
        
        [008] AskInstantiatePermission MSG[to master] 
        [009] OnInstantiatePermission   
        
        [010] Play sound object (NOT IMPLEMENNTED YET)
        [011] NULL message
        
        [012] File Demand         
        [013] Upload Candidate Ping
        [014] Upload directive 
        [015] DLL ERROR 
        [016] FILE DATA
        [017] ULL ERROR
        [018] FILE RCV PING
        
        [020] Set texture message.
        [021] Apply Point To Line Renderer
         

       [030] CREATE BASIC SOUNDBOX
            transform
            event Type       1o 
            primitive Type   1o  
            hash name        64o 
            checksum of file 32o (depend) 
            file length      4o  (depend) 

     */

    public class DefaultRPCMessage
    {
        public PhotonMessageInfo MessageInfo { get; }
        public byte[] Data { get; }

        public DefaultRPCMessage(byte[] data, PhotonMessageInfo msgInfo)
        {
            MessageInfo = msgInfo;
            Data = data;
        }
    }
    public static byte[] masterPuKey = new byte[]
    {
        6,2,0,0,0,164,0,0,82,83,65,49,0,16,0,0,1,0,1,0,181,177,
        130,249,56,235,104,57,3,209,246,140,227,196,173,197,25,
        105,213,95,65,188,36,167,132,164,218,1,244,135,227,92,
        123,93,169,196,202,69,60,72,212,57,209,252,6,49,48,228,76,
        85,58,135,71,80,39,57,206,110,158,97,55,97,97,180,175,11,243,
        145,67,75,243,223,79,36,246,147,148,201,69,248,159,247,180,178,
        158,15,223,207,114,45,208,54,190,155,91,42,83,139,43,217,237,152,
        41,210,118,1,55,200,147,195,223,60,73,86,168,88,53,17,139,71,206,
        84,250,219,238,87,137,239,198,194,167,56,236,186,93,70,237,0,7,
        149,76,226,94,210,137,120,36,102,211,214,119,203,207,99,43,190,
        67,96,176,87,230,155,216,136,121,226,250,69,245,200,147,141,203,
        152,217,101,172,52,124,77,92,99,90,119,239,194,161,96,66,75,124,
        83,40,49,13,220,212,190,93,146,124,142,218,175,207,245,93,82,150,
        200,238,3,221,206,204,230,228,77,134,50,210,129,8,238,148,208,155,
        106,64,13,177,231,179,212,221,81,100,160,45,243,218,75,51,7,227,212,
        171,133,222,210,36,65,106,220,65,108,106,178,127,112,167,95,1,187,125,
        127,20,255,74,104,22,236,227,147,41,190,107,86,46,147,147,253,36,165,176,
        164,57,189,134,37,146,18,49,40,176,163,74,212,115,156,246,143,57,158,211,251,
        105,195,142,122,195,143,56,199,60,124,177,168,129,96,197,98,213,100,243,203,102,
        43,22,93,212,56,196,238,54,160,31,42,9,44,194,8,164,61,142,143,153,58,190,240,104,
        189,129,216,226,96,150,244,168,5,129,12,47,141,23,70,40,24,138,2,203,123,129,24,190,
        206,100,181,108,73,236,45,68,198,244,32,95,102,151,194,224,52,107,38,134,14,243,26,
        85,221,38,40,36,40,153,135,136,34,65,222,210,104,73,106,211,64,120,57,111,192,136,
        157,198,3,220,136,211,86,40,184,131,89,141,95,172,5,129,140,198,186,148,52,154,56,
        224,133,132,180,167,247,96,182,140,5,4,80,114,130,219,163,47,190,225,28,7,193,116,
        195,101,162,141,117,176,212,32,91,32,175,5,57,138,138,13,38,199,41,169,156,103,187,
        194,183,48,29,144,0,85,22,137,73,11,220,98,164,163,150,151,120,146,151,171,178,

    };
    public static bool _imMaster = false;
   
    void Start()
    {
        if (!GetComponent<PhotonView>().IsMine)
        {
            return;
        }



        NetUtilities._mphotonView = GetComponent<PhotonView>();
        NetUtilities._mNetStream = this;
        IOManager.CheckFileDirectories();

        WorldUpdater.LoadWorld(); // this crash i dont know why 

        if (AmIMaster())
        {
            _imMaster = true;
        }
        else
        {
            PingMaster();
        }

        InvokeRepeating("ProccessP2PFunction", 1f, 0.1f); // maybe could be in a coroutine inside P2SHARE... called with new P2SHARE().etc.
                                                          //NetInstantiator.InstantiateCustomMeshFromMenu();
                                                          //BasicWindows.CreateOkWindow("Test 12 12");


       //GlobalMenu.OpenGlobalMenu();

    }


    public void ProccessP2PFunction()
    {
        PhotonView pw = GetComponent<PhotonView>();
        P2SHARE.ProccessUpload(pw);
        P2SHARE.CheckDLLsSanity();
    }

  
    public  void Test()
    {
        GlobalMenu.OpenGlobalMenu();
        //NetInstantiator.TryInstantiateCustomMesh(Application.persistentDataPath + "/mesh/eafb7985f897099569fbb553bff6df578ffa305f471a7e1c39f92e55497e2252",  
        //    new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0.1f, 0.1f, 0.1f));
        //  Debug.Log("Instantiating texture TEST...");
        // GameObject dest = GameObject.Find("TESTYCUBE");
        // not working with bitmap...
        // NetInstantiator.TryInstantiateCustomTexture(Application.persistentDataPath + "/texture/a5ca8bfae08d92a1b95e897bb3216891117e7a82c6d557e3571c2997941c35f5", dest);

        //0c73abea3a0e955107c825072727d4f8c492aa9afab11e70fb7387aa25605742
        //NetInstantiator.TryInstantiateSoundBox(Application.persistentDataPath + "/sound/c3441e667a86486de156ea1db531ac50fe66d07d8da04fab1a75fff105dc769f",
        //new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0.1f, 0.1f, 0.1f));
    }



    bool AmIMaster()
    {
        if (File.Exists(Application.persistentDataPath+"/mstrk"))
        {
            byte[] prkey = File.ReadAllBytes(Application.persistentDataPath + "/mstrk");
            // sign thrash 
            byte[] thrash = BitConverter.GetBytes(Utilities.GetTimeStamp());

            byte[] sign = CryptoUtilities.SignData(prkey, thrash);
            if (CryptoUtilities.VerifySign(thrash, masterPuKey, sign))
            {
                Debug.Log("I am master node. So am I?");
                return true;
            }
            else
                return false;
        }

        return false;
    }

    void PingMaster()
    {
        Debug.Log("try to contact master...");
        List<byte> data = new List<byte>();
        data.Add(6);
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(Utilities.GetTimeStamp()));
        NetUtilities.SendDataToAll(data.ToArray());
        // can also specify own node public key
    }

    void OnMasterPingReceived(byte[] data, PhotonMessageInfo info)
    {
        if (!_imMaster)
            return;

        byte[] prkey = File.ReadAllBytes(Application.persistentDataPath + "/mstrk");
        byte[] signature = CryptoUtilities.SignData(prkey, data);
        List<byte> dt = new List<byte>();
        dt.Add(7);
        BinaryUtilities.AddBytesToList(ref dt, data);
        BinaryUtilities.AddBytesToList(ref dt, signature);
        NetUtilities.SendDataToSpecific(dt.ToArray(), info.Sender);

    }

    void OnMasterManifest(byte[] data, PhotonMessageInfo info)
    {
        // verify signature
        byte[] msg = new byte[5];
        for (int i = 0; i < 5; i ++)
        {
            msg[i] = data[i + 1]; 
        }
        byte[] sign = new byte[512];
        for (int i = 0; i < 512; i ++)
        {
            sign[i] = data[i + 6];
        }

        if ( CryptoUtilities.VerifySign(msg, masterPuKey, sign) == true)
        {
            Debug.Log("Master node informations found.");
            NetUtilities.master = info.Sender;
            // Ask world save
            WorldUpdater.RequestWorldUpdate();
        }
        else
        {
            Debug.Log("Master node wrong signature.");
        }
        Debug.Log("master manifest...");

    }
    // a basic IENUMERATOR TO CHECK IF ALL FILES ARE DONE
    public IEnumerator DoMAJCheck(string[] files) 
    {
        while (true)
        {
            bool _done = true;
            foreach ( string s in files)
            {
                if ( P2SHARE.GetDLByFileName(s) != null)
                {
                    _done = false;

                }
            }
            if ( _done)
            {
                WorldUpdater.LoadBlockFiles(files);
                Debug.Log("[MAJ DONE!]");
                yield break;
            }
            yield return new WaitForSeconds(2f);
        }
    }



    [PunRPC]
    public void ReceiveData(byte[] data, PhotonMessageInfo info)
    {
        
        if (data.Length == 0)
            return;
        byte header = data[0];
        Debug.Log("[data received] s:" + data.Length + " f:" + (int)header);
       
        if ( _imMaster && WorldUpdater.RegFlags[header] == 1)
        {
            WorldUpdater._unregistered_msg.Add(data);
        }

        switch (header)
        {
            case 1: MeshSync.UpdateVerticesReceived(data); Debug.Log("updating vertices"); break;
            case 2: MeshSync.UpdateTransformReceived(data); Debug.Log("updating position"); break;
            case 3: NetInstantiator.InstantiateMesh(data); Debug.Log("Instantiate"); break;

            case 4: WorldUpdater.OnWorldUpdateRequest(data, info); break;
            case 5: WorldUpdater.OnBlocksListReceived(data, info); break;
            case 6: OnMasterPingReceived(data, info); break;
            case 7: OnMasterManifest(data, info); break;
            case 8: NetInstantiator.GiveInstantiatePermission(data, info); break;
            case 9: NetInstantiator.OnInstantiatePermissionAccepted(data); break;

            case 10:  break; // Play sound here 


            case 12: P2SHARE.OnFileRequest(data, info); break;
            case 13: P2SHARE.OnSeederJoin(data, info); break;
            case 14: P2SHARE.OnUploadDirective(data, info); break;
            case 15: P2SHARE.OnDLLError(data, info); break;
            case 16: P2SHARE.OnDataReceived(data, info); break;
            case 17: P2SHARE.OnSeederLeave(data, info); break;
            case 18: P2SHARE.OnRCVPing(data, info); break;

            case 20: NetInstantiator.InstantiateTexture(data); break;
            case 21: MeshSync.AddLine(data);break;

            case 30: NetInstantiator.InstantiateSoundBox(data); break;
        }
    }


}
