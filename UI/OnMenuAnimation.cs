using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OnMenuAnimation : MonoBehaviour
{
    public GameObject MenuIcon;
    public float iconSize = 0.1f;
    public float HeightAboveFeet = 2f;

    void Start()
    {
        if (!MenuIcon)
        {
            MenuIcon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MenuIcon.GetComponent<Renderer>().material.color = Color.blue;
        }
        MenuIcon.transform.localScale = Vector3.one * iconSize;
        // Set its position
        MenuIcon.transform.position = this.transform.position + new Vector3(0, HeightAboveFeet, 0);
        // Set also as child
        MenuIcon.transform.parent = this.transform;
        ObjectUtilities.FadeOutObject(MenuIcon, 1f);
        UnsetAureole();
    }

    void SetAureole()
    {
        ObjectUtilities.FadeInObject(MenuIcon, 1f);
    }
    void UnsetAureole()
    {
        ObjectUtilities.FadeOutObject(MenuIcon, 1f);
    }
    public static void OnMenuIconReceived(byte[] data, PhotonMessageInfo info)
    {
        byte value = data[1];
        GameObject avatar = NetUtilities.GetAvatarRootObjectByInfo(info);
        if (!avatar)
            return;

        OnMenuAnimation sc = avatar.GetComponentInChildren<OnMenuAnimation>();
        if (!sc)
            return;
        if (value == 0)
            sc.UnsetAureole();
        else
            sc.SetAureole();

;    }
}
