using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;

public class Plotting : MonoBehaviour
{

    // @ Replacement Script for Old XYPlotting.cs

    public static TcpClient Client;
    public static bool ClientCreated = false;
    public static bool XYPlotterBusy = false;

    public static void InitTCPCommunication()
    {
        try
        {
            int port = 13000;
            string IP = "127.0.0.1";
            string ini_ip = Ini.GetConfigVariable("GENERAL", "XY_IP");
            if (ini_ip.Length > 0)
            {
                IP = ini_ip;
            }
            int.TryParse(Ini.GetConfigVariable("GENERAL", "PORT"), out port);
            Client = new TcpClient(ini_ip, port);
            ClientCreated = true;
            return;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

    }
    public static void SendDataToTCPProgram(byte[] data)
    {
        if (!ClientCreated)
            return;
        try
        {
            NetworkStream stream = Client.GetStream();
            stream.ReadTimeout = 5000;
            stream.Write(data, 0, data.Length);
            stream.Read(data, 0, data.Length); // this could create issue ! 
        }
        catch (System.Exception)
        {

        }

    }

    public static void OnXYPrintingCommandReceived(byte[] data)
    {
        if (!Ini.GetConfigVariable("GENERAL", "IO_READY").Contains("TRUE") && !XYPlotterBusy)
            return;
        if (!ClientCreated)
            InitTCPCommunication();

        byte nmsize = data[1];
        char[] objname = new char[nmsize];
        for (int i = 0; i < nmsize; i++)
            objname[i] = (char)data[2 + i];
        GameObject vObj = GameObject.Find(new string(objname));
        if (!vObj)
            return;
        Paintable pt = vObj.GetComponent<Paintable>();
        if (!pt)
            return;
        pt.PrintTextureThrough3DPlotter();
    }
    public static void On3DrintingCommandReceived(byte[] data)
    {
        if (!Ini.GetConfigVariable("GENERAL", "IO_READY").Contains("TRUE"))
            return;
        if (!ClientCreated)
            InitTCPCommunication();

        byte nmsize = data[1];
        char[] objname = new char[nmsize];
        for (int i = 0; i < nmsize; i++)
            objname[i] = (char)data[2 + i];
        GameObject vObj = GameObject.Find(new string(objname));
        if (!vObj)
            return;
        Moodulable mood = vObj.GetComponent<Moodulable>();
        if (!mood)
            return;
        mood.Try3DPrintThisObject();
    }

}
