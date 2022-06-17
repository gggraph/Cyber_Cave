using UnityEngine;
using Photon.Pun;
using CaveMenu;
//using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    /// <summary>
    /// 
    /// This script is used on the OVR Camera Rig.
    /// It set up animation & rigging script. It also set up Basic Control.
    /// 
    /// </summary>

    public Transform target;
    public float rotationSpeed = 1f;

    public static byte TrackingStatus = 0;
    public static byte GetTrackingStatusByte()
    {
        bool LD = ControllerData.IsLeftControllerUsed();
        bool RD = ControllerData.IsRightControllerUsed();
        bool HD = HandUtilities.IsAnyHandTracked();

        if (HD)
            return 4;
        if (!LD && !RD && !HD)
            return 0;
        if (LD && RD && !HD)
            return 1;
        if (LD && !RD && !HD)
            return 2;
        if (!LD && RD && !HD)
            return 3;


        return 0;
    }
    public static void SyncTrackingStatusByte()
    {
        byte[] data = new byte[2];
        data[0] = 40;
        data[1] = TrackingStatus;
        NetUtilities.SendDataToAll(data);
        Debug.Log("Sync tracking : " + TrackingStatus);
        GameObject mFG3D = Camera.main.transform.root.gameObject.GetComponent<AnchorUpdater>().mFG3D;
        if (mFG3D)
        {
            mFG3D.GetComponent<BodyAnimation>().SetTrackingStatus(TrackingStatus);
        }


    }
    public static void RefreshControllerTrackingStatus(byte[] data, PhotonMessageInfo info)
    {
        GameObject cAvatar = NetUtilities.GetAvatarRootObjectByInfo(info);
        if (cAvatar == null)
            return;
        //FG3D_Char_DeuHumans
        GameObject FG3D = ObjectUtilities.FindGameObjectChild(cAvatar, "FG3D_Char_DeuHumans");
        FG3D.GetComponent<BodyAnimation>().SetTrackingStatus(data[1]);

    }
    void CheckCurrentControl()
    {
        byte s = GetTrackingStatusByte();
        if (s != TrackingStatus)
        {
            TrackingStatus = s;
            SyncTrackingStatusByte();
        }


    }

    private void Start()
    {
        target = GameObject.Find("CenterEyeAnchor").transform;
    }
    private void Update()
    {
        // update control tracking status
        CheckCurrentControl();

        // load menu if needed 
        if (!GameStatus.IsGameFlagSet(2) && GameStatus.IsGameFlagSet(5))
        {
            if ( (PoseRecognition.IsPose_OpenMenu()
                || ControllerData.GetButtonDown(ControllerData.Button.Menu)
                || Input.GetKeyDown(KeyCode.M))
                && GameStatus.IsGameFlagSet(13)
                )
            {
                 MenuCreator.OpenDefaultMenu();
            }
        }
        Vector2 leftaxis = ControllerData.GetLeftJoystickData();
        Vector2 rightaxis = ControllerData.GetRightJoystickData();
        bool BPRESSED = ControllerData.GetButtonPressed(ControllerData.Button.B);

        // Basic movement script
        if (!GameStatus.IsGameFlagSet(2))
        {
            // use left axis X to do basic movement ... 
            if (!BPRESSED)
            {
                Vector3 forward = target.forward;
                Vector3 right = target.right;
                forward.y = 0f;
                right.y = 0f;
                this.transform.position += right.normalized * (leftaxis.x / 100);
                this.transform.position += forward.normalized * (leftaxis.y / 50);
            }
            else
            {
                this.transform.position += Vector3.up * (leftaxis.y / 50);
            }

            this.transform.Rotate(0, rightaxis.x * rotationSpeed, 0);

            // this one is unused ... Only for test
            /*
            float speed = 0.02f;
            if ( Input.GetKey(KeyCode.LeftArrow))
                this.transform.position -= this.transform.right * speed;
            if (Input.GetKey(KeyCode.RightArrow))
                this.transform.position += this.transform.right * speed;
            */

        }



    }

}
