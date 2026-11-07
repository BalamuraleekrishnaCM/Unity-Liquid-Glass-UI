#if LIQUIDGLASS_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Balu.LiquidGlass.URP
{
    /// <summary>
    /// Add this feature to the active URP Renderer Data. It captures and blurs
    /// the camera color before transparent/UI rendering and publishes the result
    /// to the Liquid Glass UI shader.
    /// </summary>
    public sealed class LiquidGlassRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public sealed class FeatureSettings
        {
            [Range(0.1f, 1f)] public float resolutionScale = 0.5f;
            [Range(0f, 8f)] public float blurRadius = 3f;
            public RenderPassEvent injectionPoint = RenderPassEvent.BeforeRenderingTransparents;
            public Shader blurShader;
        }

        [SerializeField] private FeatureSettings settings = new FeatureSettings();
        private LiquidGlassBlurPass pass;

        public override void Create()
        {
            if (!settings.blurShader) settings.blurShader = Shader.Find("Hidden/Balu/LiquidGlass/GaussianBlur");
            pass = new LiquidGlassBlurPass(settings);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (pass != null)
                pass.Setup(renderer.cameraColorTargetHandle, renderingData.cameraData.cameraTargetDescriptor);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (pass == null || !settings.blurShader || !renderingData.cameraData.postProcessEnabled && renderingData.cameraData.isSceneViewCamera)
                return;
            pass.renderPassEvent = settings.injectionPoint;
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            pass?.Dispose();
            pass = null;
        }

        private sealed class LiquidGlassBlurPass : ScriptableRenderPass
        {
            private readonly FeatureSettings settings;
            private RTHandle source;
            private RTHandle blurA;
            private RTHandle blurB;
            private Material material;
            private static readonly int AvailableId = Shader.PropertyToID("_LiquidGlassBackgroundAvailable");
            private static readonly int TextureId = Shader.PropertyToID("_LiquidGlassBackground");

            public LiquidGlassBlurPass(FeatureSettings settings)
            {
                this.settings = settings;
                if (settings.blurShader) material = CoreUtils.CreateEngineMaterial(settings.blurShader);
            }

            public void Setup(RTHandle source, RenderTextureDescriptor descriptor)
            {
                this.source = source;
                descriptor.depthBufferBits = 0;
                descriptor.msaaSamples = 1;
                descriptor.width = Mathf.Max(16, Mathf.RoundToInt(descriptor.width * settings.resolutionScale));
                descriptor.height = Mathf.Max(16, Mathf.RoundToInt(descriptor.height * settings.resolutionScale));
                RenderingUtils.ReAllocateIfNeeded(ref blurA, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_LiquidGlassBlurA");
                RenderingUtils.ReAllocateIfNeeded(ref blurB, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_LiquidGlassBlurB");
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (source == null || blurA == null || blurB == null || material == null) return;
                var cmd = CommandBufferPool.Get("Liquid Glass Backdrop Blur");
                material.SetFloat("_BlurRadius", settings.blurRadius);
                Blitter.BlitCameraTexture(cmd, source, blurA, material, 0);
                Blitter.BlitCameraTexture(cmd, blurA, blurB, material, 1);
                cmd.SetGlobalTexture(TextureId, blurB);
                cmd.SetGlobalFloat(AvailableId, 1f);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public void Dispose()
            {
                blurA?.Release();
                blurB?.Release();
                blurA = null;
                blurB = null;
                CoreUtils.Destroy(material);
                material = null;
            }
        }
    }
}
#endif