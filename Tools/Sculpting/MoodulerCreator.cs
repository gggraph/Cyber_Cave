using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodulerCreator : MonoBehaviour
{
  
    [Header("Moodulable Intialisation")]
    public Material MaterialForMoodulable;
    public int     ChunkNumber = 6;
    public int     VoxelPerAxis = 10;
    public bool    useFlatShading = false;
    public float   scale = 0.1f;
    public bool    CanMerge = false;
    public int     TimeBeforeMerging = 3;
    public bool    useSmoothStep = true;
    public bool    enableCCLComputing = true;

    [Header("Moodulable Creation")]
    public bool FreezePhysicsAtCreation;

    [Header("Trigger positions")]
    public GameObject CreationRoot;
    public GameObject CubeButton;
    public GameObject SphereButton;

    private bool insideCubeButton;
    private bool insideSphereButton;

    [Header("Limitations in time")]
    public int SecondLimit = 15;
    [HideInInspector] public int CurrentClock = 15;
    
    [Header("Debug")]
    public bool ForceSphere = false;
    public bool ForceCube = false;

    void Start()
    {
        StartCoroutine(SimpleClock());
    }
    IEnumerator SimpleClock()
    {
        while ( true)
        {
            CurrentClock++;
            yield return new WaitForSeconds(1);
        }

    }
    private void Update()
    {
        if (ForceSphere)
        {
            ForceSphere = false;
            CreateMoodulableUsingSettings("Sphere", CryptoUtilities.GetUniqueName().ToString(), false);
            return;
        }
        if (ForceCube)
        {
            ForceCube = false;
            CreateMoodulableUsingSettings("Cube", CryptoUtilities.GetUniqueName().ToString(), false);
            return;
        }
        TryCreate();

    }

    void TryCreate()
    {
        int result = ObjectUtilities.DoesAnyAvatarFingerInsideObject(SphereButton);
        if (result != -1 )
        {
            if (!insideSphereButton)
            {
                insideSphereButton = true;
                //Vibrate & Sound
                if (result == 1)
                    ControllerData.SetLeftTouchVibration(200, 2, 50);
                if (result == 0)
                    ControllerData.SetRightTouchVibration(200, 2, 50);
                if (CurrentClock < SecondLimit)
                {
                    SoundMap.FastPlaySoundAtPosition("waitcooling", SphereButton.transform.position);
                    return;
                }
                //Creation
                Debug.Log("Creating one moodulable!");
                CreateMoodulableUsingSettings("Sphere", CryptoUtilities.GetUniqueName().ToString(), true);
                return;
            }
        }
        else
            insideSphereButton = false;
        result = ObjectUtilities.DoesAnyAvatarFingerInsideObject(CubeButton);
        if (result != -1)
        {
            if (!insideCubeButton)
            {
                insideCubeButton = true;
                //Vibrate & Sound
                if (result == 1)
                    ControllerData.SetLeftTouchVibration(200, 2, 50);
                if (result == 0)
                    ControllerData.SetRightTouchVibration(200, 2, 50);

                if (CurrentClock < SecondLimit)
                {
                    SoundMap.FastPlaySoundAtPosition("waitcooling", CubeButton.transform.position);
                    return;
                }
                //Creation
                Debug.Log("Creating one moodulable!");
                CreateMoodulableUsingSettings("Cube", CryptoUtilities.GetUniqueName().ToString(), true);
                return;
            }
        }
        else
            insideCubeButton = false;
    }
    // @ create a moodulable
    void ApplySettingsToMoodulable(Moodulable m)
    {
        
        m.numChunks = ChunkNumber;
        m.numPointsPerAxis = VoxelPerAxis;
        m.useFlatShading = useFlatShading;
        m.reduction = scale;
        m.material = MaterialForMoodulable;
        m.EnableMerging = CanMerge;
        m.TimeHoldBeforeMerging = TimeBeforeMerging;
        m.SmoothStepColor = useSmoothStep;
        m.EnableCCLComputing = enableCCLComputing;

    }
    void CreateMoodulableUsingSettings(string shapeName, string moodulableName, bool _Sync = false)
    {
        CurrentClock = 0;

        if (!CreationRoot)
            return;
        SoundMap.FastPlaySoundAtPosition("Onnewmood", CreationRoot.transform.position);
        GameObject m1 = new GameObject();
        m1.name = moodulableName;
        Moodulable mood1 = m1.AddComponent<Moodulable>();
        ApplySettingsToMoodulable(mood1);
        mood1.InitFromDefault(shapeName);
        mood1.transform.position = CreationRoot.transform.position;
        if (FreezePhysicsAtCreation)
            mood1.gameObject.GetComponent<Rigidbody>().isKinematic = true;

        if ( _Sync)
        {
            List<byte> data = new List<byte>();
            data.Add(26);
            if (shapeName.ToLower() == "sphere")
                data.Add(1);
            else
                data.Add(0);
            data.AddRange(CryptoUtilities.HexToSHA(m1.name)); // 32 b
            NetUtilities.SendDataToAll(data.ToArray());
        }
    }
    public  bool LoadMoodulableUsingSettings(GameObject MoodulableObject, string filePath)
    {
        CurrentClock = 0;
        Moodulable mood1 = MoodulableObject.AddComponent<Moodulable>();
        mood1.initialSculptPath = filePath;
        ApplySettingsToMoodulable(mood1);
        return mood1.InitFromFile();
    }
    
    public static void OnMoodulableCreated(byte[] data)
    {
        MoodulerCreator mc = FindObjectOfType<MoodulerCreator>();
        if (!mc)
            return;
        byte shapeID = data[1]; // not used ...
        byte[] chksum = new byte[32];
        for (int i = 2; i < 34; i++)
        {
            chksum[i - 2] = data[i];
        }
        string name = CryptoUtilities.SHAToHex(chksum);
        byte shapeId = data[1];
        if (shapeId == 1 )
            mc.CreateMoodulableUsingSettings("sphere", name, false);
        else
            mc.CreateMoodulableUsingSettings("cube", name, false);

    }
    
    
}
