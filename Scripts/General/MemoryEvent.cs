using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryEvent : MonoBehaviour
{
    public IEnumerator Routine_000()
    {
        GameObject dome = ObjectUtilities.CreateDome();

        float okctr = 0;
        float second2validated = 2f;
        GameObject win = BasicWindows.CreateOkWindow("Hello dear User! " + System.Environment.NewLine + "It seems it is your first time in CyberCave", 0, "", 1);
        GameObject RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);
        win = BasicWindows.CreateOkWindow("Let me Explain you some stuff", 0, "", 2);
        RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);
        win = BasicWindows.CreateOkWindow("Everything here is interactable with your hands", 0, "", 3);
        RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);
        GameStatus._WindowIsOpen = false; // set to false to make global menu interactable
        GlobalMenu.OpenSpecificMenu(new GlobalMenu.MenuTask(7, 3, 1, 0));
        ObjectUtilities.FadeAndDestroyObject(dome, 2f);
        yield return new WaitForSeconds(5f);
        win = BasicWindows.CreateOkWindow("This is an interactable menu. " + System.Environment.NewLine + "Try to point something", 0, "", 1);
        RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);
        // wait ok is down here ... 
        while (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0)) { yield return new WaitForEndOfFrame(); }
        GameStatus._WindowIsOpen = false; // set to false to make global menu interactable
        yield return new WaitForSeconds(5f);

        win = BasicWindows.CreateOkWindow("When item is selected by pointing it, " + System.Environment.NewLine + "Do OK sign to validate your choice", 0, "", 1);
        RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);

        win = BasicWindows.CreateOkWindow("Validate <Enter Last Room>", 0, "", 4);
        RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);
        GameStatus._WindowIsOpen = false; // set to false to make global menu interactable

        yield break;
    }

    public IEnumerator Routine_001() 
    {
        yield return new WaitForSeconds(5f);
        float okctr = 0;
        float second2validated = 2f;
        GameObject win = BasicWindows.CreateOkWindow("Well... " + System.Environment.NewLine + "It's time to say...", 0, "", 1);
        GameObject RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);
        win = BasicWindows.CreateOkWindow("WELCOME TO THE CYBERCAVE!!!!", 0, "", 1);
        RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);
        win = BasicWindows.CreateOkWindow("It's your first time, so i will need to explain you the global menu", 0, "", 1);
        RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);
        win = BasicWindows.CreateOkWindow("Do a circle with your hands ;)", 0, "", 1);
        RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);
        GameStatus._WindowIsOpen = false;
        BitMemory.SetMemoriesBit(1, true);
        yield break;
    }
    public IEnumerator Routine_002()
    {
        yield return new WaitForSeconds(2f);
        float okctr = 0;
        float second2validated = 2f;
        GameObject win = BasicWindows.CreateOkWindow("Bravo!" + System.Environment.NewLine + "This is the global menu", 0, "", 1);
        GameObject RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);
        win = BasicWindows.CreateOkWindow("I cannot tell you more because my writer has not finished me yet :'D", 0, "", 1);
        RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);
        win = BasicWindows.CreateOkWindow("So i will let you explore it and free you from my blablabla", 0, "", 1);
        RadialBar = ObjectUtilities.FindGameObjectChild(win, "RadialBar");
        RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (true)
        {
            if (HandRecognition.IsPose_OKSign() || Input.GetMouseButton(0))
                okctr += Time.deltaTime;
            else
            {
                if (okctr - Time.deltaTime >= 0)
                    okctr -= Time.deltaTime;
            }
            if (okctr >= second2validated)
            {
                okctr = 0;
                break;
            }
            RadialBar.GetComponent<UnityEngine.UI.Image>().fillAmount = (float)(okctr) / second2validated;

            yield return new WaitForEndOfFrame();
        }
        Destroy(win.gameObject);
        GameStatus._WindowIsOpen = false; 
        BitMemory.SetMemoriesBit(2, true);
        yield break;
    }
}
