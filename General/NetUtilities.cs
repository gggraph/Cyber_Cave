using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetUtilities : MonoBehaviour
{
    // Photon utilitaries. 
    public static PhotonView _mphotonView = null;
    public static NetStream _mNetStream = null;
    public static Photon.Realtime.Player master = null;
    public static void SendDataToAll(byte[] data)
    {
        if (_mphotonView == null)
            return;
        _mphotonView.RPC("ReceiveData", RpcTarget.Others, data);
    }
    public static void SendDataToSpecific(byte[] data, Photon.Realtime.Player p)
    {
        if (_mphotonView == null)
            return;
        _mphotonView.RPC("ReceiveData", p, data);

    }
    public static void SendDataToMaster(byte[] data)
    {
        if (_mphotonView == null || master == null)
            return;
        _mphotonView.RPC("ReceiveData", master, data);
    }

    public static GameObject GetAvatarRootObjectByInfo(PhotonMessageInfo info)
    {
        GameObject[] allAvatar = GameObject.FindGameObjectsWithTag("Avatar");
        foreach (GameObject go in allAvatar)
        {
            if (go.GetComponent<PhotonView>() == info.photonView)
            {
                return go;
            }
        }
        return null;
    }
    public static GameObject GetAvatarRootObjectByView(PhotonView v)
    {
        GameObject[] allAvatar = GameObject.FindGameObjectsWithTag("Avatar");
        foreach (GameObject go in allAvatar)
        {
            if (go.GetComponent<PhotonView>().ViewID == v.ViewID)
            {
                return go;
            }
        }
        return null;
    }
    public static GameObject _myAvatar = null;
    public static GameObject GetMyAvatar()
    {
        if (_myAvatar != null)
        {
            return _myAvatar;
        }
        GameObject[] allAvatar = GameObject.FindGameObjectsWithTag("Avatar");
        foreach (GameObject go in allAvatar)
        {
            if (go.GetComponent<NetStream>() == _mNetStream)
            {
                _myAvatar = go;
                return go;
            }
        }
        return null;
    }

}
