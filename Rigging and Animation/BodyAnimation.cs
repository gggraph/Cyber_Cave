using System.Collections.Generic;
using UnityEngine;

public class BodyAnimation : MonoBehaviour
{
    public List<GameObject> mAnchors = new List<GameObject>();
    public List<GameObject> fingerBones = new List<GameObject>(); // finger bones corresponding for avatarbones
    public GameObject DeuHumans;
    public GameObject DeuLEye;
    public GameObject DeuREye;
    public bool rigHumanGenerator = false;

    public GameObject LTouchObject;
    public GameObject RTouchObject;

    public Vector3 fingerHandRotation = new Vector3(0f, 0f, -90f);
    public Vector3 fingerGlobalotation = new Vector3(90f, 0f, -90f);

    protected Animator animator;
    public bool ikActive = true; // set to true
    public GameObject _p;
    public string CurrentAnimation = "Idle";

    public bool LTouchTracked = false;
    public bool RTouchTracked = false;
    public bool HandTracked = false;

    public float interPolationSpeed = 10f;

    void Start()
    {
        _p = this.transform.parent.gameObject;
        DeuHumans = this.gameObject;// ObjectUtilities.FindGameObjectChild(_p, "AVATARMODEL"); 
        animator = DeuHumans.GetComponent<Animator>();
        ikActive = true;
        GetAnchors();
        GetHumanBones();

        RTouchObject = ObjectUtilities.FindGameObjectChild(_p, "OculusTouchForQuest2RightModel");
        LTouchObject = ObjectUtilities.FindGameObjectChild(_p, "OculusTouchForQuest2LeftModel");
        

    }

    void GetAnchors()
    {
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rig")); //i:0
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_head"));
        // R anchors
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rHand"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rThumb1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rThumb2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rThumb3")); //i:5

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rIndex1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rIndex2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rIndex3"));

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rMid1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rMid2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rMid3")); //i:11

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rRing1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rRing2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rRing3"));

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rCarpal4"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rPinky1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rPinky2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rPinky3")); //i:18

        // L anchors
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lHand"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lThumb1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lThumb2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lThumb3")); //i:22

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lIndex1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lIndex2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lIndex3"));

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lMid1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lMid2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lMid3"));//i:28

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lRing1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lRing2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lRing3"));//i:31

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lCarpal4"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lPinky1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lPinky2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_lPinky3"));//i:35

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_ltouch"));//i:36
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(_p, "anc_rtouch"));//i:37

        int ctr = 0;
        foreach (GameObject go in mAnchors)
        {
            if (go == null)
            {
                Debug.Log(ctr + " was null");
                return;
            }
            ctr++;
        }
        Debug.Log("all anchors found !");
    }

    public void SetTrackingStatus(byte b)
    {
        switch (b)
        {
            case 0: // both control is untracked
                LTouchTracked = false;
                RTouchTracked = false;
                HandTracked = false;
                break;
            case 1: // Both L/R touch is tracked
                LTouchTracked = true;
                RTouchTracked = true;
                HandTracked = false;
                break;
            case 2: // Only L touch is tracked
                LTouchTracked = true;
                RTouchTracked = false;
                HandTracked = false;
                break;
            case 3: // Only R touch is tracked
                LTouchTracked = false;
                RTouchTracked = true;
                HandTracked = false;
                break;
            case 4: // Hand is tracked
                LTouchTracked = false;
                RTouchTracked = false;
                HandTracked = true;
                break;
        }
    }

    void GetHumanBones()
    {
        // Get Eye.
        if (!rigHumanGenerator)
        {
            DeuLEye = ObjectUtilities.FindGameObjectChild(_p, "lEye");
            DeuREye = ObjectUtilities.FindGameObjectChild(_p, "rEye");
        }
        else
        {
            DeuLEye = ObjectUtilities.FindGameObjectChild(_p, "eye_settings.L");
            DeuREye = ObjectUtilities.FindGameObjectChild(_p, "eye_settings.R");
        }

        /* 
         * 
         * _________________________ POUR LE MODELE 3D DE BLENDER _________________________ 
         * 
         * 
        // R anchors
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "hand.R"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "thumb.01.R"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "thumb.02.R"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "thumb.03.R"));

        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_index.01.R"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_index.02.R"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_index.03.R"));

        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_middle.02.R"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_middle.02.R"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_middle.03.R"));

        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_ring.01.R"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_ring.02.R"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_ring.03.R"));

        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "palm.04.R"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_pinky.01.R"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_pinky.02.R"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_pinky.03.R"));

        // L anchors
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "hand.L"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "thumb.01.L"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "thumb.02.L"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "thumb.03.L"));

        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_index.01.L"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_index.02.L"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_index.03.L"));

        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_middle.01.L"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_middle.02.L"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_middle.03.L"));

        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_ring.01.L"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_ring.02.L"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_ring.03.L"));

        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "palm.04.L"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_pinky.01.L"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_pinky.02.L"));
        fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_pinky.03.L"));
        */


        // _________________________ MODELE NORMAL  _________________________

        if (!rigHumanGenerator)
        {
            fingerBones.Add(null);  // we dont need the head so ... ? so we start at +2
            fingerBones.Add(null);
            // R anchors
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rHand"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rThumb1"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rThumb2"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rThumb3")); //i:5

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rIndex1"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rIndex2"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rIndex3"));

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rMid1"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rMid2"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rMid3")); // i:11

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rRing1"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rRing2"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rRing3"));

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rCarpal4"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rPinky1"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rPinky2"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "rPinky3")); //i :18

            // L anchors
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lHand"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lThumb1"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lThumb2"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lThumb3"));

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lIndex1"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lIndex2"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lIndex3")); //i:25

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lMid1"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lMid2"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lMid3"));

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lRing1"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lRing2"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lRing3"));

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lCarpal4"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lPinky1"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lPinky2"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "lPinky3"));
        }
        else
        {
            fingerBones.Add(null);  // we dont need the head so ... ? so we start at +2
            fingerBones.Add(null);
            // R anchors
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "hand.R"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "thumb.01.R"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "thumb.02.R"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "thumb.03.R")); //i:5

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_index.01.R"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_index.02.R"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_index.03.R"));

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_middle.01.R"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_middle.02.R"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_middle.03.R")); // i:11

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_ring.01.R"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_ring.02.R"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_ring.03.R"));

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "palm.04.R"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_pinky.01.R"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_pinky.02.R"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_pinky.03.R")); //i :18

            // L anchors
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "hand.L"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "thumb.01.L"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "thumb.02.L"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "thumb.03.L")); //i:5

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_index.01.L"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_index.02.L"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_index.03.L"));

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_middle.01.L"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_middle.02.L"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_middle.03.L")); // i:11

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_ring.01.L"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_ring.02.L"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_ring.03.L"));

            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "palm.04.L"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_pinky.01.L"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_pinky.02.L"));
            fingerBones.Add(ObjectUtilities.FindGameObjectChild(_p, "f_pinky.03.L")); //i :18
        }


    }

    private const int HAND_RIGHT = 2;
    private const int HAND_LEFT = 19;

    void UpdateFingers()
    {
        for (int i = 2; i < 35; i++)
        {
            fingerBones[i].transform.rotation = mAnchors[i].transform.rotation;

            switch (i)
            {
                case HAND_RIGHT:
                case HAND_LEFT:
                    fingerBones[i].transform.Rotate(fingerHandRotation);
                    break;
                default:
                    fingerBones[i].transform.Rotate(fingerGlobalotation);
                    break;
            }

            if (i > 18) // this is hacky ...
            {
                fingerBones[i].transform.Rotate(180, 0, 0);
            }
            fingerBones[i].transform.position = mAnchors[i].transform.position;


        }
    }

    // this only work on the current transform 
    void OnAnimatorIK()
    {

        // set basic animation  
        if (animator)
        {
            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {
                // Set the look target position, if one has been assigned
                if (mAnchors[1] != null)
                {
                    animator.SetLookAtWeight(1);
                    Vector3 lookPosition = mAnchors[1].transform.position + mAnchors[1].transform.forward * 2.0f; // OK

                    animator.SetLookAtPosition(lookPosition);
                }

                // Set the right hand target position and rotation, if one has been assigned
                if (mAnchors[1] != null && (RTouchTracked || HandTracked))
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    if (HandTracked)
                    {
                        animator.SetIKPosition(AvatarIKGoal.RightHand, mAnchors[2].transform.position);
                        animator.SetIKRotation(AvatarIKGoal.RightHand, mAnchors[2].transform.rotation);
                    }
                    else if (RTouchTracked)
                    {

                        Vector3 bpos = mAnchors[37].transform.position;
                        Vector3 brot = mAnchors[37].transform.eulerAngles;

                        mAnchors[37].transform.Rotate(0, 0, -90);
                        mAnchors[37].transform.position += mAnchors[37].transform.forward * (-0.08f);
                        // mAnchors[36].transform.Rotate(-90, 0, 0);
                        animator.SetIKPosition(AvatarIKGoal.RightHand, mAnchors[37].transform.position);
                        animator.SetIKRotation(AvatarIKGoal.RightHand, mAnchors[37].transform.rotation);
                        mAnchors[37].transform.eulerAngles = brot;
                        mAnchors[37].transform.position = bpos;


                    }


                }
                if (mAnchors[18] != null && (LTouchTracked || HandTracked))
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    if (HandTracked)
                    {
                        animator.SetIKPosition(AvatarIKGoal.LeftHand, mAnchors[19].transform.position);
                        animator.SetIKRotation(AvatarIKGoal.LeftHand, mAnchors[19].transform.rotation);
                    }
                    else if (LTouchTracked)
                    {
                        // apply some offset to manchors 36 ... 
                        Vector3 bpos = mAnchors[36].transform.position;
                        Vector3 brot = mAnchors[36].transform.eulerAngles;

                        mAnchors[36].transform.Rotate(0, 0, 90);
                        mAnchors[36].transform.position += mAnchors[36].transform.forward * (-0.08f);
                        // mAnchors[36].transform.Rotate(-90, 0, 0);
                        animator.SetIKPosition(AvatarIKGoal.LeftHand, mAnchors[36].transform.position);
                        animator.SetIKRotation(AvatarIKGoal.LeftHand, mAnchors[36].transform.rotation);
                        mAnchors[36].transform.eulerAngles = brot;
                        mAnchors[36].transform.position = bpos;
                    }


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

        Vector3 DeuEyesCenter = Vector3.Lerp(DeuLEye.transform.position, DeuREye.transform.position, 0.5f);
        // Get Offset Between DeuEyesCenter and mAnchors[1] (which is head) 
        Vector3 offset = mAnchors[1].transform.position - DeuEyesCenter;
        DeuHumans.transform.position += offset;
        DeuHumans.transform.rotation = mAnchors[0].transform.rotation;
       
        //[3] Update Human Fingers
        if (HandTracked)
            UpdateFingers();

        //[4] Set Touch transform
        if (LTouchTracked)
        {
            LTouchObject.transform.eulerAngles = mAnchors[36].transform.eulerAngles;
            LTouchObject.transform.position = mAnchors[36].transform.position;
        }
        if (RTouchTracked)
        {
            RTouchObject.transform.eulerAngles = mAnchors[37].transform.eulerAngles;
            RTouchObject.transform.position = mAnchors[37].transform.position;
        }

        
        
    }
}
