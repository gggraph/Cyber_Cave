using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BasicWindows : MonoBehaviour
{
    public static GameObject CreateOkWindow(string text, int contentType, string assetsName, int soundType) 
    {
        GameObject win = ObjectUtilities.InstantiateGameObjectFromAssets("WinOK");
        GameStatus._WindowIsOpen = true;

        ObjectUtilities.FadeInObject(win, 2f);
        EyeFollower ef = win.AddComponent<EyeFollower>();
        ef.SetDistanceFromCamera(5f);
        ef.EulerOffset = new Vector3(0, 0, 0);
        ef.LROffset = -0.75f; // going more left

        GameObject textObject = ObjectUtilities.FindGameObjectChild(win, "Text");
        textObject.GetComponent<TMP_Text>().text = text;
        if (soundType > 0)
        {
            SoundMap.PlaySoundAndDisposeFromAssets("nightyplus" + soundType);
        }
        return win;
    }
    public IEnumerator CreateAndRunOkWindow (string text, int contentType, string assetsName, int soundType, System.Action<bool> callback = null)
    {

        GameObject win = ObjectUtilities.InstantiateGameObjectFromAssets("WinOK");
        if ( win == null )
        {
            callback(false);
            yield break;
        }
        GameStatus._WindowIsOpen = true;
        ObjectUtilities.FadeInObject(win, 2f);
        EyeFollower ef = win.AddComponent<EyeFollower>();
        ef.SetDistanceFromCamera(5f);
        ef.EulerOffset = new Vector3(0, 0, 0);
        ef.LROffset = -0.75f; // going more left
       
        GameObject textObject = ObjectUtilities.FindGameObjectChild(win, "Text");
        textObject.GetComponent<TMP_Text>().text = text;
        if ( soundType > 0)
        {
            SoundMap.PlaySoundAndDisposeFromAssets("nightyplus" + soundType);
        }
        float okctr = 0;
        float second2validated = 2f;
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
       
        GameStatus._WindowIsOpen = false;
        Destroy(win.gameObject);
        callback(true);
        yield break;

    }

    public IEnumerator RunOKWindow(string text, int contentType, string assetsName, int soundType) 
    {
        yield break;
    }
}
