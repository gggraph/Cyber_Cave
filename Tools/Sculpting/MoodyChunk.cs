using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodyChunk
{
    public GameObject gameObject;
    public Vector3Int id;
    public Vector3 centre;
    public float size;
    int numPointsPerAxis; // @ Prob unused

    public Mesh mesh;
    public MeshFilter filter;
    MeshRenderer renderer;
    MeshCollider collider;

    // Mesh processing
    Dictionary<Vector2Int, int> vertexIndexMap;// @ Dictionnary to reduce number of vertices if flatShading enabled. 
    List<Vector3> processedVertices;
    List<Vector3> processedNormals;
    List<int> processedTriangles;
	// @ adding 07.06.2022
	List<Color> processedColor;

	Dictionary<int, int> labelCount;
    public int labelSum = 0;


    public MoodyChunk(Vector3Int coord, Vector3 centre, float size, int numPointsPerAxis, GameObject meshHolder) 
    {
        this.id = coord;
        this.centre = centre;
        this.size = size;
        this.numPointsPerAxis = numPointsPerAxis;
        this.gameObject = meshHolder;

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        filter = meshHolder.AddComponent<MeshFilter>();
        renderer = meshHolder.AddComponent<MeshRenderer>();
		filter.mesh = mesh;
		collider = renderer.gameObject.AddComponent<MeshCollider>();

        vertexIndexMap = new Dictionary<Vector2Int, int>();
        labelCount = new Dictionary<int, int>();
        processedVertices = new List<Vector3>();
        processedNormals = new List<Vector3>();
        processedTriangles = new List<int>();
		//@ adding 07.06.2022 
		processedColor = new List<Color>();
		Physics.IgnoreLayerCollision(10, 11); // 6 is moodulable. 7 is mooduler.
		

	}

	public void CreateMesh(VertexData[] vertexData, int numVertices, bool useFlatShading)
	{

		vertexIndexMap.Clear();
		processedVertices.Clear();
		processedNormals.Clear();
		processedTriangles.Clear();
		// @adding 07.06.2022
		processedColor.Clear();

		int triangleIndex = 0;

		for (int i = 0; i < numVertices; i++)
		{
			VertexData data = vertexData[i];

			int v;
			if (!labelCount.TryGetValue(data.l, out v))
			{
				labelCount.Add(data.l, 1);
			}
			else
			{
				labelCount[data.l]++;
			}

			int sharedVertexIndex;
			if (!useFlatShading && vertexIndexMap.TryGetValue(new Vector2Int(data.idX, data.idY), out sharedVertexIndex))
			{
				processedTriangles.Add(sharedVertexIndex);
			}
			else
			{
				if (!useFlatShading)
				{
					vertexIndexMap.Add(new Vector2Int(data.idX, data.idY), triangleIndex);
				}
				processedVertices.Add(data.position);
				processedNormals.Add(data.normal);
				processedTriangles.Add(triangleIndex);
				// @adding 07.06.2022
				processedColor.Add(data.Color);
				triangleIndex++;
			}
		}
		int _cmlabel = 0;
		int mctr = 0;
		//Debug.Log("Debug label key");
		foreach (KeyValuePair<int, int> kvp in labelCount)
		{
			//Debug.Log("Key = " + kvp.Key + " Value =" +  kvp.Value);
			if (kvp.Value > mctr && kvp.Key != 0)
			{
				mctr = kvp.Value;
				_cmlabel = kvp.Key;

			}
			//Debug.Log("Label:" + kvp.Key + " Count:" + kvp.Value);
		}
		labelSum = _cmlabel;
		collider.sharedMesh = null;
		collider.convex = false;

		mesh.Clear();
		mesh.SetVertices(processedVertices);
		mesh.SetTriangles(processedTriangles, 0, true);

		
		if (useFlatShading)
		{
			mesh.RecalculateNormals();
		}
		else
		{
			mesh.SetNormals(processedNormals);
		}

		mesh.colors = processedColor.ToArray();
		collider.sharedMesh = mesh;

	}
	public void TryConvexGeometry()
    {
		
		if (!mesh)
			return;
		if (!collider)
			return;
		collider.convex = true;
	}

	public Moodulable IsChunkTouchingOtherMoodulable() 
	{
		return null;
		/*
		Moodulable[] moodulables = GameObject.FindObjectsOfType<Moodulable>();
		Moodulable myMoodulable = gameObject.transform.parent.gameObject.GetComponent<Moodulable>();
		foreach ( Moodulable m in moodulables)
        {
			if (m == myMoodulable)
				continue;

			// [1] check if m bounds intersect with my Moodulable ( distance function )
			float distance = Vector3.Distance(m.transform.position, myMoodulable.transform.position);
			// distance should be lower than myMoodulable.radius + m.radius
			float rad1 = myMoodulable.boundsSize * myMoodulable.reduction;
			float rad2 = m.boundsSize * m.reduction;
			if (distance >= rad1 + rad2)
				continue;

			// [2] check bounds intersecting
			foreach (MoodyChunk chunk in m.chunks)
			{
				if ((chunk.chunkBounds.Contains(chunkBounds.min) && chunk.chunkBounds.Contains(chunkBounds.max))
					|| chunk.chunkBounds.Intersects(chunkBounds))
				{
					return m;
				}
			}
		}
		return null;
		*/
	}

	public void SetMaterial(Material material)
    {
        renderer.material = material;
    }
}
