using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// the old thing 
public class BodyAnimationB : MonoBehaviour
{
    /*
     so lets resume we need 2 script. 

     One on OVRrig which update gameobject of our instantiate avatar. ( head, lefthand, righthand and other stuff. ) 
     One on each instantiate Avatar who do IK and updating pos, rot, finger of the model.

      -_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ ALL CS FILES -_-_-_-_-_-_-_-_-_-_-_-_-_-_-
     
    [0] BodyAnimation.cs (IK, Transform Update, Animator from Instantiate Object) on FG3Dchar_DeuHumans
    [1] RigInput.cs ( For updating body animation, start detecting hand stuff, apply thing to environnement ) 
    ok[2] NetMaster.cs (connecting, unconnecting to room etc.) on an alone object
    ok[3] DataReceiver.cs (updating world, client & server services... ) on each instantiate avatar
    ok[4] ModifMesh.cs ( a file apply to all 3DMESH tag object for interacting ) on each object which can be modified
    [&5]
    [&6]
    [&8] OverDub.cs (loop and record mechanism of body movement in the cyber_cave )

     */
    // get my camera rig 
    GameObject OVRrig;
    GameObject lefthandAnchor;
    GameObject righthandAnchor;
    GameObject lefthand;
    GameObject righthand;
    GameObject HeadObj;
    GameObject Center; 

    protected Animator animator;
    public bool ikActive = true; // set to true

    List<GameObject> AvatarBones = new List<GameObject>(); // finger bones corresponding for avatarbones
    List<GameObject> debugBonesObject = new List<GameObject>();

    
    void Start()
    {
        OVRrig = GameObject.Find("OVRCameraRig"); 
        lefthandAnchor = GameObject.Find("LeftHandAnchor");
        righthandAnchor = GameObject.Find("RightHandAnchor");
        lefthand = GameObject.Find("OVRlefthand");
        righthand = GameObject.Find("OVRrighthand");
        HeadObj = GameObject.Find("CenterEyeAnchor");

        animator = GetComponent<Animator>();

        float dist = Vector3.Distance(new Vector3(0, GameObject.Find("head").transform.position.y, 0), new Vector3(0, GameObject.Find("rFoot").transform.position.y));
        // give me 1.6
        Debug.Log(dist);
        // prepare hand

        // so now apply all cube position to cutsom list of shit 
        InitFingerBones();
        foreach ( GameObject go in AvatarBones)
        {
            if ( go == null)
            {
                Debug.Log("not found");

            }
        }
    }

    void InitFingerBones() // raw stuff here for finger  bones ... 
    {

        AvatarBones.Add(GameObject.Find("rHand"));// root frame of the hand, where the wrist is located
        AvatarBones.Add(GameObject.Find("rHand")); //frame for user's forearm stub

        AvatarBones.Add(GameObject.Find("rThumb1")); //thumb trapezium bone
        AvatarBones.Add(GameObject.Find("rThumb1")); //thumb metacarpal bone
        AvatarBones.Add(GameObject.Find("rThumb2")); //thumb proximal phalange bone
        AvatarBones.Add(GameObject.Find("rThumb3")); //thumb distal phalange bone

        AvatarBones.Add(GameObject.Find("rIndex1"));//index proximal phalange bone
        AvatarBones.Add(GameObject.Find("rIndex2"));//index intermediate phalange bone
        AvatarBones.Add(GameObject.Find("rIndex3"));//index distal phalange bone

        AvatarBones.Add(GameObject.Find("rMid1"));// middle proximal phalange bone
        AvatarBones.Add(GameObject.Find("rMid2"));// middle intermediate phalange bone
        AvatarBones.Add(GameObject.Find("rMid3"));////middle distal phalange bone

        AvatarBones.Add(GameObject.Find("rRing1"));// ring proximal phalange bone
        AvatarBones.Add(GameObject.Find("rRing2"));// ring intermediate phalange bone
        AvatarBones.Add(GameObject.Find("rRing3"));// ring distal phalange bone

        AvatarBones.Add(GameObject.Find("rCarpal4"));// pinky metacarpal bone
        AvatarBones.Add(GameObject.Find("rPinky1"));// pinky proximal phalange bone
        AvatarBones.Add(GameObject.Find("rPinky2"));// // pinky intermediate phalange bone
        AvatarBones.Add(GameObject.Find("rPinky3"));// pinky distal phalange bone

        //----------------------

        AvatarBones.Add(GameObject.Find("lHand"));// root frame of the hand, where the wrist is located
        AvatarBones.Add(GameObject.Find("lHand")); //frame for user's forearm stub

        AvatarBones.Add(GameObject.Find("lThumb1")); //thumb trapezium bone
        AvatarBones.Add(GameObject.Find("lThumb1")); //thumb metacarpal bone
        AvatarBones.Add(GameObject.Find("lThumb2")); //thumb proximal phalange bone
        AvatarBones.Add(GameObject.Find("lThumb3")); //thumb distal phalange bone

        AvatarBones.Add(GameObject.Find("lIndex1"));//index proximal phalange bone
        AvatarBones.Add(GameObject.Find("lIndex2"));//index intermediate phalange bone
        AvatarBones.Add(GameObject.Find("lIndex3"));//index distal phalange bone

        AvatarBones.Add(GameObject.Find("lMid1"));// middle proximal phalange bone
        AvatarBones.Add(GameObject.Find("lMid2"));// middle intermediate phalange bone
        AvatarBones.Add(GameObject.Find("lMid3"));////middle distal phalange bone

        AvatarBones.Add(GameObject.Find("lRing1"));// ring proximal phalange bone
        AvatarBones.Add(GameObject.Find("lRing2"));// ring intermediate phalange bone
        AvatarBones.Add(GameObject.Find("lRing3"));// ring distal phalange bone

        AvatarBones.Add(GameObject.Find("lCarpal4"));// pinky metacarpal bone
        AvatarBones.Add(GameObject.Find("lPinky1"));// pinky proximal phalange bone
        AvatarBones.Add(GameObject.Find("lPinky2"));// // pinky intermediate phalange bone
        AvatarBones.Add(GameObject.Find("lPinky3"));// pinky distal phalange bone
    }

    //a callback for calculating IK
    void OnAnimatorIK()
    {
        if (animator)
        {
            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {
                
                if (righthand.GetComponent<OVRSkeleton>().Bones[0].Transform != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, righthand.GetComponent<OVRSkeleton>().Bones[0].Transform.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, righthand.GetComponent<OVRSkeleton>().Bones[0].Transform.rotation);

                }
                if (lefthand.GetComponent<OVRSkeleton>().Bones[0].Transform != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, lefthand.GetComponent<OVRSkeleton>().Bones[0].Transform.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, lefthand.GetComponent<OVRSkeleton>().Bones[0].Transform.rotation);

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


    void DebugShowFingers() // seems ok for the moment. do some invert kinematics also 

    {
        if ( debugBonesObject.Count == 0)
        {
            // max 19 first
            for (int i = 0; i < 19; i++)
            {
               // GameObject inst = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //inst.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                //debugBonesObject.Add(inst);
            }
        }
        for (int i = 0; i < 19; i++)
        {
            if (righthand.GetComponent<OVRHand>().IsTracked) // right hand has been detected
            {
                //debugBonesObject[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position;
               if (AvatarBones[i] != null)
                {
                    AvatarBones[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.rotation;
                    AvatarBones[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position; // i dont under
                }
                    
            }
        }
        for (int i = 0; i < 19; i++)
        {
            if (righthand.GetComponent<OVRHand>().IsTracked) // right hand has been detected
            {
                //debugBonesObject[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position;
                if (AvatarBones[i+19] != null)
                {
                    AvatarBones[i+19].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.rotation; // weird rot should be flip by 180°
                    AvatarBones[i+19].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position; // i dont under
                }

            }
        }



    }
    /*
     Invalid          = -1
Hand_Start       = 0
Hand_WristRoot   = Hand_Start + 0 // root frame of the hand, where the wrist is located
Hand_ForearmStub = Hand_Start + 1 // frame for user's forearm
Hand_Thumb0      = Hand_Start + 2 // thumb trapezium bone
Hand_Thumb1      = Hand_Start + 3 // thumb metacarpal bone
Hand_Thumb2      = Hand_Start + 4 // thumb proximal phalange bone
Hand_Thumb3      = Hand_Start + 5 // thumb distal phalange bone
Hand_Index1      = Hand_Start + 6 // index proximal phalange bone
Hand_Index2      = Hand_Start + 7 // index intermediate phalange bone
Hand_Index3      = Hand_Start + 8 // index distal phalange bone
Hand_Middle1     = Hand_Start + 9 // middle proximal phalange bone
Hand_Middle2     = Hand_Start + 10 // middle intermediate phalange bone
Hand_Middle3     = Hand_Start + 11 // middle distal phalange bone
Hand_Ring1       = Hand_Start + 12 // ring proximal phalange bone
Hand_Ring2       = Hand_Start + 13 // ring intermediate phalange bone
Hand_Ring3       = Hand_Start + 14 // ring distal phalange bone
Hand_Pinky0      = Hand_Start + 15 // pinky metacarpal bone
Hand_Pinky1      = Hand_Start + 16 // pinky proximal phalange bone
Hand_Pinky2      = Hand_Start + 17 // pinky intermediate phalange bone
Hand_Pinky3      = Hand_Start + 18 // pinky distal phalange bone
Hand_MaxSkinnable= Hand_Start + 19
// Bone tips are position only. They are not used for skinning but are useful for hit-testing.
// NOTE: Hand_ThumbTip == Hand_MaxSkinnable since the extended tips need to be contiguous
Hand_ThumbTip    = Hand_Start + Hand_MaxSkinnable + 0 // tip of the thumb
Hand_IndexTip    = Hand_Start + Hand_MaxSkinnable + 1 // tip of the index finger
Hand_MiddleTip   = Hand_Start + Hand_MaxSkinnable + 2 // tip of the middle finger
Hand_RingTip     = Hand_Start + Hand_MaxSkinnable + 3 // tip of the ring finger
Hand_PinkyTip    = Hand_Start + Hand_MaxSkinnable + 4 // tip of the pinky
Hand_End         = Hand_Start + Hand_MaxSkinnable + 5
Max              = Hand_End + 0
     */
    void Update()
    {
       //DebugShowFingers(); // :)
    }

    private void LateUpdate() // occurs after animation
    {
        float dist = 0f;//float dist = Vector3.Distance(new Vector3(0, HeadObj.transform.position.y, 0), new Vector3(0, Center.transform.position.y));
       

        float bodyheight = 0.4311793f - 0.05438308f;
        if (dist > 0)
        {
            float prct = bodyheight / dist;
            dist *= prct; // en l'augmentant je passe au dessus de ma tete
        }
        dist = 1.7f; // 2f trop haut, faire 1.7 [OK]
        this.transform.position = new Vector3(HeadObj.transform.position.x, HeadObj.transform.position.y - dist, HeadObj.transform.position.z); // 0.7 trop haut

        this.transform.position -= this.transform.forward * 0.2f; // valeur a changer 0.2 niquel ?
                                                                  //[3] adjust rotation
                                                                  // get the head rotation 

        DebugShowFingers(); // :)
    }
}
