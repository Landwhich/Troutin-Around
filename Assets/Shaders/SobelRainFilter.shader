Shader "Custom/SobelRainFilter"
{
    Properties
    {
        _NoiseTex ("Rain Noise", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 5
        _NoiseSpeed ("Noise Speed", Float) = 2
        _EdgeThreshold ("Edge Threshold", Float) = 0.1

        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);

        float _NoiseScale;
        float _NoiseSpeed;
        float _EdgeThreshold;
    }

    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass {
            Name "SobelRain"

            ZWrite Off
            Cull Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _TintColor;

            struct Attr
            {
                uint vertexID : SV_VertexID;
            };

            struct Vary
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D_X_FLOAT(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            Vary Vert(Attr input)
            {
                Vary output;
                output.positionHCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            //helper to convert to greyscale
            // float SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, float2 offsetUV){
            //     return  input.uv).r;
            //     float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, offsetUV).rgb;
            //     return dot(col, float3(0.299, 0.587, 0.114)); //blue and red less important than green in greyscale
            // }

            half4 Frag(Vary input) : SV_Target {
                float2 texel = 1.0/_ScreenParams.xy;
                float2 uv = input.uv;

                float tl = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv + texel * float2(-1,-1)).r;
                float t  = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv + texel * float2( 0,-1)).r;
                float tr = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv + texel * float2( 1,-1)).r;

                float l  = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv + texel * float2(-1, 0)).r;
                float r  = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv + texel * float2( 1, 0)).r;

                float bl = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv + texel * float2(-1, 1)).r;
                float b  = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv + texel * float2( 0, 1)).r;
                float br = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv + texel * float2( 1, 1)).r;

                //horizontal
                float weightH = 
                    -tl - 2*l - bl + 
                    // nothing 
                     tr + 2*r + br;
                //vetcial
                float weightV = 
                    -tl - 2*t - tr + 
                    // nothing
                     bl + 2*b + br;

                float edge = sqrt(weightH * weightH + weightV * weightV);

                edge *= 50.0;

                edge = saturate(edge);

                return float4(edge, edge, edge, 1);
            }

            ENDHLSL
        }
    }
}

float2 noiseUV = input.uv;
noiseUV.y += _Time.y * _NoiseSpeed;  // scroll downward
noiseUV *= _NoiseScale;

float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;

return float4(edge, edge, edge, 1);No

float rain = noise * edgeMask;

return float4(rain, rain, rain, 1);



