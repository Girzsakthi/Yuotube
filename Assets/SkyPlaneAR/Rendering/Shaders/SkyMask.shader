Shader "SkyPlaneAR/SkyMask"
{
    Properties
    {
        _SkyMask    ("Sky Mask",    2D)    = "black" {}
        _DebugColor ("Debug Color", Color) = (0, 0.5, 1, 0.4)
        _Threshold  ("Threshold",   Float) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Overlay"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "SkyMaskDebug"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_SkyMask);
            SAMPLER(sampler_SkyMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _SkyMask_ST;
                float4 _DebugColor;
                float  _Threshold;
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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _SkyMask);
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                float maskValue = SAMPLE_TEXTURE2D(_SkyMask, sampler_SkyMask, IN.uv).r;
                float isSky = step(_Threshold, maskValue);
                return half4(_DebugColor.rgb, _DebugColor.a * isSky);
            }
            ENDHLSL
        }
    }

    // Fallback for non-URP pipelines.
    FallBack "Hidden/InternalErrorShader"
}
