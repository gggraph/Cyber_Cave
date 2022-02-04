using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
public class SoundHit : MonoBehaviour
{

    public bool _HitOnCollider = false;
    public bool _PlayInsideCollider = false;
    public void SetEventFormat(byte format)
    {
        switch (format)
        {
            case 0:

                AudioSource AS = GetComponent<AudioSource>();
                AS.maxDistance = 3f;
                AS.spatialBlend = 1.0f;

                break;


        }
    }

    public IEnumerator LoadMusic(string songPath, bool playAfterLoad = false)
    {

        if (System.IO.File.Exists(songPath))
        {
            using (var uwr = UnityWebRequestMultimedia.GetAudioClip("file://"+songPath, AudioType.MPEG))
            {
                ((DownloadHandlerAudioClip)uwr.downloadHandler).streamAudio = true;

                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.LogError(uwr.error);
                    yield break;
                }

                DownloadHandlerAudioClip dlHandler = (DownloadHandlerAudioClip)uwr.downloadHandler;

                if (dlHandler.isDone)
                {
                    AudioClip audioClip = dlHandler.audioClip;

                    if (audioClip != null)
                    {
                        GetComponent<AudioSource>().clip = DownloadHandlerAudioClip.GetContent(uwr);
                        if (playAfterLoad)
                        {
                            GetComponent<AudioSource>().Play();
                        }
                        Debug.Log("Playing song using Audio Source!");

                    }
                    else
                    {
                        Debug.Log("Couldn't find a valid AudioClip :(");
                    }
                }
                else
                {
                    Debug.Log("The download process is not completely finished.");
                }
            }
        }
        else
        {
            Debug.Log("Unable to locate converted song file.");
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "HANDHIT")
        {

            if (collision.gameObject.transform.root.gameObject.tag != "Avatar")
            {
                return;
            }
            if (collision.gameObject.transform.root.gameObject.GetComponent<NetStream>() != NetUtilities._mNetStream)
            {
                return;
            }
            if (_PlayInsideCollider)
            {
                GetComponent<AudioSource>().Stop();
            }
           
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if ( collision.gameObject.tag == "HANDHIT")
        {
            // make sure its our Hand! 
           

            if (collision.gameObject.transform.root.gameObject.tag != "Avatar")
            {
                return;
            }
            if (collision.gameObject.transform.root.gameObject.GetComponent<NetStream>() != NetUtilities._mNetStream )
            {
                return;
            }
            if (_PlayInsideCollider)
            {
                GetComponent<AudioSource>().Play();
            }
            /*
            DO_HIT();

            uint soundID = 255;
            uint.TryParse(this.gameObject.name.Replace("SND_", ""), out soundID);
            if ( soundID != 255)
            {
                List<byte> data = new List<byte>();
                data.Add(10); // add the header 
                BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(soundID));

                NetUtilities.SendDataToAll(data.ToArray());
            }
            */


        }
      
    }
    /*
    public void DO_HIT()
    {
        if ( AS.isPlaying)
        {
            AS.Stop();
        }
        AS.Play();
        Debug.Log("Hit!");
    }*/
}
