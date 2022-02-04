using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class HandShortKey : MonoBehaviour
{

    /*
     Ok so basically this not work as good as i can think... Because we need to keep the structure of all bones and their position in the space without using distance... 
    so lets make a new HandShortKeyScript at all .. 

    Metacarp offsets are each hand bones position offset relative to its metcarp bone position... 
    BUT... Positions of metacarp bones are only valid if we set hand bone offset ... (if not  
   
    THIS IS NOT WORKING BECAUSE OF ONE THINGS : 
    >       VECTOR OFFSET ARE WRONG. WHY? BECAUSE OF ROTATION ? We need to get parents per parents all transform and rotation to get valid vector pos. Once it is done. 
        
     */

    public class HandsState
    {
        public Vector3[] eulers { get; set; }
        public Vector3[] headOffsets { get; set; }
        public Vector3[] externOffsets { get; set; }
        public Vector3[] LmetacarpOffsets { get; set; }
        public Vector3[] RmetacarpOffsets { get; set; }
        public float[] externDistances { get; set; }
        public float[] LmetacarpDistances { get; set; }
        public float[] RmetacarpDistances { get; set; }

        public HandsState(Vector3[] eul, Vector3[] hoff, Vector3[] externoff, Vector3[] lmetaoff, Vector3[] rmetaoff, 
            float[] exterdist, float[] lmetadist, float[] rmetadist)
        {
            this.eulers = eul;
            this.headOffsets = hoff;
            this.externOffsets = externoff;
            this.LmetacarpOffsets = lmetaoff;
            this.RmetacarpOffsets = rmetaoff;
            this.externDistances = exterdist;
            this.LmetacarpDistances = lmetadist;
            this.RmetacarpDistances = rmetadist;
        }
    }
    public static HandsState BinaryToHandState(byte[] bin) 
    {
        Vector3[] eulers = new Vector3[2];
        eulers[0] = BinaryUtilities.BytesToVector3(ref bin, 0);
        eulers[0] = BinaryUtilities.BytesToVector3(ref bin, 12);

        Vector3[] headOffsets = new Vector3[2];
        headOffsets[0] = BinaryUtilities.BytesToVector3(ref bin, 24);
        headOffsets[0] = BinaryUtilities.BytesToVector3(ref bin, 36);
        int boff = 48;

        Vector3[] externOffsets = new Vector3[5];
        for (int i = 0; i < 5; i ++)
        {
            externOffsets[i] = BinaryUtilities.BytesToVector3(ref bin, boff);
            boff += 12;
        }
        Vector3[] LmetacarpOffsets = new Vector3[16];
        for (int i = 0; i < 16; i++)
        {
            LmetacarpOffsets[i] = BinaryUtilities.BytesToVector3(ref bin, boff);
            boff += 12;
        }
        Vector3[] RmetacarpOffsets = new Vector3[16];
        for (int i = 0; i < 16; i++)
        {
            RmetacarpOffsets[i] = BinaryUtilities.BytesToVector3(ref bin, boff);
            boff += 12;
        }
        float[] externDistances = new float[5];
        for (int i = 0; i < 5; i++)
        {
            externDistances[i] = BitConverter.ToSingle(bin, boff);
            boff += 4;
        }
        float[] LmetacarpDistances = new float[16];
        for (int i = 0; i < 16; i++)
        {
            LmetacarpDistances[i] = BitConverter.ToSingle(bin, boff);
            boff += 4;
        }
        float[] RmetacarpDistances = new float[16];
        for (int i = 0; i < 16; i++)
        {
            RmetacarpDistances[i] = BitConverter.ToSingle(bin, boff);
            boff += 4;
        }
        return new HandsState(eulers, headOffsets, externOffsets, LmetacarpOffsets,
            RmetacarpOffsets, externDistances, LmetacarpDistances, RmetacarpDistances) ;
    }
    public static byte[] HandStateToBinary(HandsState HS)
    {
        List<byte> data = new List<byte>();

        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(HS.eulers[0]));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(HS.eulers[1]));

        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(HS.headOffsets[0]));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(HS.headOffsets[1]));

        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(HS.externOffsets[0]));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(HS.externOffsets[1]));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(HS.externOffsets[2]));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(HS.externOffsets[3]));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(HS.externOffsets[4]));
        for (int i = 0; i < 16; i++)
            BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(HS.LmetacarpOffsets[i]));
        for (int i = 0; i < 16; i++)
            BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(HS.RmetacarpOffsets[i]));

        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(HS.externDistances[0]));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(HS.externDistances[1]));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(HS.externDistances[2]));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(HS.externDistances[3]));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(HS.externDistances[4]));

        for (int i = 0; i < 16; i++)
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(HS.LmetacarpDistances[i]));
        for (int i = 0; i < 16; i++)
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(HS.RmetacarpDistances[i]));

        return data.ToArray();

    }

    public static IEnumerator CaptureHandSequenceByDifference(
        GameObject lefthand, GameObject righthand, GameObject head,
        int length, float penaleuler, float penaldisthead, float penalexdist, float penallmetadist, float penalrmetadist) 
    {
        List<HandsState> Seq = new List<HandsState>();
        byte[] bin = null;

        while ( Seq.Count < length) 
        {
            HandsState frameState = GetHandsState(ref bin, lefthand, righthand, head);
            if ( Seq.Count != 0 )
            {
                float diff = CompareHandStates(Seq[Seq.Count - 1], frameState, 
                    penaleuler, penaldisthead, penalexdist, penallmetadist, penalrmetadist); 
                if ( Math.Abs(diff * 100) > 25f) // ON PEUT CHANGER LE NIVEAU DE DIFFERENCE ICI 
                    Seq.Add(frameState);
            }
            else
                Seq.Add(frameState);

            yield return new WaitForSeconds(0.5f);
        }
    
    }

    public static bool IsStateSimilar(float mResult)
    {
        mResult = Math.Abs(mResult * 100);
        if (mResult < 10f)
            return true;
        else
            return false;
    }

    public static IEnumerator CaptureHandSequenceByTime
        (
        GameObject lefthand, GameObject righthand, GameObject head,
        float tick, float duration
        ) 
    {
        float t = 0.0f;
        byte[] bin = null;
        List<HandsState> Seq = new List<HandsState>();


        while ( t < duration) 
        {
            Seq.Add(GetHandsState(ref bin, lefthand, righthand, head));
            t += tick;
            yield return new WaitForSeconds(tick); 
        }
        yield break;
    }

    public static float CompareHandSequence ( List<HandsState> A, List<HandsState> B,
        float penaleuler, float penaldisthead, float penalexdist, float penallmetadist, float penalrmetadist)
    {
        // ca va comparer en prenant
        int max_length = A.Count;
        if (B.Count > max_length)
            max_length = B.Count;
        float tot = 0;
        for (int i = 0; i < max_length; i++)
            tot += CompareHandStates(A[i], B[i], penaleuler, penaldisthead, penalexdist, penallmetadist, penalrmetadist);

        tot /= max_length;
        return tot;

    }

    public static float CompareLeftHandStates(HandsState a, HandsState b,
        float penaleuler, float penaldisthead, float penallmetadist)
    {
        // euler 0 
        float cmpeuler = 0;

        cmpeuler += Math.Abs((Math.Abs(a.eulers[0].x - MathUtilities.nearestmultiple((int)a.eulers[0].x, 360, false)))
            - (Math.Abs(b.eulers[0].x - MathUtilities.nearestmultiple((int)b.eulers[0].x, 360, false))));
        cmpeuler += Math.Abs((Math.Abs(a.eulers[0].y - MathUtilities.nearestmultiple((int)a.eulers[0].y, 360, false)))
           - (Math.Abs(b.eulers[0].y - MathUtilities.nearestmultiple((int)b.eulers[0].y, 360, false))));
        cmpeuler += Math.Abs((Math.Abs(a.eulers[0].z - MathUtilities.nearestmultiple((int)a.eulers[0].z, 360, false)))
           - (Math.Abs(b.eulers[0].z - MathUtilities.nearestmultiple((int)b.eulers[0].z, 360, false))));
        cmpeuler *= penaleuler;

        // head offset 0 
        float Ldisthead1 = Vector3.Distance(a.headOffsets[0], new Vector3(0, 0, 0));
        float Ldisthead2 = Vector3.Distance(b.headOffsets[0], new Vector3(0, 0, 0));
        float cmpdhead = 0;
        cmpdhead += Math.Abs(Math.Abs(Ldisthead1) - Math.Abs(Ldisthead2));
        cmpdhead *= penaldisthead;

        //  lmeta distances // 5 float 
        float cmplmetadist = 0;
        for (int i = 0; i < 16; i++)
        {
            cmplmetadist += Math.Abs(Math.Abs(a.LmetacarpDistances[i])) - Math.Abs(b.LmetacarpDistances[i]);
        }
        //   float penallmetadist = 0.5f; // a tweaker /--------------------------------
        cmplmetadist *= penallmetadist;

        // CONCAT : 
        float tot = cmpeuler + cmpdhead + cmplmetadist;
        return tot;
    }

    public static float CompareRightHandStates(HandsState a, HandsState b,
        float penaleuler, float penaldisthead, float penalrmetadist)
    {
        float cmpeuler = 0;
        cmpeuler += Math.Abs((Math.Abs(a.eulers[1].x - MathUtilities.nearestmultiple((int)a.eulers[1].x, 360, false)))
            - (Math.Abs(b.eulers[1].x - MathUtilities.nearestmultiple((int)b.eulers[1].x, 360, false))));
        cmpeuler += Math.Abs((Math.Abs(a.eulers[1].y - MathUtilities.nearestmultiple((int)a.eulers[1].y, 360, false)))
           - (Math.Abs(b.eulers[1].y - MathUtilities.nearestmultiple((int)b.eulers[1].y, 360, false))));
        cmpeuler += Math.Abs((Math.Abs(a.eulers[1].z - MathUtilities.nearestmultiple((int)a.eulers[1].z, 360, false)))
            - (Math.Abs(b.eulers[1].z - MathUtilities.nearestmultiple((int)b.eulers[1].z, 360, false))));
        cmpeuler *= penaleuler;

        float Rdisthead1 = Vector3.Distance(a.headOffsets[1], new Vector3(0, 0, 0));
        float Rdisthead2 = Vector3.Distance(b.headOffsets[1], new Vector3(0, 0, 0));
        float cmpdhead = 0;
        cmpdhead += Math.Abs(Math.Abs(Rdisthead1) - Math.Abs(Rdisthead2));
        cmpdhead *= penaldisthead;

        //  rmeta distances // 5 float 
        float cmprmetadist = 0;
        for (int i = 0; i < 16; i++)
        {
            cmprmetadist += Math.Abs(Math.Abs(a.RmetacarpDistances[i])) - Math.Abs(b.RmetacarpDistances[i]);
        }
        //  float penalrmetadist = 0.5f; // a tweaker /--------------------------------
        cmprmetadist *= penalrmetadist;

        float tot = cmpeuler + cmpdhead + cmprmetadist;
        return tot;
    }
    public static float CompareHandStates(HandsState a, HandsState b,
        float penaleuler, float penaldisthead, float penalexdist, float penallmetadist, float penalrmetadist ) // scoring recognitions hand state
    {
        // [ 0 ] compare eulers 

        float cmpeuler = 0;

        cmpeuler += Math.Abs( (Math.Abs(a.eulers[0].x - MathUtilities.nearestmultiple((int)a.eulers[0].x, 360, false))) 
            - (Math.Abs(b.eulers[0].x - MathUtilities.nearestmultiple((int)b.eulers[0].x, 360, false))) );
        cmpeuler += Math.Abs ( (Math.Abs(a.eulers[1].x - MathUtilities.nearestmultiple((int)a.eulers[1].x, 360, false))) 
            - (Math.Abs(b.eulers[1].x - MathUtilities.nearestmultiple((int)b.eulers[1].x, 360, false))) );


        cmpeuler += Math.Abs ( (Math.Abs(a.eulers[0].y - MathUtilities.nearestmultiple((int)a.eulers[0].y, 360, false))) 
            - (Math.Abs(b.eulers[0].y - MathUtilities.nearestmultiple((int)b.eulers[0].y, 360, false))) ) ;
        cmpeuler += Math.Abs ( (Math.Abs(a.eulers[1].y - MathUtilities.nearestmultiple((int)a.eulers[1].y, 360, false)))
            - (Math.Abs(b.eulers[1].y - MathUtilities.nearestmultiple((int)b.eulers[1].y, 360, false))) ) ;

        cmpeuler += Math.Abs ( (Math.Abs(a.eulers[0].z - MathUtilities.nearestmultiple((int)a.eulers[0].z, 360, false))) 
            - (Math.Abs(b.eulers[0].z - MathUtilities.nearestmultiple((int)b.eulers[0].z, 360, false))) ) ;
        cmpeuler += Math.Abs ( (Math.Abs(a.eulers[1].z - MathUtilities.nearestmultiple((int)a.eulers[1].z, 360, false))) 
            - (Math.Abs(b.eulers[1].z - MathUtilities.nearestmultiple((int)b.eulers[1].z, 360, false))) ) ;

       // float penaleuler = 0.5f; // a tweaker /--------------------------------
        cmpeuler *= penaleuler;

        // [ 1 ]  compare distance from head
        float Ldisthead1 = Vector3.Distance(a.headOffsets[0], new Vector3(0, 0, 0));
        float Ldisthead2 = Vector3.Distance(b.headOffsets[0], new Vector3(0, 0, 0));
        float Rdisthead1 = Vector3.Distance(a.headOffsets[1], new Vector3(0, 0, 0));
        float Rdisthead2 = Vector3.Distance(b.headOffsets[1], new Vector3(0, 0, 0));

        float cmpdhead = 0;
        cmpdhead += Math.Abs(Math.Abs(Ldisthead1) - Math.Abs(Ldisthead2));
        cmpdhead += Math.Abs(Math.Abs(Rdisthead1) - Math.Abs(Rdisthead2));
       // float penaldisthead = 0.5f; // a tweaker /--------------------------------
        cmpdhead *= penaldisthead;

        // [ 2 ] extern distances // 5 float 
        float cmpexdist = 0;
        for (int i = 0; i < 5; i ++)
        {
            cmpexdist += Math.Abs(Math.Abs(a.externDistances[i])) - Math.Abs(b.externDistances[i]);
        }
      //  float penalexdist = 0.5f; // a tweaker /--------------------------------
        cmpexdist *= penalexdist;

        // [ 3 ] lmeta distances // 5 float 
        float cmplmetadist = 0;
        for (int i = 0; i < 16; i++)
        {
            cmplmetadist += Math.Abs(Math.Abs(a.LmetacarpDistances[i])) - Math.Abs(b.LmetacarpDistances[i]);
        }
     //   float penallmetadist = 0.5f; // a tweaker /--------------------------------
        cmplmetadist *= penallmetadist;

        // [ 4 ] rmeta distances // 5 float 
        float cmprmetadist = 0;
        for (int i = 0; i < 16; i++)
        {
            cmprmetadist += Math.Abs(Math.Abs(a.RmetacarpDistances[i])) - Math.Abs(b.RmetacarpDistances[i]);
        }
      //  float penalrmetadist = 0.5f; // a tweaker /--------------------------------
        cmprmetadist *= penalrmetadist;

        // CONCAT : 
        float tot = cmpeuler + cmpdhead  + cmpexdist + cmplmetadist + cmprmetadist;

        return tot;
    }

    public static HandsState GetHandsState( ref byte[] binary, GameObject lefthand, GameObject righthand, GameObject head) // we can copy binary into array if we want
    {
        /*
            Hands State Structure

        */
        List<byte> data = new List<byte>();
        // local euler ( 24 bytes 12+12)
        Vector3[] eulers = new Vector3[2];
        eulers[0] = lefthand.GetComponent<OVRSkeleton>().Bones[0].Transform.localEulerAngles;
        eulers[1] = righthand.GetComponent<OVRSkeleton>().Bones[0].Transform.localEulerAngles;
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(eulers[0]));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(eulers[1]));
       
       // offset tete (24o 12+12)
        Vector3[] headOffsets = new Vector3[2];
        headOffsets[0] = MathUtilities.GetOffsetFromObject(head, lefthand);
        headOffsets[1] = MathUtilities.GetOffsetFromObject(head, righthand);
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(headOffsets[0]));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(headOffsets[1]));

        // offset doit par inter main (60 octets 5*12 vec3) 
        Vector3[] externOffsets = new Vector3[5];
        externOffsets[0] = MathUtilities.GetOffsetFromVectors(lefthand.GetComponent<OVRSkeleton>().Bones[5].Transform.position,
                                                righthand.GetComponent<OVRSkeleton>().Bones[5].Transform.position);
        externOffsets[1] = MathUtilities.GetOffsetFromVectors(lefthand.GetComponent<OVRSkeleton>().Bones[8].Transform.position,
                                               righthand.GetComponent<OVRSkeleton>().Bones[8].Transform.position);
        externOffsets[2] = MathUtilities.GetOffsetFromVectors(lefthand.GetComponent<OVRSkeleton>().Bones[11].Transform.position,
                                               righthand.GetComponent<OVRSkeleton>().Bones[11].Transform.position);
        externOffsets[3] = MathUtilities.GetOffsetFromVectors(lefthand.GetComponent<OVRSkeleton>().Bones[14].Transform.position,
                                               righthand.GetComponent<OVRSkeleton>().Bones[14].Transform.position);
        externOffsets[4] = MathUtilities.GetOffsetFromVectors(lefthand.GetComponent<OVRSkeleton>().Bones[18].Transform.position,
                                               righthand.GetComponent<OVRSkeleton>().Bones[18].Transform.position);

        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(externOffsets[0]));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(externOffsets[1]));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(externOffsets[2]));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(externOffsets[3]));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(externOffsets[4]));

        // offset de chaque os du doigt par rapport au metacarpe  ( par main 16*12 = 192 octets ) 
        Vector3[] LmetacarpOffsets = new Vector3[16];
        for (int i = 2; i  < 18; i++)
        {
            LmetacarpOffsets[i - 2] = MathUtilities.GetOffsetFromVectors(lefthand.GetComponent<OVRSkeleton>().Bones[0].Transform.position,
                                                 lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position);
            BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(LmetacarpOffsets[i - 2]));
        }

        Vector3[] RmetacarpOffsets = new Vector3[16];
        for (int i = 2; i < 18; i++)
        {
            RmetacarpOffsets[i - 2] = MathUtilities.GetOffsetFromVectors(righthand.GetComponent<OVRSkeleton>().Bones[0].Transform.position,
                                                             righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position);
            BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(RmetacarpOffsets[i - 2]));
        }

        // distance intermain (20 o)
        float[] externDistances = new float[5];
        externDistances[0] = Vector3.Distance(lefthand.GetComponent<OVRSkeleton>().Bones[5].Transform.position,
                                               righthand.GetComponent<OVRSkeleton>().Bones[5].Transform.position);
        externDistances[1] = Vector3.Distance(lefthand.GetComponent<OVRSkeleton>().Bones[8].Transform.position,
                                               righthand.GetComponent<OVRSkeleton>().Bones[8].Transform.position);
        externDistances[2] = Vector3.Distance(lefthand.GetComponent<OVRSkeleton>().Bones[11].Transform.position,
                                               righthand.GetComponent<OVRSkeleton>().Bones[11].Transform.position);
        externDistances[3] = Vector3.Distance(lefthand.GetComponent<OVRSkeleton>().Bones[14].Transform.position,
                                               righthand.GetComponent<OVRSkeleton>().Bones[14].Transform.position);
        externDistances[4] = Vector3.Distance(lefthand.GetComponent<OVRSkeleton>().Bones[18].Transform.position,
                                       righthand.GetComponent<OVRSkeleton>().Bones[18].Transform.position);


        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(externDistances[0]));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(externDistances[1]));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(externDistances[2]));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(externDistances[3]));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(externDistances[4]));
        // distance os metacarpe (64 o par main ) 
        float[] LmetacarpDistances = new float[16];
        for (int i = 2; i < 18; i++)
        {
            LmetacarpDistances[i - 2] = Vector3.Distance(lefthand.GetComponent<OVRSkeleton>().Bones[0].Transform.position,
                                                 lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position);
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(    LmetacarpDistances[i - 2]));
        }
        float[] RmetacarpDistances = new float[16];
        for (int i = 2; i < 18; i++)
        {
            RmetacarpDistances[i - 2] = Vector3.Distance(righthand.GetComponent<OVRSkeleton>().Bones[0].Transform.position,
                                                             righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position);
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(RmetacarpDistances[i - 2]));
        }
        binary = data.ToArray();
        return new HandsState(eulers, headOffsets, externOffsets, LmetacarpOffsets, RmetacarpOffsets, externDistances, LmetacarpDistances, RmetacarpDistances);

    }


    float GetDistanceMetaCarpal(OVRBone finger, GameObject hand)
    {
        
        float dist = Vector3.Distance(hand.GetComponent<OVRSkeleton>().Bones[0].Transform.position, finger.Transform.position);
        dist = Mathf.Abs(dist);
        return dist;
    }
  


}
