using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grabbable : MonoBehaviour
{
    /// @ Add this script to any Object you want to be grabbable... 
    /// NULL ERROR : its seems that it was RigidBody which caused null error. Creating bad syncing when grabbing end...
    public bool isSync = true;
    public float BoundsMultiplier = 1f;
    public bool EnablePinchGrabbing = false;

    [Header("Debug")]
    public bool _grabbed = false;
    public bool _userisgrabber = false;

    private Coroutine grabbingRoutine;

    private Bounds maxBoundaries;

    private GameObject LeftHand;
    private GameObject RightHand;

    private GameObject rootOrigin;

    private bool WaitForReleasingAction;

    public enum GrabbingMode { LeftTouch, LeftHand, RightTouch, RightHand }
    [HideInInspector] public GrabbingMode _currentGrabbingMode;

    private void Awake()
    {
        transform.parent = null;
    }
    private void Start()
    {
        LeftHand = GameObject.Find("OVRlefthand");
        RightHand = GameObject.Find("OVRrighthand");
        CalculateBoundaries();
        rootOrigin = transform.root.gameObject;
    }
    public void CalculateBoundaries() // sometimes it is needed
    {
        maxBoundaries = ObjectUtilities.GetBoundsOfGroupOfMesh(this.gameObject);
        maxBoundaries.size *= BoundsMultiplier;
    }
    private void Update()
    {
        if (WaitForReleasingAction)
        {
            if (DoesUserDoReleasingAction(_currentGrabbingMode))
                WaitForReleasingAction = false;
        }
        if (!WaitForReleasingAction)
            TryGrabbing();
    }
    /// 002         menu is running
    /// 008         user is grabbing something with left hand / touch
    /// 009         user is grabbing something with right hand / touch
    void TryGrabbing()
    {
        if ( GameStatus.IsGameFlagSet(2) // user in menu
            || _grabbed)
            return;

        // Can do bad performance
        CalculateBoundaries();

        for (int i = 0; i < 4; i++)
        {
            if (DoesUserDoGrabbingAction((GrabbingMode)i))
            {
                grabbingRoutine = StartCoroutine(ProccessUserGrabbing((GrabbingMode)i));
                return;
            }
        }

    }

    bool DoesUserDoGrabbingAction(GrabbingMode mode)
    {
        if (mode == GrabbingMode.LeftTouch)
        {
            if (GameStatus.IsGameFlagSet(8)) // User is already grabbing with left touch
                return false;
            Vector3 pos = ControllerData.GetLeftControllerPosition();
            if (Vector3.Distance(pos, this.transform.position) > 0.7f)
                return false;
            // Check if controller contains inside
            if (!maxBoundaries.Contains(pos))
                return false;
            // check if Trigger pressed
            if (!ControllerData.GetButtonPressed(ControllerData.Button.LeftMainTrigger))
                return false;

            return true;
        }
        if (mode == GrabbingMode.RightTouch)
        {
            if (GameStatus.IsGameFlagSet(9)) // User is already grabbing with left touch
                return false;
            Vector3 pos = ControllerData.GetRightControllerPosition();
            if (Vector3.Distance(pos, this.transform.position) > 0.7f)
                return false;
            // Check if controller contains inside
            if (!maxBoundaries.Contains(pos))
                return false;
            // check if Trigger pressed
            if (!ControllerData.GetButtonPressed(ControllerData.Button.RightMainTrigger))
                return false;

            return true;
        }
        if (mode == GrabbingMode.LeftHand)
        {
            if (GameStatus.IsGameFlagSet(8)) // User is already grabbing with left touch
                return false;
            if (LeftHand.GetComponent<OVRSkeleton>().Bones.Count == 0)
                return false;
            if (Vector3.Distance(LeftHand.transform.position, this.transform.position) > 0.7f)
                return false;
            
            // Do different Logic if PinchGrabbing
            if (EnablePinchGrabbing)
            {
                if (   !maxBoundaries.Contains(LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position) 
                    && !maxBoundaries.Contains(LeftHand.GetComponent<OVRSkeleton>().Bones[5].Transform.position))
                    return false;

                if (!HandUtilities.IsLeftHandClosed() && !HandUtilities.DoesAnyPinchDetectedOnLeftHand())
                    return false;
            }
            else
            {
                if (!maxBoundaries.Contains(LeftHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position))
                    return false;
                if (!HandUtilities.IsLeftHandClosed())
                    return false;
            }

            return true;
        }
        if (mode == GrabbingMode.RightHand)
        {
            if (GameStatus.IsGameFlagSet(9)) // User is already grabbing with right touch
                return false;
            if (RightHand.GetComponent<OVRSkeleton>().Bones.Count == 0)
                return false;
            if (Vector3.Distance(RightHand.transform.position, this.transform.position) > 0.7f)
                return false;
            
            // Do different Logic if PinchGrabbing
            
            if (EnablePinchGrabbing)
            {
                if (!maxBoundaries.Contains(RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position)
                    && !maxBoundaries.Contains(RightHand.GetComponent<OVRSkeleton>().Bones[5].Transform.position))
                    return false;

                if (!HandUtilities.IsRightHandClosed() && !HandUtilities.DoesAnyPinchDetectedOnRightHand())
                    return false;
            }
            else
            {
                if (!maxBoundaries.Contains(RightHand.GetComponent<OVRSkeleton>().Bones[9].Transform.position))
                    return false;
                if (!HandUtilities.IsRightHandClosed())
                    return false;
            }
            return true;
        }
        return false;
    }
    bool DoesUserDoReleasingAction(GrabbingMode mode)
    {
        if (mode == GrabbingMode.LeftTouch)
        {
            return !ControllerData.GetButtonPressed(ControllerData.Button.LeftMainTrigger);
        }
        if (mode == GrabbingMode.RightTouch)
        {
            return !ControllerData.GetButtonPressed(ControllerData.Button.RightMainTrigger);
        }
        if (mode == GrabbingMode.LeftHand)
        {
            return !HandUtilities.IsHandClosed(LeftHand);
        }
        if (mode == GrabbingMode.RightHand)
        {
            return !HandUtilities.IsHandClosed(RightHand);
        }
        return false;
    }

    void AttachObjectToAvatarAnchors(GameObject Avatar, GrabbingMode mode) 
    {
        GameObject anchor = null;

        if (mode == GrabbingMode.LeftTouch)
        {
            //anc_lMid1
            anchor = ObjectUtilities.FindGameObjectChild(Avatar, "anc_lMid1");
        }
        if (mode == GrabbingMode.RightTouch)
        {
            //anc_rMid1
            anchor = ObjectUtilities.FindGameObjectChild(Avatar, "anc_rMid1");
        }
        if (mode == GrabbingMode.LeftHand)
        {
            //anc_ltouch
            anchor = ObjectUtilities.FindGameObjectChild(Avatar, "anc_ltouch");
        }
        if (mode == GrabbingMode.RightHand)
        {
            //anc_rtouch
            anchor = ObjectUtilities.FindGameObjectChild(Avatar, "anc_rtouch");
        }
        if (!anchor)
            return;

        transform.root.parent = anchor.transform;
    }
    GameObject AttachObjectToUserAnchors(GrabbingMode mode)
    {
        AnchorUpdater Updater = FindObjectOfType<AnchorUpdater>();
        
        if (!Updater)
            return null;

        GameObject anchor = null;

        if (mode == GrabbingMode.LeftTouch)
        {
            anchor = Updater.mAnchors[36];
        }
        if (mode == GrabbingMode.RightTouch)
        {
            anchor = Updater.mAnchors[37];
        }
        if (mode == GrabbingMode.LeftHand)
        {
            anchor = Updater.mAnchors[26];
        }
        if (mode == GrabbingMode.RightHand)
        {
            anchor = Updater.mAnchors[9];
        }
        if (!anchor)
            return null;
        
        transform.root.parent = anchor.transform;

        return anchor.gameObject;
    }

    // @ Force Releasing the object. 
    public void ForceReleasing()
    {
        if (!_grabbed )
            return;

       

        GameStatus.UnsetGameFlag(8); GameStatus.UnsetGameFlag(9);
        _grabbed = false; _userisgrabber = false;
        // Detach Object 
        rootOrigin.transform.parent = null;
        // Enable physics
        ObjectUtilities.EnablePhysicsOnObject(this.gameObject);
        // Sync SyncGrabbingEnd
        if (!_userisgrabber)
        {
            // Wait for Grabbed released ..
            WaitForReleasingAction = true;
            // Sync end of grabbing
            SyncGrabbingEnd(Vector3.zero, Vector3.zero);
        }
            

        try
        {
            if (grabbingRoutine != null)
                StopCoroutine(grabbingRoutine);
        }
        catch (System.Exception e) { }
       
        
    }
    IEnumerator ProccessUserGrabbing(GrabbingMode m)
    {
        // Set    Game Status depending of Mode
        int gflag = (int)m < 2 ? 8 : 9;
        GameStatus.SetGameFlag(gflag);
        _currentGrabbingMode = m;
        
        _grabbed = true;
        _userisgrabber = true;

        // Disable physics
        ObjectUtilities.DisablePhysicsOnObject(this.gameObject);

        // Attach Object depending of Mode
        GameObject tempAnchor = AttachObjectToUserAnchors(m);

        // Sync
        SyncGrabbingStart(m, this.transform.localPosition, this.transform.localEulerAngles);

        // Do nothing while grabbing
        while (!DoesUserDoReleasingAction(m))
            yield return new WaitForEndOfFrame();

        // Detach Object depending of Mode
        GameStatus.UnsetGameFlag(gflag);
        _grabbed = false;
        _userisgrabber = false;
        
        // Detach Object 
        rootOrigin.transform.parent = null;

        // Enable physics
        ObjectUtilities.EnablePhysicsOnObject(this.gameObject);

        // Do fun physics (only with touch control (even number grabbing mode)
        Vector3 linearVelocity = Vector3.zero;
        Vector3 angularVelocity = Vector3.zero;
        if ((int)m % 2 == 0)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb && tempAnchor)
            {
                OVRInput.Controller controller = (int)m == 0 ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
                OVRPose localPose = new OVRPose { position = OVRInput.GetLocalControllerPosition(controller), orientation = OVRInput.GetLocalControllerRotation(controller) };
                OVRPose trackingSpace = tempAnchor.transform.ToOVRPose() * localPose.Inverse();
                linearVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerVelocity(controller);
                angularVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerAngularVelocity(controller);
                rb.velocity = linearVelocity * 2;
                rb.angularVelocity = angularVelocity * 2;
            }
        }

        // Sync
        SyncGrabbingEnd(linearVelocity, angularVelocity);
        yield break;
    }

    #region Net
    private void SyncGrabbingStart(GrabbingMode g, Vector3 localPosition, Vector3 localEulers)
    {
        if (!isSync)
            return;

        List<byte> data = new List<byte>();
        data.Add(50);
        data.Add((byte)this.gameObject.name.Length);
        foreach (char c in this.gameObject.name.ToCharArray())
        {
            data.Add((byte)c);
        }
        // Add releasing mode info
        data.Add((byte)g);
        // Add grabbing offset 
        data.AddRange(BinaryUtilities.Vector3Tobytes(localPosition));
        data.AddRange(BinaryUtilities.Vector3Tobytes(localEulers));

        NetUtilities.SendDataToAll(data.ToArray());
    }
    private void SyncGrabbingEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        if (!isSync)
            return;
        List<byte> data = new List<byte>();
        data.Add(51);
        data.Add((byte)this.gameObject.name.Length);
        foreach (char c in this.gameObject.name.ToCharArray())
        {
            data.Add((byte)c);
        }
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.SerializeTransform(this.gameObject.transform));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(linearVelocity));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(angularVelocity));
        NetUtilities.SendDataToAll(data.ToArray());
    }
    public static void OnGrabbingReceived(byte[] msg, PhotonMessageInfo info)
    {
        byte namesize = msg[1];
        char[] goName = new char[namesize];
        for (int i = 2; i < 2 + namesize; i++)
        {
            goName[i - 2] = (char)msg[i];
        }
        GameObject avatar = NetUtilities.GetAvatarRootObjectByInfo(info);
        GameObject vObj = GameObject.Find(new string(goName));
        if (!vObj || !avatar ) return;
        Grabbable grabbable = vObj.GetComponent<Grabbable>();
        if (!grabbable) return;
       
        GrabbingMode mode = (GrabbingMode)msg[2 + namesize];

        grabbable._grabbed = true;
        ObjectUtilities.DisablePhysicsOnObject(vObj);
        grabbable.AttachObjectToAvatarAnchors(avatar,mode);
        grabbable._currentGrabbingMode = mode;

        Vector3 lpos = BinaryUtilities.BytesToVector3(ref msg, 3 + namesize);
        Vector3 lrot = BinaryUtilities.BytesToVector3(ref msg, 15 + namesize);

        vObj.transform.localPosition = lpos;
        vObj.transform.localEulerAngles = lrot;
    }
    public static void OnReleasingReceived(byte[] msg, PhotonMessageInfo info)
    {
        byte namesize = msg[1];

        char[] goName = new char[namesize];
        for (int i = 2; i < 2 + namesize; i++)
        {
            goName[i - 2] = (char)msg[i];
        }

        GameObject vObj = GameObject.Find(new string(goName));
        if (vObj == null) return;
        Grabbable grabbable = vObj.GetComponent<Grabbable>();
        if (!grabbable) return;

        grabbable.GetComponent<Grabbable>()._grabbed = false;
        BinaryUtilities.DeSerializeTransformOnObject(ref msg, 2 + namesize, vObj);
        // Detach Object 
        grabbable.rootOrigin.transform.parent = null;
        ObjectUtilities.EnablePhysicsOnObject(vObj);
        Rigidbody rb = vObj.GetComponent<Rigidbody>();
        if (rb)
        {
            Vector3 linearVelocity = BinaryUtilities.BytesToVector3(ref msg, 2 + namesize + 36);
            Vector3 angularVelocity = BinaryUtilities.BytesToVector3(ref msg, 2 + namesize + 48);
            rb.velocity = linearVelocity;
            rb.angularVelocity = angularVelocity;
        }
    }

    #endregion

}
