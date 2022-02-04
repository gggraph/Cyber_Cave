using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshModifier : MonoBehaviour
{
    public bool isSync;
    public Bounds maxBoundaries;
    public GameObject Head;
    public GameObject LeftHand;
    public GameObject RightHand;

    private void Start()
    {
        this.gameObject.AddComponent<TransformModifier>();
        this.gameObject.AddComponent<VerticesModifier>();
        this.gameObject.AddComponent<Paint>();
        maxBoundaries = ObjectUtilities.GetBoundsOfGroupOfMesh(this.gameObject);

        Head = GameObject.Find("CenterEyeAnchor");
        LeftHand = GameObject.Find("OVRlefthand");
        RightHand = GameObject.Find("OVRrighthand");
        StartCoroutine(SyncRoutine(11));
    }

    private IEnumerator SyncRoutine(int frameRate)
    {
        
        
        int fcounter = 0;
        Vector3 lpos = this.transform.position;
        Vector3 lrot = this.transform.eulerAngles;
        Vector3 lscl = this.transform.localScale;
        while ( true)
        {
            fcounter++;
            if (  fcounter == frameRate )
            {
                fcounter = 0;
                if (isSync)
                {
                    if (lpos != this.transform.position
                     || lrot != this.transform.eulerAngles
                     || lscl != this.transform.localScale
                     )
                    {
                        lpos = this.transform.position;
                        lrot = this.transform.eulerAngles;
                        lscl = this.transform.localScale;
                        // -> net update transform
                        //BinaryUtilities.SerializeTransform(this.transform);
                        List<byte> data = new List<byte>();
                        data.Add(2);
                        data.Add((byte)this.gameObject.name.Length);
                        foreach (char c in this.gameObject.name.ToCharArray())
                        {
                            data.Add((byte)c);
                        }
                        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.SerializeTransform(this.gameObject.transform));
                        NetUtilities.SendDataToAll(data.ToArray());
                    }
                }
                else
                {
                }
           
            }    
            yield return new WaitForEndOfFrame();
        }
    }

    public void RecalculateBoundaries() // sometimes it is needed
    {
        maxBoundaries = ObjectUtilities.GetBoundsOfGroupOfMesh(this.gameObject);
    }
    public void SetSyncing(bool value)
    {
        isSync = value;
    }


}
