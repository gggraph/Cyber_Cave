using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasCreator : MonoBehaviour
{
    [Header("Canvas Parameter")]
    public int TextureSize = 1024;
    public Texture InitialTexture;
    public Vector3 CanvasDimension = new Vector3(0.5f, 0.75231f, 0.03f);
    public Vector3 CanvasEuler = new Vector3(0, 0, 0);

    [Header("Creator Setup")]
    public GameObject Button;
    public GameObject RootCreation;
    public int SecondLimit = 15;
    [HideInInspector] public int CurrentClock = 15;

    [Header("Debug")]
    public bool ForceCreate;

    void Start()
    {
        StartCoroutine(SimpleClock());
    }
    IEnumerator SimpleClock()
    {
        while (true)
        {
            CurrentClock++;
            yield return new WaitForSeconds(1);
        }

    }


    // Update is called once per frame
    void Update()
    {
        if (!Button || !RootCreation)
            return;
        if (ForceCreate)
        {
            ForceCreate = false;
            CreateCanvasUsingSettings(CryptoUtilities.GetUniqueName().ToString(), false);
        }
        TryCreate();
    }
    private bool InsideLastFrame;
    void TryCreate()
    {
        int result = ObjectUtilities.DoesAnyAvatarFingerInsideObject(Button);
        if (result != -1)
        {
            if (!InsideLastFrame)
            {
                InsideLastFrame = true;
                if (CurrentClock < SecondLimit)
                {
                    SoundMap.FastPlaySoundAtPosition("waitcooling", Button.transform.position);
                    return;
                }
                CreateCanvasUsingSettings(CryptoUtilities.GetUniqueName().ToString(), true);
                // vibrate
            }

        }
        else
            InsideLastFrame = false;
    }

    public bool LoadCanvasUsingSettings(GameObject g, string texturePath)
    {
        // Craft up cube 
        g.AddComponent<MeshRenderer>();
        g.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        g.AddComponent<MeshCollider>().convex = true;
        g.AddComponent<Grabbable>();
        g.AddComponent<Rigidbody>();
        Paintable pt = g.AddComponent<Paintable>();
        pt.initialTexturePath = texturePath;
        pt.textureSize = TextureSize;

        // Update Paintables
        PainterBase ptb = FindObjectOfType<PainterBase>();
        if (ptb)
            ptb.RefindPaintables();


        return pt.Init();
    }

    void CreateCanvasUsingSettings(string name, bool _Sync = false)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(g.GetComponent<BoxCollider>());
        g.AddComponent<MeshCollider>().convex = true;
        g.AddComponent<Grabbable>();
        g.AddComponent<Rigidbody>();
        // add object saver here ?
        g.name = name;
        g.transform.position = RootCreation.transform.position;
        g.transform.localScale = CanvasDimension;
        g.transform.eulerAngles = CanvasEuler;
        Paintable pt = g.AddComponent<Paintable>();
        pt.initialTexture = InitialTexture;
        pt.textureSize = TextureSize;
        pt.InitAtStart = true;

        // Update Paintables
        PainterBase ptb = FindObjectOfType<PainterBase>();
        if (ptb)
            ptb.RefindPaintables();
        
        // Do some sound

        if (_Sync) 
        {
            List<byte> data = new List<byte>();
            data.Add(71);
            data.AddRange(CryptoUtilities.HexToSHA(name)); // 32 b
            NetUtilities.SendDataToAll(data.ToArray());
        }
    }
   

    public static void OnCanvasCreationReceived(byte[] data)
    {
        CanvasCreator cc = FindObjectOfType<CanvasCreator>();
        if (!cc)
            return;
        byte[] chksum = new byte[32];
        for (int i = 1; i < 33; i++)
        {
            chksum[i - 1] = data[i];
        }
        string name = CryptoUtilities.SHAToHex(chksum);
        cc.CreateCanvasUsingSettings(name, false);

    }
}
