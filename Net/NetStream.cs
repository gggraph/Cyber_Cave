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
    // Master node related data
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

    PhotonView photonview;
    
    void Start()
    {
        
        photonview = GetComponent<PhotonView>();
        // Check if photonview is mine
        if (!photonview.IsMine)
            return;

        GameStatus.SetGameFlag(5);
        // Load netutilities variable
        NetUtilities._mphotonView = GetComponent<PhotonView>();
        NetUtilities._mNetStream = this;

        // Check if i am master node. Else Ping it
        if (AmIMaster())
        {
            _imMaster = true;
            Debug.Log("I am master");
            GameStatus.SetGameFlag(10);
            GameStatus.SetGameFlag(11);
        }
        

        // Do some slow check of p2p mechanism. More its fast more we can dll fast...
        StartCoroutine(ProccessP2P(0.1f));

    }

    IEnumerator ProccessP2P(float repeatRate)
    {
        while ( true)
        {
            if (repeatRate == 0)
                yield return new WaitForEndOfFrame();
            else
                yield return new WaitForSeconds(repeatRate);

            P2SHARE.ProccessUpload(photonview);
            P2SHARE.CheckDLLsSanity();
        }
    }


    bool AmIMaster()
    {
        if (File.Exists(Application.persistentDataPath + "/mstrk"))
        {
            byte[] prkey = File.ReadAllBytes(Application.persistentDataPath + "/mstrk");
            // sign thrash 
            byte[] thrash = BitConverter.GetBytes(Utilities.GetTimeStamp());

            byte[] sign = CryptoUtilities.SignData(prkey, thrash);
            if (CryptoUtilities.VerifySign(thrash, masterPuKey, sign))
            {
                return true;
            }
            else
                return false;
        }

        return false;
    }


    // Miscellaneous RPC at avatar connection
    public void PingMaster()
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
        if (NetUtilities.master != null)
            return;
        Debug.Log("Will verify signature of master....");
        // verify signature
        byte[] msg = new byte[5];
        for (int i = 0; i < 5; i++)
        {
            msg[i] = data[i + 1];
        }
        byte[] sign = new byte[512];
        for (int i = 0; i < 512; i++)
        {
            sign[i] = data[i + 6];
        }

        if (CryptoUtilities.VerifySign(msg, masterPuKey, sign) == true)
        {
            Debug.Log("Master node informations found.");
            NetUtilities.master = info.Sender;
            GameStatus.SetGameFlag(10);
            // Request misc at connection
            if (!GameStatus.IsGameFlagSet(12))
                NetUtilities.SendDataToMaster(new byte[1] { 4 });
        }
        else
        {
            Debug.LogError("Master node wrong signature.");
        }
        Debug.LogError("master manifest...");

    }
   

    [PunRPC]
    public void ReceiveData(byte[] data, PhotonMessageInfo info)
    {

        if (data.Length == 0)
            return;
        byte header = data[0];
        Debug.Log("[data received] s:" + data.Length + " f:" + (int)header);

        switch (header)
        {
            case 1: MeshSync.UpdateVerticesReceived(data); Debug.Log("updating vertices"); break;
            case 2: MeshSync.UpdateTransformReceived(data); Debug.Log("updating position"); break;
            case 3: NetInstantiator.InstantiateMesh(data); Debug.Log("Instantiate"); break;

            case 4: SceneUpdater.OnRequestInfoAtConnection(data, info); break;
            case 5: SceneUpdater.CheckDLLOperationOnMasterSaveFile(data); break;

            case 6: OnMasterPingReceived(data, info); break;
            case 7: OnMasterManifest(data, info); break;
            case 8: NetInstantiator.GiveInstantiatePermission(data, info); break;
            case 9: NetInstantiator.OnInstantiatePermissionAccepted(data); break;

            case 10: break; // Play sound here 


            case 12: P2SHARE.OnFileRequest(data, info); break;
            case 13: P2SHARE.OnSeederJoin(data, info); break;
            case 14: P2SHARE.OnUploadDirective(data, info); break;
            case 15: P2SHARE.OnDLLError(data, info); break;
            case 16: P2SHARE.OnDataReceived(data, info); break;
            case 17: P2SHARE.OnSeederLeave(data, info); break;
            case 18: P2SHARE.OnRCVPing(data, info); break;

            case 20: NetInstantiator.InstantiateTexture(data); break;
            case 21: Paintable_OLD.ProccessLine(data); break; // <- UNUSED ANYMORE

            case 25: Mooduler.OnMoodulationReceived(data); break;
            case 26: MoodulerCreator.OnMoodulableCreated(data); break;
            case 27: Moodulable.OnMoodulableMergingReceived(data); break;

            case 30: NetInstantiator.InstantiateSoundBox(data); break;

            case 40: Character.RefreshControllerTrackingStatus(data, info); break;

            case 50: Grabbable.OnGrabbingReceived(data, info); break;
            case 51: Grabbable.OnReleasingReceived(data, info); break;

            case 60: AvatarSelection.Select(data, info); break;
            case 70: Painter.OnPaintingCommandReceived(data); break;
            case 71: CanvasCreator.OnCanvasCreationReceived(data); break;

            case 75: SculptRotater.OnMoodRotaterDataReceived(data); break;

            case 80: Plotting.On3DrintingCommandReceived(data); break; 
            case 81: Plotting.OnXYPrintingCommandReceived(data); break;

            case 90: OnMenuAnimation.OnMenuIconReceived(data, info); break;
        }
    }


}
