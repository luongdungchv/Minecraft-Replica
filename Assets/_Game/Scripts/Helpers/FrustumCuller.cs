using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrustumCuller : MonoBehaviour
{
    [SerializeField] private ComputeShader frustumCullShader, frustumArgsFillerShader;
    private ComputeBuffer frustumBuffer, frustumArgsBuffer;

    public ComputeBuffer ArgsBuffer => this.frustumArgsBuffer;
    public ComputeBuffer FrustumBuffer => this.frustumBuffer;

    private int width, height, maxHeight;

    public void Initialize(int width, int height, int maxHeight){
        this.width = width;
        this.height = height;  
        this.maxHeight = maxHeight;

        uint[] args = new uint[]{
            1,1,1
        };
        this.frustumArgsBuffer = new ComputeBuffer(3, sizeof(uint), ComputeBufferType.IndirectArguments);
        this.frustumArgsBuffer.SetData(args);
        this.frustumArgsFillerShader.SetBuffer(0, "args", this.frustumArgsBuffer);

        this.frustumBuffer = new ComputeBuffer(width * height * maxHeight * 6, FaceData.Size, ComputeBufferType.Append);
        frustumBuffer.SetCounterValue(0);
        frustumCullShader.SetBuffer(0, "instanceBuffer", this.GetComponent<BaseBlocksGenerator>().InstanceBuffer);
        frustumCullShader.SetBuffer(0, "faceBuffer", this.GetComponent<FaceCuller>().FaceBuffer);
        frustumCullShader.SetBuffer(0, "result", this.frustumBuffer);       
    }

    public void Cull()
    {
        var p = Camera.main.projectionMatrix;
        p.SetRow(0, new Vector4(0.84839f, 0, 0, 0));
        p.SetRow(1, new Vector4(0f, 1.51084f, 0, 0));
        var v = Camera.main.worldToCameraMatrix;
        var VP = p * v;
        frustumCullShader.SetMatrix("vp", VP);
        frustumCullShader.SetVector("camPos", Camera.main.transform.position);
        frustumCullShader.DispatchIndirect(0, this.frustumArgsBuffer);
    }
}
