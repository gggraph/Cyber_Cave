using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Dummiesman;
using TMPro;

/*
            TODO :
            Panneau informatif
            Shader ou dome 

            Horizontal & Vertical Menu
            Sign d'interaction Menu
            Ligne comme sur PS3 dans les menus . 
            Basic Disclaimer Window. 
            Write Font From Texture.. (SPRITE SYS) 

            MENU ID : 
            00       Open General Menu
            01       Custom Mesh Selection
            02       Custom Texture Selection
            03       Custom Sound File Selection
            04       Primitive Mesh Selection
            05       Select Either Primitive or Custom Mesh Menu
            06       Tool Mode Selection
 
 */
public class Menu : MonoBehaviour
{
    public Selector caller;

    public bool _readyToClose;
    public virtual void Create(Selector clr) { caller = clr; }
    public virtual void ProcessInput() { }
    public virtual void Destroy() { }
    public virtual bool IsMenuReadyToClose() { return false; }
    
}

public class CircularMenu : Menu
{
    public Pivot pivot;
    public override void Create ( Selector clr)
    {
        caller = clr;
        
        GameObject rig = GameObject.Find("OVRCameraRig");
        Vector3 center = new Vector3(rig.transform.position.x, rig.transform.position.y, rig.transform.position.z);
        GameObject p  = new GameObject();
        pivot = p.AddComponent<Pivot>();
        pivot.Initialize(center, Camera.main.transform.eulerAngles, 5f, 8);
        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i])
            {
                SmoothMovement s = caller.objects[i].AddComponent<SmoothMovement>();
                s.SetScale(caller.objects[i].transform.localScale);
                s.SetPosition(pivot.GetPivotPosition(i)); // the first position should 
                s.SetSpeed(6f);
            }
               
        }

        Vector3[] frontvecs2 = MathUtilities.GetPositionAndEulerInFrontOfPlayer(0f);
        caller.gUI = ObjectUtilities.InstantiateGameObjectFromAssets("LinearMenuUI");
        caller.gUI.transform.position = frontvecs2[0];
        caller.gUI.transform.eulerAngles = frontvecs2[1];

    }
    public int frameTick = 0;
    public override void ProcessInput()
    {
        frameTick++;
        if (frameTick > 30)
        {
            frameTick = 0;
        }
        else
        {
            return;
        }
        if (HandRecognition.IsPose_ShiftRightSign() || Input.GetKey(KeyCode.RightArrow))
        {
            // rotate smoothly pivot then if middle object is near target (treshold) load object 
            if ( caller.cursor > 0)
            {
                caller.cursor--;
                // <-- 
                // 0 1 2 3 4 5 6 7 
                // n 0 1 2 3 4 5 6 
                Destroy(caller.objects[7]); // or do a fadeoutanddestroy?
                for (int i = 7; i >= 1; i--)
                {
                    caller.objects[i] = caller.objects[i - 1];
                }
                caller.objects[0] = caller.CreateObjectFromSelectorMode(caller.cursor);
                SmoothMovement s = caller.objects[0].AddComponent<SmoothMovement>();
                s.SetScale(caller.objects[0].transform.localScale);
                caller.objects[0].transform.position = pivot.GetPivotPosition(1);
            }
            else
            {
                pivot.TurnRight();
            }

        }
        if (HandRecognition.IsPose_ShiftLeftSign() || Input.GetKey(KeyCode.LeftArrow))
        {
            
            if ( caller.choicesize-1 > caller.cursor + 7)
            {
                caller.cursor++;
                // -- > 
                // 0 1 2 3 4 5 6 7 
                // 1 2 3 4 5 6 7 n  
                
                Destroy(caller.objects[0].gameObject); // or do a fadeoutanddestroy?
                for (int i = 0; i <= 6; i++)
                {
                    caller.objects[i] = caller.objects[i + 1];
                }
                
                caller.objects[7] = caller.CreateObjectFromSelectorMode(caller.cursor + 7);
                SmoothMovement s = caller.objects[7].AddComponent<SmoothMovement>();
                s.SetScale(caller.objects[7].transform.localScale);
                caller.objects[7].transform.position = pivot.GetPivotPosition(6);

            }
            else
            {
                pivot.TurnLeft();
            }
        }

        // Apply Pointing system here 

        // 4 debug using RequireSystemKeyboard = true in OVRManager (attached on RIG) 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 20);
        List<GameObject> touched = new List<GameObject>();

        foreach (RaycastHit h in hits)
        {
            if (h.transform.gameObject != null)
            {
                if (!touched.Contains(h.transform.root.gameObject))
                {
                    List<GameObject> allchilds = new List<GameObject>();
                    ObjectUtilities.GetChildsFromParent(h.transform.root.gameObject, allchilds); // recursive loop
                    foreach (GameObject go in allchilds)
                    {
                        touched.Add(go);
                    }
                }
            }
               
        }

        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                if (HandRecognition.DoesLeftFingerPointingObject_SphereCast(caller.objects[i], 1f, 20f)
                    || HandRecognition.DoesRightFingerPointingObject_SphereCast(caller.objects[i], 1f, 20f)
                    || touched.Contains(caller.objects[i])
                    )
                {
                    if (caller.obptr != i)
                    {
                        caller.DoHighLightSound();
                        caller.RemoveHighLightSettingOfObject(caller.obptr);
                        caller.obptr = i;
                        caller.ApplyHighLightSettingToObject(caller.obptr);
                    }

                    // play sound here .... 

                }

            }
        }

        // Apply Position

        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                if (i == caller.obptr)
                {
                    caller.objects[i].GetComponent<SmoothMovement>().SetPosition(
                       Vector3.MoveTowards(pivot.GetPivotPosition(i), Camera.main.transform.position,1.5f)
                       );
                   // caller.objects[i].GetComponent<SmoothMovement>().SetScale(new Vector3(1.5f, 1.5f, 1.5f));
                }
                else
                {
                    caller.objects[i].GetComponent<SmoothMovement>().SetPosition(pivot.GetPivotPosition(i));
                   // caller.objects[i].GetComponent<SmoothMovement>().SetScale(new Vector3(1, 1, 1));
                }
            }
        }

    }

    public override void Destroy()
    {
        // Do a pause animation
        Vector3[] frontdata = MathUtilities.GetPositionAndEulerInFrontOfPlayer(5f);
        if (caller.obptr > -1)
        {
            caller.objects[caller.obptr].GetComponent<SmoothMovement>().SetPosition(frontdata[0]);
        }
        base.Destroy();
    }
}


public class VerticalMenu : Menu
{

}
public class HorizontalMenu : Menu
{
    GameObject center;
    List<Vector3> vecs = new List<Vector3>();

    public override void Create ( Selector clr)
    {
        caller = clr;
        center = new GameObject();

        // [0] set first placement 

        float intervalX = 3f;
        float cx = 0f;
        int octr = 0;
        
        foreach (GameObject go in caller.objects)
        {
            Vector3 npos = new Vector3();
            if (go != null)
            {

                npos = new Vector3(cx, 0f, 0f);
                cx += intervalX;
                octr++;
            }
            vecs.Add(npos);
        }
        // [1] recentring objects and attach to center object

        float middlex = (intervalX * (octr - 1)) / 2;

        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                // USE QUADRATIC OR BEZIER HERE ON Z-AXIS
                vecs[i] = new Vector3(vecs[i].x - middlex, 0, 0);
                caller.objects[i].transform.position = vecs[i];
                caller.objects[i].transform.parent = center.transform;
               
            }
        }

       
       
        // [2] Set center position 
        center.name = "center";
        Vector3[] frontvecs = MathUtilities.GetPositionAndEulerInFrontOfPlayer(8f);
        center.transform.position = frontvecs[0];
        center.transform.eulerAngles = frontvecs[1];

        Vector3[] frontvecs2 = MathUtilities.GetPositionAndEulerInFrontOfPlayer(0f);
        caller.gUI = ObjectUtilities.InstantiateGameObjectFromAssets("LinearMenuUI");
        caller.gUI.transform.position = frontvecs2[0];
        caller.gUI.transform.eulerAngles = frontvecs2[1];

        // [3] Initialize first menu animation
        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                vecs[i] = caller.objects[i].transform.position;
                caller.objects[i].transform.position = center.transform.position; // < - to make it smoothly from center to their position.
                SmoothMovement m = caller.objects[i].AddComponent<SmoothMovement>();
                m.SetPosition(vecs[i]);
                m.SetScale(caller.objects[i].transform.localScale);
                m.SetSpeed(3f);
            }
        }

    }
    public int frameTick = 0;
    public override void ProcessInput()
    {
        // [0] limit number of frame here 
        frameTick++;
        if (frameTick > 10)
        {
            frameTick = 0;
        }
        else
        {
            return;
        }

        // [1] Set alpha on object from their position in array 
        for (int i = 0; i < 8; i ++)
        {
            // Set alpha of object from distance they have from center position 
            if ( caller.objects[i] != null )
            {
                float alpha = 0f; // 1f - ((Vector3.Distance(center.transform.position, caller.objects[i].transform.position) / 5f));
                // apply alpha on all renderer
                List<GameObject> allgo = new List<GameObject>();
                ObjectUtilities.GetChildsFromParent(caller.objects[i], allgo); // recursive loop
                foreach ( GameObject go in allgo)
                {
                    if (go.GetComponent<Renderer>())
                    {
                        Renderer r = go.GetComponent<Renderer>();
                        if (r != null)
                        {
                            Color c = r.material.color;
                            r.material.color = new Color(c.r, c.g, c.b, alpha);
                        }
                    }
                }
                
                    
            }
        }

        // [2] Detect Right Shift
        if (HandRecognition.IsPose_ShiftRightSign() || Input.GetKey(KeyCode.RightArrow))
        {
            // rotate smoothly pivot then if middle object is near target (treshold) load object 
            if (caller.cursor > 0)
            {
                caller.cursor--;
                // <-- 
                // 0 1 2 3 4 5 6 7 
                // n 0 1 2 3 4 5 6 
                Vector3 oldpos = caller.objects[0].transform.position;
                Destroy(caller.objects[7]); // or do a fadeoutanddestroy?
                for (int i = 7; i >= 1; i--)
                {
                    caller.objects[i] = caller.objects[i - 1];
                }
                caller.objects[0] = caller.CreateObjectFromSelectorMode(caller.cursor);
                caller.objects[0].transform.position = oldpos;
                SmoothMovement s = caller.objects[0].AddComponent<SmoothMovement>();
                s.SetScale(caller.objects[0].transform.localScale);
                
            }

        }
        if (HandRecognition.IsPose_ShiftLeftSign() || Input.GetKey(KeyCode.LeftArrow))
        {

            if (caller.choicesize - 1 > caller.cursor + 7)
            {
                caller.cursor++;
                // -- > 
                // 0 1 2 3 4 5 6 7 
                // 1 2 3 4 5 6 7 n  
                Vector3 oldpos = caller.objects[7].transform.position;
                Destroy(caller.objects[0].gameObject); // or do a fadeoutanddestroy?
                for (int i = 0; i <= 6; i++)
                {
                    caller.objects[i] = caller.objects[i + 1];
                }

                caller.objects[7] = caller.CreateObjectFromSelectorMode(caller.cursor + 7);
                caller.objects[7].transform.position = oldpos;
                SmoothMovement s = caller.objects[7].AddComponent<SmoothMovement>();
                s.SetScale(caller.objects[7].transform.localScale);

            }
        }

        // [2] Set Position 
        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                if (i == caller.obptr)
                {
                    caller.objects[i].GetComponent<SmoothMovement>().SetPosition(
                       Vector3.MoveTowards(vecs[i], Camera.main.transform.position, 0.1f)
                       );
                }
                else
                {
                    caller.objects[i].GetComponent<SmoothMovement>().SetPosition(vecs[i]);
                }
            }
        }
        base.ProcessInput(); 
    }

    public override void Destroy()
    {
        // Do a pause animation
        if (caller.obptr > -1)
        {
            caller.objects[caller.obptr].GetComponent<SmoothMovement>().SetPosition(center.transform.position);
        }
        center.transform.DetachChildren();
        Destroy(center.gameObject);
        base.Destroy();
    }
}
public class LinearMenu : Menu
{
    GameObject center;
    List<Vector3> vecs = new List<Vector3>();
    // Linear Menu works up to 6 objects . The
    public override void Create(Selector clr)
    {
        Debug.Log("Creating Linear Menu");
        caller = clr;
        center = new GameObject();

        float intervalX = 3f;
        float cx = 0f;
        int octr = 0;
        foreach ( GameObject go in caller.objects)
        {
            Vector3 npos = new Vector3(); 
            if ( go != null)
            {
                
                npos = new Vector3(cx, 0f, 0f);
                cx += intervalX;
                octr++;
            }
            // set object in a line at equal distance 
            vecs.Add(npos);
            // then for each objects ... 
        }
        float middlex = (intervalX * (octr-1)) / 2;

        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                vecs[i] = new Vector3(vecs[i].x - middlex, 0, 0);
                caller.objects[i].transform.position = vecs[i];
                caller.objects[i].transform.parent = center.transform;
            }
        }
        center.name = "center";
        Vector3[] frontvecs = MathUtilities.GetPositionAndEulerInFrontOfPlayer(8f);
        center.transform.position = frontvecs[0];
        center.transform.eulerAngles = frontvecs[1];

        Vector3[] frontvecs2 = MathUtilities.GetPositionAndEulerInFrontOfPlayer(0f);
        caller.gUI = ObjectUtilities.InstantiateGameObjectFromAssets("LinearMenuUI");
        caller.gUI.transform.position = frontvecs2[0];
        caller.gUI.transform.eulerAngles = frontvecs2[1];
        // here we need to set y to rig input y pos .... or something like this 
        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                vecs[i] = caller.objects[i].transform.position;
                caller.objects[i].transform.position = center.transform.position; // < - to make it smoothly from center to their position.
                SmoothMovement m = caller.objects[i].AddComponent<SmoothMovement>();
                m.SetPosition(vecs[i]);
                m.SetSpeed(3f);
            }
        }
        base.Create(clr);
    }
    public int frameTick = 0;
    public override void ProcessInput()
    {
        frameTick++;
        // limit number of frame here 
        if (frameTick > 10)
        {
            frameTick = 0;
        }
        else
        {
            return;
        }

        // 4 debug using RequireSystemKeyboard = true in OVRManager (attached on RIG) 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 20);
        List<GameObject> touched = new List<GameObject>();
        foreach ( RaycastHit h in hits)
        {
            if (h.transform.gameObject != null)
                touched.Add(h.transform.gameObject);
        }

        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                if (HandRecognition.DoesLeftFingerPointingObject_SphereCast(caller.objects[i], 1f, 20f)
                    || HandRecognition.DoesRightFingerPointingObject_SphereCast(caller.objects[i], 1f, 20f)
                    || touched.Contains(caller.objects[i])
                    )
                {
                    if (caller.obptr != i)
                    {
                        caller.DoHighLightSound();
                        caller.RemoveHighLightSettingOfObject(caller.obptr);
                        caller.obptr = i;
                        caller.ApplyHighLightSettingToObject(caller.obptr);
                    }
                    
                    // play sound here .... 
                    
                }

            }
        }
        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                if (i == caller.obptr)
                {
                    caller.objects[i].GetComponent<SmoothMovement>().SetPosition(
                       Vector3.MoveTowards(vecs[i], Camera.main.transform.position, 0.1f)
                       );
                    caller.objects[i].GetComponent<SmoothMovement>().SetScale(new Vector3(1.5f, 1.5f, 1.5f));
                }
                else
                {
                    caller.objects[i].GetComponent<SmoothMovement>().SetPosition(vecs[i]);
                    caller.objects[i].GetComponent<SmoothMovement>().SetScale(new Vector3(1, 1, 1));
                }
            }
        }
    }
    public override void Destroy()
    {
        // Do a pause animation
        if (caller.obptr >-1)
        {
            Debug.Log("MOVING");
            Vector3[] frontdata = MathUtilities.GetPositionAndEulerInFrontOfPlayer(5f);
            caller.objects[caller.obptr].GetComponent<SmoothMovement>().SetPosition(frontdata[0]);
        }
        //center.transform.DetachChildren();
       // Destroy(center.gameObject);
        base.Destroy();
    }


}

public class WheelMenu : Menu
{
    GameObject center;
    List<Vector3> vecs = new List<Vector3>();
    // Linear Menu works up to 8 objects . The
    public override void Create(Selector clr)
    {
        Debug.Log("Creating Wheel Menu");
        caller = clr;
        center = new GameObject();
        Vector3[] wheelPoints = MathUtilities.GetCirclePoints(caller.choicesize, 2.5f, new Vector3(0, 0, 0));
        for (int i = 0; i < caller.choicesize;   i++ )
        {
            vecs.Add(new Vector3(wheelPoints[i].x, wheelPoints[i].z, wheelPoints[i].y));
        }
        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                caller.objects[i].transform.position = vecs[i];
                caller.objects[i].transform.parent = center.transform;
            }
        }

        center.name = "center";
        Vector3[] frontvecs = MathUtilities.GetPositionAndEulerInFrontOfPlayer(8f);
        center.transform.position = frontvecs[0];
        center.transform.eulerAngles = frontvecs[1];

        Vector3[] frontvecs2 = MathUtilities.GetPositionAndEulerInFrontOfPlayer(0f);
        caller.gUI = ObjectUtilities.InstantiateGameObjectFromAssets("CircularMenuUI");
        caller.gUI.transform.position = frontvecs2[0];
        caller.gUI.transform.eulerAngles = frontvecs2[1];

        // here we need to set y to rig input y pos .... or something like this 
        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                vecs[i] = caller.objects[i].transform.position;
                caller.objects[i].transform.position = center.transform.position; // < - to make it smoothly from center to their position.
                SmoothMovement m = caller.objects[i].AddComponent<SmoothMovement>();
                m.SetPosition(vecs[i]);
                m.SetSpeed(3f);
            }
        }
        base.Create(clr);
    }
    public int frameTick = 0;
    public override void ProcessInput()
    {
        frameTick++;
        // limit number of frame here 
        if (frameTick > 10)
        {
            frameTick = 0;
        }
        else
        {
            return;
        }

        // 4 debug using RequireSystemKeyboard = true in OVRManager (attached on RIG) 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 20);
        List<GameObject> touched = new List<GameObject>();
        foreach (RaycastHit h in hits)
        {
            if (h.transform.gameObject != null)
                touched.Add(h.transform.gameObject);
        }

        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                if (HandRecognition.DoesLeftFingerPointingObject_SphereCast(caller.objects[i], 1f, 20f)
                    || HandRecognition.DoesRightFingerPointingObject_SphereCast(caller.objects[i], 1f, 20f)
                    || touched.Contains(caller.objects[i])
                    )
                {
                    if (caller.obptr != i)
                    {
                        caller.DoHighLightSound();
                        caller.RemoveHighLightSettingOfObject(caller.obptr);
                        caller.obptr = i;
                        caller.ApplyHighLightSettingToObject(caller.obptr);
                    }

                    // play sound here .... 

                }

            }
        }
        for (int i = 0; i < caller.objects.Length; i++)
        {
            if (caller.objects[i] != null)
            {
                if (i == caller.obptr)
                {
                    caller.objects[i].GetComponent<SmoothMovement>().SetPosition(
                       Vector3.MoveTowards(vecs[i], Camera.main.transform.position, 0.1f)
                       );
                    caller.objects[i].GetComponent<SmoothMovement>().SetScale(new Vector3(1.5f, 1.5f, 1.5f));
                }
                else
                {
                    caller.objects[i].GetComponent<SmoothMovement>().SetPosition(vecs[i]);
                    caller.objects[i].GetComponent<SmoothMovement>().SetScale(new Vector3(1, 1, 1));
                }
            }
        }
    }
    public override void Destroy()
    {
        // Do a pause animation
        if (caller.obptr > -1)
        {
            caller.objects[caller.obptr].GetComponent<SmoothMovement>().SetPosition(center.transform.position);
        }
      //  center.transform.DetachChildren(); // it keeps tracks of objects ... 
      /*
        for ( int i = 0; i < 8; i ++)
        {
            if ( caller.objects[i] != null)
            {
                caller.objects[i].transform.parent = null; // Loose completely track of the object. it set array of null 
            }
                
           
        }
        */
      //  Destroy(center.gameObject);
        base.Destroy();
    }


}

public class Selector : MonoBehaviour
{
    /* 
               --------------------------------         SELECTOR        ----------------------------------

               MENUID                                 :   THE CURRENT ROOM TYPE
               FILES                                  :   AN ARRAY OF FILE NAME IF FILE SELECTOR READING FROM DIRECTORY
               OBJS                                   :   UP TO 8 OBJECTS WHICH ARE CURRENTLY SEEN
               SEL                                    :   THE CURRENT INDEX IN FILES CORRESPONDING TO OBJS#
               CURSOR                                 :   SET START OF THE SCOPE POSITION 
               OBPTR                                  :   Selected Object [from 0 up to #8] 
               CHOICESIZE                             :   Number of possible elements to select... 

               -------------------------------- -------------------------------- --------------------------------
                Interaction : 
                OK    validate choice
                Left  Set pointer to left
                Right Set pointer to right
                signe se rapprocher (main a plat etc. paume vers corps )           -> augmenter distance avatar/ object (rayons, distance, courbe etc. ) 
                resize signe        (main a plat paume tourné l'un vers l'autres ) -> augmenter taille des objets ( rescale ) 

   */
    public int menuID              =  0;
    public int representation_Mode =  0;
    public int sound_Mode          =  0;
    public string[] files              ;
    public GameObject[] objects        ;
    public bool _done              = false;
    public int choicesize          =  0;
    public int cursor              =  0;
    public int obptr               =  -1;
    public Menu menu                   ;
    public GameObject dome             ;
    public GameObject gUI              ;

    public IEnumerator OpenMenuAndGetSelection( int mID, int rMode, int sMode, int dMode, 
        System.Action<int> callback = null
        )
    {
        // UNRENDER ALL EXCEPT AVATAR
        Debug.Log("selector will open ");
        // init variable
        GameStatus._selectorIsOpen = true;
        menuID = mID;
        representation_Mode = rMode;
        sound_Mode = sMode;
        // Start Creating the room
        CreateRoomObjects();
        DoOpeningSound();
        dome = ObjectUtilities.CreateDome();
        CreateMenuObject();
        menu.Create(this);

        SetgUIText();
        float okctr = 0;
        float noctr = 0;
        float second2validated = 1.2f;
        GameObject RadialBar = ObjectUtilities.FindGameObjectChild(gUI, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;

        GameObject RadialBar2 = ObjectUtilities.FindGameObjectChild(gUI, "RadialBar2");
        RadialBar2.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;

        while ( true) 
        {
            if ((HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0)) && !GameStatus._WindowIsOpen)
            {
                okctr += Time.deltaTime;
                noctr = 0;
            }
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if ((HandRecognition.IsPose_CloseSign() || Input.GetMouseButton(1)) && !GameStatus._WindowIsOpen)
            {
                noctr += Time.deltaTime;
                okctr = 0;
            }
            else
            {
                if (noctr - Time.deltaTime >= 0)
                    noctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            if (noctr >= second2validated)
            {
                okctr = 0;
                obptr = -1;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;
            RadialBar2.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(noctr) / second2validated;
            if ( !GameStatus._WindowIsOpen)
                menu.ProcessInput();

            yield return new WaitForEndOfFrame();
        }
       
        DoValidateSound();
        ClearRoom();
        menu.Destroy();
        // some stuff to wait the animated selected object 
        if ( obptr > -1)
        {
            while (objects[obptr + cursor] != null)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        
        callback(obptr+cursor);
        yield break;
    }

    public void CreateMenuObject()
    {
        switch ( representation_Mode)
        {
            case 0:
                menu = new CircularMenu();
                break;
            case 1:
                menu = new HorizontalMenu();
                break;
            case 2:
                menu = new VerticalMenu();
                break;
            case 3:
                menu = new LinearMenu();
                break;
            case 4:
                menu = new WheelMenu();
                break;

        }
    }

    public void CreateRoomObjects()
    {

        objects = new GameObject[8];
        for (int i = 0; i < 8; i++)
        {
           
             objects[i] = CreateObjectFromSelectorMode(i);

        }
        ApplyHighLightSettingToObject(obptr);
    }

    public void ClearRoom()
    {
        if (objects != null)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if ( objects[i] != null)
                {
                    
                    if ( i != obptr)
                    {
                        ObjectUtilities.FadeAndDestroyObject(objects[i], 2f);
                    }
                    else
                    {
                        ObjectUtilities.FadeAndDestroyObject(objects[i], 4f);
                    }
                }
                else
                {
                    Debug.Log("Object # "+i+"was null! ");
                }
            }
           
        }
        
        if ( dome != null)
        {
            ObjectUtilities.FadeAndDestroyObject(dome, 2f);
        }
        Destroy(gUI.gameObject);
    }


    public void SetgUIText() 
    {
        string s = ""; 
        switch (menuID)
        {
            case 0: s = System.DateTime.Now.ToString("HH:mm"); break;
            case 1: s = "Select a mesh"; break;
            case 2: s = "Select a texture"; break;
            case 3: s = "Select a sound"; break;
            case 4: s = "Select a mesh"; break;
            case 5: s = "Select mesh type"; break;
            case 6: s = "Select a tool mod"; break;
            case 7: s = System.DateTime.Now.ToString("HH:mm"); break;
            case 8: s = "Select type of sound box"; break;
        }
        GameObject text = ObjectUtilities.FindGameObjectChild(gUI, "Text");
        text.GetComponent<TMP_Text>().text = s;
    }
    public GameObject CreateObjectFromSelectorMode(int index)
    {
        if (menuID == 0)
        {
            // Selection of custom file from folder. Do nothing. Just represent a cube ... 
            GameObject go = null;
            choicesize = 5;
            string objtext = "";
            switch ( index)
            {
                case 0:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    objtext = "reducto";
                    break;
                case 1:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    objtext = "tools";
                    break;
                case 2:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    objtext = "meshes";
                    break;
                case 3:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    objtext = "parameters";
                    break;
            }
            if (go == null) { return null; }
            ObjectUtilities.FadeInObject(go, 3f);
            MenuAnimation m = go.AddComponent<MenuAnimation_Default>();
            m._info = objtext;
            MaterialUtilities.SetMaterialToObjectsFromPool(go, 2);
            return go;
        }
        // This menu is for custom Mesh Selection
        if (menuID == 1)
        {
            // Selection of custom Mesh
            if (files == null)
            {
                string logpath = P2SHARE.GetDirByType(1) + "chksmlog.txt";
                if (File.Exists(logpath))
                    files = File.ReadAllLines(logpath);
                else
                {
                    Debug.Log("Failed to create room selector cause checksum log not existing");
                    return null;
                }
                choicesize = files.Length;
            }
            if (files.Length <= index)
                return null;

            GameObject go = new OBJLoader().Load(files[index]);
            float s = 2f;
            ObjectUtilities.RescaleMeshToSize(go, s);
           
            go.AddComponent<MenuAnimation_Default>();
            ObjectUtilities.CreateCustomColliderFromFirstViableMesh(go);
            MaterialUtilities.SetMaterialToObjectsFromPool(go, 2);
            ObjectUtilities.FadeInObject(go, 3f);
            return go;

        }
        if (menuID == 2)
        {
            // Selection of custom texture
            if (files == null)
            {
                string logpath = P2SHARE.GetDirByType(3) + "chksmlog.txt";
                if (File.Exists(logpath))
                    files = File.ReadAllLines(logpath);
                else
                {
                    Debug.Log("Failed to create room selector cause checksum log not existing");
                    return null;
                }
                choicesize = files.Length;
            }
            if (files.Length <= index)
                return null;
            // Create a basic cube with the texture 
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Texture2D tex = new Texture2D(1, 1);
            byte[] img = File.ReadAllBytes(files[index]);
            tex.LoadImage(img);
            go.GetComponent<Renderer>().material.mainTexture = tex;
            ObjectUtilities.FadeInObject(go, 3f);
            go.AddComponent<MenuAnimation_Default>();
            MaterialUtilities.SetMaterialToObjectsFromPool(go, 2);
            return go;
        }
        if (menuID == 3)
        {
            // Selection of custom sound for soundbox
            if (files == null)
            {
                string logpath = P2SHARE.GetDirByType(4) + "chksmlog.txt";
                if (File.Exists(logpath))
                    files = File.ReadAllLines(logpath);
                else
                {
                    Debug.Log("Failed to create room selector cause checksum log not existing");
                    return null;
                }
                choicesize = files.Length;
            }
            if (files.Length <= index)
                return null;
            GameObject go = ObjectUtilities.InstantiateGameObjectFromAssets("hifi");
            go.AddComponent<AudioSource>();
            go.AddComponent<SoundHit>();
            go.GetComponent<SoundHit>().StartCoroutine(go.GetComponent<SoundHit>().LoadMusic(files[index], true));
            ObjectUtilities.FadeInObject(go, 3f);

            go.AddComponent<MenuAnimation_SoundObject>();
            ObjectUtilities.CreateCustomColliderFromFirstViableMesh(go);
            MaterialUtilities.SetMaterialToObjectsFromPool(go, 2);
            return go;

        }
        if (menuID == 4)
        {
            // Selection of primitive mesh
            choicesize = 6;
            GameObject go = null;
            switch (index)
            {
                case 0:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case 1:
                    go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                case 2:
                    go = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    go.transform.Rotate(-90, 0, 0);
                    break;
                case 3:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    break;
                case 4:
                    go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    break;
                case 5:
                    go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    go.transform.Rotate(-180, 0, 0);
                    break;
            }
            if ( go == null) { return null; }
            ObjectUtilities.FadeInObject(go, 3f);
            go.AddComponent<MenuAnimation_Default>();
            MaterialUtilities.SetMaterialToObjectsFromPool(go, 2);
            return go;
        }
        if ( menuID == 5)
        {
            // Choice : Primitive or Custom ?
            GameObject go = null;
            string s = "";
            choicesize = 3;
            switch (index)
            {
                case 0:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); s = "Primitive mesh";
                    break;
                case 1:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); s = "Custom mesh";
                    break;
                case 2:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); s = "Sound Box";
                    break;
            }
            if (go == null) { return null; }
            ObjectUtilities.FadeInObject(go, 3f);
            MenuAnimation_Default m = go.AddComponent<MenuAnimation_Default>();
            m._info = s;
            MaterialUtilities.SetMaterialToObjectsFromPool(go, 2);
            return go;
        }
        if (menuID == 6)
        {
            // Tool Menu Selection
            GameObject go = null;
            choicesize = 4;
            string s = "";
            switch (index)
            {
                case 0:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); s = "Transform Editor";
                    break;
                case 1:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); s = "Vertices Editor";
                    break;
                case 2:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); s = "Paint Editor";
                    break;
                case 3:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); s = "Null Editor";
                    break;
            }
            if (go == null) { return null; }
            ObjectUtilities.FadeInObject(go, 3f);
            MenuAnimation_Default m = go.AddComponent<MenuAnimation_Default>();
            m._info = s;
            MaterialUtilities.SetMaterialToObjectsFromPool(go, 2);
            return go;
        }

        if (menuID == 7)
        {
            // Choice : Explore Memories, Enter Last Room, Create New Room
            GameObject go = null;
            string s = "";
            choicesize = 3;
            switch (index)
            {
                case 0:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); s = "Explore Memories";
                    break;
                case 1:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); s = "Enter Last Room";
                    break;
                case 2:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); s = "Create New Room";
                    break;
            }
            if (go == null) { return null; }
            ObjectUtilities.FadeInObject(go, 3f);
            MenuAnimation_Default m = go.AddComponent<MenuAnimation_Default>();
            m._info = s;
            MaterialUtilities.SetMaterialToObjectsFromPool(go, 2);
            return go;
        }
        if (menuID == 8)
        {
            // Choice : Primitive or Custom ?
            GameObject go = null;
            string s = "";
            choicesize = 2;
            switch (index)
            {
                case 0:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); s = "Atmosphere";
                    break;
                case 1:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); s = "Hit";
                    break;
            }
            if (go == null) { return null; }
            ObjectUtilities.FadeInObject(go, 3f);
            MenuAnimation_Default m = go.AddComponent<MenuAnimation_Default>();
            m._info = s;
            MaterialUtilities.SetMaterialToObjectsFromPool(go, 2);
            return go;
        }
        return null;
    }


    public void ApplyHighLightSettingToObject(int objectindex)
    {
        if (objectindex < 0 || objects.Length-1 < objectindex)
            return;
        if (objects[objectindex] == null)
            return;
        if (objects[objectindex].GetComponent<MenuAnimation>())
            objects[objectindex].GetComponent<MenuAnimation>().SetHighLight();
    }
    public void RemoveHighLightSettingOfObject(int objectindex)
    {
        if (objectindex < 0 || objects.Length - 1 < objectindex)
            return;
        if (objects[objectindex] == null)
            return;
        if (objects[objectindex].GetComponent<MenuAnimation>())
            objects[objectindex].GetComponent<MenuAnimation>().DisableHighLight();
    }

    public void DoOpeningSound()
    {
        switch ( sound_Mode)
        {
            case 0:
                SoundMap.PlaySoundAndDisposeFromAssets("val2");
                break;
            case 1: 
                
                break;
        }
    }
    public void DoClosingSound()
    {
        switch (sound_Mode)
        {
            case 0:
                SoundMap.PlaySoundAndDisposeFromAssets("val1");
                break;
            case 1:

                break;
        }
        
    }
    public void DoValidateSound()
    {
        switch (sound_Mode)
        {
            case 0:
                SoundMap.PlaySoundAndDisposeFromAssets("val1");
                break;
            case 1:

                break;
        }
    }
     int mysterious_counter = 0;
     List<int> mysterious_sequence = new List<int>();
    public void DoHighLightSound()
    {
        mysterious_counter++;
        
        switch (sound_Mode)
        {
            case 0:
                int rtiny = Random.Range(1, 5);
                mysterious_sequence.Add(rtiny);
                SoundMap.PlaySoundAndDisposeFromAssets("celest" + rtiny.ToString());
                break;
            case 1:
                rtiny = Random.Range(1, 5);
                mysterious_sequence.Add(rtiny);
                SoundMap.PlaySoundAndDisposeFromAssets("nighty" + rtiny.ToString());
                if (mysterious_counter > 100)
                {
                    SoundMap.PlaySoundAndDisposeFromAssets("nightyplus" + rtiny.ToString());
                }
                if ( mysterious_counter > 110 )
                {
                    mysterious_counter = 0;
                }
                break;
        }

        
    }

   
}
