#ifndef STRUCT_DEFINITION_HLSL
#define STRUCT_DEFINITION_HLSL

//Ray data
struct Ray{
    float2 origin;
    float2 direction;
};

//Vertex Data
struct ColliderEdge
{
    float2 start;
    float2 end;
};

struct HitBox
{
    float2 min;
    float2 max;
};

struct RayTracingMaterial
{
    float roughness;
    float transmission;
    float emission;
    float3 color;
};

//Ray Hit data
struct GeometryData
{
    HitBox hitBox;
    RayTracingMaterial material;
    int edgeStartIndex;
    int edgeCount;
};


//Ray Hit data
struct HitData
{
    bool didHit;
    float distance;
    float2 hitPoint;
    float2 normal;
    RayTracingMaterial material;
    int edgeIndex;
    int objectIndex;
};

#endif