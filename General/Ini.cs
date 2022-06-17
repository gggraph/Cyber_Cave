using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; 

public class Ini : MonoBehaviour
{
    public static string[] ini;
    public static void GetConfig()
    {
        string path = Application.persistentDataPath + "/config.ini";
        if (!File.Exists(path))
        {
            File.WriteAllLines(path, new string[1] { "" });
        }
        ini = File.ReadAllLines(Application.persistentDataPath + "/config.ini");
    }

    public static string[] GetConfigRegion(string regionname)
    {
        List<string> result = new List<string>();
        if (ini == null)
            return result.ToArray();
        
        bool startf = false;
        foreach ( string  s in ini)
        {
            string cl = s.Replace(" ", "");
            cl = cl.Replace("\n", " ");
            if (startf && cl.Length>0)
            {
                if ( cl[0] == '#')
                    break;
            }
                
            if (startf)
                result.Add(cl);

            if (cl == "#" + regionname)
                startf = true;
        }
        return result.ToArray();
    }

    public static string[] GetConfigRegion_Default(string regionname)
    {
        if (ini == null)
            return null;
        List<string> result = new List<string>();
        bool startf = false;
        foreach (string s in ini)
        {
            string cl = s.Replace(" ", "");
            cl = cl.Replace("\n", " ");
            if (startf && cl.Length > 0)
            {
                if (cl[0] == '#')
                    break;
            }
            if (startf)
                result.Add(s);
            if (cl == "#" + regionname)
                startf = true;
        }
        return result.ToArray();
    }

  
    public static string FastGetConfigVariable(string variablename)
    {
        foreach ( string s in ini)
        {
            string cl = s.Replace(" ", "");
            cl = cl.Replace("\n", " ");
            string[] sp = cl.Split('=');
            if ( sp.Length > 1 && sp[0] == variablename)
            {
                return sp[1];
            }
        }
        return "";
    }

    public bool FastSetConfigVariable(string variablename, string newvalue)
    {
        for ( int i = 0; i < ini.Length; i++)
        {
            string cl = ini[i].Replace(" ", "");
            cl = cl.Replace("\n", " ");
            string[] sp = cl.Split('=');
            if (sp.Length > 1 && sp[0] == variablename)
            {
                ini[i] = ini[i].Replace(sp[1], newvalue);
                SaveConfig();
                return true;
            }
        }
        return false;
    }
    public void SaveConfig()
    {
        File.WriteAllLines(Application.persistentDataPath + "/config.ini", ini);
    }


    public static string GetVariableValueInRegion ( string[] region, string variablename)
    {
        foreach( string s in region)
        {
            string[] sp = s.Split('=');
            if (sp.Length > 1 && sp[0] == variablename)
            {
                return sp[1];
            }
        }
        return "";
    }
     public static string GetConfigVariable(string regionname, string variablename)
     {
        string[] region = GetConfigRegion(regionname);
        if (region.Length == 0)
            return "";

        return GetVariableValueInRegion(region, variablename);

     }
}
