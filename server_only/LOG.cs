using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LOG : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject canv;
    void Start()
    {
        InvokeRepeating("PrintState", 3, 3);
        canv = GameObject.Find("SERVINFO");
    }

    void PrintState() 
    {
        if (canv == null)
            return;

        int numberuser = GameObject.FindGameObjectsWithTag("Avatar").Length;
        string s = "[info]";
        if ( numberuser == 0)
        {
            s += "\r\n";
            s += "Server is Offline";
        }
        else
        {
            s += "\r\n";
            s += "Server is Online";
            s += "\r\n";
            s += "Number of user connected : " + (numberuser - 1).ToString();

        }
        canv.GetComponent<UnityEngine.UI.Text>().text = s;
    }
}
