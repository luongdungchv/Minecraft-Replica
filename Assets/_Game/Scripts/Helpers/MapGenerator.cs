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
    [SerializeField] private Vector2[] logUVs;
    [SerializeField] private Texture2D[] testTexList;
    [SerializeField] private int[] logsTris;
    private ComputeBuffer vertexBuffer, drawArgsBuffer;
    private ComputeBuffer indexBuffer, uvBuffer, uvDepthBuffer;

    private BaseBlocksGenerator blocksGenerator;
    private FaceCuller faceCuller;
    private FrustumCuller frustumCuller;
    private DuplicatesRemover duplicatesRemover;

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
        this.duplicatesRemover = GetComponent<DuplicatesRemover>();
        
        this.PopulateUVDepthBuffer();
        this.PopulateIndexBuffer();
        this.PopulateVertexBuffer();
        this.PopulateUVBuffer();
        this.InitializeMapGeneration();
        this.InitArgsBuffer();

        this.bounds = new Bounds(Vector3.zero, Vector3.one * 15000);
        noiseGenerator.GenerateNoise(Vector3.zero);
        blocksGenerator.Generate(Vector3.zero);
    }

    private void Update()
    {
        this.GenerateBaseWorldAround(Vector3.zero);
        
        this.DrawBlocks(Vector3.zero);
        this.faceBuffer.SetCounterValue(0);
        this.frustumBuffer.SetCounterValue(0);
    }

    [Sirenix.OdinInspector.Button]
    private void TestDrawCube(int test)
    {
        duplicatesRemover.Perform();
        int[] temp = new int[1];
        var tempBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer.CopyCount(duplicatesRemover.OutputBuffer, tempBuffer, 0);
        tempBuffer.GetData(temp);
        var output = new int[temp[0]];
        Debug.Log(temp[0]);
        duplicatesRemover.OutputBuffer.GetData(output);
        foreach(var i in output) Debug.Log(i);
        tempBuffer.Dispose();
    }

    [Sirenix.OdinInspector.Button]
    private void Test2(){
        // this.logUVs = this.blockMesh.uv;
        // this.logsVerts = this.blockMesh.vertices;
        // this.logsTris = blockMesh.triangles;

        int[] test = new int[6];
        //this.uvDepthBuffer = new ComputeBuffer(6, sizeof(int));
        this.uvDepthBuffer.GetData(test);
        foreach(var i in test) Debug.Log(i);
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
        duplicatesRemover.Initialize(noiseGenerator.Width * noiseGenerator.Height * maxHeight);

        this.drawMaterial.SetBuffer("uvDepthBuffer", this.uvDepthBuffer);
        this.drawMaterial.SetBuffer("instanceBuffer", instanceBuffer);
        this.drawMaterial.SetBuffer("positionBuffer", this.vertexBuffer);
        this.drawMaterial.SetBuffer("faceBuffer", this.faceBuffer);
        this.drawMaterial.SetBuffer("indexBuffer", this.indexBuffer);
        this.drawMaterial.SetBuffer("uvBuffer", this.uvBuffer);
        this.drawMaterial.SetTexture("_Textures", this.CreateTextureArray());
    }
    private void GenerateBaseWorldAround(Vector3 playerPos)
    {
        //blocksGenerator.Generate(playerPos);
        faceCuller.Cull(playerPos);

        ComputeBuffer.CopyCount(faceBuffer, frustumArgsBuffer, 0);
        //this.frustumCuller.Cull();
        ComputeBuffer.CopyCount(faceBuffer, drawArgsBuffer, 4);
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

    private void PopulateUVBuffer(){
        var uv = this.blockMesh.uv;

        uv[6] = new Vector2(1, 0);
        uv[7] = new Vector2(0, 0);
        uv[11] = new Vector2(0, 1);
        uv[10] = new Vector2(1, 1);

        this.uvBuffer = new ComputeBuffer(24, sizeof(float) * 2);
        this.uvBuffer.SetData(uv);
    }

    private void PopulateUVDepthBuffer(){
        this.uvDepthBuffer = new ComputeBuffer(6, sizeof(int));
        float[] datas = {0, 1, 0, 2, 0, 0};
        uvDepthBuffer.SetData(datas);
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
        this.uvBuffer.Dispose();
        this.uvDepthBuffer.Dispose();
    }

    private Texture2DArray CreateTextureArray()
    {
        Texture2DArray texArray = new Texture2DArray(16, 16, 3, testTexList[0].format, false);
        texArray.filterMode = FilterMode.Point;
        for (int i = 0; i < 3; i++)
        {
            texArray.SetPixels32(testTexList[i].GetPixels32(), i);
        }
        texArray.Apply();

        return texArray;
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
