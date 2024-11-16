Shader "Custom/EmissiveBlinkShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Base Map", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionMap ("Emission Map", 2D) = "white" {}
        _EmissionStrength ("Emission Strength", Float) = 1.0
        _BlinkSpeed ("Blink Speed", Float) = 2.0
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
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
            };

            float4 _BaseColor;
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            float4 _EmissionColor;
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            float _EmissionStrength;
            float _BlinkSpeed;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float4x4 modelMatrix = GetObjectToWorldMatrix();
                float4x4 viewProjMatrix = GetWorldToHClipMatrix();
                output.positionHCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                output.normalWS = normalize(mul((float3x3)modelMatrix, input.normalOS));
                output.tangentWS = normalize(mul((float3x3)modelMatrix, input.tangentOS.xyz));
                output.bitangentWS = cross(output.normalWS, output.tangentWS) * input.tangentOS.w;

                return output;
            }

            half4 Frag(Varyings input) : SV_TARGET
            {
                // Sample Base Color
                half4 baseColor = _BaseColor * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                // Sample Normal Map
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                half3 normalWS = normalize(input.tangentWS * normalTS.x + input.bitangentWS * normalTS.y + input.normalWS * normalTS.z);

                // Sample Emission Map
                half4 emissionMap = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv);

                // Emission Blinking
                float emissionFactor = abs(sin(_Time.y * _BlinkSpeed));
                half3 emission = (_EmissionColor.rgb * emissionMap.rgb) * _EmissionStrength * emissionFactor;

                // Final Color
                half4 finalColor = baseColor;
                finalColor.rgb += emission;

                return finalColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragShadow
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS);
                return output;
            }

            float4 FragShadow(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    CustomEditor "UniversalRenderPipeline.SimpleLitGUI"
}
