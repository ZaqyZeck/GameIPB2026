Shader "UI/ThanosDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Thanos Dissolve Settings)]
        _Progress ("Dissolve Progress", Range(0, 1)) = 0
        _NoiseScale ("Noise Scale (Granularity)", Float) = 0.08
        _EdgeWidth ("Ember Edge Width", Range(0.02, 0.4)) = 0.15
        [HDR] _EdgeColor ("Ember Glow Color", Color) = (3.0, 1.0, 0.2, 1.0)
        _AshColor ("Ash Color", Color) = (0.1, 0.08, 0.1, 0.9)
        _DriftSpeed ("Ash Drift Speed", Float) = 25.0
        _DirectionalBias ("Directional Bias (Bottom-Up)", Range(0, 1)) = 0.35

        // UI Masking
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
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "ThanosDissolve"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 localPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _Progress;
            float _NoiseScale;
            float _EdgeWidth;
            fixed4 _EdgeColor;
            fixed4 _AshColor;
            float _DriftSpeed;
            float _DirectionalBias;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.localPos = v.vertex.xy;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            // Pseudo-random hash
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float fbm(float2 p)
            {
                float total = 0.0;
                float amp = 0.55;
                for (int i = 0; i < 3; i++)
                {
                    total += amp * valNoise(p);
                    p = p * 2.15 + float2(13.1, 7.3);
                    amp *= 0.45;
                }
                return total;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample base UI sprite texture
                fixed4 col = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif

                // If fully intact or transparent texture, return early
                if (_Progress <= 0.001 || col.a <= 0.001)
                {
                    return col;
                }

                // If fully dissolved, clip away completely
                if (_Progress >= 0.999)
                {
                    clip(-1);
                    return fixed4(0, 0, 0, 0);
                }

                // Upward ash drift based on local pixel position
                float2 driftPos = IN.localPos - float2(0.2, 1.0) * (_Progress * _DriftSpeed);

                // Multi-octave noise using local pixel position (independent of atlas coordinates!)
                float n = fbm(driftPos * _NoiseScale); // range [0, ~0.9]

                // Directional bottom-to-top bias (bottom dissolves first, drifting upward)
                float bias = IN.localPos.y * 0.012 * _DirectionalBias;
                float dissolveVal = n + bias;

                // Remap progress to cover the full dissolve range [minDissolve, maxDissolve]
                float minRange = -0.3;
                float maxRange = 1.3 + _EdgeWidth;
                float threshold = lerp(minRange, maxRange, _Progress);

                // 1. Fully disintegrated into thin air
                if (dissolveVal < threshold - _EdgeWidth)
                {
                    clip(-1);
                    return fixed4(0, 0, 0, 0);
                }

                // 2. Burning ember edge & flaky ash
                float edgeFactor = saturate((dissolveVal - (threshold - _EdgeWidth)) / _EdgeWidth);

                if (edgeFactor < 0.4)
                {
                    // Crumbling dark ash flecks
                    float ashT = edgeFactor / 0.4;
                    col.rgb = lerp(_AshColor.rgb, _EdgeColor.rgb, ashT);
                    col.a *= lerp(0.0, 0.9, ashT);
                }
                else if (edgeFactor < 1.0)
                {
                    // Glowing burning boundary
                    float emberT = (edgeFactor - 0.4) / 0.6;
                    col.rgb = lerp(_EdgeColor.rgb, col.rgb, emberT);
                    col.rgb += _EdgeColor.rgb * (1.0 - emberT) * 0.8; // Incandescent bloom
                }

                return col;
            }
            ENDCG
        }
    }
}
