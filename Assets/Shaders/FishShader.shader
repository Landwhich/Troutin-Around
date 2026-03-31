Shader "Unlit/FishShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TranslationAmount("Translation amount", float) = 1
        _DisplacementAmount("Displacement amount", float) = 1
        _DisplacementSpeed("Displacement speed", float) = 1
        _MaskOffset("Mask offset", Range(0, 1)) = 0
        _SwimPhase("Swim phase", float) = 0
        _Speed("Speed", float) = 0.5
        _AlphaCutoff("Alpha cutoff", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MaskOffset;
            float _TranslationAmount;
            float _DisplacementAmount;
            float _DisplacementSpeed;
            float _SwimPhase;
            float _Speed;
            float _AlphaCutoff;
            
            v2f vert (appdata v)
            {
                v2f o;
                float mask = saturate(sin((1 - v.uv.x + _MaskOffset) * UNITY_PI));

                v.vertex.z += sin(_SwimPhase * _DisplacementSpeed) * _TranslationAmount * (_Speed * 0.2);

                v.vertex.z += sin(1 - v.uv.x * UNITY_PI + _SwimPhase * _DisplacementSpeed) * _DisplacementAmount * mask * (_Speed * 0.2);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - _AlphaCutoff);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}