Shader "Hidden/Balu/LiquidGlass/GaussianBlur"
{
    Properties { _MainTex ("Source", 2D) = "white" {} _BlurRadius ("Blur Radius", Float) = 3 }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        float _BlurRadius;
        struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
        struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
        v2f vert(appdata v) { v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
        fixed4 blur(float2 uv, float2 axis)
        {
            float2 d = axis * _MainTex_TexelSize.xy * max(_BlurRadius, 0.0);
            fixed4 c = tex2D(_MainTex, uv) * 0.227027;
            c += tex2D(_MainTex, uv + d * 1.384615) * 0.316216;
            c += tex2D(_MainTex, uv - d * 1.384615) * 0.316216;
            c += tex2D(_MainTex, uv + d * 3.230769) * 0.070270;
            c += tex2D(_MainTex, uv - d * 3.230769) * 0.070270;
            return c;
        }
        ENDCG
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragH
            fixed4 fragH(v2f i) : SV_Target { return blur(i.uv, float2(1,0)); }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragV
            fixed4 fragV(v2f i) : SV_Target { return blur(i.uv, float2(0,1)); }
            ENDCG
        }
    }
}