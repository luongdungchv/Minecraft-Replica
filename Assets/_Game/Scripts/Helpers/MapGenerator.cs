using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Utilities;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;

    [SerializeField] private NoiseGenerator noiseGenerator;
    [SerializeField] private int maxHeight;
    [SerializeField] private Mesh blockMesh;
    //[SerializeField] private ComputeShader frustumArgsFillerShader;
    [SerializeField] private Material drawMaterial, testMat;
    [SerializeField] private List<BlockInfo> blockInfos;
    [SerializeField] private int chunkSize;
    [SerializeField] private GPUCharacterMovement player;
    private ComputeBuffer vertexBuffer, drawArgsBuffer;
    private ComputeBuffer indexBuffer, uvBuffer, uvDepthBuffer, uvDepthMapping;

    private BaseBlocksGenerator blocksGenerator;
    private FaceCuller faceCuller;
    private FrustumCuller frustumCuller;
    private DuplicatesRemover duplicatesRemover;

    [SerializeField] private Vector2Int currentCell;

    private ComputeBuffer instanceBuffer => blocksGenerator.InstanceBuffer;
    private ComputeBuffer faceBuffer => faceCuller.FaceBuffer;
    private ComputeBuffer frustumArgsBuffer => this.frustumCuller.ArgsBuffer;
    private ComputeBuffer frustumBuffer => this.frustumCuller.FrustumBuffer;

    public int MaxHeight => this.maxHeight;
    public int Size => this.noiseGenerator.Width;
    public Vector2 WorldOffset => new Vector2((float)currentCell.x * chunkSize, (float)currentCell.y * chunkSize);

    private Bounds bounds;

    private void Awake()
    {
        Instance = this;

        this.blocksGenerator = GetComponent<BaseBlocksGenerator>();
        this.faceCuller = GetComponent<FaceCuller>();
        this.frustumCuller = GetComponent<FrustumCuller>();
        this.duplicatesRemover = GetComponent<DuplicatesRemover>();

        //this.PopulateUVDepthBuffer();
        this.PopulateIndexBuffer();
        this.PopulateVertexBuffer();
        this.PopulateUVBuffer();
        this.InitializeMapGeneration();
        this.InitArgsBuffer();

        this.bounds = new Bounds(Vector3.zero, Vector3.one * 15000);
        LoadBaseBlocks(Vector3.zero);
    }

    private void Update()
    {
        this.DetectChunkChange();
        this.GenerateBaseWorldAround(Vector3.zero);

        this.DrawBlocks(Vector3.zero);
        //this.faceBuffer.SetCounterValue(0);
        this.frustumBuffer.SetCounterValue(0);

    }

    private void DetectChunkChange()
    {
        Vector2 posXZ = player.transform.position.XZ();
        posXZ += Vector2.one * chunkSize / 2;
        posXZ.x /= chunkSize;
        posXZ.y /= chunkSize;
        var cell = new Vector2Int((int)posXZ.x, (int)posXZ.y);
        if (posXZ.x < 0) cell.x--;
        if (posXZ.y < 0) cell.y--;

        if (cell != currentCell)
        {
            currentCell = cell;
            Debug.Log("chunk change");
            noiseGenerator.GenerateNoise(this.WorldOffset);
            blocksGenerator.Generate(this.WorldOffset);
            this.faceBuffer.SetCounterValue(0);
            faceCuller.Cull();
        }
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
        foreach (var i in output) Debug.Log(i);
        tempBuffer.Dispose();
    }

    [Sirenix.OdinInspector.Button]
    private void Test2()
    {
        float[] test = new float[12];
        this.uvDepthBuffer.GetData(test);
        foreach (var i in test) Debug.Log(i);
    }

    private void PopulateVertexBuffer()
    {
        var vertices = this.blockMesh.vertices;
        this.vertexBuffer = new ComputeBuffer(24, sizeof(float) * 3);
        this.vertexBuffer.SetData(vertices);
    }
    private void InitializeMapGeneration()
    {
        blocksGenerator.Initialize(noiseGenerator.Width, noiseGenerator.Height, this.maxHeight, noiseGenerator.TargetTexture);
        faceCuller.Initialize(noiseGenerator.Width, noiseGenerator.Height, this.maxHeight);
        frustumCuller.Initialize(noiseGenerator.Width, noiseGenerator.Height, this.maxHeight);
        duplicatesRemover.Initialize(noiseGenerator.Width * noiseGenerator.Height * maxHeight);


        this.drawMaterial.SetBuffer("instanceBuffer", instanceBuffer);
        this.drawMaterial.SetBuffer("positionBuffer", this.vertexBuffer);
        this.drawMaterial.SetBuffer("faceBuffer", this.frustumBuffer);
        this.drawMaterial.SetBuffer("indexBuffer", this.indexBuffer);
        this.drawMaterial.SetBuffer("uvBuffer", this.uvBuffer);
        //this.drawMaterial.SetTexture("_Textures", this.CreateTextureArray());
    }
    private void GenerateBaseWorldAround(Vector3 playerPos)
    {
        ComputeBuffer.CopyCount(faceBuffer, frustumArgsBuffer, 0);
        frustumCuller.Cull();
        ComputeBuffer.CopyCount(frustumBuffer, drawArgsBuffer, 4);
    }

    private void LoadBaseBlocks(Vector2 offset)
    {
        noiseGenerator.GenerateNoise(offset);
        blocksGenerator.Generate(offset);

        this.duplicatesRemover.Perform();

        int[] temp = new int[1];
        var tempBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer.CopyCount(duplicatesRemover.OutputBuffer, tempBuffer, 0);
        tempBuffer.GetData(temp);
        var output = new int[temp[0]];
        duplicatesRemover.OutputBuffer.GetData(output);
        output[0] = 2;
        output[1] = 1;

        this.drawMaterial.SetTexture("_Textures", this.CreateTextureArray(output));

        var uvDepthList = new List<float>();

        var map = new int[4];

        var currentCount = 0;
        for (int i = 0; i < output.Length; i++)
        {
            var blockID = output[i] - 1;
            map[blockID] = i;
            foreach (var uvDepth in blockInfos[blockID].uvDepthMap)
            {
                uvDepthList.Add(uvDepth + currentCount);
            }
            currentCount += blockInfos[blockID].textures.Count;
        }

        this.uvDepthMapping = new ComputeBuffer(4, sizeof(int), ComputeBufferType.Structured);
        this.uvDepthMapping.SetData(map);

        this.uvDepthBuffer = new ComputeBuffer(uvDepthList.Count, sizeof(int));
        this.uvDepthBuffer.SetData(uvDepthList);

        this.drawMaterial.SetBuffer("uvDepthBuffer", this.uvDepthBuffer);
        this.drawMaterial.SetBuffer("uvDepthMap", this.uvDepthMapping);
        this.drawMaterial.SetVectorArray("dirs", new Vector4[]{
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, -1),
            new Vector3(0, -1, 0),
            new Vector3(-1, 0, 0),
            new Vector3(1, 0, 0)
        });

        tempBuffer.Dispose();

        faceCuller.Cull();

    }

    private void InitArgsBuffer()
    {
        uint[] args = { 6, 0, 0, 0, 0 };
        this.drawArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        this.drawArgsBuffer.SetData(args);
    }

    private void DrawBlocks(Vector3 center)
    {
        this.bounds.center = center;
        Graphics.DrawProceduralIndirect(this.drawMaterial, this.bounds, MeshTopology.Triangles, this.drawArgsBuffer);
    }

    private void PopulateIndexBuffer()
    {
        this.indexBuffer = new ComputeBuffer(this.blockMesh.triangles.Length, sizeof(int));
        indexBuffer.SetData(this.blockMesh.triangles);
    }

    private void PopulateUVBuffer()
    {
        var uv = this.blockMesh.uv;

        uv[6] = new Vector2(1, 0);
        uv[7] = new Vector2(0, 0);
        uv[11] = new Vector2(0, 1);
        uv[10] = new Vector2(1, 1);

        this.uvBuffer = new ComputeBuffer(24, sizeof(float) * 2);
        this.uvBuffer.SetData(uv);
    }

    private void PopulateUVDepthBuffer()
    {
        this.uvDepthBuffer = new ComputeBuffer(6, sizeof(int));
        float[] datas = { 0, 1, 0, 2, 0, 0 };
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
        this.uvDepthMapping.Dispose();
    }

    private Texture2DArray CreateTextureArray(int[] availableBlocks)
    {
        var texList = new List<Texture2D>();
        foreach (var blockID in availableBlocks)
        {
            foreach (var tex in blockInfos[blockID - 1].textures)
            {
                texList.Add(tex);
            }
        }
        Texture2DArray texArray = new Texture2DArray(16, 16, texList.Count, blockInfos[0].textures[0].format, false)
        {
            filterMode = FilterMode.Point
        };
        for (int i = 0; i < texList.Count; i++)
        {
            texArray.SetPixels32(texList[i].GetPixels32(), i);
        }
        texArray.Apply();

        return texArray;
    }
}

[System.Serializable]
public struct InstanceData
{
    public Matrix4x4 trs;
    public int blockType;
    public static int Size => 16 * sizeof(float) + sizeof(int);
}
public struct FaceData
{
    public int instanceIndex;
    public int vertexIndex;
    public static int Size => sizeof(int) * 2;
}
