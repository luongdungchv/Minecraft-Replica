using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomesGenerator : MonoBehaviour
{
    [SerializeField] private RenderTexture biomeMapTex, tempMapTex, tempMapIntTex;
    [SerializeField] private int initialBiomeCellSize, biomeGenerationSize;
    [SerializeField] private ComputeShader baseMapGenShader;

    [SerializeField] private Vector2Int testOffset;
    [SerializeField] private float testSeed, testZoomLevel;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateBiomesMap(Vector2Int offset, int chunkSize)
    {

    }

    [Sirenix.OdinInspector.Button]
    private void Test()
    {
        this.baseMapGenShader.SetInts("offset", this.testOffset.x, this.testOffset.y);
        this.baseMapGenShader.SetFloat("seed", this.testSeed);
        this.baseMapGenShader.SetFloat("zoomLevel", this.testZoomLevel);

        baseMapGenShader.SetVectorArray("dirs", new Vector4[]{
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(-1, 0)
        });

        var baseMapKernel = this.baseMapGenShader.FindKernel("GenerateBase");
        var zoomKernel = this.baseMapGenShader.FindKernel("GenerateZoomLevel");
        var clearTempKernel = this.baseMapGenShader.FindKernel("ClearTempMap");
        var tempCopyKernel = this.baseMapGenShader.FindKernel("CopyIntMapToFloatMap");
        var transferTempKernel = this.baseMapGenShader.FindKernel("TransferTemp");

        this.baseMapGenShader.SetTexture(clearTempKernel, "tempMapInt", tempMapIntTex);
        this.baseMapGenShader.Dispatch(clearTempKernel, 1, 1, 1);

        this.baseMapGenShader.SetTexture(tempCopyKernel, "tempMap", tempMapTex);
        this.baseMapGenShader.SetTexture(tempCopyKernel, "tempMapInt", tempMapIntTex);
        this.baseMapGenShader.Dispatch(tempCopyKernel, 1, 1, 1);

        // this.baseMapGenShader.SetTexture(baseMapKernel, "BiomeMap", biomeMapTex);
        // this.baseMapGenShader.Dispatch(baseMapKernel,1,1,1);

        this.baseMapGenShader.SetTexture(zoomKernel, "BiomeMap", biomeMapTex);
        this.baseMapGenShader.SetTexture(zoomKernel, "tempMapInt", tempMapIntTex);
        this.baseMapGenShader.SetTexture(zoomKernel, "tempMap", tempMapTex);
        this.baseMapGenShader.Dispatch(zoomKernel, 1, 1, 1);

        // this.baseMapGenShader.SetTexture(clearTempKernel, "tempMap", tempMapTex);

        this.baseMapGenShader.SetTexture(transferTempKernel, "tempMap", tempMapTex);
        this.baseMapGenShader.SetTexture(transferTempKernel, "BiomeMap", biomeMapTex);
        this.baseMapGenShader.Dispatch(transferTempKernel, 1,1,1);
    }

    [Sirenix.OdinInspector.Button]
    private void TestGenBase()
    {
        this.baseMapGenShader.SetInts("offset", this.testOffset.x, this.testOffset.y);
        this.baseMapGenShader.SetFloat("seed", this.testSeed);
        this.baseMapGenShader.SetFloat("zoomLevel", 2);

        var baseMapKernel = this.baseMapGenShader.FindKernel("GenerateBase");
        this.baseMapGenShader.SetTexture(baseMapKernel, "BiomeMap", biomeMapTex);
        this.baseMapGenShader.Dispatch(baseMapKernel, 1, 1, 1);
    }
}
