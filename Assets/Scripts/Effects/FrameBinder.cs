using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class FrameBinder : MonoBehaviour
{
    private FrameCapturer capturer = null;
    private RenderTexture lastFrame = null;

    void Start()
    {
        capturer = Camera.main.GetComponentInChildren<FrameCapturer>();
        if(capturer != null)
        {
            Renderer renderer = this.GetComponentInChildren<Renderer>();
            for (int i = 0; i < renderer.materials.Length; ++i)
            {
                if (renderer.materials[i].HasProperty("_DistortionLayer"))
                    renderer.materials[i].SetTexture("_MainTex", capturer.lastFrame);
            }
            lastFrame = capturer.lastFrame;
        }
    }

    void Update()
    {
        if(capturer.lastFrame != lastFrame)
        {
            Renderer renderer = this.GetComponentInChildren<Renderer>();
            for (int i = 0; i < renderer.materials.Length; ++i)
            {
                if (renderer.materials[i].HasProperty("_DistortionLayer"))
                    renderer.materials[i].SetTexture("_MainTex", capturer.lastFrame);
            }
            lastFrame = capturer.lastFrame;
        }
    }
}
