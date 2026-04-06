Shader "Hidden/Pixelation"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Name "Pixelation"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // Blit.hlsl declares Vert, Varyings, _BlitTexture, sampler_LinearClamp, etc.
            // Do NOT redeclare _MainTex — Blitter uses _BlitTexture internally.
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float2 _PixelationResolution;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // input.texcoord is already in correct [0,1] space — Blit.hlsl handles
                // the Y-flip differences between DX/Vulkan/Metal for us.
                float2 uv = input.texcoord;

                // Snap to low-res pixel grid
                float2 snapped = (floor(uv * _PixelationResolution) + 0.5) / _PixelationResolution;

                // _BlitTexture and sampler_LinearClamp come from Blit.hlsl
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, snapped);
            }
            ENDHLSL
        }
    }
}
