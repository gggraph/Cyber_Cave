using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class Paintable : MonoBehaviour
{
    [Header("Texture")]
    public int textureSize = 1024;
    public Material fillCrack;

    [Header("Color")]
    public Color initialColor = Color.white;
    public Color clearColor = Color.gray;

    [HideInInspector] public RenderTexture output;
    RenderTexture[] pingPongRts;
    Mesh mesh;

    [Header("Logic")]
    public bool InitAtStart = false;

    [Header("Initialisation")]
    // @ those are for auto texturing or load previously modified texture
    public Texture initialTexture;
    [HideInInspector] public string initialTexturePath = "";

    [Header("Debug")]
    public bool _TryPrint = false;


    private void Start()
    {
        if (!GetComponent<ObjectSaver>())
            gameObject.AddComponent<ObjectSaver>();
        if (InitAtStart)
            Init();
    }
    private void Update()
    {
        if (_TryPrint)
        {
            _TryPrint = false;
            PrintTextureThrough3DPlotter();
        }
    }
    public bool Init()
    {
        GetComponent<Renderer>().material = Resources.Load("Materials/PaintingExample", typeof(Material)) as Material;
        // Create the texture & materialpropertynlock but maybe i can overwrite on existing texture? 
        output = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBHalf);
        output.Create();

        ComputeEmptyCheckSumFile();
        if (initialTexturePath != null && initialTexturePath.Length > 0)
        {
            Debug.Log("Loading texture path!");
            if (File.Exists(initialTexturePath))
            {
                Debug.Log("Creating texture from file ! ");
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(initialTexturePath));
                initialTexture = tex;
            }
            else
                return false;
        }


        var r = GetComponent<Renderer>();
        var mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        mpb.SetTexture("_MainTex", output);
        r.SetPropertyBlock(mpb);
        // Create 2 render texture
        pingPongRts = new RenderTexture[2];
        for (var i = 0; i < 2; i++)
        {
            var outputRt = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBHalf);
            outputRt.Create();
            GL.Clear(true, true, initialColor);
            pingPongRts[i] = outputRt;
        }
        // Copy original texture
        if (initialTexture != null)
        {
            // Copy original texture to current one 
            Graphics.Blit(initialTexture, output);
            Graphics.CopyTexture(output, pingPongRts[0]);

            // we need to render the texture here ... 
            // This is completely hacky but well... we have no time left ...
            Invoke("ForceDraw", 0.2f);

        }

        mesh = GetComponent<MeshFilter>().mesh;
        // This script work with UV2. UV0 can be null

        Graphics.CopyTexture(pingPongRts[0], output);

        return true;

    }
    // @ this function is poorly coded. It will not work if there is no painer in scene... This is hacky.
    private void ForceDraw()
    {
        Painter pter = FindObjectOfType<Painter>();
        PainterBase ptbase = FindObjectOfType<PainterBase>();
        if (pter && ptbase)
            ptbase.ForcePaintNothing(pter); // wtf...
    }


    private void OnDestroy()
    {
        foreach (var rt in pingPongRts)
            rt.Release();
        output.Release();
    }

    public void Draw(Material drawingMat)
    {
        drawingMat.SetTexture("_MainTex", pingPongRts[0]);

        var currentActive = RenderTexture.active;
        RenderTexture.active = pingPongRts[1];
        GL.Clear(true, true, Color.clear);
        drawingMat.SetPass(0);
        Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);
        RenderTexture.active = currentActive;

        Swap(pingPongRts);
        if (fillCrack != null)
        {
            Graphics.Blit(pingPongRts[0], pingPongRts[1], fillCrack);
            Swap(pingPongRts);
        }

        Graphics.CopyTexture(pingPongRts[0], output);
    }

    public byte[] GetTextureAsPNGBinary()
    {
        var currentActive = RenderTexture.active;
        RenderTexture.active = output;
        Texture2D tex = new Texture2D(output.width, output.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, output.width, output.height), 0, 0);
        RenderTexture.active = currentActive;
        return tex.EncodeToPNG();
    }
    public bool PrintTextureThrough3DPlotter()
    {
        byte[] bytes = GetTextureAsPNGBinary();
        string filePath = Application.persistentDataPath + "/tex_" + Utilities.GetTimeStamp() + ".png";
        System.IO.File.WriteAllBytes(filePath, bytes);
        Debug.Log("Texture file created at " + filePath);
       
        List<byte> data = new List<byte>();
        data.Add(2); // header...
        // Share the file Path
        data.Add((byte)filePath.Length);
        foreach (char c in filePath.ToCharArray())
            data.Add((byte)c);
        Plotting.SendDataToTCPProgram(data.ToArray());
        return true;
    }
    // return checksum as hexstring
    public string SaveRenderTexture()
    {
        byte[] bytes = GetTextureAsPNGBinary();
        // make those values unique by adding name
        byte[] pngchksum = CryptoUtilities.ComputeSHA(bytes);
        string chksmstr = CryptoUtilities.SHAToHex(pngchksum); // convert the checksum to hex
        // Save to disk
        string path = P2SHARE.GetDirByType((byte)P2SHARE.CustomFileType.TextureFile) + chksmstr;
        if (IOManager.AddFileToChecksumLog(path))
        {
            // also delete privously loaded texture 
            if (initialTexturePath != path)
            {
                if (initialTexturePath != null && initialTexturePath.Length > 0)
                {

                    // @ Remove only if others moodulable do not use this texturePath && initialTexturePath is not empty checksum 

                    Paintable[] Paintables = FindObjectsOfType<Paintable>();
                    bool _delete = initialTexturePath == NullDataFilePath ? false : true;
                    foreach (Paintable p in Paintables)
                    {
                        if (p == this)
                            continue;
                        if (p.initialTexturePath == initialTexturePath)
                        {
                            _delete = false;
                            break;
                        }
                    }
                    if (_delete)
                    {
                        Debug.Log("Deleting " + initialTexturePath);
                        File.Delete(initialTexturePath);
                        IOManager.RemoveFileFromChecksumLog(initialTexturePath);
                    }

                }
                System.IO.File.WriteAllBytes(path, bytes);
            }

        }
        initialTexturePath = path;
        return chksmstr;

    }
    // @ Net better performance.
    private string NullDataFilePath = "";
    private void ComputeEmptyCheckSumFile()
    {
        byte[] bytes = GetTextureAsPNGBinary();
        byte[] checksum = CryptoUtilities.ComputeSHA(bytes);
        string chksmstr = CryptoUtilities.SHAToHex(checksum);
        string path = P2SHARE.GetDirByType((byte)P2SHARE.CustomFileType.TextureFile) + chksmstr;
        if (IOManager.AddFileToChecksumLog(path))
        {
            System.IO.File.WriteAllBytes(path, bytes);
        }
        NullDataFilePath = path;

    }

    void Swap<T>(T[] array)
    {
        var tmp = array[0];
        array[0] = array[1];
        array[1] = tmp;
    }
}
