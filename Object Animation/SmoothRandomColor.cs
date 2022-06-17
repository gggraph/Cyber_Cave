using UnityEngine;

public class SmoothRandomColor : MonoBehaviour
{
    MeshFilter _meshFilter;
    Renderer render;
    public float Speed = 3.0f;
    void Start()
    {

        _meshFilter = GetComponent<MeshFilter>();
        render = GetComponent<Renderer>();
        render.material = MaterialUtilities.GetMaterialFromResourcesPool(1);
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
            render.material.color = targetColor;

            // start a new transition
            targetColor = new Color(Random.value, Random.value, Random.value);
            timeLeft = Speed;
        }
        else
        {
            // transition in progress
            // calculate interpolated color
            //setColorToVertices(Color.Lerp(render.material.color, targetColor, Time.deltaTime / timeLeft));
            render.material.color = Color.Lerp(render.material.color, targetColor, Time.deltaTime / timeLeft);

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
