using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetMaster : MonoBehaviourPunCallbacks
{
    private string _RoomName = "CyberCave";
    public GameObject PlayerPrefab;

    /*
     Ficher de Sauvegarde automatique
     * ma derniere position
     */
    private void Start()
    {
        // for testing purpose
        _RoomName = "CyberCave";
        PhotonNetwork.GameVersion = "0.0.1";
        PhotonNetwork.ConnectUsingSettings();

    }
    public void Init(string roomInfo = "CyberCave")
    {

        _RoomName = roomInfo;
        PhotonNetwork.GameVersion = "0.0.1";
        PhotonNetwork.ConnectUsingSettings();

    }
    public void InstantiateAvatar()
    {
        // Cherche un prefab appelé player dans /Resources
        PhotonNetwork.Instantiate("Avatar 2", new Vector3(0, 0, 0), Quaternion.identity, 0);

    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master.");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 6;

        PhotonNetwork.JoinOrCreateRoom(_RoomName, options, TypedLobby.Default);


        DestroyRoomPrefab();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room created!");
        base.OnCreatedRoom();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room creation failed : " + message);
        base.OnCreateRoomFailed(returnCode, message);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Room successfully joined!");
        InstantiateAvatar();
        base.OnJoinedRoom();
    }



    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Joining room failed :  " + message);
        base.OnJoinRoomFailed(returnCode, message);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo inf in roomList)
        {
            Debug.Log(inf.Name);
            Debug.Log(inf.MaxPlayers);
            Debug.Log(inf.IsOpen);
            Debug.Log(inf.IsVisible);
        }
        base.OnRoomListUpdate(roomList);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby successfully! ");
        base.OnJoinedLobby();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from server because " + cause.ToString());
        base.OnDisconnected(cause);
    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Joining " + _RoomName + " failed ! ");
        base.OnJoinRandomFailed(returnCode, message);
    }

    void DestroyRoomPrefab()
    {
       // Destroy(GameObject.Find("RoomInfo").gameObject);

    }

    [PunRPC]
    void SwitchFaceCommand(PhotonMessageInfo pif)
    {
        GameObject[] allplayer = GameObject.FindGameObjectsWithTag("Avatar"); 
        foreach ( GameObject p in allplayer) 
        { 
            if ( p.GetComponent<PhotonView>() == pif.photonView) 
            {
                GameObject GaelFace = p.transform.Find("GAEL").gameObject;
                GameObject LionelFace = p.transform.Find("LIONEL").gameObject;

                if (LionelFace.activeInHierarchy)
                {
                    LionelFace.SetActive(false);
                    GaelFace.SetActive(true);
                    return;

                }
                else
                {
                    LionelFace.SetActive(true);
                    GaelFace.SetActive(false);
                    return;
                }

                
            }
        
        }

    }

}

