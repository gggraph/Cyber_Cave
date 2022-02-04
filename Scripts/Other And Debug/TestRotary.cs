using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRotary : MonoBehaviour
{
    GameObject pivot;
    GameObject[] gos = new GameObject[8];
    Vector3 targetpos = new Vector3();
    bool _isClockwise = false;


    void Start()
    {
        
        for (int i = 0; i < 8; i++)
        {
            gos[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gos[i].name = "rottest" + i.ToString();
        }
        Vector3 cntr = new Vector3(0, 0, 0);
        pivot = new GameObject();
        pivot.transform.position = cntr;
        Vector3[] vectors = GetCirclePoints(8, 6, cntr);
        for (int i = 0; i < 8; i ++)
        {
            gos[i].transform.position = vectors[i];
            gos[i].transform.parent = pivot.transform;
        }
        targetpos =  gos[0].transform.position;

        InvokeRepeating("RandomMove", 5, 5);
    }


    private void RandomMove()
    {
        int rd = Random.RandomRange(0, 2);
        if ( rd == 0)
        {
            MoveAntiClock();
        }
        else
        {
            MoveClock();
        }
    }
    private void MoveAntiClock()
    {
        if ( gos.Length > 1)
        {
            targetpos = gos[1].transform.position;
        }
        _isClockwise = false;
    }
    private void MoveClock()
    {
        targetpos = gos[gos.Length - 1].transform.position;
        _isClockwise = true;
    }

    private void Update()
    {
        if ( gos[0].transform.position != targetpos)
        {
            // check if - or + 
            float dir = 1f;
            if (!_isClockwise)
                dir = -dir;
            pivot.transform.Rotate(0, dir, 0);
        }    
    
    }

    Vector3[] GetCirclePoints(int points, float radius, Vector2 center)
    {
        Vector3[] v = new Vector3[points];
        float slice = 2 * Mathf.PI / points;
        for (int i = 0; i < points; i++)
        {

            float angle = slice * i;
            float newX = (center.x + radius * Mathf.Cos(angle));
            float newY = (center.y + radius * Mathf.Sin(angle));
            v[i] = new Vector3(newX, 0, newY);
        }
        return v;
    }
}
