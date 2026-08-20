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
            "Queue" = "Transparent+10"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "ProjectorOverlay"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZWrite Off
            ZTest LEqual
            Offset -1, -1

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment Fragment
            #include "UnityCG.cginc"

            sampler2D _ProjectionTexture;
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
                float4 positionWS = mul(unity_ObjectToWorld, input.positionOS);
                output.positionCS = mul(UNITY_MATRIX_VP, positionWS);
                output.positionPS = mul(_ProjectorWorldToProjection, positionWS).xyz;
                return output;
            }

            float4 Fragment(Varyings input) : SV_Target
            {
                float3 distanceToBoundary = 0.5 - abs(input.positionPS);
                clip(min(distanceToBoundary.x, min(distanceToBoundary.y, distanceToBoundary.z)));
                float2 uv = input.positionPS.xy + 0.5;
                return tex2D(_ProjectionTexture, uv) * _ProjectionColor;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
