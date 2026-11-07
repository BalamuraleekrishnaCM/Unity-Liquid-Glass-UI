using UnityEngine;
using UnityEngine.EventSystems;

namespace Balu.LiquidGlass
{
    /// <summary>Optional pointer feedback for mouse, touch and compatible XR UI input modules.</summary>
    [DisallowMultipleComponent]
    public sealed class GlassPointerFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private GlassGraphic glass;
        [SerializeField, Range(0f, 1f)] private float hoverBoost = 0.12f;
        [SerializeField, Range(0f, 1f)] private float pressedBoost = 0.22f;
        [SerializeField, Min(0.01f)] private float response = 12f;

        private float baseIntensity;
        private float targetIntensity;

        private void Awake()
        {
            if (!glass) glass = GetComponentInChildren<GlassGraphic>();
            if (glass) baseIntensity = glass.Intensity;
            targetIntensity = baseIntensity;
        }

        private void Update()
        {
            if (!glass) return;
            glass.Intensity = Mathf.Lerp(glass.Intensity, targetIntensity, 1f - Mathf.Exp(-response * Time.unscaledDeltaTime));
        }

        public void OnPointerEnter(PointerEventData eventData) => targetIntensity = Mathf.Clamp01(baseIntensity + hoverBoost);
        public void OnPointerExit(PointerEventData eventData) => targetIntensity = baseIntensity;
        public void OnPointerDown(PointerEventData eventData) => targetIntensity = Mathf.Clamp01(baseIntensity + pressedBoost);
        public void OnPointerUp(PointerEventData eventData) => targetIntensity = Mathf.Clamp01(baseIntensity + hoverBoost);
    }
}