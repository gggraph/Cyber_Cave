using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GlobalMenu : MonoBehaviour
{
    public class MenuTask
    {
        public int mID { get; }
        public int rMode { get; }
        public int sMode { get; }
        public int dMode { get; }

        public MenuTask ( int menu_id, int representationMode, int soundMode, int domeMode)
        {
            mID = menu_id;
            rMode = representationMode;
            sMode = soundMode;
            dMode = domeMode;
        }
    }
    public List<MenuTask> _tasks = new List<MenuTask>();
    public static void OpenGlobalMenu()
    {
        Debug.Log("MENU IS OPEN ? " + 
            GameStatus._selectorIsOpen);
        if (!GameStatus._selectorIsOpen)
        {
            GameStatus._selectorIsOpen = true;
            GameObject ghost = new GameObject();
            GlobalMenu n = ghost.AddComponent<GlobalMenu>();
            n._tasks.Add(new MenuTask(0, 4, 0, 0));
            ObjectUtilities.FadeOutObjectsAround(Camera.main.transform.position, 20f, 2f);
            n.RunMenuTask();
        }

    }
    public static void OpenSpecificMenu(MenuTask menuTask)
    {
        Debug.Log("MENU IS OPEN ? " +
    GameStatus._selectorIsOpen);
        if (!GameStatus._selectorIsOpen)
        {
            GameStatus._selectorIsOpen = true;
            GameObject ghost = new GameObject();
            GlobalMenu n = ghost.AddComponent<GlobalMenu>();
            n._tasks.Add(menuTask);
            ObjectUtilities.FadeOutObjectsAround(Camera.main.transform.position, 20f, 2f);
            n.RunMenuTask();
        }
    }
    
    public void RunMenuTask()
    {
        if (_tasks.Count == 0)
        {
            GameStatus._selectorIsOpen = false;
            ObjectUtilities.FadeInObjectsAround(20f, 2f); // Ok this put bad stuff here 
            Debug.Log("CLOSING MENU");
            Destroy(this.gameObject);
            return;
        }
        

        Debug.Log("[MENU] Will Run New Menu : " + this._tasks[_tasks.Count - 1].mID);
        StartCoroutine(new Selector().OpenMenuAndGetSelection(
            _tasks[_tasks.Count-1].mID, 
            _tasks[_tasks.Count - 1].rMode, 
            _tasks[_tasks.Count - 1].sMode, 
            _tasks[_tasks.Count - 1].dMode, 
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

        if  ( itemIndex == -1)
        {
            Debug.Log("removing task at # " + (_tasks.Count - 1));
            _tasks.RemoveAt(_tasks.Count - 1);
            RunMenuTask();
            return;
        }

        // -------------------------------- GLOBAL MENU ------------------------------ \\
        if ( task.mID == 0)
        {
            if (itemIndex == 0) // Open Reducto world
            {
                _tasks = new List<MenuTask>();
                Reducto.TryRunReducto();
                RunMenuTask();
                return;

            }
            if (itemIndex == 1) // Open Tool Menu
            {
               // do not remove task cause we want to get back to previous menu if needed
                _tasks.Add(new MenuTask(6, 3, 1, 0));
                Debug.Log("Adding new menu task. ID #6");
                RunMenuTask();
                return;
            }
            if ( itemIndex == 2) // Instantiate Mesh ( choose Primitive or Custom ) 
            {
                // do not remove task cause we want to get back to previous menu if needed
                _tasks.Add(new MenuTask(5, 3, 1, 0));
                RunMenuTask();
                return;
            }

        }
        if ( task.mID == 4) 
        {
            _tasks = new List<MenuTask>(); // clear all tabs
            Vector3[] frontvecs = MathUtilities.GetPositionAndEulerInFrontOfPlayer(1f);
            NetInstantiator.InstantiatePrimitiveMesh((byte)(itemIndex+1), frontvecs[0], new Vector3(0,0,0), new Vector3(1,1,1));
            RunMenuTask();
            return;
        }
        // Instantiating Sound ! 
        if ( task.mID == 3)
        {
            string logpath = P2SHARE.GetDirByType(4) + "chksmlog.txt";
            string[] files = File.ReadAllLines(logpath);
            if (files.Length > itemIndex)
            {
                _tasks = new List<MenuTask>();
                Debug.Log("WILL INSTANTIATE FILES : " + files[itemIndex]);
                Vector3[] frontvecs = MathUtilities.GetPositionAndEulerInFrontOfPlayer(1f);
                NetInstantiator.TryInstantiateSoundBox(files[itemIndex], frontvecs[0], new Vector3(0, 0, 0), new Vector3(1, 1, 1), 0);
            }
            RunMenuTask();
            return;
        }
        // Instantiating custom mesh ! 
        if ( task.mID == 1) 
        {

            // get the string of the file... 
            string logpath = P2SHARE.GetDirByType(1) + "chksmlog.txt";
            string[] files = File.ReadAllLines(logpath);
            if ( files.Length > itemIndex)
            {
                _tasks = new List<MenuTask>();
                Debug.Log("WILL INSTANTIATE FILES : " + files[itemIndex]);
                Vector3[] frontvecs = MathUtilities.GetPositionAndEulerInFrontOfPlayer(1f);
                NetInstantiator.TryInstantiateCustomMesh(files[itemIndex], frontvecs[0], new Vector3(0, 0, 0), new Vector3(1, 1, 1));
               
          
            }
            RunMenuTask();
            return;
        }
        // -------------------------------- MENU 5  ------------------------------ \\
        if ( task.mID == 5)
        {
            if ( itemIndex == 0) // instantiate primitive (4) 
            {
                // do not remove task cause we want to get back to previous menu if needed
                _tasks.Add(new MenuTask(4, 0, 1, 0));
                RunMenuTask();
                return;
            }
            if (itemIndex == 1) // TryInstantiate Menu
            {
                // do not remove task cause we want to get back to previous menu if needed
                _tasks.Add(new MenuTask(1, 0, 1, 0));
                RunMenuTask();
                return;
            }
            if (itemIndex == 2) // TryInstantiate Sound
            {
                // do not remove task cause we want to get back to previous menu if needed
                _tasks.Add(new MenuTask(8, 3, 1, 0));
                RunMenuTask();
                return;
            }
        }
        if ( task.mID == 6)
        {
            if (itemIndex == 0) // instantiate primitive (4) 
            {
                // do not remove task cause we want to get back to previous menu if needed
                _tasks = new List<MenuTask>();
                ToolMod.value = itemIndex;
                Debug.Log("Editor Mode set to Transform!!!");
                RunMenuTask();
                return;
            }
            if (itemIndex == 1)
            {
                _tasks = new List<MenuTask>();
                return;
            }
            if (itemIndex == 2)
            {
                _tasks = new List<MenuTask>();
                ToolMod.value = itemIndex;
                RunMenuTask();
                Debug.Log("Editor Mode set to Painting!!!");
                return;
            }
        }
        if ( task.mID == 7) 
        {
            if ( itemIndex == 0) // explore memories
            {
        
            }
            if ( itemIndex == 1 ) // connect to last room ... 
            {
                if (!BitMemory.GetMemoriesBit(1)) 
                {
                    
                    MemoryEvent m = Camera.main.transform.root.gameObject.GetComponent<MemoryEvent>();
                    m.StartCoroutine(m.Routine_001());
                }
                _tasks = new List<MenuTask>();
                Camera.main.transform.root.GetComponent<Boot>().ConnectToCYBERCAVE(); // seems to crash here ?
                RunMenuTask();
                return;

            }
            if ( itemIndex == 2) // create new room 
            {
            
            }
        
        }
        if (task.mID == 8 )
        {
            if ( itemIndex == 0)
            {
                _tasks.Add(new MenuTask(3, 0, 1, 0));
                RunMenuTask();
                return;
            }
            else
            {
                _tasks.RemoveAt(_tasks.Count - 1);
                RunMenuTask();
                return;
            }
        }
        _tasks.RemoveAt(_tasks.Count - 1);
        RunMenuTask();
        return;
        // close if needed

    }
}
