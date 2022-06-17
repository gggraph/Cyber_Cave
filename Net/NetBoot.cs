using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetBoot : MonoBehaviourPunCallbacks
{

    private string _RoomName = "CyberCave";
    public GameObject PlayerPrefab;
 
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
        GameObject av = PhotonNetwork.Instantiate("Avatar", new Vector3(0, 0, 0), Quaternion.identity, 0);

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
    // @ Alert with UI If master
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer == NetUtilities.master)
        {
            Debug.LogError("MASTER LEAVE!!");
            NetUtilities.master = null;
            GameStatus.UnsetGameFlag(10);
            GameStatus.UnsetGameFlag(13);
            // Send UI INFO & leave
            Boot b = Camera.main.transform.root.gameObject.GetComponent<Boot>();
            b.CharacterBootPosition = Camera.main.transform.root.transform.position;
            b.ForceConnectionRoutineToEnd();
            if (!b.ByPassMaster)
                b.StartCoroutine(b.ConnectToCyberCaveThroughMaster(true));
        }
        base.OnPlayerLeftRoom(otherPlayer);
    }
    // @ Alert with UI
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from server because " + cause.ToString());
        
        Boot b = Camera.main.transform.root.gameObject.GetComponent<Boot>();
        b.ForceConnectionRoutineToEnd();
        GameStatus.UnsetGameFlag(13);
        b.StartCoroutine(b.QuitCyberCaveSafely("you have been disconnected. App will quit"));

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

}
