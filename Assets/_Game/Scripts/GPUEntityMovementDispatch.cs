using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUEntityMovementDispatch : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private ComputeShader movementHandleShader;
    [SerializeField] private ComputeBuffer entityBuffer, logBuffer;
    private List<EntityData> entitiesDataList;
    private LogData[] logData;

    public ComputeBuffer EntityBuffer => this.entityBuffer;

    void Awake()
    {
        this.entityBuffer = new ComputeBuffer(this.entitiesDataList.Count, EntityData.Size, ComputeBufferType.Structured);
        this.logBuffer = new ComputeBuffer(1, LogData.Size, ComputeBufferType.Structured);
    }

    private void Start() {
        Debug.Log(entitiesDataList[0].trs);

        this.logData = new LogData[1];
        logData[0] = new LogData(){
            normal = Vector3.one,
            result = 0
        };
 
        this.entityBuffer.SetData(entitiesDataList);
        this.logBuffer.SetData(logData);

        this.movementHandleShader.SetBuffer(0, "entityBuffer", this.entityBuffer);
        this.movementHandleShader.SetBuffer(0, "logBuffer", this.logBuffer);
        this.movementHandleShader.SetBuffer(0, "instanceBuffer", MapGenerator.Instance.GetComponent<BaseBlocksGenerator>().InstanceBuffer);
        this.movementHandleShader.SetInt("maxHeight", MapGenerator.Instance.MaxHeight);
        this.movementHandleShader.SetInt("size", MapGenerator.Instance.Size);
    }
    void Update()
    {
        this.EntitySystemDispatch();
    }
    public int RegisterEntity(EntityData data){
        this.entitiesDataList ??= new List<EntityData>();
        this.entitiesDataList.Add(data);
        return this.entitiesDataList.Count - 1;
    }
    private void EntitySystemDispatch(){
        this.movementHandleShader.SetFloat("deltaTime", Time.deltaTime);
        this.movementHandleShader.Dispatch(0, Mathf.CeilToInt((float)this.entitiesDataList.Count / 8), 1, 1);
    }

    public EntityData GetEntityData(int index){
        return this.entitiesDataList[index];
    }

    private void OnDestroy() {
        this.entityBuffer.Dispose();
        this.logBuffer.Dispose();
    }

    [Sirenix.OdinInspector.Button]
    private void Test(){
        var data = new LogData[1];
        this.logBuffer.GetData(data);
        Debug.Log((data[0].normal, data[0].result));
    }
}

public struct EntityData{
    public Vector3 movement;
    public Vector3 extents;
    public Matrix4x4 trs;
    public float fallingSpd;
    public float orientation;
    public static int Size => sizeof(float) * 24;
}

public struct LogData{
    public Vector3 normal, test1, test2;
    public int result;
    public static int Size => sizeof(float) * 9 + sizeof(int);
}
