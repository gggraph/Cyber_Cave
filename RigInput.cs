using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;

public class RigInput : MonoBehaviour
{
    /*
     Anchors structure : 
      head, left hand, left fingers, right hand, right fingers
     
     */
    public GameObject head;
    public GameObject lefthand;
    public GameObject righthand;
    public List<GameObject> mAnchors = new List<GameObject>();
    public bool _AvatarFound = false;

    void Start()
    {
        head = GameObject.Find("CenterEyeAnchor");
        lefthand = GameObject.Find("OVRlefthand");
        righthand = GameObject.Find("OVRrighthand");
        InvokeRepeating("GetMyAnchors", 2, 2f);
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


    private void GetMyAnchors() // search anchors object
    {
        GameObject[] avatars = GameObject.FindGameObjectsWithTag("Avatar");
        foreach ( GameObject go in avatars )
        {
            if (go.GetComponent<DataReceiver>()._isMine)
            {
                // we are unable to get phon view object. there are not attached to transform in some way
                mAnchors.Add(FindGameObjectChild(go, "anc_head"));

                /*
                 * & all of them 
                 */

                // R anchors
                mAnchors.Add(FindGameObjectChild(go, "anc_rHand"));
                mAnchors.Add(FindGameObjectChild(go, "anc_rThumb1"));
                mAnchors.Add(FindGameObjectChild(go, "anc_rThumb2"));
                mAnchors.Add(FindGameObjectChild(go, "anc_rThumb3"));

                mAnchors.Add(FindGameObjectChild(go, "anc_rIndex1"));
                mAnchors.Add(FindGameObjectChild(go, "anc_rIndex2"));
                mAnchors.Add(FindGameObjectChild(go, "anc_rIndex3"));

                mAnchors.Add(FindGameObjectChild(go, "anc_rMid1"));
                mAnchors.Add(FindGameObjectChild(go, "anc_rMid2"));
                mAnchors.Add(FindGameObjectChild(go, "anc_rMid3"));

                mAnchors.Add(FindGameObjectChild(go, "anc_rRing1"));
                mAnchors.Add(FindGameObjectChild(go, "anc_rRing2"));
                mAnchors.Add(FindGameObjectChild(go, "anc_rRing3"));

                mAnchors.Add(FindGameObjectChild(go, "anc_rCarpal4"));
                mAnchors.Add(FindGameObjectChild(go, "anc_rPinky1"));
                mAnchors.Add(FindGameObjectChild(go, "anc_rPinky2"));
                mAnchors.Add(FindGameObjectChild(go, "anc_rPinky3"));

                // L anchors
                mAnchors.Add(FindGameObjectChild(go, "anc_lHand"));
                mAnchors.Add(FindGameObjectChild(go, "anc_lThumb1"));
                mAnchors.Add(FindGameObjectChild(go, "anc_lThumb2"));
                mAnchors.Add(FindGameObjectChild(go, "anc_lThumb3"));

                mAnchors.Add(FindGameObjectChild(go, "anc_lIndex1"));
                mAnchors.Add(FindGameObjectChild(go, "anc_lIndex2"));
                mAnchors.Add(FindGameObjectChild(go, "anc_lIndex3"));

                mAnchors.Add(FindGameObjectChild(go, "anc_lMid1"));
                mAnchors.Add(FindGameObjectChild(go, "anc_lMid2"));
                mAnchors.Add(FindGameObjectChild(go, "anc_lMid3"));

                mAnchors.Add(FindGameObjectChild(go, "anc_lRing1"));
                mAnchors.Add(FindGameObjectChild(go, "anc_lRing2"));
                mAnchors.Add(FindGameObjectChild(go, "anc_lRing3"));

                mAnchors.Add(FindGameObjectChild(go, "anc_lCarpal4"));
                mAnchors.Add(FindGameObjectChild(go, "anc_lPinky1"));
                mAnchors.Add(FindGameObjectChild(go, "anc_lPinky2"));
                mAnchors.Add(FindGameObjectChild(go, "anc_lPinky3"));

                int ctr = 0;
                foreach ( GameObject g in mAnchors)
                {
                    if ( g == null)
                    {
                        mAnchors = new List<GameObject>();
                        Debug.Log("My avatar failed:"+ ctr + "# not found");
                        return;
                    }
                    ctr++;
                }
             
                _AvatarFound = true;
                Debug.Log("My avatar found!");
                CancelInvoke("GetMyAnchors");
                return;

            }
        }

    }
    private void UpdateAnchors() // update all anchors objects
    {
        // 34 anchor to update
        if (!_AvatarFound || !righthand.GetComponent<OVRHand>().IsTracked || !lefthand.GetComponent<OVRHand>().IsTracked)
            return; // return if not required


        for (int i = 0; i < 34; i++)
        {
            switch (i)
            {
                case 0:
                    mAnchors[i].transform.rotation = head.transform.rotation;
                    mAnchors[i].transform.position = head.transform.position;
                    break; // the head ...
                case 1:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[0].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[0].Transform.position;
                    break; // rHand
                case 2:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[2].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[2].Transform.position;
                    break; //rthumb1
                case 3:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[4].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[4].Transform.position;
                    break; //rthumb2
                case 4:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[5].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[5].Transform.position;
                    break; //rthumb3
                case 5:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[6].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[6].Transform.position;
                    break; //rIndex1
                case 6:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[7].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[7].Transform.position;
                    break; //rIndex2
                case 7:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[8].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;
                    break; //rIndex3
                case 8:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[9].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[9].Transform.position;
                    break; //rMid1
                case 9:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[10].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[10].Transform.position;
                    break; //rMid2
                case 10:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[11].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[11].Transform.position;
                    break; //rMid3
                case 11:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[12].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[12].Transform.position;
                    break; //rRing1
                case 12:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[13].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[13].Transform.position;
                    break; //rRing2
                case 13:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[14].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[14].Transform.position;
                    break; //rRing3
                case 14:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[15].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[15].Transform.position;
                    break; //rCarpal4
                case 15:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[16].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[16].Transform.position;
                    break; //rPinky1
                case 16:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[17].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[17].Transform.position;
                    break; //rPinky2
                case 17:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[18].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[18].Transform.position;
                    break; //rPinky3

                case 18:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[0].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[0].Transform.position;
                    break; // lHand
                case 19:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[2].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[2].Transform.position;
                    break; //lthumb1
                case 20:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[4].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[4].Transform.position;
                    break; //lthumb2
                case 21:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[5].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[5].Transform.position;
                    break; //lthumb3
                case 22:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[6].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[6].Transform.position;
                    break; //lIndex1
                case 23:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[7].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[7].Transform.position;
                    break; //lIndex2
                case 24:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[8].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;
                    break; //lIndex3
                case 25:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[9].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[9].Transform.position;
                    break; //lMid1
                case 26:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[10].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[10].Transform.position;
                    break; //lMid2
                case 27:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[11].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[11].Transform.position;
                    break; //lMid3
                case 28:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[12].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[12].Transform.position;
                    break; //lRing1
                case 29:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[13].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[13].Transform.position;
                    break; //lRing2
                case 30:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[14].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[14].Transform.position;
                    break; //lRing3
                case 31:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[15].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[15].Transform.position;
                    break; //lCarpal4
                case 32:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[16].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[16].Transform.position;
                    break; //lPinky1
                case 33:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[17].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[17].Transform.position;
                    break; //lPinky2
                case 34:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[18].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[18].Transform.position;
                    break; //lPinky3
            }
        }

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

    void SpawnPrimitive() // for now, only cube, but we could spawn light or other stuff i guess...
    {
        GameObject inst = GameObject.CreatePrimitive(PrimitiveType.Cube);
        inst.transform.position = Camera.main.transform.position;
        inst.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        inst.AddComponent<ModifMesh>();
        inst.GetComponent<ModifMesh>().ForceStart();
        if (!inst.GetComponent<MeshCollider>())
        {
            inst.AddComponent<MeshCollider>();
        }
        //inst.GetComponent<Rigidbody>().isKinematic = false;
        inst.tag = "3DMESH";
        inst.name = GetUniqueName();
        // this new one has to get a unique name // but we also need to name 


        // send to server 
        List<byte> data = new List<byte>();
        data.Add(3);
        data = AddBytesToList(data, BitConverter.GetBytes(inst.transform.position.x));
        data = AddBytesToList(data, BitConverter.GetBytes(inst.transform.position.y));
        data = AddBytesToList(data, BitConverter.GetBytes(inst.transform.position.z));
        foreach (char c in inst.name.ToCharArray())  // always 64 bytes
        {
            data.Add((byte)c);
        }

        // instantiate the thing 
        GameObject[] allAvatar = GameObject.FindGameObjectsWithTag("Avatar");
        foreach (GameObject go in allAvatar)
        {
            if (go.GetComponent<DataReceiver>()._isMine)
            {
                go.GetComponent<DataReceiver>().SendData(ListToByteArray(data));
                break;
            }
        }
    }

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
                    pieces[1].name = GetUniqueName();
                    // next set a name for those go .. piece 0 will be ancient name , piece 1 will be a new hash name 

                    List<byte> data = new List<byte>();
                    data.Add(5); // the header for cutting will be five.
                    data.Add((byte)ancient_name.Length);
                    for (int i = 0; i < ancient_name.ToCharArray().Length; i++)
                    {
                        data.Add((byte)ancient_name.ToCharArray()[i]);
                    }
                    data = AddBytesToList(data, BitConverter.GetBytes(righthand.transform.position.x));
                    data = AddBytesToList(data, BitConverter.GetBytes(righthand.transform.position.y));
                    data = AddBytesToList(data, BitConverter.GetBytes(righthand.transform.position.z));
                    data = AddBytesToList(data, BitConverter.GetBytes(righthand.transform.right.x));
                    data = AddBytesToList(data, BitConverter.GetBytes(righthand.transform.right.y));
                    data = AddBytesToList(data, BitConverter.GetBytes(righthand.transform.right.z));
                    foreach (char c in pieces[1].name.ToCharArray())
                    {
                        data.Add((byte)c);
                    }
                    // add the name of pieces 2 

                    GameObject[] allAvatar = GameObject.FindGameObjectsWithTag("Avatar");
                    foreach (GameObject go in allAvatar)
                    {
                        if (go.GetComponent<DataReceiver>()._isMine)
                        {
                            go.GetComponent<DataReceiver>().SendData(ListToByteArray(data));
                            return;
                        }
                    }
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
    bool _grabbing = false;
    void StartGrabbingVertices()
    {
        if (!_grabbing)
        {
            _grabbing = true;
            // get vertices point near heand // get all gameobject tag 3DMesh
            GameObject[] smeshes = GameObject.FindGameObjectsWithTag("3DMESH");
            foreach (GameObject go in smeshes)
            {
                if (go.GetComponent<ModifMesh>() != null)
                {
                    StartCoroutine(go.GetComponent<ModifMesh>().GetVerticesNearHand());

                }
            }
        }
    }

    void StopGrabbingVertices()
    {
        if (_grabbing)
        {
            _grabbing = false;
            // cancel all update
            GameObject[] smeshes = GameObject.FindGameObjectsWithTag("3DMESH");
            foreach (GameObject go in smeshes)
            {
                if (go.GetComponent<ModifMesh>() != null)
                {
                    go.GetComponent<ModifMesh>().StopGrabbingVertices();

                }
            }

        }
    }

    bool _deplacing = false;
    void StartMovingMesh()
    {
        if (!_deplacing)
        {
            _deplacing = true;
            // get vertices point near heand // get all gameobject tag 3DMesh
            GameObject[] smeshes = GameObject.FindGameObjectsWithTag("3DMESH");
            foreach (GameObject go in smeshes)
            {
                if (go.GetComponent<ModifMesh>() != null)
                {
                    StartCoroutine(go.GetComponent<ModifMesh>().StartDeplacingFullMesh());

                }
            }
        }
    }

    void StopMovingMesh()
    {
        if (_deplacing)
        {
            _deplacing = false;
            // cancel all update
            GameObject[] smeshes = GameObject.FindGameObjectsWithTag("3DMESH");
            foreach (GameObject go in smeshes)
            {
                if (go.GetComponent<ModifMesh>() != null)
                {
                    go.GetComponent<ModifMesh>().StopMovingEntireMesh();

                }
            }

        }
    }

    void OldRotateStuff() // unused..
    {
        /*
        if (primary2DAxisValueB != Vector2.zero)
        {
            this.transform.Rotate(0, (primary2DAxisValueB.x * 5), 0);
        }
        */

    }


    // the loop
    void Update()
    {
        // [0] update Anchors 
        UpdateAnchors();
        // [1] Get Gestures ID for both hand.

        // [2] Then, foreach Gestures ID detected, interact with the world.
       
    }

    // Misc
    public string GetUniqueName()
    {
        double ts = (double)(DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1))).TotalMilliseconds;
        string hashname = SHAToHex(ComputeSHA(BitConverter.GetBytes(ts)), false);
        return hashname;
        // hashame will always be equal to 
    }
    public byte[] ComputeSHA(byte[] msg)
    {

        SHA256 sha = SHA256.Create();
        byte[] result = sha.ComputeHash(msg);
        return result;
    }
    public string SHAToHex(byte[] bytes, bool upperCase)
    {
        StringBuilder result = new StringBuilder(bytes.Length * 2);

        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

        return result.ToString();
    }

    public static List<byte> AddBytesToList(List<byte> list, byte[] bytes)
    {
        foreach (byte b in bytes) { list.Add(b); }
        return list;
    }
    public static byte[] ListToByteArray(List<byte> list)
    {
        byte[] result = new byte[list.Count];
        for (int i = 0; i < list.Count; i++) { result[i] = list[i]; }
        return result;
    }

}
