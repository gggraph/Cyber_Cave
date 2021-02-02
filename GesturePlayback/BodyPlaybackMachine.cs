using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Diagnostics;
using System.Threading;
using System;
using System.Drawing;

public class BodyPlaybackMachine : MonoBehaviour
{
    pdollar _r;
    public static long RECFILE_MAX_SIZE = 100000000000000;
    public bool _playingBack = false;

    public void StartRecordingGesture() 
    {
        // First Generate A File depending on User ID, username

        Thread t = new Thread(new ThreadStart(RecordGesture));
        t.IsBackground = true;
        t.Start(); 

    }

    public void RecordGesture() // /!\  __ We need to record offset from body for head and controllers part __ /!\
    {
        GameObject rightHand = this.transform.Find("RightHand").gameObject;
        GameObject leftHand = this.transform.Find("LeftHand").gameObject;
        GameObject head = this.transform.Find("Head").gameObject;
        GameObject Body = this.transform.Find("Center").gameObject;
        /*-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- Just For testing Purpose. File setup -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- */
        
        string filePath = Application.persistentDataPath + "/rectest";

        File.WriteAllBytes(filePath, new byte[0]); // better than File.Create or File.Delete

        /*-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- Add the Starting Time of the Record -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- */
        uint unixTimestamp = (uint)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        AppendBytesToFile(filePath, BitConverter.GetBytes(unixTimestamp));

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
                if (o == null) return;// Print an error
                // we can check how many records we can do before breaking File Max Length . But dont play with this for the moment. 

                // all are float32 (single precision)

                // for position : 
                // should be offsets and not raw Vector Value if not body (i>0)
                if ( i == 0) 
                {
                    rc = AddBytesToList(rc, BitConverter.GetBytes(o.transform.position.x));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(o.transform.position.y));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(o.transform.position.z));
                }
                else 
                {

                    Vector3 offset = Body.transform.position - o.transform.position; // carefull here for the order
                    rc = AddBytesToList(rc, BitConverter.GetBytes(offset.x));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(offset.y));
                    rc = AddBytesToList(rc, BitConverter.GetBytes(offset.z));
                }
                

                Vector3 rot = o.transform.rotation.eulerAngles.GetXCorrectedEuler();
                rc = AddBytesToList(rc, BitConverter.GetBytes(rot.x));
                rc = AddBytesToList(rc, BitConverter.GetBytes(rot.y));
                rc = AddBytesToList(rc, BitConverter.GetBytes(rot.z));

                AppendBytesToFile(filePath, ListToByteArray(rc)); 
            }


            Thread.Sleep(40);// should be the same as Playback
            // some additional info:
            // generate 2500 bytes per second (96*25) (so like 0.0025 mb)
            // so 1000 second will weight 2.5mb ( 16 minutes average )2500000
        }

    }
    public void StartPlayBack(string filePath, bool _updateRawPosition, int msOffset, int frameLength)
    {
        _playingBack = true;
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            PlayBack(filePath, _updateRawPosition, msOffset, frameLength );
        }).Start();

    }

    public void PlayBack(string filePath, bool _updateRawPosition, int msOffset, int frameLenght) 
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

        while ( byteOffset < fileLength) 
        {
            // get the next 24 bytes for offset
            // 24 bytes foreach. 96 bytes for all 4 objects
            byte[] data = GetBytesFromFile(byteOffset, 96, filePath);

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
                if (o == null) return; // Print an error

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
                }
                else 
                {
                    Vector3 oPos = cPos + new Vector3(pX, pY, pZ); // dont know if we should use + or - here .... we will see
                    o.transform.position = oPos;
                }
                
                o.transform.Rotate(rX, rY, rZ); // probably bad... 

            }
            frameCounter++; 
            if ( frameCounter == frameLenght) { break; }
            byteOffset += 96; 
            Thread.Sleep(40); // should be the same as RecordGesture
        }
        _playingBack = false;

    }


    // Do MachineIsAParrot inside a thread. 
    public void MachineIsAParrot() 
    {
        while ( true) 
        {
        
            if (!_playingBack) 
            {
                FakeMePlease();
            }
            Thread.Sleep(2000); // just a  delay to avoid huge computation...
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
            if ( go == this.gameObject) { } // do nothing? a voir + tard .... 


            // [0] Get the stream of the player  
            byte[] _stream = ListToByteArray(go.GetComponent<GestureStreamingMachine>()._data);

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

            string[] rcFiles = new string[0]; // should get all files in folder of specific go character ! 

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
                    offset += TrainingSetLength;
                }
                if ( msOffset != -1 ) { break;  } // stop if we match someone
            }

            if (msOffset != -1) { break; } // stop if we match someone 

        }

        if ( TS != 0) 
        {
            // Get the msOffset needed 
            // search for a matching time in our rec files 
            string[] rcFiles = new string[0]; // should get all files in folder of specific go character ! 
            foreach ( string s in rcFiles) 
            {
                uint ts = BitConverter.ToUInt32(GetBytesFromFile(0, 4, s),0);
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

    public byte[] GetBytesFromFile(long startIndex, uint length, string _filePath)
    {
         using (MemoryMappedFile memFile = MemoryMappedFile.CreateFromFile(_filePath)) 
        //{
        // using (MemoryMappedViewStream memoryMappedViewStream = memoryMappedFile.CreateViewStream(startIndex, length, MemoryMappedFileAccess.Read))
       // using (MemoryMappedFile memFile = MemFile(_filePath))
        {
            using (MemoryMappedViewStream memoryMappedViewStream = memFile.CreateViewStream(startIndex, length, MemoryMappedFileAccess.Read))
            {
                byte[] result = new byte[length];
                for (uint i = 0; i < length; i++)
                {
                    result[i] = (byte)memoryMappedViewStream.ReadByte();
                }

                return result;
            }
        }
    }
    public void OverWriteBytesInFile(uint startIndex, string _filePath, byte[] bytes) // can result an error. cant use file get length. 
    {

        using (MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateFromFile(_filePath))
        {
            using (MemoryMappedViewStream memoryMappedViewStream = memoryMappedFile.CreateViewStream(startIndex, bytes.Length))
            {
                memoryMappedViewStream.WriteAsync(bytes, 0, bytes.Length);
            }
        }
    }
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
