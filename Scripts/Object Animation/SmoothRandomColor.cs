using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothRandomColor : MonoBehaviour
{
    MeshFilter _meshFilter;
    Renderer renderer;
    void Start()
    {

        _meshFilter = GetComponent<MeshFilter>();
        renderer = GetComponent<Renderer>();
        renderer.material = MaterialUtilities.GetMaterialFromResourcesPool(1);
        //setColorToVertices(Color.red);
    }

    private void setColorToVertices(Color target)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        // create new colors array where the colors will be created.
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            //colors[i] = Color.Lerp(new Color(target.r / 2, target.g / 2, target.b / 2), target, vertices[i].y);
            colors[i] = Color.Lerp(Color.blue, target, vertices[i].y);
        }
           

        // assign the array of colors to the Mesh.
        mesh.colors = colors;

    }
    float timeLeft;
    Color targetColor;

    void Update()
    {
        if (timeLeft <= Time.deltaTime)
        {
            // transition complete
            // assign the target color
            //setColorToVertices(targetColor);
            renderer.material.color = targetColor;

            // start a new transition
            targetColor = new Color(Random.value, Random.value, Random.value);
            timeLeft = 3.0f;
        }
        else
        {
            // transition in progress
            // calculate interpolated color
            //setColorToVertices(Color.Lerp(renderer.material.color, targetColor, Time.deltaTime / timeLeft));
            renderer.material.color = Color.Lerp(renderer.material.color, targetColor, Time.deltaTime / timeLeft);

            // update the timer
            timeLeft -= Time.deltaTime;
        }
    }
    private Color getRandomColor()
    {
        float red = Random.Range(0, 1.0f);
        float green = Random.Range(0, 1.0f);
        float blue = Random.Range(0, 1.0f);
        return new Color(red, green, blue);
    }
}
