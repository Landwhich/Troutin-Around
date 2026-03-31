Shader "Custom/StylizedWater"
{
    Properties
    {
        _ShallowColor       ("Shallow Color",           Color)      = (0.2, 0.6, 0.9, 0.5)
        _DeepColor          ("Deep Color",              Color)      = (0.04, 0.2, 0.37, 1.0)
        _FoamColor          ("Foam Color",              Color)      = (1, 1, 1, 1)
        _DepthFadeDistance  ("Depth Fade Distance",     Float)      = 3.0
        _FoamEdgeWidth      ("Foam Edge Width",         Float)      = -1.6
        _FoamNoiseScale     ("Foam Noise Scale",        Float)      = 20
        _FoamSpeed          ("Foam Speed",              Float)      = 0.2
        _FoamCutoff         ("Foam Cutoff",             Range(0,1)) = 0.5
        _WaveSpeed          ("Wave Speed",              Float)      = 0.25
        _WaveScale          ("Wave Scale",              Float)      = 4.0
        _WaveStrength       ("Wave Strength",           Range(0,1)) = 0.3
        _Smoothness         ("Smoothness",              Range(0,1)) = 0.6
    }

    SubShader
    {
        Tags
        {
            "RenderType"        = "Transparent"
            "Queue"             = "Transparent-10"
            "RenderPipeline"    = "UniversalPipeline"
            "IgnoreProjector"   = "True"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4  _ShallowColor;
                half4  _DeepColor;
                half4  _FoamColor;
                float  _DepthFadeDistance;
                float  _FoamEdgeWidth;
                float  _FoamNoiseScale;
                float  _FoamSpeed;
                float  _FoamCutoff;
                float  _WaveSpeed;
                float  _WaveScale;
                float  _WaveStrength;
                float  _Smoothness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float  fogCoord   : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            //  noise gen
            float2 _hash2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                           dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }

            float valueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep
                float a = _hash2(i).x;
                float b = _hash2(i + float2(1, 0)).x;
                float c = _hash2(i + float2(0, 1)).x;
                float d = _hash2(i + float2(1, 1)).x;
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float foamNoise(float2 uv, float t)
            {
                float2 s1 = uv + float2( t * _FoamSpeed,        t * _FoamSpeed * 0.6);
                float2 s2 = uv + float2(-t * _FoamSpeed * 0.7,  t * _FoamSpeed * 0.4);
                return (valueNoise(s1 * _FoamNoiseScale) +
                        valueNoise(s2 * _FoamNoiseScale * 1.3)) * 0.5;
            }

            float waveBright(float2 uv, float t)
            {
                float2 u1 = uv * _WaveScale + float2( t * _WaveSpeed,        t * _WaveSpeed * 0.5);
                float2 u2 = uv * _WaveScale + float2(-t * _WaveSpeed * 0.6,  t * _WaveSpeed * 0.8);
                return ((valueNoise(u1) + valueNoise(u2)) * 0.5) * _WaveStrength;
            }

            Varyings vert(Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varyings OUT;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = pos.positionCS;
                OUT.positionWS = pos.positionWS;
                OUT.screenPos  = ComputeScreenPos(OUT.positionCS);
                OUT.uv         = IN.uv;
                OUT.fogCoord   = ComputeFogFactor(OUT.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                // Depth intersection
                float rawDepth   = SampleSceneDepth(screenUV);
                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float surfDepth  = IN.screenPos.w;
                float depthDiff  = sceneDepth - surfDepth;

                // Colour from depth
                float  depthT    = saturate(depthDiff / _DepthFadeDistance);
                half4  col       = lerp(_ShallowColor, _DeepColor, depthT);

                // Animated wave highlights
                col.rgb += waveBright(IN.uv, _Time.y) * half3(0.5, 0.7, 1.0);

                // Shoreline foam
                float foamT    = saturate(depthDiff / _FoamEdgeWidth);
                float foam     = foamNoise(IN.uv, _Time.y);
                float foamMask = step(_FoamCutoff, foam) * (1.0 - foamT);
                col = lerp(col, _FoamColor, foamMask);

                // Specular highlight
                Light  mainLight = GetMainLight();
                float3 viewDir   = normalize(GetWorldSpaceViewDir(IN.positionWS));
                float3 halfDir   = normalize(mainLight.direction + viewDir);
                float  spec      = pow(saturate(dot(float3(0, 1, 0), halfDir)),
                                       lerp(16.0, 512.0, _Smoothness));
                col.rgb += mainLight.color * spec * _Smoothness * 0.5;

                // Final alpha
                col.a = max(lerp(_ShallowColor.a, _DeepColor.a, depthT), foamMask);

                col.rgb = MixFog(col.rgb, IN.fogCoord);
                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
