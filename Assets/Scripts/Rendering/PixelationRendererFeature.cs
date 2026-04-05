using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class PixelationSettings
{
    [Range(1f, 32f)]
    [Tooltip("Higher values = chunkier pixels. 1 = no effect. Try 4–8 for a classic look.")]
    public float pixelSize = 4f;
}

/// <summary>
/// URP Renderer Feature for screen-space pixelation.
///
/// Setup:
///   1. Select your URP Renderer asset in the Project window.
///   2. Click Add Renderer Feature → Pixelation Renderer Feature.
///   3. Adjust Pixel Size in the Inspector.
/// </summary>
public class PixelationRendererFeature : ScriptableRendererFeature
{
    [Header("Pixelation Settings")]
    public PixelationSettings settings = new PixelationSettings();

    private Material _material;
    private PixelationRenderPass _renderPass;

    // Shader path must match the shader's Name field exactly
    private const string ShaderName = "Hidden/Pixelation";

    public override void Create()
    {
        var shader = Shader.Find(ShaderName);

        if (shader == null)
        {
            Debug.LogError($"[PixelationRendererFeature] Could not find shader '{ShaderName}'. " +
                           "Make sure PixelationShader.shader is in your project.");
            return;
        }

        _material   = CoreUtils.CreateEngineMaterial(shader);
        _renderPass = new PixelationRenderPass(_material, settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_renderPass == null) return;

        // Skip in Scene view to keep editing comfortable
        if (renderingData.cameraData.cameraType == CameraType.SceneView) return;

        renderer.EnqueuePass(_renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(_material);
    }
}
