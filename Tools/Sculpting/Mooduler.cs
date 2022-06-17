using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class Mooduler : MonoBehaviour
{
    public enum ActionMode { Add, Substract, Paint, ClearColor }
    [Header("Modes")]
    public ActionMode actionMode;
    public enum ShapeMode { Sphere, Ellipsoid, Cube, Cylinder }
    public ShapeMode  shapeMode;
    public enum TriggerMode { OnContact, RayCast, Free }
    public TriggerMode triggerMode;

    [Header("Configuration")]
    //@ limité à 0.01 sinon probleme de tracking 
    [Range(0.001f, 3f)] public float Weight = 0.001f;
    public Vector3 ShapeExtents;
    public Vector3 ShapeRotation;
    public bool doSmoothStep = false;

    public Color color = Color.cyan;
    
    public float Distance = 1f;
    public LayerMask layerMask;
    public enum RayMode { Forward, BackWard, Left, Right, Up, Down }
    public RayMode RayDirection;


    [Header("Debug")]
    public bool ShowSkeletonObject = false;
    public bool DoAction = false;
    public bool AllowDebugInput = false;

    private int ModeSwitchForDebug;


    GameObject root;
    GameObject Shape;
    Moodulable latestMoodulable;
    bool _requireCCL = false;

    private ShapeMode lastShapeMode;

    private void Start()
    {
        UpdateShapeMeshFromMode();
        if (!ShowSkeletonObject)
            Shape.GetComponent<MeshRenderer>().enabled = false;
        Shape.layer = 11;
        this.gameObject.layer = 11;
        Destroy(Shape.GetComponent<Collider>());
        Shape.AddComponent<BoxCollider>();
        //@ For Net
        StartCoroutine(MoodSyncing(0.11f));
        StartCoroutine(ProccessSyncMessage());
        lastShapeMode = shapeMode;
        root = this.transform.root.gameObject;

        // Create a looker to clean memory of Net CCL26 computation
        if ( !FindObjectOfType<CCL26>())
        {
            GameObject g = new GameObject("CCL Looker");
            g.AddComponent<CCL26>();
        }
    }


    #region Moodulation

    public void UpdateShapeMeshFromMode()
    {
        if (Shape)
            Destroy(Shape.gameObject);
        switch (shapeMode)
        {
            case ShapeMode.Cube: Shape = GameObject.CreatePrimitive(PrimitiveType.Cube); return;
            case ShapeMode.Cylinder: Shape = GameObject.CreatePrimitive(PrimitiveType.Cylinder); return;
        }
        Shape =  GameObject.CreatePrimitive(PrimitiveType.Sphere); 
    }
    public void UpdateShapeObjectFromVariables()
    {
        if (shapeMode == ShapeMode.Sphere)
            Shape.transform.localScale = new Vector3(ShapeExtents.x, ShapeExtents.x, ShapeExtents.x);
        else
        {
            Shape.transform.localScale = ShapeExtents;
            Shape.transform.eulerAngles = ShapeRotation; //@ here this not working
        }
        Shape.transform.position = this.transform.position + GetDirectionFromMode() * 0.2f;
    }

    
    private void Update()
    {
        if  (lastShapeMode != shapeMode)
        {
            UpdateShapeMeshFromMode();
            lastShapeMode = shapeMode;
        }

        UpdateShapeObjectFromVariables();

        Grabbable b = root.GetComponent<Grabbable>();
        if (b)
        {
            if (b._grabbed && b._userisgrabber)
            {
                TryEdit();
                if (actionMode == ActionMode.Paint)
                    TryPumpColorFromPicker();
            }
                
        }
        
        // Basic DebugControl
        if (!AllowDebugInput)
            return;

        if (!ControllerData.GetButtonPressed(ControllerData.Button.A)
            && !ControllerData.GetButtonPressed(ControllerData.Button.B))
        {
            // Basic Mooduler variable change 
            if (ControllerData.GetButtonPressed(ControllerData.Button.Y))
            {
                if (shapeMode == ShapeMode.Sphere)
                    ShapeExtents.x += 0.01f;
                UpdateShapeInformationToSyncingQueue();
            }
            if (ControllerData.GetButtonPressed(ControllerData.Button.X))
            {
                if (shapeMode == ShapeMode.Sphere)
                    ShapeExtents.x -= 0.01f;
                UpdateShapeInformationToSyncingQueue();
            }
            return;
            if (ControllerData.GetButtonDown(ControllerData.Button.LeftSideTrigger))
            {
                ModeSwitchForDebug++;
                if (ModeSwitchForDebug > 2)
                    ModeSwitchForDebug = 0;
                actionMode = (ActionMode)ModeSwitchForDebug;
                UpdateShapeInformationToSyncingQueue();
            }
            if (ControllerData.GetButtonDown(ControllerData.Button.RighSideTrigger))
            {
                ModeSwitchForDebug--;
                if (ModeSwitchForDebug < 0)
                    ModeSwitchForDebug = 2;

                actionMode = (ActionMode)ModeSwitchForDebug;
                UpdateShapeInformationToSyncingQueue();
            }
        }
    }
    void TryPumpColorFromPicker()
    {

        // Check if we collide 
        Ray ray = new Ray(this.transform.position, GetDirectionFromMode());
        
        // distance to find
        RaycastHit[] Hits = Physics.RaycastAll(ray, 0.2f + ShapeExtents.x/2 );

        foreach (RaycastHit hit in Hits)
        {
            if (hit.collider.tag == "ColorPicker")
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
                UpdateShapeInformationToSyncingQueue();

                // Vibrate
                Grabbable gb = root.GetComponent<Grabbable>();
                if (gb._currentGrabbingMode == Grabbable.GrabbingMode.LeftTouch)
                    ControllerData.SetLeftTouchVibration(40, 2, 10);
                if (gb._currentGrabbingMode == Grabbable.GrabbingMode.RightTouch)
                    ControllerData.SetRightTouchVibration(40, 2, 10);
                return;
            }
        }
    }

    public void ChangeColor(Color c)
    {
        color = c;
        //@ do some stuff here if we need 
        Shape.GetComponent<Renderer>().material.color = c;
    }
    void TryEdit()
    {
        Moodulable m = null;
        Vector3 hitPoint = Vector3.zero;
        // Detect Moodulable from Trigger Mode
        if (triggerMode == TriggerMode.OnContact)
        {
            m = DetectMoodulableCollision();
            hitPoint = Shape.transform.position;
        }
        else if (triggerMode == TriggerMode.RayCast)
        {
            RaycastHit hit = RayCastMoodulable();
            if (hit.collider)
            {
                m = hit.collider.gameObject.transform.root.GetComponent<Moodulable>();
                hitPoint = hit.point;
                Shape.transform.position = hitPoint;
            }
        }
        else
        {
            // Find nearest Moodulable
            Moodulable[] moods = FindObjectsOfType<Moodulable>();
            float mindist = float.MaxValue;
            foreach ( Moodulable md in moods)
            {
                float d = Vector3.Distance(Shape.transform.position, md.gameObject.transform.position);
                if (d < mindist)
                    m = md;
            }
            hitPoint = Shape.transform.position;
        }
        // Proccess CCL26 physics computation
        CCL26.ComputeTask task = CCL26.GetTaskFromMoodulable(latestMoodulable);
        if (task != null)
        {
            if (task._Done)
            {
                task._Moodulable.classCCLBuff.SetData(task.classifications);
                task._Moodulable.GenerateAllChunks();
                bool success = task._Moodulable.TrySplitChunksPerLabel();
                if (success && !task._FromNet)
                    UpdateCCLComputationToSyncingQueue(latestMoodulable);
                // Remove Task From Memory...
                CCL26.RemoveTask(task);
            }
            else // Do not edit if we are still computing. should try an elseif
            {
                if (task._Moodulable == m)
                    return;
            }
        }

        if (!m)
        {
            if ( task == null    // No CCL 26 task
            && latestMoodulable // check if  we have touched something before
            && _requireCCL      // Check if we have sub the form before
            && (!ControllerData.GetButtonPressed(ControllerData.Button.B) // check if action false
            && !DoAction))      // check if action false
            {
                _requireCCL = false;
                latestMoodulable.ComputeCCL26(false);
            }
            return;
        }

        // Do not edit if CCL26 computing moodulable
        if (task != null)
        {
            if (!task._Done)
                return;
        }

        if (m != latestMoodulable)
            UpdateMoodulableInformationToSyncingQueue(m);

        latestMoodulable = m;

        //Apply Action from Action Mode
        if (!ControllerData.GetButtonPressed(ControllerData.Button.B) && !DoAction)
            return;

        bool paint = false;
        if (actionMode == ActionMode.Paint || actionMode ==  ActionMode.ClearColor)
            paint = true;
        Modify(m, hitPoint, paint);
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
    RaycastHit RayCastMoodulable()
    {
        Vector3 dir = GetDirectionFromMode();

        Ray ray = new Ray(Shape.transform.position, dir);
        RaycastHit[] Hits = Physics.RaycastAll(ray, Distance, layerMask);
        foreach (RaycastHit hit in Hits)
        {
            Moodulable m = hit.collider.gameObject.transform.root.GetComponent<Moodulable>();
           
            if (m) 
            {
                return hit;
            }
        }
        return new RaycastHit();
    }
    Moodulable DetectMoodulableCollision()
    {
        Moodulable[] moods = FindObjectsOfType<Moodulable>();
        foreach (Moodulable m in moods)
        {
            if (Vector3.Distance(Shape.transform.position, m.gameObject.transform.position)
                <= m.boundsSize / 2)
            {
                foreach (MoodyChunk chunk in m.chunks)
                {
                    if (chunk.gameObject.GetComponent<Collider>().bounds.Intersects(Shape.gameObject.GetComponent<Collider>().bounds))
                    {
                        return m;
                    }
                }
            }
        }
        return null;
    }

    void Modify(Moodulable moodulable, Vector3 hitPoint, bool Paint)
    {
        // Deform base on shape
        string ShapeName = "";

        switch (shapeMode)
        {
            case ShapeMode.Sphere: ShapeName = "sphere"; break;
            case ShapeMode.Cube: ShapeName = "box"; break;
            case ShapeMode.Ellipsoid: ShapeName = "ellipsoid"; break;
            case ShapeMode.Cylinder: ShapeName = "cylinder"; break;
        }

        float w = actionMode == ActionMode.Add ? -Weight : Weight;
        w = actionMode == ActionMode.ClearColor ? -Weight : w;

        Vector3 convertedPoint = Vector3.zero;
        // Position is overwritten...when we call with applyshape

        if (Paint)
            convertedPoint = moodulable.ApplyShape(ShapeName, Shape, color, w, true, doSmoothStep);
        else
            convertedPoint = moodulable.ApplyShape(ShapeName, Shape, w, true, doSmoothStep) ;
        

        UpdatePositionInformationToSyncingQueue(convertedPoint);

        // Vibrate
        Grabbable gb = root.GetComponent<Grabbable>();
        if (gb._currentGrabbingMode == Grabbable.GrabbingMode.LeftTouch)
            ControllerData.SetLeftTouchVibration(40, 2, 50);
        if (gb._currentGrabbingMode == Grabbable.GrabbingMode.RightTouch)
            ControllerData.SetRightTouchVibration(40, 2, 50);

        if (!Paint)
        {
            _requireCCL = actionMode == ActionMode.Substract ? true : _requireCCL;
            moodulable.TryApplyConvexGeometry();
        }
           

    }
    // @ seems rad to set it twice. but we are in a rush men! :3
    void ModifyFromNet(Moodulable moodulable, Vector3 hitPoint, bool Paint)
    {

        string ShapeName = "";

        switch (shapeMode)
        {
            case ShapeMode.Sphere: ShapeName = "sphere"; break;
            case ShapeMode.Cube: ShapeName = "box"; break;
            case ShapeMode.Ellipsoid: ShapeName = "ellipsoid"; break;
            case ShapeMode.Cylinder: ShapeName = "cylinder"; break;
        }
        // Synchronise que la couleur
        float w = actionMode == ActionMode.Add ? -Weight : Weight;
        w = actionMode == ActionMode.ClearColor ? -Weight : w;

        // Ok here it seems to fuck up with rot and other stuff. If we do not use transform rotation
        if (Paint)
            moodulable.ApplyShape(ShapeName, hitPoint, ShapeExtents, Shape.transform.rotation, w, color, false, doSmoothStep);

        else
        {
            moodulable.ApplyShape(ShapeName, hitPoint, ShapeExtents, Shape.transform.rotation, w, false, doSmoothStep);
            moodulable.TryApplyConvexGeometry();
        }
      
    }

    #endregion

    #region NetSyncing

    // @ Sync Only Net Variable To Avoid Issue with Grabbing mechanics
    [HideInInspector] public Vector3 syncedPos;

    List<byte[]> rawSyncData = new List<byte[]>();
    [HideInInspector] public List<byte[]> unprocessedSyncMessage = new List<byte[]>();

    public void UpdateShapeInformationToSyncingQueue() 
    {
        List<byte> data = new List<byte>();
        data.Add(1);
        /*
         * (Update Moodulable information) 
            actionMode      (1 byte)
		    shapeMod        (1 byte)
		    Radius          (4b)
		    Weight          (4b)
		    ShapeExtents;   (12b)
		    ShapeRotation;  (12b)
		    color           (16b)
         */
        data.Add((byte)actionMode);
        data.Add((byte)shapeMode);
        data.AddRange(BitConverter.GetBytes(Weight));
        data.AddRange(BinaryUtilities.Vector3Tobytes(ShapeExtents));
        data.AddRange(BinaryUtilities.Vector3Tobytes(ShapeRotation));
        data.AddRange(BitConverter.GetBytes(color.r));
        data.AddRange(BitConverter.GetBytes(color.g));
        data.AddRange(BitConverter.GetBytes(color.b));

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

    public void UpdateCCLComputationToSyncingQueue(Moodulable m)
    {
        List<byte> data = new List<byte>();
        data.Add(3);
        if (rawSyncData.Count > 1)
        {
            byte lrflag = rawSyncData[rawSyncData.Count - 1][0];
            if (lrflag == 3)
                return;
        }
        rawSyncData.Add(data.ToArray());
        Debug.Log("Sending CCL computation Request through Net");
    }
    public void UpdateMoodulableInformationToSyncingQueue(Moodulable m)
    {
        List<byte> data = new List<byte>();
        data.Add(2); // flag for setting new moodulable
        data.Add((byte)m.gameObject.name.Length);
        foreach (char c in m.gameObject.name.ToCharArray())
            data.Add((byte)c);
        if (rawSyncData.Count > 1)
        {
            byte lrflag = rawSyncData[rawSyncData.Count - 1][0];
            if (lrflag == 2)
            {
                rawSyncData[rawSyncData.Count - 1] = data.ToArray();
                return;
            }
        }
        rawSyncData.Add(data.ToArray());
    }
    public void UpdatePositionInformationToSyncingQueue(Vector3 hitPoint) 
    {
        List<byte> data = new List<byte>();
        data.Add(0); 
        data.AddRange(BitConverter.GetBytes(hitPoint.x)); 
        data.AddRange(BitConverter.GetBytes(hitPoint.y));
        data.AddRange(BitConverter.GetBytes(hitPoint.z));
        rawSyncData.Add(data.ToArray());

    }
    byte[] PackSyncingData(int maxBufferSize)
    {
        if (rawSyncData.Count == 0) // do nothing if rawSync list is empty
            return null;

        List<byte> result = new List<byte>();
        // Add generic flag
        result.Add(25); // generic flag 
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
                ;
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
    int GetCountOfPositionDataFromOrigin() 
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

    public static void OnMoodulationReceived(byte[] data) // < This One need to be added to a queue i guess ... 
    {

        // Get object name 
        byte nmsize = data[1];
        char[] objname = new char[nmsize];
        for (int i = 0; i < nmsize; i++)
            objname[i] = (char)data[2 + i];
        GameObject vObj = GameObject.Find(new string(objname));
        if (!vObj)
            return;

        Mooduler mdler = vObj.GetComponent<Mooduler>();
        if (!mdler)
            return;

        mdler.unprocessedSyncMessage.Add(data);

    }
     // Time@Delta time do bad computation
    IEnumerator ProccessSyncMessage()
    {
        while (true)
        {
            if (unprocessedSyncMessage.Count > 0)
            {
                byte[] data = unprocessedSyncMessage[0];
                int nmsize = this.name.Length;
                Mooduler mdler = this;
                int bctr = 2 + nmsize;

                while (bctr < data.Length)
                {
                    byte flg = data[bctr]; bctr++; // Get actual file reading 
                    if (flg == 0) // IF 0 reading position entry 
                    {
                        int entriescounter = BitConverter.ToInt32(data, bctr); bctr += 4; // read pos rep counter
                        for (int i = 0; i < entriescounter; i++)
                        {
                            Vector3 hitPoint = BinaryUtilities.BytesToVector3(ref data, bctr); bctr += 12;
                            
                            bool Paint = false;

                            if (mdler.actionMode == ActionMode.Paint || mdler.actionMode == ActionMode.ClearColor)
                                Paint = true;

                            mdler.ModifyFromNet(mdler.latestMoodulable, hitPoint, Paint);
                            // wait for end of frame
                            yield return new WaitForEndOfFrame();

                        }
                        continue;
                    }
                    if (flg == 1)
                    {
                        byte amode = data[bctr]; bctr++;
                        byte bmode = data[bctr]; bctr++;
                        mdler.actionMode = (ActionMode)amode;
                        mdler.shapeMode = (ShapeMode)bmode;

                        mdler.Weight = BitConverter.ToSingle(data, bctr); bctr += 4;
                        mdler.ShapeExtents = BinaryUtilities.BytesToVector3(ref data, bctr); bctr += 12;
                        mdler.ShapeRotation = BinaryUtilities.BytesToVector3(ref data, bctr); bctr += 12;

                        float r = BitConverter.ToSingle(data, bctr); bctr += 4;
                        float g = BitConverter.ToSingle(data, bctr); bctr += 4;
                        float b = BitConverter.ToSingle(data, bctr); bctr += 4;
                        mdler.ChangeColor(new Color(r, g, b, 1f));

                        // Update shape...
                        mdler.UpdateShapeObjectFromVariables();
                        continue;
                    }
                    if (flg == 2)
                    {
                        byte mdnmsize = data[bctr]; bctr++;
                        char[] moodulablenm = new char[mdnmsize];
                        for (int i = 0; i < mdnmsize; i++)
                            moodulablenm[i] = (char)data[bctr + i];
                        bctr += mdnmsize;
                        GameObject moodulableObj = GameObject.Find(new string(moodulablenm));
                        mdler.latestMoodulable = moodulableObj.GetComponent<Moodulable>();
                        continue;
                    }
                    if (flg == 3)
                    {
                        mdler.latestMoodulable.ComputeCCL26(true);
                        continue;
                    }
                    else
                    {
                        Debug.Log("Cannot proccess Sync Messgae. Flag is incorrect.");
                        break;
                    }

                }

                // popping element 
                unprocessedSyncMessage.RemoveAt(0); 
            }

            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator MoodSyncing(float rateseconds)
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
    #endregion
}