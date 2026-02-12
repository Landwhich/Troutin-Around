using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ComputeScreenFeature : ScriptableRendererFeature
{
    class ComputePass : ScriptableRenderPass
    {
        ComputeShader compute;
        int kernel;

        RTHandle source;
        RTHandle tempTexture;

        public ComputePass(ComputeShader compute)
        {
            this.compute = compute;
            kernel = compute.FindKernel("CSMain");
        }

        public void Setup(RTHandle source)
        {
            this.source = source;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor desc)
        {
            desc.enableRandomWrite = true;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, desc);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Compute Screen Effect");

            int width = source.rt.width;
            int height = source.rt.height;

            // Copy camera to temp
            Blitter.BlitCameraTexture(cmd, source, tempTexture);

            // Bind textures
            cmd.SetComputeTextureParam(compute, kernel, "Source", tempTexture);
            cmd.SetComputeTextureParam(compute, kernel, "Result", tempTexture);

            int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(height / 8.0f);

            cmd.DispatchCompute(compute, kernel, threadGroupsX, threadGroupsY, 1);

            // Blit back to camera
            Blitter.BlitCameraTexture(cmd, tempTexture, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (tempTexture != null)
                tempTexture.Release();
        }
    }

    public ComputeShader computeShader;

    ComputePass pass;

    public override void Create()
    {
        pass = new ComputePass(computeShader);
        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.Setup(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(pass);
    }
}
