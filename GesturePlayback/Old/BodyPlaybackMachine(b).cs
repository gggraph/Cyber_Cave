using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Diagnostics;
using System.Threading;
using System;
using System.Drawing;

public class BodyPlaybackMachine_old : MonoBehaviour
{
    pdollar _r;
    public static long RECFILE_MAX_SIZE = 100000000000000;
    public bool _playingBack = false;

    public string _dataPath;

    Coroutine rec_thrd;
   // string _mynickname;
    
    public void Start()
    {

        _dataPath = Application.persistentDataPath;
        // Thread t = new Thread(new ThreadStart(Test));
        //  t.IsBackground = true;
        //   t.Start();
        StartCoroutine(Test());
    }
    public IEnumerator Test() // Will Register then PlayBack 
    {
        yield return new WaitForSeconds(5f);
        UnityEngine.Debug.Log(_dataPath);
        // first verify files folder
        VerifyFiles();
        GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = "file Ok";
        // record gesture during like 30 000 ms
        string nextfp = GetNewRecordFilePath(_dataPath + "/" + this.GetComponent<AvatarScript>()._nickname);
        GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = "rec start";
        StartRecordingGesture();

        yield return new WaitForSeconds(30f); // wait 10f
        //StopCoroutine(rec_thrd);
        GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = "rec end";
       
        if (StopRecordingGesture()) 
        {
            GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = "starting playback";
            // disabling net mov sync for the moment ( for solo test )
            GameObject.Find("XR Rig").transform.Find("Camera Offset").GetComponent<MovSync>().enabled = false;
            while (true) 
            {
                if (!_playingBack) 
                {
                    StartPlayBack(nextfp, false, 0, int.MaxValue);
                }
              
                yield return new WaitForSeconds(1f); // w
            }
           
        }
        yield return 0;
       
       
    }


    public void VerifyFiles() 
    {
        // we are 5 . Lionel, Theo, Mathilde, Gael, Florent

        string[] avatar_nickname = new string[5] { "Lionel", "Theo", "Mathilde", "Gael", "Florent" };
        foreach ( string s in avatar_nickname) 
        {
            if (!Directory.Exists(_dataPath + "/"+s))
            {
                Directory.CreateDirectory(_dataPath + "/"+s);
            }
        }
    }
    public string GetNewRecordFilePath(string directoryPath)
    {
        string[] files = Directory.GetFiles(directoryPath);
        return directoryPath + "/" + files.Length.ToString();
    }
    public void StartRecordingGesture()
    {

        rec_thrd = StartCoroutine(RecordGesture());

    }
    public bool StopRecordingGesture()
    {
        // First Generate A File depending on User ID, username
        StopCoroutine(rec_thrd);
        return true;
       
      
    }


    public IEnumerator RecordGesture() // /!\  __ We need to record offset from body for head and controllers part __ /!\
    {
        GameObject rightHand = this.transform.Find("RightHand").gameObject;
        GameObject leftHand = this.transform.Find("LeftHand").gameObject;
        GameObject head = this.transform.Find("Head").gameObject;
        GameObject Body = this.transform.Find("Center").gameObject;
        /*-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- Just For testing Purpose. File setup -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- */

        string filePath = GetNewRecordFilePath(_dataPath + "/" + this.GetComponent<AvatarScript>()._nickname);
        File.WriteAllBytes(filePath, new byte[0]); // better than File.Create or File.Delete

        /*-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- Add the Starting Time of the Record -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- */
        uint unixTimestamp = (uint)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        AppendBytesToFile(filePath, BitConverter.GetBytes(unixTimestamp));
        UnityEngine.Debug.Log(BitConverter.GetBytes(unixTimestamp).Length);
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
            for (int i = 0; i < 4; i ++) 
            {
                GameObject o = null; 
                switch(i) 
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
                if ( i == 0) 
                {
                    cPos = o.transform.position;
                    rc = AddBytesToList(rc, BitConverter.GetBytes(o.transform.position.x));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(o.transform.position.y));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(o.transform.position.z));
                  //  UnityEngine.Debug.Log(rc.Count + o.transform.position.ToString());
                }
                else 
                {

                    Vector3 offset = cPos - o.transform.position; // carefull here for the order
                    rc = AddBytesToList(rc, BitConverter.GetBytes(offset.x));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(offset.y));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(offset.z));
                  //  UnityEngine.Debug.Log(rc.Count);
                }


                Vector3 rot = o.transform.localEulerAngles;
                rc = AddBytesToList(rc, BitConverter.GetBytes(rot.x));
                rc = AddBytesToList(rc, BitConverter.GetBytes(rot.y));
                rc = AddBytesToList(rc, BitConverter.GetBytes(rot.z));
               // UnityEngine.Debug.Log(rc.Count);

              
            }
            AppendBytesToFile(filePath, ListToByteArray(rc));
            UnityEngine.Debug.Log(rc.Count);
            yield return new WaitForSeconds(0.04f);
            
            // break;
            //Thread.Sleep(40);// should be the same as Playback
            // some additional info:
            // generate 2500 bytes per second (96*25) (so like 0.0025 mb)
            // so 1000 second will weight 2.5mb ( 16 minutes average )2500000
        }
        yield return 0;

    }
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
        while ( byteOffset < fileLength) 
        {
            // get the next 24 bytes for offset
            // 24 bytes foreach. 96 bytes for all 4 objects
            //byte[] data = GetBytesFromFile(byteOffset, 96, filePath); // memorymappedfiles not working
            byte[] data = new byte[96];
            for (long i = byteOffset; i < byteOffset+96; i++) 
            {
                data[i-byteOffset] = RAWDATA[i];
            }
            int byteOffsetB = 0; 

            for (int i = 0; i < 4; i++) 
            {
                GameObject o = null; 
                switch(i) 
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

                if ( i == 0) 
                { 
                    cPos = new Vector3(pX, pY, pZ);
                    if (_updateRawPosition) o.transform.position = cPos;
                    GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = cPos.ToString();
                }
                else 
                {
                    Vector3 oPos = cPos - new Vector3(pX, pY, pZ); 
                    o.transform.position = oPos;
                }
                o.transform.localEulerAngles = new Vector3(rX, rY, rZ);

            }
            frameCounter++; 
            if ( frameCounter == frameLenght) { break; }
            byteOffset += 96;
            UnityEngine.Debug.Log(byteOffset);
            yield return new WaitForSeconds(0.04f);// should be the same as RecordGesture
        }
        GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = "ending playback";
        _playingBack = false;
      
    }


    // Do MachineIsAParrot inside a thread. 
    public IEnumerator MachineIsAParrot() 
    {
        // should be done each by each avatar to avoid memory protected access
        // should not reading rec files that is currently recording
        while ( true) 
        {
        
            if (!_playingBack) 
            {
                FakeMePlease();
            }
            yield return new WaitForSeconds(2f); // just a  delay to avoid huge computation...
        }
       
    }
    public bool FakeMePlease() 
    {
        /*-_-_-_-_-_-_-_-_-_-_-_-_-_- SOME IDEA HERE_-_-_-_-_-_-_-_-_--_-_-_-_--_-_-_-_*/
        /*
         * First idea is to Search Déjà-Vu Gesture of the current moment in Records File. 
         *  -> For this, we need to stream all Connected User Gesture as a byte array in the same format of the recording 
         *  process. 
         *  -> Then, searching this arrays as a subpattern in user record files using Aho Corasick or >Boyer Moore<.
         *  -----> we could use gesture recognizer p$ for a matching algorythm ( we need to translate rotation and z to 2d plane points ) 
         *  !!! we work only with offsets. 
         *  -----> Data Compression like Image Compression or Reduction could be also useful
         * 
         * ----> if Déjà-Vu, search the timeStamp and 40ms offset in our avatar record file and play back the moment
        */
        /*-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_--_-_-_-_--_-_-_-_*/

        GameObject[] Players = GameObject.FindGameObjectsWithTag("Avatar");

        uint TS = 0;
        int msOffset = -1;

        List<GameObject> Concurrents = new List<GameObject>(); // list of concurrent here 
        foreach (GameObject go in Players) 
        {
            // if go is not us and distance is not too far, and go facing me 
            if ( go != this.gameObject 
                && Vector3.Distance(go.transform.position, this.transform.position) < 1.0f) 
                // && add also facing check
            {
                Concurrents.Add(go);
            }
        
        }

        foreach ( GameObject go in Concurrents) 
        {



            // [0] Get the stream of the player  
            byte[] _stream = new byte[0]; // ListToByteArray(go.GetComponent<GestureStreamingMachine>()._data);

            // [1] Get Some Info Like : Average distance from us.  (opt) 
            //float _avrDistance = GetDistanceFromStream(_stream);

            // [2] Get the offsets.
            _stream = GetOffsetsFormatFromStreamFormat(_stream, 0); // this is a stream so i need 0 as args

            _r = new pdollar();
            _r.InitGestures();
            
            // get streamed gesture as list of 2d points array
            List<List<Point>> _stPts = OffsetFormatTo2dPoints(_stream);
            int c = 0;

            // Build a new Training Set from this array of list of points
            foreach (List<Point> lp in _stPts) 
            {
                List<float> pXs = new List<float>();
                List<float> pYs = new List<float>();

                string name = "_str" + c.ToString(); 
                foreach(Point p in lp) 
                {
                    pXs.Add(p.X);
                    pYs.Add(p.Y);
                   
                }
                // Add 2 The Gesture Set 
                _r.AddGesture(pXs, pYs, name);
                c++;
            }
            int TrainingSetLength = c; 
            // [3] Get the offsets. searching a Deja Vu

            string[] rcFiles = Directory.GetFiles(_dataPath + "/" + go.GetComponent<AvatarScript>()._nickname); 
            // should get all files in folder of specific go character ! 

            foreach( string s in rcFiles)
            {
                // Get as offset format
                byte[] _rec = GetOffsetsFormatFromStreamFormat(File.ReadAllBytes(s), 4);
                // Get as 2d point clouds
                List<List<Point>> _rcPts = OffsetFormatTo2dPoints(_rec);

                // Compare _rec and _stream. we have multiple choice. Boyer Moore? P$?  
                // apparently $P recognizer (2d space) is not a bad idea. Some research has been done to do 3d model recognition using this algo.
                // https://www.inderscience.com/info/inarticle.php?artid=95591

                // Get Total Score from all Gesture in the matching Order
                int offset = 0;
                while ( offset < offset + TrainingSetLength) 
                {
                    List<List<Point>> subList = new List<List<Point>>(); 
                    for (int i = offset; i < offset + TrainingSetLength; i++)
                    {
                        // create the sublist from specific index ( time ) 
                        subList.Add(_rcPts[i]);
                    }
                    // compare in order the new list of point with gesture
                    double[] results = _r.GetScoreMatchingFromPointSeries(subList);

                    // Now we will Break if rsults are sup than a specific trigger 
                    double total = 0; 
                    foreach(double d in results) 
                    {
                        total += d;
                    }
                    if ( total >= 0.24f * results.Length) 
                    {
                        // we found a matching pattern ! 
                        // get the TimeStamp of the s file path 
                        // and get the offset ... (  40ms*offset )
                        
                        TS = BitConverter.ToUInt32(_rec, 0);
                        msOffset = offset;
;                       break; 
                    }
                    offset++;//+= TrainingSetLength;
                }
                if ( msOffset != -1 ) { break;  } // stop if we match someone
            }

            if (msOffset != -1) { break; } // stop if we match someone 

        }

        if ( TS != 0) 
        {
            // Get the msOffset needed 
            // search for a matching time in our rec files 
            string[] rcFiles = Directory.GetFiles(_dataPath + "/" + this.GetComponent<AvatarScript>()._nickname);  
            // should get all files in folder of this character ! 
            foreach ( string s in rcFiles) 
            {
                uint ts = 0;// BitConverter.ToUInt32(GetBytesFromFile(0, 4, s),0); //< --- MMF PROBLEM
                long totalFrames = GetFramesNumberOfRecFiles(s);

                uint cEnTS = ts + (uint)(totalFrames / 25);

                uint lEnTS = TS + (uint)(msOffset / 25);

                if (lEnTS >= ts && lEnTS < cEnTS) 
                {
                    // we got it. start playback from here

                    // lENTS is the precise second we want to start Playback 
                    int msof = (int)(lEnTS - ts);
                    msof *= 25;
                    // skipp the msof frames
                    // but only start playback if no anom
                    StartPlayBack(s, false, msof, 125 ); // playing during 5s by default ( so 25 * 5 )
                    return true;
                
                }
                // divide by 25 to get seconds . 
                // check if ts 
            }

        }
        return false;

    }
    /*-_---_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- CURRENT GESTURE SYSTEMS -_---_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-*/
   
    // Find A Bark for this specific Avatar 
    public bool DoABark(GameObject go) 
    {

        GameObject[] Players = GameObject.FindGameObjectsWithTag("Avatar");

        List<GameObject> Concurrents = new List<GameObject>(); // list of concurrent here 
        foreach (GameObject g in Players)
        {
            // -_-_-_-_-_-_-_-_-_-_-_--_-_-_-_-_ WARNING  -_-_-_-_-_-_-_-_-_-_-_--_-_-_-_-_  \\
            //      ------------------ add facing check ------------------
            // if go is not us and distance is not too far, and go facing me 
            if (g != go
                && Vector3.Distance(g.transform.position, go.transform.position) < 1.0f)
            // && add also facing check
            {
                Concurrents.Add(g);
            }

        }
        foreach ( GameObject g in Concurrents) 
        {
            uint DejaVuResult = DejaVu(g);
            if ( DejaVuResult != 0) 
            {
                if ( PlayRecordGestureFromTimeStamp(go, DejaVuResult)) 
                {
                    return true;
                }
            }
        }
        return false; 
    }

    // Start Playing a Record Gesture from specific time
    public bool PlayRecordGestureFromTimeStamp(GameObject go, uint TimeStamp) 
    {
        string[] rcFiles = Directory.GetFiles(_dataPath + "/" + go.GetComponent<AvatarScript>()._nickname);

        foreach ( string s in rcFiles) 
        {
            uint fileTs = BitConverter.ToUInt32(File.ReadAllBytes(s), 0);
            uint secondCount = (uint)GetFramesNumberOfRecFiles(s) / 25;

            if ( TimeStamp >= fileTs && TimeStamp <= fileTs + secondCount) 
            {
                StartPlayBack(s, false, (int)((secondCount * 25) * 96), 125); // playing during 5s by default ( so 25 * 5 )
                return true;
            }
        
        
        }

        return false;
    }

    // Find a Deja Vu Gesture from this Avatar. Returning Timestamp. 
    public uint DejaVu(GameObject go) 
    {

        // [1] Get the offsets as Vector3.
        List<Vector3> _offsetsStream = new List<Vector3>(); //  GetOffsetsVectorsFromStreamFormat(ListToByteArray(go.GetComponent<GestureStreamingMachine>()._data), 0);

        // [2] Search in every Avatars records a similar gesture
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Avatar"); // get the files... we prefer first the go guy ...

        // [3] Put first go guy. It makes senses
        //...

        // [4] Iterate  
        for (int i = 0; i < Players.Length; i++) 
        {
            // [4b] Iterate records data s 
            string[] rcFiles = Directory.GetFiles(_dataPath + "/" + Players[i].GetComponent<AvatarScript>()._nickname);
            foreach ( string s in rcFiles) 
            {
                // Only procceed if gesture was in our field view at the time 
                byte[] _raw = File.ReadAllBytes(s);
                List<Vector3> _offsetsRecord = GetOffsetsVectorsFromStreamFormat(_raw, 4);

                int vectorOffset = 0; 
                while ( vectorOffset < vectorOffset + _offsetsStream.Count) 
                {
                    List<Vector3> subList = new List<Vector3>();
                    for (int n = vectorOffset; n < vectorOffset + _offsetsStream.Count; n++)
                    {
                        // create the sublist from specific index ( time ) 
                        subList.Add(_offsetsRecord[n]);
                    }

                    //  [4c] Compare
                    float MatchScore = MatchingGesturesFromDistancePoints(_offsetsStream, subList);

                    // -_-_-_-_-_-_-_-_-_-_-_--_-_-_-_-_ WARNING  -_-_-_-_-_-_-_-_-_-_-_--_-_-_-_-_  \\
                    if ( MatchScore < 0.01f) // ?? minimal trigger unknown ... test & try
                    {
                        uint TimeStamp = BitConverter.ToUInt32(_raw, 0);
                        // get vectorOffset divided by 25 ( cause 0.04f is 1 second divided by 25 )
                        uint SecondOffset = (uint)(vectorOffset / 25) ; 
                        return TimeStamp + SecondOffset;
                    }
                     /*-_-_-_-_-_-_-_-_-_-_-_--_-_-_-_-_           -_-_-_-_-_-_-_-_-_-_-_--_-_-_-_-_  */
                    vectorOffset++; // increment by 1 
                }

            }
        }
        return 0;
    }
    public List<Vector3> GetOffsetsVectorsFromStreamFormat(byte[] _stream, long byteOffset) // should be 0 or 4 depending of the file
    {
        List<Vector3> result = new List<Vector3>();
        while (byteOffset < _stream.Length)
        {
            byteOffset += 24;
            byte[] data = new byte[72];
            // we only need byteOffset+24 , byteOffset+96
            for (long i = byteOffset; i < byteOffset + 72; i++)
            {
                data[i - byteOffset] = _stream[i];
            }
            int byteOffsetB = 0;
            for (int i = 0; i < 3; i++)
            {

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

                // -_-_-_-_-_-_-_-_-_-_-_--_-_-_-_-_ WARNING  -_-_-_-_-_-_-_-_-_-_-_--_-_-_-_-_  \\
                // we will add two vectors in series ( pos and rot ) -- but could maybe bad 
                // we should normalize position from rotation vector3 and dont add 2 vectors !
                result.Add(new Vector3(pX, pY, pZ));
                result.Add(new Vector3(rX, rY, rZ));


            }
            byteOffset += 72;
        }
        return result;
    }

    // MATCHING ALGORYTHM
    public float MatchingGesturesFromDistancePoints(List<Vector3> vA, List<Vector3> vB) 
    {
        // -_-_-_-_-_-_-_-_-_-_-_--_-_-_-_-_ WARNING  -_-_-_-_-_-_-_-_-_-_-_--_-_-_-_-_  \\
        // Will need to dig into this function
        // add penality for hand etc. 
        float result = 0f;
        for (int i = 0; i < vA.Count; i++) 
        {
            result +=  Vector3.Distance(vA[i], vB[i]);

        }
        result /= vA.Count;
        return result;
    }
    /*-_---_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- END  -_---_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-*/

   

    public long GetFramesNumberOfRecFiles(string s) 
    {
        long length = new FileInfo(s).Length;
        length -= 4; // 
        length /= 96;
        return length; 
    }
    public List<List<Point>> OffsetFormatTo2dPoints(byte[] _data) 
    {
        List<List<Point>> result = new List<List<Point>>();
        long byteOffset = 0;
        // getting 72 bytes 
        while (byteOffset < _data.Length)
        {
            List<Point> pts = new List<Point>(); 
            byte[] data = new byte[72];
            for (long n = byteOffset; n < byteOffset + 72; n++)
            {
                data[n - byteOffset] = _data[n];
            }
            int byteOffsetB = 0;
            for (int i = 1; i < 3; i++)
            {
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

                // just testing with x and y for the moment ... 
                int precision = 1000;
                pts.Add(new Point((int)(pX*precision), (int)(pY*precision))); // need to convert float to int ... so add precision 
               

            }
            result.Add(pts);
            byteOffset += 72;
        }
        return result;
    }
    public float GetDistanceFromStream(byte[] _stream)
    {
        GameObject Body = this.transform.Find("Center").gameObject;
        long byteOffset = 0; // dont put 4 here cause timestamp is not used

        List<float> rDistances = new List<float>(); 
        // float in Unity are 4 bytes
        while (byteOffset < _stream.Length)
        {
            // get the next 24 bytes for offset
            // 24 bytes foreach. 96 bytes for all 4 objects
            byte[] data = new byte[12]; 
            for (long i  = byteOffset; i < byteOffset + 12; i++) 
            {
                data[i - byteOffset] = _stream[i];
            }

            int byteOffsetB = 0;
            float pX, pY, pZ;
            pX = BitConverter.ToSingle(data, byteOffsetB);
            byteOffsetB += 4;
            pY = BitConverter.ToSingle(data, byteOffsetB);
            byteOffsetB += 4;
            pZ = BitConverter.ToSingle(data, byteOffsetB);

            Vector3 v = new Vector3(pX, pY, pZ);
            rDistances.Add(Vector3.Distance(v, Body.transform.position));

            byteOffset += 96;
        }

        float result = 0f; 
        foreach ( float f in rDistances) 
        {
            result += f; 
        }
        result /= rDistances.Count;

        return result; 
    }

    public byte[] GetOffsetsFormatFromStreamFormat(byte[] _stream, long byteOffset) // should be 0 or 4 depending of the file
    {
        List<byte> b = new List<byte>(); 
        while (byteOffset < _stream.Length)
        {
            // we only need byteOffset+24 , byteOffset+96
            for (long i = byteOffset + 24; i < byteOffset+96; i++) 
            {
                b.Add(_stream[i]); 
            }
            byteOffset += 96;
        }
        return ListToByteArray(b);
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
    public void TruncateFile(string _filePath, uint length) // can result an error. cant use file get length. 
    {
        FileInfo fi = new FileInfo(_filePath);
        FileStream fs = new FileStream(_filePath, FileMode.Open);

        fs.SetLength(fi.Length - length);
        fs.Close();
    }

}
