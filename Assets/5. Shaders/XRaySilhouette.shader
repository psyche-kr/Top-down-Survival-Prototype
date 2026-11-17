// 오브젝트 뒤에 가려진 캐릭터를 실루엣으로 보여주는 셰이더 (URP)
// 렌더 상태(ZTest, Stencil 등)를 머티리얼 속성으로 받아서,
// XRaySilhouette 컴포넌트가 같은 셰이더로 머티리얼 두 개를 만든다.
//   1) Mark      : 화면에 보이는 캐릭터 픽셀에 스텐실 표시 (색은 그리지 않음)
//   2) Silhouette: 가려진 부분(ZTest Greater) 중 표시되지 않은 곳에만 색을 그림
Shader "Custom/XRaySilhouette"
{
    Properties
    {
        _BaseColor ("Color", Color) = (0.25, 0.75, 1, 0.6)
        _RimPower ("Rim Power", Range(0.5, 8)) = 2

        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 5          // Greater
        [Enum(UnityEngine.Rendering.ColorWriteMask)] _ColorMask ("Color Mask", Float) = 15 // RGBA
        _StencilRef ("Stencil Ref", Float) = 64
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comp", Float) = 6 // NotEqual
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Stencil Pass", Float) = 0       // Keep
        _OffsetFactor ("Offset Factor", Float) = 0
        _OffsetUnits ("Offset Units", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "XRay"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            ZTest [_ZTest]
            Cull Back
            Offset [_OffsetFactor], [_OffsetUnits]
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask [_ColorMask]

            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilRef]
                WriteMask [_StencilRef]
                Comp [_StencilComp]
                Pass [_StencilPass]
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _RimPower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float3 viewDirWS  : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // 가장자리일수록 진하게 (림 라이트 느낌의 실루엣)
                half ndotv = saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS)));
                half rim = pow(1.0h - ndotv, _RimPower);
                half alpha = _BaseColor.a * lerp(0.35h, 1.0h, rim);
                return half4(_BaseColor.rgb, alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
