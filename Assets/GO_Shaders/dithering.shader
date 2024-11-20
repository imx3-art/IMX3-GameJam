Shader "Custom/DitheringShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Base Map", 2D) = "white" {}
        _DitherStrength ("Dither Strength", Float) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
        LOD 200

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _BaseColor;
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float _DitherStrength;

            // Dithering pattern (Bayer matrix 4x4 for a more "dot-like" effect)
            static const float4x4 DitherPattern = float4x4(
                0.0625, 0.5625, 0.1875, 0.6875,
                0.8125, 0.3125, 0.9375, 0.4375,
                0.25, 0.75, 0.125, 0.625,
                1.0, 0.5, 0.875, 0.375
            );

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_TARGET
            {
                // Sample Base Color
                half4 baseColor = _BaseColor * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                // Calculate Dithering
                float2 ditherCoords = input.uv * 4.0; // Scale UV to match Bayer matrix size
                int2 ditherIndex = int2(floor(ditherCoords)) % 4;
                float ditherValue = DitherPattern[ditherIndex.x][ditherIndex.y];

                // Apply Dithering based on threshold
                float threshold = frac(_Time.y * 100.0) * _DitherStrength;
                if (ditherValue < threshold)
                {
                    baseColor.rgb *= 0.5; // Reduce intensity for dithering effect
                }

                return baseColor;
            }
            ENDHLSL
        }
    }
    
    CustomEditor "UniversalRenderPipeline.SimpleLitGUI"
}