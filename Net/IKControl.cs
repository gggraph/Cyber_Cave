using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class IKControl : MonoBehaviour
{

    protected Animator animator;

    public bool ikActive = true; // set to true
    public Transform rightHandObj = null; // have to be found somewhere aha !!! 
    public Transform leftHandObj = null; // have to be found somewhere aha !!! 
    public Transform lookObj = null;
    public Transform headTransform = null;
    private GameObject HeadObj;
    private GameObject Center;

   // private GameObject GaelFace;
   // private GameObject LionelFace;


    void Start()
    {
        rightHandObj = this.transform.parent.transform.Find("RightHand");
        leftHandObj = this.transform.parent.transform.Find("LeftHand");
        lookObj = this.transform.parent.transform.Find("RightHand");
        animator = GetComponent<Animator>();
        HeadObj = this.transform.parent.transform.Find("Head").gameObject;
        headTransform = HeadObj.transform;
        Center = this.transform.parent.transform.Find("Center").gameObject;
        //GaelFace = this.transform.parent.transform.Find("GAEL").gameObject;
        //LionelFace = this.transform.parent.transform.Find("Lionel").gameObject;

    }

    //a callback for calculating IK
    void OnAnimatorIK()
    {
        if (animator)
        {
            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {
                // Set the look target position, if one has been assigned
                if (lookObj != null)
                {
                    animator.SetLookAtWeight(1);
                    Vector3 lookPosition = HeadObj.transform.position + HeadObj.transform.forward * 2.0f;
                    animator.SetLookAtPosition(lookPosition);
                }

                // Set the right hand target position and rotation, if one has been assigned
                if (rightHandObj != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
                   
                }
                if (leftHandObj != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
                 
                }

            }

            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);

                animator.SetLookAtWeight(0);
            }
        }
    }


    private void Update()
    {
        //new Vector3(Center.transform.position.x, Center.transform.position.y , Center.transform.position.z);// prendre 'objet XR RIG
        // update body transform : 
        // keep head for positioning. center for right high 
        this.transform.position = new Vector3(HeadObj.transform.position.x, Center.transform.position.y - 0.55f, HeadObj.transform.position.z); // 0.7 trop haut
        this.transform.position -= this.transform.forward * 0.2f; // valeur a changer 0.2 niquel ?
        this.transform.rotation = Center.transform.rotation; 
    }
}
