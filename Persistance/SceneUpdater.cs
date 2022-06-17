using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Photon.Pun;

/*
        How scene loading and updating from master node works?
    [0] At start, user load its own save file. 
    [1] Client will also ask master node save file.
    [2] If client achieves to download master savefile. Client will then compare its new save file from oldest savefile.
    [3] Client will probably need to check if there is new moodulable, moodulable, hash change or pantable hash change. If so
    he will request to download specific texturesfile. When all texturesfile are downloaded, he can reload paintable and moodulable...
 
 */
public class SceneUpdater : MonoBehaviour
{
    
    public class SaveEntry
    {
        public string objectName;
        public GameObject gameObject;
        public byte[] checksum;
        public bool isPaintable;
        public bool isMoodulable;
        public string texturePath;
        public string sculptPath;
        public int    textureSize;
        public int    sculptSize;
        // raw data
        public byte[] transformData;

        // some Helper value for merging
        public int byteOffset;
        public int byteLength;

        // null constructor
        public SaveEntry() { }

        public void Print()
        {
            string s = objectName;
            s = isPaintable == true ? s + " is Paintable " : s;
            s = isMoodulable == true ? s + " is Moodulable " : s;
            Debug.Log(s);

        }
        public bool Proccess()
        {
            // @ process gameObject
            if (!gameObject)
            {
                // Try to find the gameObject. If null & moodulable, its normal : create it.
                gameObject = GameObject.Find(objectName);
                // Not elegant but double check is needed
                if (!gameObject)
                {
                    if (isMoodulable || isPaintable)
                        gameObject = new GameObject(objectName);
                    else
                        return false;
                }
                gameObject.name = objectName;
                    
            }
            // @ Deserialize transform
            BinaryUtilities.DeSerializeTransformOnObject(ref transformData, 0, gameObject);

            //@ Apply Paintable
            if ( isPaintable)
            {
                Paintable pt = gameObject.GetComponent<Paintable>();
                if (!pt) 
                {
                   CanvasCreator cc = FindObjectOfType<CanvasCreator>();
                    if (cc == null)
                        return false;
                    if(!cc.LoadCanvasUsingSettings(gameObject, texturePath))
                    {
                        return false;
                    }
                }
                else
                {
                    pt.initialTexturePath = texturePath;
                    pt.Init();
                }
                
            }
            if (isMoodulable)
            {
                Moodulable m = gameObject.GetComponent<Moodulable>();

                if (m)
                    m.Delete(true, false); // Destroy the previous object to avoid Conflict

                // reset scale or it will modify chunk size ...
                gameObject.transform.localScale = new Vector3(1, 1, 1);

                // Find LoadMoodulableUsingSettings
                MoodulerCreator mc = FindObjectOfType<MoodulerCreator>();
                if (mc == null)
                    return false;
                if (!mc.LoadMoodulableUsingSettings(gameObject, sculptPath))
                {
                    return false;
                }
     
            }

            return true;
        }
    }
    public static SaveEntry[] GetEntriesFromSaveData(byte[] data, int byteOffset = 0)
    {
        // return Entries of the current files 
        int entryCount = BitConverter.ToInt32(data, 4 + byteOffset);
        SaveEntry[] result = new SaveEntry[entryCount];

        // Iterate through all entries 
        int bctr = 8 + byteOffset;
        for (int i = 0; i < entryCount; i++)
        {
            result[i] = new SaveEntry();
            result[i].byteOffset = bctr - byteOffset;
            // Get entry object Name
            byte namelen = data[bctr]; bctr++;
            char[] name = new char[namelen];
            for (int n = bctr; n < bctr + namelen; n++)
            {
                name[n - bctr] = (char)data[n];
            }
            result[i].objectName = new string(name);
            bctr += namelen; 
        
            // Get entry checksum
            byte[] checksum = new byte[32]; 
            for (int n = 0; n < 32; n ++)
            {
                checksum[n] = data[bctr + n];
            }
            result[i].checksum = checksum;
            bctr += 32;


            // Read component flags
            byte componentFlag = data[bctr]; bctr++;
            if (BinaryUtilities.IsBitSet(componentFlag, 1))
            {
                result[i].isMoodulable = true;
            }
            if (BinaryUtilities.IsBitSet(componentFlag, 0))
            {
                result[i].isPaintable = true;
            }
            
            // Read entry transform data
            byte[] transformdata = new byte[36];
            for (int n = 0; n < 36; n++)
            {
                transformdata[n] = data[bctr + n];
            }
            result[i].transformData = transformdata;
            bctr += 36;

            if (result[i].isPaintable)
            {
                byte[] texchksum = new byte[32];
                for (int n = 0; n < 32; n++)
                {
                    texchksum[n] = data[bctr + n];
                }
                string texPath = P2SHARE.GetDirByType((byte)P2SHARE.CustomFileType.TextureFile) + CryptoUtilities.SHAToHex(texchksum);
                result[i].texturePath = texPath;
                bctr += 32;
                int fsize = BitConverter.ToInt32(data, bctr); bctr += 4;
                result[i].textureSize = fsize;
            }
            if (result[i].isMoodulable)
            {
                byte[] texchksum = new byte[32];
                for (int n = 0; n < 32; n++)
                {
                    texchksum[n] = data[bctr + n];
                }
                string texPath = P2SHARE.GetDirByType((byte)P2SHARE.CustomFileType.SculptFile) + CryptoUtilities.SHAToHex(texchksum);
                result[i].sculptPath = texPath;
                bctr += 32;
                int fsize = BitConverter.ToInt32(data, bctr); bctr += 4;
                result[i].sculptSize = fsize;
            }
            result[i].byteLength = (bctr- byteOffset) - result[i].byteOffset;
        }
        return result;
    }
    public static bool ProccessSaveFile()
    {
        // @ Called WorldSave
        string savePath = Application.persistentDataPath + "/worldsave";
        if (!File.Exists(savePath))
        {
            Debug.Log("No save file ...");
            return false;
        }
            
        byte[] data = File.ReadAllBytes(savePath);
        SaveEntry[] entries = GetEntriesFromSaveData(data);
        Debug.Log(entries.Length);
        foreach (SaveEntry en in entries)
        {
            if (!en.Proccess())
            {
                Debug.LogError("Failed to load updates...");
                File.Delete(Application.persistentDataPath + "/worldsave");
                return false;
            }
        }
            
         
        return true;
    }

    // -__________________________________----- NET -----_______________________________-----

    public static void OnRequestInfoAtConnection(byte[] data, PhotonMessageInfo info)
    {
        // On node only. Send information like current online avatar texture & pseudo etc? Ingame Time? & savefile
        byte[] savedata = SceneSaver.SaveSceneAsFile();
        List<byte> msg = new List<byte>();
        string savePath = Application.persistentDataPath + "/worldsave";
        msg.Add(5);
        msg.AddRange(savedata);
        NetUtilities.SendDataToSpecific(msg.ToArray(), info.Sender);
    }
    public static void CheckDLLOperationOnMasterSaveFile(byte[] data)
    {
        SaveEntry[] myEntries     = GetEntriesFromSaveData(data, 1);
        SaveEntry[] serverEntries = GetEntriesFromSaveData(data, 1); // +1 cause first byte 
        List<P2SHARE.DL> reqDlls = new List<P2SHARE.DL>();
        foreach ( SaveEntry a in serverEntries)
        {
            // CHeck if paths exists then start download
            if (a.isPaintable)
            {
                if (!File.Exists(a.texturePath))
                {
                    // create checksum string for path 
                    Debug.Log("Will DLL new paint texture Object!");
                    string[] parser = a.texturePath.Split('/');
                    string name = parser[parser.Length - 1];
                    byte[] sha = CryptoUtilities.GetUniqueHashFromViewID(name, NetUtilities._mphotonView); // Create unique hash from User. We can use a specific thing like (stringname+player+time) 
                    // dll missing message info is important. It can help us to retry dll
                    reqDlls.Add(P2SHARE.RequestNewFile(name,sha, a.textureSize, (byte)P2SHARE.CustomFileType.TextureFile, null));
                }
                
            }
            if (a.isMoodulable)
            {
                if (!File.Exists(a.sculptPath))
                {
                    Debug.Log("Will DLL new sculpture Object!");
                    string[] parser = a.sculptPath.Split('/');
                    string name = parser[parser.Length - 1];
                    byte[] sha = CryptoUtilities.GetUniqueHashFromViewID(name, NetUtilities._mphotonView); // Create unique hash from User
                    // dll missing message info is important. It can help us to retry dll
                    reqDlls.Add(P2SHARE.RequestNewFile(name,sha, a.sculptSize, (byte)P2SHARE.CustomFileType.SculptFile, null));
                 
                }
            }
        }
        GameStatus.SetGameFlag(12);
        if (reqDlls.Count > 0) 
        {
            GameObject cor = new GameObject("coroutine for update");
            SceneUpdater su = cor.AddComponent<SceneUpdater>();
            su.StartCoroutine(su.LoadMasterSaveWhenDLLsAreDone(reqDlls, data));
            GameStatus.SetGameFlag(12);
            return;
        }
        byte[] nsavedata = new byte[data.Length - 1];
        for (int i = 1; i < data.Length; i++)
            nsavedata[i - 1] = data[i];
        File.WriteAllBytes(Application.persistentDataPath + "/worldsave", nsavedata);
        // run save 
        ProccessSaveFile();
        // start logic

    }

    IEnumerator LoadMasterSaveWhenDLLsAreDone(List<P2SHARE.DL> dlls, byte[] serverSaveMsg )
    {
        while ( true)
        {
            bool _alld = true;
            foreach ( P2SHARE.DL dl in dlls)
            {
                // if the dl is not null
                if (dl != null)
                {
                    if (!dl._done)
                        _alld = false;
                }

            }
            if (_alld)
            {
                // copy bytes of serverSaveMsg & rplace save
                byte[] data = new byte[serverSaveMsg.Length - 1];
                for (int i = 1; i < serverSaveMsg.Length; i++)
                    data[i - 1] = serverSaveMsg[i];
                File.WriteAllBytes(Application.persistentDataPath + "/worldsave", data);
                // run save 
                ProccessSaveFile();
                Destroy(this.gameObject); // destroy gameObject attach for this coroutinge
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
    }


    public bool ForceSave = false;
    public bool ForceLoad = false;
    public bool testHashReversibility = false;

    private void Update()
    {
        if (ForceSave)
        {
            ForceSave = false;
            SceneSaver.SaveSceneAsFile();
        }
        if (ForceLoad)
        {
            ForceLoad = false;
            SceneUpdater.ProccessSaveFile();
        }
        
    }
}
