using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMap : MonoBehaviour
{
    public static AudioSource SoundModule = null;
    public static void Init()
    {
        GameObject smod = new GameObject();
        smod.AddComponent<AudioSource>();
        SoundModule = smod.GetComponent<AudioSource>();
    }

    public static void PlaySoundFromAssets(string sndName)
    {
        if (!SoundModule)
            Init();

        AudioClip ac = (AudioClip)Resources.Load("sound/" + sndName);
        if ( ac)
        {
            SoundModule.PlayOneShot(ac);
        }
        else
        {
            Debug.Log("cannot load sound");
        }
        
    }

    public static void PlaySoundAndDisposeFromAssets(string sndName)
    {

        AudioClip ac = (AudioClip)Resources.Load("sound/" + sndName);
        if (ac)
        {
            AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        }
        else
        {
            Debug.Log("cannot load sound");
        }

    }
}
