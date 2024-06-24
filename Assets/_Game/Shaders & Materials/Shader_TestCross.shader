Shader "Custom/URP Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "SRPDefaultUnlit"}
        LOD 100

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
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                uniform float4 _MainTex_ST;
            CBUFFER_END

            Varyings vert (Attributes attr)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(attr.positionOS);
                output.uv = TRANSFORM_TEX(attr.uv, _MainTex);
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float3 c = cross(float3(-1, 0, 0), float3(0, 0, 1));
                float4 col = float4(c, 1);
                return col;
            }
            ENDHLSL
        }
    }
}