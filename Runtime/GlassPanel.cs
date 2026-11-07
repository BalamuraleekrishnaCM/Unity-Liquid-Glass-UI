using UnityEngine;
using UnityEngine.UI;

namespace Balu.LiquidGlass
{
    [AddComponentMenu("UI/Liquid Glass/Glass Panel")]
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public sealed class GlassPanel : MonoBehaviour
    {
        [SerializeField] private Color tint = new Color(0.82f, 0.91f, 1f, 0.24f);
        [SerializeField, Range(0f, 1f)] private float intensity = 0.65f;
        private Image image;

        private void Awake() => Apply();
        private void OnValidate() { if (isActiveAndEnabled) Apply(); }

        public void Apply()
        {
            if (!image) image = GetComponent<Image>();
            if (!image) return;
            image.color = tint;
            var glass = GetComponent<GlassGraphic>();
            if (!glass) glass = gameObject.AddComponent<GlassGraphic>();
            glass.Tint = tint;
            glass.Intensity = intensity;
        }
    }
}