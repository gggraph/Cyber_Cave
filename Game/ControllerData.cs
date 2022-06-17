using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerData : MonoBehaviour
{
    ///
    /// Access To Controller Variable.
    /// This Script has to be set on any object in scene to refresh Up/Down Status of Controller Triggers. 
    ///
    public void Start()
    {
        GetButtonDown(Button.A);
    }
    public enum Button : int
    {
        A  = 0, 
        B  = 1,
        X =  2,
        Y =  3,
        RightStick = 4,
        RightMainTrigger = 5,
        RighSideTrigger = 6,
        LeftStick = 7,
        LeftMainTrigger = 8,
        LeftSideTrigger = 9,
        Oculus = 10, 
        Menu = 11
    }
    public static bool CheckSanity()
    {
        return false;
    }
    public static bool IsBothControllersUsed()
    {
        if (IsLeftControllerUsed() && IsRightControllerUsed())
            return true;
        return false;
    }
    public static bool IsAnyControllerUsed()
    {
        if (IsLeftControllerUsed() || IsRightControllerUsed())
            return true;
        return false;
    }
    public static bool IsLeftControllerUsed() // seems weird
    {
        return OVRInput.GetControllerPositionTracked(OVRInput.Controller.LTouch);
        //return OVRInput.IsControllerConnected(OVRInput.Controller.LTouch);
    }
    public static bool IsRightControllerUsed()
    {
        return OVRInput.GetControllerPositionTracked(OVRInput.Controller.RTouch);
        //return OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);
    }

    public static Vector3 GetLeftControllerPosition()
    {
        return Camera.main.transform.root.gameObject.transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch));
    }
    public static Vector3 GetLeftControllerRotation() // not working properly...
    {
        //return Camera.main.transform.root.gameObject.transform.TransformDirection(OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch).eulerAngles);
        return Camera.main.transform.root.gameObject.transform.eulerAngles + OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch).eulerAngles;
    }
    public static Vector3 GetRightControllerPosition()
    {
        return Camera.main.transform.root.gameObject.transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
    }
    public static Vector3 GetRightControllerRotation()
    {
        // return Camera.main.transform.root.gameObject.transform.TransformDirection(OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch).eulerAngles);
        return Camera.main.transform.root.gameObject.transform.eulerAngles + OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch).eulerAngles;
    }
    public static bool DoesLeftTouchPointingObject_SphereCast(GameObject o, float radius = 1f, float distance = 10f)
    {
        // Get Controller position (which is anchors) 
        if (!IsLeftControllerUsed())
            return false;
        GameObject mFG3D = Camera.main.transform.root.gameObject.GetComponent<AnchorUpdater>().mFG3D;
        if (!mFG3D)
            return false;
        GameObject TouchObject = mFG3D.GetComponent<BodyAnimation>().LTouchObject;
        RaycastHit[] hits;
        Ray ray = new Ray(TouchObject.transform.position, TouchObject.transform.forward);
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
    public static bool DoesRightTouchPointingObject_SphereCast(GameObject o, float radius = 1f, float distance = 10f)
    {
        // Get Controller position (which is anchors) 
        if (!IsRightControllerUsed())
            return false;

        GameObject mFG3D = Camera.main.transform.root.gameObject.GetComponent<AnchorUpdater>().mFG3D;
        if (!mFG3D)
            return false;
        GameObject TouchObject = mFG3D.GetComponent<BodyAnimation>().RTouchObject;
        RaycastHit[] hits;
        Ray ray = new Ray(TouchObject.transform.position, TouchObject.transform.forward);
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
    // Tout cela est a revoir. 
    public static bool GetButtonDown(Button b)
    {
        switch (b)
        {
            case Button.A:
                return OVRInput.GetDown(OVRInput.Button.One);
            case Button.B:
                return OVRInput.GetDown(OVRInput.Button.Two);
            case Button.X:
                return OVRInput.GetDown(OVRInput.Button.Three);
            case Button.Y:
                return OVRInput.GetDown(OVRInput.Button.Four);
            case Button.LeftMainTrigger:
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch) > 0.6f)
                    return true;
                else
                    return false;
            case Button.LeftSideTrigger:
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.Touch) > 0.6f)
                    return true;
                else
                    return false;
            case Button.RighSideTrigger:
                if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch) > 0.6f)
                    return true;
                else
                    return false;
            case Button.RightMainTrigger:
                if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) > 0.6f)
                    return true;
                else
                    return false;
            case Button.RightStick:
                return OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick);
            case Button.LeftStick:
                return OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick);
            case Button.Oculus:
                return OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick);
            case Button.Menu:
                return OVRInput.GetDown(OVRInput.Button.Start);
        }

        return false;
       
    }

    public static bool GetButtonUp(Button b)
    {
        return false;
    }

    public static bool GetButtonPressed(Button b)
    {

        switch (b)
        {
            case Button.A:
                return OVRInput.Get(OVRInput.Button.One);
            case Button.B:
                return OVRInput.Get(OVRInput.Button.Two);
            case Button.X:
                return OVRInput.Get(OVRInput.Button.Three);
            case Button.Y:
                return OVRInput.Get(OVRInput.Button.Four);
            case Button.LeftMainTrigger:
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch) > 0.6f)
                    return true;
                else
                    return false;
            case Button.LeftSideTrigger:
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.Touch) > 0.6f)
                    return true;
                else
                    return false;
            case Button.RighSideTrigger:
                if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch) > 0.6f)
                    return true;
                else
                    return false;
            case Button.RightMainTrigger:
                if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) > 0.6f)
                    return true;
                else
                    return false;
            case Button.RightStick:
                return OVRInput.Get(OVRInput.Button.SecondaryThumbstick);
            case Button.LeftStick:
                return OVRInput.Get(OVRInput.Button.PrimaryThumbstick);
            case Button.Oculus:
                return OVRInput.Get(OVRInput.Button.SecondaryThumbstick);
            case Button.Menu:
                return OVRInput.Get(OVRInput.Button.SecondaryThumbstick);
        }

        return false;
    }
 
    public static Vector2 GetLeftJoystickData()
    {
        if (!IsLeftControllerUsed())
            return new Vector2();
        return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
    }
    public static Vector2 GetRightJoystickData()
    {
        if (!IsRightControllerUsed())
            return new Vector2();
        return OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
    }
    public static void SetLeftTouchVibration(int iteration, int frequency, int strength)
    {
        OVRHapticsClip clip = new OVRHapticsClip();
        for (int i = 0; i < iteration; i++)
        {
            clip.WriteSample(i % frequency == 0 ? (byte)strength : (byte)0);
        }
        OVRHaptics.LeftChannel.Preempt(clip);
    }
    public static void SetRightTouchVibration(int iteration, int frequency, int strength)
    {
        OVRHapticsClip clip = new OVRHapticsClip();
        for (int i = 0; i < iteration; i++)
        {
            clip.WriteSample(i % frequency == 0 ? (byte)strength : (byte)0);
        }
        OVRHaptics.RightChannel.Preempt(clip);
    }
}
