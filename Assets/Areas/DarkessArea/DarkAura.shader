Shader "Custom/DarkAura"
{
    Properties
    {
        _DarkColor("Dark Color", Color) = (0,0,0,1)
        _CenterDarkness("Center Darkness", Range(0,1)) = 1
        _Falloff("Falloff", Range(0.1,5)) = 1
        _Shape("Shape", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _DarkColor;
            float _CenterDarkness;
            float _Falloff;
            float4 _Shape;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Convert UV to center-based coordinates (-0.5 ~ 0.5)
                float2 centered = (i.uv - 0.5) * 2.0;

                // Apply shape scaling
                centered.x /= _Shape.x;
                centered.y /= _Shape.y;

                // Distance from center
                float dist = length(centered);

                // Smooth falloff
                float intensity = smoothstep(1.0, _Falloff, dist);

                // Invert so center = 1, edge = 0
                intensity = 1.0 - intensity;

                // Multiply by center darkness
                intensity *= _CenterDarkness;

                return float4(_DarkColor.rgb, intensity * _DarkColor.a);
            }
            ENDHLSL
        }
    }
}
