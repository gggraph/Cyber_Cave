using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

public struct VertexData
{
    public Vector3 position;
    public Vector3 normal;
    public int idX;
    public int idY;
    public int l;
    // @added 07.06
    public Color Color;

}
// @ Dispatch / Release / GetStride
public static class ComputeHelper 
{
    public static Vector3Int GetThreadGroupSizes(ComputeShader compute, int kernelIndex = 0)
    {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        return new Vector3Int((int)x, (int)y, (int)z);
    }
    public static void Dispatch(ComputeShader cs, int numIterationsX, int numIterationsY = 1, int numIterationsZ = 1, int kernelIndex = 0)
    {
        Vector3Int threadGroupSizes = GetThreadGroupSizes(cs, kernelIndex);
        int numGroupsX = Mathf.CeilToInt(numIterationsX / (float)threadGroupSizes.x);
        int numGroupsY = Mathf.CeilToInt(numIterationsY / (float)threadGroupSizes.y);
        int numGroupsZ = Mathf.CeilToInt(numIterationsZ / (float)threadGroupSizes.y);
        cs.Dispatch(kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
    }
    public static void Release(params ComputeBuffer[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            if (buffers[i] != null)
            {
                buffers[i].Release();
            }
        }
    }
    
    public static int GetStride<T>()
    {
       return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
    }
}

public static class MoodulableManager
{
    static int MaxNumberLoaded = 10;
    /*
     We will have a list of moodulable object. Only 3 should be loaded at runtime for performance reason. 
     When a new Moodulable is created. Call OnNewMoodulable(). when a moodulable is destroy call OnDeleteMoodulable(). 
     when a moodulable is loaded call OnFocus(). 
     When we call OnFocus it will check reorder moodulable list and set moodulable at position 0. 
     Everytime we want we can call DisabledUnFocusMoodulable() It will unload every moodulable after index+2 that are loaded.

        OnNewMoodulable is called at init. 
        OnDeleteMoodulable is called at delete 
        OnFocusmoodulable is called at load. 
        UnloadUnfocusedMoodulable is called when needed & should be called often
     */
    public static List<Moodulable> Moodulables = new List<Moodulable>();
    public static void OnNewMoodulable(Moodulable m) 
    {
        Moodulables.Insert(0, m);
    }
    public static void OnDeleteMoodulable(Moodulable m) 
    {
        int index = Moodulables.IndexOf(m);
        if (index == -1)
        {
            return;
        }
        Debug.Log(index);
        // remove at index
        Moodulables.RemoveAt(index);

    }
    public static void OnFocusMoodulable(Moodulable m) 
    {
        OnDeleteMoodulable(m);
        //insert at 0
        Moodulables.Insert(0, m);
    }
    public static void UnloadUnfocusedMoodulable() 
    {
        if ( Moodulables.Count > MaxNumberLoaded)
        {
            for (int i = MaxNumberLoaded; i < Moodulables.Count; i++)
            {
                if (Moodulables[i].loaded)
                    Moodulables[i].UnLoad();
            }
        }
    }

}
public class Moodulable : MonoBehaviour
{

    // Error of bizarre texture

    // @ Create a nest of chunks that can be march-ize. Core sculpting file. 

    // num chunks was 6 & numpoint was 10
    [Header("Init Settings")]
    public string initialSculptPath;
    public int numChunks = 6;
    public int numPointsPerAxis = 10;
    public float boundsSize = 10;
    public float isoLevel = 0f;
    public bool useFlatShading = false;
    public float reduction = 0.1f;
    public Material material;
    public bool EnableMerging = true;
    public int TimeHoldBeforeMerging = 3;
    public bool EnableCCLComputing = true;

    [Header("References")]
    public ComputeShader meshCompute;
    public ComputeShader editCompute;
    public ComputeShader CCLCompute;


    ComputeBuffer triangleBuffer;
    ComputeBuffer triCountBuffer;
    ComputeBuffer bitCCLBuff;
    [HideInInspector] public ComputeBuffer classCCLBuff;

    [HideInInspector] public RenderTexture rawDensityTexture;
    
    VertexData[] vertexDataArray;
    [HideInInspector] public MoodyChunk[] chunks;

    [Header("Coloring Logic")]
    [HideInInspector] public RenderTexture ColoringTexture;
    [HideInInspector] public RenderTexture ColoringBuffer;
    public bool SmoothStepColor = true;

    [Header("State")]
    public bool loaded;

    [Header("Debug")]
    public bool ApplyCCL = false;
    public bool _Save = false;
    public bool _Recompute = false;
    public bool _debug3DPrint;


    #region Initialization
    public void InitGlobalData()
    {
        Debug.Log("Loading Object : " + this.name);
        meshCompute = Resources.Load("Compute/MarchingCubes") as ComputeShader;
        
        // Do some Coloring Service
        if (SmoothStepColor)
            editCompute = Resources.Load("Compute/VoxelEditorDoublePass") as ComputeShader;
        else
            editCompute = Resources.Load("Compute/VoxelEditor") as ComputeShader;


        CCLCompute =  Resources.Load("Compute/CCL") as ComputeShader;

        // Layer
        this.gameObject.layer = 10;
        // Other components
        this.gameObject.AddComponent<Rigidbody>();
        this.gameObject.AddComponent<Grabbable>();
        this.gameObject.AddComponent<ObjectSaver>();

        StartCoroutine(DetetectMerging());

    }
    public void InitFromDefault(string FormName = "") 
    {
        loaded = true;
        InitGlobalData();
        InitTextures();
        CreateBuffers();
        ComputeEmptyCheckSumFile();
        CreateChunks();

        // @ create default shape
        if ( FormName.ToLower() == "cube")
        {
            //@ not working
            ApplyShape("box", this.transform.position, 
                new Vector3(boundsSize / 4, boundsSize / 4, boundsSize / 4) * reduction, Quaternion.identity, -0.4f);
        }
        else
        {
            ApplyShape("sphere", this.transform.position,
                new Vector3(boundsSize / 4, boundsSize / 4, boundsSize / 4) * reduction, Quaternion.identity, -0.4f);
        }
        // @ Try Apply Convex Geometry
        TryApplyConvexGeometry();
       
        // apply reduction
        this.gameObject.transform.localScale = new Vector3(reduction, reduction, reduction);
        MoodulableManager.OnNewMoodulable(this);
        
    }
    public void InitFromChunks(Moodulable otherMoodulable, List<MoodyChunk> otherChunks, bool Center = false) 
    {
        loaded = true;
        InitGlobalData();
        InitTextures();
        ComputeEmptyCheckSumFile();
        CreateBuffers();
        CreateChunks();
        MergeMoodulable(otherMoodulable, otherChunks, false, true);
        GenerateAllChunks(); // needed because it is already called inside mergefunctions
        TryApplyConvexGeometry();
        this.gameObject.transform.localScale = new Vector3(reduction, reduction, reduction);
        MoodulableManager.OnNewMoodulable(this);
       
    }
    public bool InitFromFile()
    {
        loaded = true;
        InitGlobalData();
        if (!LoadFromFile(initialSculptPath, true))
        {
            Debug.Log("Should delete save file...");
            loaded = false;
            return false;
        }
        ComputeEmptyCheckSumFile();
      
        // apply reduction
        this.gameObject.transform.localScale = new Vector3(reduction, reduction, reduction);
        TryApplyConvexGeometry();
        MoodulableManager.OnNewMoodulable(this);
        return true;
    }
    bool LoadFromFile(string filePath, bool createChunks = true) 
    {
        if ( !File.Exists(filePath))
        {
            Debug.LogError("Cannot load moodulable from file because file does not exist : " + "\r\n"  + filePath);
            return false;
        }
       
        //@ Read Bytes
        byte[] data = File.ReadAllBytes(filePath);

        //@ Get important variables
        numChunks = BitConverter.ToInt32(data, 0);
        numPointsPerAxis = BitConverter.ToInt32(data, 4);
        boundsSize = BitConverter.ToSingle(data, 8);

        // @ Now create buffers & textures
        InitTextures();
        CreateBuffers();

        // Compute size of voxels...
        int size = numChunks * (numPointsPerAxis - 1) + 1;
        
        //@ Reading Data
        float[] voxelsData = new float[size * size * size];
        float[] colorData = new float[size * size * size * 4];

        // @ some sanitycheck needed here 

        int boff = 12;
        for (int i = 0; i < voxelsData.Length; i++)
        {
            voxelsData[i] = BitConverter.ToSingle(data, boff);
            boff += 4;
        }
        for (int i = 0; i < colorData.Length; i++)
        {
            colorData[i] = BitConverter.ToSingle(data, boff);
            boff += 4;
        }

        // @ If chunks needed to be created 
        if ( createChunks)
            CreateChunks();
       
        // @ Populate
        ComputeBuffer recipient = new ComputeBuffer(size * size * size, sizeof(float), ComputeBufferType.Default);
        ComputeBuffer recipientB = new ComputeBuffer(size * size * size * 4, sizeof(float), ComputeBufferType.Default);
        recipient.SetData(voxelsData);
        recipientB.SetData(colorData);

        int kIndex = editCompute.FindKernel("Populate");
        int editTextureSize = rawDensityTexture.width;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        editCompute.SetBuffer(kIndex, "recipient", recipient);
        editCompute.SetBuffer(kIndex, "recipientB", recipientB);
        editCompute.SetInt("textureSize", editTextureSize);
        editCompute.SetInt("numPointsPerAxis", numPointsPerAxis);
        PassTextureOnKernel(editCompute, "Populate");
        foreach ( MoodyChunk chunk in chunks)
        {
            Vector3 chunkCoord = (Vector3)chunk.id * (numPointsPerAxis - 1);
            editCompute.SetVector("chunkCoord", chunkCoord);
            ComputeHelper.Dispatch(editCompute, numVoxelsPerAxis, numVoxelsPerAxis, numVoxelsPerAxis, kIndex);
        }

        recipient.Release();

        // @Update chunk
        if ( createChunks)
            GenerateAllChunks();

        return true;

    }
    void CopySettingsFromOtherMoodulable(Moodulable m )
    {
        numChunks = m.numChunks;
        numPointsPerAxis = m.numPointsPerAxis;
        boundsSize = m.boundsSize;
        isoLevel = m.isoLevel;
        useFlatShading = m.useFlatShading;
        reduction = m.reduction;
        material = m.material;
        EnableMerging = m.EnableMerging;
        SmoothStepColor = m.SmoothStepColor;
    }
    #endregion

    #region IO
    public void Load()
    {
        if (loaded)
            return;
        LoadFromFile(initialSculptPath,false);
        MoodulableManager.OnFocusMoodulable(this);
        loaded = true;
        Debug.Log("Loading moodulable " + gameObject.name);
    }
    public void UnLoad() 
    {
        if (!loaded)
            return;
        // @ Save data
        Save();
        // @ Release buffers & textures
        ReleaseBuffers();
        // @ Set Load = false
        loaded = false;
        Debug.Log("Unloading moodulable " + gameObject.name);
    }
    public void Delete(bool destroyObject, bool deleteSculptPath)
    {
        // clear saved data.
        if (File.Exists(initialSculptPath) && deleteSculptPath)
        {
            IOManager.RemoveFileFromChecksumLog(initialSculptPath);
            File.Delete(initialSculptPath);
            return;
        }
        MoodulableManager.OnDeleteMoodulable(this);
        if (destroyObject)
            Destroy(this.gameObject);
        else
            Destroy(this);
    }
    public string Save()
    {

        List<byte> data = new List<byte>();
        // save all importants variables
        data.AddRange(BitConverter.GetBytes(numChunks));
        data.AddRange(BitConverter.GetBytes(numPointsPerAxis));
        data.AddRange(BitConverter.GetBytes(boundsSize));

        // Dump Voxel Data

        int size = numChunks * (numPointsPerAxis - 1) + 1;
        ComputeBuffer recipient = new ComputeBuffer(size * size * size, sizeof(float), ComputeBufferType.Default);
        ComputeBuffer recipientB = new ComputeBuffer(size * size * size * 4, sizeof(float), ComputeBufferType.Default);

        int kIndex = editCompute.FindKernel("Dump");
        int editTextureSize = rawDensityTexture.width;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        editCompute.SetInt("textureSize", editTextureSize);
        editCompute.SetBuffer(kIndex, "recipient", recipient);
        editCompute.SetBuffer(kIndex, "recipientB", recipientB);
        editCompute.SetInt("numPointsPerAxis", numPointsPerAxis);
        PassTextureOnKernel(editCompute, "Dump");
        foreach ( MoodyChunk chunk in chunks)
        {
            Vector3 chunkCoord = (Vector3)chunk.id * (numPointsPerAxis - 1);
            editCompute.SetVector("chunkCoord", chunkCoord);
            ComputeHelper.Dispatch(editCompute, numVoxelsPerAxis, numVoxelsPerAxis, numVoxelsPerAxis, kIndex);
        }

        float[] textureDump = new float[size * size * size];
        recipient.GetData(textureDump,0,0,textureDump.Length); 
        recipient.Release(); // release buffer

        float[] colorDump = new float[size * size * size * 4];
        recipientB.GetData(colorDump, 0, 0, colorDump.Length); 
        recipientB.Release(); // release buffer

        //store voxel
        foreach ( float f in textureDump)
        {
            data.AddRange(BitConverter.GetBytes(f));
        }
        //store color
        foreach (float f in colorDump)
        {
            data.AddRange(BitConverter.GetBytes(f));
        }

      
        byte[] checksum = CryptoUtilities.ComputeSHA(data.ToArray());
        string chksmstr = CryptoUtilities.SHAToHex(checksum);
        string path = P2SHARE.GetDirByType((byte)P2SHARE.CustomFileType.SculptFile) + chksmstr;

        if (IOManager.AddFileToChecksumLog(path)) // Return true if FileData not already exists
        {
            // also delete privously loaded texture 
            if (initialSculptPath != path)
            {
                if (initialSculptPath != null && initialSculptPath.Length > 0)
                {
                    // @ Remove only if others moodulable do not use this sculptPath && initialsculptpath is not empty checksum 
                    Moodulable[] moodulables = FindObjectsOfType<Moodulable>();
                    bool _delete = initialSculptPath == NullDataFilePath ? false : true;
                    foreach ( Moodulable m in moodulables)
                    {
                        if (m == this)
                            continue;
                        if (m.initialSculptPath == initialSculptPath)
                        {
                            _delete = false;
                            break;
                        }
                    }
                    if (_delete)
                    {
                        File.Delete(initialSculptPath);
                        IOManager.RemoveFileFromChecksumLog(initialSculptPath);
                    }

                }
                Debug.Log("Sculpt save at " + path);
                System.IO.File.WriteAllBytes(path, data.ToArray());
            }

        }
        initialSculptPath = path;
        return chksmstr;
    }
    // @ Net better performance.
    private string NullDataFilePath = "";
    private void ComputeEmptyCheckSumFile()
    {
        int size = numChunks * (numPointsPerAxis - 1) + 1;
        int totalfloat = (size * size * size) + (size * size * size * 4);
        List<byte> data = new List<byte>();
        // Save variables 
        data.AddRange(BitConverter.GetBytes(numChunks));
        data.AddRange(BitConverter.GetBytes(numPointsPerAxis));
        data.AddRange(BitConverter.GetBytes(boundsSize));
        
        for (int i = 0; i < totalfloat; i++)
        {
            data.AddRange(BitConverter.GetBytes((float)0));
        }
        byte[] checksum = CryptoUtilities.ComputeSHA(data.ToArray());
        string chksmstr = CryptoUtilities.SHAToHex(checksum);
        string path = P2SHARE.GetDirByType((byte)P2SHARE.CustomFileType.SculptFile) + chksmstr;
        if (IOManager.AddFileToChecksumLog(path))
        {
            System.IO.File.WriteAllBytes(path, data.ToArray());
        }
        NullDataFilePath = path;

    }
    
    #endregion

    private void Update()
    {
        if (ApplyCCL)
        {
            ComputeCCL26(true);
            ApplyCCL = false;
        }
        if (_Save)
        {
            Save();
            _Save = false;
        }
        if (_Recompute)
        {
            GenerateAllChunks();
            _Recompute = false;
        }
        if (_debug3DPrint)
        {
            _debug3DPrint = false;
            Try3DPrintThisObject();
        }

    }

    #region Memory Texture Initialization
    void InitTextures() 
    {
        // @ Create 3d textures. rawDensityTexture is the voxel area
        int size = numChunks * (numPointsPerAxis - 1) + 1;
        Create3DTexture(ref rawDensityTexture, size, "Raw Density Texture");
        Create3DColorTexture(ref ColoringTexture, size, "Coloring Texture");

        // @ Load texture to shaders foreach kernel
        meshCompute.SetTexture(0, "DensityTexture", rawDensityTexture);
        editCompute.SetTexture(0, "DensityTexture", rawDensityTexture);
        editCompute.SetTexture(editCompute.FindKernel("ApplyBox"), "DensityTexture", rawDensityTexture);
        editCompute.SetTexture(editCompute.FindKernel("ApplyCylinder"), "DensityTexture", rawDensityTexture);
        editCompute.SetTexture(editCompute.FindKernel("ApplyEllipsoid"), "DensityTexture", rawDensityTexture);
        // @ For Dumping
        editCompute.SetTexture(editCompute.FindKernel("Dump"), "DensityTexture", rawDensityTexture);
        editCompute.SetTexture(editCompute.FindKernel("Populate"), "DensityTexture", rawDensityTexture);
        editCompute.SetTexture(editCompute.FindKernel("Dump"), "ColoringTexture", ColoringTexture);
        editCompute.SetTexture(editCompute.FindKernel("Populate"), "ColoringTexture", ColoringTexture);

        // @edit 07.06 Set also coloring texture
        meshCompute.SetTexture(0, "ColoringTexture", ColoringTexture);
        editCompute.SetTexture(0, "ColoringTexture", ColoringTexture);
        //@ !!! PASS POINTER FOR CYLIDNER,BOX,ELLIPSOID
        editCompute.SetTexture(editCompute.FindKernel("ApplyBox"), "ColoringTexture", ColoringTexture);
        editCompute.SetTexture(editCompute.FindKernel("ApplyCylinder"), "ColoringTexture", ColoringTexture);
        editCompute.SetTexture(editCompute.FindKernel("ApplyEllipsoid"), "ColoringTexture", ColoringTexture);

        if (SmoothStepColor)
        {
            Create3DColorTexture(ref ColoringBuffer, size, "Coloring Buffer");
            editCompute.SetTexture(0, "ColoringBuffer", ColoringBuffer); // @ Note : this is apply sphere
            editCompute.SetTexture(editCompute.FindKernel("ApplyBox"), "ColoringBuffer", ColoringBuffer);
            editCompute.SetTexture(editCompute.FindKernel("ApplyCylinder"), "ColoringBuffer", ColoringBuffer);
            editCompute.SetTexture(editCompute.FindKernel("ApplyEllipsoid"), "ColoringBuffer", ColoringBuffer);
            //@ !!! PASS POINTER FOR CYLIDNER,BOX,ELLIPSOID
            editCompute.SetTexture(editCompute.FindKernel("CopyColorBuffer"), "ColoringBuffer", ColoringBuffer);
            editCompute.SetTexture(editCompute.FindKernel("CopyColorBuffer"), "ColoringBuffer", ColoringBuffer);
            editCompute.SetTexture(editCompute.FindKernel("CopyColorBuffer"), "ColoringTexture", ColoringTexture);
        }
    }
    // Long wrapper that will help passing texure pointer  
    public void PassTextureOnKernel(ComputeShader cs , string kernelName)
    {
        int kIndex = cs.FindKernel(kernelName);
        switch (kernelName)
        {
            case "ApplySphere":
                cs.SetTexture(0, "ColoringTexture", ColoringTexture);
                cs.SetTexture(0, "DensityTexture", rawDensityTexture);
                if (SmoothStepColor)
                    cs.SetTexture(0, "ColoringBuffer", ColoringBuffer);
                break;
            case "ProcessCube":
                cs.SetTexture(kIndex, "ColoringTexture", ColoringTexture);
                cs.SetTexture(kIndex, "DensityTexture", rawDensityTexture);
                break;
            case "ApplyBox":
                cs.SetTexture(kIndex, "ColoringTexture", ColoringTexture);
                cs.SetTexture(kIndex, "DensityTexture", rawDensityTexture);
                if (SmoothStepColor)
                    cs.SetTexture(kIndex, "ColoringBuffer", ColoringBuffer);
                break;
            case "ApplyEllipsoid":
                cs.SetTexture(kIndex, "ColoringTexture", ColoringTexture);
                cs.SetTexture(kIndex, "DensityTexture", rawDensityTexture);
                if (SmoothStepColor)
                    cs.SetTexture(kIndex, "ColoringBuffer", ColoringBuffer);
                break;
            case "ApplyCylinder":
                cs.SetTexture(kIndex, "ColoringTexture", ColoringTexture);
                cs.SetTexture(kIndex, "DensityTexture", rawDensityTexture);
                if (SmoothStepColor)
                    cs.SetTexture(kIndex, "ColoringBuffer", ColoringBuffer);
                break;
            case "CopyColorBuffer":
                cs.SetTexture(kIndex, "ColoringTexture", ColoringTexture);
                cs.SetTexture(kIndex, "ColoringBuffer", ColoringBuffer);
                break;
            case "ClearChunk":
                cs.SetTexture(kIndex, "DensityTexture", rawDensityTexture);
                break;
            case "CopyChunk":
                cs.SetTexture(kIndex, "ColoringTexture", ColoringTexture);
                cs.SetTexture(kIndex, "DensityTexture", rawDensityTexture);
                break;
            case "Dump":
                cs.SetTexture(kIndex, "ColoringTexture", ColoringTexture);
                cs.SetTexture(kIndex, "DensityTexture", rawDensityTexture);
                break;
            case "Populate":
                cs.SetTexture(kIndex, "ColoringTexture", ColoringTexture);
                cs.SetTexture(kIndex, "DensityTexture", rawDensityTexture);
                break;
            case "CCL_SIMPLIFY":
                cs.SetTexture(kIndex, "DensityTexture", rawDensityTexture);
                break;

        }


    }
    void CreateBuffers() 
    {
        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;
        int maxVertexCount = maxTriangleCount * 3;

        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        triangleBuffer = new ComputeBuffer(maxVertexCount, ComputeHelper.GetStride<VertexData>(), ComputeBufferType.Append);
        vertexDataArray = new VertexData[maxVertexCount];
        // @ Create Texture buffer for CCL.
        int voxelNumbers = numChunks * (numPointsPerAxis - 1) + 1;
        classCCLBuff = new ComputeBuffer(voxelNumbers * voxelNumbers * voxelNumbers, sizeof(int), ComputeBufferType.Default);
        bitCCLBuff = new ComputeBuffer(voxelNumbers * voxelNumbers * voxelNumbers, sizeof(int), ComputeBufferType.Default); // this one probably crashing too ... 
        

    }
    void ReleaseBuffers()
    {
        ComputeHelper.Release(triangleBuffer, triCountBuffer, bitCCLBuff, classCCLBuff);
        rawDensityTexture.Release();
        ColoringTexture.Release();
        if (SmoothStepColor)
            ColoringBuffer.Release();
        vertexDataArray = new VertexData[0];
    }
    void OnDestroy()
    {
       
        ReleaseBuffers();

        
    }

    #endregion

    #region 3DPRINTING
    // @ Send Print Command for 3D Printing via TCP communication
    // @ from 07.06.2022
    public bool Try3DPrintThisObject()
    {
        GameObject _toExport = CreateSingleMesh();
        string filePath = Application.persistentDataPath + "/mesh_"+ Utilities.GetTimeStamp() +".obj"; 
        ObjExporter.ExportMesh(_toExport, filePath);
        // Construct Byte Array Command

        List<byte> data = new List<byte>();
        data.Add(1); // header...
        // Share the file Path
        data.Add((byte)filePath.Length);
        foreach (char c in filePath.ToCharArray())
            data.Add((byte)c);

        Plotting.SendDataToTCPProgram(data.ToArray());
        Destroy(_toExport.gameObject);
        return true;
    }
    // @ Create one single mesh from Chunk
    // @ from 07.06.2022
    GameObject CreateSingleMesh(bool _useSubMesh = false)
    {
        // Prepare Mesh Object
        GameObject g = new GameObject(this.name + "_SingleMesh");
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        MeshFilter filter = g.AddComponent<MeshFilter>();
        MeshRenderer renderer = g.AddComponent<MeshRenderer>();
        filter.mesh = mesh;
        g.transform.localScale = this.transform.localScale;
        renderer.material = material;
        // Just concat all tris, vertices and normal of chunk meshes 
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> tris = new List<int>();
        // we can also use submesh but well
        int vertOffset = 0;
        foreach (MoodyChunk chunk in chunks)
        {
            // Do not proccess empty mesh
            if (!chunk.mesh)
                continue;
            if (chunk.mesh.vertexCount == 0)
                continue;
            // add vertices and normals of current chunk 
            vertices.AddRange(chunk.mesh.vertices);
            normals.AddRange(chunk.mesh.normals);
            // add vertices relatives to vertOffset; 
            for (int i = 0; i < chunk.mesh.triangles.Length; i++)
            {
                tris.Add(chunk.mesh.triangles[i] + vertOffset);
            }
            vertOffset = vertices.Count;

        }
        // @ Apply
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();

        if (useFlatShading)
            mesh.RecalculateNormals();
        else
            mesh.normals = normals.ToArray();


        return g;

    }
    #endregion

    #region MeshCreation
    void CreateChunks() 
    {
        chunks = new MoodyChunk[numChunks * numChunks * numChunks];
        float chunkSize = (boundsSize) / numChunks;
        int i = 0;

        for (int y = 0; y < numChunks; y++)
        {
            for (int x = 0; x < numChunks; x++)
            {
                for (int z = 0; z < numChunks; z++)
                {
                    Vector3Int coord = new Vector3Int(x, y, z);
                    float posX = (-(numChunks - 1f) / 2 + x) * chunkSize;
                    float posY = (-(numChunks - 1f) / 2 + y) * chunkSize;
                    float posZ = (-(numChunks - 1f) / 2 + z) * chunkSize;
                    Vector3 centre = new Vector3(posX, posY, posZ);

                    GameObject meshHolder = new GameObject($"Chunk ({x}, {y}, {z})");
                    meshHolder.transform.parent = transform;
                    meshHolder.layer = gameObject.layer;

                    MoodyChunk chunk = new MoodyChunk(coord, centre, chunkSize, numPointsPerAxis, meshHolder);
                    chunk.SetMaterial(material);
                    chunks[i] = chunk;
                    i++;
                }
            }
        }
    }
    public void GenerateAllChunks()
    {
       
        for (int i = 0; i < chunks.Length; i++)
        {
            GenerateChunk(chunks[i]);
        }
    }
    public void TryApplyConvexGeometry()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].TryConvexGeometry();
        }
    }
    void GenerateChunk(MoodyChunk chunk) 
    {
        // Marching cubes
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int marchKernel = 0;

        PassTextureOnKernel(meshCompute, "ProcessCube");
        meshCompute.SetInt("textureSize", rawDensityTexture.width);
        meshCompute.SetInt("numPointsPerAxis", numPointsPerAxis);
        meshCompute.SetFloat("isoLevel", isoLevel);
        meshCompute.SetFloat("boundsSize", boundsSize); 
        triangleBuffer.SetCounterValue(0);
        meshCompute.SetBuffer(marchKernel, "triangles", triangleBuffer);

        Vector3 chunkCoord = (Vector3)chunk.id * (numPointsPerAxis - 1);
        meshCompute.SetVector("chunkCoord", chunkCoord);

        meshCompute.SetBuffer(marchKernel, "labels", classCCLBuff); // this one is crashing
        ComputeHelper.Dispatch(meshCompute, numVoxelsPerAxis, numVoxelsPerAxis, numVoxelsPerAxis, marchKernel);

        // Create mesh
        int[] vertexCountData = new int[1];
        triCountBuffer.SetData(vertexCountData);
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);

        triCountBuffer.GetData(vertexCountData);

        int numVertices = vertexCountData[0] * 3;

        // Fetch vertex data from GPU

        triangleBuffer.GetData(vertexDataArray, 0, 0, numVertices);

        chunk.CreateMesh(vertexDataArray, numVertices, useFlatShading);
    }
    #endregion

    #region Mesh Generation
    public void CleanChunks(List<MoodyChunk> chs)
    {
        if (!loaded)
            Load();

        int kIndex = editCompute.FindKernel("ClearChunk");
        int editTextureSize = rawDensityTexture.width;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        PassTextureOnKernel(editCompute, "ClearChunk");
        foreach ( MoodyChunk chunk in chs)
        {
            editCompute.SetInt("numPointsPerAxis", numPointsPerAxis);
            editCompute.SetInt("textureSize", editTextureSize); // needed ?
            Vector3 chunkCoord = (Vector3)chunk.id * (numPointsPerAxis - 1);
            editCompute.SetVector("chunkCoord", chunkCoord);
            editCompute.SetTexture(kIndex, "DensityTexture", rawDensityTexture);
            ComputeHelper.Dispatch(editCompute, numVoxelsPerAxis, numVoxelsPerAxis, numVoxelsPerAxis, kIndex);
        }
    }
    public void MergeMoodulable(Moodulable otherMoodulable, List<MoodyChunk> otherChunks, bool _substract, bool _recenter) 
    {
        if (otherChunks.Count == 0)
            return;

        if (!loaded)
            Load();

        int kIndex = editCompute.FindKernel("CopyChunk");
        int editTextureSize = rawDensityTexture.width;
        editCompute.SetInt("copyTexSize", otherMoodulable.rawDensityTexture.width);
        editCompute.SetInt("textureSize", rawDensityTexture.width);
        editCompute.SetFloat("isoLevel", isoLevel);
        // pass texture
        PassTextureOnKernel(editCompute, "CopyChunk");
        editCompute.SetTexture(kIndex, "copyTexture", otherMoodulable.rawDensityTexture);
        editCompute.SetTexture(kIndex, "DensityTexture", rawDensityTexture);
        editCompute.SetTexture(kIndex, "copyColor", otherMoodulable.ColoringTexture);
        editCompute.SetTexture(kIndex, "ColoringTexture", ColoringTexture);
        

        // set offset here but should be in arguments...
        editCompute.SetInt("copyNPPA", otherMoodulable.numPointsPerAxis);
        editCompute.SetBool("Substract", _substract);

        // Compute Offset if needed
        GameObject CopyMoodulable = otherChunks[0].gameObject.transform.parent.gameObject;
        // calculate offset of different position
        Vector3 Offset = this.transform.position- CopyMoodulable.transform.position; 
        Offset *= (1 / reduction);
        Offset = ConvertWorldToTextureCoord(Offset); // return 27 27 
        Vector3Int origin = ConvertWorldToTextureCoord(new Vector3(0, 0, 0));
        Offset -= origin;


        foreach ( MoodyChunk chunk in otherChunks) 
        {
            int numVoxelsPerAxis = otherMoodulable.numPointsPerAxis - 1;
            Vector3 chunkCoord = (Vector3)chunk.id * (numPointsPerAxis - 1);
            if (!_recenter)
                editCompute.SetVector("copyOffset",-Offset);
            else
                editCompute.SetVector("copyOffset", Vector3.zero);
            editCompute.SetVector("copychunkCoord", chunkCoord);
            ComputeHelper.Dispatch(editCompute, numVoxelsPerAxis, numVoxelsPerAxis, numVoxelsPerAxis, kIndex);
        }

    }
    public Vector3Int ConvertWorldToTextureCoord(Vector3 point)
    {
        // we admit point is already convert to moodable local space 

        int editTextureSize = rawDensityTexture.width;
        float tx = Mathf.Clamp01((point.x + boundsSize / 2) / boundsSize);
        float ty = Mathf.Clamp01((point.y + boundsSize / 2) / boundsSize);
        float tz = Mathf.Clamp01((point.z + boundsSize / 2) / boundsSize);

        int editX = Mathf.RoundToInt(tx * (editTextureSize - 1));
        int editY = Mathf.RoundToInt(ty * (editTextureSize - 1));
        int editZ = Mathf.RoundToInt(tz * (editTextureSize - 1));
        return new Vector3Int(editX, editY, editZ);
    }
    #endregion

    #region Voxel Editing
    // @ Wrapper 
    // @ No Paint. No Shape.
    // Use Dictionnary to record memory of touched Object
    private Dictionary<Moodulable, int> MoodulableTouched = new Dictionary<Moodulable, int>();
    public IEnumerator DetetectMerging()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (!loaded)
                continue;
            if (!EnableMerging)
                continue;
            // Do not merge if not grabbing
            Grabbable myGrabbable = GetComponent<Grabbable>();
            if (!myGrabbable)
                continue;
            if (!myGrabbable._grabbed || !myGrabbable._userisgrabber)
                continue;
           
            // we can use GetBoundariesofgroupofchunk
            Moodulable[] moodulables = FindObjectsOfType<Moodulable>();
            List<Moodulable> touched = new List<Moodulable>();
            // My Bounds
            Bounds A = GetBoundariesOfGroupOfChunk(chunks.ToList());
            foreach (Moodulable m in moodulables)
            {
                if (m == this)
                    continue;

                // check if touched Contains... cannot list.contains? 
                bool _contains = false;
                foreach (Moodulable t in touched)
                {
                    if (t.gameObject == gameObject)
                    {
                        _contains = true;
                        break;
                    }
                }
                if (_contains)
                    continue;

                // Only Merge with object which are not grabbed by others player..
                Grabbable b = m.GetComponent<Grabbable>();
                if (b) 
                { 
                    if (b._grabbed && !b._userisgrabber)
                        continue;
                }


                // [1] check if m bounds intersect with my Moodulable ( distance function )
                float distance = Vector3.Distance(m.transform.position, transform.position);
                // distance should be lower than myMoodulable.radius + m.radius
                float rad1 = boundsSize * reduction;
                float rad2 = m.boundsSize * m.reduction;
                if (distance >= rad1 + rad2)
                    continue;

                // [2] check bounds intersecting
                Bounds B = GetBoundariesOfGroupOfChunk(m.chunks.ToList());
                if ((A.Contains(B.min) && A.Contains(B.max)) || A.Intersects(B))
                {
                    touched.Add(m);
                }
            }
            // Update Dictionnary from current values 
            foreach (Moodulable t in touched)
            {
                if (MoodulableTouched.ContainsKey(t))
                {
                    MoodulableTouched[t]++;
                }
                else
                    MoodulableTouched.Add(t, 0);
            }
            // Merge and Pop 
            foreach (Moodulable t in touched)
            {
                if (MoodulableTouched[t] > TimeHoldBeforeMerging)
                {
                    if (t)
                    {
                        Debug.Log("Merging " + this.name + " with " + t.name);
                        MergeMoodulable(t, t.chunks.ToList(), false, false);
                        
                        // Sync
                        List<byte> data = new List<byte>();
                        data.Add(27);
                        data.Add((byte)this.name.Length);
                        foreach (char c in this.name.ToCharArray())
                            data.Add((byte)c);
                        data.Add((byte)t.name.Length);
                        foreach (char c in t.name.ToCharArray())
                            data.Add((byte)c);
                        NetUtilities.SendDataToAll(data.ToArray());
                        
                        // Destroy because we not copy paste...
                        t.Delete(true, false); 
                    }
                    // pop 
                    MoodulableTouched.Remove(t);
                    // regen
                    GenerateAllChunks();
                    TryApplyConvexGeometry();
                    break;
                }
                // We can update graphics here
            }


        }

    }
    // Fast Net merging implementation
    public static void OnMoodulableMergingReceived(byte[] data)
    {
        byte namesize = data[1];
        char[] mchar = new char[namesize];
        int bctr = 2;
        for (int i = 0; i < namesize; i++)
        {
            mchar[i] = (char)data[bctr]; bctr++;
        }
        GameObject mood1 = GameObject.Find(new string(mchar));
        
        namesize = data[bctr];
        mchar = new char[namesize];
        for (int i = 0; i < namesize; i++)
        {
            mchar[i] = (char)data[bctr]; bctr++;
        }

        GameObject mood2 = GameObject.Find(new string(mchar));
        if (!mood1 || !mood2)
            return;

        Moodulable mood = mood1.GetComponent<Moodulable>();
        Moodulable othermood = mood2.GetComponent<Moodulable>();
        mood.MergeMoodulable(othermood, othermood.chunks.ToList(), false, false);
        othermood.Delete(true, false);
        mood.GenerateAllChunks();
        mood.TryApplyConvexGeometry();
    }
    
    public Vector3 ApplyShape(string ShapeName,  
                              Vector3 position, Vector3 scale, Quaternion rotation, 
                              float weight, bool ConvertPointToLocal = true, bool Smoothstep = true)
    {
        switch (ShapeName.Replace(" ","").ToLower())
        {
            case "box": return ApplyBox(null, position, scale, rotation, weight, false, Color.black, ConvertPointToLocal, Smoothstep);
            case "cylinder": return ApplyCylinder(null, position, scale, rotation, weight, false, Color.black, ConvertPointToLocal, Smoothstep);
            case "ellipsoid": return ApplyEllipsoid(null, position, scale, rotation, weight, false, Color.black, ConvertPointToLocal, Smoothstep);
            case "sphere": return ApplySphere(null, position, weight, scale.x * (1 / reduction), false, Color.black, ConvertPointToLocal, Smoothstep);
        }

        return Vector3.zero;
    }
    public Vector3 ApplyShape(string ShapeName,
                             Vector3 position, Vector3 scale, Quaternion rotation,
                             float weight, Color c, bool ConvertPointToLocal = true, bool Smoothstep = true)
    {
        switch (ShapeName.Replace(" ", "").ToLower())
        {
            case "box": return ApplyBox(null, position, scale, rotation, weight, true, c, ConvertPointToLocal, Smoothstep);
            case "cylinder": return ApplyCylinder(null, position, scale, rotation, weight, true, c, ConvertPointToLocal, Smoothstep);
            case "ellipsoid": return ApplyEllipsoid(null, position, scale, rotation, weight, true, c, ConvertPointToLocal, Smoothstep);
            case "sphere": return ApplySphere(null, position, weight, scale.x * (1 / reduction), true, c, ConvertPointToLocal, Smoothstep);
        }

        return Vector3.zero;
    }
    // @ Paint && Shape.
    public Vector3 ApplyShape(string ShapeName, GameObject Shape, Color c, float weight, bool ConvertPointToLocal = true, bool Smoothstep = true)
    {
        switch (ShapeName.Replace(" ", "").ToLower())
        {
            case "box": return ApplyBox(Shape, Shape.transform.position, Shape.transform.localScale, Shape.transform.rotation,
            weight, true, c, ConvertPointToLocal, Smoothstep);
            case "cylinder":
                return ApplyCylinder(Shape, Shape.transform.position, Shape.transform.localScale, Shape.transform.rotation,
                                weight, true, c, ConvertPointToLocal, Smoothstep);
            case "ellipsoid":
                return ApplyEllipsoid(Shape, Shape.transform.position, Shape.transform.localScale, Shape.transform.rotation,
                                weight, true, c, ConvertPointToLocal, Smoothstep);
            case "sphere": return ApplySphere(null, Shape.transform.position, weight, Shape.transform.localScale.x * (1 / reduction), true, c, ConvertPointToLocal, Smoothstep);
        }

        return Vector3.zero;
    }
    // @ Shape.
    public Vector3 ApplyShape(string ShapeName, GameObject Shape, float weight, bool ConvertPointToLocal = true, bool Smoothstep = true)
    {
        switch (ShapeName.Replace(" ", "").ToLower())
        {
            case "box": return ApplyBox(Shape, Shape.transform.position, Shape.transform.localScale, Shape.transform.rotation,
            weight, false, Color.black, ConvertPointToLocal, Smoothstep);
            case "cylinder":
                return ApplyCylinder(Shape, Shape.transform.position, Shape.transform.localScale, Shape.transform.rotation,
                                weight, false, Color.black, ConvertPointToLocal, Smoothstep);
            case "ellipsoid":
                return ApplyEllipsoid(Shape, Shape.transform.position, Shape.transform.localScale, Shape.transform.rotation,
                                weight, false, Color.black, ConvertPointToLocal, Smoothstep);
            case "sphere": return ApplySphere(null, Shape.transform.position, weight, Shape.transform.localScale.x * (1 / reduction), 
                false, Color.black, ConvertPointToLocal, Smoothstep);
        }

        return Vector3.zero;
    }

    public Vector3 ApplyBox(GameObject shape, Vector3 position, Vector3 scale, Quaternion rotation, float weight, bool Paint, Color c,
                                bool ConvertPointToLocal = true, bool Smoothstep = true)
    {
        if (!loaded)
            Load();

        int kIndex = editCompute.FindKernel("ApplyBox");

        Vector3 point = position;
        Vector3 convertedLocal = Vector3.zero;
        
        if (ConvertPointToLocal)
        {// Set point to its origin.
            point -= this.transform.position;
            // Apply rotation of this point
            point = Quaternion.Inverse(this.transform.rotation) * point;
            // Scale this point to real-pseudo size
            Vector3 sc = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
            point = Vector3.Scale(point, sc);
            convertedLocal = new Vector3(point.x, point.y, point.z);

        }
        
        // Convert this point to texture coord
        Vector3Int coordPos = ConvertWorldToTextureCoord(point); // coord is good. 

        // get extends
        Vector3 extends = scale * 0.5f;
        extends *= (1 / reduction);

        Vector3Int BoxExtends = ConvertWorldToTextureCoord(extends);
        Vector3Int origin = ConvertWorldToTextureCoord(new Vector3(0, 0, 0));
        BoxExtends -= origin;

        // set up quat to translate local coord
        Quaternion inv = Quaternion.Inverse(rotation);

        int editTextureSize = rawDensityTexture.width;
        editCompute.SetInts("brushCentre", coordPos.x, coordPos.y, coordPos.z); // OK
        editCompute.SetInts("extends", BoxExtends.x, BoxExtends.y, BoxExtends.z);
        editCompute.SetFloats("quaternion", inv.x, inv.y, inv.z, inv.w);
        editCompute.SetBool("_smoothstep", Smoothstep);
        editCompute.SetFloat("weight", weight);
        editCompute.SetInt("textureSize", editTextureSize);
        editCompute.SetBool("Paint", Paint);
        PassTextureOnKernel(editCompute, "ApplyBox");
        if (Paint)
            editCompute.SetFloats("brushColor", c.r, c.g, c.b, c.a);
        ComputeHelper.Dispatch(editCompute, editTextureSize, editTextureSize, editTextureSize, kIndex);
        if (SmoothStepColor) //@ 2nd round dispatch
        {
            PassTextureOnKernel(editCompute, "CopyColorBuffer");
            ComputeHelper.Dispatch(editCompute, editTextureSize, editTextureSize, editTextureSize, editCompute.FindKernel("CopyColorBuffer"));
        }
        

        if (shape)
        {
            LayerMask mask = LayerMask.GetMask("Mooduler");
            for (int i = 0; i < chunks.Length; i++)
            {
                MoodyChunk chunk = chunks[i];
                Vector3 offset = chunk.centre * reduction;
                if ( Physics.CheckBox(chunk.gameObject.transform.position + offset, 
                    (Vector3.one * chunk.size) * reduction * 0.5f, 
                    transform.rotation))
                {
                    GenerateChunk(chunk);
                }
            }
        }
        else
            GenerateAllChunks();
      
        return convertedLocal;

    }
    public Vector3 ApplyEllipsoid(GameObject shape, Vector3 position, Vector3 scale, Quaternion rotation, float weight, bool Paint, Color c,
                                bool ConvertPointToLocal = true, bool Smoothstep = true)
    {
        if (!loaded)
            Load();

        int kIndex = editCompute.FindKernel("ApplyEllipsoid");

        Vector3 point = position;
        Vector3 convertedLocal = Vector3.zero;
        if (ConvertPointToLocal)
        {
            // Set point to its origin.
            point -= this.transform.position;
            // Apply rotation of this point
            point = Quaternion.Inverse(this.transform.rotation) * point;
            // Scale this point to real-pseudo size
            Vector3 sc = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
            point = Vector3.Scale(point, sc);
            convertedLocal = new Vector3(point.x, point.y, point.z);
            // Convert this point to texture coord
        }

        Vector3Int coordPos = ConvertWorldToTextureCoord(point); // coord is good. 

        // get extends
        Vector3 extends = scale;// * 0.5f; we do not need to mul by one for this i guess 
        extends *= (1 / reduction);

        Vector3Int sphextends = ConvertWorldToTextureCoord(extends);
        Vector3Int origin = ConvertWorldToTextureCoord(new Vector3(0, 0, 0));
        sphextends -= origin;

        // set up quat to translate local coord
        Quaternion inv = Quaternion.Inverse(rotation);

        int editTextureSize = rawDensityTexture.width;
        editCompute.SetInts("brushCentre", coordPos.x, coordPos.y, coordPos.z); 
        editCompute.SetInts("extends", sphextends.x, sphextends.y, sphextends.z);
        editCompute.SetFloats("quaternion", inv.x, inv.y, inv.z, inv.w);
        editCompute.SetBool("_smoothstep", Smoothstep);
        editCompute.SetFloat("weight", weight);
        editCompute.SetBool("Paint", Paint);
        PassTextureOnKernel(editCompute, "ApplyEllipsoid");
        if (Paint)
            editCompute.SetFloats("brushColor", c.r, c.g, c.b, c.a);
        ComputeHelper.Dispatch(editCompute, editTextureSize, editTextureSize, editTextureSize, kIndex);
        if (SmoothStepColor) //@ 2nd round dispatch
        {
            PassTextureOnKernel(editCompute, "CopyColorBuffer");
            ComputeHelper.Dispatch(editCompute, editTextureSize, editTextureSize, editTextureSize, editCompute.FindKernel("CopyColorBuffer"));
        }

        if (shape)
        {
            LayerMask mask = LayerMask.GetMask("Mooduler");
            for (int i = 0; i < chunks.Length; i++)
            {
                MoodyChunk chunk = chunks[i];
                Vector3 offset = chunk.centre * reduction;
                if (Physics.CheckBox(chunk.gameObject.transform.position + offset,
                    (Vector3.one * chunk.size) * reduction * 0.5f,
                    transform.rotation))
                {
                    GenerateChunk(chunk);
                }
            }
        }
        else
            GenerateAllChunks();

        return convertedLocal;
    }

    public Vector3 ApplyCylinder(GameObject shape, Vector3 position, Vector3 scale, Quaternion rotation, float weight, bool Paint, Color c,
                                bool ConvertPointToLocal = true, bool Smoothstep = true) 
    {

        if (!loaded)
            Load();

        int kIndex = editCompute.FindKernel("ApplyCylinder");

        Vector3 point = position;
        Vector3 convertedLocal = Vector3.zero;
        if (ConvertPointToLocal)
        {
            // Set point to its origin.
            point -= this.transform.position;
            // Apply rotation of this point
            point = Quaternion.Inverse(this.transform.rotation) * point;
            // Scale this point to real-pseudo size
            Vector3 sc = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
            point = Vector3.Scale(point, sc);
            convertedLocal = new Vector3(point.x, point.y, point.z);
        }
        
       
        // Convert this point to texture coord
        Vector3Int coordPos = ConvertWorldToTextureCoord(point); // coord is good. 


        // Height seems not be acurate
        
        Vector3 extends = scale * 0.5f;
        extends *= (1 / reduction);
        Vector3Int Cylextends = ConvertWorldToTextureCoord(extends);
        Vector3Int origin = ConvertWorldToTextureCoord(new Vector3(0, 0, 0));
        Cylextends -= origin;
        float radius = Cylextends.x / 2;
        
        //float radius = scale.x;
        // Compute height (Y) 

        extends = scale;
        extends *= (1 / reduction);
        Vector3Int CylextendsB = ConvertWorldToTextureCoord(extends);
        CylextendsB -= origin;
        float height = CylextendsB.y;


        // set up quat to translate local coord
        Quaternion inv = Quaternion.Inverse(rotation);

        int editTextureSize = rawDensityTexture.width;
        editCompute.SetInts("brushCentre", coordPos.x, coordPos.y, coordPos.z); // OK
        editCompute.SetFloats("quaternion", inv.x, inv.y, inv.z, inv.w);
        editCompute.SetBool("_smoothstep", Smoothstep);
        editCompute.SetFloat("weight", weight);
        editCompute.SetFloat("brushRadius", radius);
        editCompute.SetFloat("height", height);
        editCompute.SetInt("textureSize", editTextureSize);
        editCompute.SetBool("Paint", Paint);
        PassTextureOnKernel(editCompute, "ApplyCylinder");
        if (Paint)
            editCompute.SetFloats("brushColor", c.r, c.g, c.b, c.a);
        ComputeHelper.Dispatch(editCompute, editTextureSize, editTextureSize, editTextureSize, kIndex);
        if (SmoothStepColor) //@ 2nd round dispatch
        {
            PassTextureOnKernel(editCompute, "CopyColorBuffer");
            ComputeHelper.Dispatch(editCompute, editTextureSize, editTextureSize, editTextureSize, editCompute.FindKernel("CopyColorBuffer"));
        }


        if (shape)
        {
            LayerMask mask = LayerMask.GetMask("Mooduler");
            for (int i = 0; i < chunks.Length; i++)
            {
                MoodyChunk chunk = chunks[i];
                Vector3 offset = chunk.centre * reduction;
                if (Physics.CheckBox(chunk.gameObject.transform.position + offset,
                    (Vector3.one * chunk.size) * reduction * 0.5f,
                    transform.rotation))
                {
                    GenerateChunk(chunk);
                }
            }
        }
        else
            GenerateAllChunks();

        return convertedLocal;

    }

    public Vector3 ApplySphere(GameObject shape, Vector3 point, float weight, float radius, bool Paint, Color c,
                                bool ConvertPointToLocal = true, bool Smoothstep = true) 
    {
        if (!loaded)
            Load();

        // I really need to rewrite this function because its badly coded
        // @ Get Kernel
        int kIndex = editCompute.FindKernel("ApplySphere");
        Vector3 convertedLocal = new Vector3();
        if (ConvertPointToLocal)
        {
            // Set point to its origin.
            point -= this.transform.position;
            // Apply rotation of this point
            point = Quaternion.Inverse(this.transform.rotation) * point;
            // Scale this point to real-pseudo size
            Vector3 sc = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
            point = Vector3.Scale(point, sc);
            convertedLocal = new Vector3(point.x, point.y, point.z); // it seems to not render stuff
            
        }
        // Convert this point to texture coord
        Vector3Int coordPos = ConvertWorldToTextureCoord(point);


        int editTextureSize = rawDensityTexture.width;
        float editPixelWorldSize = boundsSize / editTextureSize;
        int editRadius = Mathf.CeilToInt(radius / editPixelWorldSize);

        editCompute.SetBool("_smoothstep", Smoothstep);
        editCompute.SetFloat("weight", weight);
        editCompute.SetInts("brushCentre", coordPos.x, coordPos.y, coordPos.z);
        editCompute.SetFloat("brushRadius", editRadius);
        editCompute.SetInt("textureSize", editTextureSize);
        editCompute.SetBool("Paint", Paint);
        PassTextureOnKernel(editCompute, "ApplySphere");
        
        if ( Paint)
            editCompute.SetFloats("brushColor", c.r, c.g, c.b, c.a);


        ComputeHelper.Dispatch(editCompute, editTextureSize, editTextureSize, editTextureSize, kIndex); 
        if (SmoothStepColor) //@ 2nd round dispatch
        {
            PassTextureOnKernel(editCompute, "CopyColorBuffer");
            ComputeHelper.Dispatch(editCompute, editTextureSize, editTextureSize, editTextureSize, editCompute.FindKernel("CopyColorBuffer"));
        }
             

        // @ Recompute all chunks that intersect sphere humm i understaand now ...
        float worldRadius = (editRadius + 1) * editPixelWorldSize;

        for (int i = 0; i < chunks.Length; i++) // Modify chunk if they meet the stuff 
        {
            MoodyChunk chunk = chunks[i];
            if (MathUtilities.SphereIntersectsBox(point, worldRadius, chunk.centre, Vector3.one * chunk.size))
            {
                GenerateChunk(chunk); // re-generate the chunk if inside collision ... 
            }
        }

        return convertedLocal;

    }

    #endregion

    #region CCL26 + Physics
    public CCL26.ComputeTask ComputeCCL26(bool clearTaskWhenDone, bool onCPU = true)
    {
        if (!EnableCCLComputing)
            return null;
        if (!loaded)
            Load();

        Debug.Log("APPLYING CCL");
        int CCL_SIMPLIFY_kIndex = CCLCompute.FindKernel("CCL_SIMPLIFY");
        int CCL_INIT_kIndex = CCLCompute.FindKernel("CCL_INIT");
        int CCL_ROWCOLSCAN_kIndex = CCLCompute.FindKernel("CCL_ROWCOLSCAN");
        int CCL_ROWSCAN_kIndex = CCLCompute.FindKernel("CCL_ROWSCAN");
        int CCL_REFINE_kIndex = CCLCompute.FindKernel("CCL_REFINE");

        int voxelNumbers = numChunks * (numPointsPerAxis - 1) + 1;

        //@ Set up ALL-USE variables
        CCLCompute.SetInt("voxelNumbers", voxelNumbers);
        CCLCompute.SetFloat("isoLevel", isoLevel);
        CCLCompute.SetInt("numPointsPerAxis", numPointsPerAxis);
        PassTextureOnKernel(CCLCompute, "CCL_SIMPLIFY");

        int numVoxelsPerAxis = numPointsPerAxis - 1;
        // @ Apply CCL_SIMPLIFIY

        foreach (MoodyChunk chunk in chunks)
        {
            Vector3 chunkCoord = (Vector3)chunk.id * (numPointsPerAxis - 1);
            CCLCompute.SetVector("chunkCoord", chunkCoord);
            CCLCompute.SetBuffer(CCL_SIMPLIFY_kIndex, "bit", bitCCLBuff);
            CCLCompute.SetTexture(CCL_SIMPLIFY_kIndex, "EditTexture", rawDensityTexture);
            ComputeHelper.Dispatch(CCLCompute, numVoxelsPerAxis, numVoxelsPerAxis, numVoxelsPerAxis, CCL_SIMPLIFY_kIndex);
        }


        // Test CPU-Side. Performance is bad (40seconds) for large chunk but very fast for smaller chunk. 
        // There is an issue inside my computeshader. Because i got DX11 exception (timeout) 

        if (onCPU)
        {

            int[] bitsarray = new int[voxelNumbers * voxelNumbers * voxelNumbers];
            bitCCLBuff.GetData(bitsarray, 0, 0, bitsarray.Length);
            return CCL26.ComputeCCL(this, bitsarray, voxelNumbers, clearTaskWhenDone);

        }

        /*
        // @ Apply CCL_INIT
        CCLCompute.SetBuffer(CCL_INIT_kIndex, "bit", bitCCLBuff);
        CCLCompute.SetBuffer(CCL_INIT_kIndex, "classifications", classCCLBuff);
        CCLCompute.SetBuffer(CCL_INIT_kIndex, "equivalents", eqCCLBuff);
        ComputeHelper.Dispatch(CCLCompute, voxelNumbers, voxelNumbers, voxelNumbers, CCL_INIT_kIndex);
        // @ Apply CCL_ROWCOLSCAN_kIndex
        CCLCompute.SetBuffer(CCL_ROWCOLSCAN_kIndex, "bit", bitCCLBuff);
        CCLCompute.SetBuffer(CCL_ROWCOLSCAN_kIndex, "classifications", classCCLBuff);
        CCLCompute.SetBuffer(CCL_ROWCOLSCAN_kIndex, "equivalents", eqCCLBuff);
        ComputeHelper.Dispatch(CCLCompute, voxelNumbers, voxelNumbers, voxelNumbers, CCL_ROWCOLSCAN_kIndex);

        // @ Apply CCL_ROWSCAN_kIndex
        CCLCompute.SetBuffer(CCL_ROWSCAN_kIndex, "bit", bitCCLBuff);
        CCLCompute.SetBuffer(CCL_ROWSCAN_kIndex, "classifications", classCCLBuff);
        CCLCompute.SetBuffer(CCL_ROWSCAN_kIndex, "equivalents", eqCCLBuff);
        ComputeHelper.Dispatch(CCLCompute, voxelNumbers, voxelNumbers, voxelNumbers, CCL_ROWSCAN_kIndex);

        // @ Apply CCL_REFINE
        CCLCompute.SetBuffer(CCL_REFINE_kIndex, "classifications", classCCLBuff);
        CCLCompute.SetBuffer(CCL_REFINE_kIndex, "equivalents", eqCCLBuff);
        ComputeHelper.Dispatch(CCLCompute, voxelNumbers, voxelNumbers, voxelNumbers, CCL_REFINE_kIndex);
        foreach (MoodyChunk chunk in chunks)
            GenerateChunk(chunk);
        TrySplitChunksPerLabel();
        */
        return null;

    }
    public Bounds GetBoundariesOfGroupOfChunk(List<MoodyChunk> chks)
    {

        Bounds bounds = new Bounds();
        List<MeshCollider> colliders = new List<MeshCollider>();
        foreach ( MoodyChunk chunk in chks)
        {
            colliders.Add(chunk.gameObject.GetComponent<MeshCollider>());
        }
        if (colliders.Count > 0)
        {
            // Copy first collider bounds ... 
            MeshCollider c = colliders[0];
            // Because collider is a pointer and we will encapsulate. We should copy and not pass pointer. 
            bounds.center = new Vector3(c.bounds.center.x, c.bounds.center.y, c.bounds.center.z);
            bounds.extents = new Vector3(c.bounds.extents.x, c.bounds.extents.y, c.bounds.extents.z);
            bounds.min = new Vector3(c.bounds.min.x, c.bounds.min.y, c.bounds.min.z);
            bounds.max = new Vector3(c.bounds.max.x, c.bounds.max.y, c.bounds.max.z);
            bounds.size = new Vector3(c.bounds.size.x, c.bounds.size.y, c.bounds.size.z);
            // Encapsulate
            foreach (Collider coll in colliders)
            {
                bounds.Encapsulate(coll.bounds);
            }
        }
        return bounds;
    }
    public bool TrySplitChunksPerLabel()
    {
        Dictionary<int, List<MoodyChunk>> spls = new Dictionary<int, List<MoodyChunk>>();
        foreach ( MoodyChunk chunk in chunks)
        {
            if (chunk.labelSum > 0)
            {
                if (!spls.ContainsKey(chunk.labelSum))
                {
                    spls.Add(chunk.labelSum, new List<MoodyChunk>() { chunk });
                }
                else
                {
                    spls[chunk.labelSum].Add(chunk);
                }
            }
        }

        //@ Do refinement
        /*
            We have set up a dictionnary of label containing all of its chunk. 
            We should first find which label will keep as moodulable. 
            For this We need to be sure there are at least two big labels. the first one of the two bigs will be this moodulable. so no need 
            
                        
            We should first detach small label (if there are too small there will be called dust part) 
            We should create a function like Get boundaries of chunk group
                If a label all mesh boundaries is lower than a specific size -> just detach() :') and clear chunks.

         */
        float sizethreshold = 1f * reduction; // 1/10 de la matiere. (minimum volume) 
        Dictionary<int, List<MoodyChunk>> eligiable = new Dictionary<int, List<MoodyChunk>>();
        Dictionary<int, List<MoodyChunk>> dust = new Dictionary<int, List<MoodyChunk>>();
       
        foreach (KeyValuePair<int, List<MoodyChunk>> kvp in spls)
        {
            Bounds b = GetBoundariesOfGroupOfChunk(kvp.Value);
            Debug.Log("KVP size # "+ kvp.Key +" " + b.size);
            if ( b.size.x >= sizethreshold && b.size.y >= sizethreshold && b.size.z >= sizethreshold) 
            {
                eligiable.Add(kvp.Key, kvp.Value);
            }
            else
            {
                dust.Add(kvp.Key, kvp.Value);
            }
        }
        if (eligiable.Count < 2 )
            return false;

        // Create All eligiable new moodulable
        // Ok here is the problem. the first moodulable disappear (elligiable(0) ) ... We will just create new moodulable for each parts then destroy this moodulable...
        for (int i = 0; i < eligiable.Count; i++)
        {
            GameObject newMoodulable = new GameObject(this.gameObject.name + " _split_" + eligiable.ElementAt(i).Key);
            Debug.Log("Creating from label: " + eligiable.ElementAt(i).Key);
            List<MoodyChunk> chks = eligiable.ElementAt(i).Value;
            Moodulable eligiableMood = newMoodulable.AddComponent<Moodulable>();
            eligiableMood.CopySettingsFromOtherMoodulable(this);
            eligiableMood.InitFromChunks(this, chks);
            // used for debug but irrelevant.
            //newMoodulable.GetComponent<Rigidbody>().isKinematic = GetComponent<Rigidbody>().isKinematic;

            newMoodulable.transform.position = this.transform.position;
            newMoodulable.transform.rotation = this.transform.rotation;
            // Debug
            // Then clear all chunks in this moodulable
            //CleanChunks(chks); // but i feel there is weird thing with eligiable and others... // this will be not needed i guess

        }
        // Separate dust as new object
        foreach (KeyValuePair<int, List<MoodyChunk>> kvp in dust)
        {
            GameObject newHolder = new GameObject(this.gameObject.name + " _dust");
            
            foreach (MoodyChunk chunk in kvp.Value)
            {
                chunk.gameObject.layer = 0;
                chunk.gameObject.transform.parent = newHolder.transform;
            }
            newHolder.transform.position = this.transform.position;
            newHolder.transform.rotation = this.transform.rotation;
            newHolder.AddComponent<Rigidbody>();
        }
        // Do some performance here using moodulablemanager
        loaded = false;
        MoodulableManager.UnloadUnfocusedMoodulable();

        // Destroy this gameObject. Can conflit with Grabbable
        Delete(true, false);

        return true;
     
    }

    #endregion

    #region Texture3D Creation
    void Create3DColorTexture(ref RenderTexture texture, int size, string name)
    {
        if (texture == null || !texture.IsCreated() || texture.width != size || texture.height != size || texture.volumeDepth != size)
        {
            if (texture != null)
            {
                texture.Release();
            }
            texture = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBHalf);
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            texture.volumeDepth = size;
            texture.Create();
        }
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        texture.name = name;
        

    }
    void Create3DTexture(ref RenderTexture texture, int size, string name)
    {
        //32-bit signed floating-point format that has a single 32-bit R component
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;

        if (texture == null || !texture.IsCreated() || texture.width != size || texture.height != size || texture.volumeDepth != size || texture.graphicsFormat != format)
        {
            if (texture != null)
            {
                texture.Release();
            }

            const int numBitsInDepthBuffer = 0;
            texture = new RenderTexture(size, size, numBitsInDepthBuffer);
            texture.graphicsFormat = format;
            texture.volumeDepth = size;
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;

            texture.Create();
        }
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        texture.name = name;
    }
    #endregion

    
}
