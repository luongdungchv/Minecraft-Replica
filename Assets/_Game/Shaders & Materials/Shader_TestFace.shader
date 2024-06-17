Shader "Custom/Block face"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("_Color", Color) = (1,1,1,1)
        _Textures("Textures", 2DArray) = ""{}
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
            #include "Assets/Shader Headers/Random.cginc"

            struct InstanceData{
                float4x4 trs;
                int available;
            };
            struct FaceData{
                uint instanceIndex;
                uint vertexIndex;
            };

            struct Attributes
            {
                float2 uv : TEXCOORD0;
                uint instanceID: SV_InstanceID;
                uint vertexID: SV_VertexID;
            };

            struct Varyings
            {
                float3 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float4 color: TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                uniform float4 _MainTex_ST;
                uniform float4 _Color;
            CBUFFER_END
            Texture2DArray _Textures;
            SamplerState sampler_Textures;

            StructuredBuffer<InstanceData> instanceBuffer;
            StructuredBuffer<FaceData> faceBuffer;
            StructuredBuffer<float3> positionBuffer;
            StructuredBuffer<int> indexBuffer;
            StructuredBuffer<float> uvDepthBuffer;
            StructuredBuffer<float2> uvBuffer;

            Varyings vert (Attributes attr)
            {
                Varyings output;
                //output.positionCS = TransformObjectToHClip(attr.positionOS);
                float4x4 trs = instanceBuffer[faceBuffer[attr.instanceID].instanceIndex].trs;
                int index = indexBuffer[attr.vertexID + faceBuffer[attr.instanceID].vertexIndex * 6];

                float3 pos3 = positionBuffer[index];
                float4 position = float4(pos3, 1);
                float3 positionWS = mul(trs, position).xyz;

                output.positionCS = TransformObjectToHClip(positionWS);

                float uvDepth = uvDepthBuffer[faceBuffer[attr.instanceID].vertexIndex];
                float2 uv = uvBuffer[index];

                output.uv = float3(uv, uvDepth);
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                //float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float4 col = SAMPLE_TEXTURE2D_ARRAY(_Textures, sampler_Textures, input.uv.xy, input.uv.z);
                return col;
            }
            ENDHLSL
        }
    }
}