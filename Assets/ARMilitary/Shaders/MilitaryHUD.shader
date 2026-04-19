Shader "ARMilitary/MilitaryHUD"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color   ("Tint Color", Color) = (0.2, 1.0, 0.2, 0.85)
        _Glow    ("Glow Intensity", Range(0,2)) = 0.4
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float  _Glow;
            CBUFFER_END

            struct Attributes { float4 posOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS = TransformObjectToHClip(IN.posOS.xyz);
                OUT.uv    = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float4 col = tex * _Color;
                col.rgb   += col.rgb * _Glow * tex.a;
                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
