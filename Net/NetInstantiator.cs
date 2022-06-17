using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System;

public class NetInstantiator : MonoBehaviour
{

    /*
     
        Instantiation avec permission : 


	[ULL]
	Lors de l'utilisation de sons, textures ou de Meshs customisé, 
	le client doit obtenir la permission du serveur à l'Update. 
	Il envoie une demande de permission d'un type de message avec le fichier en question a uploadé.
	Si le serveur donne le message de permission, le client réalise le message embarqué à tout le monde.
	Lorsque le client envoie une demande de permission, il place le message dans la permission queue.
	La Permission Queue va réitérer le message toutes les x secondes.
	
	[DLL]   
	    Lorsque un DL se crée, ( à partir de requestnewfile) un objet missingmessageInfo est attaché au DL. Il contient le pointeur du DL en question, le FileDemandMessage.
        Le message Unprocessed est à créer à postiriori ou sinon en argument dans la fonction RequestNewFile.
        Un DLL fait toujours suite à un message non procédé. Ce message lorsqu'il est traité, soit crée un nouveau DL, soit crée une autre sortie. 

    [UL] 
        Lorsque le client demande une permission, il embarque un message a traité si il obtient cette dite permission. La permission est offerte a la fin de DL du serveur
        si un telechargement a été nécessaire.
     */

    public class MissingMessageInfo
    {
        public NetStream.DefaultRPCMessage FileDemandMessage { get; set; }
        public NetStream.DefaultRPCMessage UnProccessedMessage { get; set; }
        public P2SHARE.DL DLL { get; set; }

        public MissingMessageInfo(NetStream.DefaultRPCMessage fdemand, NetStream.DefaultRPCMessage unpromsg, P2SHARE.DL dll )
        {
            FileDemandMessage = fdemand;
            UnProccessedMessage = unpromsg;
            DLL = dll;
        }
    }


    /*
        Eg. of 2 type of msg asking the same stuff :
        Instantiating a custom Mesh. 
        Adding a custom texture to object. 
        Adding a custom voice to avatar. 
        
        AskPermission structure: 
        [8]
        file type  1o
        checksum   32o
        filelength 4o 
        Embedded msg relative [obtain the message type as first byte] 
        
     */


    public static void InstantiateCustomMeshFromMenu()
    {
        if ( !GameStatus.IsGameFlagSet(2))
        {
            GameObject ghost = new GameObject();
            NetInstantiator n = ghost.AddComponent<NetInstantiator>();
            n.SelectCustomMeshToInstantiate();
        }
     
    }
    // this function self-destruct after object call 
    public void SelectCustomMeshToInstantiate()
    {
        /*
        StartCoroutine(new Selector().OpenMenuAndGetSelection(  1, 0, 0, 0,
            // new Selector object will not be destroyed because of callback 
            objectindex =>
            {
                Debug.Log("selected index was " + objectindex);
                Debug.Log("proccess end. Object will be destroyed");
                // Ok then create the object from index . 
                string logpath = P2SHARE.GetDirByType(1) + "chksmlog.txt";
                string[] files = null;
                if (File.Exists(logpath))
                    files = File.ReadAllLines(logpath);

                if (files == null)
                {
                    Destroy(this.gameObject);
                    return;
                }

                GameObject g = new OBJLoader().Load(files[objectindex]);
                ObjectUtilities.FadeInObject(g, 3f);
                float s = MathUtilities.GetMaxSpaceTakenByBounds(ObjectUtilities.GetBoundsOfGroupOfMesh(g));
                // max mesh size is 10 ... So set it to 
                float targetsize = 3f;
                float prct = targetsize / s;
                if (prct < 1f)
                {
                    g.transform.localScale = new Vector3(prct, prct, prct);
                }
                Vector3 nv = Camera.main.transform.position + Camera.main.transform.forward * 3f;
                Vector3 frontpos = new Vector3(nv.x, 0, nv.z);
                g.transform.position = frontpos;
                AutoRotate ar = g.AddComponent<AutoRotate>();
                ar.SetSpeed(0.5f);
                StartCoroutine(new BasicInteraction().ResizeObjectWithHands(g, spscale =>
                {
                    // here set up the whole stuff
                   
                }));
                Destroy(this.gameObject);

            })

            );
        */
    }
    public static void InstantiatePrimitiveMesh(byte primitiveType, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        // append embedded msg 
        /*
         msg struct : 
         msg flag         1o +0
         primitive Type   1o +1 
         hash name        64o +2 
         checksum of file 32o (depend) +66
         file length      4o  (depend) +98
         transform        36o +102 or +66

      */
        List<byte> data = new List<byte>();
        data.Add(3);
        data.Add(primitiveType); // It is a custom Mesh so put 0 here ...
        string n = CryptoUtilities.GetUniqueName();
        foreach (char c in n.ToCharArray())
        {
            data.Add((byte)c);
        }
     
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(pos));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(rot));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(scale));

        NetUtilities.SendDataToAll(data.ToArray());
        NetUtilities._mNetStream.ReceiveData(data.ToArray(), new PhotonMessageInfo());
    }
    public static void TryInstantiateSoundBox(string filePath, Vector3 pos, Vector3 rot, Vector3 scale, byte soundConfig ) 
    {

        FileInfo ff = new FileInfo(filePath);
        List<P2SHARE.UL> uls = P2SHARE.GetULsByFilePath(ff.FullName);
        foreach (P2SHARE.UL u in uls)
        {
            if (u.leecher == NetUtilities.master)
            {
                Debug.Log("already uploading...");
                return;
            }
        }


        List<byte> data = new List<byte>();

        // build ask permission
        data.Add(8);
        data.Add(4); // dir type is 4 
        byte[] chksm = CryptoUtilities.HexToSHA(ff.Name); //Utils.DoFileCheckSum(filePath); DO NOT RECOMPUTE FILECHECKSUM. SUMLOG IS ALL ABOUT NOT DOING IT AGAIN AND AGAIN...
        BinaryUtilities.AddBytesToList(ref data, chksm);
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes((int)ff.Length));

        /*
            msg struct : 
            msg flag         1o +0
            primitive type   1o +1 
            hash name        64o +2 
            checksum of file 32o (depend) +66
            file length      4o  (depend) +98
            transform        36o +102 or +66
        */
        data.Add(30);
        data.Add(soundConfig); // put 0 as soundboxtype
        string n = CryptoUtilities.GetUniqueName();
        foreach (char c in n.ToCharArray())
        {
            data.Add((byte)c);
        }
        BinaryUtilities.AddBytesToList(ref data, chksm);
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes((int)ff.Length));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(pos));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(rot));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(scale));


        NetUtilities.SendDataToMaster(data.ToArray());
        PlaceHolder.CreatePlaceHolderFromWaitingPermission(data.ToArray());
    }
    public static void TryInstantiateCustomTexture(string filePath, GameObject go)
    {
        FileInfo ff = new FileInfo(filePath);
        List<P2SHARE.UL> uls = P2SHARE.GetULsByFilePath(ff.FullName);
        foreach (P2SHARE.UL u in uls)
        {
            if (u.leecher == NetUtilities.master)
            {
                Debug.Log("already uploading...");
                return;
            }
        }
        Debug.Log("try uploading new texture...");
        /*

  structure : 
      header +0
      taille du char arr du nom de go +1 
      nom du gameobject (this)
      primitive Type   1o  
      checksum of file 32o (depend) 
      file length      4o  (depend) 

*/
        List<byte> data = new List<byte>();
        // DEMAND PERMISSION : 
        data.Add(8);
        data.Add(3);
        byte[] chksm = CryptoUtilities.HexToSHA(ff.Name); 
        BinaryUtilities.AddBytesToList(ref data, chksm);
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes((int)ff.Length));
        // MSG TO EMBEDDED : 
        data.Add(20);                        // header of set texture message
        data.Add((byte)go.name.ToCharArray().Length);
        for (int i = 0; i < go.name.ToCharArray().Length; i++)
        {
            data.Add((byte)go.name.ToCharArray()[i]);
        }
        data.Add(0);  // its a custom texture so set 0. 
        BinaryUtilities.AddBytesToList(ref data, chksm);
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes((int)ff.Length));

        NetUtilities.SendDataToMaster(data.ToArray());
        PlaceHolder.CreatePlaceHolderFromWaitingPermission(data.ToArray());

    }
    public static void TryInstantiateCustomMesh(string filePath, Vector3 pos, Vector3 rot, Vector3 scale )
    {
        // DO I HAVE TO PUT THE CHECKCSUM
        // dont ask if we are currently uploading the file to master node
        FileInfo ff = new FileInfo(filePath);
        List<P2SHARE.UL> uls = P2SHARE.GetULsByFilePath(ff.FullName); 
        foreach (P2SHARE.UL  u in uls)
        {
            if ( u.leecher == NetUtilities.master)
            {
                Debug.Log("already uploading...");
                return;
            }
        }
        
       
        List<byte> data = new List<byte>();

        // build ask permission
        data.Add(8);
        data.Add(1);
        byte[] chksm = CryptoUtilities.HexToSHA(ff.Name); //Utils.DoFileCheckSum(filePath); DO NOT RECOMPUTE FILECHECKSUM. SUMLOG IS ALL ABOUT NOT DOING IT AGAIN AND AGAIN...
        BinaryUtilities.AddBytesToList(ref data, chksm);
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes((int) ff.Length));

        // append embedded msg 
        /*
         msg struct : 
         msg flag         1o +0
         primitive Type   1o +1 
         hash name        64o +2 
         checksum of file 32o (depend) +66
         file length      4o  (depend) +98
         transform        36o +102 or +66

      */
        data.Add(3);
        data.Add(0); // It is a custom Mesh so put 0 here ...
        string n = CryptoUtilities.GetUniqueName();
        foreach ( char c in n.ToCharArray())
        {
            data.Add((byte)c);
        }
        BinaryUtilities.AddBytesToList(ref data, chksm);
        BinaryUtilities.AddBytesToList(ref data, BitConverter.GetBytes((int)ff.Length));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(pos));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(rot));
        BinaryUtilities.AddBytesToList(ref data, BinaryUtilities.Vector3Tobytes(scale));


        NetUtilities.SendDataToMaster(data.ToArray());

        PlaceHolder.CreatePlaceHolderFromWaitingPermission(data.ToArray());
    }

    // ---> Master Answer <---- 
    public static void GiveInstantiatePermission(byte[] msg, PhotonMessageInfo info)
    {
        byte[] chksm = new byte[32];
        for (int i = 0; i < 32; i ++)
        {
            chksm[i] = msg[i + 2];
        }
        string fpath = P2SHARE.GetDirByType(msg[1]);
        fpath += CryptoUtilities.SHAToHex(chksm);
        
        if (P2SHARE.GetDLByChecksum(ref chksm) == null)
        {
            if (!File.Exists(fpath))
            {
                P2SHARE.DL dl = P2SHARE.RequestNewFile(fpath,chksm, BitConverter.ToInt32(msg, 34), msg[1],
                    new NetStream.DefaultRPCMessage(msg, info));
                return;
            }
            else
            {
                // give instantiate permission 
                List<byte> data = new List<byte>();
                BinaryUtilities.AddBytesToList(ref data, msg);
                data[0] = 9;
                NetUtilities.SendDataToSpecific(data.ToArray(), info.Sender);
            }
        }
       
        
    }


    public static void OnInstantiatePermissionAccepted(byte[] msg)
    {
        // Obtain the embedded msg . (start at 38 ) 
        byte[] embedded = new byte[msg.Length - 38];
        for (int i = 38; i <msg.Length; i++)
        {
            embedded[i - 38] = msg[i];
        }
        Debug.Log("Permission Obtained!!!");
        NetUtilities.SendDataToAll(embedded); // is it sending to someone or ?
        NetUtilities._mNetStream.ReceiveData(embedded, new PhotonMessageInfo());
        PlaceHolder.DestroyPlaceHolderFromEmbeddedMessage(embedded);
    }

 
    public static void InstantiateTexture(byte[] msg)
    {

        /*

      structure : 
          header +0
          taille du char arr du nom de go +1 
          nom du gameobject (this)
          primitive Type   1o  
          checksum of file 32o (depend) 
          file length      4o  (depend) 

  */

        byte namesize = msg[1];

        char[] goName = new char[namesize];
        for (int i = 2; i < 2 + namesize; i++)
        {
            goName[i - 2] = (char)msg[i];
        }
        Debug.Log(new string(goName));
        GameObject vObj = GameObject.Find(new string(goName));
        if (vObj == null) return;

        byte _primitive = msg[2+namesize];

        if ( _primitive > 0)
        {

        }
        else
        {
            byte[] chksm = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                chksm[i] = msg[i + 3 + namesize];
            }
            string sumstr = CryptoUtilities.SHAToHex(chksm);
            // do i have it in my obj\\ directory ?
            string texPath = Application.persistentDataPath + "/texture/" + sumstr;
            if (File.Exists(texPath))
            {
                Texture2D tex = new Texture2D(1, 1);
                byte[] img = File.ReadAllBytes(texPath);
                tex.LoadImage(img);
                vObj.GetComponent<Renderer>().material.mainTexture = tex;
                return;
            }
            else
            {

                if (P2SHARE.GetDLByChecksum(ref chksm) != null)
                    return;

                P2SHARE.DL dl = P2SHARE.RequestNewFile("", chksm, BitConverter.ToInt32(msg, 34+namesize), 3,
                    new NetStream.DefaultRPCMessage(msg, new PhotonMessageInfo()));
            }
        }

    }

    public static void InstantiateSoundBox(byte[] msg)
    {
        byte _sndType = msg[1]; // we dont bother actually ... 
        char[] name = new char[64];
        for (int i = 0; i < 64; i++)
        {
            name[i] = (char)msg[i + 2];
        }
        byte[] chksm = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            chksm[i] = msg[i + 66];
        }
        string sumstr = CryptoUtilities.SHAToHex(chksm);
        // do i have it in my obj\\ directory ?
        string soundPath = Application.persistentDataPath + "/sound/" + sumstr;
        if (File.Exists(soundPath))
        {

            GameObject inst = GameObject.CreatePrimitive(PrimitiveType.Cube);
            BinaryUtilities.DeSerializeTransformOnObject(ref msg, 102, inst);
            ObjectUtilities.AddDefaultComponentsToObjectInstance(inst);
            inst.tag = "3DMESH";
            inst.name = new string(name);
            inst.GetComponent<Renderer>().material.shader = Shader.Find("UnityLibrary/Effects/Wireframe");
            inst.AddComponent<AudioSource>();
            // do something with soundPath
            return;
        }
        else
        {
            if (P2SHARE.GetDLByChecksum(ref chksm) != null)
                return;

            P2SHARE.DL dl = P2SHARE.RequestNewFile(soundPath, chksm, BitConverter.ToInt32(msg, 98), 4,
                new NetStream.DefaultRPCMessage(msg, new PhotonMessageInfo()));
        }

    }
    public static void InstantiateMesh(byte[] msg) // This stuff crash some time IDK WHY Probably a  bad save file server side ?
    {
        /*
         msg struct : 
            msg flag         1o +0
            primitive Type   1o +1 
            hash name        64o +2 
            checksum of file 32o (depend) +66
            file length      4o  (depend) +98
            transform        36o +102 or +66
            
         */
        /*
        byte _primitive = msg[1];
        char[] name = new char[64];
        for (int i = 0; i < 64; i++)
        {
            name[i] = (char)msg[i + 2];
        }
        if (_primitive > 0)
        {
            // is instantiate stuff.
            GameObject inst = null;
            switch (_primitive)
            {
                case 1:
                    inst = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case 2:
                    inst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                case 3:
                    inst = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    inst.transform.Rotate(-90, 0, 0);
                    break;
                case 4:
                    inst = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    break;
                case 5:
                    inst = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    break;
                case 6:
                    inst = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    inst.transform.Rotate(-180, 0, 0);
                    break;
            }
            if (inst == null)
                return;
            BinaryUtilities.DeSerializeTransformOnObject(ref msg, 66, inst);
            ObjectUtilities.RescaleMeshToSize(inst, 0.3f);
            ObjectUtilities.AddDefaultComponentsToObjectInstance(inst);
            inst.name = new string(name);
            return;
        }
        else
        {
            byte[] chksm = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                chksm[i] = msg[i + 66];
            }
            string sumstr = CryptoUtilities.SHAToHex(chksm);
            // do i have it in my obj\\ directory ?
            string objPath = Application.persistentDataPath + "/mesh/" + sumstr;
            if (File.Exists(objPath))
            {

                GameObject inst = new OBJLoader().Load(objPath);
                if (inst == null)
                {
                    Debug.Log("fail to import OBJ");
                    return;
                }
                BinaryUtilities.DeSerializeTransformOnObject(ref msg, 102, inst);
                ObjectUtilities.RescaleMeshToSize(inst, 0.3f);
                ObjectUtilities.AddDefaultComponentsToObjectInstance(inst);
                inst.name = new string(name);
                return;
            }
            else
            {

                if (P2SHARE.GetDLByChecksum(ref chksm) != null)
                    return;

                P2SHARE.DL dl = P2SHARE.RequestNewFile(objPath, chksm, BitConverter.ToInt32(msg, 98), 1, 
                    new NetStream.DefaultRPCMessage(msg, new PhotonMessageInfo()));

            }
        }
        */
    }
}
