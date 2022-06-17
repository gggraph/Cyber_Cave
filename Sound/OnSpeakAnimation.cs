using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSpeakAnimation : MonoBehaviour
{
    // # Something to attach to each player that will make a little animation when they speak & audio stream is received 
    [Header("Treshold")]
    public float AmplitudeTreshold = 0f;
    [Header("Icon Configuration")]
    public GameObject SpeakerIcon;
    public float iconSize = 0.1f;
    public float HeightAboveFeet = 2f;

    [Header("Debug")]
    public bool currentlySpeaking = false;
    public float currentAmplitude;

    private AudioSource audioSource;
    public float updateStep = 0.1f;
    private int sampleDataLength = 1024;

    private float currentUpdateTime = 0f;

    private float[] clipSampleData;

    private float clipLoudness;

   

    // Use this for initialization
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            Debug.LogError(GetType() + ".Awake: there was no audioSource set.");
        }
        clipSampleData = new float[sampleDataLength];

        // Set up speaker Icon...
        // For the debug use sphere. Else instantiate from assets.
        if ( !SpeakerIcon)
        {
            SpeakerIcon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }
        SpeakerIcon.transform.localScale = Vector3.one * iconSize;
        // Set its position
        SpeakerIcon.transform.position = this.transform.position + new Vector3(0, HeightAboveFeet, 0);
        // Set also as child
        SpeakerIcon.transform.parent = this.transform;
        ObjectUtilities.FadeOutObject(SpeakerIcon, 1f);


    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource)
            return;
        if (!audioSource.clip)
            return;
        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= updateStep)
        {

            currentUpdateTime = 0f;

            audioSource.clip.GetData(clipSampleData, audioSource.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
            clipLoudness = 0f;
            foreach (var sample in clipSampleData)
            {
                clipLoudness += Mathf.Abs(sample);
            }
            clipLoudness /= sampleDataLength; //clipLoudness is what you are looking for
        }
        currentAmplitude = clipLoudness;
        // Here set is speaking. If true, show object else do not show object... 
        bool isSpeaking = clipLoudness <= AmplitudeTreshold ? false : true;
        if ( isSpeaking && !currentlySpeaking)
        {
            // fade in icon object
            ObjectUtilities.FadeInObject(SpeakerIcon, 1f);
        }
        if ( !isSpeaking && currentlySpeaking)
        {
            // fadeout the icon object
            ObjectUtilities.FadeOutObject(SpeakerIcon, 1f);
        }
        currentlySpeaking = isSpeaking;

        SpeakerIcon.transform.localScale = Vector3.one * iconSize;
        SpeakerIcon.transform.position = this.transform.position + new Vector3(0, HeightAboveFeet, 0);
    }
}
