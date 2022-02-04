using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeFollower : MonoBehaviour
{

    // will let a plane object to follow camera smootly. 
    public float distance = 10f;
    public float speed    = 6f;


    private Vector3 targetPos;
    private Vector3 targetEuler;
    private Vector3 lastRigPos;
    private Vector3 lastCamPos; // as euler
    private bool needUpdate = true;
    public Vector3 EulerOffset = new Vector3(-90, 0, 0);
    public float LROffset = 0f;
    public float UDOffset = 0f;

    private void Start()
    {
        needUpdate = true;

        Vector3[] frontvecs = MathUtilities.GetPositionAndEulerInFrontOfPlayer(distance);
        targetPos = frontvecs[0] + (this.transform.right * LROffset) + (this.transform.up * UDOffset);
        this.transform.position = targetPos + (this.transform.up * 2f) + (this.transform.forward * 2f);
        this.transform.eulerAngles = frontvecs[1] + EulerOffset;
     
    }
    public void SetSpeed(float sp)
    {
        speed = sp;
    }
    public void SetDistanceFromCamera(float dist)
    {
        distance = dist; needUpdate = true;
    }

    /**
     The relevant window position is front of camera : which is camera.main transform.forward + distance. Y value will be zeroed.  
        * The euler seems good then.  
        *   
        *   
        *   
        *   
     */
    private void Update()
    {
        Vector3[] frontvecs = MathUtilities.GetPositionAndEulerInFrontOfPlayer(distance);
        if ( needUpdate)
        {
            targetPos = frontvecs[0] + (this.transform.right * LROffset) + (this.transform.up * UDOffset);
            targetEuler = frontvecs[1] + EulerOffset;
            needUpdate = false;
        }
        if (Vector3.Distance(frontvecs[0], targetPos) > 2f) // valeur a trouver ici
            needUpdate = true;

        if (this.transform.position != targetPos)
        {
            float step = (speed * Time.deltaTime) * (Vector3.Distance(transform.position, Camera.main.transform.position) / 5);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        }
        if (this.transform.eulerAngles != targetEuler)
        {
            transform.eulerAngles = targetEuler;
        }
   
    }
}
