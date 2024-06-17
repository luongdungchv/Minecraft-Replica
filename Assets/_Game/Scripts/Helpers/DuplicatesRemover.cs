using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuplicatesRemover : MonoBehaviour
{
    [SerializeField] private ComputeShader duplicateRemoveShader;
    [SerializeField] private ComputeBuffer checkerBuffer, outputBuffer;

    public ComputeBuffer CheckerBuffer => this.checkerBuffer;
    public ComputeBuffer OutputBuffer => this.outputBuffer;

    private int inputBufferSize;

    public void Initialize(int inputBufferSize){
        checkerBuffer = new ComputeBuffer(5, sizeof(int));
        checkerBuffer.SetData(new int[]{0,0,0});
        outputBuffer = new ComputeBuffer(5, sizeof(int), ComputeBufferType.Append);
        outputBuffer.SetCounterValue(0);

        duplicateRemoveShader.SetBuffer(0, "input", this.GetComponent<BaseBlocksGenerator>().InstanceBuffer);
        duplicateRemoveShader.SetBuffer(0, "checker", this.checkerBuffer);
        duplicateRemoveShader.SetBuffer(0, "output", this.outputBuffer);

        this.inputBufferSize = inputBufferSize;
    }
    public void Perform(){
        this.duplicateRemoveShader.Dispatch(0, Mathf.CeilToInt((float)inputBufferSize / 128), 1, 1);
    }

    private void OnDestroy() {
        this.checkerBuffer.Dispose();
        this.outputBuffer.Dispose();
    }
}
