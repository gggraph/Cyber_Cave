using UnityEngine;
using System.Text;
using System.IO;

public class ObjExporter : MonoBehaviour
{
    static Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle)
    {
        return angle * (point - pivot) + pivot;
    }
    static Vector3 MultiplyVec3s(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    public static void ExportMesh(GameObject go, string exportPath, bool applyScale = true, bool applyRotation = false, bool applyPosition = false)
    {
        MeshFilter mf = go.GetComponent<MeshFilter>();
        StringBuilder sb = new StringBuilder();

        string meshName = go.name;
        MeshRenderer mr = mf.gameObject.GetComponent<MeshRenderer>();

        Mesh msh = mf.sharedMesh;
        int faceOrder = (int)Mathf.Clamp((mf.gameObject.transform.lossyScale.x * mf.gameObject.transform.lossyScale.z), -1, 1);

        //export vector data (FUN :D)!
        foreach (Vector3 vx in msh.vertices)
        {
            Vector3 v = vx;
            if (applyScale)
            {
                v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale);
            }

            if (applyRotation) // ??? this ?
            {

                v = RotateAroundPoint(v, Vector3.zero, mf.gameObject.transform.rotation);
            }

            if (applyPosition)
            {
                v += mf.gameObject.transform.position;
            }
            v.x *= -1; // peut etre ici ?
            sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);
        }
        foreach (Vector3 vx in msh.normals)
        {
            Vector3 v = vx;

            if (applyScale)
            {
                v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale.normalized);
            }
            if (applyRotation)
            {
                v = RotateAroundPoint(v, Vector3.zero, mf.gameObject.transform.rotation);
            }
            v.x *= -1;
            sb.AppendLine("vn " + v.x + " " + v.y + " " + v.z);

        }
        foreach (Vector2 v in msh.uv)
        {
            sb.AppendLine("vt " + v.x + " " + v.y);
        }

        for (int j = 0; j < msh.subMeshCount; j++)
        {
            if (mr != null && j < mr.sharedMaterials.Length)
            {
                string matName = mr.sharedMaterials[j].name;
                sb.AppendLine("usemtl Default-Material");//"usemtl " + matName);
            }
            else
            {
                sb.AppendLine("usemtl Default-Material");//sb.AppendLine("usemtl " + meshName + "_sm" + j);
            }
            // Here some issue ... 

            int[] tris = msh.GetTriangles(j);
            for (int t = 0; t < tris.Length; t += 3)
            {
                int idx2 = tris[t] + 1;
                int idx1 = tris[t + 1] + 1;
                int idx0 = tris[t + 2] + 1;
                if (faceOrder < 0)
                {
                    sb.AppendLine("f " + ConstructOBJString(idx2) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx0));
                }
                else
                {
                    sb.AppendLine("f " + ConstructOBJString(idx0) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx2));
                }

            }
        }


        //write to disk
        string txts = sb.ToString();
        txts = txts.Replace(',', '.'); // replace ',' of float string conversion
        System.IO.File.WriteAllText(exportPath, txts);
    }
    static string ConstructOBJString(int index)
    {
        string idxString = index.ToString();
        return idxString + "/" + idxString + "/" + idxString;
    }


    public static string MeshToString(MeshFilter mf)
    {
        Mesh m = mf.mesh;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;


        StringBuilder sb = new StringBuilder();

        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 v in m.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in m.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }
        return sb.ToString();
        // for (int material = 0; material < m.subMeshCount; material++)
        // {
        //     sb.Append("\n");
        //     sb.Append("usemtl ").Append(mats[material].name).Append("\n");
        //     sb.Append("usemap ").Append(mats[material].name).Append("\n");

        //     int[] triangles = m.GetTriangles(material);
        //     for (int i = 0; i < triangles.Length; i += 3)
        //     {
        //         sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
        //         triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
        //     }
        // }
        // return sb.ToString();
    }

    public static bool MeshToFile(MeshFilter mf, string filename)
    {
        if (mf == null)
        {
            Debug.Log("OBJ FAILED TO EXPORT. Meshfilter was null");

            return false;
        }


        using (StreamWriter sw = new StreamWriter(filename))
        {

            sw.Write(MeshToString(mf));

        }
        Debug.Log("Obj file successfully created at " + filename);
        return true;
    }


}
