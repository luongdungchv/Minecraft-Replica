using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomesGenerator : MonoBehaviour
{
    [SerializeField] private RenderTexture biomeMapTex, tempMapTex, secondTempMapTex;
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
    private void TestAddIsland()
    {
        this.baseMapGenShader.SetInts("offset", this.testOffset.x, this.testOffset.y);
        this.baseMapGenShader.SetFloat("seed", this.testSeed);
        this.baseMapGenShader.SetFloat("zoomLevel", this.testZoomLevel);

        var addIslandKernel = this.baseMapGenShader.FindKernel("AddIslands");
        var transferSecondTempKernel = this.baseMapGenShader.FindKernel("TransferTempToSec");
        
        this.baseMapGenShader.SetTexture(addIslandKernel, "tempMap", tempMapTex);
        this.baseMapGenShader.SetTexture(addIslandKernel, "secondTempMap", secondTempMapTex);
        var size = testZoomLevel >= 8 ? (int)Mathf.Pow(2, testZoomLevel - 8) : 1;
        this.baseMapGenShader.Dispatch(addIslandKernel, size, size, 1);
        
        
    }

    [Sirenix.OdinInspector.Button]
    private void TestTransferTempToSec(){
         var transferSecondTempKernel = this.baseMapGenShader.FindKernel("TransferTempToSec");
        var size = testZoomLevel >= 8 ? (int)Mathf.Pow(2, testZoomLevel - 8) : 1;
        this.baseMapGenShader.SetTexture(transferSecondTempKernel, "tempMap", tempMapTex);
        this.baseMapGenShader.SetTexture(transferSecondTempKernel, "secondTempMap", secondTempMapTex);
        this.baseMapGenShader.Dispatch(transferSecondTempKernel, 1, 1, 1);
    }
    
    [Sirenix.OdinInspector.Button]
    private void TestTransferTemp(){
        this.baseMapGenShader.SetInts("offset", this.testOffset.x, this.testOffset.y);
        this.baseMapGenShader.SetFloat("seed", this.testSeed);
        this.baseMapGenShader.SetFloat("zoomLevel", this.testZoomLevel);
        
        var transferTempKernel = this.baseMapGenShader.FindKernel("TransferTemp");
        var transferTempNoCutKernel = this.baseMapGenShader.FindKernel("TransferTempNoCut");
        this.baseMapGenShader.SetTexture(transferTempKernel, "tempMap", tempMapTex);
        this.baseMapGenShader.SetTexture(transferTempKernel, "BiomeMap", biomeMapTex);
        var size = testZoomLevel >= 8 ? (int)Mathf.Pow(2, testZoomLevel - 8) : 1;
        if(testZoomLevel < 8){
            this.baseMapGenShader.Dispatch(transferTempKernel, 1, 1, 1);
        }
        else this.baseMapGenShader.Dispatch(transferTempNoCutKernel, size, size, 1);

    }

    [Sirenix.OdinInspector.Button]
    private void TestGenBase()
    {
        this.baseMapGenShader.SetInts("offset", this.testOffset.x, this.testOffset.y);
        this.baseMapGenShader.SetFloat("seed", this.testSeed);
        this.baseMapGenShader.SetFloat("zoomLevel", 2);
        
        baseMapGenShader.SetVectorArray("dirs", new Vector4[]{
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(-1, 0)
        });

        var baseMapKernel = this.baseMapGenShader.FindKernel("GenerateBase");
        this.baseMapGenShader.SetTexture(baseMapKernel, "BiomeMap", biomeMapTex);
        this.baseMapGenShader.Dispatch(baseMapKernel, 1, 1, 1);
        
        
    }

    [Sirenix.OdinInspector.Button]
    private void TestGenFinal(){
        var zoomLevel = this.testZoomLevel - 6;
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
    [Sirenix.OdinInspector.Button]
    private void TestZoomIn(){
        var zoomKernel = this.baseMapGenShader.FindKernel("GenerateZoomLevel");
        this.baseMapGenShader.SetTexture(zoomKernel, "secondTempMap", secondTempMapTex);
        this.baseMapGenShader.SetTexture(zoomKernel, "BiomeMap", biomeMapTex);
        this.baseMapGenShader.Dispatch(zoomKernel, 1,1,1);
    }
    
    [Sirenix.OdinInspector.Button]
    private void TestRemoveOceans(int zoomLevel){
        var removeOceansKernel = this.baseMapGenShader.FindKernel("RemoveOceans");
        this.baseMapGenShader.SetTexture(removeOceansKernel, "BiomeMap", biomeMapTex);
        this.baseMapGenShader.Dispatch(removeOceansKernel, (int)Mathf.Pow(2, zoomLevel), (int)Mathf.Pow(2, zoomLevel), 1);
    }
}
