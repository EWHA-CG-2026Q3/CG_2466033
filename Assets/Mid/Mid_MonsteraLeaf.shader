Shader "Mid/MonsteraLeaf"
{
    Properties
    {
        _BaseColor ("Leaf Color", Color) = (0.13, 0.40, 0.12, 1)
        _LightColor ("Light Green", Color) = (0.28, 0.57, 0.19, 1)
        _VeinColor ("Vein Color", Color) = (0.43, 0.65, 0.28, 1)
        _TextureStrength ("Texture Strength", Range(0, 1)) = 0.45
        _Smoothness ("Leaf Glossiness", Range(0, 1)) = 0.75
        _SpecularStrength ("Highlight Strength", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite On

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _LightColor;
                half4 _VeinColor;
                float _TextureStrength;
                float _Smoothness;
                float _SpecularStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(float3(0, 1, 0));
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                float edge = abs(uv.y * 2.0 - 1.0);
                float gradient = saturate(1.0 - edge * 0.55);

                half3 color = lerp(
                    _BaseColor.rgb,
                    _LightColor.rgb,
                    gradient * 0.65
                );

                float centerVein =
                    1.0 - smoothstep(0.008, 0.025, edge);

                float veinWave =
                    abs(sin(uv.x * 45.0 + edge * 9.0));

                float sideVeins =
                    (1.0 - smoothstep(0.015, 0.065, veinWave))
                    * smoothstep(0.04, 0.18, edge)
                    * (1.0 - smoothstep(0.85, 1.0, edge));

                color = lerp(
                    color,
                    _VeinColor.rgb,
                    centerVein * 0.75 + sideVeins * 0.24
                );

                float pattern =
                    sin(uv.x * 85.0 + uv.y * 24.0) *
                    sin(uv.y * 67.0 - uv.x * 17.0);

                color *= 1.0 +
                    pattern * 0.055 * _TextureStrength;

                Light mainLight = GetMainLight();

                float3 N = normalize(input.normalWS);
                float3 V = normalize(GetWorldSpaceViewDir(input.positionWS));
                float3 L = normalize(mainLight.direction);
                float3 H = normalize(L + V);

                float specular = pow(
                    saturate(dot(N, H)),
                    lerp(8.0, 128.0, _Smoothness)
                );

                color += mainLight.color *
                    specular * _SpecularStrength;

                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
