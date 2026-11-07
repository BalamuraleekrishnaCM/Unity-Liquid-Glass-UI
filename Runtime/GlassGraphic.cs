using UnityEngine;
using UnityEngine.UI;

namespace Balu.LiquidGlass
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("UI/Liquid Glass/Glass Graphic")]
    public sealed class GlassGraphic : MonoBehaviour
    {
        [SerializeField] private Color tint = new Color(0.82f, 0.91f, 1f, 0.30f);
        [SerializeField, Range(0f, 1f)] private float intensity = 0.72f;
        [SerializeField, Range(0f, 12f)] private float blurRadius = 3.5f;
        [SerializeField, Range(0f, 0.08f)] private float refraction = 0.012f;
        [SerializeField, Range(0f, 1f)] private float frost = 0.12f;
        [SerializeField, Range(0f, 2f)] private float rimStrength = 0.55f;
        [SerializeField] private Material materialOverride;
        [SerializeField] private GlassPreset preset;

        private Graphic target;
        private Material runtimeMaterial;
        private Material originalMaterial;
        private bool originalMaterialCaptured;
        private static readonly int TintId = Shader.PropertyToID("_GlassTint");
        private static readonly int IntensityId = Shader.PropertyToID("_GlassIntensity");
        private static readonly int BlurRadiusId = Shader.PropertyToID("_BlurRadius");
        private static readonly int RefractionId = Shader.PropertyToID("_Refraction");
        private static readonly int FrostId = Shader.PropertyToID("_Frost");
        private static readonly int RimId = Shader.PropertyToID("_RimStrength");
        private static readonly int GlassRectId = Shader.PropertyToID("_GlassRect");

        public Color Tint { get => tint; set { tint = value; Apply(); } }
        public float Intensity { get => intensity; set { intensity = Mathf.Clamp01(value); Apply(); } }
        public float BlurRadius { get => blurRadius; set { blurRadius = Mathf.Clamp(value, 0f, 12f); Apply(); } }
        public float Refraction { get => refraction; set { refraction = Mathf.Clamp(value, 0f, 0.08f); Apply(); } }
        public float Frost { get => frost; set { frost = Mathf.Clamp01(value); Apply(); } }
        public float RimStrength { get => rimStrength; set { rimStrength = Mathf.Clamp(value, 0f, 2f); Apply(); } }

        private void Awake()
        {
            Cache();
            if (target) originalMaterial = target.material;
            Apply();
        }

        private void OnEnable()
        {
            Cache();
            if (preset) ApplyPreset(preset);
            else Apply();
        }
        private void OnValidate()
        {
            if (!isActiveAndEnabled) return;
            Cache();
            if (preset) ApplyPreset(preset);
            else Apply();
        }

        public void ApplyPreset(GlassPreset preset)
        {
            if (!preset) return;
            this.preset = preset;
            tint = preset.Tint;
            intensity = preset.Intensity;
            blurRadius = preset.BlurRadius;
            refraction = preset.Refraction;
            frost = preset.Frost;
            rimStrength = preset.RimStrength;
            materialOverride = preset.Material;
            Apply();
        }

        public void Apply()
        {
            if (!target) Cache();
            if (!target) return;

            Material bundledTemplate = Resources.Load<Material>("LiquidGlassDefault");
            Shader shader = materialOverride ? materialOverride.shader :
                (bundledTemplate ? bundledTemplate.shader : Shader.Find("Balu/LiquidGlass/UI"));
            if (!shader) return;

            if (!runtimeMaterial || runtimeMaterial.shader != shader)
            {
                DestroyRuntimeMaterial();
                runtimeMaterial = materialOverride
                    ? new Material(materialOverride)
                    : new Material(shader);
                runtimeMaterial.name = "Liquid Glass UI (Instance)";
                runtimeMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            if (materialOverride && runtimeMaterial.shader == materialOverride.shader)
                runtimeMaterial.CopyPropertiesFromMaterial(materialOverride);

            if (runtimeMaterial.HasProperty(TintId)) runtimeMaterial.SetColor(TintId, tint);
            if (runtimeMaterial.HasProperty(IntensityId)) runtimeMaterial.SetFloat(IntensityId, intensity);
            if (runtimeMaterial.HasProperty(BlurRadiusId)) runtimeMaterial.SetFloat(BlurRadiusId, blurRadius);
            if (runtimeMaterial.HasProperty(RefractionId)) runtimeMaterial.SetFloat(RefractionId, refraction);
            if (runtimeMaterial.HasProperty(FrostId)) runtimeMaterial.SetFloat(FrostId, frost);
            if (runtimeMaterial.HasProperty(RimId)) runtimeMaterial.SetFloat(RimId, rimStrength);
            if (runtimeMaterial.HasProperty(GlassRectId))
            {
                Rect rect = target.rectTransform.rect;
                runtimeMaterial.SetVector(GlassRectId, new Vector4(rect.xMin, rect.yMin, rect.width, rect.height));
            }

            target.material = runtimeMaterial;
            target.color = Color.white;
        }

        private void OnRectTransformDimensionsChange() { if (isActiveAndEnabled) Apply(); }

        private void Cache()
        {
            target = GetComponent<Graphic>();
            if (target && !originalMaterialCaptured)
            {
                originalMaterial = target.material;
                originalMaterialCaptured = true;
            }
        }

        private void OnDestroy()
        {
            if (target && target.material == runtimeMaterial)
                target.material = originalMaterial;
            DestroyRuntimeMaterial();
        }

        private void DestroyRuntimeMaterial()
        {
            if (!runtimeMaterial) return;
            if (Application.isPlaying) Destroy(runtimeMaterial);
            else DestroyImmediate(runtimeMaterial);
            runtimeMaterial = null;
        }
    }
}