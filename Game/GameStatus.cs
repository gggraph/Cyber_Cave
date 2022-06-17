using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatus : MonoBehaviour
{
    /// <summary>
    /// General Game booleans information.
    /// 
    /// GameFlag !
    /// 
    /// 000         null
    /// 001         a window is opened
    /// 002         menu is running
    /// 003         user is in transform edition mode
    /// 004         user is in painting edition mode
    /// 005         user is connected to  cyber_cave
    /// 008         user is grabbing something with left hand / touch
    /// 009         user is grabbing something with right hand / touch
    /// 010         master node has been found and connected 
    /// 011         user is master node
    /// 012         Update has been received 
    /// 013         Update has been proccessed. user is inside the cyber_cave 
    /// </summary>
    public static byte[] GameFlags = new byte[1000];
  
    public static void SetGameFlag(int p)
    {

        int q = p >> 3; // divide by 8.
        int r = p % 8; // get remainder...
        BinaryUtilities.SetBit(ref GameFlags[q], r, 1);
        DebugFlags(0, 10);
        
    }
    public static void UnsetGameFlag(int p)
    {
        int q = p >> 3;
        int r = p % 8;
        BinaryUtilities.SetBit(ref GameFlags[q], r, 0);
        DebugFlags(0, 10);
    }

    public static bool IsGameFlagSet(int p)
    {
       
        int q = p >> 3;
        int r = p % 8;
        return BinaryUtilities.IsBitSet(GameFlags[q], r);
    }
    public static void DebugByteFlag(byte b)
    {
        string s = "Flags :";
        for (int i = 0; i < 8; i ++ )
        {
            if (BinaryUtilities.IsBitSet(b,i))
                s += "1,";
            else
                s += "0,";
        }
        Debug.Log(s);
    }
    public static void DebugFlags(int start, int range)
    {
        string s = "Flags From [" + start + "-" + (start + range).ToString() + "]:";
        for (int i = start; i < start+range; i++)
        {
            if (IsGameFlagSet(i))
                s += "1,";
            else
                s += "0,";
        }
        Debug.Log(s);
    }
}
