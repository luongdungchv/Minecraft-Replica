using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private NoiseGenerator noiseGenerator;
    [SerializeField] private int maxHeight;
    [SerializeField] private Mesh blockMesh;
    [SerializeField] private ComputeShader mapGenShader, faceCullShader, frustumCullShader, frustumArgsFillerShader;
    [SerializeField] private Material drawMaterial, testMat;
    [SerializeField] private InstanceData[] datas;
    [SerializeField] private Vector3[] logsVerts;
    [SerializeField] private int[] logsTris;
    private ComputeBuffer vertexBuffer, instanceBuffer, drawArgsBuffer, frustumArgsBuffer, faceBuffer, testBuffer, frustumBuffer;
    private ComputeBuffer indexBuffer;

    private Bounds bounds;

    private void Awake()
    {
        this.PopulateIndexBuffer();
        this.PopulateVertexBuffer();
        this.InitializeMapGeneration();
        this.InitArgsBuffer();
        this.bounds = new Bounds(Vector3.zero, Vector3.one * 15000);
    }

    private void Update()
    {
        noiseGenerator.GenerateNoise(Vector3.zero);
        this.GenerateBaseWorldAround(Vector3.zero);
        
        this.DrawBlocks(Vector3.zero);
        this.faceBuffer.SetCounterValue(0);
        this.frustumBuffer.SetCounterValue(0);

        // noiseGenerator.GenerateNoise(Vector3.zero);
        // this.GenerateBaseWorldAround(Vector3.zero);
        // ComputeBuffer.CopyCount(testBuffer, argsBuffer, 4);
        // this.DrawBlocks(Vector3.zero);
        // this.testBuffer.SetCounterValue(0);
    }

    [Sirenix.OdinInspector.Button]
    private void TestDrawCube(int test)
    {
        noiseGenerator.GenerateNoise(Vector3.zero);
        this.GenerateBaseWorldAround(Vector3.zero);
        ComputeBuffer.CopyCount(faceBuffer, drawArgsBuffer, 4);
        //this.DrawBlocks(Vector3.zero);
        InstanceData[] datas = new InstanceData[noiseGenerator.Width * noiseGenerator.Height * maxHeight];
        uint[] args = new uint[5];
        this.drawArgsBuffer.GetData(args);
        FaceData[] faces = new FaceData[240000];
        faceBuffer.GetData(faces);
        // Debug.Log(datas[test].trs);
        // Debug.Log(datas[test].available);
        //Debug.Log
        drawArgsBuffer.GetData(args);
        Debug.Log(args[1]);
        Debug.Log(Camera.main.projectionMatrix);
        this.testBuffer.SetCounterValue(0);
    }

    [Sirenix.OdinInspector.Button]
    private void Test2(){
        noiseGenerator.GenerateNoise(Vector3.zero);
        this.GenerateBaseWorldAround(Vector3.zero);
        int[] args = new int[3];
        this.frustumArgsBuffer.GetData(args);
        Debug.Log(args[0]);
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

    private void PopulateVertexBuffer()
    {
        var vertices = this.blockMesh.vertices;
        this.vertexBuffer = new ComputeBuffer(24, sizeof(float) * 3);
        this.vertexBuffer.SetData(vertices);
        this.logsVerts = vertices;
        this.logsTris = this.blockMesh.triangles;
    }
    private void InitializeMapGeneration()
    {
        this.instanceBuffer = new ComputeBuffer(noiseGenerator.Width * noiseGenerator.Height * maxHeight, InstanceData.Size, ComputeBufferType.Structured);
        this.testBuffer = new ComputeBuffer(noiseGenerator.Width * noiseGenerator.Height * maxHeight, InstanceData.Size, ComputeBufferType.Append);
        testBuffer.SetCounterValue(0);

        this.instanceBuffer.SetCounterValue(0);

        int kernelIndex = mapGenShader.FindKernel("GenerateMap");

        mapGenShader.SetBuffer(kernelIndex, "instancesData", this.instanceBuffer);
        mapGenShader.SetTexture(kernelIndex, "Input", this.noiseGenerator.TargetTexture);
        mapGenShader.SetFloat("size", this.noiseGenerator.Width);
        mapGenShader.SetFloat("maxHeight", (float)this.maxHeight);
        mapGenShader.SetInt("width", noiseGenerator.Width);
        mapGenShader.SetInt("height", noiseGenerator.Height);
        mapGenShader.SetBuffer(0, "testBuffer", this.testBuffer);

        this.faceBuffer = new ComputeBuffer(noiseGenerator.Width * noiseGenerator.Height * maxHeight * 6, FaceData.Size, ComputeBufferType.Append);
        faceBuffer.SetCounterValue(0);

        faceCullShader.SetBuffer(0, "Input", instanceBuffer);
        faceCullShader.SetBuffer(0, "Result", faceBuffer);
        faceCullShader.SetBuffer(0, "testResult", testBuffer);
        faceCullShader.SetInt("width", noiseGenerator.Width);
        faceCullShader.SetInt("height", noiseGenerator.Height);
        faceCullShader.SetFloat("maxHeight", this.maxHeight);
        faceCullShader.SetVectorArray("dirs", new Vector4[]{
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, -1, 0),
            new Vector3(0, 0, -1),
            new Vector3(-1, 0, 0),
            new Vector3(1, 0, 0)
        });

        this.frustumBuffer = new ComputeBuffer(noiseGenerator.Width * noiseGenerator.Height * maxHeight * 6, FaceData.Size, ComputeBufferType.Append);
        frustumBuffer.SetCounterValue(0);
        frustumCullShader.SetBuffer(0, "instanceBuffer", this.instanceBuffer);
        frustumCullShader.SetBuffer(0, "faceBuffer", this.faceBuffer);
        frustumCullShader.SetBuffer(0, "result", this.frustumBuffer);

        this.drawMaterial.SetBuffer("instanceBuffer", instanceBuffer);
        this.drawMaterial.SetBuffer("positionBuffer", this.vertexBuffer);
        this.drawMaterial.SetBuffer("faceBuffer", this.frustumBuffer);
        this.drawMaterial.SetBuffer("indexBuffer", this.indexBuffer);

        this.testMat.SetBuffer("instanceBuffer", this.testBuffer);
    }
    private void GenerateBaseWorldAround(Vector3 playerPos)
    {
        mapGenShader.SetVector("offset", (Vector2)playerPos);
        mapGenShader.Dispatch(0, Mathf.CeilToInt(noiseGenerator.Width / 8), Mathf.CeilToInt(noiseGenerator.Height / 8), Mathf.CeilToInt(maxHeight / 8));
        faceCullShader.SetVector("camPos", Camera.main.transform.position);
        faceCullShader.Dispatch(0, Mathf.CeilToInt(noiseGenerator.Width / 8), Mathf.CeilToInt(noiseGenerator.Height / 8), Mathf.CeilToInt(maxHeight / 8));

        ComputeBuffer.CopyCount(faceBuffer, drawArgsBuffer, 4);
        ComputeBuffer.CopyCount(faceBuffer, frustumArgsBuffer, 0);
        frustumArgsFillerShader.Dispatch(0,1,1,1);

        var p = Camera.main.projectionMatrix;
        p.SetRow(0, new Vector4(0.84839f, 0, 0, 0));
        p.SetRow(1, new Vector4(0f, 1.51084f, 0, 0));
        var v = Camera.main.worldToCameraMatrix;
        var VP = p * v;
        frustumCullShader.SetMatrix("vp", VP);
        frustumCullShader.SetVector("camPos", Camera.main.transform.position);
        frustumCullShader.DispatchIndirect(0, this.frustumArgsBuffer);
    }

    private void InitArgsBuffer()
    {
        uint[] args = {6,0,0,0,0};
        // uint[] args = {
        //     this.blockMesh.GetIndexCount(0),
        //     (uint)(noiseGenerator.Width * noiseGenerator.Height * maxHeight),
        //     this.blockMesh.GetIndexStart(0),
        //     this.blockMesh.GetBaseVertex(0),
        //     0
        // };
        this.drawArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        this.drawArgsBuffer.SetData(args);

        args = new uint[]{
            1,1,1
        };
        this.frustumArgsBuffer = new ComputeBuffer(3, sizeof(uint), ComputeBufferType.IndirectArguments);
        this.frustumArgsBuffer.SetData(args);
        this.frustumArgsFillerShader.SetBuffer(0, "args", this.frustumArgsBuffer);
    }

    private void DrawBlocks(Vector3 center)
    {
        this.bounds.center = center;
        //Graphics.DrawMeshInstancedIndirect(this.blockMesh, 0, this.testMat, this.bounds, this.argsBuffer);
        Graphics.DrawProceduralIndirect(this.drawMaterial, this.bounds, MeshTopology.Triangles, this.drawArgsBuffer);
    }

    private void InitializeTests()
    {
        this.instanceBuffer = new ComputeBuffer(noiseGenerator.Width * noiseGenerator.Height * maxHeight, InstanceData.Size, ComputeBufferType.Structured);
        var data = new List<InstanceData>();
        for (int i = 0; i < 10; i++)
        {
            var trs = Matrix4x4.TRS(new Vector3(Random.Range(1, 10), 1, Random.Range(1, 10)), transform.rotation, Vector3.one);
            var a = new InstanceData();
            a.trs = trs;
            data.Add(a);
        }



        instanceBuffer.SetData(data);
        this.drawMaterial.SetBuffer("instanceBuffer", instanceBuffer);
    }

    private void PopulateIndexBuffer()
    {
        // this.indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, 36, sizeof(int));
        // indexBuffer.SetData(this.blockMesh.triangles);
        this.indexBuffer = new ComputeBuffer(this.blockMesh.triangles.Length, sizeof(int));
        indexBuffer.SetData(this.blockMesh.triangles);
    }

    private void OnDestroy()
    {
        this.drawArgsBuffer.Dispose();
        this.instanceBuffer.Dispose();
        this.vertexBuffer.Dispose();
        this.faceBuffer.Dispose();
        this.indexBuffer.Dispose();
        this.testBuffer.Dispose();
        this.frustumArgsBuffer.Dispose();
        this.frustumBuffer.Dispose();
    }
}

[System.Serializable]
public struct InstanceData
{
    public Matrix4x4 trs;
    public int available;
    public static int Size => 16 * sizeof(float) + sizeof(int);
}
public struct FaceData
{
    public int instanceIndex;
    public int vertexIndex;
    public static int Size => sizeof(int) * 2;
}
