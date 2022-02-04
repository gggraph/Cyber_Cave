using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Utilities : MonoBehaviour
{
    public static uint GetTimeStamp()
    {
        return (uint)(DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1))).TotalSeconds;
    }

    public static DateTime UnixTimeStampToDateTime(uint timestamp) 
    {
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(timestamp);
        return dtDateTime;

    }
    public static void PrintInfo(string s)
    {
        Debug.Log(s);
    }

    public static void FadeOutFadeInScreen(float seconds)
    {

    }
}
