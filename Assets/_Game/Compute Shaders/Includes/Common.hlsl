struct FaceData{
    uint instanceIndex;
    uint vertexIndex;
};
struct InstanceData{
    float4x4 trs;
    int blockType;
}; 
struct EntityData{
    float3 movement;
    float3 extents;
    float4x4 trs; 
    float fallingSpd;
    float orientation;
};