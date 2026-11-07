#if LIQUIDGLASS_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if LIQUIDGLASS_URP17
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
#endif

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
            pass?.Dispose();
            if (!settings.blurShader)
            {
                var bundledBlur = Resources.Load<Material>("LiquidGlassBlurURP");
                settings.blurShader = bundledBlur ? bundledBlur.shader : Shader.Find("Hidden/Balu/LiquidGlass/URPGaussianBlur");
            }
            pass = new LiquidGlassBlurPass(settings);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
#if !LIQUIDGLASS_URP17
            if (pass != null)
                pass.Setup(renderer.cameraColorTargetHandle, renderingData.cameraData.cameraTargetDescriptor);
#endif
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (pass == null || !settings.blurShader || renderingData.cameraData.cameraType == CameraType.Preview)
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
            private Vector4 textureSize;
            private static readonly int TexelSizeId = Shader.PropertyToID("_LiquidGlassBackground_TexelSize");
            private static readonly int AvailableId = Shader.PropertyToID("_LiquidGlassBackgroundAvailable");
            private static readonly int TextureId = Shader.PropertyToID("_LiquidGlassBackground");

            public LiquidGlassBlurPass(FeatureSettings settings)
            {
                this.settings = settings;
                ConfigureInput(ScriptableRenderPassInput.Color);
#if LIQUIDGLASS_URP17
                requiresIntermediateTexture = true;
#endif
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
                textureSize = new Vector4(1f / descriptor.width, 1f / descriptor.height, descriptor.width, descriptor.height);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (source == null || blurA == null || blurB == null || material == null) return;
                var cmd = CommandBufferPool.Get("Liquid Glass Backdrop Blur");
                material.SetFloat("_BlurRadius", settings.blurRadius);
                Blitter.BlitCameraTexture(cmd, source, blurA, material, 0);
                Blitter.BlitCameraTexture(cmd, blurA, blurB, material, 1);
                cmd.SetGlobalTexture(TextureId, blurB.nameID);
                cmd.SetGlobalVector(TexelSizeId, textureSize);
                cmd.SetGlobalFloat(AvailableId, 1f);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }


#if LIQUIDGLASS_URP17
            private sealed class GlobalTexturePassData
            {
                public Vector4 texelSize;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var resources = frameData.Get<UniversalResourceData>();
                if (resources.isActiveTargetBackBuffer || material == null) return;

                TextureHandle sourceTexture = resources.activeColorTexture;
                if (!sourceTexture.IsValid()) return;

                var descriptor = renderGraph.GetTextureDesc(sourceTexture);
                descriptor.name = "_LiquidGlassBlurA";
                descriptor.width = Mathf.Max(16, Mathf.RoundToInt(descriptor.width * settings.resolutionScale));
                descriptor.height = Mathf.Max(16, Mathf.RoundToInt(descriptor.height * settings.resolutionScale));
                descriptor.depthBufferBits = 0;
                descriptor.msaaSamples = MSAASamples.None;
                descriptor.clearBuffer = false;

                TextureHandle blurTextureA = renderGraph.CreateTexture(descriptor);
                descriptor.name = "_LiquidGlassBlurB";
                TextureHandle blurTextureB = renderGraph.CreateTexture(descriptor);

                material.SetFloat("_BlurRadius", settings.blurRadius);
                renderGraph.AddBlitPass(
                    new RenderGraphUtils.BlitMaterialParameters(sourceTexture, blurTextureA, material, 0),
                    "Liquid Glass Horizontal Blur");
                renderGraph.AddBlitPass(
                    new RenderGraphUtils.BlitMaterialParameters(blurTextureA, blurTextureB, material, 1),
                    "Liquid Glass Vertical Blur");

                using (var builder = renderGraph.AddRasterRenderPass<GlobalTexturePassData>(
                    "Publish Liquid Glass Backdrop", out var passData))
                {
                    passData.texelSize = new Vector4(
                        1f / descriptor.width, 1f / descriptor.height,
                        descriptor.width, descriptor.height);
                    builder.UseTexture(blurTextureB, AccessFlags.Read);
                    builder.SetGlobalTextureAfterPass(blurTextureB, TextureId);
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc(static (GlobalTexturePassData data, RasterGraphContext context) =>
                    {
                        context.cmd.SetGlobalVector(TexelSizeId, data.texelSize);
                        context.cmd.SetGlobalFloat(AvailableId, 1f);
                    });
                }
            }
#endif

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