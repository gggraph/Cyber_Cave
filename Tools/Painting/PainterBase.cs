using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PainterBase : MonoBehaviour
{
    // This is just not working if no instance. Let's try to make it work with instances.
    // Material
    Material drawingMat;
    int shadowMapResolution = 1024;
    Shader depthRenderShader;

    new Camera camera
    {
        get
        {
            if (_c == null)
            {
                _c = GetComponent<Camera>();
                if (_c == null)
                    _c = gameObject.AddComponent<Camera>();
                depthOutput = new RenderTexture(shadowMapResolution, shadowMapResolution, 16, RenderTextureFormat.RFloat);
                depthOutput.wrapMode = TextureWrapMode.Clamp;
                depthOutput.Create();
                _c.targetTexture = depthOutput;
                _c.SetReplacementShader(depthRenderShader, "RenderType");
                _c.clearFlags = CameraClearFlags.Nothing;
                _c.nearClipPlane = 0.01f;
                _c.enabled = false;
            }
            return _c;
        }
    }
    public Camera _c;
    RenderTexture depthOutput;

    public void UpdateDrawingMat(float intensity, float angle, float range, Color color, Texture Mask)
    {
        var currentRt = RenderTexture.active;
        RenderTexture.active = depthOutput;
        GL.Clear(true, true, Color.white * camera.farClipPlane);
        camera.fieldOfView = angle;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = range;
        camera.cullingMask = ~(1 << 12); // Do not draw painter
        camera.Render();
        RenderTexture.active = currentRt;

        var projMatrix = camera.projectionMatrix;
        var worldToDrawerMatrix = transform.worldToLocalMatrix;

        drawingMat.SetVector("_DrawerPos", transform.position);
        drawingMat.SetFloat("_Emission", intensity * 0.02f); 
        drawingMat.SetColor("_Color", color);
        drawingMat.SetMatrix("_WorldToDrawerMatrix", worldToDrawerMatrix);
        drawingMat.SetMatrix("_ProjMatrix", projMatrix);
        drawingMat.SetTexture("_Cookie", Mask);
        drawingMat.SetTexture("_DrawerDepth", depthOutput);
    }

    public void Draw(Paintable pt)
    {
        pt.Draw(drawingMat);
    }

    // APT
    public Paintable[] paintables;
    public GameObject debugCast;
    private void Start()
    {
        // Create default variable at start 
        Material minst = Resources.Load("Materials/Hidden_SpotDrawer", typeof(Material)) as Material;
        if ( minst == null)
        {
            Debug.Log("CANNOT LOAD SHADERS!!!");
        }
        drawingMat = new Material(minst.shader);
        
        depthRenderShader = Resources.Load("Shaders/depthRender", typeof(Shader)) as Shader;
        paintables = FindObjectsOfType<Paintable>(); // should be refreshed !!!  
        
    }
    public void RefindPaintables()
    {
        paintables = FindObjectsOfType<Paintable>();
        Debug.Log("Found " + paintables.Length + " paintable objects");
    }
    public void ProjectionPaint(Painter pt)
    {
        // set this position at painter position
        this.transform.position = pt.transform.position;
        this.transform.LookAt(pt.transform.position + (pt.transform.up * 5f));
        UpdateDrawingMat(pt.intensity, pt.width, pt.RangeForSpray, pt.color, pt.Mask);
        foreach (var paintable in paintables)
            Draw(paintable);
    }
    public void ProjectionPaint_Advanced(Vector3 projectionPos, 
        Vector3 projectionDir, float intensity, float angle, float range, 
        Painter pt, Paintable ptb)
    {
        this.transform.position = projectionPos;
        this.transform.LookAt(projectionDir);
        UpdateDrawingMat(intensity, angle, range, pt.color, pt.Mask);
        Draw(ptb);
    }
    public void ForcePaintNothing(Painter pt)
    {
        UpdateDrawingMat(0f, 0f, 0f, pt.color, pt.Mask);
        foreach (var paintable in paintables)
            Draw(paintable);
    }

}
