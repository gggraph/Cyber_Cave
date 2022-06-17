using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodulableTester : MonoBehaviour
{
 
    void Start()
    {
        // Create one moodulable
        GameObject m1 = new GameObject();
        m1.name = "Moodulable";
        Moodulable mood1 = m1.AddComponent<Moodulable>();
        mood1.InitFromDefault();
        mood1.transform.position = new Vector3(-5, 0.8f, 5);
        mood1.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        mood1.gameObject.AddComponent<Grabbable>();
      // add grabbable :) 
    }

}
