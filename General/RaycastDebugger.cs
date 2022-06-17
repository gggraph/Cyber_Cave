using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastDebugger : MonoBehaviour
{
    private LineRenderer LeftRay;
    private LineRenderer RightRay;
    public float RayDistance = 4f;
    public float RayWidth = 0.02f;
    public bool Show = false;

    public void Start()
    {
        RayDistance = 10f;
        CreateLineRenderers();
    }

    public void ShowRays()
    {
        Show = true;
    }
    public void UnShowRays()
    {
        Show = false;
        RightRay.SetPosition(0, Vector3.zero);
        RightRay.SetPosition(1, Vector3.zero);
        LeftRay.SetPosition(0, Vector3.zero);
        LeftRay.SetPosition(1, Vector3.zero);
    }
    public void Update()
    {
        if(!Show)
            return;
        if (ControllerData.IsAnyControllerUsed())
            UpdateRayFromTouch();

    }
    private void UpdateRayFromTouch()
    {
        if (!ControllerData.IsRightControllerUsed())
            return;

        GameObject mFG3D = Camera.main.transform.root.gameObject.GetComponent<AnchorUpdater>().mFG3D;
        if (!mFG3D)
            return;
        GameObject TouchObject = mFG3D.GetComponent<BodyAnimation>().RTouchObject;
        RightRay.SetPosition(0, TouchObject.transform.position);
        RightRay.SetPosition(1, TouchObject.transform.position+TouchObject.transform.forward * RayDistance);

        TouchObject = mFG3D.GetComponent<BodyAnimation>().LTouchObject;
        LeftRay.SetPosition(0, TouchObject.transform.position);
        LeftRay.SetPosition(1, TouchObject.transform.position+TouchObject.transform.forward * RayDistance);
    }
    private void CreateLineRenderers()
    {
        GameObject  lobj = new GameObject("RayDebug");
        LeftRay = lobj.AddComponent<LineRenderer>();

        GameObject robj = new GameObject();
        RightRay = robj.AddComponent<LineRenderer>();

        LeftRay.alignment = LineAlignment.View;
        RightRay.alignment = LineAlignment.View;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
                   new GradientColorKey[] { new GradientColorKey(new Color(0,0,1), 0f), new GradientColorKey(new Color(0, 0, 1), 1f) },
                   new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.1f, 1f) }
               );

        RightRay.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        LeftRay.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

        RightRay.colorGradient = gradient;
        RightRay.startWidth = RayWidth;
        RightRay.endWidth = RayWidth;
        RightRay.positionCount = 2;

        LeftRay.colorGradient = gradient;
        LeftRay.startWidth = RayWidth;
        LeftRay.endWidth = RayWidth;
        LeftRay.positionCount = 2;



    }
 
}
