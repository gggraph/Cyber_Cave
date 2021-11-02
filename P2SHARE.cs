using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using System.IO;

public class P2SHARE : MonoBehaviour
{
    /*
     -_-_-_-- Reserved flag --_-_-_-

     [12] File Demand           : Ask for a specific file through the network. Creating new Download object. 
     [13] Upload Candidate Ping : Inform to a file demander that you possess the file. Creating new Upload object. 
     [14] Upload directive      : Send to a file possessor offset and length of bytes needed in the file.
     [15] DLL ERROR             : Destroy Upload object at seeder
     [16] FILE DATA             : Chunk data of a file
     [17] ULL ERROR             : Inform to leecher to set abandonned current cell of 
     */

    public class UploadCell
    {
        public UploadCell(int off, int len, Photon.Realtime.Player N)
        {
            o = off;
            l = len;
            node = N;
            cell_fill_status = 0;
            last_timestamp = Utils.GetTimeStamp();
        }
        public int o { get; set; } // byte address to start uploading
        public int l { get; set; } // data size to upload 
        public int cell_fill_status { get; set; } // current bytes received in this cell
        public Photon.Realtime.Player node { get; set; } // Uploader of this specific cell
        public uint last_timestamp{ get; set; }

    }

    public class DL
    {
        public DL(byte[] chksm, uint fsize, byte type, string fPath)
        {
            filePath = fPath;
            checksum = chksm;
            fileSize = fsize;
            fileType = type;
            _cells = new List<UploadCell>();
            ini_timestamp = Utils.GetTimeStamp();
            _done = false;
        }
        public string filePath { get; set; }
        public byte[] checksum { get; set; }
        public uint fileSize { get; set; }
        public byte fileType { get; set; }
        public uint ini_timestamp { get; set; }
        public bool _done { get; set; }
        public List<UploadCell> _cells { get; set; }
        
    }
    public class UL
    {
        public UL(Photon.Realtime.Player lchr, byte[] chksm, string fPath)
        {
            ini_offset = -1;
            target_len = -1;
            current_offset = -1;
            checksum = chksm;
            filePath = fPath;
            leecher = lchr;

        }
        public byte [] checksum { get; set; }
        public int ini_offset { get; set; }
        public int target_len { get; set; }
        public int current_offset { get; set; }
        public string filePath { get; set; }
        public Photon.Realtime.Player leecher { get; set; }
    }


    // -_-_-_-_-_-_-_-_-_-_-_-_-_-_- Cell Management -_-_-_-_-_-_-_-_-_-_-_-_-_-_-\\
    public static void SendUploaderCellInstructions(DL dl, Photon.Realtime.Player u, int no, int nl)
    {
        /*
            Upload Directive message 
            flag 14            1 byte
            checksum           32 byte
            new offset         4 byte
            new length         4 byte

         */
        List<byte> data = new List<byte>();
        data.Add(14);
        Utils.AddBytesToList(ref data, dl.checksum);
        Utils.AddBytesToList(ref data, BitConverter.GetBytes(no));
        Utils.AddBytesToList(ref data, BitConverter.GetBytes(nl));
        // -> send
        

    }
    public static void GiveNewCell(DL dl, Photon.Realtime.Player u)
    {
        // Does there is an abandonned cell ?
        UploadCell c = GetAbandonnedCell(dl);
        if (c != null && c.cell_fill_status < c.l)
        {
            GiveAbandonnedCell(dl, u, c);
            return;
        }
        // if their is no actual cell 
        CreateNewCellForUploader(dl, u);
    }

    public static void GiveAbandonnedCell(DL dl,Photon.Realtime.Player u, UploadCell c)
    {
        // creating a new cell
        UploadCell N = new UploadCell(c.o + c.cell_fill_status, c.l - c.cell_fill_status, u);
        SendUploaderCellInstructions(dl, u, N.o, N.l);
        dl._cells.Add(N);
        c.l = c.cell_fill_status;
        c.node = u;

    }

    public static UploadCell GetAbandonnedCell(DL dl)
    {
        for (int i = 0; i < dl._cells.Count; i++) 
        {
            if (dl._cells[i].node == null)
            {
                return dl._cells[i];
            }
        }

        return null;
    }

    public static void CreateNewCellForUploader(DL dl, Photon.Realtime.Player u)
    {
        if (dl._cells.Count == 0) // Create and give the first cell
        {
            UploadCell N = new UploadCell(0, (int)dl.fileSize, u);
            dl._cells.Add(N);
            SendUploaderCellInstructions(dl, u, 0, (int)dl.fileSize);
            return;
        }
        else
        {
            // find cell with lowest completion
            UploadCell candidate = dl._cells[0];

            for (int i = 1; i < dl._cells.Count; i++)
            {
                if (dl._cells[i].l - dl._cells[i].cell_fill_status
                    > candidate.l - candidate.cell_fill_status)
                {
                    candidate = dl._cells[i];
                }
            }

            if (candidate.l - candidate.cell_fill_status <= 64000)
            {
                return;
            }
            // Get the missing data
            int gap = candidate.l - candidate.cell_fill_status;
            // lower the gap by 2 
            candidate.l -= gap / 2;
            SendUploaderCellInstructions(dl, candidate.node, -1, candidate.l);
            // create new cell starting at candidate.o + candidate.l for gap/2
            UploadCell N = new UploadCell(candidate.o + candidate.l, gap / 2, u);
            dl._cells.Add(N);
            SendUploaderCellInstructions(dl, u, N.o, N.l);

        }
 


    }
    public static void FreeCell(DL dl, Photon.Realtime.Player u)
    {
        UploadCell Q = GetActiveCellfromUploader(dl,u);
        if (Q != null)
        {
            Q.node = null;
        }
    }
    public static UploadCell GetActiveCellfromUploader(DL dl, Photon.Realtime.Player  u)
    {
        for (int i = 0; i < dl._cells.Count; i++)
        {
            if (dl._cells[i].node == u)
            {
                if (dl._cells[i].cell_fill_status < dl._cells[i].o + dl._cells[i].l)
                    return dl._cells[i];
            }
        }
        return null;
    }
    public static UploadCell GetCellByPosition(DL dl, int pos)
    {
        for (int i = 0; i < dl._cells.Count; i++)
        {
            if (dl._cells[i].o == pos)
            {
                return dl._cells[i];
            }
        }
        return null;
    }


    // -_-_-_-_-_-_-_-_-_-_-_-_-_-_- DLL EVENT  -_-_-_-_-_-_-_-_-_-_-_-_-_-_-\\
    public static string GetDirByType(byte t)
    {
        switch (t)
        {
            case 1:
                return "obj\\";

        }
        return "default\\";
    }


    public static List<DL> _dlls = new List<DL>();

    public static void RequestNewFile(byte[] checksum, int fSize, byte type)
    {
        if (GetDLByChecksum(ref checksum) != null)
            return;

        // create recipient file ... If files is heavy, it will freeze... so need better stuff here
        string sumstr = Utils.SHAToHex(checksum, false);
        string fPath = GetDirByType(type);
        fPath += sumstr;
        File.WriteAllBytes(fPath, new byte[fSize]);

        List<byte> data = new List<byte>();
        data.Add(12);
        data.Add(type);
        Utils.AddBytesToList(ref data, checksum);
        
        //create the dll
        _dlls.Add(new DL(checksum, (uint)fSize, type, fPath));
        

    }

    public static void SendDLLError(ref byte [] chksm, PhotonMessageInfo info )
    {
        List<byte> data = new List<byte>();
        data.Add(15);
        Utils.AddBytesToList(ref data, chksm);
        // send->
    }
    public static void OnSeederJoin(byte[] data, PhotonMessageInfo info)
    {
        /*
         seeder ping struct : 
         flag 13            1o 
         checksum           32o
         */
        byte[] chksm = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            chksm[i] = data[i + 1];
        }
        DL dl = GetDLByChecksum(ref chksm);
        if ( dl == null)
        {
            SendDLLError(ref chksm, info);
            return;
        }
        GiveNewCell(dl, info.Sender); 

    }
    public static void OnSeederLeave(byte[] data, PhotonMessageInfo info)
    {
        byte[] chksm = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            chksm[i] = data[i + 1];
        }
        DL dl = GetDLByChecksum(ref chksm);
        if (dl == null)
        {
            return;
        }
        FreeCell(dl, info.Sender);
    }

    public static void OnDataReceived ( byte[] data, PhotonMessageInfo info)
    {
        /*
         message structure : 
        flag 16                 1b  +0
        checksum                32b +1
        origin offset           4b  +33 
        curr  offset            4b  +37
        chunk size              4b  +41
        data                    r   +45

         */
  
        byte[] chksm = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            chksm[i] = data[i + 1];
        }
        DL dl = GetDLByChecksum(ref chksm);
        if (dl == null)
        {
            SendDLLError(ref chksm, info); // we dont have the DL or already downloaded. Inform the seeder.
            return;
        }
        // copy chunk content .... 
        byte[] chunkdata = new byte[BitConverter.ToInt32(data, 41)];
        for (int i = 45; i < 45 + BitConverter.ToInt32(data, 41); i++)
        {
            chunkdata[i - 45] = data[i];
        }
        int off = BitConverter.ToInt32(data, 33) + BitConverter.ToInt32(data, 37);
        // overwrite
        Utils.OverWriteBytesInFile(dl.filePath, chunkdata, off);

        // update cell status
        // 2 solutions :  GetActiveCellfromUploader() or GetCellByPosition()
        UploadCell c = GetCellByPosition(dl,BitConverter.ToInt32(data, 33));
        if ( c == null)
        {
            // give seeder another cell?
            return;
        }
        c.cell_fill_status += chunkdata.Length;
        c.last_timestamp = Utils.GetTimeStamp();

        // Check if dl is done.
        if ( dl._cells.Count > 0)
        {
            dl._done = true;
            foreach (UploadCell uc in dl._cells)
            {
                if ( uc.cell_fill_status < uc.l)
                {
                    dl._done = false;
                    break;
                }
            }
            if (dl._done)
            {
                // Proccess the file 
                // inform seeder we have enough
                EndDLL(dl);
                SendDLLError(ref chksm, info);
                return;
            }
        }
        
        // 
        if ( c.cell_fill_status >= c.l)
        {
            //cell is done. Create &/or give another cell to seeder
            GiveNewCell(dl, info.Sender);
        }
    }

    public static void CheckCellsPurishment(DL dl)
    {
        uint ts = Utils.GetTimeStamp();
        foreach (UploadCell uc in dl._cells)
        {
            if (uc.last_timestamp  + 12000 < ts 
                || (uc.cell_fill_status < uc.l && uc.node == null) 
                )
            {
                uc.node = null; // abandon the cell
            }
        }
    }

    public static void EndDLL(DL dl)
    {
        // FIle is downloaded. Do something?

        // otherwise just remove the dl from memory
        _dlls.Remove(dl);
    }

    public static DL GetDLByChecksum(ref byte[] chksm)
    {
        foreach ( DL dl in _dlls)
        {
            if ( dl.checksum.Equals(chksm))
            {
                return dl;
            }

        }
        return null;
    }
    /*
-_-_-_-- Reserved flag --_-_-_-

[12] File Demand           : Ask for a specific file through the network. Creating new Download object. 
[13] seeder           Ping : Inform to a file demander that you possess the file.
[14] Upload directive      : Send to a file possessor offset and length of bytes needed in the file.
[15] DLL ERROR             : Destroy Upload object at seeder
[16] FILE DATA             : Chunk data of a file
[17] ULL ERROR             : Inform to leecher to set abandonned current cell of 
*/

    // -_-_-_-_-_-_-_-_-_-_-_-_-_-_- ULL EVENT  -_-_-_-_-_-_-_-_-_-_-_-_-_-_-\\
    public static List<UL> _ulls = new List<UL>();

    // this one need threading ->
    public static void ProccessUpload()
    {
        while ( true)
        {
            for (int i = 0;  i < _ulls.Count; i++)
            {
                if ( _ulls[i].ini_offset == -1)
                {
                    // MISSING DIRECTIVE HERE
                    continue;
                }
                if (_ulls[i].current_offset >= _ulls[i].ini_offset + _ulls[i].target_len)
                {
                    // MY UPLOAD INSTRUCTION HAS BEEN DONE. Wait for another one or a end message.
                    continue; // MY UPLOAD INSTRUCTION HAS BEEN DONE...
                }
                int chunksize = 64000;
                if (_ulls[i].current_offset + chunksize > _ulls[i].ini_offset + _ulls[i].target_len)
                {
                    chunksize = (_ulls[i].ini_offset + _ulls[i].target_len) - _ulls[i].current_offset;
                }
                // fill outbuffer
                byte[] chunkdata = Utils.ReadBytesInFiles(_ulls[i].filePath, _ulls[i].current_offset, chunksize);

                /*
                message structure : 
                flag 16                 1b  +0
                checksum                32b +1
                origin offset           4b  +33 
                curr  offset            4b  +37
                chunk size              4b  +41
                data                    r   +45
    
                */

                // send data to the leecher 
                List<byte> data = new List<byte>();
                data.Add(16);
                Utils.AddBytesToList(ref data, _ulls[i].checksum);
                Utils.AddBytesToList(ref data, BitConverter.GetBytes(_ulls[i].ini_offset));
                Utils.AddBytesToList(ref data, BitConverter.GetBytes(_ulls[i].current_offset));
                Utils.AddBytesToList(ref data, BitConverter.GetBytes(chunksize));
                Utils.AddBytesToList(ref data, chunkdata);
                // -> send to leecher

                // upload current_offset
                _ulls[i].current_offset += chunksize;
            }
        }
    }
    public static void OnFileRequest(byte[] data, PhotonMessageInfo info)
    {
     
        byte[] chksm = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            chksm[i] = data[i + 2];
        }
        string fpath = GetDirByType(data[1]);
        fpath += Utils.SHAToHex(chksm, false);
        if (!File.Exists(fpath))
            return;

        List<byte> msg = new List<byte>();
        msg.Add(13);
        Utils.AddBytesToList(ref msg, chksm);
        //-> send
        // Create the ull 
        _ulls.Add(new UL(info.Sender,chksm, fpath));
    }
    public static void OnDLLError(byte[] data, PhotonMessageInfo info)
    {
        byte[] chksm = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            chksm[i] = data[i + 1];
        }
        UL u = GetULByChecksum(ref chksm);
        if ( u!= null)
        {
            _ulls.Remove(u);
        }

    }

    public static void SendUploadError(ref byte[] chksm, PhotonMessageInfo info)
    {
        List<byte> data = new List<byte>();
        data.Add(17);
        Utils.AddBytesToList(ref data, chksm);
        //-> send 
    }
    public static void OnUploadDirective(byte[] data, PhotonMessageInfo info)
    {

        byte[] chksm = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            chksm[i] = data[i + 1];
        }
        UL u = GetULByChecksum(ref chksm);
        if ( u == null)
        {
            //send ul error // or free the cell
            SendUploadError(ref chksm, info);
            return;
        }
        int no = BitConverter.ToInt32(data, 33);
        int nl = BitConverter.ToInt32(data, 37);

        if (no >= 0)
        {
            u.ini_offset = no;
            u.current_offset = no;
        }
        if (nl >= 0)
        {
            u.target_len = nl;
        }
    }
    public static UL GetULByChecksum(ref byte[] chksm)
    {
        foreach (UL ul in _ulls)
        {
            if (ul.checksum.Equals(chksm))
            {
                return ul;
            }

        }
        return null;
    }
}
