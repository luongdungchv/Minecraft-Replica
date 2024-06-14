using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    [SerializeField] private ComputeShader noiseGeneratorCS;
    [SerializeField] private RenderTexture targetTexture;
    [SerializeField] private float noiseScale;
    [SerializeField] private Vector2 testOffset;
    public RenderTexture NoiseTexture => this.targetTexture;

    private void Start() {
        this.GenerateNoise(this.testOffset);
    }

    public void GenerateNoise(Vector2 offset){
        var kernelIndex = noiseGeneratorCS.FindKernel("GenerateNoise");

        noiseGeneratorCS.SetTexture(kernelIndex, "Result", targetTexture);
        noiseGeneratorCS.SetFloat("noiseScale", noiseScale);
        noiseGeneratorCS.SetVector("offset", offset);
        
        var textureWidth = (float)targetTexture.width;
        var textureHeight = (float)targetTexture.height;

        noiseGeneratorCS.Dispatch(kernelIndex, Mathf.CeilToInt(textureWidth / 8), Mathf.CeilToInt(textureHeight / 8), 1);
    }



    // private void Update(){
    //     GenerateNoise(testOffset);
    // }
    
}
