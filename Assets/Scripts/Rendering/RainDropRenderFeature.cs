using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class RainDropRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public ComputeShader rainDropShader;
        public Texture2D     noiseTex;          // Must be Texture2D for Inspector assignment
        public Color         dropColor       = Color.white;
        [Range(1, 20)]  public int   thickness      = 3;
        [Range(0f, 1f)] public float sobelThreshold = 0.166f;
        [Range(0f, 1f)] public float rainDropScale  = 0.5f;
        public float           dropSpeed            = 100f;
        public RenderPassEvent renderPassEvent      = RenderPassEvent.AfterRenderingTransparents;
        public bool            previewInSceneView   = true;
    }

    [SerializeField] public Settings settings = new Settings();
    private RainDropRenderPass m_Pass;

    public override void Create()
    {
        m_Pass = new RainDropRenderPass(settings)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.rainDropShader == null || settings.noiseTex == null)
            return;

        if (!settings.previewInSceneView &&
            (renderingData.cameraData.isSceneViewCamera ||
             renderingData.cameraData.isPreviewCamera))
            return;

        renderer.EnqueuePass(m_Pass);
    }

    protected override void Dispose(bool disposing)
    {
        m_Pass?.Dispose();
    }

    class RainDropRenderPass : ScriptableRenderPass
    {
        private Settings m_Settings;
        private RTHandle  m_NoiseRTHandle; 

        public RainDropRenderPass(Settings settings)
        {
            m_Settings = settings;
            requiresIntermediateTexture = false;
        }
        private void EnsureNoiseHandle()
        {
            if (m_NoiseRTHandle == null && m_Settings.noiseTex != null)
                m_NoiseRTHandle = RTHandles.Alloc(m_Settings.noiseTex);
        }

        public void Dispose()
        {
            m_NoiseRTHandle?.Release();
            m_NoiseRTHandle = null;
        }


        public override void RecordRenderGraph(RenderGraph renderGraph,
                                               ContextContainer frameData)
        {
            EnsureNoiseHandle();
            if (m_NoiseRTHandle == null) return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData   = frameData.Get<UniversalCameraData>();

            if (!m_Settings.previewInSceneView && cameraData.isSceneViewCamera)
                return;

            int width  = cameraData.cameraTargetDescriptor.width;
            int height = cameraData.cameraTargetDescriptor.height;

            var desc = new RenderTextureDescriptor(width, height,
                                                   RenderTextureFormat.ARGB32, 0)
            {
                enableRandomWrite = true,
                msaaSamples       = 1
            };

            TextureHandle outputTex = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, desc, "_RainDropOutput", false);

            TextureHandle cameraColor = resourceData.cameraColor;
            TextureHandle cameraDepth = resourceData.cameraDepthTexture;
            TextureHandle noiseHandle = renderGraph.ImportTexture(m_NoiseRTHandle); // uses cached RTHandle

            Settings s = m_Settings;

            int noiseW = m_Settings.noiseTex.width;
            int noiseH = m_Settings.noiseTex.height;

            //  Compute pass 
            using (var builder = renderGraph.AddComputePass<ComputePassData>(
                       "RainDrop_Compute", out var data))
            {
                builder.UseTexture(cameraColor, AccessFlags.Read);
                builder.UseTexture(cameraDepth, AccessFlags.Read);
                builder.UseTexture(outputTex,   AccessFlags.Write);
                builder.UseTexture(noiseHandle, AccessFlags.Read);

                data.shader      = s.rainDropShader;
                data.noiseTex    = noiseHandle;   // TextureHandle, imported above
                data.noiseWidth  = noiseW;        // dimensions stored separately
                data.noiseHeight = noiseH;
                data.settings    = s;
                data.width       = width;
                data.height      = height;
                data.colorIn     = cameraColor;
                data.depthIn     = cameraDepth;
                data.outputTex   = outputTex;

                builder.AllowPassCulling(false);
                builder.SetRenderFunc((ComputePassData d, ComputeGraphContext ctx) =>
                    ExecuteCompute(d, ctx));
            }

            using (var builder = renderGraph.AddRasterRenderPass<BlitPassData>(
                       "RainDrop_Blit", out var data))
            {
                data.source = outputTex;
                builder.SetRenderAttachment(cameraColor, 0, AccessFlags.Write);
                builder.UseTexture(outputTex, AccessFlags.Read);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((BlitPassData d, RasterGraphContext ctx) =>
                    ExecuteBlit(d, ctx));
            }
        }

        //  Compute execute 
        static void ExecuteCompute(ComputePassData d, ComputeGraphContext ctx)
        {
            var cmd    = ctx.cmd;
            var shader = d.shader;
            var s      = d.settings;
            int kernel = shader.FindKernel("ScreenSpaceRainDrop");

            cmd.SetComputeTextureParam(shader, kernel, "_InputColorTex", d.colorIn);
            cmd.SetComputeTextureParam(shader, kernel, "_InputDepthTex", d.depthIn);
            cmd.SetComputeTextureParam(shader, kernel, "_NoiseTex",      d.noiseTex);
            cmd.SetComputeTextureParam(shader, kernel, "_OutputTex",     d.outputTex);

            cmd.SetComputeIntParam   (shader, "_Width",         d.width);
            cmd.SetComputeIntParam   (shader, "_Height",        d.height);
            cmd.SetComputeIntParam   (shader, "_Thickness",     s.thickness);
            cmd.SetComputeFloatParam (shader, "_EdgeThreshold", s.sobelThreshold);
            cmd.SetComputeFloatParam (shader, "_RainDropScale", s.rainDropScale);
            cmd.SetComputeIntParam   (shader, "_NoiseWidth",    d.noiseWidth);  // from pass data, not TextureHandle
            cmd.SetComputeIntParam   (shader, "_NoiseHeight",   d.noiseHeight);
            cmd.SetComputeVectorParam(shader, "_Time",          Shader.GetGlobalVector("_Time"));
            cmd.SetComputeFloatParam (shader, "_DropSpeed",     s.dropSpeed);
            cmd.SetComputeVectorParam(shader, "_DropColor",     (Vector4)(Color)s.dropColor);

            int gx = Mathf.CeilToInt(d.width  / 8f);
            int gy = Mathf.CeilToInt(d.height / 8f);
            cmd.DispatchCompute(shader, kernel, gx, gy, 1);
        }

        static void ExecuteBlit(BlitPassData d, RasterGraphContext ctx)
        {
            Blitter.BlitTexture(ctx.cmd, d.source, new Vector4(1, 1, 0, 0), 0, false);
        }

        class ComputePassData
        {
            public ComputeShader shader;
            public TextureHandle noiseTex;     
            public int           noiseWidth; 
            public int           noiseHeight;
            public Settings      settings;
            public int           width, height;
            public TextureHandle colorIn;
            public TextureHandle depthIn;
            public TextureHandle outputTex;
        }

        class BlitPassData
        {
            public TextureHandle source;
        }
    }
}