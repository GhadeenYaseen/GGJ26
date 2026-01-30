Shader "Custom/OutlineBackface"
{
    Properties
    {
        _Color("Color", Color) = (1, 0.8, 0.2, 1)
        _Thickness("Thickness", Range(0.0, 0.05)) = 0.02
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float4 _Color;
            float _Thickness;

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                positionWS += normalize(normalWS) * _Thickness;
                o.positionHCS = TransformWorldToHClip(positionWS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}
