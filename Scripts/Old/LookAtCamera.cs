using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    bool _alignOnAxis = true;

    public void SetAlignmentOnAxis(bool val)
    {
        _alignOnAxis = val;
    }

    void Update()
    {
        transform.LookAt(Camera.main.transform);
        if ( _alignOnAxis)
        {
            // get nearest quarter (0,90,180 etc. ) 
            int y = MathUtilities.nearestmultiple((int)this.transform.localEulerAngles.y, 90, true);
            this.transform.localEulerAngles = new Vector3(0, y, 0);
        }
    }

}
