using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boot : MonoBehaviour
{
    private void Start()
    {
        GameObject avatar = ObjectUtilities.InstantiateGameObjectFromAssets("AvatarOffLine");
        avatar.name = "basicavatar";
        GetComponent<AnchorUpdater>().StartCoroutine(GetComponent<AnchorUpdater>().TryUpdatingAnchors_Offline());
        BitMemory.LoadMemoriesFromLatestUser();
       // BitMemory.ClearMemoriesOfCurrentUser();

        if ( !BitMemory.GetMemoriesBit(1)) 
        {
            GetComponent<MemoryEvent>().StartCoroutine(GetComponent<MemoryEvent>().Routine_000());
        }
        else
        {
            GlobalMenu.OpenSpecificMenu(new GlobalMenu.MenuTask(7, 3, 1, 0));
        }
        
    }
    public void ConnectToCYBERCAVE() 
    {

        if (GetComponent<AnchorUpdater>())
        {
            GetComponent<AnchorUpdater>().StartCoroutine(GetComponent<AnchorUpdater>().TryUpdatingAnchors_Online());
        }
        
        this.gameObject.AddComponent<NetMaster>();
        GameObject.Find("trail").transform.position = new Vector3(0,-2f,1.7f);
    }


    /*
    void BootRoutine_000()
    {
        GameObject dome = ObjectUtilities.CreateDome();
        StartCoroutine(new BasicWindows().CreateAndRunOkWindow(
               "Hello dear User! "+System.Environment.NewLine+"It seems it is your first time in CyberCave",
               0, "", 1,
               // new Selector object will not be destroyed because of callback 
               result =>
               {
                   StartCoroutine(new BasicWindows().CreateAndRunOkWindow(
                    "Let me Explain you some stuff",
                    0, "", 2,
                    // new Selector object will not be destroyed because of callback 
                    result =>
                    {
                        StartCoroutine(new BasicWindows().CreateAndRunOkWindow(
                        "Everything here is interactable with your hands",
                        0, "", 3,
                        // new Selector object will not be destroyed because of callback 
                        result =>
                        {
                            GlobalMenu.OpenGlobalMenu();
                            dome.AddComponent<FadeAndDestroy>().Init(2f);

                            Invoke("BootRoutine_001", 5f);
                            
                        }));
                    }));
               }));
    }

    void BootRoutine_001() 
    {
        StartCoroutine(new BasicWindows().CreateAndRunOkWindow(
               "This is an interactable menu. Try to point something.",
               0, "", 4,
               // new Selector object will not be destroyed because of callback 
               result =>
               {
                  
        }));
    }

    void BootRootine_002() 
    {
       
    }
    */

}
