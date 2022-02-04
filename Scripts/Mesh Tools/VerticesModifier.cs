using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticesModifier : MonoBehaviour
{
    MeshModifier Modifier;

    // need to be really carefull with this ... because mesh is ok but vertices is real heavy ... 
    public Mesh mesh;
    public Vector3[] vertices;

    void Start()
    {
        Modifier = GetComponent<MeshModifier>();
    }

    void Update()
    {
        
    }
}
