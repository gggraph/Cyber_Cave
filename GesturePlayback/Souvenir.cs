using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Photon.Pun;

public class Souvenir : MonoBehaviour
{

    /* This class is only set once somewhere. It rules every offline Avatar gestures */

   public IEnumerator SouvenirRoutine() 
   {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Avatar");
        foreach ( GameObject go in Players) 
        {
            if ( go.GetComponent<PhotonView>() == null && !go.GetComponent<GestureSaver>()._playingBack) 
            {
                DoABark(go);
            }
        }
        yield return 0;
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
        foreach (GameObject g in Concurrents)
        {
            uint DejaVuResult = DejaVu(g);
            if (DejaVuResult != 0)
            {
                if (PlayRecordGestureFromTimeStamp(go, DejaVuResult))
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
        // -_-_-_-_-_ WARNING SHOULD GET PLAYBACK DEPENDING OF PROXIMITY AND FACE CHECKING
        string[] rcFiles = Directory.GetFiles(Application.persistentDataPath + "/" + go.GetComponent<AvatarScript>()._nickname);

        foreach (string s in rcFiles)
        {
            uint fileTs = BitConverter.ToUInt32(File.ReadAllBytes(s), 0);
            uint secondCount = (uint)GetFramesNumberOfRecFiles(s) / 25;

            if (TimeStamp >= fileTs && TimeStamp <= fileTs + secondCount)
            {
                go.GetComponent<GestureSaver>().StartPlayBack(s, false, (int)((secondCount * 25) * 96), 125); // playing during 5s by default ( so 25 * 5 )
                return true;
            }


        }

        return false;
    }

    // Find a Deja Vu Gesture from this Avatar. Returning Timestamp. 
    public uint DejaVu(GameObject go)
    {

        // [1] Get the offsets as Vector3.
        List<Vector3> _offsetsStream = GetOffsetsVectorsFromStreamFormat(ListToByteArray(go.GetComponent<GestureSaver>()._streamData), 0);

        // [2] Search in every Avatars records a similar gesture
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Avatar"); // get the files... we prefer first the go guy ...

        // [3] Put first go guy. It makes senses
        //...

        // [4] Iterate  
        for (int i = 0; i < Players.Length; i++)
        {
            // [4b] Iterate records data s 
            string[] rcFiles = Directory.GetFiles(Application.persistentDataPath + "/" + Players[i].GetComponent<AvatarScript>()._nickname);
            foreach (string s in rcFiles)
            {
                // Only procceed if gesture was in our field view at the time 
                byte[] _raw = File.ReadAllBytes(s);
                List<Vector3> _offsetsRecord = GetOffsetsVectorsFromStreamFormat(_raw, 4);

                int vectorOffset = 0;
                while (vectorOffset < vectorOffset + _offsetsStream.Count)
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
                    if (MatchScore < 0.01f) // ?? minimal trigger unknown ... test & try
                    {
                        uint TimeStamp = BitConverter.ToUInt32(_raw, 0);
                        // get vectorOffset divided by 25 ( cause 0.04f is 1 second divided by 25 )
                        uint SecondOffset = (uint)(vectorOffset / 25);
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
            result += Vector3.Distance(vA[i], vB[i]);

        }
        result /= vA.Count;
        return result;
    }

    public long GetFramesNumberOfRecFiles(string s)
    {
        long length = new FileInfo(s).Length;
        length -= 4; // 
        length /= 96;
        return length;
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
}
