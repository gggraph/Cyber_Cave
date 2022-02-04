using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    public float speed = 1;
    public void SetSpeed(float s)
    {
        speed = s;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, -speed, 0);
    }
}
