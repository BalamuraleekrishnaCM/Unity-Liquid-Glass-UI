using UnityEngine;

namespace Balu.LiquidGlass
{
    /// <summary>
    /// Optional single-camera background capture helper. For XR, configure a
    /// stereo-aware capture path appropriate to the active render pipeline.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/Liquid Glass/Glass Background Capture")]
    public sealed class GlassBackgroundCapture : MonoBehaviour
    {
        [SerializeField] private Camera sourceCamera;
        [SerializeField] private RenderTexture target;
        [SerializeField] private int width = 1280;
        [SerializeField] private int height = 720;

        public RenderTexture Texture => target;

        private void OnEnable()
        {
            if (!sourceCamera) sourceCamera = Camera.main;
            if (!target)
            {
                target = new RenderTexture(Mathf.Max(16, width), Mathf.Max(16, height), 0,
                    RenderTextureFormat.ARGB32) { name = "LiquidGlass Background" };
                target.Create();
            }
            if (sourceCamera) sourceCamera.targetTexture = target;
        }

        private void OnDisable()
        {
            if (sourceCamera && sourceCamera.targetTexture == target)
                sourceCamera.targetTexture = null;
        }

        private void OnDestroy()
        {
            if (target)
            {
                target.Release();
                if (Application.isPlaying) Destroy(target);
                else DestroyImmediate(target);
            }
        }
    }
}