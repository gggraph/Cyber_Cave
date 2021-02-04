using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovSync : MonoBehaviour
{

    private GameObject MainCam;
    private GameObject LeftController;
    private GameObject RightController;

    public GameObject Head;
    private GameObject LeftHand;
    private GameObject RightHand;
    private GameObject Center;
    private GameObject Light; // for test 


    void Start()
    {
        MainCam = this.transform.Find("Main Camera").gameObject;
        LeftController = this.transform.Find("LeftHand Controller").gameObject;
        RightController = this.transform.Find("RightHand Controller").gameObject;
        InvokeRepeating("GetAvatar", 2, 2f);


    }

    void GetAvatar() 
    {
        GameObject[] avatars = GameObject.FindGameObjectsWithTag("Avatar");
        foreach ( GameObject go in avatars) 
        { 
            if (go.GetComponent<AvatarScript>()._isMine) 
            {
                Head = go.transform.Find("Head").gameObject;
                LeftHand = go.transform.Find("LeftHand").gameObject;
                RightHand = go.transform.Find("RightHand").gameObject;
                Center = go.transform.Find("Center").gameObject;
                Light = go.transform.Find("Point Light").gameObject;
                CancelInvoke("GetAvatar");
                Debug.Log("avatar founded!");
                break; 
            
            }
        }
    }

 
   
    void Update()
    {
        if ( Head != null) 
        {


            if ( Head != null && LeftHand != null && RightHand != null ) 
            {
                // [0] Sync Head
                Head.transform.position = MainCam.transform.position; 
                Head.transform.rotation = MainCam.transform.rotation;
                // [1] Sync Left Hand
                LeftHand.transform.position = LeftController.transform.position;
                LeftHand.transform.rotation = LeftController.transform.rotation;
                // [2] Sync Right Hand
                RightHand.transform.position = RightController.transform.position;
                RightHand.transform.rotation = RightController.transform.rotation;
                // [3] Sync Rig
                Center.transform.position = this.transform.parent.transform.position;
                Center.transform.rotation = this.transform.parent.transform.rotation; 

            }
            if (Light != null)
                Light.transform.position = new Vector3(Center.transform.position.x, Center.transform.position.y + 1f, Center.transform.position.z);


        }
    }
}
