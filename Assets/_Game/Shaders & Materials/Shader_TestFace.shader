Shader "Custom/URP Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("_Color", Color) = (1,1,1,1)
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
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float4 color: TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                uniform float4 _MainTex_ST;
                uniform float4 _Color;
            CBUFFER_END

            StructuredBuffer<InstanceData> instanceBuffer;
            StructuredBuffer<FaceData> faceBuffer;
            StructuredBuffer<float3> positionBuffer;
            StructuredBuffer<int> indexBuffer;

            Varyings vert (Attributes attr)
            {
                Varyings output;
                //output.positionCS = TransformObjectToHClip(attr.positionOS);
                output.uv = attr.uv;

                float4x4 trs = instanceBuffer[faceBuffer[attr.instanceID].instanceIndex].trs;
                float3 pos3 = positionBuffer[indexBuffer[attr.vertexID + faceBuffer[attr.instanceID].vertexIndex * 6]];
                float4 position = float4(pos3, 1);
                float3 positionWS = mul(trs, position).xyz;
                output.positionCS = TransformObjectToHClip(positionWS);
                output.color = float4(rand3dTo3d(pos3), 1);


                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                return input.color;
            }
            ENDHLSL
        }
    }
}