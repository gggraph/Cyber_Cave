using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System; 

public class GestureStreamingMachine : MonoBehaviour
{
    public List<byte> _data;
    public long STREAM_MAX_LENGTH = 25000; // (2500 /s ) // so here max stream size is 10 secondes

    public void StartStreamingGesture()
    {
        // First Generate A File depending on User ID, username

        Thread t = new Thread(new ThreadStart(StreamGesture));
        t.IsBackground = true;
        t.Start();

    }

    public void StreamGesture() // /!\  __ We need to record offset from body for head and controllers part __ /!\
    {
        GameObject rightHand = this.transform.Find("RightHand").gameObject;
        GameObject leftHand = this.transform.Find("LeftHand").gameObject;
        GameObject head = this.transform.Find("Head").gameObject;
        GameObject Body = this.transform.Find("Center").gameObject;

        while (true)
        {
            /*-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- RECORD DATA STRUCTURE -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_- */
            /*
             - Body Position. Body Rotation. 
             - Head position. Head Rotation.
             - Controllers position. Controllers Rotation.
            
             */

            // Serialization
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
                if (o == null) return;// Print an error
                // we can check how many records we can do before breaking File Max Length . But dont play with this for the moment. 

                // all are float32 (single precision)

                // for position : 
                // should be offsets and not raw Vector Value if not body (i>0)
                if (i == 0)
                {
                    _data = AddBytesToList(_data, BitConverter.GetBytes(o.transform.position.x));
                    _data = AddBytesToList(_data, BitConverter.GetBytes(o.transform.position.y));
                    _data = AddBytesToList(_data, BitConverter.GetBytes(o.transform.position.z));
                }
                else
                {

                    Vector3 offset = Body.transform.position - o.transform.position; // carefull here for the order
                    _data = AddBytesToList(_data, BitConverter.GetBytes(offset.x));
                    _data = AddBytesToList(_data, BitConverter.GetBytes(offset.y));
                    _data = AddBytesToList(_data, BitConverter.GetBytes(offset.z));
                }


                Vector3 rot = o.transform.rotation.eulerAngles.GetXCorrectedEuler();
                _data = AddBytesToList(_data, BitConverter.GetBytes(rot.x));
                _data = AddBytesToList(_data, BitConverter.GetBytes(rot.y));
                _data = AddBytesToList(_data, BitConverter.GetBytes(rot.z));

          
            }

            FlushStream();
            Thread.Sleep(40);// should be the same as Playback
            // some additional info:
            // generate 2500 bytes per second (96*25) (so like 0.0025 mb)
            // so 1000 second will weight 2.5mb ( 16 minutes average )

        }

    }

    public  bool FlushStream() 
    {
        if ( _data.Count > STREAM_MAX_LENGTH) 
        {
            // remove first second ( first 2500 bytes )  
            for (int i = 0; i < 2500; i++)
            {
                _data.RemoveAt(0); // probably inneficient code here
            }
        }
     
        return true;
    }
    public static List<byte> AddBytesToList(List<byte> list, byte[] bytes)
    {
        foreach (byte b in bytes) { list.Add(b); }
        return list;
    }
}
