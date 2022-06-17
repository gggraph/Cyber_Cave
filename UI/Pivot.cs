using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pivot : MonoBehaviour
{
    private Vector3 target;
    private bool   _clockwise;

    private GameObject leftObject;
    private GameObject middleObject;
    private GameObject rightObject;

    private List<GameObject> objects = new List<GameObject>();
    private GameObject centerObject; 
    public void Initialize(Vector3 center, Vector3 euler, float distance, int numberOfObjects)
    {
        this.transform.position = center;
        Vector3[] vectors = MathUtilities.GetCirclePoints(numberOfObjects, distance, center);
        centerObject = new GameObject();
        centerObject.transform.position = center ;
        for (int i = 0; i < numberOfObjects; i++ )
        {
            GameObject g = new GameObject();
            g.transform.position = vectors[i];
            g.transform.parent = this.transform;
            objects.Add(g);
            g.transform.parent = centerObject.transform;
        }
        centerObject.transform.localEulerAngles = new Vector3(0, euler.y, 0);
        middleObject = objects[numberOfObjects/2];
        rightObject = objects[(numberOfObjects / 2) + 1];
        leftObject = objects[(numberOfObjects / 2) - 1];
        target = middleObject.transform.position;
    }
    private void OnDestroy()
    {
        foreach ( GameObject go in objects) 
        {
            Destroy(go.gameObject);
        }
        Destroy(centerObject.gameObject);
    }

    public void TurnLeft()
    {
        target = leftObject.transform.position;
        _clockwise = false;
    }
    public void TurnRight()
    {
        target = rightObject.transform.position;
        _clockwise = true;
    }
    public Vector3 GetPivotPosition(int pointnumber)
    {
        return objects[pointnumber].transform.position;
    }
    
    void Update()
    {
        if (!middleObject || !leftObject || !rightObject)
            return;

        if ( middleObject.transform.position != target )
        {

            float dir = 1f;
            if (_clockwise)
                dir = -dir;

            transform.Rotate(0, dir, 0);
        }
        
      
    }
}
