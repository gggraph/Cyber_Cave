using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMeshDeformation : MonoBehaviour
{
    Mesh deformingMesh;
    Vector3[] originalVertices, displacedVertices;
	Vector3[] vertexVelocities;
	public float force = 10f;
	public float forceOffset = 0.1f;
	void Start()
	{
		deformingMesh = GetComponent<MeshFilter>().mesh;
		originalVertices = deformingMesh.vertices;
		displacedVertices = new Vector3[originalVertices.Length];
		for (int i = 0; i < originalVertices.Length; i++)
		{
			displacedVertices[i] = originalVertices[i];

		}
		vertexVelocities = new Vector3[originalVertices.Length];
	}
	void HandleInput(Vector3 ptposition)
	{
		/*
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(inputRay, out hit))
		{
			MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
			if (deformer)
			{
				Vector3 point = hit.point; 
				point += hit.normal * forceOffset;
				deformer.AddDeformingForce(point, force);
			}
		}
		*/
		AddDeformingForce(ptposition, force);
	}
	public void AddDeformingForce(Vector3 point, float force)
	{
		point = transform.InverseTransformPoint(point);
		for (int i = 0; i < displacedVertices.Length; i++)
		{
			AddForceToVertex(i, point, force);
		}
	}

	void AddForceToVertex(int i, Vector3 point, float force)
	{
		Vector3 pointToVertex = displacedVertices[i] - point;
		pointToVertex *= uniformScale;
		float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
		float velocity = attenuatedForce * Time.deltaTime;
		vertexVelocities[i] += pointToVertex.normalized * velocity;
	}
	float uniformScale = 1f;
	void Update()
	{
		uniformScale = transform.localScale.x;
		HandleInput(this.transform.position + new Vector3(Random.Range(-1f, 1f) , Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
		for (int i = 0; i < displacedVertices.Length; i++)
		{
			UpdateVertex(i);
		}
		deformingMesh.vertices = displacedVertices;
		deformingMesh.RecalculateNormals();
	}
	public float springForce = 20f; public float damping = 5f;
	void UpdateVertex(int i)
	{
		Vector3 velocity = vertexVelocities[i];
		Vector3 displacement = displacedVertices[i] - originalVertices[i];
		displacement *= uniformScale;
		velocity -= displacement * springForce * Time.deltaTime;
		velocity *= 1f - damping * Time.deltaTime;
		vertexVelocities[i] = velocity;
		displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);
	}
}
