using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SculptRotater : MonoBehaviour
{

    // Button system is bad. Rework it or bad syncing...

    // Start is called before the first frame update
    [Header("Setup")]
    public GameObject AxeCollider;
    public GameObject LeftButton;
    public GameObject RightButton;
    public float rotationSpeed = 0.5f;

    private GameObject MoodulableAttached;
    private Vector3 pos, fw, up;


    public int RotatingMode = 2; // 0 is anticlockwise, 1 is stopped, 2 is clockwise
    private bool InsideRightButton;
    private bool InsideLeftButton;

    AudioSource audio;
    void Start()
    {
        rotationSpeed = 0.5f;
        // Set up audio
        audio = SoundMap.PlayLoopFromAssets("moodrotater", AxeCollider.transform.position).GetComponent<AudioSource>();
        audio.spatialBlend = 1.0f;
        audio.minDistance = 0.1f;
        audio.maxDistance = 3f;
        audio.rolloffMode = AudioRolloffMode.Linear;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if (!AxeCollider || !LeftButton || !RightButton)
             return;

        TryAttach();
        TrySpin();
        SetHighlightingFromMode();
        
        if (RotatingMode == 0)
        {
            AxeCollider.transform.parent.Rotate(0, 0, rotationSpeed);
            if (!audio.isPlaying)
                audio.Play();
        }
        else if  (RotatingMode == 2)
        {
            AxeCollider.transform.parent.Rotate(0, 0, -rotationSpeed);
            if (!audio.isPlaying)
                audio.Play();
        }
        else if (RotatingMode == 1)
        {
            // Stop sound
            if (audio.isPlaying)
                audio.Pause();
        }

        if (MoodulableAttached)
        {
            Grabbable g = MoodulableAttached.GetComponent<Grabbable>();
            if (g._grabbed)//(g._grabbed && g._userisgrabber)
            {
                MoodulableAttached = null;
                //Sync releasing on rotater
                return;
            }
               
            GameObject Parent = AxeCollider.transform.parent.gameObject;
            var newpos = Parent.transform.TransformPoint(pos);
            var newfw = Parent.transform.TransformDirection(fw);
            var newup = Parent.transform.TransformDirection(up);
            var newrot = Quaternion.LookRotation(newfw, newup);
            MoodulableAttached.transform.position = newpos;
            MoodulableAttached.transform.rotation = newrot;
            
        }

    }
  
    void SetHighlightingFromMode()
    {

    }
    void TrySpin()
    {
       
        int result = ObjectUtilities.DoesAnyAvatarFingerInsideObject(LeftButton);
        if (result != -1)
        {
            if (!InsideLeftButton) // was not inside last frame
            {
                if (RotatingMode % 2 == 0) // if equal to 0 or 2 
                {
                    RotatingMode = 1;
                }
                else
                    RotatingMode = 0;

                // Sync 
                SyncRotatingMode();
            }
            // Vibrate & Sound
            if (result == 1)
                ControllerData.SetLeftTouchVibration(200, 2, 50);
            if (result == 0)
                ControllerData.SetRightTouchVibration(200, 2, 50);
            // end
            return;
        }
        else
        {
            InsideLeftButton = false;
        }
      
        result = ObjectUtilities.DoesAnyAvatarFingerInsideObject(RightButton);
        if (result != -1)
        {
            if (!InsideRightButton) // was not inside last frame
            {
                if (RotatingMode % 2 == 0) 
                {
                    RotatingMode = 1;
                }
                else
                    RotatingMode = 2;

                // Sync
                SyncRotatingMode();
            }
            // Vibrate & Sound
            if (result == 1)
                ControllerData.SetLeftTouchVibration(200, 2, 50);
            if (result == 0)
                ControllerData.SetRightTouchVibration(200, 2, 50);
        }
        else
        {
            InsideRightButton = false;
        }
    }

    void TryAttach()
    {
        if (!AxeCollider || MoodulableAttached)
            return ;
        Moodulable[] moods = FindObjectsOfType<Moodulable>();
        foreach (Moodulable m in moods)
        {
            Grabbable g = m.GetComponent<Grabbable>();
            if (!g)
                continue;
            if (g._grabbed)
                continue;
            // Get distance from axe
            float distance = Vector3.Distance(m.transform.position, AxeCollider.transform.position);
            float rad1 = m.boundsSize * m.reduction;
            if (distance > rad1)
                continue;

            // Detect if touching 
            Bounds moodBounds = m.GetBoundariesOfGroupOfChunk(m.chunks.ToList());
            if (AxeCollider.GetComponent<MeshCollider>().bounds.Intersects(moodBounds))
            {
               
                // Fake attach g.gameObject to axe gameObject.
                ObjectUtilities.DisablePhysicsOnObject(g.gameObject);

                // Set Moodulable is attached 
                MoodulableAttached = m.gameObject;
                //Offset vector
                GameObject Parent = AxeCollider.transform.parent.gameObject;
                pos = Parent.transform.InverseTransformPoint(MoodulableAttached.transform.position);
                fw = Parent.transform.InverseTransformDirection(MoodulableAttached.transform.forward);
                up = Parent.transform.InverseTransformDirection(MoodulableAttached.transform.up);

                break;
            }
        }
    }

    // @ Unused but can get some performance
    IEnumerator TryAttachCor() // Should be called every demisec
    {
        while ( true)
        {
            yield return new WaitForSeconds(1f);// should be less
            if (!AxeCollider || MoodulableAttached)
                continue;
            Moodulable[] moods = FindObjectsOfType<Moodulable>();
            foreach (Moodulable m in moods)
            {
                Grabbable g = m.GetComponent<Grabbable>();
                if (!g)
                    continue;
                /*
                if (!g._userisgrabber || !g._grabbed)
                    continue;
                */
                if (g._grabbed)
                    continue;
                // Get distance from axe
                float distance = Vector3.Distance(m.transform.position, AxeCollider.transform.position);
                float rad1 = m.boundsSize * m.reduction;
                if (distance > rad1)
                    continue;

                // Detect if touching 
                Bounds moodBounds = m.GetBoundariesOfGroupOfChunk(m.chunks.ToList());
                if (AxeCollider.GetComponent<MeshCollider>().bounds.Intersects(moodBounds))
                {
                    // Force DeGrab 
                    /*g.ForceReleasing();*/
                    // Create some vibration 

                    // Fake attach g.gameObject to axe gameObject.
                    ObjectUtilities.DisablePhysicsOnObject(g.gameObject);
                    
                    // Set Moodulable is attached 
                    MoodulableAttached = m.gameObject;
                    //Offset vector
                    GameObject Parent = AxeCollider.transform.parent.gameObject;
                    pos = Parent.transform.InverseTransformPoint(MoodulableAttached.transform.position);
                    fw = Parent.transform.InverseTransformDirection(MoodulableAttached.transform.forward);
                    up = Parent.transform.InverseTransformDirection(MoodulableAttached.transform.up);
                   
                    break;
                }
            }
        }
       
    }


    void SyncRotatingMode()
    {
        List<byte> data = new List<byte>();
        data.Add(75);
        data.Add((byte)this.gameObject.name.Length);
        foreach (char c in this.gameObject.name.ToCharArray())
        {
            data.Add((byte)c);
        }
        data.Add((byte)RotatingMode);
        NetUtilities.SendDataToAll(data.ToArray());
    }

    public static void OnMoodRotaterDataReceived(byte[] data)
    {
        int namelength = (int)data[1];
        char[] chars = new char[namelength];
        for (int i = 0; i < namelength; i++)
        {
            chars[i] = (char)data[i + 2];
        }
        GameObject vObj = GameObject.Find(new string(chars));
        if (!vObj)
            return;
        SculptRotater r = vObj.GetComponent<SculptRotater>();
        if (!r)
            return;
        int mode = (int)data[2 + namelength];
        r.RotatingMode = mode;
    }
}
