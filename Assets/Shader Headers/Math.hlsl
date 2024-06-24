struct Box{
    float3 center;
    float3 extents;
    float orientation;
};

float2 GetProjectionRange(Box b, float2 projMask){
    float2 dirs[] = {
        float2(1, 1),
        float2(-1, 1),
        float2(-1, -1),
        float2(1, -1)
    };

    float minVal = 2147483647;
    float maxVal = -minVal;

    for(int i = 0; i < 4; i++){
        float2 dir = dirs[i] * b.extents.xz;
        float2 corner = dir;

        float2 temp = corner;
        float sine = sin(b.orientation);
        float cosine = cos(b.orientation);

        corner.x = temp.x * cosine - temp.y * sine;
        corner.y = temp.x * sine + temp.y * cosine;
        corner += b.center.xz;

        corner *= projMask;
        float length = corner.x + corner.y;
        if(length < minVal){
            minVal = length;
        }
        if(length > maxVal) maxVal = length;
    }

    return float2(minVal, maxVal);
}

bool IsCubesOverlap(Box b1, Box b2, out float3 normal){
    float2 b1X = GetProjectionRange(b1, float2(1, 0));
    float2 b2X = GetProjectionRange(b2, float2(1, 0));

    float2 b1Y = GetProjectionRange(b1, float2(0, 1));
    float2 b2Y = GetProjectionRange(b2, float2(0, 1));

    float b1XCenter = (b1X.x + b1X.y) / 2;
    float b2XCenter = (b2X.x + b2X.y) / 2;
    float collideDistX = (b1X.y - b1X.x) / 2 + (b2X.y - b2X.x) / 2;
    float trueDistX = abs(b1XCenter - b2XCenter);

    float b1YCenter = (b1Y.x + b1Y.y) / 2;
    float b2YCenter = (b2Y.x + b2Y.y) / 2;
    float collideDistY = (b1Y.y - b1Y.x) / 2 + (b2Y.y - b2Y.x) / 2;
    float trueDistY = abs(b1YCenter - b2YCenter);

    float collideDistZ = (b1.extents.y + b2.extents.y);
    float trueDistZ = abs(b1.center.y - b2.center.y);

    float xDiff = trueDistX - collideDistX;
    float yDiff = trueDistY - collideDistY;
    float zDiff = trueDistZ - collideDistZ;

    if(xDiff > 0 || yDiff > 0 || zDiff > 0){
        normal = float3(0,0,0);
        return false;
    }
    float maxDiff = max(max(xDiff, yDiff), zDiff);
    if(xDiff == maxDiff) normal = float3(b1XCenter > b2XCenter ? 1 : -1, 0, 0);
    else if(yDiff == maxDiff) normal = float3(0, 0, b1YCenter > b2YCenter ? 1 : -1);
    else if(zDiff == maxDiff) normal = float3(0, b1.center.y > b2.center.y ? 1 : -1, 0);
    else normal = float3(0,0,0);
    // normal = float3(xDiff, yDiff, zDiff);
    // normal = float3(b1X, zDiff);
    return true;
}
