using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FrameCapturer : MonoBehaviour
{
    public RenderTexture lastFrame;
    
    // Use this for initialization
    void Awake()
    {
        if(SystemInfo.supportsImageEffects == false)
        {
            enabled = false;
            return;
        }
        
        this.lastFrame = new RenderTexture(Screen.width, Screen.height, 24);
        this.lastFrame.filterMode = FilterMode.Point;
        this.GetComponent<Camera>().targetTexture = this.lastFrame;
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }

    void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        this.GetComponent<Camera>().ResetAspect();
        Graphics.Blit(sourceTexture, lastFrame);
        Graphics.Blit(sourceTexture, destTexture);
    }

    void Update()
    {
        if(Screen.width != lastFrame.width || Screen.height != lastFrame.height)
        {
            // User has resized the window.
            RenderTexture resizedRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            resizedRenderTexture.filterMode = FilterMode.Point;
            GetComponent<Camera>().targetTexture = resizedRenderTexture;
            lastFrame.Release();
            lastFrame = resizedRenderTexture;
        }
    }

    void OnDisable()
    {
        if(this.lastFrame)
        {
            DestroyImmediate(this.lastFrame);
        }
    }
}
