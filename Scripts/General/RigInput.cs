using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

public class RigInput : MonoBehaviour
{
    /*
     TODO : 
     
    * FINGER Undo transform When Tracked is not done 
    * Send Finger INFO always ( when not tracked ) for updating
    * Left Hand Grabbing
    * Better Pose Detector


     
     */
    public GameObject head;
    public GameObject lefthand;
    public GameObject righthand;


    void Start()
    {
        head = GameObject.Find("CenterEyeAnchor");
        lefthand = GameObject.Find("OVRlefthand");
        righthand = GameObject.Find("OVRrighthand");

    }

    void CreateAndSeeHandGesture(string filePath)
    {
        byte[] bin = File.ReadAllBytes(filePath);
        HandShortKey.HandsState pose = HandShortKey.BinaryToHandState(bin);
        // so know parse the stuff
        GameObject handprefab = Instantiate(Resources.Load("OculusHand_R") as GameObject);
        handprefab.transform.position = new Vector3(5, 0, 5);

        /*
         (b_r_wrist) bones 0 
         b_r_index1 (2 & 3)
         b_r_middle1 (2 & 3) 
         b_r_pinky0 (1 & 2 & 3) 
         b_r_ring1 (2 & 3) 
         b_r_thumb0 (1 & 2 & 3) 

         //RmetacarpOffsets data : 
         bones 2 à 18 comparé a bones 0  

         let's resume : 

         b_r_thumb0         -> bones[2]     -> RmetacarpOffsets[0] (always -2 from bones) 
         b_r_thumb1         -> bones[3]     -> RmetacarpOffsets[1] (always -2 from bones) 
         b_r_thumb2         -> bones[4]     -> RmetacarpOffsets[2] (always -2 from bones) 
         b_r_thumb3         -> bones[5]     -> RmetacarpOffsets[3] (always -2 from bones) 

         b_r_index1         -> bones[6]     -> RmetacarpOffsets[4] (always -2 from bones) 
         b_r_index2         -> bones[7]     -> RmetacarpOffsets[5] (always -2 from bones) 
         b_r_index3         -> bones[8]     -> RmetacarpOffsets[6] (always -2 from bones) 

         b_r_middle1         -> bones[9]      -> RmetacarpOffsets[7] (always -2 from bones) 
         b_r_middle2         -> bones[10]     -> RmetacarpOffsets[8] (always -2 from bones) 
         b_r_middle3         -> bones[11]     -> RmetacarpOffsets[9] (always -2 from bones) 

         b_r_pinky0         -> bones[15]     -> RmetacarpOffsets[13] (always -2 from bones) 
         b_r_pinky1         -> bones[16]     -> RmetacarpOffsets[14] (always -2 from bones) 
         b_r_pinky2         -> bones[17]     -> RmetacarpOffsets[15] (always -2 from bones) 
         b_r_pinky3         -> bones[18]     -> RmetacarpOffsets[16] (always -2 from bones) 

         b_r_ring1          -> bones[12]      -> RmetacarpOffsets[10] (always -2 from bones) 
         b_r_ring2         -> bones[13]     -> RmetacarpOffsets[11] (always -2 from bones) 
         b_r_ring3        -> bones[14]     -> RmetacarpOffsets[12] (always -2 from bones) 
         */
        GameObject metacarp = ObjectUtilities.FindGameObjectChild(handprefab, "b_r_wrist");
        metacarp.transform.localEulerAngles = pose.eulers[1];
        // set the rot euler of metacarp 
        Vector3 npos;
        // dist is equal to bones[0] - bones[i] . so to get same offset do . bones[0] - RmetacarpOffsets or the inverse . 
        // warning distance depends of rotation ... 

        npos = metacarp.transform.position - pose.RmetacarpOffsets[0];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_thumb0").transform.position = npos;
        npos = metacarp.transform.position - pose.RmetacarpOffsets[1];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_thumb1").transform.position = npos;
        npos = metacarp.transform.position - pose.RmetacarpOffsets[2];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_thumb2").transform.position = npos;
        npos = metacarp.transform.position - pose.RmetacarpOffsets[3];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_thumb3").transform.position = npos;

        npos = metacarp.transform.position - pose.RmetacarpOffsets[4];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_index1").transform.position = npos;
        npos = metacarp.transform.position - pose.RmetacarpOffsets[5];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_index2").transform.position = npos;
        npos = metacarp.transform.position - pose.RmetacarpOffsets[6];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_index3").transform.position = npos;

        npos = metacarp.transform.position - pose.RmetacarpOffsets[7];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_middle1").transform.position = npos;
        npos = metacarp.transform.position - pose.RmetacarpOffsets[8];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_middle2").transform.position = npos;
        npos = metacarp.transform.position - pose.RmetacarpOffsets[9];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_middle3").transform.position = npos;

        npos = metacarp.transform.position - pose.RmetacarpOffsets[10];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_ring1").transform.position = npos;
        npos = metacarp.transform.position - pose.RmetacarpOffsets[11];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_ring2").transform.position = npos;
        npos = metacarp.transform.position - pose.RmetacarpOffsets[12];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_ring3").transform.position = npos;

        npos = metacarp.transform.position - pose.RmetacarpOffsets[13];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_pinky0").transform.position = npos;
        npos = metacarp.transform.position - pose.RmetacarpOffsets[14];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_pinky1").transform.position = npos;
        npos = metacarp.transform.position - pose.RmetacarpOffsets[15];
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_pinky2").transform.position = npos;

        return;
        npos = metacarp.transform.position - pose.RmetacarpOffsets[16]; // this is null ... 
        ObjectUtilities.FindGameObjectChild(handprefab, "b_r_pinky3").transform.position = npos;


    }

    //----------- MIC UNUSED STUFF
    private AudioSource audio;
    private string _SelectedDevice;

    void ConfigureMic()
    {
        if (Microphone.devices.Length > 0)
        {
            _SelectedDevice = Microphone.devices[0].ToString();
            audio = GetComponent<AudioSource>(); // need audio source on the model 
            audio.clip = Microphone.Start(_SelectedDevice, true, 10, 48000);
            audio.loop = true;
            while (!(Microphone.GetPosition(null) > 0)) { }
            audio.Play();

        }


    }

    //----------- for lip sync idk .... 
    // there is some prebuilt lip sync with ovr so maybe used it when we got less static face? 
    float GetAverageVolume()
    {
        float[] data = new float[256];
        float a = 0;
        audio.GetOutputData(data, 0);
        foreach (float s in data)
        {
            a += Mathf.Abs(s);
        }
        return a / 256;
    }

    //_-_-_-_-_-_-_- DEPLACEMENT  -_-_-_-_-_-_-_-_-

    void Move_XZ(bool _fwrd = true)
    {
        if (_fwrd)
        {
            Vector3 nv = transform.position + (Camera.main.transform.forward * Time.deltaTime * 2f);
            transform.position = new Vector3(nv.x, transform.position.y, nv.z);
        }
        else
        {
            Vector3 nv = transform.position - (Camera.main.transform.forward * Time.deltaTime * 2f);
            transform.position = new Vector3(nv.x, transform.position.y, nv.z);
        }

    }

    void Move_Y(bool _2ThEsKy) // for no gravity space only :)
    {
        if (_2ThEsKy)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z);
        }
    }
    //_-_-_-_-_-_-_- BASIC INTERACTION WITH THE WORLD -_-_-_-_-_-_-_-_-


    public Material capMaterial;
    void Cutcutcut()
    {
        RaycastHit hit;

        if (Physics.Raycast(righthand.transform.position, righthand.transform.forward, out hit))
        {

            GameObject victim = hit.collider.gameObject;

            if (victim.tag == "3DMESH")
            {
                string ancient_name = victim.name;
                GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(victim, righthand.transform.position, righthand.transform.right, capMaterial);
                // it will probably return 2 pieces 
                if (!pieces[1].GetComponent<Rigidbody>())
                {
                    // will send 2 * 64 bytes ( name of the 2 pieces ) 
                    pieces[0].tag = "3DMESH";
                    if (!pieces[0].GetComponent<ModifMesh>())
                    {
                        pieces[0].AddComponent<ModifMesh>();
                        pieces[0].GetComponent<ModifMesh>().ForceStart();
                    }

                    pieces[0].name = ancient_name;
                    pieces[1].name = CryptoUtilities.GetUniqueName();
                    // next set a name for those go .. piece 0 will be ancient name , piece 1 will be a new hash name 

                    List<byte> data = new List<byte>();
                    data.Add(0); // CUT HEADER ???? PLEASE?
                    data.Add((byte)ancient_name.Length);
                    for (int i = 0; i < ancient_name.ToCharArray().Length; i++)
                    {
                        data.Add((byte)ancient_name.ToCharArray()[i]);
                    }
                    BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.transform.position.x));
                    BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.transform.position.y));
                    BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.transform.position.z));
                    BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.transform.right.x));
                    BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.transform.right.y));
                    BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(righthand.transform.right.z));
                    foreach (char c in pieces[1].name.ToCharArray())
                    {
                        data.Add((byte)c);
                    }
                    // add the name of pieces 2 

                    NetUtilities.SendDataToAll(data.ToArray());
                    // --

                    Mesh mesh = pieces[1].GetComponent<MeshFilter>().mesh;
                    MeshCollider mc = pieces[1].AddComponent<MeshCollider>();
                    mc.sharedMesh = mesh;
                    mc.convex = true;

                    pieces[1].AddComponent<Rigidbody>();
                    // we should add to pieces 1 a 3d meshes tag and modif mesh component ????
                    pieces[1].tag = "3DMESH";
                    pieces[1].AddComponent<ModifMesh>();
                    pieces[1].GetComponent<ModifMesh>().ForceStart();
                }
                else
                {
                    //--- (-_-) ----
                }
            }


            //Destroy(pieces[1], 1);
        }
        //canCut = false;
    }
   

  
   
    void Update()
    {
        /*
        GameObject[] pobjs = HandRecognition.GetMeshesPointedByRightFinger();
        if (pobjs != null )
        {
            if (pobjs.Length > 0)
            {
                Debug.Log("pointing " + pobjs[0]);
            }
            else
            {
                Debug.Log("pointing nothing");
            }

        }
       */
        //---------------------------------------- [old]
        Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
       
        if (primaryAxis.y > 0f )
        {
            Move_XZ(true);
        }
        if (primaryAxis.y < 0f)
        {
            Move_XZ(false);
        }
        if (primaryAxis.x > 0f)
        {
            // rotate y
            this.transform.Rotate(0, (primaryAxis.x * 5), 0); // ok ?
        }
        if (primaryAxis.x < 0f)
        {
            // rotate y
            this.transform.Rotate(0, (primaryAxis.x * 5), 0); // ok?
        }
      
       
    }



}
