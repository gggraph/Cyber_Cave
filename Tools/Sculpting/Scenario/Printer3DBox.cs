using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class Printer3DBox : MonoBehaviour
{
    [Header("Setup")]
    public GameObject PrintingSpace;
    public GameObject Button;
    public GameObject TextObject;

    Moodulable MoodulableToPrint;


    private void Start()
    {
        if (!PrintingSpace || !Button || !TextObject)
            return;
        SetText("no sculpture found");
        StartCoroutine(DetectMoodulableToPrint());

    }
    void Update()
    {
        if (!PrintingSpace || !Button || !TextObject)
            return;

        TryPrint();

        
    }

    void SetText(string s)
    {
        TextObject.GetComponent<TextMeshPro>().text = s;
    }
    

    IEnumerator DetectMoodulableToPrint()
    {
        while ( true)
        {
            yield return new WaitForSeconds(1f);

            Moodulable[] moods = FindObjectsOfType<Moodulable>();
            List<Moodulable> insideMoodulables = new List<Moodulable>();
            foreach (Moodulable m in moods)
            {
                Grabbable g = m.GetComponent<Grabbable>();
                if (!g)
                    continue;
                if (g._grabbed)
                    continue;
                // Get distance from axe
                float distance = Vector3.Distance(m.transform.position, PrintingSpace.transform.position);
                float rad1 = m.boundsSize * m.reduction;
                if (distance > rad1)
                    continue;

                // Detect if touching 
                Bounds moodBounds = m.GetBoundariesOfGroupOfChunk(m.chunks.ToList());
                if (PrintingSpace.GetComponent<MeshCollider>().bounds.Intersects(moodBounds))
                {
                    insideMoodulables.Add(m);
                }
            }
            // If 3d machine not busy (but we canot know)
            if (insideMoodulables.Count > 1)
            {
                SetText("only one sculpture allowed");
            }
            else if (insideMoodulables.Count == 0)
            {
                MoodulableToPrint = null;
                SetText("no sculpture found");
            }
            else
            {
                MoodulableToPrint = insideMoodulables[0];
                SetText("ready to print");
            }
        }
        
    }

    bool InsideLastFrame = false;
    // @ Do a good button sys please 
    void TryPrint()
    {
        int result = ObjectUtilities.DoesAnyAvatarFingerInsideObject(Button);
        if (result != -1)
        {
            if (!InsideLastFrame)
            {
                InsideLastFrame = true;
                if (MoodulableToPrint)
                {
                    // Ask to print 
                    SoundMap.FastPlaySoundAtPosition("3dprintingcommand", PrintingSpace.transform.position);
                    // 80
                    List<byte> data = new List<byte>();
                    data.Add(80);
                    data.Add((byte)MoodulableToPrint.name.Length);
                    foreach (char c in MoodulableToPrint.name.ToCharArray())
                        data.Add((byte)c);
                    NetUtilities.SendDataToAll(data.ToArray());
                }

            }
           
        }
        else
            InsideLastFrame = false;
    }
}
