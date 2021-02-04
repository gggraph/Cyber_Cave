using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using Photon.Realtime;
using System.IO; 


public class InputOculus : MonoBehaviour
{
    private InputDevice targetDevice; // right pad
    private InputDevice targetDeviceB; // left pad 
    
    // Start is called before the first frame update
    void Start()
    {

        /* essaie de lire fichier et de ne pas le lire */
        /*
        string path = Application.persistentDataPath + "/test.txt"; 
        if (File.Exists(path)) 
        {
            File.Delete(path);
        }

        File.CreateText(path); 

        if (File.Exists(path)) 
        {
            GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = "fichier trouvé"; 
        
        }
        */
        ConfigureMic(); 
        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);

        if ( devices.Count > 0) 
        {
            targetDevice = devices[0]; 
        }
        InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, devices);

        if (devices.Count > 0)
        {
            targetDeviceB = devices[0];
        }

      

    }

    private AudioSource audio;
    private string _SelectedDevice;

    void ConfigureMic() 
    {
        GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = Microphone.devices.Length.ToString();
        if (Microphone.devices.Length > 0) 
        {
            _SelectedDevice = Microphone.devices[0].ToString();
            audio = GetComponent<AudioSource>();
            audio.clip = Microphone.Start(_SelectedDevice, true, 10, 48000);
            audio.loop = true;
            while (!(Microphone.GetPosition(null) > 0)) { }
            audio.Play();

        }


    }
    float GetAverageVolume()
    {
        float[] data = new float[256];
        float a = 0;
        audio.GetOutputData(data, 0);
        foreach (float s in data)
        {
            a += Mathf.Abs(s);
        }
        return a / 256;
    }

    // Update is called once per frame
    void Update()
    {
        targetDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxisValue);
        if (primary2DAxisValue != Vector2.zero)
        {
           // move towards is better !!! will use only Y axis ... 
           // Vector2 npos = new Vector2(this.transform.position.x, this.transform.position.z);
           //npos += primary2DAxisValue;
           // this.transform.position = new Vector3(npos.x, this.transform.position.y, npos.y);
            if ( primary2DAxisValue.y > 0.02f ) 
            {
                //transform.position -= Vector3.forward * Time.deltaTime * 2f;
                Vector3 nv = transform.position + (Camera.main.transform.forward * Time.deltaTime * 2f);
                transform.position = new Vector3 ( nv.x, transform.position.y, nv.z)  ; 
            }

            if (primary2DAxisValue.y < -0.02f )
            {
                Vector3 nv = transform.position - (Camera.main.transform.forward * Time.deltaTime * 2f);
                transform.position = new Vector3(nv.x, transform.position.y, nv.z);
                // transform.position += Vector3.forward * Time.deltaTime * 2f;
            }

           // GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = primary2DAxisValue.y.ToString();


        }
        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
        targetDeviceB.TryGetFeatureValue(CommonUsages.trigger, out float triggerValueB);

        if ( triggerValue > 0.1f && triggerValueB < 0.1f) 
        {
            transform.position = new Vector3(transform.position.x, this.transform.position.y + 0.1f, this.transform.position.z);
        }

        if (triggerValueB > 0.1f && triggerValue < 0.1f)
        {
            transform.position = new Vector3(transform.position.x, this.transform.position.y - 0.1f, this.transform.position.z);
        }

        if (triggerValueB > 0.1f && triggerValue > 0.1f) // change Camera here using enabled boolean
        {
            GameObject headObject = this.transform.Find("Camera Offset").gameObject.GetComponent<MovSync>().Head; 
            if (headObject!= null) 
            {
                PhotonView photonView = headObject.transform.parent.GetComponent<PhotonView>(); 
                photonView.RPC("SwitchFaceCommand", RpcTarget.All);
            }
        }

        targetDeviceB.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxisValueB);

        if (primary2DAxisValueB != Vector2.zero)
        {
            this.transform.Rotate(0, (primary2DAxisValueB.x*5), 0);
        }

        // get mic volume
        if (audio != null) 
        {
            float sensitivity = 1000;
            float loudness = GetAverageVolume() * sensitivity;
          //  GameObject.Find("DEBUGGUI").GetComponent<UnityEngine.UI.Text>().text = loudness.ToString();

        }
  
    }
}
