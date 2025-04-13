using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingMaterial : MonoBehaviour
{
    [Range(0, 1)]
    public float roughness;
    public float emission;
    public Color color;

    [Range(0, 1)]
    public float transmission;
}
