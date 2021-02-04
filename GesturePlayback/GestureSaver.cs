using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Photon.Pun;
public class GestureSaver : MonoBehaviour
{
    /*
     This class is root on every Avatar ( offline or online ) 
     Recording into files is done only If Avatar has PhotonView. 

     */

    // STREAM
    public List<byte> _streamData; // the stream data in byte
    public long STREAM_MAX_LENGTH = 25000; // (2500 /s ) // so here max stream size is 10 secondes

    // RECORD
    public static long RECFILE_CHUNK = 100000000000000; // Max size of a record file ? < - implement it !  
    Coroutine rec_thrd;



    private void Start()
    {
        VerifyFiles();

        if ( this.GetComponent<PhotonView>() != null) // avatar is online. Start Recording into new file...
        {
            StartRecordingGesture(true);
        
        }
        else 
        {
            StartRecordingGesture(false); // avatar is offline. Just Get the stream...
        }
    }

    public void VerifyFiles()
    {
        // we are 5 . Lionel, Theo, Mathilde, Gael, Florent

        string[] avatar_nickname = new string[5] { "Lionel", "Theo", "Mathilde", "Gael", "Florent" };
        foreach (string s in avatar_nickname)
        {
            if (!Directory.Exists(Application.persistentDataPath + "/" + s))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/" + s);
            }
        }
    }
    public string GetNewRecordFilePath(string directoryPath)
    {
        string[] files = Directory.GetFiles(directoryPath);
        return directoryPath + "/" + files.Length.ToString();
    }
    public void StartRecordingGesture(bool _record)
    {

        rec_thrd = StartCoroutine(StreamGesture(_record));

    }
    public bool StopRecordingGesture()
    {
        // First Generate A File depending on User ID, username
        StopCoroutine(rec_thrd);
        return true;

    }

    public IEnumerator StreamGesture(bool _record) // /!\  __ We need to record offset from body for head and controllers part __ /!\
    {
        GameObject rightHand = this.transform.Find("RightHand").gameObject;
        GameObject leftHand = this.transform.Find("LeftHand").gameObject;
        GameObject head = this.transform.Find("Head").gameObject;
        GameObject Body = this.transform.Find("Center").gameObject;

        string filePath = GetNewRecordFilePath(Application.persistentDataPath + "/" + this.GetComponent<AvatarScript>()._nickname);

        if ( _record) 
        {
            /*-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- Just For testing Purpose. File setup -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- */
           
            File.WriteAllBytes(filePath, new byte[0]); // better than File.Create or File.Delete

            /*-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- Add the Starting Time of the Record -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- */
            uint unixTimestamp = (uint)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            AppendBytesToFile(filePath, BitConverter.GetBytes(unixTimestamp));
            UnityEngine.Debug.Log(BitConverter.GetBytes(unixTimestamp).Length);
        }
      
        Vector3 cPos = new Vector3();
        while (true)
        {
            /*-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- RECORD DATA STRUCTURE -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- */
            /*
             - Body Position. Body Rotation. 
             - Head position. Head Rotation.
             - Controllers position. Controllers Rotation.
            
             */

            // Serialization
            List<byte> rc = new List<byte>();
            for (int i = 0; i < 4; i++)
            {
                GameObject o = null;
                switch (i)
                {
                    case 0: o = Body; break;
                    case 1: o = head; break;
                    case 2: o = rightHand; break;
                    case 3: o = leftHand; break;

                }
                if (o == null) { UnityEngine.Debug.Log("object was null"); yield break; }// Print an error
                // we can check how many records we can do before breaking File Max Length . But dont play with this for the moment. 

                // all are float32 (single precision)

                // for position : 
                // should be offsets and not raw Vector Value if not body (i>0)
                if (i == 0 && _record)
                {
                    cPos = o.transform.position;
                    rc = AddBytesToList(rc, BitConverter.GetBytes(o.transform.position.x));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(o.transform.position.y));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(o.transform.position.z));
                    Vector3 rot = o.transform.localEulerAngles;
                    rc = AddBytesToList(rc, BitConverter.GetBytes(rot.x));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(rot.y));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(rot.z));
                    //  UnityEngine.Debug.Log(rc.Count + o.transform.position.ToString());
                }
                if ( i > 0 )
                {

                    Vector3 offset = cPos - o.transform.position; // carefull here for the order
                    rc = AddBytesToList(rc, BitConverter.GetBytes(offset.x));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(offset.y));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(offset.z));
                    Vector3 rot = o.transform.localEulerAngles;
                    rc = AddBytesToList(rc, BitConverter.GetBytes(rot.x));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(rot.y));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(rot.z));

                    // update also the stream
                    _streamData = AddBytesToList(_streamData, BitConverter.GetBytes(offset.x));
                    _streamData = AddBytesToList(_streamData, BitConverter.GetBytes(offset.y));
                    _streamData = AddBytesToList(_streamData, BitConverter.GetBytes(offset.z));

                    _streamData = AddBytesToList(_streamData, BitConverter.GetBytes(rot.x));
                    _streamData = AddBytesToList(_streamData, BitConverter.GetBytes(rot.y));
                    _streamData = AddBytesToList(_streamData, BitConverter.GetBytes(rot.z));
                }


            }
            if ( _record) 
            {
                AppendBytesToFile(filePath, ListToByteArray(rc));
            }

            // Flush the stream;
            FlushStream(); 

            yield return new WaitForSeconds(0.04f);

            // break;
            //Thread.Sleep(40);// should be the same as Playback
            // some additional info:
            // generate 2500 bytes per second (96*25) (so like 0.0025 mb)
            // so 1000 second will weight 2.5mb ( 16 minutes average )2500000
        }
        yield return 0;

    }

    public bool FlushStream()
    {
        if (_streamData.Count > STREAM_MAX_LENGTH)
        {
            // remove first second ( first 2500 bytes )  
            for (int i = 0; i < 2500; i++)
            {
                _streamData.RemoveAt(0); // probably inneficient code here
            }
        }

        return true;
    }

    public bool _playingBack;

    // -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- PLAYBACK FUNCTION  -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- \\

    public void StartPlayBack(string filePath, bool _updateRawPosition, int msOffset, int frameLength)
    {
        _playingBack = true;

        StartCoroutine(PlayBack(filePath, _updateRawPosition, msOffset, frameLength));

    }

    public IEnumerator PlayBack(string filePath, bool _updateRawPosition, int msOffset, int frameLenght)
    {
        GameObject rightHand = this.transform.Find("RightHand").gameObject;
        GameObject leftHand = this.transform.Find("LeftHand").gameObject;
        GameObject head = this.transform.Find("Head").gameObject;
        GameObject Body = this.transform.Find("Center").gameObject;

        // first 4 bytes of the file are time stamp data so we will start recording at 4
        long byteOffset = 4;
        long fileLength = new FileInfo(filePath).Length;
        Vector3 cPos = new Vector3();
        // float in Unity are 4 bytes

        // Skip at msOffset
        byteOffset += 96 * msOffset;

        int frameCounter = 0;
        byte[] RAWDATA = File.ReadAllBytes(filePath);

        GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = fileLength.ToString();
        while (byteOffset < fileLength)
        {
            // get the next 24 bytes for offset
            // 24 bytes foreach. 96 bytes for all 4 objects
            //byte[] data = GetBytesFromFile(byteOffset, 96, filePath); // memorymappedfiles not working
            byte[] data = new byte[96];
            for (long i = byteOffset; i < byteOffset + 96; i++)
            {
                data[i - byteOffset] = RAWDATA[i];
            }
            int byteOffsetB = 0;

            for (int i = 0; i < 4; i++)
            {
                GameObject o = null;
                switch (i)
                {
                    case 0: o = Body; break;
                    case 1: o = head; break;
                    case 2: o = rightHand; break;
                    case 3: o = leftHand; break;

                }
                if (o == null) { GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = "problem"; yield break; }// Print an error

                // de-Serialization

                float pX, pY, pZ, rX, rY, rZ; // float will need 

                pX = BitConverter.ToSingle(data, byteOffsetB);
                byteOffsetB += 4;
                pY = BitConverter.ToSingle(data, byteOffsetB);
                byteOffsetB += 4;
                pZ = BitConverter.ToSingle(data, byteOffsetB);
                byteOffsetB += 4;
                rX = BitConverter.ToSingle(data, byteOffsetB);
                byteOffsetB += 4;
                rY = BitConverter.ToSingle(data, byteOffsetB);
                byteOffsetB += 4;
                rZ = BitConverter.ToSingle(data, byteOffsetB);
                byteOffsetB += 4;

                if (i == 0)
                {
                    cPos = new Vector3(pX, pY, pZ);
                    if (_updateRawPosition) o.transform.position = cPos;
                   
                }
                else
                {
                    Vector3 oPos = cPos - new Vector3(pX, pY, pZ);
                    o.transform.position = oPos;
                }
                o.transform.localEulerAngles = new Vector3(rX, rY, rZ);

            }
            frameCounter++;
            if (frameCounter == frameLenght) { break; }
            byteOffset += 96;
            UnityEngine.Debug.Log(byteOffset);
            yield return new WaitForSeconds(0.04f);// should be the same as RecordGesture
        }
        GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = "ending playback";
        _playingBack = false;

    }



    // byte helper
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
    // File writing and reading Methods

    public void AppendBytesToFile(string _filePath, byte[] bytes)
    {

        using (FileStream f = new FileStream(_filePath, FileMode.Append))
        {
            f.Write(bytes, 0, bytes.Length);
        }

    }


}
