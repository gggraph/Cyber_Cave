using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureInterpreter : MonoBehaviour
{
    private Vector3 lastLookAtPosition;

    private void Start()
    {
        Vector3[] frontvecs = MathUtilities.GetPositionAndEulerInFrontOfPlayer(8f);
        lastLookAtPosition = frontvecs[0];
    }
    void CheckCameraAndRigCalibration()
    {
        return;
        // If head more than 45 degree. Update.... 
        GameObject rig = GameObject.Find("OVRCameraRig");
        float diff = Mathf.Abs(Mathf.DeltaAngle(rig.transform.eulerAngles.y, Camera.main.transform.localEulerAngles.y));
        if ( diff > 45)
        {
            Debug.Log("need a rotate... ");
            rig.transform.eulerAngles -= new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
            Camera.main.transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, 0, Camera.main.transform.eulerAngles.z);
        }
        /*
        Vector3[] frontvecs = MathUtilities.GetPositionAndEulerInFrontOfPlayer(8f);
        if (Vector3.Distance(frontvecs[0], lastLookAtPosition) > 2f) // valeur a trouver ici
        {
            lastLookAtPosition = frontvecs[0];
            GameObject rig = GameObject.Find("OVRCameraRig");
            rig.transform.eulerAngles -= new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
            Camera.main.transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, 0, Camera.main.transform.eulerAngles.z);
        }
        */
    }

        void Update()
    {
        CheckCameraAndRigCalibration();    
    }
}
