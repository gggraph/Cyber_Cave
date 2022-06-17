using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;

public class XYPlotting : MonoBehaviour
{
    /*
        IO PORTS IS NOT WORKING ON APK BUILD SO WE WILL NEED TO CREATE A CUSTOM TCP SYSTEM THAT DIALOG WITH OTHER SOFTWARE 
        ON THE CURRENT MACHINE.
     
     */
    public static TcpClient Client;
    public static bool ClientCreated = false;
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
            //Client = new TcpClient(ip, Port);
            Client = new TcpClient(ini_ip, port);
            ClientCreated = true;
            return;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

    }

    /*
    public static void TrySendAllLinesFromPaintableObject(Paintable pt)
    {
        if (pt == null || !ClientCreated)
            return;
        List<byte> data = new List<byte>();
        // get all TubeRenderer on pt.
        data.Add(0); // header message who say we are sending all lines from plane object

        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(pt.gameObject.transform.eulerAngles));
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(pt.Lines.Count));
        // also apply euler angles because it's important i guess... 

        foreach (TubeRenderer tbr in pt.Lines)
        {

            Vector3[] lns = tbr.GetPositions();
            BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lns.Length)); //int32
            foreach (Vector3 v in lns)
            {
                BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(v));
            }
        }
        if (data.Count > 64000) // too much data
            return;
        Debug.Log("sending " + data.Count + " bytes");
        TrySendDataToXYSoft(data.ToArray());


    }
    public static void TrySendLinesFromTubeRenderer(TubeRenderer tbr)
    {
        if (tbr == null || !ClientCreated)
            return;
        List<byte> data = new List<byte>();
        data.Add(1); // header message who say we are sending a lines from plane object

        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(tbr.gameObject.transform.eulerAngles));
        Vector3[] lns = tbr.GetPositions();
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes(lns.Length)); //int32
        foreach (Vector3 v in lns)
        {
            BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(v));
        }
        if (data.Count > 64000) // too much data
            return;
        Debug.Log("sending " + data.Count + " bytes");
        TrySendDataToXYSoft(data.ToArray());
    }

    public static void TrySendDataToXYSoft(byte[] data)
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
    */

}
