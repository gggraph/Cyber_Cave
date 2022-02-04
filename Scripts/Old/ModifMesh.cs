using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class ModifMesh : MonoBehaviour
{
    // Start is called before the first frame update
    OVRBone rightHandObj;
    OVRBone leftHandObj;
   // GameObject RIG;
    public Mesh mesh;
    public Vector3[] vertices;
    Bounds boundaries;

    public Coroutine ModThread;
    public Coroutine MovThread;

    public  List<GrabbedVertice> _grabbedVertices = new List<GrabbedVertice>();

    bool needOpti = false;

    public class GrabbedVertice 
    {
        public Vector3 Offset { get; set; }
        public uint VerticesIndex { get; set; }
        public bool _isLeftHand { get; set; }
        public GrabbedVertice(Vector3 offset, uint verticesIndex, bool isLeftHand) 
        {
            this.Offset = offset;
            this.VerticesIndex = verticesIndex;
            this._isLeftHand = isLeftHand;
        
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
        if (GameObject.Find("OVRlefthand").GetComponent<OVRSkeleton>().Bones.Count == 0)
            return;
        _grabbedVertices = new List<GrabbedVertice>(); 
        leftHandObj = GameObject.Find("OVRlefthand").GetComponent<OVRSkeleton>().Bones[10]; // corresponding to mid majeur bones when down
        rightHandObj = GameObject.Find("OVRrighthand").GetComponent<OVRSkeleton>().Bones[10];// corresponding to mid majeur bones when down
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        boundaries = GetComponent<MeshRenderer>().bounds;
        if ( vertices.Length > 100) {
            needOpti = true;
        }
       
    }

    public IEnumerator StartDeplacingFullMesh() // I guess this was a coroutine cause its tooks to much time 
    {
        if (boundaries.Contains(rightHandObj.Transform.position) || boundaries.Contains(leftHandObj.Transform.position) || !needOpti)
        {
            bool leftInside = false;
            bool rightInside = false; 
            for (uint i = 0; i < vertices.Length; i++)
            {
                Vector3 v = transform.TransformPoint(vertices[i]);
                float distA = Mathf.Abs(Vector3.Distance(v, rightHandObj.Transform.position));
                float distB = Mathf.Abs(Vector3.Distance(v, leftHandObj.Transform.position));
                if (distA < 0.15f) // treshold a definir
                {

                    rightInside = true;

                }
                if (distB < 0.15f) // treshold a definir
                {

                    leftInside = true;

                }
            }
            if ( leftInside || rightInside) 
            {
                try
                {
                    StopCoroutine(MovThread);
                }
                catch { }

                Vector3 offset = rightHandObj.Transform.position - this.transform.position;
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
            this.transform.position = rightHandObj.Transform.position - offset;
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
        BinaryUtilities.AddBytesToList(ref syncVertData, BinaryUtilities.SerializeTransform(this.transform));

        NetUtilities.SendDataToAll(syncVertData.ToArray());
    }
    // ------------------------------------------------> NEED TO AUTORISE LEFTHAND
    public IEnumerator GetVerticesNearHand( bool _isLeftHand) 
    {
        OVRBone HandBone; 
        if ( _isLeftHand)
        {
            HandBone = leftHandObj;
        }
        else
        {
            HandBone = rightHandObj;
        }

        //_grabbedVertices = new List<GrabbedVertice>(); // nop :) 
        if (boundaries.Contains(HandBone.Transform.position) || !needOpti)
        {

            for (uint i = 0; i < vertices.Length; i++)
            {
                Vector3 v = transform.TransformPoint(vertices[i]);
                float dist = Mathf.Abs(Vector3.Distance(v, HandBone.Transform.position));
                if (dist < 0.08f) // treshold a definir
                {
                    Vector3 offset = HandBone.Transform.position - v;
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
                        _grabbedVertices.Add(new GrabbedVertice(offset, i, _isLeftHand));
                    }
                   
               
                }
            }
        
        }

        if ( !modCo_isrunning) // only launch new coroutine if not running actually
        {
            try
            {
                StopCoroutine(ModThread); // stop the coroutine ????
                modCo_isrunning = false;
            }
            catch { }
            ModThread = StartCoroutine(UpdateGrabbedVerticesPosition()); // this is ok but 
        }
        
        yield return 0;
    }
    GameObject dbgtext;
    public void PrintInfo(string s)
    {
        if (dbgtext == null)
            dbgtext = GameObject.Find("DEBUG");

        dbgtext.GetComponent<UnityEngine.UI.Text>().text = s;
    }
    public int ucall = 0;
    public void StopGrabbingVertices(bool _isLeftHand) // will send info to other clients 
    {
        ucall++;
        // always called
        // delete all lefthand instance. if _grabbedVertices . count = 0 stop the coroutine

        // only released hand assign vertices
        List<GrabbedVertice> newgv = new List<GrabbedVertice>(); 
        foreach ( GrabbedVertice gv in _grabbedVertices)
        {
            if ( gv._isLeftHand != _isLeftHand)
            {
                newgv.Add(gv);
            }
        }
        
        _grabbedVertices = newgv;
        PrintInfo(ucall.ToString() + ":"+_grabbedVertices.Count.ToString() + " " + _isLeftHand.ToString());

        if ( _grabbedVertices.Count == 0)
        {
            _grabbedVertices = new List<GrabbedVertice>(); // not so needed
            try
            {
                StopCoroutine(ModThread);
                modCo_isrunning = false;
            }
            catch { }
        }
        SendVerticesUpdatedForSync();

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
            BinaryUtilities.AddBytesToList(ref syncVertData, BitConverter.GetBytes(g.VerticesIndex));
            BinaryUtilities.AddBytesToList(ref syncVertData, BitConverter.GetBytes(vertices[g.VerticesIndex].x));
            BinaryUtilities.AddBytesToList(ref syncVertData, BitConverter.GetBytes(vertices[g.VerticesIndex].y));
            BinaryUtilities.AddBytesToList(ref syncVertData, BitConverter.GetBytes(vertices[g.VerticesIndex].z));
        }
        NetUtilities.SendDataToAll(syncVertData.ToArray());

    }
    private bool modCo_isrunning = false;
    IEnumerator UpdateGrabbedVerticesPosition() 
    {
        modCo_isrunning = true;
        byte syncCounter = 0; 
        while ( true) 
        {
            foreach ( GrabbedVertice gv in _grabbedVertices) 
            {
                Vector3 nV; 
                if (gv._isLeftHand)
                {
                    nV = leftHandObj.Transform.position - gv.Offset;
                }
                else
                {
                    nV = rightHandObj.Transform.position - gv.Offset;

                }
                //Vector3 nV = rightHandObj.Transform.position - gv.Offset; // + or -??
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

  





}
