// 오브젝트·유닛 강조 셰이더 (URP)
// ObjectOutline 컴포넌트가 같은 셰이더로 머티리얼 세 개를 만들어 렌더러에 덧붙인다.
//   1) Mask   : 오브젝트 모양대로 스텐실만 찍는다 (색 없음, 가려져도 찍음)
//   2) Fill   : 오브젝트 위에 반투명 색을 살짝 칠한다 (호버 시)
//   3) Outline: 법선 방향으로 화면 픽셀 단위만큼 부풀린 뒤, 스텐실이 없는 곳(= 바깥 테두리)만 그린다
// ZTest를 Always로 주면 다른 물체에 가려져도 테두리가 보인다.
Shader "Custom/HighlightOutline"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width (px)", Float) = 0

        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 8          // Always
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2                   // Back
        [Enum(UnityEngine.Rendering.ColorWriteMask)] _ColorMask ("Color Mask", Float) = 15 // RGBA
        _StencilRef ("Stencil Ref", Float) = 32
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comp", Float) = 8 // Always
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Stencil Pass", Float) = 0       // Keep
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
            Name "Highlight"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            ZTest [_ZTest]
            Cull [_Cull]
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
                float _OutlineWidth;
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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float4 positionCS = TransformObjectToHClip(input.positionOS.xyz);

                // 화면 공간에서 법선 방향으로 일정 픽셀만큼 밀어내기 (거리와 상관없이 같은 두께)
                if (_OutlineWidth > 0)
                {
                    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                    float2 normalCS = mul((float3x3)UNITY_MATRIX_VP, normalWS).xy;
                    float len = length(normalCS);
                    if (len > 1e-5)
                    {
                        float2 offset = (normalCS / len) * _OutlineWidth * 2.0 / _ScreenParams.xy;
                        positionCS.xy += offset * positionCS.w;
                    }
                }

                output.positionCS = positionCS;
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
