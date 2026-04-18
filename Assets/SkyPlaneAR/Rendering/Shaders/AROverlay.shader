Shader "SkyPlaneAR/AROverlay"
{
    Properties
    {
        _MainTex   ("Albedo (RGB)",  2D)    = "white" {}
        _Color     ("Color",         Color) = (1, 1, 1, 1)
        _SkyMask   ("Sky Mask",      2D)    = "black" {}
        _Threshold ("Sky Threshold", Float) = 0.5
        _Glossiness("Smoothness",    Range(0,1)) = 0.5
        _Metallic  ("Metallic",      Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Geometry"
        }

        Pass
        {
            Name "AROverlayForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);  SAMPLER(sampler_MainTex);
            TEXTURE2D(_SkyMask);  SAMPLER(sampler_SkyMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float  _Threshold;
                float  _Glossiness;
                float  _Metallic;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS   = nrmInputs.normalWS;
                OUT.uv         = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.screenPos  = ComputeScreenPos(posInputs.positionCS);
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                // Sample sky mask in screen-space and discard sky-region fragments.
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float  skyValue = SAMPLE_TEXTURE2D(_SkyMask, sampler_SkyMask, screenUV).r;
                clip(_Threshold - skyValue - 0.001);    // discard if sky (skyValue >= threshold)

                // Albedo
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;

                // Simple URP Lambert + main light
                InputData inputData = (InputData)0;
                inputData.positionWS     = IN.positionWS;
                inputData.normalWS       = normalize(IN.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.shadowCoord    = TransformWorldToShadowCoord(IN.positionWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo     = albedo.rgb;
                surfaceData.alpha      = albedo.a;
                surfaceData.metallic   = _Metallic;
                surfaceData.smoothness = _Glossiness;
                surfaceData.normalTS   = half3(0, 0, 1);

                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }

        // Shadow caster pass so AR objects cast shadows.
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual Cull Back

            HLSLPROGRAM
            #pragma vertex   ShadowVert
            #pragma fragment ShadowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
