using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Concurrent;

public class Painter : MonoBehaviour
{
    /*
       Add this script to any gameObject that can be used to paint on thing.
       
    */

    PainterBase painter;
    
    public enum DrawModes { Pencil, Spray, Brush }
    
    [Header("Modes")]
    public DrawModes PaintingMode;
    public enum RayMode { Forward, BackWard, Left, Right, Up, Down }
    public RayMode RayDirection;


    [Header("Configuration")]
    public float intensity = 500f;
    public Color color = Color.white;
    [Range(0.01f, 90f)] public float width = 10f;
    public float RangeForSpray = 10f;
    [Header("Mask")]
    public Texture Mask;
    
    [Header("Debug")]
    public bool ShowSkeletonObject = false;
    public bool doAction;
    private GameObject root;
    private GameObject Shape;

    private void Start()
    {
        // Create PainterBase if not existing. PainterBase is a gameObject called PainterBase...
        GameObject pb = GameObject.Find("PainterBase");
        if (!pb)
        {
            // Create PainterBase
            GameObject pbase = new GameObject("PainterBase");
            painter = pbase.AddComponent<PainterBase>();
        }
        else
            painter = pb.GetComponent<PainterBase>();

        if ( Mask == null)
        {
            // Set Mask to default particles 
        }

        root = this.transform.root.gameObject;
        StartCoroutine(PaintSyncing(0.11f));
        StartCoroutine(ProccessingReceivedMessages());

        Shape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Shape.transform.localScale = Vector3.one * 0.1f;
        
        if (!ShowSkeletonObject)
            Shape.GetComponent<MeshRenderer>().enabled = false;

        // Add to those object a layer not renderered from painter cam to avoid blitting renderer issue
        // Set layer to this gameobject root and all its child 
        Shape.layer = 12;
        root.gameObject.layer = 12;
        for (int i = 0; i < root.transform.childCount; i++)
        {
            root.transform.GetChild(i).gameObject.layer = 12;
        }

    }
   

    Vector3 GetDirectionFromMode()
    {
        Vector3 dir = Vector3.zero;
        switch (RayDirection)
        {
            case RayMode.Forward: dir = this.transform.forward; break;
            case RayMode.BackWard: dir = -this.transform.forward; break;
            case RayMode.Left: dir = -this.transform.right; break;
            case RayMode.Right: dir = this.transform.right; break;
            case RayMode.Down: dir = -this.transform.up; break;
            case RayMode.Up: dir = this.transform.up; break;
        }
        return dir;
    }
    private void Update()
    {
        Shape.transform.position = this.transform.position + GetDirectionFromMode() * 0.2f;
      

        Grabbable gb = root.GetComponent<Grabbable>();
        if (!gb) return;
        if (!gb._userisgrabber || !gb._grabbed) return;
        
        TryPumpColorFromPicker();

        switch (PaintingMode)
        {
            case DrawModes.Pencil:
                TryPaintAsPencil(); break;
            case DrawModes.Brush:
                TryPaintAsBrush(); break;
            case DrawModes.Spray:
                TryPaintAsSpray(); break;
        }
    }

    
    void TryPaintAsSpray(bool fromNet = false)
    {
        if (fromNet)
        {
            painter.ProjectionPaint(this);
            return;
        }

       if (ControllerData.GetButtonPressed(ControllerData.Button.A) || doAction)
       {
            painter.ProjectionPaint(this);
            UpdatePainting();
            
            // Vibrate
            Grabbable gb = root.GetComponent<Grabbable>();
            if (gb._currentGrabbingMode == Grabbable.GrabbingMode.LeftTouch)
                ControllerData.SetLeftTouchVibration(40, 2, 50);
            if (gb._currentGrabbingMode == Grabbable.GrabbingMode.RightTouch)
                ControllerData.SetRightTouchVibration(40, 2, 50);
        }
    }

    void TryPaintAsPencil(bool fromNet = false)
    {
        float distance = this.transform.localScale.y;
        Ray ray = new Ray(this.transform.position, GetDirectionFromMode());
        RaycastHit[] Hits = Physics.RaycastAll(ray, distance);
        foreach (RaycastHit hit in Hits)
        {
            Paintable ptb = hit.collider.gameObject.GetComponent<Paintable>();
            if (ptb)
            {
                // ok now get hitpoint distance 
                float hpdist = Vector3.Distance(hit.point, transform.position);
                // corrected dir
                // this one is good 
                Vector3 ppos = hit.point + (hit.normal* (distance*2));
                // WARNING HERE. PDIR is not a direction but the desired point where projection point will look at.
                Vector3 pdir = ppos - hit.normal;
   
                // Intensity and angle should be always same value
                painter.ProjectionPaint_Advanced(ppos, pdir, intensity, width, 1f, this, ptb);

                if (!fromNet)
                {
                    UpdatePainting();
                    // Vibrate
                    Grabbable gb = root.GetComponent<Grabbable>();
                    if (gb._currentGrabbingMode == Grabbable.GrabbingMode.LeftTouch)
                        ControllerData.SetLeftTouchVibration(40, 2, 10);
                    if (gb._currentGrabbingMode == Grabbable.GrabbingMode.RightTouch)
                        ControllerData.SetRightTouchVibration(40, 2, 10);
                }
                   
                return;
            }
        }
    }
    void TryPaintAsBrush(bool fromNet = false) // Used for Painter
    {
        float distance = this.transform.localScale.y; 
        Ray ray = new Ray(this.transform.position, GetDirectionFromMode());
        RaycastHit[] Hits = Physics.RaycastAll(ray, distance);
        foreach ( RaycastHit hit in Hits)
        {
            Paintable ptb = hit.collider.gameObject.GetComponent<Paintable>();
            if (ptb)
            {
                float hpdist = Vector3.Distance(hit.point, transform.position);
       
                Vector3 ppos = transform.position;
                Vector3 pdir = transform.position + (GetDirectionFromMode() * 5f); // inclinaison & co are not corrected while using brysg

                // More angle depth if we push on brush
                // 90f for maxangle is good for 0.05 of 
                float maxangle = (transform.localScale.x / 0.05f) * 90f;
                float calcangle = maxangle * (hpdist / distance);
                calcangle = maxangle - calcangle;

                // we can also adjust intensity base on if brush is depth or not 200 is raw intensity. but we can go lower... 
                float maxintensity = 200f;
                float calcintensity = maxintensity * (hpdist / distance);
                calcintensity = maxintensity - calcintensity;

                painter.ProjectionPaint_Advanced(ppos, pdir, calcintensity, calcangle, 1f, this, ptb);
                if (!fromNet)
                {
                    UpdatePainting();
                    // Vibrate
                    Grabbable gb = root.GetComponent<Grabbable>();
                    if (gb._currentGrabbingMode == Grabbable.GrabbingMode.LeftTouch)
                        ControllerData.SetLeftTouchVibration(40, 2, 10);
                    if (gb._currentGrabbingMode == Grabbable.GrabbingMode.RightTouch)
                        ControllerData.SetRightTouchVibration(40, 2, 10);
                }
                    
                return;
            }
        }
    }
    private void ChangeColor(Color newColor)
    {
        // Set 
        color = newColor;
        // we can set here 
        GetComponent<Renderer>().material.color = color;
    }

    void TryPumpColorFromPicker()
    {
        
        // Check if we collide 
        float distance = this.transform.localScale.y; // we can add more dist ( like mul it per 1.2) 
        Ray ray = new Ray(this.transform.position, GetDirectionFromMode());
        RaycastHit[] Hits = Physics.RaycastAll(ray, distance);
        
        foreach ( RaycastHit hit in Hits)
        {
            if ( hit.collider.tag == "ColorPicker")
            {
                // It works but colorpicker collider should not be convex!
                Renderer rend = hit.transform.GetComponent<Renderer>();
                if (!rend || !rend.material || !rend.sharedMaterial.mainTexture) return;
                Texture2D tex = rend.material.mainTexture as Texture2D;
               
                Vector2 pixelUV = hit.textureCoord;
                pixelUV.x *= tex.width;
                pixelUV.y *= tex.height;
                Color c = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y);

                ChangeColor(c);
                UpdateColorInformation();

                // Vibrate
                Grabbable gb = root.GetComponent<Grabbable>();
                if (gb._currentGrabbingMode == Grabbable.GrabbingMode.LeftTouch)
                    ControllerData.SetLeftTouchVibration(40, 2, 10);
                if (gb._currentGrabbingMode == Grabbable.GrabbingMode.RightTouch)
                    ControllerData.SetRightTouchVibration(40, 2, 10);

                return ;
            }
        }
    }

    // ----- NET HERE -- 
    List<byte[]> rawSyncData = new List<byte[]>();
    List<byte[]> unprocessedRcv = new List<byte[]>();

    public void UpdateColorInformation()
    {
        List<byte> data = new List<byte>();
        data.Add(1); // entry flag
        data.AddRange(BitConverter.GetBytes(color.r)); // is float
        data.AddRange(BitConverter.GetBytes(color.g)); // is float
        data.AddRange(BitConverter.GetBytes(color.b)); // is float
        if (rawSyncData.Count > 1)
        {
            byte lrflag = rawSyncData[rawSyncData.Count - 1][0];
            if (lrflag == 1)
            {
                rawSyncData[rawSyncData.Count - 1] = data.ToArray();
                return;
            }
        }
        rawSyncData.Add(data.ToArray());
    }
    public void UpdatePainting()
    {
        List<byte> data = new List<byte>();
        data.Add(0); // entry flag
        // add new byte with transform position
        data.AddRange(BitConverter.GetBytes(transform.position.x)); // is float
        data.AddRange(BitConverter.GetBytes(transform.position.y)); // is float
        data.AddRange(BitConverter.GetBytes(transform.position.z)); // is float
        // add rotate information
        data.AddRange(BitConverter.GetBytes(transform.eulerAngles.x)); // is float
        data.AddRange(BitConverter.GetBytes(transform.eulerAngles.y)); // is float
        data.AddRange(BitConverter.GetBytes(transform.eulerAngles.z)); // is float

        rawSyncData.Add(data.ToArray());
    }

    byte[] PackSyncingData(int maxBufferSize)  // OK
    {
        if (rawSyncData.Count == 0) // do nothing if rawSync list is empty
            return null;

        List<byte> result = new List<byte>();
        // Add generic flag
        result.Add(70); // generic flag 
        result.Add((byte)this.gameObject.name.Length); // length of mooduler name
        foreach (char c in this.gameObject.name.ToCharArray())
            result.Add((byte)c);

        int bctr = 2 + this.gameObject.name.Length; // ok setup a byte counter
        bool _concatPos = false; // starting concatening pos enables 

        // ok until here
        while (bctr < maxBufferSize && rawSyncData.Count > 0)
        {
            // fast compr data
            byte entryFlag = rawSyncData[0][0]; // get entry flag of current rawsyncdata
            if (entryFlag == 0) // if position entry
            {
                if (!_concatPos) // if not concatening create new comp entry with 0 flag. then number of position following
                {
                    int pcounter = GetCountOfPositionDataFromOrigin();
                    result.Add(0);
                    result.AddRange(BitConverter.GetBytes(pcounter)); // add int (4 bytes) 
                    bctr += 5; // increment byte counter by 5 (0 flag + rep number) 
                    _concatPos = true;
                }
                // copy rawsyncdata (except flag byte) 
                for (int i = 1; i < rawSyncData[0].Length; i++)
                    result.Add(rawSyncData[0][i]);

                bctr += rawSyncData[0].Length - 1;
                
            }
            else
            {
                // not a position entry. copy data... 
                result.AddRange(rawSyncData[0]);
                bctr += rawSyncData[0].Length;
                _concatPos = false;
            }

            // Pop first element
            rawSyncData.RemoveAt(0);
        }

        return result.ToArray();

    }
    int GetCountOfPositionDataFromOrigin() // OK 
    {
        int count = 0;
        for (int i = 0; i < rawSyncData.Count; i++)
        {
            if (rawSyncData[i][0] == 0)
                count++;
            else
                break;
        }
        
        return count;
    }

    IEnumerator PaintSyncing(float rateseconds)
    {
        while (true)
        {
            // Send data packet up to 64kb
            if (rateseconds > 0f)
            {
                yield return new WaitForSeconds(rateseconds);
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
            byte[] data = PackSyncingData(64000);
            if (data != null)
            {
                NetUtilities.SendDataToAll(data);
            }
        }
    }

    IEnumerator ProccessingReceivedMessages()
    {
        
        while ( true)
        {
            lock (unprocessedRcv)
            {
                if (unprocessedRcv.Count > 0)
                {
                    byte[] data = unprocessedRcv[0];

                    int bctr = 2 + this.name.Length;

                    while (bctr < data.Length)
                    {
                        byte flg = data[bctr]; bctr++; // Get actual file reading 
                        if (flg == 0) // IF 0 reading position entry 
                        {
                            int entriescounter = BitConverter.ToInt32(data, bctr); bctr += 4; // read pos rep counter
                            for (int i = 0; i < entriescounter; i++)
                            {
                                // read pos
                                float px = BitConverter.ToSingle(data, bctr); bctr += 4; // read x val 
                                float py = BitConverter.ToSingle(data, bctr); bctr += 4; // read y val
                                float pz = BitConverter.ToSingle(data, bctr); bctr += 4; // read z val 

                                // read rot
                                float rx = BitConverter.ToSingle(data, bctr); bctr += 4; // read x val 
                                float ry = BitConverter.ToSingle(data, bctr); bctr += 4; // read y val
                                float rz = BitConverter.ToSingle(data, bctr); bctr += 4; // read z val 

                                transform.position = new Vector3(px, py, pz);
                                transform.eulerAngles = new Vector3(rx, ry, rz);
                                switch (PaintingMode)
                                {
                                    case Painter.DrawModes.Pencil: TryPaintAsPencil(true); break;
                                    case Painter.DrawModes.Spray: TryPaintAsSpray(true); break;
                                    case Painter.DrawModes.Brush: TryPaintAsBrush(true); break;
                                }
                                yield return new WaitForEndOfFrame();

                            }
                            yield return new WaitForEndOfFrame();
                            continue;
                        }
                        if (flg == 1)
                        {
                            // read color
                            float r = BitConverter.ToSingle(data, bctr); bctr += 4; // read x val 
                            float g = BitConverter.ToSingle(data, bctr); bctr += 4; // read y val
                            float b = BitConverter.ToSingle(data, bctr); bctr += 4; // read z val 
                            Color ncolor = new Color(r, g, b, 1.0f);
                            ChangeColor(ncolor);
                            continue;
                        }
                        else
                        {
                            Debug.Log("FLAG ERROR!!!");
                            break;
                        }

                    }

                    unprocessedRcv.RemoveAt(0); // popping element 
                }
            }
            
            yield return new WaitForEndOfFrame();
        }
    }

    public static void OnPaintingCommandReceived(byte[] data)
    {
        byte nmsize = data[1];
        char[] objname = new char[nmsize];
        for (int i = 0; i < nmsize; i++)
            objname[i] = (char)data[2 + i];
        GameObject vObj = GameObject.Find(new string(objname));
        if (!vObj)
            return;

        Painter pnter = vObj.GetComponent<Painter>();
        if (!pnter)
            return;
      
        pnter.unprocessedRcv.Add(data); 
    }

 
}
