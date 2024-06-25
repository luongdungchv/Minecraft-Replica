using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUCharacterMovement : Sirenix.OdinInspector.SerializedMonoBehaviour
{
    [SerializeField] private GPUEntityMovementDispatch movementDispatcher;
    [SerializeField] private ComputeShader characterMoveShader, dataRetrieveShader;
    [SerializeField] private float gravityAcceleration;
    [SerializeField] private float speed, rotationSpd;
    [SerializeField] private Matrix4x4 logMatrix;
    [SerializeField] private Vector3 logVector;
    [SerializeField] private GameObject markerCube;

    private int entityID;
    private EntityData[] dataStore;
    private ComputeBuffer dataBuffer;

    private void Awake(){
        var entity = new EntityData(){
            movement = Vector3.zero,
            extents = new Vector3(0.3f, 1f, 0.3f),
            trs = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one),
            fallingSpd = 0,
            orientation = transform.eulerAngles.y,
        };
        this.entityID = movementDispatcher.RegisterEntity(entity);
        this.dataStore = new EntityData[1];
        this.dataBuffer = new ComputeBuffer(1, EntityData.Size);
        dataBuffer.SetData(dataStore);
    }

    private void Start(){
        this.dataRetrieveShader.SetBuffer(0, "entity", dataBuffer);
        this.dataRetrieveShader.SetBuffer(0, "entityBuffer", movementDispatcher.EntityBuffer);
        this.characterMoveShader.SetBuffer(0, "entityBuffer", movementDispatcher.EntityBuffer);
    }

    private void Update() {
        var x = Input.GetAxisRaw("Horizontal");
        var z = Input.GetAxisRaw("Vertical");
        var moveVector = (transform.right * x + transform.forward.Set(y: 0).normalized * z).normalized * speed;
        this.logVector = moveVector;
        moveVector += Vector3.up * (Input.GetKeyDown(KeyCode.Space) ? 5 : 0);
        this.characterMoveShader.SetVector("movement", moveVector);


        this.PerformRotation(); 
        this.characterMoveShader.Dispatch(0, 1, 1, 1);
    }

    private void PerformRotation(){
        var rotY = Input.GetAxis("Mouse X");
        var rotX = Input.GetAxis("Mouse Y");
        var eulers = transform.eulerAngles;
        eulers.y += rotY * rotationSpd * Time.deltaTime;
        eulers.x -= rotX * rotationSpd * Time.deltaTime;

        //characterMoveShader.SetFloat("orientation", eulers.y); 
        transform.eulerAngles = eulers;
    }

    private void LateUpdate() {
        this.dataRetrieveShader.Dispatch(0, 1, 1, 1);
        dataBuffer.GetData(this.dataStore);
        this.logMatrix = dataStore[0].trs;
        transform.position = (Vector3)dataStore[0].trs.GetColumn(3);
        //transform.eulerAngles = transform.eulerAngles.Set(y: dataStore[0].orientation);
        markerCube.transform.position = transform.position;
    }

    private void OnDestroy() {
        this.dataBuffer.Dispose();
    }
}
