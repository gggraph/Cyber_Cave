using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeObjectWithHands : MonoBehaviour
{
    /*

    // Start is called before the first frame update

    private FileSelector _fileselector ;
    public Vector3 _iniScale;
    public void Init(FileSelector FS) 
    {
        _fileselector = FS;
        _iniScale = this.transform.localScale;
        InvokeRepeating("GetGesture", 4f, 1f);

    }
    public void GetGesture()
    {
        if (HandRecognition.IsPose_OKSign())
        {
         
            if (_fileselector != null) // mean 
            {
                _fileselector.OnResizeValidate(this.transform.localScale);

                Destroy(this.gameObject);
            }
            else
            {
                // send a resize msg to everyone..
                Destroy(this);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        // get Vector Y Distance
        float distx = HandRecognition.GetXDistanceOfHands();
        float disty = HandRecognition.GetYDistanceOfHands();
        float distz = HandRecognition.GetZDistanceOfHands();
        // Debug.Log("DISTANCE HAND X Y Z:" + distx + " " + disty + " "+ distz);
        // go from 0.0 to 0.7 
        float mutliplier = 2f;

        // Ok so will vary between 0.2 and 0.7. 0.7 is 3f. 0.2f is 0.0, 0.7f is 1.00 
        if ( disty < 0.1f)
        {
            disty = 0.1f;
        }
        else if ( disty >0.7f)
        {
            disty = 0.7f;
        }
        float prct = (disty - 0.1f) / 0.6f;
        float newscale = prct * (_iniScale.x*2f);
        if ( newscale == 0f)
        {
            newscale = 0.001f;
        }

        if ( distx < 0.2f && distz < 0.2f && !HandRecognition.IsLeftHandClosed() && !HandRecognition.IsRightHandClosed())
        {
            this.transform.localScale = new Vector3(newscale, newscale, newscale);
        }
       
    }
    */
}
