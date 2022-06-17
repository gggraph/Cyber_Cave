using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorUpdater : MonoBehaviour
{
    public GameObject head;
    public GameObject lefthand;
    public GameObject righthand;
    public List<GameObject> mAnchors = new List<GameObject>();

    public GameObject mAvatar;
    public GameObject mFG3D;

    public bool HandTracked = false;
    public bool TouchTracked = false;

    void Start()
    {
        head = GameObject.Find("CenterEyeAnchor");
        lefthand = GameObject.Find("OVRlefthand");
        righthand = GameObject.Find("OVRrighthand");

    }
    public static void SwitchToOffLineMode()
    {
        AnchorUpdater au = Camera.main.transform.root.gameObject.GetComponent<AnchorUpdater>();
        au.StartCoroutine(au.TryUpdatingAnchors_Offline());
    }
    public static void SwitchToOnlineMode()
    {
        AnchorUpdater au = Camera.main.transform.root.gameObject.GetComponent<AnchorUpdater>();
        au.StartCoroutine(au.TryUpdatingAnchors_Online());
    }

    public IEnumerator TryUpdatingAnchors_Offline()
    {
        Debug.Log("switching to Offline");
        mAvatar = null;
        mFG3D = null;
        mAnchors = new List<GameObject>();
        while (!GetMyAnchors_Offline()) { yield return new WaitForSeconds(2f); }
        mAvatar.name = "My Avatar Offline";
        // Inform some stuff too like tracking status 
        Character.SyncTrackingStatusByte();
        yield break;
    }
    public IEnumerator TryUpdatingAnchors_Online()
    {
        Debug.Log("switching to Online");
        mAvatar.transform.position = new Vector3(9999, 0, 0); // jerter le vieux NPC... 
        mAvatar = null;
        mFG3D = null;
        mAnchors = new List<GameObject>();
        while (!GetMyAnchors_Online()) { yield return new WaitForSeconds(2f); }
        Character.SyncTrackingStatusByte();
        mAvatar.name = "My Avatar Online";
        yield break;
    }
    private bool GetMyAnchors_Offline()
    {
        GameObject[] avatars = GameObject.FindGameObjectsWithTag("Avatar");
        if (avatars.Length > 0)
        {
            if (GetAnchorsFromAvatar(avatars[0]))
            {
                mAvatar = avatars[0];
                mFG3D = ObjectUtilities.FindGameObjectChild(mAvatar, "FG3D_Char_DeuHumans");
                // Inform some stuff too like tracking status 
                Character.SyncTrackingStatusByte();
                return true;

            }
        }
        return false;
    }
    private bool GetMyAnchors_Online()
    {
        GameObject[] avatars = GameObject.FindGameObjectsWithTag("Avatar");
        foreach (GameObject go in avatars)
        {
            if (go.GetComponent<NetStream>())
            {
                if (go.GetComponent<NetStream>() == NetUtilities._mNetStream) // problem here?
                {
                    if (GetAnchorsFromAvatar(go))
                    {
                        mAvatar = go;
                        mFG3D = ObjectUtilities.FindGameObjectChild(mAvatar, "FG3D_Char_DeuHumans");
                        Debug.LogError("Online avatar found");
                        return true;
                    }
                }
            }

        }
        return false;
    }

    private bool GetAnchorsFromAvatar(GameObject go)
    {
        mAnchors = new List<GameObject>();
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rig")); // i : 0
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_head"));

        /*
         * & all of them 
         */

        // R anchors
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rHand"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rThumb1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rThumb2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rThumb3")); // i : 5

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rIndex1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rIndex2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rIndex3"));

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rMid1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rMid2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rMid3")); //i : 11

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rRing1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rRing2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rRing3"));

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rCarpal4"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rPinky1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rPinky2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rPinky3")); // i : 18

        // L anchors
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lHand"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lThumb1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lThumb2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lThumb3")); //i : 22

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lIndex1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lIndex2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lIndex3"));

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lMid1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lMid2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lMid3"));

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lRing1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lRing2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lRing3")); //i :31

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lCarpal4"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lPinky1"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lPinky2"));
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_lPinky3")); //i : 35

        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_ltouch")); //i : 36
        mAnchors.Add(ObjectUtilities.FindGameObjectChild(go, "anc_rtouch")); //i : 37

        int ctr = 0;
        foreach (GameObject g in mAnchors)
        {
            if (g == null)
            {
                mAnchors = new List<GameObject>();

                Debug.LogError("My avatar failed:" + ctr + "# not found");
                return false;
            }
            ctr++;
        }
        return true;

    }

    private void UpdateAnchors() // update all anchors objects
    {
        // we really need to keep offset here 
        if (!mAvatar)
            return;

        if (mAnchors.Count == 0)
            return;

        mAnchors[0].transform.rotation = this.transform.rotation;
        mAnchors[0].transform.position = this.transform.position;
        if (righthand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return;

        // rotation correction? 
        mAnchors[2].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[0].Transform.rotation;
        mAnchors[2].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[0].Transform.position;
        mAnchors[19].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[0].Transform.rotation;
        mAnchors[19].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[0].Transform.position;


        // there should some weird stuff here ... 
        for (int i = 1; i < 38; i++)
        {
            switch (i)
            {
             
                case 1:
                    mAnchors[i].transform.rotation = head.transform.rotation;
                    mAnchors[i].transform.position = head.transform.position;
                    break; // the head ...
                case 2:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[0].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[0].Transform.position;
                    break; // rHand
                case 3:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[2].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[2].Transform.position;
                    break; //rthumb1
                case 4:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[4].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[4].Transform.position;
                    break; //rthumb2
                case 5:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[5].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[5].Transform.position;
                    break; //rthumb3
                case 6:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[6].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[6].Transform.position;
                    break; //rIndex1
                case 7:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[7].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[7].Transform.position;
                    break; //rIndex2
                case 8:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[8].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;
                    break; //rIndex3
                case 9:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[9].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[9].Transform.position;
                    break; //rMid1
                case 10:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[10].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[10].Transform.position;
                    break; //rMid2
                case 11:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[11].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[11].Transform.position;
                    break; //rMid3
                case 12:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[12].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[12].Transform.position;
                    break; //rRing1
                case 13:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[13].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[13].Transform.position;
                    break; //rRing2
                case 14:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[14].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[14].Transform.position;
                    break; //rRing3
                case 15:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[15].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[15].Transform.position;
                    break; //rCarpal4
                case 16:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[16].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[16].Transform.position;
                    break; //rPinky1
                case 17:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[17].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[17].Transform.position;
                    break; //rPinky2
                case 18:
                    mAnchors[i].transform.rotation = righthand.GetComponent<OVRSkeleton>().Bones[18].Transform.rotation;
                    mAnchors[i].transform.position = righthand.GetComponent<OVRSkeleton>().Bones[18].Transform.position;
                    break; //rPinky3

                case 19:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[0].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[0].Transform.position;
                    break; // lHand
                case 20:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[2].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[2].Transform.position;
                    break; //lthumb1
                case 21:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[4].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[4].Transform.position;
                    break; //lthumb2
                case 22:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[5].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[5].Transform.position;
                    break; //lthumb3
                case 23:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[6].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[6].Transform.position;
                    break; //lIndex1
                case 24:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[7].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[7].Transform.position;
                    break; //lIndex2
                case 25:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[8].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;
                    break; //lIndex3
                case 26:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[9].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[9].Transform.position;
                    break; //lMid1
                case 27:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[10].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[10].Transform.position;
                    break; //lMid2
                case 28:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[11].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[11].Transform.position;
                    break; //lMid3
                case 29:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[12].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[12].Transform.position;
                    break; //lRing1
                case 30:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[13].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[13].Transform.position;
                    break; //lRing2
                case 31:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[14].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[14].Transform.position;
                    break; //lRing3
                case 32:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[15].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[15].Transform.position;
                    break; //lCarpal4
                case 33:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[16].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[16].Transform.position;
                    break; //lPinky1
                case 34:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[17].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[17].Transform.position;
                    break; //lPinky2
                case 35:
                    mAnchors[i].transform.rotation = lefthand.GetComponent<OVRSkeleton>().Bones[18].Transform.rotation;
                    mAnchors[i].transform.position = lefthand.GetComponent<OVRSkeleton>().Bones[18].Transform.position;
                    break; //lPinky3
                case 36:
                    mAnchors[i].transform.eulerAngles = ControllerData.GetLeftControllerRotation();
                    mAnchors[i].transform.position = ControllerData.GetLeftControllerPosition();
                    break; //ltouch
                case 37:
                    mAnchors[i].transform.eulerAngles = ControllerData.GetRightControllerRotation();
                    mAnchors[i].transform.position = ControllerData.GetRightControllerPosition();
                    break; //rtouch
            }
        }

    }

    void Update()
    {
        // [0] 
        // [1] update Anchors 
        UpdateAnchors();
    }

    //----------- MIC UNUSED STUFF : Can be interesting for lipsyncing... 
    private AudioSource audioSource;
    private string _SelectedDevice;
    void ConfigureMic()
    {
        if (Microphone.devices.Length > 0)
        {
            _SelectedDevice = Microphone.devices[0].ToString();
            audioSource = GetComponent<AudioSource>(); // need audioSource source on the model 
            audioSource.clip = Microphone.Start(_SelectedDevice, true, 10, 48000);
            audioSource.loop = true;
            while (!(Microphone.GetPosition(null) > 0)) { }
            audioSource.Play();

        }
    }

    //----------- for lip sync idk .... 
    // there is some prebuilt lip sync with ovr so maybe used it when we got less static face? 
    float GetAverageVolume()
    {
        float[] data = new float[256];
        float a = 0;
        audioSource.GetOutputData(data, 0);
        foreach (float s in data)
        {
            a += Mathf.Abs(s);
        }
        return a / 256;
    }

}
