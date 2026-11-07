using UnityEngine;
using UnityEngine.Rendering;

namespace Balu.LiquidGlass
{
    /// <summary>
    /// Built-in Render Pipeline backdrop capture and two-pass separable blur.
    /// For URP, add LiquidGlassRendererFeature to the active Renderer Data instead.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/Liquid Glass/Glass Background Capture (Built-in RP)")]
    public sealed class GlassBackgroundCapture : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 1f)] private float resolutionScale = 0.5f;
        [SerializeField, Range(0f, 8f)] private float blurRadius = 3f;
        [SerializeField] private Shader blurShader;

        private Camera sourceCamera;
        private Material blurMaterial;
        private RenderTexture horizontal;
        private RenderTexture blurred;
        private static readonly int BlurRadiusId = Shader.PropertyToID("_BlurRadius");
        private static readonly int BackgroundAvailableId = Shader.PropertyToID("_LiquidGlassBackgroundAvailable");

        private void OnEnable()
        {
            sourceCamera = GetComponent<Camera>();
            if (GraphicsSettings.currentRenderPipeline != null)
                Debug.LogWarning("GlassBackgroundCapture is for the Built-in Render Pipeline. Add LiquidGlassRendererFeature to your URP Renderer Data instead.", this);

            if (!blurShader)
            {
                var bundledBlur = Resources.Load<Material>("LiquidGlassBlurBuiltIn");
                blurShader = bundledBlur ? bundledBlur.shader : Shader.Find("Hidden/Balu/LiquidGlass/GaussianBlur");
            }
            if (blurShader) blurMaterial = new Material(blurShader) { hideFlags = HideFlags.HideAndDontSave };
            Shader.SetGlobalFloat(BackgroundAvailableId, 0f);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (GraphicsSettings.currentRenderPipeline != null || !blurMaterial)
            {
                Graphics.Blit(source, destination);
                return;
            }

            int width = Mathf.Max(16, Mathf.RoundToInt(source.width * resolutionScale));
            int height = Mathf.Max(16, Mathf.RoundToInt(source.height * resolutionScale));
            EnsureTargets(width, height);
            blurMaterial.SetFloat(BlurRadiusId, blurRadius);
            Graphics.Blit(source, horizontal, blurMaterial, 0);
            Graphics.Blit(horizontal, blurred, blurMaterial, 1);
            Shader.SetGlobalTexture("_LiquidGlassBackground", blurred);
            Shader.SetGlobalFloat(BackgroundAvailableId, 1f);
            Graphics.Blit(source, destination);
        }

        private void EnsureTargets(int width, int height)
        {
            if (horizontal && horizontal.width == width && horizontal.height == height) return;
            ReleaseTargets();
            horizontal = CreateTarget(width, height, "LiquidGlass Blur Horizontal");
            blurred = CreateTarget(width, height, "LiquidGlass Blur Vertical");
        }

        private static RenderTexture CreateTarget(int width, int height, string label)
        {
            var rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                name = label,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap = false,
                autoGenerateMips = false
            };
            rt.Create();
            return rt;
        }

        private void OnDisable()
        {
            Shader.SetGlobalFloat(BackgroundAvailableId, 0f);
            ReleaseTargets();
            if (blurMaterial)
            {
                if (Application.isPlaying) Destroy(blurMaterial);
                else DestroyImmediate(blurMaterial);
            }
        }

        private void ReleaseTargets()
        {
            Release(horizontal);
            Release(blurred);
            horizontal = null;
            blurred = null;
        }

        private static void Release(RenderTexture rt)
        {
            if (!rt) return;
            rt.Release();
            if (Application.isPlaying) Destroy(rt);
            else DestroyImmediate(rt);
        }
    }
}