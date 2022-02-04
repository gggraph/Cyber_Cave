using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicInteraction : MonoBehaviour
{

    public IEnumerator DestroyObjectOnValidateSign(GameObject g, float secondInterval)
    {
        while (!HandRecognition.IsPose_OKSign())
        {
            yield return new WaitForSeconds(secondInterval);
        }
        Destroy(g.gameObject);
        yield break;
    }
    public IEnumerator ResizeObjectWithHands(GameObject g, System.Action<Vector3> callback = null)
    {
        Vector3 _iniScale = g.transform.localScale; 

        while (!HandRecognition.IsPose_OKSign())
        {
            // also do a sign to reset object and close to initial value if needed 
            // get Vector Y Distance
            float distx = HandRecognition.GetXDistanceOfHands();
            float disty = HandRecognition.GetYDistanceOfHands();
            float distz = HandRecognition.GetZDistanceOfHands();
            // Debug.Log("DISTANCE HAND X Y Z:" + distx + " " + disty + " "+ distz);
            // go from 0.0 to 0.7 
            float mutliplier = 2f;

            // Ok so will vary between 0.2 and 0.7. 0.7 is 3f. 0.2f is 0.0, 0.7f is 1.00 
            if (disty < 0.1f)
            {
                disty = 0.1f;
            }
            else if (disty > 0.7f)
            {
                disty = 0.7f;
            }
            float prct = (disty - 0.1f) / 0.6f;
            float newscale = prct * (_iniScale.x * 2f);
            if (newscale == 0f)
            {
                newscale = 0.001f;
            }

            if (distx < 0.2f && distz < 0.2f && !HandRecognition.IsLeftHandClosed() && !HandRecognition.IsRightHandClosed())
            {
                g.transform.localScale = new Vector3(newscale, newscale, newscale);
            }
            yield return new WaitForEndOfFrame();
        }
        callback(g.transform.localScale);
        yield break;
    }
}
