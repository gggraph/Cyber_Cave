using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogHook : MonoBehaviour
{
    static string myLog = "";
    private string output;
    private string stack;
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;
        myLog = output + "\n" + myLog;
        if (myLog.Length > 5000)
        {
            myLog = myLog.Substring(0, 4000);
        }
        this.GetComponent<UnityEngine.UI.Text>().text = myLog;
    }
}
