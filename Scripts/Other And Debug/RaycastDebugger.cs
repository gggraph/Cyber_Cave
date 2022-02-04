using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastDebugger : MonoBehaviour
{
    public LineRenderer LeftRay;
    public LineRenderer RightRay;

    public void Start()
    {
        CreateLineRenderers();
    }

    public void Update()
    {
        GameObject righthand = GameObject.Find("OVRrighthand");
        GameObject lefthand = GameObject.Find("OVRlefthand");

        if (righthand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return;
        if (lefthand.GetComponent<OVRSkeleton>().Bones.Count == 0)
            return;


        RaycastHit[] hits;

        Vector3 fromPosition = lefthand.GetComponent<OVRSkeleton>().Bones[7].Transform.position;
        Vector3 toPosition = lefthand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;
        Vector3 direction = toPosition - fromPosition;


        Ray ray = new Ray(fromPosition, direction);
        hits = Physics.SphereCastAll(ray, 0.2f, 0.00001f);

        LeftRay.SetPosition(0, fromPosition);
        foreach ( RaycastHit hit in hits)
        {
            LeftRay.SetPosition(1, hit.point);
        }
        fromPosition = righthand.GetComponent<OVRSkeleton>().Bones[7].Transform.position;
        toPosition = righthand.GetComponent<OVRSkeleton>().Bones[8].Transform.position;
        direction = toPosition - fromPosition;
        ray = new Ray(fromPosition, direction);
        hits = Physics.SphereCastAll(ray, 0.2f, 0.00001f);
        RightRay.SetPosition(0, fromPosition);
        foreach (RaycastHit hit in hits)
        {
            RightRay.SetPosition(1, hit.point);
        }
    }
    public void CreateLineRenderers()
    {
        GameObject  lobj = new GameObject();
        LeftRay = lobj.AddComponent<LineRenderer>();

        GameObject robj = new GameObject();
        RightRay = robj.AddComponent<LineRenderer>();

        LeftRay.alignment = LineAlignment.TransformZ;
        RightRay.alignment = LineAlignment.TransformZ;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
                   new GradientColorKey[] { new GradientColorKey(PaintingTool.BrushColor, 1.0f), new GradientColorKey(PaintingTool.BrushColor, 1.0f) },
                   new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 1.0f), new GradientAlphaKey(1.0f, 1.0f) }
               );
        RightRay.colorGradient = gradient;
        RightRay.startWidth = PaintingTool.BrushSize;
        RightRay.endWidth = PaintingTool.BrushSize;
        RightRay.positionCount = 2;

        LeftRay.colorGradient = gradient;
        LeftRay.startWidth = PaintingTool.BrushSize;
        LeftRay.endWidth = PaintingTool.BrushSize;
        LeftRay.positionCount = 2;


    }
 
}
