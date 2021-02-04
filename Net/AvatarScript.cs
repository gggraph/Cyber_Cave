using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class AvatarScript : MonoBehaviour
{

    public bool _isMine = false;
    public string _nickname = "Gael";


    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            _isMine = true;
        }
    }
    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // e.g. store this gameobject as this player's charater in Player.TagObject
        info.Sender.TagObject = this.gameObject;

    }

   
    
}
