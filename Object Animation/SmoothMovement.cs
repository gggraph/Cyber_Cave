using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothMovement : MonoBehaviour
{
    public Vector3 TargetPosition;
    public Vector3 TargetScale = new Vector3(1,1,1); 
    public float speed = 1f;

    public void SetSpeed ( float sp)
    {
        speed = sp;
    }
    public void SetPosition(Vector3 pos)
    {
        TargetPosition = pos;
    }
    public void SetScale(Vector3 scale)
    {
        TargetScale = scale;
    }
    // Update is called once per frame
    void Update()
    {
        if ( transform.position != TargetPosition)
        {
            float step = (speed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, TargetPosition, step);
        }
        if (transform.localScale != TargetScale)
        {
            float step = (speed * Time.deltaTime) / 2;
            transform.localScale = Vector3.MoveTowards(transform.localScale, TargetScale, step);
        }
    }
}
