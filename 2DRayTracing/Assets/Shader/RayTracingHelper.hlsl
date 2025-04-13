#ifndef RAY_TRACING_HELPER_HLSL
#define RAY_TRACING_HELPER_HLSL

#include "StructDefinition.hlsl"

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


#endif