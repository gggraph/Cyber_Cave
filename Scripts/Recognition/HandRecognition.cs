using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class HandRecognition : MonoBehaviour
{
    // stream class to start handle gesture recognition... This is attach to rig object.
    public int counter = 0;
    public static string[] pose_ini; 

    public void Start()
    {
        GetIniFile();
        InvokeRepeating("CoreRecognition", 5f, 1f);
        //InvokeRepeating("GetPoseStrategy1", 1f, 1f);
        TestDebug(); 
        TestDebug();

    }

    public void Update()
    {
        // This one is for later ;) 
        if (!GameStatus._WindowIsOpen && !GameStatus._selectorIsOpen)
        {
            if (IsPose_Walk())
            {
                var speed = 8f * Time.deltaTime;
                Vector3[] frontdata = MathUtilities.GetPositionAndEulerInFrontOfPlayer(1f);
                this.transform.position = Vector3.MoveTowards(this.transform.position, frontdata[0], speed);
                //this.transform.rotation = Quaternion.LookRotation(frontdata[0]);
                this.transform.LookAt(frontdata[0]); /// seems good 
            }
        }
    }

    public void GetIniFile()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/pose"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/pose");
        }
        // write specific data in ini files. 
        if ( File.Exists(Application.persistentDataPath + "/pose/pose.ini"))
        {
            pose_ini = File.ReadAllLines(Application.persistentDataPath + "/pose/pose.ini");
        }
        else
        {
            /*
             format is : id-cmpmode-scorethreshold-MetaDistancePenality-HeadDistancePenality-EulerPenality-ExternalDistancePenality
             cmpmode 0 is both left and right comparing. 1 is only left, 2 is only right
             */
            pose_ini = new string[]
            {
                "ok-2-0,4-1-0-0,0035-0",
                "shiftleft-2-0,6-1-0-0,0035-0",
                "shiftright-1-0,6-1-0-0,0035-0",
                "openmenu-0-1,0-1-0-0,0035-2",
                "close-0-1,0-1-0-0,0035-2",
                "walk-0-1,0-1-0-0,0035-2"

            };
            File.WriteAllLines(Application.persistentDataPath + "/pose/pose.ini", pose_ini);
        }
        int poseIndex = 0;
        int cmode = GetPoseCompareModeFromIni(poseIndex);
        float scth = GetScoreTresholdFromIni(poseIndex);
        float mtcdst = GetMetacarpDistancePenalityFromIni(poseIndex);
        float hddst = GetHeadDistancePenalityFromIni(poseIndex);
        float eudst = GetEulerPenalityFromIni(poseIndex);
        float exdst = GetExternalDistancePenalityFromIni(poseIndex);

    }

    public static int GetPoseCompareModeFromIni(int poseIndex)
    {
        return int.Parse(pose_ini[poseIndex].Split('-')[1]);
    }
    public static float GetScoreTresholdFromIni(int poseIndex)
    {
        return float.Parse(pose_ini[poseIndex].Split('-')[2]);
    }
    public static float GetMetacarpDistancePenalityFromIni(int poseIndex)
    {
        return float.Parse(pose_ini[poseIndex].Split('-')[3]);
    }
    public static float GetHeadDistancePenalityFromIni(int poseIndex)
    {
        return float.Parse(pose_ini[poseIndex].Split('-')[4]);
    }
    public static float GetEulerPenalityFromIni(int poseIndex)
    {
        return float.Parse(pose_ini[poseIndex].Split('-')[5]);
    }
    public static float GetExternalDistancePenalityFromIni(int poseIndex)
    {
        return float.Parse(pose_ini[poseIndex].Split('-')[6]);
    }

    public static bool DetectPoseFromIni(int poseIndex)
    {
        GameObject head = GameObject.Find("CenterEyeAnchor");
        GameObject left = GameObject.Find("OVRlefthand");
        GameObject right = GameObject.Find("OVRrighthand");

        if (left.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return false;
        HandPose current = CreateHandPoseStructureFromBinary(GetHandsPoseAsBinary(left, right, head));
        HandPose origin = CreateHandPoseStructureFromBinary(File.ReadAllBytes(Application.persistentDataPath + "/pose/"+poseIndex.ToString())); // pose 11 

        if ( poseIndex == 0)
        {
            DebugHand(current, 0, -0.5f);
            DebugHand(origin, 1, 0.5f);
        }
        int cmode = GetPoseCompareModeFromIni(poseIndex);
        float scth = GetScoreTresholdFromIni(poseIndex);
        float mtcdst = GetMetacarpDistancePenalityFromIni(poseIndex);
        float hddst = GetHeadDistancePenalityFromIni(poseIndex);
        float eudst = GetEulerPenalityFromIni(poseIndex);
        float exdst = GetExternalDistancePenalityFromIni(poseIndex);

        bool _result = false;
        if ( cmode == 0)
        {
            float score = CompareHandPoses(origin, current, mtcdst, exdst, hddst, eudst);
            if (score < scth)
                _result = true;
        }
        else if ( cmode == 1)
        {
            float score = CompareLeftHandPose(origin, current, mtcdst, hddst, eudst);
            if (score < scth)
                _result = true;
        }
        else
        {
            float score = CompareRightHandPose(origin, current, mtcdst, hddst, eudst);
            if (score < scth)
                _result = true;
        }

        current.DestroyObjects();
        origin.DestroyObjects();

        return _result;
    }

    public void TestRecognitionSomeSign()
    {
       

        HandPose h1 = CreateHandPoseStructureFromBinary(File.ReadAllBytes(@"C:\Users\gaelg\Documents\CYBER_CAVE\Pose\4-1-final test\pose9"));
        HandPose h2 = CreateHandPoseStructureFromBinary(File.ReadAllBytes(@"C:\Users\gaelg\Documents\CYBER_CAVE\Pose\4-1-final test\pose27"));
        // 59 is bad it was 3.9. 45 is nearly equal. it was 0.17. 37 look similar but it is wrong. i got 0.17... only... so i guess euler is not too accurate. need to work on it more.  
        //float score = CompareHandPoses(h1, h2, 1f, 2f,0f);
        float score = CompareRightHandPose(h1, h2, 1f, 0f);
        // result was 0.9
        score =  Mathf.Round(score * 100f) / 100f;
        Debug.Log("Result of recognition : " + score);
        // Destroy HandPoses Objects. 
       // h1.DestroyObjects();
     //   h2.DestroyObjects();

    }
    public static void RefreshCachePoses()
    {
        /*
        GameObject head = GameObject.Find("CenterEyeAnchor");
        GameObject left = GameObject.Find("OVRlefthand");
        GameObject right = GameObject.Find("OVRrighthand");

        if (left.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return;

        if (_cachePoses.Count > 16)
        {
            _cachePoses[0].DestroyObjects();
            _cachePoses.RemoveAt(0);
            _cachePoses.Add(CreateHandPoseStructureFromBinary(GetHandsPoseAsBinary(left, right, head)));
        }
        else
        {
            _cachePoses.Add(CreateHandPoseStructureFromBinary(GetHandsPoseAsBinary(left, right, head)));
        }
        */
    }
    public void CoreRecognition()
    {
       
        string result = "";
        if (IsPose_OKSign())
        {
            result += "OK.";
        }
        if (IsPose_CloseSign())
        {
            result += "CLOSE.";
        }
        if (IsPose_ShiftLeftSign())
        {
            result += "LEFT.";
        }
        if (IsPose_ShiftRightSign())
        {
            result += "RIGHT.";
        }
        if (IsPose_OpenMenu() || Input.GetKey(KeyCode.M))
        {
            result += "MENU.";
            GlobalMenu.OpenGlobalMenu();
            if (!BitMemory.GetMemoriesBit(2))
                GetComponent<MemoryEvent>().StartCoroutine(GetComponent<MemoryEvent>().Routine_002());
        }

       
       
        if ( result.Length == 0)
        {
           
        }
        else
        {
            Debug.Log(result);
        }
    }
    
    public static bool IsPose_OKSign( ) 
    {
        return DetectPoseFromIni(0);
    }
    public static bool IsPose_CloseSign()
    {
        return DetectPoseFromIni(4);
    }
    public static bool IsPose_ShiftLeftSign() 
    {
        return DetectPoseFromIni(1);
    }
    public static bool IsPose_ShiftRightSign() 
    {
        return DetectPoseFromIni(2);
    }

    public static bool IsPose_OpenMenu()
    {
        return DetectPoseFromIni(3);
    }

    public static bool IsPose_Walk()
    {
        return DetectPoseFromIni(5);
    }


    public void GetPoseStrategy1()
    {
        GameObject head =  GameObject.Find("CenterEyeAnchor");
        GameObject left =  GameObject.Find("OVRlefthand");
        GameObject right = GameObject.Find("OVRrighthand");

        byte[] bin = GetHandsPoseAsBinary(left,right,head);
        if ( bin.Length >0)
        {
            File.WriteAllBytes(Application.persistentDataPath + "/pose" + counter.ToString(), bin);
            Debug.Log("Pose saved #" + counter.ToString());
            counter++;
        }
       
        
    }


    /*--------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    public static float CompareLeftHandPose(HandPose A, HandPose B, float MetaDistancePenality = 1.0f, float HeadDistancePenality = 10.0f, float EulerPenality = 0.0035f)
    {
        Vector3[] LMetaOffset_A = A.GetVectorOffsetsFromLeftMetacarp();
        Vector3[] LMetaOffset_B = B.GetVectorOffsetsFromLeftMetacarp();
        float LMetaDist = GetSumOfVectorsDistance(LMetaOffset_A, LMetaOffset_B);
        LMetaDist *= MetaDistancePenality;
        // good value is like below 0.3. bad value is like 0.7-1.0 ect. 

        // QUATERNION. is like 
        //Mathf.DeltaAngle(1080, 90)

        // ok value is like 50. then 200 sucks hard.

        // Head Distance 
        Vector3 LHeadOffset_A = A.GetHeadVectorOffsetOfLeftHand();
        Vector3 LHeadOffset_B = B.GetHeadVectorOffsetOfLeftHand();
        float LHeadDistance = Math.Abs(Vector3.Distance(LHeadOffset_A, LHeadOffset_B));
        // good value is 0.02 bad vvalue is like 0.1 
        LHeadDistance *= HeadDistancePenality;

        A.CorrectMetacarpEulersFromCamera();
        B.CorrectMetacarpEulersFromCamera();
        float Ld1 = Math.Abs(Mathf.DeltaAngle(A.LeftHand[0].transform.eulerAngles.x, B.LeftHand[0].transform.eulerAngles.x));
        float Ld2 = Math.Abs(Mathf.DeltaAngle(A.LeftHand[0].transform.eulerAngles.y, B.LeftHand[0].transform.eulerAngles.y));
        float Ld3 = Math.Abs(Mathf.DeltaAngle(A.LeftHand[0].transform.eulerAngles.z, B.LeftHand[0].transform.eulerAngles.z));
        float Ldsum = Ld1 + Ld2 + Ld3;
        float LeulerDist = Ldsum * EulerPenality;

        return LMetaDist + LHeadDistance + LeulerDist;

    }
    public static float CompareRightHandPose(HandPose A, HandPose B, float MetaDistancePenality = 1.0f, float HeadDistancePenality = 10.0f, float EulerPenality = 0.0035f)
    {

        Vector3[] RMetaOffset_A = A.GetVectorOffsetsFromRightMetacarp();
        Vector3[] RMetaOffset_B = B.GetVectorOffsetsFromRightMetacarp();
        float RMetaDist = GetSumOfVectorsDistance(RMetaOffset_A, RMetaOffset_B);
        RMetaDist *= MetaDistancePenality;
        // good value is like below 0.3. bad value is like 0.7-1.0 ect. 


        // ok value is like 50. then 200 sucks hard.

        // Head Distance 
        Vector3 RHeadOffset_A = A.GetHeadVectorOffsetOfRightHand();
        Vector3 RHeadOffset_B = B.GetHeadVectorOffsetOfRightHand();
        float RHeadDistance = Math.Abs(Vector3.Distance(RHeadOffset_A, RHeadOffset_B));
        // good value is 0.02 bad vvalue is like 0.1 
        RHeadDistance *= HeadDistancePenality;

        // QUATERNION. is like 
        A.CorrectMetacarpEulersFromCamera();
        B.CorrectMetacarpEulersFromCamera();
        //Mathf.DeltaAngle(1080, 90)
        float Rd1 = Math.Abs(Mathf.DeltaAngle(A.RightHand[0].transform.eulerAngles.x, B.RightHand[0].transform.eulerAngles.x));
        float Rd2 = Math.Abs(Mathf.DeltaAngle(A.RightHand[0].transform.eulerAngles.y, B.RightHand[0].transform.eulerAngles.y));
        float Rd3 = Math.Abs(Mathf.DeltaAngle(A.RightHand[0].transform.eulerAngles.z, B.RightHand[0].transform.eulerAngles.z));
        float Rdsum = Rd1 + Rd2 + Rd3;
        float ReulerDist = Rdsum * EulerPenality;

        return RMetaDist + RHeadDistance + ReulerDist;

    }
    public static float CompareHandPoses(HandPose A, HandPose B, float MetaDistancePenality = 1.0f, float ExternalDistancePenality = 2.0f, float HeadDistancePenality = 10f, float EulerPenality = 0.0035f)
    {
        // [1] MetaCarp Distance 
        Vector3[] RMetaOffset_A =  A.GetVectorOffsetsFromRightMetacarp();
        Vector3[] RMetaOffset_B =  B.GetVectorOffsetsFromRightMetacarp();
        float RMetaDist = GetSumOfVectorsDistance(RMetaOffset_A, RMetaOffset_B);

        Vector3[] LMetaOffset_A = A.GetVectorOffsetsFromLeftMetacarp();
        Vector3[] LMetaOffset_B =  B.GetVectorOffsetsFromLeftMetacarp();
        float LMetaDist = GetSumOfVectorsDistance(LMetaOffset_A, LMetaOffset_B);

        RMetaDist *= MetaDistancePenality;
        LMetaDist *= MetaDistancePenality;
        // Average Ressemblance is like 0.10 or so ... Similarity but not exact is like 0.40.. Absolutely not exact is 0.8 or so :) 

        // [2] External Distance 
        Vector3 externoffset_A = A.GetVectorOffsetOfTwoHands();
        Vector3 externoffset_B = B.GetVectorOffsetOfTwoHands();

        // 0.04 is Good, 0.2 is BAD  [  So need *2 penal multiplier ... ] [ BUT BECAUSE METACARP ARE MORE PRESENT, WE NEED PROBALY MULTIPLY BY 4.0]  
        float ExternalDist = Math.Abs(Vector3.Distance(externoffset_A, externoffset_B));
        ExternalDist *= ExternalDistancePenality;

        // [3] Head Distance 
        Vector3 LHeadOffset_A = A.GetHeadVectorOffsetOfLeftHand();
        Vector3 LHeadOffset_B = B.GetHeadVectorOffsetOfLeftHand();
        float LHeadDistance = Math.Abs(Vector3.Distance(LHeadOffset_A, LHeadOffset_B));

        Vector3 RHeadOffset_A = A.GetHeadVectorOffsetOfRightHand();
        Vector3 RHeadOffset_B = B.GetHeadVectorOffsetOfRightHand();
        float RHeadDistance = Math.Abs(Vector3.Distance(RHeadOffset_A, RHeadOffset_B));

        LHeadDistance *= HeadDistancePenality;
        RHeadDistance *= HeadDistancePenality;

        // [4] Quaternions
        A.CorrectMetacarpEulersFromCamera();
        B.CorrectMetacarpEulersFromCamera();
        float Rd1 = Math.Abs(Mathf.DeltaAngle(A.RightHand[0].transform.eulerAngles.x, B.RightHand[0].transform.eulerAngles.x));
        float Rd2 = Math.Abs(Mathf.DeltaAngle(A.RightHand[0].transform.eulerAngles.y, B.RightHand[0].transform.eulerAngles.y));
        float Rd3 = Math.Abs(Mathf.DeltaAngle(A.RightHand[0].transform.eulerAngles.z, B.RightHand[0].transform.eulerAngles.z));
        float Rdsum = Rd1 + Rd2 + Rd3;
        float ReulerDist = Rdsum * EulerPenality;

        float Ld1 = Math.Abs(Mathf.DeltaAngle(A.LeftHand[0].transform.eulerAngles.x, B.LeftHand[0].transform.eulerAngles.x));
        float Ld2 = Math.Abs(Mathf.DeltaAngle(A.LeftHand[0].transform.eulerAngles.y, B.LeftHand[0].transform.eulerAngles.y));
        float Ld3 = Math.Abs(Mathf.DeltaAngle(A.LeftHand[0].transform.eulerAngles.z, B.LeftHand[0].transform.eulerAngles.z));
        float Ldsum = Ld1 + Ld2 + Ld3;
        float LeulerDist = Ldsum * EulerPenality;


        return RMetaDist + LMetaDist + ExternalDist + LHeadDistance + RHeadDistance + ReulerDist + ReulerDist;
    }

    public static GameObject DebugHandA;
    public static GameObject DebugHandB;

    public static void TestDebug() 
    {
        HandPose origin = CreateHandPoseStructureFromBinary(File.ReadAllBytes(Application.persistentDataPath + "/pose/1"));
        DebugHand(origin, 0, 0f);
    }
    public static void DebugHand(HandPose h, int debughandnumber, float LROffset) 
    {
        return;
        GameObject handprefab = null;
        if ( debughandnumber == 0)
        {
            if (DebugHandA == null)
            {
                DebugHandA = ObjectUtilities.InstantiateGameObjectFromAssets("OculusHand_R");

            }
            handprefab = DebugHandA;
        }
        else
        {
            if (DebugHandB == null)
            {
                DebugHandB = ObjectUtilities.InstantiateGameObjectFromAssets("OculusHand_R");

            }
            handprefab = DebugHandB;
        }
      
        handprefab.transform.position = new Vector3(0, 0, 0);
        string[] handstructstr = new string[16]
        {
                "b_r_thumb0",
                "b_r_thumb1",
                "b_r_thumb2",
                "b_r_thumb3",
                "b_r_index1",
                "b_r_index2",
                "b_r_index3",
                "b_r_middle1",
                "b_r_middle2",
                "b_r_middle3",
                "b_r_ring1",
                "b_r_ring2",
                "b_r_ring3",
                "b_r_pinky0",
                "b_r_pinky1",
                "b_r_pinky2",

        };
        
        Vector3 rposm = new Vector3(h.RightHand[0].transform.position.x, h.RightHand[0].transform.position.y, h.RightHand[0].transform.position.z);
        Vector3 rrotm = new Vector3(h.RightHand[0].transform.eulerAngles.x, h.RightHand[0].transform.eulerAngles.y, h.RightHand[0].transform.eulerAngles.z);
        h.RightHand[0].transform.position = new Vector3(0, 0, 0);
        h.RightHand[0].transform.eulerAngles = new Vector3(0, 0, 0);
        
        GameObject metacarp = ObjectUtilities.FindGameObjectChild(handprefab, "b_r_wrist");
        metacarp.transform.localPosition = new Vector3(h.RightHand[0].transform.position.x , h.RightHand[0].transform.position.y, h.RightHand[0].transform.position.z);
        metacarp.transform.localEulerAngles = new Vector3(h.RightHand[0].transform.eulerAngles.x, h.RightHand[0].transform.eulerAngles.y, h.RightHand[0].transform.eulerAngles.z) ;

        for (int i = 0; i < 16; i++)
        {
            GameObject b = ObjectUtilities.FindGameObjectChild(handprefab, handstructstr[i]);
            b.transform.position = new Vector3(h.RightHand[i].transform.position.x, h.RightHand[i].transform.position.y, h.RightHand[i].transform.position.z) ;
            b.transform.eulerAngles = new Vector3(h.RightHand[i].transform.eulerAngles.x, h.RightHand[i].transform.eulerAngles.y, h.RightHand[i].transform.eulerAngles.z); 
        }
        
        h.RightHand[0].transform.position = rposm; // this is not reseting
        h.RightHand[0].transform.eulerAngles = rrotm; // this is not reseting
        
        // setup front of player of offset 
        Vector3[] frontdata = MathUtilities.GetPositionAndEulerInFrontOfPlayer(1f);
        handprefab.transform.position = frontdata[0] + (handprefab.transform.right * LROffset);
        return;
       
    } 

    public class HandPose
    {
        public GameObject   Head      { get; set; }
        public GameObject[] LeftHand  { get; set; }
        public GameObject[] RightHand { get; set; }

        public Vector3 cameraEulers { get; set; }
        public HandPose(GameObject H, GameObject[] L, GameObject[] R, Vector3 cEUler)
        {
            Head      = H;
            LeftHand  = L;
            RightHand = R;
            cameraEulers = cEUler;
        }
        public Vector3 GetVectorOffsetOfTwoHands()
        {
            // Do I need to reset Pos? NO because we compare Bones#0
            return MathUtilities.GetOffsetFromVectors(LeftHand[0].transform.position, RightHand[0].transform.position);
        }
        public Vector3 GetHeadVectorOffsetOfRightHand()
        {
            return MathUtilities.GetOffsetFromVectors(Head.transform.position, RightHand[0].transform.position);
        }
        public Vector3 GetHeadVectorOffsetOfLeftHand()
        {
            return MathUtilities.GetOffsetFromVectors(Head.transform.position, LeftHand[0].transform.position);
        }
        public Vector3[] GetVectorOffsetsFromLeftMetacarp()
        {

            // temporaly zeroing transform of metacarp. 
            Vector3 rposm = new Vector3(LeftHand[0].transform.position.x, LeftHand[0].transform.position.y, LeftHand[0].transform.position.z);
            Vector3 rrotm = new Vector3(LeftHand[0].transform.eulerAngles.x, LeftHand[0].transform.eulerAngles.y, LeftHand[0].transform.eulerAngles.z) ;
            LeftHand[0].transform.position = new Vector3(0, 0, 0);
            LeftHand[0].transform.eulerAngles = new Vector3(0, 0, 0);


            List<Vector3> vecs = new List<Vector3>();
            for (int i = 0; i < 24; i ++)
            {
                vecs.Add(MathUtilities.GetOffsetFromVectors(LeftHand[0].transform.position, LeftHand[i].transform.position));
            }
          
            // reset transform metacarp
            LeftHand[0].transform.position = rposm;
            LeftHand[0].transform.eulerAngles = rrotm;
  
            return vecs.ToArray();
        }

        

        public Vector3[] GetVectorOffsetsFromRightMetacarp()
        {
            // temporaly zeroing transform of metacarp. i means it is good .
            Vector3 rpos = new Vector3(RightHand[0].transform.position.x, RightHand[0].transform.position.y, RightHand[0].transform.position.z);
            Vector3 rrot = new Vector3(RightHand[0].transform.eulerAngles.x, RightHand[0].transform.eulerAngles.y, RightHand[0].transform.eulerAngles.z);

            // have zero impact on bones child of metacarp or i really do not know
            RightHand[0].transform.position = new Vector3(0, 0, 0);
            RightHand[0].transform.eulerAngles = new Vector3(0, 0, 0);

            List<Vector3> vecs = new List<Vector3>();
            for (int i = 0; i < 24; i++) // this is good 
            {
                vecs.Add(MathUtilities.GetOffsetFromVectors(RightHand[0].transform.position, RightHand[i].transform.position));
            }

            // reset transform of hand 
            RightHand[0].transform.position = rpos;
            RightHand[0].transform.eulerAngles = rrot;

   
            return vecs.ToArray();
        }

        public void CorrectMetacarpEulersFromCamera()
        {
            RightHand[0].transform.eulerAngles -= cameraEulers;
        }

        public void DestroyObjects()
        {
            if (Head != null)
                Destroy(Head.gameObject);
            if (LeftHand != null)
            {
                foreach ( GameObject g in LeftHand)
                {
                    Destroy(g.gameObject);
                }
            }
            if (RightHand != null)
            {
                foreach (GameObject g in RightHand)
                {
                    Destroy(g.gameObject);
                }
            }
        }
    }

    public static Vector3 RoundEulerAngle(Vector3 euler)
    {
        int nx = MathUtilities.nearestmultiple((int)euler.x, 360, true);
        int ny = MathUtilities.nearestmultiple((int)euler.y, 360, true);
        int nz = MathUtilities.nearestmultiple((int)euler.z, 360, true);

        return new Vector3(euler.x - nx, euler.y - ny, euler.z - nz);

    }
    public static float GetSumOfVectorsDistance(Vector3[] A, Vector3[] B)
    {
        float r = 0f;
        for (int i = 0; i < 24; i++)
        {
            r += Math.Abs(Vector3.Distance(A[i], B[i]));
        }
        return r;
    }
    public static byte[] GetHandsPoseAsBinary(GameObject lefthand, GameObject righthand, GameObject head)
    {
        List<byte> data = new List<byte>();
        // 24 bones 
        for (int i = 0; i < 24; i++)
        {
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].ParentBoneIndex)); // this is a short
            
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position.x));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position.y));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position.z));

            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.eulerAngles.x));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.eulerAngles.y));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.eulerAngles.z));

            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.localPosition.x));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.localPosition.y));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.localPosition.z));

            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.localEulerAngles.x));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.localEulerAngles.y));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lefthand.GetComponent<OVRSkeleton>().Bones[i].Transform.localEulerAngles.z));

        }
        // offset is 1200 here 
        for (int i = 0; i < 24; i++)
        {
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].ParentBoneIndex)); // this is a short

            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position.x));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position.y));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.position.z));

            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.eulerAngles.x));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.eulerAngles.y));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.eulerAngles.z));

            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.localPosition.x));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.localPosition.y));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.localPosition.z));

            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.localEulerAngles.x));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.localEulerAngles.y));
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.GetComponent<OVRSkeleton>().Bones[i].Transform.localEulerAngles.z));

           
        }
        // offset is 2400 here 

        // get the offset of head from rig position ... 
        GameObject rig = GameObject.Find("OVRCameraRig");
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(head.transform.position.x));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(head.transform.position.y));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(head.transform.position.z)); // +12
        
        // offset is 2410 here 

        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(head.transform.eulerAngles.x));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(head.transform.eulerAngles.y));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(head.transform.eulerAngles.z)); //+24

        // offset is 2424 here 

        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(head.transform.localEulerAngles.x));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(head.transform.localEulerAngles.y));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(head.transform.localEulerAngles.z)); //+36


        // offset is 2436 here
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(Camera.main.transform.eulerAngles.x));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(Camera.main.transform.eulerAngles.y));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(Camera.main.transform.eulerAngles.z)); // +48

        // offset is 2448 here 
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(rig.transform.position.x));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(rig.transform.position.y));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(rig.transform.position.z));
        
        // offset is 2460 here 
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(rig.transform.eulerAngles.x)); 
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(rig.transform.eulerAngles.y));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(rig.transform.eulerAngles.z));

        // offset is 2472 here 


        return data.ToArray();
    }
    public static HandPose CreateHandPoseStructureFromBinary(byte[] data) 
    {
        /*
         je crée le rig avec sa rotation a l'instant t
         j'attache tous mes objets. 
         je hard place mes objets (sans utilisé de local...) 
         je redefinis la rotation du rig a 0 0 0
         */

        // [1] Create rig and set up its rotation at time T
        GameObject fRig = new GameObject();
        float rigposx = BitConverter.ToSingle(data, 2448);
        float rigposy = BitConverter.ToSingle(data, 2452);
        float rigposz = BitConverter.ToSingle(data, 2456);

        float rigrotx = BitConverter.ToSingle(data, 2460);
        float rigroty = BitConverter.ToSingle(data, 2464);
        float rigrotz = BitConverter.ToSingle(data, 2468);
        fRig.transform.position = new Vector3(rigposx, rigposy, rigposz);
        fRig.transform.eulerAngles = new Vector3(rigrotx, rigroty, rigrotz);

        // [2] Create all finger bones objects
        GameObject[] L_objs = new GameObject[24];
        GameObject[] R_objs = new GameObject[24];

        bool _showobjects = false; float objsize = 0.01f;
        for (int i = 0; i < 24; i++)
        {
            if (!_showobjects)
            {
                L_objs[i] = new GameObject();
                R_objs[i] = new GameObject();

            }
            else
            {
                L_objs[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                R_objs[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                
                L_objs[i].transform.localScale = new Vector3(objsize, objsize, objsize);
                R_objs[i].transform.localScale = new Vector3(objsize, objsize, objsize);
            }
            L_objs[i].name = "LBONES #" + i.ToString();
            R_objs[i].name = "RBONES #" + i.ToString();
        }

        // [3] Proccess all parents 

        // Set parenting of index#0 with rig
        L_objs[0].transform.parent = fRig.transform;
        R_objs[0].transform.parent = fRig.transform;

        // Set parenting of all fingers 
        int ptr = 0;
        for (int i = 0; i < 24; i++)
        {
            short pIndex = BitConverter.ToInt16(data, ptr);
            if (pIndex >= 0)
            {
                L_objs[i].transform.parent = L_objs[pIndex].transform;

            }

            ptr += 24 + 24 + 2;
        }
        for (int i = 0; i < 24; i++)
        {
            short pIndex = BitConverter.ToInt16(data, ptr);
            if (pIndex >= 0)
            {
                R_objs[i].transform.parent = R_objs[pIndex].transform;

            }

            ptr += 24 + 24 + 2;
        }

        // [4] Force all objects to rotation and position 
        ptr = 0;
        for (int i = 0; i < 24; i++)
        {
            float px = BitConverter.ToSingle(data, ptr + 2);
            float py = BitConverter.ToSingle(data, ptr + 6);
            float pz = BitConverter.ToSingle(data, ptr + 10);
            float rx = BitConverter.ToSingle(data, ptr + 14);
            float ry = BitConverter.ToSingle(data, ptr + 18);
            float rz = BitConverter.ToSingle(data, ptr + 22);
            L_objs[i].transform.position = new Vector3(px, py, pz);
            L_objs[i].transform.eulerAngles = new Vector3(rx, ry, rz);
            // go next 
            ptr += 2 + 24 + 24;
        }

        for (int i = 0; i < 24; i++)
        {
            float px = BitConverter.ToSingle(data, ptr + 2);
            float py = BitConverter.ToSingle(data, ptr + 6);
            float pz = BitConverter.ToSingle(data, ptr + 10);
            float rx = BitConverter.ToSingle(data, ptr + 14);
            float ry = BitConverter.ToSingle(data, ptr + 18);
            float rz = BitConverter.ToSingle(data, ptr + 22);
            R_objs[i].transform.position = new Vector3(px, py, pz);
            R_objs[i].transform.eulerAngles = new Vector3(rx, ry, rz);
            ptr += 2 + 24 + 24;

        }

        // [5] Instantiate and set up head component.
        GameObject head;
        if (_showobjects)
        {
            head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.localScale = new Vector3(objsize * 5, objsize * 5, objsize * 5);
        }
        else
        {
            head = new GameObject();
        }
        

        head.transform.position = new Vector3(BitConverter.ToSingle(data, ptr ), BitConverter.ToSingle(data, ptr + 4), BitConverter.ToSingle(data, ptr + 8));
        head.transform.eulerAngles = new Vector3(BitConverter.ToSingle(data, ptr + 12 ), BitConverter.ToSingle(data, ptr + 16), BitConverter.ToSingle(data, ptr + 20));
        head.transform.parent = fRig.transform;

        // do some stuff with ceuler ... && fRig
        GameObject eulercam = new GameObject();
        eulercam.transform.parent = fRig.transform;
        eulercam.transform.eulerAngles = new Vector3(BitConverter.ToSingle(data, ptr + 36), BitConverter.ToSingle(data, ptr + 40), BitConverter.ToSingle(data, ptr + 44));
        //Vector3 ceuler = new Vector3(BitConverter.ToSingle(data, ptr + 36), BitConverter.ToSingle(data, ptr + 40), BitConverter.ToSingle(data, ptr + 44));
 
        // [6]  Reset Rig Object transform
        fRig.transform.position    = new Vector3(0, 0, 0);
        fRig.transform.eulerAngles = new Vector3(0, 0, 0);

        // [7] Done 
        // i really have to destroy frig ...
        fRig.transform.DetachChildren();
        Destroy(fRig.gameObject);
        Vector3 ceuler = new Vector3(eulercam.transform.eulerAngles.x, eulercam.transform.eulerAngles.y, eulercam.transform.eulerAngles.z);
        Destroy(eulercam.gameObject);
        return new HandPose(head, L_objs, R_objs, ceuler);

    }

    public static float GetYDistanceOfHands()
    {
        GameObject left = GameObject.Find("OVRlefthand");
        GameObject right = GameObject.Find("OVRrighthand");

        return Math.Abs(right.GetComponent<OVRSkeleton>().Bones[0].Transform.position.y - left.GetComponent<OVRSkeleton>().Bones[0].Transform.position.y);
    }
    public static float GetXDistanceOfHands()
    {
        GameObject left = GameObject.Find("OVRlefthand");
        GameObject right = GameObject.Find("OVRrighthand");

        return Math.Abs(right.GetComponent<OVRSkeleton>().Bones[0].Transform.position.x - left.GetComponent<OVRSkeleton>().Bones[0].Transform.position.x);
    }
    public static float GetZDistanceOfHands()
    {
        GameObject left = GameObject.Find("OVRlefthand");
        GameObject right = GameObject.Find("OVRrighthand");

        return Math.Abs(right.GetComponent<OVRSkeleton>().Bones[0].Transform.position.z - left.GetComponent<OVRSkeleton>().Bones[0].Transform.position.z);
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
        GameObject hand = GameObject.Find("OVRlefthand");
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

    public static bool IsRightHandClosed()
    {
        GameObject hand = GameObject.Find("OVRrighthand");
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

    public static float GetDistanceFromFingers(OVRBone fingerA, OVRBone fingerB)
    {
        float dist = Vector3.Distance(fingerA.Transform.position, fingerB.Transform.position);
        dist = Mathf.Abs(dist);
        return dist;
    }
    public static bool DoesLeftFingerPointingObject_RayCast(GameObject o)
    {
        GameObject hand = GameObject.Find("OVRlefthand");
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
    /*
    public static bool DoesRightFingerPointingObject_RayCast(GameObject o)
    {
        GameObject hand = GameObject.Find("OVRrighthand");
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
    */
    public static bool DoesLeftFingerPointingObject_SphereCast(GameObject o, float radius = 1f, float distance = 10f)
    {
        GameObject hand = GameObject.Find("OVRlefthand");
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
        foreach ( GameObject g in allchilds)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.gameObject == g)
                    return true;
            }
        }
       
        return false;
    }
    /*
    public static RaycastHit[] GetObjectTouchedByRightFinger_Thin(float dist = 0.02f) 
    {
        GameObject hand = GameObject.Find("OVRrighthand");
        if (hand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return null;

        Ray ray = new Ray(hand.GetComponent<OVRSkeleton>().Bones[7].Transform.position, hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position);
        return Physics.RaycastAll(ray, dist);
    }
    */
    public static RaycastHit[] GetObjectTouchedByRightFinger(float radius = 0.02f)
    {
        GameObject hand = GameObject.Find("OVRrighthand");
        if (hand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return null;
        RaycastHit[] hits;
        Ray ray = new Ray(hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position, hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position);
        hits = Physics.SphereCastAll(ray, radius, 0.00001f);
        return hits;

    }

    public static RaycastHit IsObjectTouchedByHand(GameObject hand, GameObject go, float radius = 0.02f) 
    {
        if (hand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return new RaycastHit();
        RaycastHit[] hits;
        Ray ray = new Ray(hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position, hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position);
        hits = Physics.SphereCastAll(ray, radius, 0.00001f);
        foreach ( RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == go)
                return hit;
        }
        return new RaycastHit();
    }
    public static RaycastHit DoesRightFingerTouchingObject_SphereCast (GameObject o, float radius = 0.2f)
    {
        GameObject hand = GameObject.Find("OVRrighthand");
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
        GameObject hand = GameObject.Find("OVRrighthand");
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
    /*
    public static GameObject[] GetMeshesPointedByRightFinger()
    {
        // get vector rotation between bone 6 (milieu) et 8 (bout) 
        GameObject hand = GameObject.Find("OVRrighthand");
        if (hand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return null;
        Vector3 origin = hand.GetComponent<OVRSkeleton>().Bones[6].Transform.position;
        Vector3 dest = hand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;

        GameObject collider = GameObject.CreatePrimitive(PrimitiveType.Cube);
        collider.transform.position = origin;
        float width = 1f;
        float distance = 8f;
        collider.transform.localScale = new Vector3(width, width, 8f);
        collider.transform.LookAt(dest);
        Bounds colliderbound = collider.GetComponent<Renderer>().bounds;
      
        GameObject[] meshes = GameObject.FindGameObjectsWithTag("3DMESH");
        List<GameObject> result = new List<GameObject>();

        Plane[] CamPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        foreach (GameObject go in meshes)
        {
            Bounds b = ObjectUtilities.GetBoundsOfGroupOfMesh(go);
            if (GeometryUtility.TestPlanesAABB(CamPlanes, b))
            {
                if (b.Intersects(colliderbound))
                {
                    result.Add(go);
                    break;
                }
            }

        }

        Destroy(collider.gameObject);
        return result.ToArray();

    }
    */

}

// W


