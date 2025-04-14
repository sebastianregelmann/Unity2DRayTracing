#ifndef RAY_TRACING_HELPER_HLSL
#define RAY_TRACING_HELPER_HLSL

#include "StructDefinition.hlsl"

//Prototype definition
bool inHitBox(float2 position, HitBox hitBox);
bool rayEdgeIntersection(ColliderEdge edge, Ray ray, out float2 hitPoint);
Ray generateStartRay(int index, float2 rayOrigin, int totalRayCount);
Ray generateRandomRay(float2 rayOrigin, inout uint rngSeed);
uint nextRandom(inout uint rngSeed);
float randomValue(inout uint rngSeed);


//checks if a position is inside a HitBox
bool inHitBox(float2 position, HitBox hitBox)
{
    if(position.x >= hitBox.min.x && position.x <= hitBox.max.x )
    {
        if(position.y >= hitBox.min.y && position.y <= hitBox.max.y)
        {
            return true;
        }
    }
    return false;
}


//Checks if a ray intersects with a hitbox
bool rayHitBoxIntersection(Ray ray, HitBox hitBox)
{
    float2 dir = ray.direction;

    // Avoid divide - by - zero with large number fallback
    float2 invDir = 1.0 / (abs(dir) < 0.000001 ? sign(dir) * 0.000001 : dir);

    float2 t1 = (hitBox.min - ray.origin) * invDir;
    float2 t2 = (hitBox.max - ray.origin) * invDir;

    float2 tMin = min(t1, t2);
    float2 tMax = max(t1, t2);

    float tNear = max(tMin.x, tMin.y);
    float tFar = min(tMax.x, tMax.y);

    // tFar < 0 = box is behind the ray
    // tNear > tFar = ray misses box
    return tFar >= 0 && tNear <= tFar;
}


float2 getNormal(Ray ray, ColliderEdge edge)
{ // Edge vector (from start to end)
    float2 edgeVec = edge.end - edge.start;

    // Perpendicular vector (rotate edge vector 90° counter - clockwise)
    float2 normal = float2(- edgeVec.y, edgeVec.x);

    // Normalize the normal
    normal = normalize(normal);

    // Ensure normal faces opposite the ray direction
    if (dot(normal, ray.direction) > 0)
    {
        normal = - normal;
    }

    return normal;
}

//Checks if a ray intersect with an Edge
bool rayEdgeIntersection(ColliderEdge edge, Ray ray, out float2 hitPoint)
{
    float2 edgeDir = edge.end - edge.start;

    // Solve for intersection using line parametric form
    float2 v = ray.origin - edge.start;

    float det = edgeDir.x * (- ray.direction.y) - edgeDir.y * (- ray.direction.x);

    // If det is close to 0, the lines are parallel
    if (abs(det) < 0.00001)
    {
        hitPoint = float2(0.0, 0.0);
        return false;
    }

    float t = (v.x * - ray.direction.y - v.y * - ray.direction.x) / det;
    float u = (v.x * edgeDir.y - v.y * edgeDir.x) / det;

    // t must be between 0 and 1 (within the edge), u >= 0 (in front of ray)
    if (t >= 0.0 && t <= 1.0 && u >= 0.0)
    {
        hitPoint = edge.start + t * edge.end;
        return true;
    }

    hitPoint = float2(0.0, 0.0);
    return false;
}

//Generates a ray based on the index and the number of total rays
Ray generateStartRay(int index, float2 rayOrigin, int totalRayCount)
{
    // Angle per ray in radians
    float angle = (2.0 * 3.14159265) * (index / (float)totalRayCount);

    // Convert polar to Cartesian
    float2 rayDirection = normalize(float2(cos(angle), sin(angle)));

    //Create the ray
    Ray newRay;
    newRay.origin = rayOrigin;
    newRay.direction = rayDirection;

    return newRay;
}


//Generates a ray into a random direction
Ray generateRandomRay(float2 rayOrigin, inout uint rngSeed)
{
    // Angle per ray in radians
    float angle = randomValue(rngSeed) * 2.0 * 3.14159265;

    // Convert polar to Cartesian
    float2 rayDirection = normalize(float2(cos(angle), sin(angle)));

    //Create the ray
    Ray newRay;
    newRay.origin = rayOrigin;
    newRay.direction = rayDirection;

    return newRay;
}


//Generates a random Integer
uint nextRandom(inout uint rngSeed)
{
    rngSeed = rngSeed * 747796405 + 2891336453;
    uint result = ((rngSeed >> ((rngSeed >> 28) + 4)) ^ rngSeed) * 277803737;
    result = (result >> 22) ^ result;
    return result;
}

//Generates a random float between 0 and 1
float randomValue(inout uint rngSeed)
{
    return nextRandom(rngSeed) / 4294967295.0; // 2 ^ 32 - 1
}



Ray getRayReflection(Ray originalRay, HitData hit, uint rngSeed)
{

    float2 perfectReflection = originalRay.direction - 2.0 * dot(originalRay.direction, hit.normal) * hit.normal;

    bool positiv = randomValue(rngSeed) >= 0.5;

    float maxAngle = hit.material.roughness * 3.14159265 / 2;
    maxAngle = positiv ? maxAngle : - maxAngle;

    float angle = lerp(0, maxAngle, randomValue(rngSeed));

    float2 rotatedDirection = float2(perfectReflection.x * cos(angle) - perfectReflection.y * sin(angle), perfectReflection.x * sin(angle) + perfectReflection.y * cos(angle));

    Ray result;
    result.origin = hit.hitPoint;
    result.direction = rotatedDirection;
    return result;
}


#endif