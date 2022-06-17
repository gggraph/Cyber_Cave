using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CaveMenu
{
    /* Script for menu*/

    #region MenuCreator
    public class MenuCreator : MonoBehaviour
    {
        // Class to Handle Menu Creation/Navigation
        public class MenuTask
        {
            public int mID { get; }
            public Menu.DisplayStyle rMode { get; }
            public Menu.SoundStyle sMode { get; }
            public MenuItem[] items { get; }

            public MenuTask(int menu_id, Menu.DisplayStyle representationMode, Menu.SoundStyle soundMode, MenuItem[] Items)
            {
                mID = menu_id;
                rMode = representationMode;
                sMode = soundMode;
                items = Items;
            }
        }
        public List<MenuTask> _tasks = new List<MenuTask>();
        public static void OpenDefaultMenu()
        {

            if (!GameStatus.IsGameFlagSet(2) && GameStatus.IsGameFlagSet(13))
            {
                GameStatus.SetGameFlag(2);
                GameObject ghost = new GameObject();
                MenuCreator n = ghost.AddComponent<MenuCreator>();
                // Craft MenuItem... Could Load a JSON next time
                MenuItem[] items = new MenuItem[8];
                items[0] = new MenuItem("cube.fbx", "nothing", true, "Quit CyberCave");
                items[1] = new MenuItem("cube.fbx", "nothing", true, "Free movement");
                items[2] = new MenuItem("cube.fbx", "nothing", true, "garden");
                items[3] = new MenuItem("cube.fbx", "nothing", true, "sculpt room");
                items[4] = new MenuItem("cube.fbx", "nothing", true, "paint room");
                items[5] = new MenuItem("cube.fbx", "nothing", true, "chill room");
                items[6] = new MenuItem("cube.fbx", "nothing", true, "guest room");
                items[7] = new MenuItem("cube.fbx", "nothing", true, "about");

                // Pass Offline & TP
                AnchorUpdater au = Camera.main.transform.root.GetComponent<AnchorUpdater>();
                au.StartCoroutine(au.TryUpdatingAnchors_Offline());
                Camera.main.transform.root.GetComponent<RaycastDebugger>().ShowRays();
                Camera.main.transform.root.gameObject.GetComponent<Boot>().CharacterBootPosition =
                    Camera.main.transform.root.gameObject.transform.position;
                Camera.main.transform.root.gameObject.transform.position = new Vector3(-35f, 500f, -35f);
                // Create Task and Run
                n._tasks.Add(new MenuTask(0, Menu.DisplayStyle.Wheel, Menu.SoundStyle.Plucky, items));
                n.RunMenuTask();

                //Sync
                NetUtilities.SendDataToAll(new byte[2] { 90,1 });
            }

        }
        public void RunMenuTask()
        {
            if (_tasks.Count == 0)
            {
                GameStatus.UnsetGameFlag(2);
                ObjectUtilities.FadeInObjectsAround(20f, 2f); // Ok this put bad stuff here 
                Debug.Log("CLOSING MENU");
                // Pass Online & TP BACK TO POSITION
                AnchorUpdater au = Camera.main.transform.root.GetComponent<AnchorUpdater>();
                au.StartCoroutine(au.TryUpdatingAnchors_Online());
                Camera.main.transform.root.gameObject.transform.position = Camera.main.transform.root.gameObject.GetComponent<Boot>().CharacterBootPosition;
                Camera.main.transform.root.GetComponent<RaycastDebugger>().UnShowRays();
                //Sync
                NetUtilities.SendDataToAll(new byte[2] { 90 , 0});

                //Destroy
                Destroy(this.gameObject);
                return;
            }


            Debug.Log("[MENU] Will Run New Menu : " + this._tasks[_tasks.Count - 1].mID);
            StartCoroutine(new MenuInstance().OpenMenuAndGetSelection(
                _tasks[_tasks.Count - 1].mID,
                _tasks[_tasks.Count - 1].rMode,
                _tasks[_tasks.Count - 1].sMode,
                _tasks[_tasks.Count - 1].items,
                // new Selector object will not be destroyed because of callback 
                objectindex =>
                {
                    MenuTask task = this._tasks[_tasks.Count - 1];
                    Debug.Log("[MENU] Item selected was : " + objectindex + " on menu Identifier #" + task.mID);
                    this.DoEffectByItemSelection(objectindex, task);
                }));
        }
        public void DoEffectByItemSelection(int itemIndex, MenuTask task)
        {
            if (itemIndex == -1)
            {
                Debug.Log("removing task at # " + (_tasks.Count - 1));
                _tasks.RemoveAt(_tasks.Count - 1);
                RunMenuTask();
                return;
            }
            if (task.mID == 0)
            {
                
                if (itemIndex == 0)
                {
                    Boot b = Camera.main.transform.root.gameObject.GetComponent<Boot>();
                    b.ForceConnectionRoutineToEnd();
                    GameStatus.UnsetGameFlag(13);
                    b.StartCoroutine(b.QuitCyberCaveSafely("App will quit"));
                }
                else if (itemIndex == 1)
                {
                    // Set Free/UnFreeMovement...
                    _tasks.RemoveAt(_tasks.Count - 1);
                    RunMenuTask();
                }
                else if (itemIndex == 2)
                {
                    // TP TO GARDEN
                    Boot b = Camera.main.transform.root.gameObject.GetComponent<Boot>();
                    b.CharacterBootPosition = new Vector3(7.33158f, 0.8072857f, 24.011f);
                    _tasks.RemoveAt(_tasks.Count - 1);
                    RunMenuTask();
                }
                else if (itemIndex == 3)
                {
                    // TP TO SCULPT
                    Boot b = Camera.main.transform.root.gameObject.GetComponent<Boot>();
                    b.CharacterBootPosition = new Vector3(7.33158f, 0.8072857f, 13.69f);
                    _tasks.RemoveAt(_tasks.Count - 1);
                    RunMenuTask();
                }
                else if (itemIndex == 4)
                {
                    // TP TO PAINT
                    Boot b = Camera.main.transform.root.gameObject.GetComponent<Boot>();
                    b.CharacterBootPosition = new Vector3(-6.24f, 0.8072857f, 13.69f);
                    _tasks.RemoveAt(_tasks.Count - 1);
                    RunMenuTask();
                }
                else if (itemIndex == 5)
                {
                    // TP TO CHILL
                    Boot b = Camera.main.transform.root.gameObject.GetComponent<Boot>();
                    b.CharacterBootPosition = new Vector3(-8.184f, 0.8072857f, 2.57f);
                    _tasks.RemoveAt(_tasks.Count - 1);
                    RunMenuTask();
                }
                else if (itemIndex == 6)
                {
                    // TP TO GUEST
                    Boot b = Camera.main.transform.root.gameObject.GetComponent<Boot>();
                    b.CharacterBootPosition = new Vector3(9.15f, 0.8072857f, -1.269f);
                    _tasks.RemoveAt(_tasks.Count - 1);
                    RunMenuTask();
                }
                else if (itemIndex == 7)
                {
                    // CREDIT
                    _tasks.RemoveAt(_tasks.Count - 1);
                    RunMenuTask();
                }


            }
           
        }
    }
    #endregion

    #region Menu
    public class Menu : MonoBehaviour
    {
        public enum DisplayStyle { Circular, Vertical, Horizontal, Linear, Wheel }
        public enum SoundStyle { Plucky, Nighty }

        // Virtual class to access Menu
        public MenuInstance caller;
        public bool _readyToClose;
        public virtual void Create(MenuInstance clr) { caller = clr; }
        public virtual void ProcessInput() { }
        public virtual void Destroy() { }
        public virtual bool IsMenuReadyToClose() { return false; }

    }

    #region Wheel
    public class WheelMenu : Menu
    {
        GameObject center;
        List<Vector3> vecs = new List<Vector3>();
        // Linear Menu works up to 8 objects . The
        public override void Create(MenuInstance clr)
        {
            Debug.Log("Creating Wheel Menu");
            caller = clr;
            center = new GameObject();
            Vector3[] wheelPoints = MathUtilities.GetCirclePoints(caller.choicesize, 2.5f, new Vector3(0, 0, 0));
            for (int i = 0; i < caller.choicesize; i++)
            {
                vecs.Add(new Vector3(wheelPoints[i].x, wheelPoints[i].z, wheelPoints[i].y));
            }
            for (int i = 0; i < caller.LoadedItems.Length; i++)
            {
                if (caller.LoadedItems[i].gameObject != null)
                {
                    caller.LoadedItems[i].gameObject.transform.position = vecs[i];
                    caller.LoadedItems[i].gameObject.transform.parent = center.transform;
                }
            }
            center.name = "center";
            Vector3[] frontvecs = MathUtilities.GetPositionAndEulerInFrontOfPlayer(8f);
            center.transform.position = frontvecs[0];
            Debug.Log("center position is " + frontvecs[0]);
            center.transform.eulerAngles = frontvecs[1];

            Vector3[] frontvecs2 = MathUtilities.GetPositionAndEulerInFrontOfPlayer(0f);
            caller.gUI = ObjectUtilities.InstantiateGameObjectFromAssets("CircularMenuUI");
            caller.gUI.transform.position = frontvecs2[0];
            caller.gUI.transform.eulerAngles = frontvecs2[1];

            // here we need to set y to rig input y pos .... or something like this 
            for (int i = 0; i < caller.LoadedItems.Length; i++)
            {
                if (caller.LoadedItems[i].gameObject != null)
                {
                    vecs[i] = caller.LoadedItems[i].gameObject.transform.position;
                    caller.LoadedItems[i].gameObject.transform.position = center.transform.position; // < - to make it smoothly from center to their position.
                    SmoothMovement m = caller.LoadedItems[i].gameObject.AddComponent<SmoothMovement>();
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

            float raySize = 0.6f;
            for (int i = 0; i < caller.LoadedItems.Length; i++)
            {
                if (caller.LoadedItems[i].gameObject != null)
                {
                    if (HandUtilities.DoesLeftFingerPointingObject_SphereCast(caller.LoadedItems[i].gameObject, raySize, 20f)
                        || HandUtilities.DoesRightFingerPointingObject_SphereCast(caller.LoadedItems[i].gameObject, raySize, 20f)
                        || ControllerData.DoesRightTouchPointingObject_SphereCast(caller.LoadedItems[i].gameObject, raySize, 20f)
                        || ControllerData.DoesLeftTouchPointingObject_SphereCast(caller.LoadedItems[i].gameObject, raySize, 20f)
                        || touched.Contains(caller.LoadedItems[i].gameObject)
                        )
                    {
                        if (caller.obptr != i)
                        {
                            caller.DoHighLightSound();
                            if (caller.obptr>-1)
                                caller.LoadedItems[caller.obptr].DisableHightLight();
                            caller.obptr = i;
                            Debug.Log("setting obptr to " + i);
                            caller.LoadedItems[caller.obptr].HightLight();
                        }

                    }

                }
            }

            // We can also use Input for help 

            for (int i = 0; i < caller.LoadedItems.Length; i++)
            {
                if (caller.LoadedItems[i] != null)
                {
                    if (i == caller.obptr)
                    {
                        caller.LoadedItems[i].gameObject.GetComponent<SmoothMovement>().SetPosition(
                           Vector3.MoveTowards(vecs[i], Camera.main.transform.position, 0.1f)
                           );
                        caller.LoadedItems[i].gameObject.GetComponent<SmoothMovement>().SetScale(new Vector3(1.5f, 1.5f, 1.5f));
                    }
                    else
                    {
                        caller.LoadedItems[i].gameObject.GetComponent<SmoothMovement>().SetPosition(vecs[i]);
                        caller.LoadedItems[i].gameObject.GetComponent<SmoothMovement>().SetScale(new Vector3(1, 1, 1));
                    }
                }
            }
        }
        public override void Destroy()
        {
            // Do a pause animation
            if (caller.obptr > -1)
            {
                caller.LoadedItems[caller.obptr].gameObject.GetComponent<SmoothMovement>().SetPosition(center.transform.position);
            }

            base.Destroy();
        }
    }
    #endregion
    #endregion

    public class MenuItem
    {
        // Item Inside Menu
        public GameObject gameObject;
        public string     meshPath;
        public string     materialPath;
        public bool       InstantiateFromAssets;
        public string     InfoText ="";

        public MenuItem(string mesh, string material, bool fromAssets, string info)
        {
            meshPath = mesh;
            materialPath = material;
            InstantiateFromAssets = fromAssets;
            InfoText = info;
        }
        public void CreateObject()
        {
            // Instantiate the mesh
            if (InstantiateFromAssets)
                gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            else
                gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //Resize
            float s = 2f;
            ObjectUtilities.RescaleMeshToSize(gameObject, s);

            // Set Text
            MenuAnimation m = gameObject.AddComponent<MenuAnimation_Default>();
            m._info = InfoText;

            // Set material (only if  instantiate from assets ? ) 
            //gameObject.GetComponent<Renderer>().material = g.
            
            //Fade In
            ObjectUtilities.FadeInObject(gameObject, 3f);
          
        }
        public void HightLight()
        {
            if (!gameObject)
                return;
            if (gameObject.GetComponent<MenuAnimation>())
                gameObject.GetComponent<MenuAnimation>().SetHighLight();
        }
        public void DisableHightLight()
        {
            if (!gameObject)
                return;
            if (gameObject.GetComponent<MenuAnimation>())
                gameObject.GetComponent<MenuAnimation>().DisableHighLight();
        }

    }

    public class MenuInstance : MonoBehaviour
    {
        // Identifier of the menu
        public int menuID = 0;
        // Display Mode of the menu
        public Menu.DisplayStyle representation_Mode = 0;
        // Sound type   of the menu
        public Menu.SoundStyle sound_Mode = 0;
        // Items in menu
        public MenuItem[] Items;
        public MenuItem[] LoadedItems;
        // Logic 
        public bool _done = false;
        public int choicesize = 0;
        public int cursor = 0;
        public int obptr = -1;
        // Object data
        public Menu menu;
        public GameObject dome;
        public GameObject gUI;

        public IEnumerator OpenMenuAndGetSelection(int mID, Menu.DisplayStyle rMode, Menu.SoundStyle sMode, MenuItem[] items,
            System.Action<int> callback = null
            )
        {
            // UNRENDER ALL EXCEPT AVATAR
            Debug.Log("selector will open ");
            // init variable
            GameStatus.SetGameFlag(2);
            menuID = mID;
            representation_Mode = rMode;
            sound_Mode = sMode;
            Items = items; 
            choicesize = items.Length;
            // Start Creating the room
            CreateRoomObjects();
            DoOpeningSound();
            dome = ObjectUtilities.CreateDome();
            CreateMenuObject();
           
            menu.Create(this);

            SetGUIText();
            Debug.Log("OK");
            float okctr = 0;
            float noctr = 0;
            float second2validated = 1.2f;
            GameObject RadialBar = ObjectUtilities.FindGameObjectChild(gUI, "RadialBar");
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;

            GameObject RadialBar2 = ObjectUtilities.FindGameObjectChild(gUI, "RadialBar2");
            RadialBar2.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;

            while (true)
            {
                if ((PoseRecognition.IsPose_OKSign() || Input.GetMouseButton(0) || ControllerData.GetButtonPressed(ControllerData.Button.A)) && !GameStatus.IsGameFlagSet(1))
                {
                    okctr += Time.deltaTime;
                    noctr = 0;
                }
                else
                {
                    if (okctr - Time.deltaTime >= 0)
                        okctr -= Time.deltaTime;
                }
                if ((PoseRecognition.IsPose_CloseSign() || Input.GetMouseButton(1) || ControllerData.GetButtonPressed(ControllerData.Button.B)) && !GameStatus.IsGameFlagSet(1))
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
                if (!GameStatus.IsGameFlagSet(1))
                    menu.ProcessInput();

                SetGUIText();
                yield return new WaitForEndOfFrame();
            }

            DoValidateSound();
            ClearRoom();
            menu.Destroy();
            // some stuff to wait the animated selected object 
            if (obptr > -1)
            {
                while (LoadedItems[obptr + cursor].gameObject != null)
                {
                    yield return new WaitForEndOfFrame();
                }
            }

            callback(obptr + cursor);
            yield break;
        }
        public void CreateMenuObject()
        {
            switch (representation_Mode)
            {
                case Menu.DisplayStyle.Circular:
                    //menu = new CircularMenu();
                    break;
                case Menu.DisplayStyle.Horizontal:
                    //menu = new HorizontalMenu();
                    break;
                case Menu.DisplayStyle.Vertical:
                    //menu = new VerticalMenu();
                    break;
                case Menu.DisplayStyle.Linear:
                    //menu = new LinearMenu();
                    break;
                case Menu.DisplayStyle.Wheel:
                    menu = new WheelMenu();
                    break;

            }
        }
        public void CreateRoomObjects()
        {

            LoadedItems = new MenuItem[8];

            for (int i = 0; i < 8; i++)
            {
                if (Items.Length > i)
                {
                    LoadedItems[i] = Items[i];
                    LoadedItems[i].CreateObject();
                }
            }
            if (obptr < 0)
                return;
            if (LoadedItems[obptr].gameObject)
                LoadedItems[obptr].HightLight();
        }

        public void ClearRoom()
        {
            if (LoadedItems != null)
            {
                for (int i = 0; i < LoadedItems.Length; i++)
                {
                    if (LoadedItems[i].gameObject != null)
                    {

                        if (i != obptr)
                        {
                            ObjectUtilities.FadeAndDestroyObject(LoadedItems[i].gameObject, 2f);
                        }
                        else
                        {
                            ObjectUtilities.FadeAndDestroyObject(LoadedItems[i].gameObject, 4f);
                        }
                    }
                    else
                    {
                        Debug.Log("Object # " + i + "was null! ");
                    }
                }

            }

            if (dome != null)
            {
                ObjectUtilities.FadeAndDestroyObject(dome, 2f);
            }
            Destroy(gUI.gameObject);
        }

        public void SetGUIText()
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


        public void DoOpeningSound()
        {
            switch (sound_Mode)
            {
                case Menu.SoundStyle.Plucky:
                    SoundMap.PlaySoundAndDisposeFromAssets("val2");
                    break;
                case Menu.SoundStyle.Nighty:
                    break;
            }
        }
        public void DoClosingSound()
        {
            switch (sound_Mode)
            {
                case Menu.SoundStyle.Plucky:
                    SoundMap.PlaySoundAndDisposeFromAssets("val1");
                    break;
                case Menu.SoundStyle.Nighty:
                    break;
            }

        }
        public void DoValidateSound()
        {
            switch (sound_Mode)
            {
                case Menu.SoundStyle.Plucky:
                    SoundMap.PlaySoundAndDisposeFromAssets("val1");
                    break;
                case Menu.SoundStyle.Nighty:
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
                case Menu.SoundStyle.Plucky:
                    int rtiny = Random.Range(1, 5);
                    mysterious_sequence.Add(rtiny);
                    SoundMap.PlaySoundAndDisposeFromAssets("celest" + rtiny.ToString());
                    break;
                case Menu.SoundStyle.Nighty:
                    rtiny = Random.Range(1, 5);
                    mysterious_sequence.Add(rtiny);
                    SoundMap.PlaySoundAndDisposeFromAssets("nighty" + rtiny.ToString());
                    if (mysterious_counter > 100)
                    {
                        SoundMap.PlaySoundAndDisposeFromAssets("nightyplus" + rtiny.ToString());
                    }
                    if (mysterious_counter > 110)
                    {
                        mysterious_counter = 0;
                    }
                    break;
            }


        }
    }


}
