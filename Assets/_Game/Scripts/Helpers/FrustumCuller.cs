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
    private Matrix4x4 p;

    private void Awake(){
        p = Camera.main.projectionMatrix;
        var tanAngle = Mathf.Tan(90 / 2 * Mathf.Deg2Rad);
        p.SetRow(0, new Vector4(1 / (tanAngle * Camera.main.aspect), 0, 0, 0));
        p.SetRow(1, new Vector4(0f, 1 / tanAngle, 0, 0));
    }

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
        frustumCullShader.SetVectorArray("dirs", new Vector4[]{
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, -1),
            new Vector3(0, -1, 0),
            new Vector3(-1, 0, 0),
            new Vector3(1, 0, 0)
        });      
    }

    public void Cull()
    {
        frustumArgsFillerShader.Dispatch(0, 1, 1, 1);     
        var v = Camera.main.worldToCameraMatrix;
        var VP = p * v;
        frustumCullShader.SetMatrix("vp", VP);
        frustumCullShader.SetVector("camPos", Camera.main.transform.position);
        frustumCullShader.SetVector("camForward", Camera.main.transform.forward);
        frustumCullShader.DispatchIndirect(0, this.frustumArgsBuffer);
    }
}
