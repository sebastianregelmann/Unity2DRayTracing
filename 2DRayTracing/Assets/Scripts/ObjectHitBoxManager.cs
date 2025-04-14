using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectHitBoxManager : MonoBehaviour
{
    /// <summary>
    /// List with all relevant Gameobjects
    /// </summary>
    private static List<GameObject> relevantObjects = new List<GameObject>();

    /// <summary>
    /// List with all collider Edges in the scene
    /// </summary>
    public static List<ColliderEdge> colliderEdges = new List<ColliderEdge>();

    /// <summary>
    /// List with all geometry data of objects with 2D Collider
    /// </summary>
    public static List<GeometryData> geometryDatas = new List<GeometryData>();
    void Update()
    {
        //List with all Gameobjects in the scene
        List<GameObject> gameObjectsInScene = FindObjectsOfType<GameObject>().ToList();

        getRelevantObjects(gameObjectsInScene);
        extractGeometryData();
    }


    /// <summary>
    /// Gets all gameobjects from a list that have a 2D Collider and a RayTracing material attached
    /// </summary>
    /// <param name="allObjects"></param>
    private void getRelevantObjects(List<GameObject> allObjects)
    {
        relevantObjects.Clear();

        foreach (GameObject gameObject in allObjects)
        {
            if (gameObject.GetComponent<Collider2D>() != null && gameObject.GetComponent<RayTracingMaterial>() != null)
            {
                relevantObjects.Add(gameObject);
            }
        }
    }


    /// <summary>
    /// Extracst the geometry Data for each object in the Collider List
    /// </summary>
    private void extractGeometryData()
    {
        //Clear the lists
        geometryDatas.Clear();
        colliderEdges.Clear();

        //loop over each collider and extract the geometry data
        foreach (GameObject gameObject in relevantObjects)
        {
            geometryDatas.Add(getGeometryData(gameObject));
        }
    }

    /// <summary>
    /// generates the geometry Data for a 2d collider
    /// </summary>
    private GeometryData getGeometryData(GameObject gameObject)
    {
        Collider2D collider = gameObject.GetComponent<Collider2D>();
        //Get the edges of the collider 
        List<ColliderEdge> currentColliderEdges = getColliderEdges(collider);

        int startIndex = colliderEdges.Count;
        int edgeCount = currentColliderEdges.Count;
        HitBox currentColliderHitBox = getHitBox(collider);
        RayTracingMaterialStruct material = getRayTracingMaterial(gameObject);

        //add the edges to the whole lists
        colliderEdges.AddRange(currentColliderEdges);

        //Return the geometry Data
        return new GeometryData
        {
            hitBox = currentColliderHitBox,
            material = material,
            edgeStartIndex = startIndex,
            edgeCount = edgeCount
        };
    }


    /// <summary>
    /// Gets the edges of a collider
    /// </summary>
    private List<ColliderEdge> getColliderEdges(Collider2D collider)
    {
        switch (collider)
        {
            case EdgeCollider2D edgeCollider:
                return getEdgesEdgeCollider(edgeCollider);

            case BoxCollider2D boxCollider:
                return getEdgesBoxCollider(boxCollider);

            case PolygonCollider2D polygonCollider:
                return getEdgesPolygonCollider(polygonCollider);
            default:
                return new List<ColliderEdge>();
        }
    }




    /// <summary>
    /// Get the edges of a box collider
    /// </summary>
    private List<ColliderEdge> getEdgesBoxCollider(BoxCollider2D collider)
    {
        List<ColliderEdge> edges = new List<ColliderEdge>();

        // Get local half-size
        Vector2 size = collider.size;
        Vector2 offset = collider.offset;
        Vector2 halfSize = size * 0.5f;

        // Local-space corners (relative to the collider's offset)
        Vector2 topLeft = new Vector2(-halfSize.x, halfSize.y) + offset;
        Vector2 topRight = new Vector2(halfSize.x, halfSize.y) + offset;
        Vector2 bottomRight = new Vector2(halfSize.x, -halfSize.y) + offset;
        Vector2 bottomLeft = new Vector2(-halfSize.x, -halfSize.y) + offset;

        // Transform to world space (including rotation)
        Transform transform = collider.transform;

        topLeft = transform.TransformPoint(topLeft);
        topRight = transform.TransformPoint(topRight);
        bottomRight = transform.TransformPoint(bottomRight);
        bottomLeft = transform.TransformPoint(bottomLeft);

        // Add edges
        edges.Add(new ColliderEdge { start = topLeft, end = topRight });
        edges.Add(new ColliderEdge { start = topRight, end = bottomRight });
        edges.Add(new ColliderEdge { start = bottomRight, end = bottomLeft });
        edges.Add(new ColliderEdge { start = bottomLeft, end = topLeft });

        return edges;
    }



    /// <summary>
    /// Get the edges of an EdgeCollider
    /// </summary>
    private List<ColliderEdge> getEdgesEdgeCollider(EdgeCollider2D collider)
    {
        List<ColliderEdge> edges = new List<ColliderEdge>();
        Vector2[] edgePoints = collider.points;
        for (int i = 0; i < edgePoints.Length - 1; i++)
        {
            Vector2 a = collider.transform.TransformPoint(edgePoints[i]);
            Vector2 b = collider.transform.TransformPoint(edgePoints[i + 1]);
            edges.Add(new ColliderEdge { start = a, end = b });
        }

        return edges;
    }


    /// <summary>
    /// Get the edges of an Polygon Collider
    /// </summary>
    private List<ColliderEdge> getEdgesPolygonCollider(PolygonCollider2D collider)
    {
        List<ColliderEdge> edges = new List<ColliderEdge>();
        for (int p = 0; p < collider.pathCount; p++)
        {
            Vector2[] path = collider.GetPath(p);
            for (int i = 0; i < path.Length; i++)
            {
                Vector2 a = collider.transform.TransformPoint(path[i]);
                Vector2 b = collider.transform.TransformPoint(path[(i + 1) % path.Length]);
                edges.Add(new ColliderEdge { start = a, end = b });
            }
        }
        return edges;
    }

    private RayTracingMaterialStruct getRayTracingMaterial(GameObject gameObject)
    {
        RayTracingMaterial rayTracingMaterial = gameObject.GetComponent<RayTracingMaterial>();

        return new RayTracingMaterialStruct
        {
            roughness = rayTracingMaterial.roughness,
            transmission = rayTracingMaterial.transmission,
            emission = rayTracingMaterial.emission,
            color = new Vector3(rayTracingMaterial.color.r, rayTracingMaterial.color.g, rayTracingMaterial.color.b)
        };
    }


    /// <summary>
    /// Gets the hitbox of a 2D collider
    /// </summary>
    private HitBox getHitBox(Collider2D collider)
    {
        return new HitBox
        {
            min = collider.bounds.min,
            max = collider.bounds.max
        };
    }

    /// <summary>
    /// HitBox of an Object
    /// </summary>
    public struct HitBox
    {
        public Vector2 min { get; set; }
        public Vector2 max { get; set; }
    }


    /// <summary>
    /// Edge of an Object
    /// </summary>
    public struct ColliderEdge
    {
        public Vector2 start { get; set; }
        public Vector2 end { get; set; }
    }



    /// <summary>
    /// Struct to store data of the object
    /// </summary>
    public struct GeometryData
    {
        public HitBox hitBox { get; set; }
        public RayTracingMaterialStruct material { get; set; }
        public int edgeStartIndex { get; set; }
        public int edgeCount { get; set; }
    }

    /// <summary>
    /// Struct for the material
    /// </summary>
    public struct RayTracingMaterialStruct
    {
        public float roughness { get; set; }
        public float transmission { get; set; }
        public float emission { get; set; }
        public Vector3 color { get; set; }
    }
}
