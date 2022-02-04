using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
public class CryptoUtilities : MonoBehaviour
{

    public static string GetUniqueName()
    {
        double ts = (double)(DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1))).TotalMilliseconds;
        string hashname = SHAToHex(ComputeSHA(BitConverter.GetBytes(ts)));
        return hashname;
        // hashame will always be equal to 
    }

    public static byte[] ComputeSHA(byte[] msg)
    {

        SHA256 sha = SHA256.Create();
        byte[] result = sha.ComputeHash(msg);
        return result;
    }
    public static string SHAToHex(byte[] bytes)
    {
        StringBuilder hex = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
            hex.AppendFormat("{0:x2}", b);

        return hex.ToString();
    }
    public static byte[] HexToSHA(string hexstr)
    {
        byte[] bytes = new byte[32];
        for (int i = 0; i < 64; i += 2)
            bytes[i / 2] = Convert.ToByte(hexstr.Substring(i, 2), 16);
        return bytes;

    }

    public static byte[] DoFileCheckSum(string fileName)
    {
        byte[] r = new byte[32];
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            using (var stream = System.IO.File.OpenRead(fileName))
            {
                r = sha256.ComputeHash(stream);
                return r;
            }
        }
    }

    public static byte[] SignData(byte[] prkey, byte[] msg)
    {
        RSACryptoServiceProvider _MyPrRsa = new RSACryptoServiceProvider();

        try
        {
            _MyPrRsa.ImportCspBlob(prkey);

            SHA256 sha = SHA256.Create();
            msg = sha.ComputeHash(msg);
            byte[] Signature = _MyPrRsa.SignHash(msg, CryptoConfig.MapNameToOID("SHA256"));
            _MyPrRsa.Clear();
            return Signature;
        }
        catch (Exception e)
        {
            _MyPrRsa.Clear();
            return null;
        }


    }

    public static bool VerifySign(byte[] msg, byte[] pukey, byte[] sign)
    {

        RSACryptoServiceProvider _MyPuRsa = new RSACryptoServiceProvider();
        try
        {
            _MyPuRsa.ImportCspBlob(pukey);
            bool success = _MyPuRsa.VerifyData(msg, CryptoConfig.MapNameToOID("SHA256"), sign);
            if (success)
            {
                _MyPuRsa.Clear();
                return true;
            }
            else
            {
                _MyPuRsa.Clear();
                return false;
            }


        }
        catch (CryptographicException e)
        {
            Debug.Log(e.Message);
            _MyPuRsa.Clear();
            return false;
        }


    }

    public static void GenerateNewPairKey() // better to use offline & on another device. 
    {

        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096);
        byte[] _privateKey = rsa.ExportCspBlob(true);
        byte[] _publicKey = rsa.ExportCspBlob(false);
        File.WriteAllBytes("mstrk", rsa.ExportCspBlob(true));
        File.WriteAllBytes("puk", rsa.ExportCspBlob(false));
        rsa.Clear();
        string s = "";

        foreach (byte b in _publicKey)
        {
            int bb = (int)b;
            s += bb.ToString() + ",";
        }

        File.WriteAllLines("puk.log", new string[1] { s });

    }

}
