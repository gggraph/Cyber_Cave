using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LogHook : MonoBehaviour
{
    static string myLog = "";
    private string output;
    private string stack;

    public bool onlyError;
    public bool onlyLog;
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        InvokeRepeating("SaveLog", 30, 30);
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type != LogType.Error && onlyError)
            return;
        if (type == LogType.Error && onlyLog)
            return;
        output = logString;
        stack = stackTrace;
        myLog = output + "\n" + myLog;
        if (myLog.Length > 5000)
        {
            myLog = myLog.Substring(0, 4000);
        }
        this.GetComponent<TextMesh>().text = myLog;
    }
    void SaveLog()
    {
        File.WriteAllLines(Application.persistentDataPath + "/logfile.txt", new string[1] { output });
    }
}
