Shader "Custom/URP Lit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Textures("Textures", 2DArray) = ""{}
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="UniversalForward" "RenderPipeline"="UniversalPipeline"}
        //Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        Pass{
            Tags { "LightMode" = "UniversalForwardOnly" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct InstanceData{
                float4x4 trs;
                int blockType;
            };
            struct FaceData{
                uint instanceIndex;
                uint vertexIndex;
            };

            struct Attributes{
                float4 positionOS: POSITION;
                float2 uv: TEXCOORD0;
                float4 normal: NORMAL; 

                uint instanceID: SV_InstanceID;
                uint vertexID: SV_VertexID; 
            };
            struct Varyings{
                float4 positionCS: SV_POSITION;
                float3 uv: TEXCOORD0;
                float3 normalWS: TEXCOORD1;
                float3 positionWS: TEXCOORD5;
                float4 shadowCoord: TEXCOORD2;
                float fogCoord: TEXCOORD3;
                half3 viewDirectionWS: TEXCOORD4; 
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                uniform float4 _Color;
                uniform float _Metallic;
                uniform float _Glossiness;
                uniform float3 dirs[6];
            CBUFFER_END

            Texture2DArray _Textures;
            SamplerState sampler_Textures;

            StructuredBuffer<InstanceData> instanceBuffer;
            StructuredBuffer<FaceData> faceBuffer;
            StructuredBuffer<float3> positionBuffer;
            StructuredBuffer<int> indexBuffer;
            StructuredBuffer<int> uvDepthMap;
            StructuredBuffer<float> uvDepthBuffer;
            StructuredBuffer<float2> uvBuffer;

            Varyings vert(Attributes vertexInput){
                Varyings output = (Varyings)0;

                int instID = faceBuffer[vertexInput.instanceID].instanceIndex;
                float4x4 trs = instanceBuffer[instID].trs;
                int blockType = instanceBuffer[instID].blockType;

                int index = indexBuffer[vertexInput.vertexID + faceBuffer[vertexInput.instanceID].vertexIndex * 6];

                float3 pos3 = positionBuffer[index];
                float4 position = float4(pos3, 1);
                float3 positionWS = mul(trs, position).xyz;

                //output.positionCS = TransformObjectToHClip(vertexInput.positionOS);
                output.positionCS = TransformObjectToHClip(positionWS);

                float uvDepth = uvDepthBuffer[(uvDepthMap[blockType - 1]) * 6 + faceBuffer[vertexInput.instanceID].vertexIndex];
                //float uvDepth = uvDepthBuffer[1];
                float2 uv = uvBuffer[index];
                output.uv = float3(uv, uvDepth);

                //output.uv = vertexInput.uv;
                //output.normalWS = TransformObjectToWorldNormal(vertexInput.normal);
                output.normalWS = dirs[faceBuffer[vertexInput.instanceID].vertexIndex];
                //output.fogCoord = ComputeFogFactor(output.positionCS.z);
                //output.positionWS = positionWS;
                //output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
                //output.viewDirectionWS = GetWorldSpaceNormalizeViewDir(output.positionWS);

                return output;
            }

            float4 frag(Varyings input): SV_Target{
                // SurfaceData surfData = InitializeSurfaceData(input);
                // InputData pbrInput = InitializePBRInput(input);
                // half4 color = UniversalFragmentPBR(pbrInput, surfData);
                // color = half4(MixFog(color.rgb, input.fogCoord), color.a);
                // color *= _Color;
                // color += half4(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w, 1);
                // clip(color.a - 0.01);

                float4 color = SAMPLE_TEXTURE2D_ARRAY(_Textures, sampler_Textures, input.uv.xy, input.uv.z);
                Light light = GetMainLight();

                half3 lambert = max(dot(light.direction.xyz, input.normalWS), 0.0);
                half3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
                half3 lighting = ambient + lambert;

                color *= half4(lighting, 1);

                // half4 lighting = half4(VertexLighting(input.positionWS, input.normalWS), 1);
                //col *= lighting;
                return color;
            }
            
            ENDHLSL
        }
    }
}
