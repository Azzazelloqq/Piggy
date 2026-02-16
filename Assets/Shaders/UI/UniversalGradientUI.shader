Shader "UI/Universal Gradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Gradient)]
        _GradAColor0 ("Color 0", Color) = (1,1,1,1)
        _GradAColor1 ("Color 1", Color) = (0,0,0,1)
        _GradAType ("Type (0=Linear,1=Radial,2=Angular)", Float) = 0
        _GradAAngle ("Angle", Range(0,360)) = 0
        _GradACenter ("Center (UV)", Vector) = (0.5,0.5,0,0)
        _GradAScale ("Scale", Vector) = (1,1,0,0)
        _GradAOffset ("Offset", Vector) = (0,0,0,0)
        _GradAInvert ("Invert", Range(0,1)) = 0

        [Header(With Texture)]
        _GradientMix ("Gradient Mix", Range(0,1)) = 1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            fixed4 _GradAColor0;
            fixed4 _GradAColor1;
            float _GradAType;
            float _GradAAngle;
            float4 _GradACenter;
            float4 _GradAScale;
            float4 _GradAOffset;
            float _GradAInvert;
            float _GradientMix;

            float2 DirFromAngle(float angleDeg)
            {
                float a = radians(angleDeg);
                float s;
                float c;
                sincos(a, s, c);
                return float2(c, s);
            }

            float GradientT(float2 uv, float2 center, float2 scale, float2 offset, float angleDeg, float type, float invert)
            {
                float2 p = (uv + offset - center) * scale;
                float t;
                if (type < 0.5)
                {
                    float2 dir = DirFromAngle(angleDeg);
                    t = dot(p, dir) + 0.5;
                }
                else if (type < 1.5)
                {
                    t = length(p) * 1.41421356;
                }
                else
                {
                    t = atan2(p.y, p.x) / (2.0 * UNITY_PI) + 0.5;
                }
                t = saturate(t);
                t = lerp(t, 1.0 - t, invert);
                return t;
            }

            fixed4 EvalGradient(float2 uv, fixed4 c0, fixed4 c1, float type, float angleDeg, float2 center, float2 scale, float2 offset, float invert)
            {
                float t = GradientT(uv, center, scale, offset, angleDeg, type, invert);
                return lerp(c0, c1, t);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                fixed4 baseSample = (tex2D(_MainTex, uv) + _TextureSampleAdd) * i.color;

                fixed4 gradA = EvalGradient(uv, _GradAColor0, _GradAColor1, _GradAType, _GradAAngle,
                    _GradACenter.xy, _GradAScale.xy, _GradAOffset.xy, _GradAInvert);

                float3 gradRgb = saturate(gradA.rgb) * i.color.rgb;
                float gradAlpha = gradA.a;

                float3 outRgb = lerp(baseSample.rgb, gradRgb, _GradientMix);
                float outAlpha = baseSample.a * lerp(1.0, gradAlpha, _GradientMix);

                float clipFactor = UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                outAlpha *= clipFactor;
                outRgb *= clipFactor;
                #ifdef UNITY_UI_ALPHACLIP
                clip(outAlpha - 0.001);
                #endif

                return fixed4(outRgb, outAlpha);
            }
            ENDCG
        }
    }
}
