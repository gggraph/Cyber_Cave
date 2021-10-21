using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class HandGestureR : MonoBehaviour
{
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

    public class HandGesture
    {
        public OVRBone[] fBones = new OVRBone[19];
        public int ID { get;  }
        public byte HandSide { get;  } // 0 for left, 1 for right

        public HandGesture(OVRBone[] os, int id, byte hs)
        {
            this.fBones = os;
            this.ID = id;
            this.HandSide = hs;
        }
    }

    public List<HandGesture> _Poses = new List<HandGesture>();

    public void Start()
    {
        LoadPoses();
    }

    GameObject dbgtext; 
   
    public void LoadPoses()
    {
        _Poses = new List<HandGesture>();

        if ( !File.Exists(Application.persistentDataPath + "/" + "pose"))
        {
            File.WriteAllBytes(Application.persistentDataPath + "/" + "pose", new byte[0]);
        }
        byte[] rawdat = File.ReadAllBytes(Application.persistentDataPath + "/" + "pose");

        int byteOffset = 0; 
        while ( byteOffset < rawdat.Length)
        {
            byte hs = rawdat[byteOffset]; byteOffset++;
            int id = BitConverter.ToInt32(rawdat,byteOffset); byteOffset += 4;
            OVRBone[] candidate = new OVRBone[19];
            for (int i = 0; i < 19; i++)
            {
                OVRBone b = new OVRBone();
                b.Transform.position = new Vector3(BitConverter.ToSingle(rawdat, byteOffset), BitConverter.ToSingle(rawdat, byteOffset+4),
                    BitConverter.ToSingle(rawdat, byteOffset+8));
                byteOffset += 12;
                b.Transform.localEulerAngles = new Vector3(BitConverter.ToSingle(rawdat, byteOffset), BitConverter.ToSingle(rawdat, byteOffset + 4),
                    BitConverter.ToSingle(rawdat, byteOffset + 8));
                byteOffset += 12;
                candidate[i] = b;
            }
            _Poses.Add(new HandGesture(candidate, id, hs));
        }
        Debug.Log("Poses loaded.");
    }

    public void CreatePose(GameObject hand, byte handside)
    {
        OVRBone[] candidate = new OVRBone[19];
        for (int i = 0; i < 19; i++)
        {
            OVRBone b = new OVRBone();
            b.Transform.position = new Vector3(hand.GetComponent<OVRSkeleton>().Bones[i].Transform.position.x,
                hand.GetComponent<OVRSkeleton>().Bones[i].Transform.position.y,
                hand.GetComponent<OVRSkeleton>().Bones[i].Transform.position.z);
            b.Transform.localEulerAngles = new Vector3(hand.GetComponent<OVRSkeleton>().Bones[i].Transform.localEulerAngles.x,
                hand.GetComponent<OVRSkeleton>().Bones[i].Transform.localEulerAngles.y,
                hand.GetComponent<OVRSkeleton>().Bones[i].Transform.localEulerAngles.z);

            candidate[i] = b;  //  referencement ibdirect
            // normalize from bones 0 (whistle)
            candidate[i].Transform.position -= hand.GetComponent<OVRSkeleton>().Bones[0].Transform.position;
        }

        // parsing this and print it 
        List<byte> dataBytes = new List<byte>();
        Utils.AddBytesToList(ref dataBytes, BitConverter.GetBytes(_Poses.Count));
        dataBytes.Add(handside);
        foreach ( OVRBone b in candidate)
        {
            Utils.AddBytesToList(ref dataBytes, BitConverter.GetBytes(b.Transform.position.x));
            Utils.AddBytesToList(ref dataBytes, BitConverter.GetBytes(b.Transform.position.y));
            Utils.AddBytesToList(ref dataBytes, BitConverter.GetBytes(b.Transform.position.z));

            Utils.AddBytesToList(ref dataBytes, BitConverter.GetBytes(b.Transform.localEulerAngles.x));
            Utils.AddBytesToList(ref dataBytes, BitConverter.GetBytes(b.Transform.localEulerAngles.y));
            Utils.AddBytesToList(ref dataBytes, BitConverter.GetBytes(b.Transform.localEulerAngles.z));
        }
        // append it to pose file 
        //(Application.persistentDataPath + "/" + "pose");
        AppendBytesToFile(Application.persistentDataPath + "/" + "pose", dataBytes.ToArray());
        Debug.Log("Bones successfully write");
        // then reload poses
        LoadPoses();
    }

    public int RecognizeGesture(GameObject hand, byte handside)
    {
        // build candidate pose
        OVRBone[] candidate = new OVRBone[19];
        for(int i = 0; i < 19; i++)
        {
            OVRBone b = new OVRBone();
            b.Transform.position = new Vector3(hand.GetComponent<OVRSkeleton>().Bones[i].Transform.position.x,
                hand.GetComponent<OVRSkeleton>().Bones[i].Transform.position.y,
                hand.GetComponent<OVRSkeleton>().Bones[i].Transform.position.z);
            b.Transform.localEulerAngles = new Vector3(hand.GetComponent<OVRSkeleton>().Bones[i].Transform.localEulerAngles.x,
                hand.GetComponent<OVRSkeleton>().Bones[i].Transform.localEulerAngles.y,
                hand.GetComponent<OVRSkeleton>().Bones[i].Transform.localEulerAngles.z);

            candidate[i] = b;  //  referencement ibdirect
            // normalize from bones 0 (whistle)
            candidate[i].Transform.position -= hand.GetComponent<OVRSkeleton>().Bones[0].Transform.position;
        }

        float bestscore = 0;
        int id = 0;
        foreach ( HandGesture hg in _Poses)
        {
            if ( hg.HandSide == handside)
            {
                float score = 100f; // max score
                for (int i = 0; i < 19; i++)
                {
                    float distpos = Vector3.Distance(hg.fBones[i].Transform.position, candidate[i].Transform.position);
                    // abs 
                    if (distpos < 0)
                        distpos = -distpos;

                    score -= distpos; // apply modifier here

                    // do some penality with euler angles
                    /*
                    float distrot = Vector3.Distance(hg.fBones[i].Transform.localEulerAngles, candidate[i].Transform.localEulerAngles);

                    */
                }
                if (score > bestscore)
                {
                    
                    bestscore = score;
                    id = hg.ID;
                }
            }
            
        }
        if (bestscore > 50) // seuil a définir
            return id;
        else
            return 0;


    }
    public static void AppendBytesToFile(string _filePath, byte[] bytes)
    {

        using (FileStream f = new FileStream(_filePath, FileMode.Append))
        {
            f.Write(bytes, 0, bytes.Length);
        }

    }
   

}
