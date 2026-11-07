using UnityEngine;

namespace Balu.LiquidGlass
{
    [CreateAssetMenu(menuName = "Liquid Glass/Glass Preset", fileName = "GlassPreset")]
    public sealed class GlassPreset : ScriptableObject
    {
        [SerializeField] private Color tint = new Color(0.82f, 0.91f, 1f, 0.30f);
        [SerializeField, Range(0f, 1f)] private float intensity = 0.72f;
        [SerializeField, Range(0f, 12f)] private float blurRadius = 3.5f;
        [SerializeField, Range(0f, 0.08f)] private float refraction = 0.012f;
        [SerializeField, Range(0f, 1f)] private float frost = 0.12f;
        [SerializeField, Range(0f, 2f)] private float rimStrength = 0.55f;
        [SerializeField] private Material material;

        public Color Tint => tint;
        public float Intensity => intensity;
        public float BlurRadius => blurRadius;
        public float Refraction => refraction;
        public float Frost => frost;
        public float RimStrength => rimStrength;
        public Material Material => material;
    }
}