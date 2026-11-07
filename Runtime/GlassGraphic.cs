using UnityEngine;
using UnityEngine.UI;

namespace Balu.LiquidGlass
{
    /// <summary>
    /// Lightweight glass surface marker for uGUI Graphics. Assign a compatible
    /// material/shader to Material Override to render a custom glass treatment.
    /// The default appearance remains a translucent tint when no material is set.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("UI/Liquid Glass/Glass Graphic")]
    public sealed class GlassGraphic : MonoBehaviour
    {
        [SerializeField] private Color tint = new Color(0.82f, 0.91f, 1f, 0.28f);
        [SerializeField, Range(0f, 1f)] private float intensity = 0.65f;
        [SerializeField] private Material materialOverride;

        public Color Tint { get => tint; set { tint = value; Apply(); } }
        public float Intensity { get => intensity; set { intensity = Mathf.Clamp01(value); Apply(); } }

        private Graphic target;
        private Material runtimeMaterial;

        private void Awake() { Cache(); Apply(); }
        private void OnEnable() { Cache(); Apply(); }
        private void OnValidate() { if (isActiveAndEnabled) { Cache(); Apply(); } }
        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                if (Application.isPlaying) Destroy(runtimeMaterial);
                else DestroyImmediate(runtimeMaterial);
            }
        }

        private void Cache() => target = GetComponent<Graphic>();

        private void Apply()
        {
            if (!target) Cache();
            if (!target) return;
            target.color = tint;
            if (materialOverride == null) return;
            if (runtimeMaterial == null || runtimeMaterial.shader != materialOverride.shader)
            {
                if (runtimeMaterial != null)
                {
                    if (Application.isPlaying) Destroy(runtimeMaterial);
                    else DestroyImmediate(runtimeMaterial);
                }
                runtimeMaterial = new Material(materialOverride) { name = materialOverride.name + " (Glass Instance)" };
            }
            target.material = runtimeMaterial;
            if (runtimeMaterial.HasProperty("_GlassIntensity"))
                runtimeMaterial.SetFloat("_GlassIntensity", intensity);
        }
    }
}