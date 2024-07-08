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
        baseMapGenShader.SetVectorArray("dirs", new Vector4[]{
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(-1, 0)
        });
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

    [Sirenix.OdinInspector.Button]
    private void TestGenFinal(int zoomLevel){
        if(zoomLevel < 2) return;
        this.baseMapGenShader.SetInts("offset", this.testOffset.x, this.testOffset.y);
        this.baseMapGenShader.SetFloat("seed", this.testSeed);
        this.baseMapGenShader.SetFloat("zoomLevel", zoomLevel);
        
        var finalGenKernel = this.baseMapGenShader.FindKernel("GenerateFinalPhase");
        var transferNoCutKernel = this.baseMapGenShader.FindKernel("TransferTempNoCut");
        
        this.baseMapGenShader.SetTexture(finalGenKernel, "BiomeMap", biomeMapTex);
        this.baseMapGenShader.SetTexture(finalGenKernel, "tempMap", tempMapTex);
        this.baseMapGenShader.Dispatch(finalGenKernel, (int)Mathf.Pow(2, zoomLevel - 2), (int)Mathf.Pow(2, zoomLevel - 2), 1);
        
        this.baseMapGenShader.SetTexture(transferNoCutKernel, "BiomeMap", biomeMapTex);
        this.baseMapGenShader.SetTexture(transferNoCutKernel, "tempMap", tempMapTex);
        this.baseMapGenShader.Dispatch(transferNoCutKernel, (int)Mathf.Pow(2, zoomLevel - 2), (int)Mathf.Pow(2, zoomLevel - 2), 1);
    }
    
    [Sirenix.OdinInspector.Button]
    private void RemoveIsolated(){
        var removeIsolatedKernel = this.baseMapGenShader.FindKernel("RemoveIsolated");
        this.baseMapGenShader.SetTexture(removeIsolatedKernel, "BiomeMap", biomeMapTex);
        this.baseMapGenShader.Dispatch(removeIsolatedKernel, 32, 32, 1);
    }
}
