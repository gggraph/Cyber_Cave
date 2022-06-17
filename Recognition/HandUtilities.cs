using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class HandUtilities : MonoBehaviour
{
    /// <summary>
    ///  Give basic hand interaction and Hand data. Has to be set up on player.
    /// </summary>
    public static GameObject LeftHand;
    public static GameObject RightHand;
    public static GameObject Head;


    void Start()
    {
        LeftHand = GameObject.Find("OVRlefthand");
        RightHand = GameObject.Find("OVRrighthand");
        Head = GameObject.Find("CenterEyeAnchor");

       // InvokeRepeating("GetPoseStrategy1", 5, 1);
    }
    public static int counter;

    // Get Each Bones (both right hand & left hand velocity ) 
    private void Update()
    {
        
    }
    public void GetPoseStrategy1()
    {

        byte[] bin = PoseRecognition.GetHandsPoseAsBinary(LeftHand, RightHand, Head);
        if (bin.Length > 0)
        {
            File.WriteAllBytes(Application.persistentDataPath + "/pose" + counter.ToString(), bin);
            Debug.LogError("Pose saved #" + counter.ToString());
            counter++;
        }


    }


    public static bool IsBothHandTracked()
    {
        if (LeftHand.GetComponent<OVRHand>().IsTracked && RightHand.GetComponent<OVRHand>().IsTracked)
            return true;
        else
            return false;
    }
    public static bool IsAnyHandTracked()
    {
        if (LeftHand.GetComponent<OVRHand>().IsTracked || RightHand.GetComponent<OVRHand>().IsTracked)
            return true;
        else
            return false;
    }
    public static bool IsRightHandTracked()
    {
        return RightHand.GetComponent<OVRHand>().IsTracked;
    }
    public static bool IsLeftHandTracked()
    {
        return LeftHand.GetComponent<OVRHand>().IsTracked;
    }

    public static float GetYDistanceOfHands()
    {
        return Math.Abs(RightHand.GetComponent<OVRSkeleton>().Bones[0].Transform.position.y - LeftHand.GetComponent<OVRSkeleton>().Bones[0].Transform.position.y);
    }
    public static float GetXDistanceOfHands()
    {
        return Math.Abs(RightHand.GetComponent<OVRSkeleton>().Bones[0].Transform.position.x - LeftHand.GetComponent<OVRSkeleton>().Bones[0].Transform.position.x);
    }
    public static float GetZDistanceOfHands()
    {
        return Math.Abs(RightHand.GetComponent<OVRSkeleton>().Bones[0].Transform.position.z - LeftHand.GetComponent<OVRSkeleton>().Bones[0].Transform.position.z);
    }

    public static bool IsHandClosed(GameObject hand)
    {
        if (hand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return false;

        float min_dist = 0.125f;
        if (GetDistanceFromFingers(hand.GetComponent<OVRSkeleton>().Bones[11], hand.GetComponent<OVRSkeleton>().Bones[0]) <= min_dist
          && GetDistanceFromFingers(hand.GetComponent<OVRSkeleton>().Bones[8], hand.GetComponent<OVRSkeleton>().Bones[0]) <= min_dist
          && GetDistanceFromFingers(hand.GetComponent<OVRSkeleton>().Bones[14], hand.GetComponent<OVRSkeleton>().Bones[0]) <= min_dist
          && GetDistanceFromFingers(hand.GetComponent<OVRSkeleton>().Bones[18], hand.GetComponent<OVRSkeleton>().Bones[0]) <= min_dist
          )
        {
            return true;
        }
        else
            return false;
    }
    public static bool IsLeftHandClosed()
    {
        return IsHandClosed(LeftHand);
    }

    public static bool IsRightHandClosed()
    {
        return IsHandClosed(RightHand);
    }
    
    public static bool DoesAnyPinchDetectedOnLeftHand()
    {
        return DoesAnyPinchDetectedOnHand(LeftHand);
    }
    public static bool DoesAnyPinchDetectedOnRightHand()
    {
        return DoesAnyPinchDetectedOnHand(RightHand);
    }
    public static bool DoesAnyPinchDetectedOnHand(GameObject hand)
    {
        if (hand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return false;
        float min_dist = 0.2f;
        if (GetDistanceFromFingers(hand.GetComponent<OVRSkeleton>().Bones[11], hand.GetComponent<OVRSkeleton>().Bones[5]) <= min_dist
          || GetDistanceFromFingers(hand.GetComponent<OVRSkeleton>().Bones[8], hand.GetComponent<OVRSkeleton>().Bones[5]) <= min_dist
          || GetDistanceFromFingers(hand.GetComponent<OVRSkeleton>().Bones[14], hand.GetComponent<OVRSkeleton>().Bones[5]) <= min_dist
          //|| GetDistanceFromFingers(hand.GetComponent<OVRSkeleton>().Bones[18], hand.GetComponent<OVRSkeleton>().Bones[5]) <= min_dist // Pinky
          )
        {
            return true;
        }
        else
            return false;
    }
    public static float GetDistanceFromFingers(OVRBone fingerA, OVRBone fingerB)
    {
        float dist = Vector3.Distance(fingerA.Transform.position, fingerB.Transform.position);
        dist = Mathf.Abs(dist);
        return dist;
    }


    public static bool DoesLeftFingerPointingObject_RayCast(GameObject o)
    {
        GameObject hand = LeftHand;
        if (hand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return false;
        RaycastHit hit;
        Ray ray = new Ray(hand.GetComponent<OVRSkeleton>().Bones[7].Transform.position, hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position);
        if (Physics.Raycast(ray, out hit, 50f))
        {
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == o)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool DoesLeftFingerPointingObject_SphereCast(GameObject o, float radius = 1f, float distance = 10f)
    {
        if (!IsLeftHandTracked())
            return false;

        GameObject hand = LeftHand; 
        if (hand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return false;
        RaycastHit[] hits;

        Vector3 fromPosition = hand.GetComponent<OVRSkeleton>().Bones[7].Transform.position;
        Vector3 toPosition = hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;
        Vector3 direction = toPosition - fromPosition;
        Ray ray = new Ray(fromPosition, direction);
        hits = Physics.SphereCastAll(ray, radius, distance);

        List<GameObject> allchilds = new List<GameObject>();
        ObjectUtilities.GetChildsFromParent(o, allchilds); // recursive loop
        foreach (GameObject g in allchilds)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.gameObject == g)
                    return true;
            }
        }

        return false;
    }

    public static RaycastHit[] GetObjectTouchedByRightFinger(float radius = 0.02f)
    {
        GameObject hand = RightHand;
        if (hand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return null;
        RaycastHit[] hits;
        Ray ray = new Ray(hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position, new Vector3(0,0,0));
        hits = Physics.SphereCastAll(ray, radius, 0.00001f);
        return hits;

    }

    public static RaycastHit IsObjectTouchedByHand(GameObject hand, GameObject go, float radius = 0.02f)
    {
        if (hand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return new RaycastHit();
        Vector3 fromPosition = hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;
        Vector3 toPosition = hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;
        Vector3 direction = toPosition - fromPosition;

        RaycastHit[] hits;
        Ray ray = new Ray(fromPosition, direction);
        hits = Physics.SphereCastAll(ray, radius, 0.00001f);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == go)
                return hit;
        }
        return new RaycastHit();
    }
    public static RaycastHit DoesRightFingerTouchingObject_SphereCast(GameObject o, float radius = 0.2f)
    {
       

        GameObject hand = RightHand;
        if (hand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return new RaycastHit();

        RaycastHit[] hits;
        Vector3 fromPosition = hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;
        Vector3 toPosition = hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;
        Vector3 direction = toPosition - fromPosition;
        Ray ray = new Ray(fromPosition, direction);
        hits = Physics.SphereCastAll(ray, radius, 0.00001f);

        List<GameObject> allchilds = new List<GameObject>();
        ObjectUtilities.GetChildsFromParent(o, allchilds); // recursive loop
        foreach (GameObject g in allchilds)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.gameObject == g)
                    return hits[i];
            }
        }
        return new RaycastHit();
    }

    public static bool DoesRightFingerPointingObject_SphereCast(GameObject o, float radius = 1f, float distance = 10f)
    {
        if (!IsRightHandTracked())
            return false;

        GameObject hand = RightHand;
        if (hand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return false;
        RaycastHit[] hits;
        Vector3 fromPosition = hand.GetComponent<OVRSkeleton>().Bones[7].Transform.position;
        Vector3 toPosition = hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;
        Vector3 direction = toPosition - fromPosition;
        Ray ray = new Ray(fromPosition, direction);
        hits = Physics.SphereCastAll(ray, radius, distance);

        List<GameObject> allchilds = new List<GameObject>();
        ObjectUtilities.GetChildsFromParent(o, allchilds); // recursive loop
        foreach (GameObject g in allchilds)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.gameObject == g)
                    return true;
            }
        }
        return false;
    }




}
