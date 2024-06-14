using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private NoiseGenerator noiseGenerator;
    [SerializeField] private int maxHeight;
    [SerializeField] private Mesh blockMesh;
    private ComputeBuffer vertexBuffer, instanceBuffer;
    private GraphicsBuffer indexBuffer;

    private void Awake()
    {
        this.PopulateIndexBuffer();
        // this.PopulateVertexBuffer();
        
    }
    [Sirenix.OdinInspector.Button]
    private void TestDrawCube(){
        //uint[] args = {}
    }

    // private void PopulateVertexBuffer()
    // {
    //     Vector3[] vertexDatas = new Vector3[24];
    //     vertexDatas[0] = -Vector3.one;
    //     vertexDatas[1] = new Vector3(-1, 1, -1) * 0.5f;
    //     vertexDatas[2] = new Vector3(1, 1, -1) * 0.5f;
    //     vertexDatas[3] = new Vector3(1, -1, -1) * 0.5f;

    //     vertexDatas[4] = new Vector3(-1, -1, 1) * 0.5f;
    //     vertexDatas[5] = new Vector3(-1, 1, 1) * 0.5f;
    //     vertexDatas[6] = new Vector3(1, 1, 1) * 0.5f;
    //     vertexDatas[7] = new Vector3(1, -1, 1) * 0.5f;

    //     vertexDatas[8] = vertexDatas[3];
    //     vertexDatas[9] = vertexDatas[2];
    //     vertexDatas[10] = vertexDatas[6];
    //     vertexDatas[11] = vertexDatas[7];


    //     vertexDatas[12] = vertexDatas[4];
    //     vertexDatas[13] = vertexDatas[5];
    //     vertexDatas[14] = vertexDatas[1];
    //     vertexDatas[15] = vertexDatas[0];

    //     vertexDatas[16] = vertexDatas[1];
    //     vertexDatas[17] = vertexDatas[5];
    //     vertexDatas[18] = vertexDatas[6];
    //     vertexDatas[19] = vertexDatas[2];

    //     vertexDatas[20] = vertexDatas[4];
    //     vertexDatas[21] = vertexDatas[0];
    //     vertexDatas[22] = vertexDatas[3];
    //     vertexDatas[23] = vertexDatas[7];

    //     this.vertexBuffer = new ComputeBuffer(24, sizeof(float) * 3);
    //     this.vertexBuffer.SetData(vertexDatas);
    // }

    private void PopulateVertexBuffer(){
        var vertices = this.blockMesh.vertices;
        this.vertexBuffer = new ComputeBuffer(24, sizeof(float) * 3);
        this.vertexBuffer.SetData(vertices);
    }

    private void PopulateIndexBuffer()
    {
        int[] triangles = new int[]{
            0,1,3,3,1,2,
            3,2,7,7,2,6,
            7,6,4,4,6,5,
            4,5,0,0,5,1,
            1,5,2,2,5,6,
            4,0,7,7,0,3
        };

        this.indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, 36, sizeof(int));
    }
    private void PopulateInstanceBuffer(){
        var instanceList = new List<InstanceData>();
        
    }
}

public struct InstanceData{
    public Matrix4x4 trs;
    public int id;
}
public struct FaceData{
    public int instanceIndex;
    public int face;
}
