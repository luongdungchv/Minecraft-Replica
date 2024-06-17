using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCuller : MonoBehaviour
{
    [SerializeField] private ComputeShader faceCullShader;
    [SerializeField] private ComputeBuffer faceBuffer;

    private int width, height, maxHeight;

    public ComputeBuffer FaceBuffer => this.faceBuffer;

    private Camera mainCam;

    private void Awake() {
        mainCam = Camera.main;
    }

    private ComputeBuffer instanceBuffer => GetComponent<BaseBlocksGenerator>().InstanceBuffer;
    public void Initialize(int width, int height, int maxHeight){
        this.width = width;
        this.height = height;  
        this.maxHeight = maxHeight;

        this.faceBuffer = new ComputeBuffer(width * height * maxHeight * 6, FaceData.Size, ComputeBufferType.Append);
        faceBuffer.SetCounterValue(0);

        faceCullShader.SetBuffer(0, "Input", instanceBuffer);
        faceCullShader.SetBuffer(0, "Result", faceBuffer);
        //faceCullShader.SetBuffer(0, "testResult", testBuffer);
        faceCullShader.SetInt("width", this.width);
        faceCullShader.SetInt("height", this.height);
        faceCullShader.SetFloat("maxHeight", this.maxHeight);
        faceCullShader.SetVectorArray("dirs", new Vector4[]{
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, -1, 0),
            new Vector3(0, 0, -1),
            new Vector3(-1, 0, 0),
            new Vector3(1, 0, 0)
        });
    }

    public void Cull(Vector3 camPos){
        faceCullShader.SetVector("camPos", mainCam.transform.position);
        faceCullShader.SetVector("camForward", mainCam.transform.forward);
        faceCullShader.Dispatch(0, Mathf.CeilToInt(width / 8), Mathf.CeilToInt(height / 8), Mathf.CeilToInt(maxHeight / 8));
    }
}
