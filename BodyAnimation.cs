﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyAnimation: MonoBehaviour
{
    public List<GameObject> mAnchors = new List<GameObject>();
    public List<GameObject> fingerBones = new List<GameObject>(); // finger bones corresponding for avatarbones
    public GameObject DeuHumans;
    protected Animator animator;
    public bool ikActive = true; // set to true
    public GameObject _p;

    void Start()
    {
        _p = this.transform.parent.gameObject;
        DeuHumans = this.gameObject;//FindGameObjectChild(_p, "FG3D_Char_DeuHumans");
        animator = DeuHumans.GetComponent<Animator>();
        ikActive = true;
        GetAnchors();
        GetFingerBones();
    }


    void GetAnchors()
    {
        mAnchors.Add(FindGameObjectChild(_p, "anc_head"));
        // R anchors
        mAnchors.Add(FindGameObjectChild(_p, "anc_rHand"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_rThumb1"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_rThumb2"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_rThumb3"));

        mAnchors.Add(FindGameObjectChild(_p, "anc_rIndex1"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_rIndex2"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_rIndex3"));

        mAnchors.Add(FindGameObjectChild(_p, "anc_rMid1"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_rMid2"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_rMid3"));

        mAnchors.Add(FindGameObjectChild(_p, "anc_rRing1"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_rRing2"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_rRing3"));

        mAnchors.Add(FindGameObjectChild(_p, "anc_rCarpal4"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_rPinky1"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_rPinky2"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_rPinky3"));

        // L anchors
        mAnchors.Add(FindGameObjectChild(_p, "anc_lHand"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_lThumb1"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_lThumb2"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_lThumb3"));

        mAnchors.Add(FindGameObjectChild(_p, "anc_lIndex1"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_lIndex2"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_lIndex3"));

        mAnchors.Add(FindGameObjectChild(_p, "anc_lMid1"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_lMid2"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_lMid3"));

        mAnchors.Add(FindGameObjectChild(_p, "anc_lRing1"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_lRing2"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_lRing3"));

        mAnchors.Add(FindGameObjectChild(_p, "anc_lCarpal4"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_lPinky1"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_lPinky2"));
        mAnchors.Add(FindGameObjectChild(_p, "anc_lPinky3"));
        int ctr = 0;
        foreach ( GameObject go in mAnchors)
        {
            if ( go == null)
            {
                Debug.Log(ctr + " was null");
            }
            ctr++;
        }
    }

    void GetFingerBones()
    {
        fingerBones.Add(null);  // we dont need the head so ... ? so we start at +1 
        // R anchors
        fingerBones.Add(FindGameObjectChild(_p, "rHand"));
        fingerBones.Add(FindGameObjectChild(_p, "rThumb1"));
        fingerBones.Add(FindGameObjectChild(_p, "rThumb2"));
        fingerBones.Add(FindGameObjectChild(_p, "rThumb3"));

        fingerBones.Add(FindGameObjectChild(_p, "rIndex1"));
        fingerBones.Add(FindGameObjectChild(_p, "rIndex2"));
        fingerBones.Add(FindGameObjectChild(_p, "rIndex3"));

        fingerBones.Add(FindGameObjectChild(_p, "rMid1"));
        fingerBones.Add(FindGameObjectChild(_p, "rMid2"));
        fingerBones.Add(FindGameObjectChild(_p, "rMid3"));

        fingerBones.Add(FindGameObjectChild(_p, "rRing1"));
        fingerBones.Add(FindGameObjectChild(_p, "rRing2"));
        fingerBones.Add(FindGameObjectChild(_p, "rRing3"));

        fingerBones.Add(FindGameObjectChild(_p, "rCarpal4"));
        fingerBones.Add(FindGameObjectChild(_p, "rPinky1"));
        fingerBones.Add(FindGameObjectChild(_p, "rPinky2"));
        fingerBones.Add(FindGameObjectChild(_p, "rPinky3"));

        // L anchors
        fingerBones.Add(FindGameObjectChild(_p, "lHand"));
        fingerBones.Add(FindGameObjectChild(_p, "lThumb1"));
        fingerBones.Add(FindGameObjectChild(_p, "lThumb2"));
        fingerBones.Add(FindGameObjectChild(_p, "lThumb3"));

        fingerBones.Add(FindGameObjectChild(_p, "lIndex1"));
        fingerBones.Add(FindGameObjectChild(_p, "lIndex2"));
        fingerBones.Add(FindGameObjectChild(_p, "lIndex3"));

        fingerBones.Add(FindGameObjectChild(_p, "lMid1"));
        fingerBones.Add(FindGameObjectChild(_p, "lMid2"));
        fingerBones.Add(FindGameObjectChild(_p, "lMid3"));

        fingerBones.Add(FindGameObjectChild(_p, "lRing1"));
        fingerBones.Add(FindGameObjectChild(_p, "lRing2"));
        fingerBones.Add(FindGameObjectChild(_p, "lRing3"));

        fingerBones.Add(FindGameObjectChild(_p, "lCarpal4"));
        fingerBones.Add(FindGameObjectChild(_p, "lPinky1"));
        fingerBones.Add(FindGameObjectChild(_p, "lPinky2"));
        fingerBones.Add(FindGameObjectChild(_p, "lPinky3"));
    }
    GameObject FindGameObjectChild(GameObject fParent, string name)
    {


        List<GameObject> allchilds = new List<GameObject>();
        allchilds = GetChildsFromParent(fParent, allchilds); // recursive loop
        
        foreach (GameObject go in allchilds)
        {
            if (go.name == name)
            {
                return go.gameObject;
            }

        }
        return null;

    }

    List<GameObject> GetChildsFromParent(GameObject Parent, List<GameObject> aChild)
    {
  
        aChild.Add(Parent.gameObject);
        for (int a = 0; a < Parent.transform.childCount; a++)
        {

            aChild.Add(Parent.transform.GetChild(a).gameObject);
            if (Parent.transform.GetChild(a).transform.childCount > 0)
            {
                aChild = GetChildsFromParent(Parent.transform.GetChild(a).gameObject, aChild); // recursive loop
            }
        }
        return aChild;
    }

    void UpdateFingers()
    {
        for (int i = 1; i < 34; i++)
        {
            fingerBones[i].transform.rotation = mAnchors[i].transform.rotation;
            fingerBones[i].transform.position = mAnchors[i].transform.position;
        }
    }

    // this only work on the current transform 
    void OnAnimatorIK()
    {
        if (animator)
        {
            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {

                Debug.Log("ok");
                // Set the look target position, if one has been assigned
                if (mAnchors[0] != null)
                {
                    animator.SetLookAtWeight(1);
                    Vector3 lookPosition = mAnchors[0].transform.position + mAnchors[0].transform.forward * 2.0f;
                    animator.SetLookAtPosition(lookPosition);
                }

                // Set the right hand target position and rotation, if one has been assigned
                if (mAnchors[1] != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, mAnchors[1].transform.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, mAnchors[1].transform.rotation);

                }
                if (mAnchors[18] != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, mAnchors[18].transform.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, mAnchors[18].transform.rotation);

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


    private void LateUpdate()
    {
        float dist = 1.7f; // 2f trop haut, faire 1.7 [OK]
        DeuHumans.transform.position = new Vector3(mAnchors[0].transform.position.x, mAnchors[0].transform.position.y - dist, mAnchors[0].transform.position.z); // 0.7 trop haut
        DeuHumans.transform.position -= DeuHumans.transform.forward * 0.2f; // valeur a changer 0.2 niquel ?
        
        //[3] adjust rotation
        UpdateFingers();
    }
}
