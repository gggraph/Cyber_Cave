using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlaceHolder : MonoBehaviour
{
    public class MissingNO
    {
        public GameObject gameobject { get; }
        public P2SHARE.DL dll { get; }
        public byte[] embedded_msg { get; }

        public MissingNO(GameObject go, P2SHARE.DL dl, byte[] emsg)
        {
            gameobject = go;
            dll = dl;
            embedded_msg = emsg;
        }
    }

    public static List<MissingNO> HoldersObjects = new List<MissingNO>();
    // Instancie un objet remplacant un DL
    public static GameObject CreatePlaceHolderFromDL(P2SHARE.DL dl) //< this crash the game
    {
        Debug.Log("Creatin new place holder : " + dl.fileType);

        // create a DL depending by type .
        // If the DL type is a Mesh. Put a PlaceHodler Mesh. at the position. There is a lot of chance DL unporocees message contains transform position of the DL. 
        // If the DL type is a texture there is a lot of chance there is gameobject attached to unprocessed message ... 
        GameObject result = null;
        switch (dl.fileType)
        {
            case 1:
                if (dl.missingDataInfo.UnProccessedMessage == null)
                    result = null;
                if (dl.missingDataInfo.UnProccessedMessage.Data[0] != 3)
                    result = null;
                // get the transform (actually at offset 102) 
                byte[] instructions = dl.missingDataInfo.UnProccessedMessage.Data;
                GameObject ph = ObjectUtilities.InstantiateGameObjectFromAssets("missingmesh");
                // set a texture of 100*100
                Texture2D tex = new Texture2D(100, 100);
                ph.GetComponent<Renderer>().material.mainTexture = tex;
                ph.transform.position = BinaryUtilities.TransformDataToPosition(ref instructions, 102);
                ph.name = dl.filePath;
                ph.AddComponent<AutoRotate>();
                HoldersObjects.Add(new MissingNO(ph, dl, null));
                result = ph;

                break;
            case 3:
                if (dl.missingDataInfo == null)
                {
                    result = null; break;
                }
                   
                // texture. Return the Object. 
                if (dl.missingDataInfo.UnProccessedMessage == null)
                    result = null;
                if (dl.missingDataInfo.UnProccessedMessage.Data[0] != 20)
                    result = null;
                // Get the Object. Set the PendingDL Texture. return the gameObject
                break;
            case 4:
                // Do not do anything for soundBox for the moment? Or change color of shader of the soundBox? Or instantiate 
                break;
        }
        return result;
    }
    public static bool DestroyPlaceHolderFromDL(P2SHARE.DL dl)
    {
        MissingNO mno = GetHolderFromDL(dl);
        if (mno == null)
            return false;

        if (mno.gameobject != null)
        {
            Destroy(mno.gameobject);
        }
        HoldersObjects.Remove(mno);
        return true;
    }

    public static MissingNO GetHolderFromDL(P2SHARE.DL dl)
    {

        foreach (MissingNO m in HoldersObjects)
        {
            if (m.dll == dl) // idk if it will throw an exception if m.dll is null
            {
                return m;
            }
        }
        return null;
    }

    public static MissingNO GetHolderFromEmbeddedMessage(byte[] data)
    {

        foreach (MissingNO m in HoldersObjects)
        {
            if (m.embedded_msg != null)
            {
                if (m.embedded_msg.SequenceEqual(data))
                    return m;
            }
        }
        return null;
    }



    public static void UpdateHolderTextureFromDLStatus(P2SHARE.DL dl)
    {
        MissingNO mno = GetHolderFromDL(dl);
        if (mno == null)
            return;

        Texture2D tex = mno.gameobject.GetComponent<Renderer>().material.mainTexture as Texture2D;
        int totalPixel = tex.width * tex.height;
        Color b1 = Color.blue;
        Color b2 = Color.green;
        foreach (P2SHARE.UploadCell c in dl._cells)
        {

            if (c.node == null)
            {
                b1 = Color.red;
            }
            float x1 = 0f;
            x1 = (float)c.o / (float)dl.fileSize;
            x1 *= totalPixel;

            float w1 = (float)c.l / (float)dl.fileSize;
            w1 *= totalPixel;

            float w2 = 0f;
            w2 = (float)c.cell_fill_status / (float)dl.fileSize;
            w2 *= totalPixel;
            Debug.Log("w1 is " + w1.ToString());

            //_g.FillRectangle(b1, x1, 0, w1, 20);
            //_g.FillRectangle(b2, x1, 0, w2, 20);
            // Fill the texture from starting pixel 

            int startX, startY, endX1, endY1, endX2, endY2;

            startY = (int)(x1 / tex.width);
            startX = (int)(x1 - (startY * tex.width));

            endY1 = (int)((x1 + w1) / tex.width);
            endX1 = (int)((x1 + w1) - (endY1 * tex.width));

            endY2 = (int)((x1 + w2) / tex.width);
            endX2 = (int)((x1 + w2) - (endY2 * tex.width));

            // put pixels :  fill whole cell 
            for (int x = startX; x < endX1; x++)
            {
                for (int y = startY; y < endY1; y++)
                {
                    tex.SetPixel(x, y, b1);
                }
            }
            // put pixels :  fill dlled cell 
            for (int x = startX; x < endX2; x++)
            {
                for (int y = startY; y < endY2; y++)
                {
                    tex.SetPixel(x, y, b2);
                }
            }


        }
        tex.Apply();

    }
    // Instantier un objet en cours de permission . 
    public static void CreatePlaceHolderFromWaitingPermission(byte[] permissionMessage)
    {
        // get embedded msg flag (which is at data+38) 
        byte[] embedded = new byte[permissionMessage.Length - 38];
        for (int i = 38; i < permissionMessage.Length; i++)
        {
            embedded[i - 38] = permissionMessage[i];
        }
        byte flag = embedded[0];
        Debug.Log("creating missing no from flag " + flag);
        switch (flag)
        {
            case 3:
                // this is an instantiate custom mesh message... 
                GameObject ph = ObjectUtilities.InstantiateGameObjectFromAssets("missingmesh");
                // transform is at embedded+102 
                BinaryUtilities.DeSerializeTransformOnObject(ref embedded, 102, ph);
                // reset local scale & rot. we dont need it  ->
                ph.transform.localScale = new Vector3(1, 1, 1);
                ph.transform.eulerAngles = new Vector3(0, 0, 0);
                // add specific animation script or ? :) ?
                ph.AddComponent<AutoRotate>();
                HoldersObjects.Add(new MissingNO(ph, null, embedded));
                break;
            case 30:
                // this is an instantiate custom soundbox message ... this as same structure as mesh instantiating message...
                ph = ObjectUtilities.InstantiateGameObjectFromAssets("missingmesh");
                // transform is at embedded+102 
                BinaryUtilities.DeSerializeTransformOnObject(ref embedded, 102, ph);
                // reset local scale & rot. we dont need it  ->
                ph.transform.localScale = new Vector3(1, 1, 1);
                ph.transform.eulerAngles = new Vector3(0, 0, 0);
                // add specific animation script or ? :) ?
                ph.AddComponent<AutoRotate>();
                HoldersObjects.Add(new MissingNO(ph, null, embedded));
                break;
        }
    }

    public static void DestroyPlaceHolderFromEmbeddedMessage(byte[] embeddedMessage)
    {
        MissingNO n = GetHolderFromEmbeddedMessage(embeddedMessage);
        if (n != null)
        {
            if (n.gameobject != null)
            {
                Destroy(n.gameobject);

            }
            HoldersObjects.Remove(n);
        }
    }
}
