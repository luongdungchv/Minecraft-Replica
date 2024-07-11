using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private Vector3Int worldSize;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material drawMaterial;
    private ComputeBuffer instanceBuffer, argsBuffer;
    private Bounds bounds;
    private void Start()
    {
        this.InitializeData();
    }
    private void InitializeData()
    {
        var instanceDataList = new List<InstanceData>();
        for (int x = 0; x < worldSize.x; x++)
        {
            for (int y = 0; y < worldSize.y; y++)
            {
                for (int z = 0; z < worldSize.z; z++)
                {
                    var position = new Vector3(x, y, z);
                    var instanceData = new InstanceData()
                    {
                        trs = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one),
                        blockType = 0,
                    };
                    instanceDataList.Add(instanceData);
                }
            }
        }

        this.instanceBuffer = new ComputeBuffer(instanceDataList.Count, InstanceData.Size);
        instanceBuffer.SetData(instanceDataList);

        this.argsBuffer = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
        uint[] args = { mesh.GetIndexCount(0), (uint)instanceDataList.Count, mesh.GetIndexStart(0), mesh.GetBaseVertex(0), 0 };
        this.argsBuffer.SetData(args);

        this.bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

        this.drawMaterial.SetBuffer("instanceBuffer", this.instanceBuffer);
    }

    private void Update()
    {
        Graphics.DrawMeshInstancedIndirect(this.mesh, 0, this.drawMaterial, bounds, argsBuffer);
    }
    private void OnDestroy()
    {
        this.instanceBuffer.Dispose();
        this.argsBuffer.Dispose();
    }
}

