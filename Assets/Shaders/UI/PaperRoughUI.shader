Shader "UI/Paper Rough"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _AgeTex ("Age Texture", 2D) = "white" {}
        _AgeStrength ("Age Strength", Range(0,1)) = 0.5

        _EdgeCutoff ("Edge Cutoff", Range(0,0.2)) = 0.02
        _EdgeSoftness ("Edge Softness", Range(0.0001,0.1)) = 0.01
        _EdgeNoiseScale ("Edge Noise Scale", Range(1,256)) = 32
        _EdgeNoiseStrength ("Edge Noise Strength", Range(0,0.1)) = 0.02
        _TearNoiseScale ("Tear Noise Scale", Range(1,128)) = 8
        _TearNoiseStrength ("Tear Noise Strength", Range(0,0.2)) = 0.06
        _ShadowColor ("Shadow Color", Color) = (0,0,0,0.35)
        _ShadowOffset ("Shadow Offset (UV)", Vector) = (0.012,-0.012,0,0)
        _ShadowSoftness ("Shadow Softness", Range(0,0.05)) = 0.008

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

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            sampler2D _MainTex;

            sampler2D _AgeTex;
            float4 _AgeTex_ST;
            float _AgeStrength;

            float _EdgeCutoff;
            float _EdgeSoftness;
            float _EdgeNoiseScale;
            float _EdgeNoiseStrength;
            float _TearNoiseScale;
            float _TearNoiseStrength;
            fixed4 _ShadowColor;
            float4 _ShadowOffset;
            float _ShadowSoftness;

            float Hash21(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float EdgeMask(float2 uv)
            {
                float edgeNoise = (ValueNoise(uv * _EdgeNoiseScale) - 0.5) * _EdgeNoiseStrength;
                float tearNoise = ValueNoise(uv * _TearNoiseScale) - 0.5;
                tearNoise = -abs(tearNoise) * _TearNoiseStrength;
                float edgeDist = min(min(uv.x, uv.y), min(1.0 - uv.x, 1.0 - uv.y));
                return smoothstep(_EdgeCutoff, _EdgeCutoff + _EdgeSoftness, edgeDist + edgeNoise + tearNoise);
            }

            float SampleAlpha(float2 uv)
            {
                float inX = step(0.0, uv.x) * step(uv.x, 1.0);
                float inY = step(0.0, uv.y) * step(uv.y, 1.0);
                float inside = inX * inY;
                return tex2D(_MainTex, uv).a * EdgeMask(uv) * inside;
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
                fixed4 baseSample = (tex2D(_MainTex, uv) + _TextureSampleAdd);
                float3 baseRgb = baseSample.rgb * i.color.rgb;
                float baseAlpha = baseSample.a * i.color.a;

                float2 ageUV = TRANSFORM_TEX(uv, _AgeTex);
                float ageSample = tex2D(_AgeTex, ageUV).r;
                float ageMod = lerp(1.0, ageSample, _AgeStrength);
                baseRgb *= ageMod;

                float edgeMask = EdgeMask(uv);
                baseAlpha *= edgeMask;
                baseRgb = saturate(baseRgb);

                float2 shadowUV = uv + _ShadowOffset.xy;
                float shadowAlpha = SampleAlpha(shadowUV);
                float2 blur = float2(_ShadowSoftness, _ShadowSoftness);
                shadowAlpha += SampleAlpha(shadowUV + float2(blur.x, 0.0));
                shadowAlpha += SampleAlpha(shadowUV + float2(-blur.x, 0.0));
                shadowAlpha += SampleAlpha(shadowUV + float2(0.0, blur.y));
                shadowAlpha += SampleAlpha(shadowUV + float2(0.0, -blur.y));
                shadowAlpha *= 0.2;
                shadowAlpha *= _ShadowColor.a;

                float outAlpha = baseAlpha + shadowAlpha * (1.0 - baseAlpha);
                float3 outRgb = baseRgb * baseAlpha + _ShadowColor.rgb * shadowAlpha * (1.0 - baseAlpha);
                if (outAlpha > 0.0001)
                {
                    outRgb /= outAlpha;
                }

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
