Shader "Custom/UnlitBubbleShader"
{
    Properties
    {
                _FresnelPower ("Fresnel Power", Float) = 2.0
                _Intensity ("Intensity", Float) = 1.0
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _ReflectionColor ("Reflection Color", Color) = (0.7, 0.9, 1, 1)
        _RimPower ("Rim Power", Float) = 2.0
            }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Overlay" }
        LOD 200

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            float4 _BaseColor;
            float4 _ReflectionColor;
            float _RimPower;
float _FresnelPower;
float _Intensity;
            
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float4x4 modelMatrix = GetObjectToWorldMatrix();
                float4x4 viewProjMatrix = GetWorldToHClipMatrix();
                output.positionHCS = mul(viewProjMatrix, mul(modelMatrix, input.positionOS));
                output.normalWS = normalize(mul((float3x3)modelMatrix, input.normalOS));
                float3 worldPos = mul(modelMatrix, input.positionOS).xyz;
                output.viewDirWS = normalize(GetWorldSpaceViewDir(worldPos));
                return output;
            }

            half4 Frag(Varyings input) : SV_TARGET
            {
                // Calculate fresnel effect
                float fresnelFactor = pow(1.0 - saturate(dot(input.viewDirWS, input.normalWS)), _FresnelPower);
                // Calculate rim effect
                float rimFactor = pow(1.0 - saturate(dot(input.normalWS, input.viewDirWS)), _RimPower);

                // Interpolate base and reflection colors
                half4 color = lerp(_BaseColor, _ReflectionColor, rimFactor) * _Intensity;

                // Add fresnel effect
                color.rgb += _ReflectionColor.rgb * fresnelFactor * _Intensity;

                                return color;
            }
            ENDHLSL
        }
    }
    
    CustomEditor "UniversalRenderPipeline.SimpleLitGUI"
}
