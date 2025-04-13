using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingManager : MonoBehaviour
{

    /// <summary>
    /// Texture that get's rendered to the screen
    /// </summary>
    public RenderTexture finalImageTexure;

    /// <summary>
    /// Shader for rendering the image
    /// </summary>
    public ComputeShader computeShader;

    private Camera mainCamera;

    //Compute buffer to store geometryData and hitboxes
    private ComputeBuffer geometryDataBuffer;
    private ComputeBuffer colliderEdgesBuffer;


    //Settings variables
    public int numberOfRays = 1;

    //Variables to track if camera data changed
    private int lastScreenHeight = 0;
    private int lastScreenWidth = 0;
    private Vector2 lastCameraPosition = Vector2.positiveInfinity;
    private float lastCameraSize = 0;

    //Variables to track settings variables
    private int lastNumberOfRays;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //Update data of the shader
        updateShaderRenderTexture();
        updateShaderCameraData();
        updateShaderGeometryData();
        updateShaderSettings();

        //Dispatch the shader
        dispatchShader();
        //update tracking variables
        updateTrackingVariables();
    }


    private void updateShaderCameraData()
    {
        //Update when camera position changed 
        if (cameraDataChanged())
        {
            computeShader.SetFloat("cameraOrthographicSize", mainCamera.orthographicSize);
            computeShader.SetFloat("cameraAspect", mainCamera.aspect);
            computeShader.SetFloats("cameraPosition", mainCamera.transform.position.x, mainCamera.transform.position.y);
        }
    }


    /// <summary>
    /// Updates the render Texture if screen size changed
    /// </summary>
    private void updateShaderRenderTexture()
    {
        if (resolutionChanged())
        {
            //Create new Render Texture
            finalImageTexure = new RenderTexture(Screen.width, Screen.height, 24);
            finalImageTexure.enableRandomWrite = true;
            finalImageTexure.Create();

            //Assing changes to the resolution to the shader
            computeShader.SetFloats("resolution", Screen.width, Screen.height);

            //Assign texture to shader
            computeShader.SetTexture(0, "Result", finalImageTexure);
        }
    }


    /// <summary>
    /// Checks if camera position changed
    /// </summary>
    private bool cameraDataChanged()
    {
        return lastCameraPosition != new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.y)
        || lastCameraSize != mainCamera.orthographicSize;
    }


    /// <summary>
    /// Check if resolution changed
    /// </summary>
    private bool resolutionChanged()
    {
        return lastScreenHeight != Screen.height || lastScreenWidth != Screen.width;
    }


    /// <summary>
    /// Checks if settings have changed
    /// </summary>
    /// <returns></returns>
    private bool settingsChanged()
    {
        return lastNumberOfRays != numberOfRays;
    }


    /// <summary>
    /// Sends setting updates to the shader
    /// </summary>
    private void updateShaderSettings()
    {
        if (settingsChanged())
        {
            computeShader.SetInt("numberOfRays", numberOfRays);
        }
    }

    /// <summary>
    /// Updates the buffers that hold scene objects informations
    /// </summary>
    private void updateShaderGeometryData()
    {
        //Create buffer
        geometryDataBuffer = new ComputeBuffer(ObjectHitBoxManager.geometryDatas.Count, sizeof(float) * 10 + sizeof(int) * 2);
        colliderEdgesBuffer = new ComputeBuffer(ObjectHitBoxManager.colliderEdges.Count, sizeof(float) * 4);

        //fill buffer
        geometryDataBuffer.SetData(ObjectHitBoxManager.geometryDatas.ToArray());
        colliderEdgesBuffer.SetData(ObjectHitBoxManager.colliderEdges.ToArray());

        //Assign buffer to the shader
        computeShader.SetBuffer(0, "_GeometryDatas", geometryDataBuffer);
        computeShader.SetBuffer(0, "_ColliderEdges", colliderEdgesBuffer);
        computeShader.SetInt("numberOfObjects", ObjectHitBoxManager.geometryDatas.Count);
    }


    private void dispatchShader()
    {
        // Dispatch
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        computeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }



    /// <summary>
    /// Updates all variables that track data over frames
    /// </summary>
    private void updateTrackingVariables()
    {
        //Camera Data variables
        lastCameraPosition = mainCamera.transform.position;
        lastCameraSize = mainCamera.orthographicSize;
        lastScreenHeight = Screen.height;
        lastScreenWidth = Screen.width;

        //Settings variables
        lastNumberOfRays = numberOfRays;

    }
    /// <summary>
    /// Renders the texture to the screen
    /// </summary>
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (finalImageTexure != null)
        {
            // If a custom output texture is assigned, draw that texture onto the screen.
            Graphics.Blit(finalImageTexure, dest);
        }
        else
        {
            // Otherwise, simply copy the source texture to the destination (default rendering behavior).
            Graphics.Blit(src, dest);
        }
    }
}
