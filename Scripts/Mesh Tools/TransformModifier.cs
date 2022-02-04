using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TransformModifier : MonoBehaviour
{
   /*
    Interaction with objects. 
        * Grab with one hand to move an object...
        * Grab Two hand : -> 
    
    */
    MeshModifier Modifier;
    bool _grabbed = false;

    private void Start()
    {
        Modifier = GetComponent<MeshModifier>();
    }

    private void Update()
    {
        if (GameStatus._selectorIsOpen)
            return;

        TryMovingTransform();
    }

    private void TryMovingTransform()
    {
        if ( ToolMod.value > 0)
        {
            return;
        }
        if (Modifier.RightHand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return;

        if (IsBoundariesBelowSize(0.4f))
        {
            if (TryReScaleWithHands())
                return;

            if (TryGrabbingWithHand(Modifier.LeftHand))
                return;
            if (TryGrabbingWithHand(Modifier.RightHand))
                return;

            
            return;
        }
        else
        {
           // Debug.Log("too big");
        }
        
    }
    private bool TryReScaleWithHands() 
    {
        if (_grabbed)
        {
            return false;
           // Debug.Log("grabbed false");
        }
           
        if (!HandRecognition.IsHandClosed(Modifier.RightHand) || !HandRecognition.IsHandClosed(Modifier.LeftHand))
            return false;

        if (!Modifier.maxBoundaries.Contains((Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position)) 
            || !Modifier.maxBoundaries.Contains((Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position)))
        {
            return false;
        }

        int m = -1;

        // get scale system -> X AND Y COND ACTING REALLY WEIRD... SO I NEED TO INVERT DEPENDING OF ROTATION I GUESS
        // check y scaling condition :: 
        float targH = Modifier.maxBoundaries.center.y + Modifier.maxBoundaries.extents.y;
        float targL = Modifier.maxBoundaries.center.y - Modifier.maxBoundaries.extents.y;
        
        if (Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.y >= targH - (0.2f * Modifier.maxBoundaries.size.y)
            && Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.y <= targL + (0.2f * Modifier.maxBoundaries.size.y))
            m = 1;
        if (Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.y >= targH - (0.2f * Modifier.maxBoundaries.size.y)
            && Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.y <= targL + (0.2f * Modifier.maxBoundaries.size.y))
            m = 1;

        targH = Modifier.maxBoundaries.center.x + Modifier.maxBoundaries.extents.x;
        targL = Modifier.maxBoundaries.center.x - Modifier.maxBoundaries.extents.x;

        if (Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.x >= targH - (0.2f * Modifier.maxBoundaries.size.x)
            && Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.x <= targL + (0.2f * Modifier.maxBoundaries.size.x))
            m = 0;
        if (Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.x >= targH - (0.2f * Modifier.maxBoundaries.size.x)
            && Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.x <= targL + (0.2f * Modifier.maxBoundaries.size.x))
            m = 0;

        targH = Modifier.maxBoundaries.center.z + Modifier.maxBoundaries.extents.z;
        targL = Modifier.maxBoundaries.center.z - Modifier.maxBoundaries.extents.z;

        if (Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.z >= targH - (0.2f * Modifier.maxBoundaries.size.z)
            && Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.z <= targL + (0.2f * Modifier.maxBoundaries.size.z))
            m = 2;
        if (Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.z >= targH - (0.2f * Modifier.maxBoundaries.size.z)
            && Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.z <= targL + (0.2f * Modifier.maxBoundaries.size.z))
            m = 2;

        if (m > -1)
            StartCoroutine(ScaleObjectWithHands(Modifier.RightHand, Modifier.LeftHand, m));



        return true;
    }

    private IEnumerator ScaleObjectWithHands(GameObject RightHand, GameObject LeftHand, int Axis) 
    {
        _grabbed = true;
        float odist = 0f;
        float osc = 1f;
        switch (Axis)
        {
            case 0: odist = Mathf.Abs(Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.x
                - Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.x);
                osc = this.transform.localScale.x;
                break;
            case 1:
                odist = Mathf.Abs(Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.y
       - Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.y);
                osc = this.transform.localScale.y;
                break;
            case 2:
                odist = Mathf.Abs(Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.z
       - Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.z);
                osc = this.transform.localScale.z;
                break;
        }
        while (HandRecognition.IsHandClosed(RightHand) && HandRecognition.IsHandClosed(LeftHand))
        {
            float cdist = 0f;
            switch (Axis)
            {
                case 0:
                    cdist = Mathf.Abs(Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.x
                - Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.x);
                    break;
                case 1:
                    cdist = Mathf.Abs(Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.y
           - Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.y);
                    break;
                case 2:
                    cdist = Mathf.Abs(Modifier.RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.z
           - Modifier.LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position.z);
                    break;
            }
            float prct = cdist / odist;
            switch (Axis)
            {
                case 0: this.transform.localScale = new Vector3(osc * prct, this.transform.localScale.y, this.transform.localScale.z); break;
                case 1: this.transform.localScale = new Vector3(this.transform.localScale.x, osc * prct, this.transform.localScale.z); break;
                case 2: this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, osc * prct); break;
            }
            
            yield return new WaitForEndOfFrame();
        }
        Modifier.RecalculateBoundaries();
        _grabbed = false;
        yield break;
    }
    private bool TryGrabbingWithHand(GameObject Hand)
    {
        if (Vector3.Distance(Hand.transform.position, this.transform.position) > 0.7f)
        {
            //Debug.Log("Too much distance. ");
            return false;
        }
        // check if left hand is inside boundaries
        if (!Modifier.maxBoundaries.Contains(Hand.GetComponent<OVRSkeleton>().Bones[9].Transform.position))
        {
            //Debug.Log("not inside Boundaries. ");
            return false;
        }
        // check if hand is closed 
        if (!HandRecognition.IsHandClosed(Hand))
        {
            //Debug.Log("Hand not closed. ");
            return false;
        }

        // start coroutine
        if (!_grabbed)
            StartCoroutine(GrabObjectWithHand(Hand));
        
        return true;
    }

    private IEnumerator GrabObjectWithHand(GameObject Hand)
    {
        Debug.Log("start grabbing object!");
        Transform root = this.transform.root;
        root.parent = Hand.GetComponent<OVRSkeleton>().Bones[9].Transform;
        _grabbed = true;
        while ( HandRecognition.IsHandClosed(Hand))
        {
            yield return new WaitForEndOfFrame();
        }
       
        root.parent = null; // need to save root because its changed by the time...
        _grabbed = false;
        Modifier.RecalculateBoundaries(); // it is not working because object property has changed
        Debug.Log("stop grabbing object!");
        yield break;

    }
  
    private bool IsBoundariesBelowSize(float size)
    {
        if (Modifier.maxBoundaries.size.x < size
            && Modifier.maxBoundaries.size.y < size
            && Modifier.maxBoundaries.size.z < size)
        {
            return true;
        }
        return false;
    }

}
