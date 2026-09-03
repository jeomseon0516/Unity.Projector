Shader "Hidden/Jeomseon/Projector/Mesh Projection"
{
    Properties
    {
        [HideInInspector] _ProjectorEffectVersion("Projector Effect Version", Float) = 1
        [NoScaleOffset] _ProjectionTexture("Projection Texture", 2D) = "white" {}
        _ProjectionColor("Projection Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent+10"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "ProjectorOverlay"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZWrite Off
            ZTest LEqual
            Offset -1, -1

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Fed per draw through a MaterialPropertyBlock, so these stay outside UnityPerMaterial.
            TEXTURE2D(_ProjectionTexture);
            SAMPLER(sampler_ProjectionTexture);
            float4 _ProjectionColor;
            float4x4 _ProjectorWorldToProjection;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionPS : TEXCOORD0;
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionPS = mul(_ProjectorWorldToProjection, float4(positionWS, 1.0)).xyz;
                return output;
            }

            float4 Fragment(Varyings input) : SV_Target
            {
                float3 distanceToBoundary = 0.5 - abs(input.positionPS);
                clip(min(distanceToBoundary.x, min(distanceToBoundary.y, distanceToBoundary.z)));
                float2 uv = input.positionPS.xy + 0.5;
                return SAMPLE_TEXTURE2D(_ProjectionTexture, sampler_ProjectionTexture, uv) * _ProjectionColor;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
