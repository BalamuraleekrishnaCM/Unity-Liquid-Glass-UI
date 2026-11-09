using System.Collections.Generic;
using Balu.LiquidGlass.Samples;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Balu.LiquidGlass.Editor
{
    public static class LiquidGlassShowcaseBuilder
    {
        private const string ScenePath = "Assets/LiquidGlassShowcase.unity";
        private static readonly Color White = new Color(.93f, .97f, 1f);
        private static readonly Color Accent = new Color(.28f, .72f, 1f);
        private static readonly List<GlassGraphic> Glass = new List<GlassGraphic>();

        [MenuItem("Tools/Liquid Glass/Create Sample Scene")]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Glass.Clear();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.035f, .075f, .17f);
            camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            if (GraphicsSettings.currentRenderPipeline == null)
                camera.gameObject.AddComponent<GlassBackgroundCapture>();

            // Colorful scene-space shapes are deliberately behind the overlay canvas.
            for (int i = 0; i < 8; i++)
            {
                var shape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shape.name = "Backdrop Orb " + (i + 1);
                shape.transform.position = new Vector3(-7f + (i % 4) * 4.4f, 2.5f - (i / 4) * 5.5f, 2);
                shape.transform.localScale = Vector3.one * (2.4f + (i % 3));
                var renderer = shape.GetComponent<Renderer>();
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (!shader) shader = Shader.Find("Unlit/Color");
                if (shader)
                {
                    var material = new Material(shader);
                    var color = Color.HSVToRGB((i * .17f + .54f) % 1f, .75f, .8f);
                    if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
                    if (material.HasProperty("_Color")) material.SetColor("_Color", color);
                    renderer.sharedMaterial = material;
                }
                Object.DestroyImmediate(shape.GetComponent<Collider>());
            }

            var root = new GameObject("Liquid Glass Showcase", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            var page = Rect("Page", root.transform, new Vector2(0, 0), new Vector2(1600, 900));
            Panel(page, "Header", new Vector2(0, 355), new Vector2(1490, 135));
            Text(page, "Title", "LIQUID GLASS  /  INTERACTIVE SHOWCASE", new Vector2(-330, 379), new Vector2(780, 56), 34, FontStyles.Bold);
            Text(page, "Subtitle", "Built-in RP + URP  |  uGUI + TextMeshPro  |  XR-oriented UI", new Vector2(-325, 330), new Vector2(790, 32), 20);

            var left = Panel(page, "Feature Preview", new Vector2(-340, -30), new Vector2(805, 585));
            Text(left, "Preview Title", "Frosted surfaces", new Vector2(0, 210), new Vector2(700, 58), 40, FontStyles.Bold);
            Text(left, "Description", "Tint • blur • rim light • refraction • rounded corners", new Vector2(0, 150), new Vector2(710, 42), 22);
            Panel(left, "Floating Glass Card", new Vector2(0, -45), new Vector2(610, 250));
            Text(left, "Card Headline", "A softer way to build interfaces", new Vector2(0, -8), new Vector2(565, 52), 29, FontStyles.Bold);
            Text(left, "Card Description", "Change the controls to preview the material in real time.", new Vector2(0, -65), new Vector2(550, 66), 20);
            var button = Button(left, "Action Button", "Try Interaction", new Vector2(0, -222), new Vector2(340, 64));

            var right = Panel(page, "Control Deck", new Vector2(460, -30), new Vector2(550, 585));
            Text(right, "Controls Title", "Live controls", new Vector2(0, 242), new Vector2(470, 52), 34, FontStyles.Bold);
            var blur = LabeledSlider(right, "Blur radius", 164, 0, 12, 3.5f);
            var frost = LabeledSlider(right, "Frost", 74, 0, 1, .12f);
            var refraction = LabeledSlider(right, "Refraction", -16, 0, .08f, .012f);
            var rim = Toggle(right, "Rim highlight", new Vector2(-100, -105));
            var dropdown = Dropdown(right, new Vector2(95, -171));
            var input = Input(right, new Vector2(0, -238));

            var status = Text(page, "Status", "Ready — try the controls", new Vector2(0, -391), new Vector2(1440, 48), 23);
            var demo = root.AddComponent<LiquidGlassShowcase>();
            demo.surfaces = Glass.ToArray();
            demo.blurSlider = blur;
            demo.frostSlider = frost;
            demo.refractionSlider = refraction;
            demo.rimToggle = rim;
            demo.themeDropdown = dropdown;
            demo.nameInput = input;
            demo.statusLabel = status;
            demo.actionButton = button;

            if (!Object.FindObjectOfType<EventSystem>())
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
            Debug.Log("Liquid Glass sample created at " + ScenePath +
                ". For URP add Liquid Glass Renderer Feature to your active Renderer Data. " +
                "Import TMP Essential Resources if text is missing.");
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        private static RectTransform Panel(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var rt = Rect(name, parent, pos, size);
            rt.gameObject.AddComponent<Image>().color = Color.white;
            Glass.Add(rt.gameObject.AddComponent<GlassGraphic>());
            return rt;
        }

        private static TMP_Text Text(Transform parent, string name, string value, Vector2 pos, Vector2 size,
            float fontSize, FontStyles style = FontStyles.Normal)
        {
            var rt = Rect(name, parent, pos, size);
            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = value;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = White;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = true;
            label.raycastTarget = false;
            return label;
        }

        private static Button Button(Transform parent, string name, string caption, Vector2 pos, Vector2 size)
        {
            var rt = Panel(parent, name, pos, size);
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = rt.GetComponent<Image>();
            rt.gameObject.AddComponent<GlassPointerFeedback>();
            Text(rt, "Label", caption, Vector2.zero, size - new Vector2(16, 8), 24, FontStyles.Bold);
            return button;
        }

        private static Slider LabeledSlider(Transform parent, string name, float y, float min, float max, float value)
        {
            Text(parent, name + " Label", name, new Vector2(-120, y + 24), new Vector2(300, 34), 22);
            var rt = Rect(name, parent, new Vector2(0, y - 15), new Vector2(420, 35));
            var track = Rect("Track", rt, Vector2.zero, new Vector2(410, 12));
            track.gameObject.AddComponent<Image>().color = new Color(.4f, .6f, .85f, .55f);
            var fill = Rect("Fill", track, Vector2.zero, new Vector2(410, 12));
            fill.anchorMin = new Vector2(0, .5f);
            fill.anchorMax = new Vector2(0, .5f);
            fill.pivot = new Vector2(0, .5f);
            fill.anchoredPosition = new Vector2(-205, 0);
            fill.gameObject.AddComponent<Image>().color = Accent;
            var handle = Rect("Handle", rt, new Vector2(-205, 0), new Vector2(30, 30));
            handle.gameObject.AddComponent<Image>().color = White;
            var slider = rt.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            return slider;
        }

        private static Toggle Toggle(Transform parent, string name, Vector2 pos)
        {
            var rt = Rect(name, parent, pos, new Vector2(230, 48));
            var box = Rect("Checkbox", rt, new Vector2(-90, 0), new Vector2(38, 38));
            var image = box.gameObject.AddComponent<Image>();
            image.color = new Color(.45f, .62f, .86f, .9f);
            var mark = Rect("Check", box, Vector2.zero, new Vector2(24, 24));
            var checkImage = mark.gameObject.AddComponent<Image>();
            checkImage.color = Accent;
            Text(rt, "Label", name, new Vector2(25, 0), new Vector2(180, 44), 20);
            var toggle = rt.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = image;
            toggle.graphic = checkImage;
            toggle.isOn = true;
            return toggle;
        }

        private static TMP_Dropdown Dropdown(Transform parent, Vector2 pos)
        {
            var rt = Rect("Theme Dropdown", parent, pos, new Vector2(220, 52));
            var image = rt.gameObject.AddComponent<Image>();
            image.color = new Color(.22f, .36f, .58f, .95f);
            var dropdown = rt.gameObject.AddComponent<TMP_Dropdown>();
            dropdown.targetGraphic = image;
            var caption = Text(rt, "Caption", "Arctic", Vector2.zero, new Vector2(200, 42), 22);
            dropdown.captionText = (TextMeshProUGUI)caption;
            var template = Rect("Template", rt, new Vector2(0, -55), new Vector2(220, 160));
            template.anchorMin = template.anchorMax = new Vector2(.5f, 0);
            template.pivot = new Vector2(.5f, 1);
            template.gameObject.AddComponent<Image>().color = new Color(.12f, .22f, .4f, .98f);
            var scroll = template.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            var viewport = Rect("Viewport", template, Vector2.zero, new Vector2(216, 156));
            viewport.gameObject.AddComponent<Image>().color = Color.white;
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var content = Rect("Content", viewport, Vector2.zero, new Vector2(216, 156));
            content.anchorMin = new Vector2(.5f, 1);
            content.anchorMax = new Vector2(.5f, 1);
            content.pivot = new Vector2(.5f, 1);
            content.anchoredPosition = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var item = Rect("Item", content, Vector2.zero, new Vector2(216, 48));
            item.gameObject.AddComponent<LayoutElement>().preferredHeight = 48;
            var itemImage = item.gameObject.AddComponent<Image>();
            itemImage.color = new Color(.3f, .48f, .7f, .75f);
            var itemToggle = item.gameObject.AddComponent<Toggle>();
            itemToggle.targetGraphic = itemImage;
            var itemText = Text(item, "Item Label", "Option", Vector2.zero, new Vector2(196, 40), 20);
            scroll.viewport = viewport;
            scroll.content = content;
            dropdown.template = template;
            dropdown.itemText = (TextMeshProUGUI)itemText;
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string> { "Arctic", "Rose", "Mint" });
            template.gameObject.SetActive(false);
            return dropdown;
        }

        private static TMP_InputField Input(Transform parent, Vector2 pos)
        {
            var rt = Rect("Name Input", parent, pos, new Vector2(420, 52));
            var image = rt.gameObject.AddComponent<Image>();
            image.color = new Color(.22f, .36f, .58f, .95f);
            var input = rt.gameObject.AddComponent<TMP_InputField>();
            input.targetGraphic = image;
            var viewport = Rect("Text Area", rt, Vector2.zero, new Vector2(395, 44));
            viewport.gameObject.AddComponent<RectMask2D>();
            var label = Text(viewport, "Text", "", Vector2.zero, new Vector2(385, 40), 21);
            var placeholder = Text(viewport, "Placeholder", "Type your name...", Vector2.zero, new Vector2(385, 40), 21);
            placeholder.color = new Color(.7f, .82f, .94f, .7f);
            input.textViewport = viewport;
            input.textComponent = (TextMeshProUGUI)label;
            input.placeholder = (TextMeshProUGUI)placeholder;
            input.lineType = TMP_InputField.LineType.SingleLine;
            return input;
        }
    }
}
