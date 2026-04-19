Shader "ARMilitary/ARTargetHighlight"
{
    Properties
    {
        _BaseColor  ("Base Color",  Color) = (0.2, 0.8, 0.2, 1.0)
        _RimColor   ("Rim Color",   Color) = (0.0, 1.0, 0.5, 1.0)
        _RimPower   ("Rim Power",   Range(0.5, 8.0)) = 3.0
        _PulseSpeed ("Pulse Speed", Range(0.0, 5.0)) = 1.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RimColor;
                float  _RimPower;
                float  _PulseSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 posOS   : POSITION;
                float3 normalOS: NORMAL;
            };

            struct Varyings
            {
                float4 posCS   : SV_POSITION;
                float3 normalWS: TEXCOORD0;
                float3 viewDirWS: TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS     = TransformObjectToHClip(IN.posOS.xyz);
                OUT.normalWS  = TransformObjectToWorldNormal(IN.normalOS);
                float3 posWS  = TransformObjectToWorld(IN.posOS.xyz);
                OUT.viewDirWS = GetWorldSpaceViewDir(posWS);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);

                // Rim lighting
                float rim = 1.0 - saturate(dot(N, V));
                rim = pow(rim, _RimPower);

                // Pulse
                float pulse = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed * 6.2831);

                float4 col = _BaseColor;
                col.rgb   += _RimColor.rgb * rim * (0.6 + 0.4 * pulse);
                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
