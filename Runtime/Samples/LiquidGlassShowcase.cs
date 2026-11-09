using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Balu.LiquidGlass.Samples
{
    /// <summary>Wires the generated showcase to live glass parameters.</summary>
    public sealed class LiquidGlassShowcase : MonoBehaviour
    {
        public GlassGraphic[] surfaces;
        public Slider blurSlider;
        public Slider frostSlider;
        public Slider refractionSlider;
        public Toggle rimToggle;
        public TMP_Dropdown themeDropdown;
        public TMP_InputField nameInput;
        public TMP_Text statusLabel;
        public Button actionButton;

        private void Start()
        {
            if (blurSlider) { blurSlider.onValueChanged.AddListener(SetBlur); SetBlur(blurSlider.value); }
            if (frostSlider) { frostSlider.onValueChanged.AddListener(SetFrost); SetFrost(frostSlider.value); }
            if (refractionSlider) { refractionSlider.onValueChanged.AddListener(SetRefraction); SetRefraction(refractionSlider.value); }
            if (rimToggle) { rimToggle.onValueChanged.AddListener(SetRim); SetRim(rimToggle.isOn); }
            if (themeDropdown) { themeDropdown.onValueChanged.AddListener(SetTheme); SetTheme(themeDropdown.value); }
            if (nameInput) nameInput.onValueChanged.AddListener(SetName);
            if (actionButton) actionButton.onClick.AddListener(Click);
            SetStatus("Ready — try the controls");
        }

        private void OnDestroy()
        {
            if (blurSlider) blurSlider.onValueChanged.RemoveListener(SetBlur);
            if (frostSlider) frostSlider.onValueChanged.RemoveListener(SetFrost);
            if (refractionSlider) refractionSlider.onValueChanged.RemoveListener(SetRefraction);
            if (rimToggle) rimToggle.onValueChanged.RemoveListener(SetRim);
            if (themeDropdown) themeDropdown.onValueChanged.RemoveListener(SetTheme);
            if (nameInput) nameInput.onValueChanged.RemoveListener(SetName);
            if (actionButton) actionButton.onClick.RemoveListener(Click);
        }

        private void ForEach(System.Action<GlassGraphic> action)
        {
            if (surfaces == null) return;
            foreach (var surface in surfaces) if (surface) action(surface);
        }

        public void SetBlur(float value) { ForEach(g => g.BlurRadius = value); SetStatus("Surface blur radius: " + value.ToString("0.0")); }
        public void SetFrost(float value) { ForEach(g => g.Frost = value); SetStatus("Frost: " + value.ToString("0.00")); }
        public void SetRefraction(float value) { ForEach(g => g.Refraction = value); SetStatus("Refraction: " + value.ToString("0.000")); }
        public void SetRim(bool enabled) { ForEach(g => g.RimStrength = enabled ? 0.8f : 0f); SetStatus("Rim highlight: " + (enabled ? "On" : "Off")); }
        public void SetTheme(int index)
        {
            Color color = index == 1 ? new Color(1f, .67f, .87f, .36f) :
                index == 2 ? new Color(.67f, 1f, .85f, .32f) :
                new Color(.72f, .86f, 1f, .32f);
            ForEach(g => g.Tint = color);
            SetStatus("Theme: " + (index == 1 ? "Rose" : index == 2 ? "Mint" : "Arctic"));
        }
        public void SetName(string value) { SetStatus(string.IsNullOrWhiteSpace(value) ? "Type your name" : "Hello, " + value); }
        public void Click() { SetStatus("Button clicked — pointer feedback is active"); }
        private void SetStatus(string message) { if (statusLabel) statusLabel.text = message; }
    }
}
