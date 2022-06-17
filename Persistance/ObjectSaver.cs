using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ObjectSaver : MonoBehaviour
{
    // @ Add this script to all objects you want to save position / textures for paintable and 3dtexture for moodable
    // @ Be careful about using this script on child object... Parent relative position can be modify. 

    public byte[] GetEntryData()
    {
        
        // Compute hash of name 
        char[] cname = this.name.ToCharArray();
        byte[] nameAsByte = new byte[cname.Length];
        for (int i = 0; i < cname.Length; i++)
            nameAsByte[i] = (byte)cname[i];
        //byte[] hashname = CryptoUtilities.ComputeSHA(nameAsByte); // Will not be used ...

        List<byte> unsignedData = new List<byte>();
        // Get components 
        Paintable ptb = GetComponent<Paintable>();
        Moodulable mdb = GetComponent<Moodulable>();
        // build components flag 
        byte componentflag = 0;
        if (ptb) 
            BinaryUtilities.SetBit(ref componentflag, 0, 1);
        if (mdb)
            BinaryUtilities.SetBit(ref componentflag, 1, 1);
        // Add component flag
        unsignedData.Add(componentflag);
        // Add transform data
        byte[] transformdata = BinaryUtilities.SerializeTransform(this.gameObject.transform);
        unsignedData.AddRange(transformdata);

        if (ptb)
        {
            string hexpath = ptb.SaveRenderTexture();
            unsignedData.AddRange(CryptoUtilities.HexToSHA(hexpath)); // recovering the hex to checksum
            FileInfo f = new FileInfo(ptb.initialTexturePath);
            unsignedData.AddRange(BitConverter.GetBytes((int)f.Length));
        }
        if (mdb)
        {
            string hexpath = mdb.Save();
            unsignedData.AddRange(CryptoUtilities.HexToSHA(hexpath));
            FileInfo f = new FileInfo(mdb.initialSculptPath);
            unsignedData.AddRange(BitConverter.GetBytes((int)f.Length));
        }
        // hash whole 
        byte[] chksum = CryptoUtilities.ComputeSHA(unsignedData.ToArray());
        // craft whole
        List<byte> res = new List<byte>();
        res.Add((byte)nameAsByte.Length);
        res.AddRange(nameAsByte);
        res.AddRange(chksum);
        res.AddRange(unsignedData);

        return res.ToArray();
    }
}
