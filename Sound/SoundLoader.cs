using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class SoundLoader : MonoBehaviour
{
    private void Start()
    {
        if (!GetComponent<AudioSource>())
            this.gameObject.AddComponent<AudioSource>();
    }
    public IEnumerator LoadMusic(string songPath, bool playAfterLoad = false)
    {

        if (System.IO.File.Exists(songPath))
        {
            using (var uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + songPath, AudioType.MPEG))
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

}
