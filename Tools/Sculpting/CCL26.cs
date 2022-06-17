using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;

public class CCL26 : MonoBehaviour
{
    // @ CCL 26-connectivity ON CPU.
    // This is used to detect islands in mesh. This takes 1s for like 6 Millions possible vertices.
    public class ComputeTask
    {
        public Moodulable _Moodulable;
        public bool       _Done;
        public bool       _Success;
        public bool       _FromNet;
        // Variables
        public int[] bit;
        public int[] classifications;
        public int[] equivalents;
        public int voxelNumbers;
        public ComputeTask(Moodulable md, int[] bufferData, int texSize, bool _fNet) 
        { 
            _Moodulable = md; _Done = false; _Success = false; _FromNet = _fNet;

             classifications = new int[bufferData.Length];
            equivalents = new int[bufferData.Length];
            bit = bufferData;
            voxelNumbers = texSize;

        }
    }
    public static Vector3[] neighboorsTable =
    {

             new Vector3(-1,0,0),           // 02: w
             new Vector3(-1,-1,0),          // 03: sw
             new Vector3(-1,0,-1),          // 05: fw
             new Vector3(-1,-1,-1),         // 06: fsw
             new Vector3(-1, 1,-1),         // 07: fnw
             new Vector3(1,-1,0),          // 12: se
             new Vector3(1,0,-1),          // 14: fe
             new Vector3(1,-1,-1),         // 15: fse
             new Vector3(1,1, -1),         // 16: fne
             new Vector3(0,-1,0),          // 21: se
             new Vector3(0,0,-1),          // 23: fe
             new Vector3(0,-1,-1),         // 24: fse
             new Vector3(0,1, -1),         // 25: fne
    };

    
    public static List<ComputeTask> tasks = new List<ComputeTask>();

    public static ComputeTask GetTaskFromMoodulable(Moodulable m)
    {
        foreach ( ComputeTask t in tasks)
        {
            if (t._Moodulable.gameObject.name == m.gameObject.name)
                return t;
        }
        return null;
    }
    public static void RemoveTask(ComputeTask t)
    {
        Debug.Log("Clearing CCL data from " + t._Moodulable.name);
        tasks.Remove(t);
    }

    // Looker Function To cleanup trailing task
    private void Update()
    {
        foreach ( ComputeTask task in tasks)
        {
            if (task._FromNet && task._Done)
            {
                task._Moodulable.classCCLBuff.SetData(task.classifications);
                task._Moodulable.GenerateAllChunks();
                task._Moodulable.TrySplitChunksPerLabel();
                RemoveTask(task);
            }
        }
    }
    public static ComputeTask ComputeCCL(Moodulable moodulable, int[] bufferData, int texSize, bool _FromNet)
    {
        Debug.Log("Computing CCL on " + moodulable.name);
        ComputeTask currentTask = new ComputeTask(moodulable, bufferData, texSize, _FromNet);
        tasks.Add(currentTask);
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            DoCCLComputation(currentTask);
            currentTask._Done = true;

        }).Start();

        return currentTask;
    }

    public static void DoCCLComputation(ComputeTask task)
    {
        for (int z = 0; z < task.voxelNumbers; z++) { 
            for (int y = 0; y < task.voxelNumbers; y++) { 
                for (int x = 0; x < task.voxelNumbers; x++) { CCL_INIT(new Vector3(x, y, z), task); } } }
        for (int z = 0; z < task.voxelNumbers; z++) { 
            for (int y = 0; y < task.voxelNumbers; y++) { 
                for (int x = 0; x < task.voxelNumbers; x++) { CCL_ROWCOLSCAN(new Vector3(x, y, z), task); } } }
        for (int z = 0; z < task.voxelNumbers; z++) { 
            for (int y = 0; y < task.voxelNumbers; y++) { 
                for (int x = 0; x < task.voxelNumbers; x++) { CCL_ROWSCAN(new Vector3(x, y, z), task); } } }
        for (int z = 0; z < task.voxelNumbers; z++) { 
            for (int y = 0; y < task.voxelNumbers; y++) { 
                for (int x = 0; x < task.voxelNumbers; x++) { CCL_REFINE(new Vector3(x, y, z), task); } } }

    }
    public static void CCL_INIT(Vector3 id, ComputeTask task)
    {
        int idx = GetValueFromVectorPosition(id, task);
        task.classifications[idx] = idx;
        task.equivalents[idx] = idx;
    }

    public static void CCL_ROWCOLSCAN(Vector3 id, ComputeTask task)
    {
        if (task.bit[GetValueFromVectorPosition(id, task)] == 0)
            return;

        for (int i = 0; i < neighboorsTable.Length; i++)
        {
            Vector3 coord = id + neighboorsTable[i];
            if (coord.x < task.voxelNumbers && coord.y < task.voxelNumbers && coord.z < task.voxelNumbers
                  && coord.x >= 0 && coord.y >= 0 && coord.z >= 0)
            {
                if (task.bit[GetValueFromVectorPosition(coord, task)] == 1)
                {
                    if (task.classifications[GetValueFromVectorPosition(coord, task)] < task.classifications[GetValueFromVectorPosition(id, task)])
                    {
                        task.equivalents[task.classifications[GetValueFromVectorPosition(id, task)]] =
                            task.equivalents[task.classifications[GetValueFromVectorPosition(coord, task)]];
                    }
                }
            }
        }
    }
    public static void CCL_ROWSCAN(Vector3 id, ComputeTask task)
    {
        if (task.bit[GetValueFromVectorPosition(id, task)] == 0)
            return;

        Vector3 west = id + new Vector3(-1, 0, 0);
        if (id.x > 0 && task.bit[GetValueFromVectorPosition(west, task)] == task.bit[GetValueFromVectorPosition(id, task)])
        {
            if (task.classifications[GetValueFromVectorPosition(west, task)] < task.classifications[GetValueFromVectorPosition(id, task)])
            {
                task.equivalents[findRoot(west, task)] = findRoot(id, task);
            }
            else if (task.classifications[GetValueFromVectorPosition(west, task)] > task.classifications[GetValueFromVectorPosition(id, task)])
            {
                task.equivalents[findRoot(id, task)] = findRoot(west, task);
            }
        }
    }
    public static void CCL_REFINE(Vector3 id, ComputeTask task)
    {
        if (task.bit[GetValueFromVectorPosition(id, task)] == 0)
            return;

        task.classifications[GetValueFromVectorPosition(id, task)] = findRoot(id, task);
    }
    public static int findRoot(Vector3 id, ComputeTask task)
    {
        int root = task.equivalents[task.classifications[GetValueFromVectorPosition(id,task)]];
        int parent = task.equivalents[root];

        while (root != parent)
        {
            root = parent;
            parent = task.equivalents[root];
        }
        return root;
    }
    public static Vector3 GetPositionFromIndexValue(int index, ComputeTask task)
    {
        int x = index % task.voxelNumbers;
        int y = (index % (task.voxelNumbers * task.voxelNumbers)) / task.voxelNumbers;
        int z = index / (task.voxelNumbers * task.voxelNumbers);
        return new Vector3(x, y, z);
    }
    public static int GetValueFromVectorPosition(Vector3 vec, ComputeTask task)
    {
        return (int)vec.z * task.voxelNumbers * task.voxelNumbers + (int)vec.y * task.voxelNumbers + (int)vec.x;
    }
}
