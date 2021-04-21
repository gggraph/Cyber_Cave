using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class ModifMesh : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject rightHandObj;
    GameObject leftHandObj;
   // GameObject RIG;
    public Mesh mesh;
    public Vector3[] vertices;
    Bounds boundaries;

    public Coroutine ModThread;
    public Coroutine MovThread;

    List<GrabbedVertice> _grabbedVertices = new List<GrabbedVertice>();

    bool needOpti = false;

    class GrabbedVertice 
    {
        public Vector3 Offset { get; set; }
        public uint VerticesIndex { get; set; }
        public GrabbedVertice(Vector3 offset, uint verticesIndex) 
        {
            this.Offset = offset;
            this.VerticesIndex = verticesIndex;
        
        }
    
    }

    private void Start()
    {
        if (vertices == null)
        {
            ForceStart();
        }
    }
    public void ForceStart()
    {
       
        leftHandObj = GameObject.Find("OVRlefthand");
        rightHandObj = GameObject.Find("OVRrighthand");
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        boundaries = GetComponent<MeshRenderer>().bounds;
        if ( vertices.Length > 100) {
            needOpti = true;
        }
       
    }

    public IEnumerator StartDeplacingFullMesh() 
    {
        if (boundaries.Contains(rightHandObj.transform.position) || !needOpti)
        {
            bool leftInside = false;
            bool rightInside = false; 
            for (uint i = 0; i < vertices.Length; i++)
            {
                Vector3 v = transform.TransformPoint(vertices[i]);
                float distA = Mathf.Abs(Vector3.Distance(v, rightHandObj.transform.position));
                float distB = Mathf.Abs(Vector3.Distance(v, leftHandObj.transform.position));
                if (distA < 0.15f) // treshold a definir
                {

                    rightInside = true;

                }
                if (distB < 0.15f) // treshold a definir
                {

                    leftInside = true;

                }
            }
            if ( leftInside &&  rightInside) 
            {
                try
                {
                    StopCoroutine(MovThread);
                }
                catch { }

                Vector3 offset = rightHandObj.transform.position - this.transform.position;
                MovThread = StartCoroutine(MoveEntireMesh(offset));
            }

        }

        yield return 0; 
    }

    public void StopMovingEntireMesh() 
    {
        try
        {
            StopCoroutine(MovThread);
        }
        catch { }
        SendMeshDeplacingForSync();
    }

    public IEnumerator MoveEntireMesh(Vector3 offset) 
    {
        // need to also update the rot 
        byte syncCounter = 0;
        while (true)
        {
            this.transform.position = rightHandObj.transform.position - offset;
            yield return new WaitForSeconds(0.1f);
            syncCounter++;
            if (syncCounter == 4)
            {
                syncCounter = 0;
                SendMeshDeplacingForSync();
                // SendVerticesUpdatedForSync();
            }
        }
        yield return 0;
    }

    public void SendMeshDeplacingForSync() 
    {
        List<byte> syncVertData = new List<byte>();
        // add the header
        syncVertData.Add(2);

        // add the go gameobject name ? // max 32 octets
        syncVertData.Add((byte)this.name.ToCharArray().Length);
        for (int i = 0; i < this.name.ToCharArray().Length; i++)
        {
            syncVertData.Add((byte)this.name.ToCharArray()[i]);
        }
        syncVertData = AddBytesToList(syncVertData, BitConverter.GetBytes(this.transform.position.x));
        syncVertData = AddBytesToList(syncVertData, BitConverter.GetBytes(this.transform.position.y));
        syncVertData = AddBytesToList(syncVertData, BitConverter.GetBytes(this.transform.position.z));
        syncVertData = AddBytesToList(syncVertData, BitConverter.GetBytes(this.transform.localEulerAngles.x));
        syncVertData = AddBytesToList(syncVertData, BitConverter.GetBytes(this.transform.localEulerAngles.y));
        syncVertData = AddBytesToList(syncVertData, BitConverter.GetBytes(this.transform.localEulerAngles.z));

        GameObject[] allAvatar = GameObject.FindGameObjectsWithTag("Avatar");
        foreach (GameObject go in allAvatar)
        {
            if (go.GetComponent<DataReceiver>()._isMine)
            {
                go.GetComponent<DataReceiver>().SendData(ListToByteArray(syncVertData));
                return;
            }
        }
    }
    // 
    public IEnumerator GetVerticesNearHand() 
    {
        _grabbedVertices = new List<GrabbedVertice>();
        if (boundaries.Contains(rightHandObj.transform.position) || !needOpti)
        {

            for (uint i = 0; i < vertices.Length; i++)
            {
                Vector3 v = transform.TransformPoint(vertices[i]);
                float dist = Mathf.Abs(Vector3.Distance(v, rightHandObj.transform.position));
                if (dist < 0.08f) // treshold a definir
                {
                    Vector3 offset = rightHandObj.transform.position - v;
                    bool iscontained = false;
                    foreach ( GrabbedVertice g in _grabbedVertices) 
                    {
                        if ( g.VerticesIndex == i) 
                        {
                            iscontained = true;
                        }
                    }
                    if ( !iscontained) 
                    {
                        _grabbedVertices.Add(new GrabbedVertice(offset, i));
                    }
                   
               
                }
            }
        
        }
        try
        {
            StopCoroutine(ModThread);
        }
        catch { }
        ModThread = StartCoroutine(UpdateGrabbedVerticesPosition());
        yield return 0;
    }
    public void SendVerticesUpdatedForSync() 
    {

        // -_-_-_-_-_-_-_-_ START SYNC ALL THE VERTICES MOVED -_-_-_-_-_-_-_-_

        if (_grabbedVertices.Count == 0)
            return;

        List<byte> syncVertData = new List<byte>();
        /*
            -_-_-_-_-_-_-_-data packet structure-_-_-_-_-_-_
                                header
                                taille du char arr du nom de go
                                nom du gameobject (this)
                                
                                index du vertices
                                pos x
                                pos y 
                                pos z
                                
         */
        // add the header
        syncVertData.Add(1);

        // add the go gameobject name ? // max 32 octets
        syncVertData.Add((byte)this.name.ToCharArray().Length);
        for (int i = 0; i < this.name.ToCharArray().Length; i++)
        {
            syncVertData.Add((byte)this.name.ToCharArray()[i]);
        }

        foreach (GrabbedVertice g in _grabbedVertices)
        {
            // vertices index
            syncVertData = AddBytesToList(syncVertData, BitConverter.GetBytes(g.VerticesIndex));
            syncVertData = AddBytesToList(syncVertData, BitConverter.GetBytes(vertices[g.VerticesIndex].x));
            syncVertData = AddBytesToList(syncVertData, BitConverter.GetBytes(vertices[g.VerticesIndex].y));
            syncVertData = AddBytesToList(syncVertData, BitConverter.GetBytes(vertices[g.VerticesIndex].z));
        }

        GameObject[] allAvatar = GameObject.FindGameObjectsWithTag("Avatar");
        foreach (GameObject go in allAvatar)
        {
            if (go.GetComponent<DataReceiver>()._isMine)
            {
                go.GetComponent<DataReceiver>().SendData(ListToByteArray(syncVertData));
                return;
            }
        }

    }
    public void StopGrabbingVertices() // will send info to other clients 
    {
        try
        {
            StopCoroutine(ModThread);
        }
        catch { }

        SendVerticesUpdatedForSync();

    }
    IEnumerator UpdateGrabbedVerticesPosition() 
    {
        byte syncCounter = 0; 
        while ( true) 
        {
            foreach ( GrabbedVertice gv in _grabbedVertices) 
            {
                Vector3 nV = rightHandObj.transform.position - gv.Offset; // + or -??
                vertices[gv.VerticesIndex] = transform.InverseTransformPoint(nV); 
            }
            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            yield return new WaitForSeconds(0.1f);
            syncCounter++; 
            if ( syncCounter == 8) 
            {
                syncCounter = 0;
                SendVerticesUpdatedForSync();
            }
        }
        yield return 0;
    }
   
 
    void GetCenterOfMesh() 
    {
        Vector3 center = GetComponent<MeshRenderer>().bounds.center;
        
        float lowestX = float.MaxValue;
        float highestX = float.MinValue;
        float lowestY = float.MaxValue;
        float highestY = float.MinValue;
        float lowestZ = float.MaxValue;
        float highestZ = float.MinValue;
        for (var i = 0; i < GetComponent<MeshFilter>().mesh.vertices.Length; i++)
        {
            Vector3 v = GetComponent<MeshFilter>().mesh.vertices[i];
            if (v.x < lowestX) lowestX = v.x;
            if (v.y < lowestY) lowestY = v.y;
            if (v.z < lowestZ) lowestZ = v.z;
            if (v.x > highestX) highestX = v.x;
            if (v.y > highestY) highestY = v.y;
            if (v.z> highestZ) highestZ = v.z;

        }
        Vector3 p1 = new Vector3(lowestX,lowestY,highestZ);
        Vector3 p2 = new Vector3(highestX, lowestY, highestZ);
        Vector3 p3 = new Vector3(lowestX, lowestY, lowestZ);
        Vector3 p4 = new Vector3(highestX, lowestY, lowestZ);
        Vector3 p5 = new Vector3(lowestX, highestY, highestZ);
        Vector3 p6 = new Vector3(highestX, highestY, highestZ);
        Vector3 p7 = new Vector3(lowestX, highestY, lowestZ);
        Vector3 p8 = new Vector3(highestX, highestY, lowestZ);

      
    }

    //  Mathilde
    // raw merge of 2 meshes  ( just append second gameobject vertices to the first one ) 
    public void MergeMesh(GameObject other)
    {
        // return if other go has no mesh
        if (!other.GetComponent<MeshFilter>())
            return;

        // prepare
        Vector3[] otherVerts = other.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] newVerts = new Vector3[vertices.Length + otherVerts.Length]; 

        // concatenate
        for (int i = 0; i < vertices.Length; i++)
            newVerts[i] = vertices[i];
        
        for (int i = vertices.Length; i < vertices.Length + otherVerts.Length; i++)
            newVerts[i] = otherVerts[i - vertices.Length];
        
        
        // apply
        mesh.vertices = newVerts;
        mesh.RecalculateBounds();

        //delete other gameobject because now its merged... 
        Destroy(other.gameObject);

    }
    void Update()
    {
      
      
    }

    void SetVerticePosition(Vector3 nPos, int verticeindex) 
    {
        vertices[verticeindex] = nPos;

    }
    void DeformRandom(float rX, float rY, float rZ, int verticeindex)
    {
        Vector3 offset = new Vector3((float)UnityEngine.Random.Range(0f, rX), (float)UnityEngine.Random.Range(0f, rY), (float)UnityEngine.Random.Range(0f, rZ));
        vertices[verticeindex] += offset;
    }

    void ReassignVertices() 
    {
        // assign the local vertices array into the vertices array of the Mesh.
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        Debug.Log("reassign called");
    }

    public static List<byte> AddBytesToList(List<byte> list, byte[] bytes)
    {
        foreach (byte b in bytes) { list.Add(b); }
        return list;
    }
    public static byte[] ListToByteArray(List<byte> list)
    {
        byte[] result = new byte[list.Count];
        for (int i = 0; i < list.Count; i++) { result[i] = list[i]; }
        return result;
    }





}
