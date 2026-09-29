Shader "UI/SpotlightOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Overlay Color", Color) = (0,0,0,0.6)
        _RingColor ("Ring Color", Color) = (1,1,1,1)
        _RingWidth ("Ring Width", Float) = 0.01
        _Softness ("Softness", Float) = 0.02
        _AspectRatio ("Aspect Ratio", Float) = 1.777
        _HighlightCount ("Highlight Count", Int) = 1
        [Toggle] _EnableRing ("Enable Ring", Float) = 1
        
        // Standard UI Stencil properties
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _RingColor;
            float _RingWidth;
            float _Softness;
            float _AspectRatio;
            int _HighlightCount;
            float _EnableRing;

            // Arrays exposed for C# scripts
            float4 _Centers[4];
            float _Radii[4];

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float finalMask = 1.0;
                float finalRing = 0.0;

                for (int i = 0; i < 4; i++)
                {
                    if (i >= _HighlightCount) break;

                    float2 uv = IN.texcoord;
                    float2 center = _Centers[i].xy;

                    uv.x *= _AspectRatio;
                    center.x *= _AspectRatio;

                    float dist = distance(uv, center);
                    float radius = _Radii[i];

                    float mask = smoothstep(radius, radius + _Softness, dist);
                    finalMask = min(finalMask, mask);

                    float outerEdge = 1.0 - smoothstep(radius + _RingWidth, radius + _RingWidth + _Softness, dist);
                    float innerEdge = smoothstep(radius, radius + _Softness, dist);
                    
                    // Multiply by _EnableRing to toggle it on/off
                    float ring = outerEdge * innerEdge * _EnableRing;
                    finalRing = max(finalRing, ring);
                }

                half4 texColor = tex2D(_MainTex, IN.texcoord);
                half4 overlay = texColor * IN.color;

                fixed4 result;
                result.rgb = lerp(overlay.rgb, _RingColor.rgb, finalRing);
                result.a = max(overlay.a * finalMask, _RingColor.a * finalRing);

                return result;
            }
            ENDCG
        }
    }
}