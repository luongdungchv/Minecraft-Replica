using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private NoiseGenerator noiseGenerator;
    [SerializeField] private int maxHeight;
    [SerializeField] private Mesh blockMesh;
    //[SerializeField] private ComputeShader frustumArgsFillerShader;
    [SerializeField] private Material drawMaterial, testMat;
    [SerializeField] private InstanceData[] datas;
    [SerializeField] private Vector3[] logsVerts;
    [SerializeField] private int[] logsTris;
    private ComputeBuffer vertexBuffer, drawArgsBuffer;
    private ComputeBuffer indexBuffer;

    private BaseBlocksGenerator blocksGenerator;
    private FaceCuller faceCuller;
    private FrustumCuller frustumCuller;

    private ComputeBuffer instanceBuffer => blocksGenerator.InstanceBuffer;
    private ComputeBuffer faceBuffer => faceCuller.FaceBuffer;
    private ComputeBuffer frustumArgsBuffer => this.frustumCuller.ArgsBuffer;
    private ComputeBuffer frustumBuffer => this.frustumCuller.FrustumBuffer;

    private Bounds bounds;

    private void Awake()
    {
        this.blocksGenerator = GetComponent<BaseBlocksGenerator>();
        this.faceCuller = GetComponent<FaceCuller>();
        this.frustumCuller = GetComponent<FrustumCuller>();
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
    }

    [Sirenix.OdinInspector.Button]
    private void Test2(){
        noiseGenerator.GenerateNoise(Vector3.zero);
        this.GenerateBaseWorldAround(Vector3.zero);
        int[] args = new int[3];
        this.frustumArgsBuffer.GetData(args);
        Debug.Log(args[0]);
    }

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
        blocksGenerator.Initialize(noiseGenerator.Width, noiseGenerator.Height, this.maxHeight, noiseGenerator.TargetTexture);
        faceCuller.Initialize(noiseGenerator.Width, noiseGenerator.Height, this.maxHeight);
        frustumCuller.Initialize(noiseGenerator.Width, noiseGenerator.Height, this.maxHeight);

        this.drawMaterial.SetBuffer("instanceBuffer", instanceBuffer);
        this.drawMaterial.SetBuffer("positionBuffer", this.vertexBuffer);
        this.drawMaterial.SetBuffer("faceBuffer", this.frustumBuffer);
        this.drawMaterial.SetBuffer("indexBuffer", this.indexBuffer);
    }
    private void GenerateBaseWorldAround(Vector3 playerPos)
    {
        blocksGenerator.Generate(playerPos);
        faceCuller.Cull(playerPos);

        ComputeBuffer.CopyCount(faceBuffer, drawArgsBuffer, 4);
        ComputeBuffer.CopyCount(faceBuffer, frustumArgsBuffer, 0);
        this.frustumCuller.Cull();
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
    }

    private void DrawBlocks(Vector3 center)
    {
        this.bounds.center = center;
        //Graphics.DrawMeshInstancedIndirect(this.blockMesh, 0, this.testMat, this.bounds, this.argsBuffer);
        Graphics.DrawProceduralIndirect(this.drawMaterial, this.bounds, MeshTopology.Triangles, this.drawArgsBuffer);
    }

    private void PopulateIndexBuffer()
    {
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
