Shader "Balu/LiquidGlass/UI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Graphic Color", Color) = (1,1,1,1)
        _GlassTint ("Glass Tint", Color) = (0.82,0.91,1,0.3)
        _GlassIntensity ("Glass Intensity", Range(0,1)) = 0.72
        _BlurRadius ("Blur Radius", Range(0,12)) = 3.5
        _Refraction ("Refraction", Range(0,0.08)) = 0.012
        _Frost ("Frost", Range(0,1)) = 0.12
        _RimStrength ("Rim Strength", Range(0,2)) = 0.55
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
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
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
            Name "LiquidGlassUI"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_LiquidGlassBackground);
            float4 _LiquidGlassBackground_TexelSize;
            fixed4 _Color;
            fixed4 _GlassTint;
            float _GlassIntensity, _BlurRadius, _Refraction, _Frost, _RimStrength;
            float _LiquidGlassBackgroundAvailable;
            float4 _MainTex_ST;
            float4 _ClipRect;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                fixed4 sprite = tex2D(_MainTex, i.uv) * i.color;
                float2 screenUV = i.screenPos.xy / max(i.screenPos.w, 0.0001);
                float2 centered = i.uv - 0.5;
                float2 warp = centered * _Refraction * (0.25 + _GlassIntensity);
                float2 sampleUV = saturate(screenUV + warp);

                float2 blurStep = _LiquidGlassBackground_TexelSize.xy * max(_BlurRadius, 0.0);
                fixed3 backdrop = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_LiquidGlassBackground, sampleUV).rgb * 0.40;
                backdrop += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_LiquidGlassBackground, sampleUV + float2(blurStep.x, 0)).rgb * 0.15;
                backdrop += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_LiquidGlassBackground, sampleUV - float2(blurStep.x, 0)).rgb * 0.15;
                backdrop += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_LiquidGlassBackground, sampleUV + float2(0, blurStep.y)).rgb * 0.15;
                backdrop += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_LiquidGlassBackground, sampleUV - float2(0, blurStep.y)).rgb * 0.15;
                fixed3 glass = lerp(_GlassTint.rgb, backdrop, saturate(_LiquidGlassBackgroundAvailable * _GlassIntensity));
                glass = lerp(glass, _GlassTint.rgb, _Frost * 0.45);

                float edge = min(min(i.uv.x, 1.0 - i.uv.x), min(i.uv.y, 1.0 - i.uv.y));
                float rim = (1.0 - smoothstep(0.0, 0.055, edge)) * _RimStrength;
                glass += rim * 0.18;
                glass = lerp(_GlassTint.rgb, glass, saturate(_GlassIntensity + 0.2));

                fixed alpha = sprite.a * saturate(_GlassTint.a + _GlassIntensity * 0.18);
                fixed4 result = fixed4(glass * sprite.rgb, alpha);
                #ifdef UNITY_UI_CLIP_RECT
                    result.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                    clip(result.a - 0.001);
                #endif
                return result;
            }
            ENDCG
        }
    }
}