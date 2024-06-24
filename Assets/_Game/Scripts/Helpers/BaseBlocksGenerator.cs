using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBlocksGenerator : MonoBehaviour
{
    [SerializeField] private ComputeShader mapGenShader;
    [SerializeField] private ComputeBuffer instanceBuffer;

    public ComputeBuffer InstanceBuffer => this.instanceBuffer;

    private int width, height, maxHeight;

    public void Initialize(int width, int height, int maxHeight, Texture targetTexture){
        this.instanceBuffer = new ComputeBuffer(width * height * maxHeight, InstanceData.Size, ComputeBufferType.Structured);

        this.width = width;
        this.height = height;
        this.maxHeight = maxHeight;

        mapGenShader.SetBuffer(0, "instancesData", this.instanceBuffer);
        mapGenShader.SetTexture(0, "Input", targetTexture);
        mapGenShader.SetFloat("size", width);
        mapGenShader.SetFloat("maxHeight", maxHeight);
        mapGenShader.SetInt("width", width);
        mapGenShader.SetInt("height", height);
        //mapGenShader.SetBuffer(0, "testBuffer", );
    }
    public void Generate(Vector2 camPos){
        mapGenShader.SetVector("offset", camPos.XZ());
        mapGenShader.Dispatch(0, Mathf.CeilToInt(this.width / 8), Mathf.CeilToInt(this.height / 8), Mathf.CeilToInt(maxHeight / 8));
    }
}
