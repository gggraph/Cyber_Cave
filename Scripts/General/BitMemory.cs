using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class BitMemory : MonoBehaviour
{

    /*
        user.data structure
            number of user (max 255) 1 byte
            last user connected      1 byte
            user data
                Memories [1000 bytes]
                HandPassword [?]
     
     */
    public static byte[] UserData;
    public static byte userLogged;

    public static void LoadMemoriesOfSpecificUser(byte user)
    {
        if (!File.Exists(Application.persistentDataPath + "/user.data"))
        {
            return;
        }
        UserData = File.ReadAllBytes(Application.persistentDataPath + "/user.data");
        userLogged = user;
    }
    public static void LoadMemoriesFromLatestUser()
    {
        if (!File.Exists(Application.persistentDataPath + "/user.data"))
        {
            CreateDefaultMemories();
        }
        UserData = File.ReadAllBytes(Application.persistentDataPath + "/user.data");
        userLogged = UserData[1];
    }
    public static void CreateDefaultMemories()
    {
        byte[] data = new byte[1002];
        data[0] = 0x1;
        data[1] = 0x1;
        File.WriteAllBytes(Application.persistentDataPath + "/user.data", data);

    }
    public static void SaveMemories()
    {
        if (UserData.Length == 0)
            return;
        File.WriteAllBytes(Application.persistentDataPath + "/user.data", UserData);
    }
    public static void ClearMemoriesOfCurrentUser()
    {
        int boff = 2 + ((int)(userLogged - 1) * 1000);
        for (int i = boff; i < boff+1000; i ++)
        {
            UserData[i] = 0;
        }
        SaveMemories();
        Debug.Log("user data cleared");
    }

    public static void SetMemoriesBit(int p, bool v)
    {
        int boff = 2 + ((int)(userLogged - 1) * 1000);
        int q = p >> 3;
        int r = p % 3;
        if (v)
            BinaryUtilities.SetBit(ref UserData[q + boff], r, 1);
        else
            BinaryUtilities.SetBit(ref UserData[q + boff], r, 0);
    }

    public static bool GetMemoriesBit(int p)
    {
        int boff = 2 + ((int)(userLogged - 1) * 1000);
        int q = p >> 3;
        int r = p % 3;
        return BinaryUtilities.IsBitSet(UserData[q + boff], r);
    }

  
}
