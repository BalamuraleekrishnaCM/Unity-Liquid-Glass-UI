Shader "Hidden/Balu/LiquidGlass/URPGaussianBlur"
{
    Properties { _BlurRadius ("Blur Radius", Float) = 3 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off
        HLSLINCLUDE
        #pragma target 3.5
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        float _BlurRadius;
        float4 FragHorizontal(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 d = float2(_BlitTexture_TexelSize.x * max(_BlurRadius, 0.0), 0);
            float4 c = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord) * 0.227027;
            c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord + d * 1.384615) * 0.316216;
            c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord - d * 1.384615) * 0.316216;
            c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord + d * 3.230769) * 0.070270;
            c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord - d * 3.230769) * 0.070270;
            return c;
        }
        float4 FragVertical(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 d = float2(0, _BlitTexture_TexelSize.y * max(_BlurRadius, 0.0));
            float4 c = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord) * 0.227027;
            c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord + d * 1.384615) * 0.316216;
            c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord - d * 1.384615) * 0.316216;
            c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord + d * 3.230769) * 0.070270;
            c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord - d * 3.230769) * 0.070270;
            return c;
        }
        ENDHLSL
        Pass
        {
            Name "Horizontal"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragHorizontal
            #pragma multi_compile_instancing
            ENDHLSL
        }
        Pass
        {
            Name "Vertical"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragVertical
            #pragma multi_compile_instancing
            ENDHLSL
        }
    }
}