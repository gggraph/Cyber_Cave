// Author: Mathias Soeholm
// Date: 05/10/2016
// No license, do whatever you want with this script
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEngine.Serialization;
/*
[ExecuteInEditMode]
*/
public class TubeRenderer : MonoBehaviour
{
	/*
	[SerializeField] Vector3[] _positions;
	[SerializeField] int _sides;
	[SerializeField] float _radiusOne;
	[SerializeField] float _radiusTwo;
	[SerializeField] bool _useWorldSpace = true;
	[SerializeField] bool _useTwoRadii = false;
	*/
	public Vector3[] _positions;
	public Vector3[] _wPositions;
	public int _sides;
	public float _radiusOne;
	public float _radiusTwo;
	public bool _useWorldSpace = true;
	public bool _useTwoRadii = false;

	private Vector3[] _vertices;
	private Mesh _mesh;
	private MeshFilter _meshFilter;
	private MeshRenderer _meshRenderer;

	public Material material
	{
		get { return _meshRenderer.material; }
		set { _meshRenderer.material = value; }
	}

	void Awake()
	{
		_meshFilter = GetComponent<MeshFilter>();
		if (_meshFilter == null)
		{
			_meshFilter = gameObject.AddComponent<MeshFilter>();
		}

		_meshRenderer = GetComponent<MeshRenderer>();
		if (_meshRenderer == null)
		{
			_meshRenderer = gameObject.AddComponent<MeshRenderer>();
		}

		_mesh = new Mesh();
		_meshFilter.mesh = _mesh;
	}

	private void OnEnable()
	{
		_meshRenderer.enabled = true;
	}

	private void OnDisable()
	{
		_meshRenderer.enabled = false;
	}

	void Update() // Oh ok it updates the mesh every frames. 
	{
		//GenerateMesh();
	}

	private void OnValidate()
	{
		_sides = Mathf.Max(3, _sides);
	}

	// - GET STUFF
	
	public int GetPositionsCount()
    {
		if (_positions == null)
			return 0;
		return _positions.Length;
    }

	
	public void SetSidesNumber(int number)
    {
		_sides = number;
    }
	public void SetRadius1(float number)
	{
		_radiusOne = number;
	}
	public void SetMaterial(Material m)
	{
		material = m;
	}

	// _positions should be local and not depends of current gameObject world position and rotation

	public Vector3[] GetPositions()
	{
		return _positions;
	}
	public Vector3 GetPosition(int index)
	{
		if (_positions.Length <= index)
			return new Vector3();

		return _positions[index];
	}
	public Vector3 GetPositionAsWorldCoordinate(int index)
    {
		return ApplyTransformToVector(_positions[index]);

	}
	public Vector3[] GetPositionsAsWorldCoordinates()
    {
		Vector3[] worldPositions = new Vector3[_positions.Length];
		for (int i = 0; i < _positions.Length; i++)
		{
			worldPositions[i] = ApplyTransformToVector(_positions[i]);

		}
		return worldPositions;
	}
	public void AddNewPositionFromWoorldCoordinate(Vector3 coord)
	{
		if ( _positions == null)
        {
			_positions = new Vector3[1] { UnapplyTransformToVector(coord) };
        }
        else
        {
			List<Vector3> lps = _positions.ToList();
			lps.Add(UnapplyTransformToVector(coord));
			_positions = lps.ToArray();
			_wPositions = GetPositionsAsWorldCoordinates();
		}
		
		GenerateMesh();
	}

	public Vector3 UnapplyTransformToVector(Vector3 p)
    {
		Vector3 r = new Vector3(p.x, p.y, p.z);
		r -= transform.position;
		r = Quaternion.Euler(-transform.rotation.x, -transform.rotation.y, -transform.rotation.z) * r;
		return r;
	}
	public Vector3 ApplyTransformToVector(Vector3 p)
	{
		Vector3 r = new Vector3(p.x, p.y, p.z);
		r += transform.position;
		r = Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z) * r;
		return r;
	}


	private void GenerateMesh()
	{
		if (_mesh == null || _positions == null || _positions.Length <= 1)
		{
			_mesh = new Mesh();
			return;
		}

		var verticesLength = _sides * _positions.Length;
		if (_vertices == null || _vertices.Length != verticesLength)
		{
			_vertices = new Vector3[verticesLength];

			var indices = GenerateIndices();
			var uvs = GenerateUVs();

			if (verticesLength > _mesh.vertexCount)
			{
				_mesh.vertices = _vertices;
				_mesh.triangles = indices;
				_mesh.uv = uvs;
			}
			else
			{
				_mesh.triangles = indices;
				_mesh.vertices = _vertices;
				_mesh.uv = uvs;
			}
		}

		var currentVertIndex = 0;

		for (int i = 0; i < _positions.Length; i++)
		{
			var circle = CalculateCircle(i);
			foreach (var vertex in circle)
			{
				_vertices[currentVertIndex++] = _useWorldSpace ? transform.InverseTransformPoint(vertex) : vertex;
			}
		}

		_mesh.vertices = _vertices;
		_mesh.RecalculateNormals();
		_mesh.RecalculateBounds();

		_meshFilter.mesh = _mesh;
	}

	private Vector2[] GenerateUVs()
	{
		var uvs = new Vector2[_positions.Length * _sides];

		for (int segment = 0; segment < _positions.Length; segment++)
		{
			for (int side = 0; side < _sides; side++)
			{
				var vertIndex = (segment * _sides + side);
				var u = side / (_sides - 1f);
				var v = segment / (_positions.Length - 1f);

				uvs[vertIndex] = new Vector2(u, v);
			}
		}

		return uvs;
	}

	private int[] GenerateIndices()
	{
		// Two triangles and 3 vertices
		var indices = new int[_positions.Length * _sides * 2 * 3];

		var currentIndicesIndex = 0;
		for (int segment = 1; segment < _positions.Length; segment++)
		{
			for (int side = 0; side < _sides; side++)
			{
				var vertIndex = (segment * _sides + side);
				var prevVertIndex = vertIndex - _sides;

				// Triangle one
				indices[currentIndicesIndex++] = prevVertIndex;
				indices[currentIndicesIndex++] = (side == _sides - 1) ? (vertIndex - (_sides - 1)) : (vertIndex + 1);
				indices[currentIndicesIndex++] = vertIndex;

				// Triangle two
				indices[currentIndicesIndex++] = (side == _sides - 1) ? (prevVertIndex - (_sides - 1)) : (prevVertIndex + 1);
				indices[currentIndicesIndex++] = (side == _sides - 1) ? (vertIndex - (_sides - 1)) : (vertIndex + 1);
				indices[currentIndicesIndex++] = prevVertIndex;
			}
		}

		return indices;
	}

	private Vector3[] CalculateCircle(int index)
	{
		var dirCount = 0;
		var forward = Vector3.zero;
		// If not first index
		if (index > 0)
		{
			forward += (_wPositions[index] - _wPositions[index - 1]).normalized;//(_positions[index] - _positions[index - 1]).normalized;
			dirCount++;
		}

		// If not last index
		if (index < _positions.Length - 1)
		{
			forward += (_wPositions[index + 1] - _wPositions[index]).normalized;//(_positions[index + 1] - _positions[index]).normalized;
			dirCount++;
		}

		// Forward is the average of the connecting edges directions
		forward = (forward / dirCount).normalized;
		var side = Vector3.Cross(forward, forward + new Vector3(.123564f, .34675f, .756892f)).normalized;
		var up = Vector3.Cross(forward, side).normalized;

		var circle = new Vector3[_sides];
		var angle = 0f;
		var angleStep = (2 * Mathf.PI) / _sides;

		var t = index / (_positions.Length - 1f);
		var radius = _useTwoRadii ? Mathf.Lerp(_radiusOne, _radiusTwo, t) : _radiusOne;

		for (int i = 0; i < _sides; i++)
		{
			var x = Mathf.Cos(angle);
			var y = Mathf.Sin(angle);

			circle[i] = _wPositions[index] + side * x * radius + up * y * radius;//_positions[index] + side * x * radius + up * y * radius;

			angle += angleStep;
		}

		return circle;
	}
}
