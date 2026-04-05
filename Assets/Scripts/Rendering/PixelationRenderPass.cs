using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PixelationRenderPass : ScriptableRenderPass
{
    private readonly Material _material;
    private readonly PixelationSettings _settings;

    private static readonly int PixelationResolutionID = Shader.PropertyToID("_PixelationResolution");

    private class PassData
    {
        public TextureHandle source;
        public Material material;
        public Vector2 pixelationResolution;
    }

    public PixelationRenderPass(Material material, PixelationSettings settings)
    {
        _material = material;
        _settings = settings;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (_material == null) return;

        var resourceData = frameData.Get<UniversalResourceData>();
        if (resourceData.isActiveTargetBackBuffer) return;

        var source = resourceData.activeColorTexture;

        var desc = renderGraph.GetTextureDesc(source);
        float factor = Mathf.Max(1f, _settings.pixelSize);
        var lowRes = new Vector2(
            Mathf.Floor(desc.width  / factor),
            Mathf.Floor(desc.height / factor)
        );

        // Intermediate texture — same format, same size as screen
        var tempDesc         = desc;
        tempDesc.name        = "Pixelation_Temp";
        tempDesc.clearBuffer = false;
        var temp = renderGraph.CreateTexture(tempDesc);

        // Pass 1: pixelate source → temp
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelation Blit", out var passData))
        {
            passData.source               = source;
            passData.material             = _material;
            passData.pixelationResolution = lowRes;

            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(temp, 0, AccessFlags.Write);

            builder.SetRenderFunc(static (PassData data, RasterGraphContext ctx) =>
            {
                // Set uniforms inside SetRenderFunc — this is the only place
                // they are guaranteed to reach the GPU command buffer
                data.material.SetVector(PixelationResolutionID, data.pixelationResolution);
                Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        // Pass 2: copy temp → active colour target (plain copy, no material)
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelation Copy Back", out var passData))
        {
            passData.source = temp;

            builder.UseTexture(temp, AccessFlags.Read);
            builder.SetRenderAttachment(source, 0, AccessFlags.Write);

            builder.SetRenderFunc(static (PassData data, RasterGraphContext ctx) =>
            {
                Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
            });
        }
    }
}
