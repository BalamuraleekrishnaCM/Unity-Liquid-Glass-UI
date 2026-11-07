using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Balu.LiquidGlass;

namespace Balu.LiquidGlass.Editor
{
    public static class LiquidGlassMenu
    {
        private const string Root = "GameObject/UI/Liquid Glass/";

        [MenuItem(Root + "Glass Canvas", false, 10)]
        private static void CreateCanvas(MenuCommand cmd)
        {
            var go = new GameObject("Liquid Glass Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            go.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            Place(go, cmd);
            EnsureEventSystem();
        }

        [MenuItem(Root + "Panel", false, 20)]
        private static void CreatePanel(MenuCommand cmd) => CreateGlassGraphic("Glass Panel", cmd);

        [MenuItem(Root + "Image", false, 21)]
        private static void CreateImage(MenuCommand cmd)
        {
            var go = CreateRect("Glass Image");
            var image = go.AddComponent<Image>();
            image.color = new Color(0.85f, 0.93f, 1f, 0.28f);
            go.AddComponent<GlassGraphic>();
            Place(go, cmd);
        }

        [MenuItem(Root + "Raw Image", false, 22)]
        private static void CreateRawImage(MenuCommand cmd)
        {
            var go = CreateRect("Glass Raw Image");
            var image = go.AddComponent<RawImage>();
            image.color = new Color(0.85f, 0.93f, 1f, 0.28f);
            go.AddComponent<GlassGraphic>();
            Place(go, cmd);
        }

        [MenuItem(Root + "Button (TMP)", false, 30)]
        private static void CreateButton(MenuCommand cmd)
        {
            var go = CreateControl("Glass Button", cmd);
            var plate = AddImage(go.transform, "Glass Surface", new Color(0.78f, 0.89f, 1f, 0.30f));
            plate.gameObject.AddComponent<GlassGraphic>();
            var button = go.AddComponent<Button>();
            go.AddComponent<GlassPointerFeedback>();
            button.targetGraphic = plate;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = SoftColors();
            var label = AddTMP(go.transform, "Label", "Button");
            Stretch(label.rectTransform, 12, 8, 12, 8);
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 28;
            Place(go, cmd);
        }

        [MenuItem(Root + "Text (TMP)", false, 31)]
        private static void CreateText(MenuCommand cmd)
        {
            var go = CreateRect("Text (TMP)");
            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = "New Text";
            text.fontSize = 28;
            text.color = Color.white;
            Place(go, cmd);
        }

        [MenuItem(Root + "Input Field (TMP)", false, 32)]
        private static void CreateInput(MenuCommand cmd)
        {
            var go = CreateControl("Glass Input Field", cmd);
            var bg = AddImage(go.transform, "Glass Surface", new Color(0.78f, 0.89f, 1f, 0.26f));
            bg.gameObject.AddComponent<GlassGraphic>();
            var field = go.AddComponent<TMP_InputField>();
            field.targetGraphic = bg;

            var viewport = CreateRect("Text Area", go.transform);
            Stretch(viewport, 10, 6, 10, 6);
            viewport.gameObject.AddComponent<RectMask2D>();
            var text = AddTMP(viewport, "Text", "");
            Stretch(text.rectTransform, 0, 0, 0, 0);
            text.fontSize = 24;
            text.color = Color.white;
            var placeholder = AddTMP(viewport, "Placeholder", "Enter text...");
            Stretch(placeholder.rectTransform, 0, 0, 0, 0);
            placeholder.fontSize = 24;
            placeholder.fontStyle = FontStyles.Italic;
            placeholder.color = new Color(1, 1, 1, 0.45f);
            field.textViewport = viewport;
            field.textComponent = text;
            field.placeholder = placeholder;
            field.lineType = TMP_InputField.LineType.SingleLine;
            Place(go, cmd);
        }

        [MenuItem(Root + "Toggle", false, 33)]
        private static void CreateToggle(MenuCommand cmd)
        {
            var go = CreateControl("Glass Toggle", cmd);
            var bg = AddImage(go.transform, "Glass Surface", new Color(0.78f, 0.89f, 1f, 0.24f));
            bg.gameObject.AddComponent<GlassGraphic>();
            var toggle = go.AddComponent<Toggle>();
            go.AddComponent<GlassPointerFeedback>();
            toggle.targetGraphic = bg;

            var mark = AddImage(go.transform, "Checkmark", new Color(0.35f, 0.85f, 1f, 0.95f));
            mark.rectTransform.anchorMin = new Vector2(0, 0.2f);
            mark.rectTransform.anchorMax = new Vector2(0, 0.8f);
            mark.rectTransform.pivot = new Vector2(0, 0.5f);
            mark.rectTransform.anchoredPosition = new Vector2(12, 0);
            mark.rectTransform.sizeDelta = new Vector2(22, 22);
            toggle.graphic = mark;

            var label = AddTMP(go.transform, "Label", "Toggle");
            label.rectTransform.anchorMin = new Vector2(0, 0);
            label.rectTransform.anchorMax = new Vector2(1, 1);
            label.rectTransform.offsetMin = new Vector2(48, 4);
            label.rectTransform.offsetMax = new Vector2(-8, -4);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.fontSize = 24;
            toggle.isOn = true;
            Place(go, cmd);
        }

        [MenuItem(Root + "Slider", false, 34)]
        private static void CreateSlider(MenuCommand cmd)
        {
            var go = CreateControl("Glass Slider", cmd);
            var bg = AddImage(go.transform, "Glass Track", new Color(0.78f, 0.89f, 1f, 0.24f));
            bg.gameObject.AddComponent<GlassGraphic>();
            var slider = go.AddComponent<Slider>();
            go.AddComponent<GlassPointerFeedback>();
            slider.targetGraphic = bg;

            var fillArea = CreateRect("Fill Area", go.transform);
            fillArea.anchorMin = new Vector2(0, 0.25f);
            fillArea.anchorMax = new Vector2(1, 0.75f);
            fillArea.offsetMin = new Vector2(8, 0);
            fillArea.offsetMax = new Vector2(-8, 0);
            var fill = AddImage(fillArea, "Fill", new Color(0.35f, 0.78f, 1f, 0.9f));
            Stretch(fill.rectTransform, 0, 0, 0, 0);

            var handleArea = CreateRect("Handle Slide Area", go.transform);
            Stretch(handleArea, 8, 0, 8, 0);
            var handle = AddImage(handleArea, "Handle", new Color(1f, 1f, 1f, 0.9f));
            handle.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            handle.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            handle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            handle.rectTransform.anchoredPosition = Vector2.zero;
            handle.rectTransform.sizeDelta = new Vector2(26, 26);
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.direction = Slider.Direction.LeftToRight;
            slider.value = 0.5f;
            Place(go, cmd);
        }

        [MenuItem(Root + "Dropdown (TMP)", false, 35)]
        private static void CreateDropdown(MenuCommand cmd)
        {
            var go = CreateControl("Glass Dropdown", cmd);
            var bg = AddImage(go.transform, "Glass Surface", new Color(0.78f, 0.89f, 1f, 0.28f));
            bg.gameObject.AddComponent<GlassGraphic>();
            var dropdown = go.AddComponent<TMP_Dropdown>();
            go.AddComponent<GlassPointerFeedback>();
            dropdown.targetGraphic = bg;

            var caption = AddTMP(go.transform, "Caption", "Option A");
            caption.rectTransform.anchorMin = Vector2.zero;
            caption.rectTransform.anchorMax = Vector2.one;
            caption.rectTransform.offsetMin = new Vector2(14, 5);
            caption.rectTransform.offsetMax = new Vector2(-34, -5);
            caption.alignment = TextAlignmentOptions.MidlineLeft;
            caption.fontSize = 23;
            dropdown.captionText = caption;

            var template = CreateRect("Template", go.transform);
            template.anchorMin = new Vector2(0, 0);
            template.anchorMax = new Vector2(1, 0);
            template.pivot = new Vector2(0.5f, 1);
            template.anchoredPosition = new Vector2(0, -4);
            template.sizeDelta = new Vector2(0, 180);
            var templateBg = AddImage(template, "Template Background", new Color(0.10f, 0.15f, 0.22f, 0.96f));
            templateBg.gameObject.AddComponent<GlassGraphic>();
            var scroll = template.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var viewport = CreateRect("Viewport", template);
            Stretch(viewport, 4, 4, 4, 4);
            viewport.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var content = CreateRect("Content", viewport);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 2f;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = new Vector2(0, 36);

            var item = CreateRect("Item", content);
            item.anchorMin = new Vector2(0, 1);
            item.anchorMax = new Vector2(1, 1);
            item.pivot = new Vector2(0.5f, 1);
            item.sizeDelta = new Vector2(0, 36);
            var itemBg = AddImage(item, "Item Background", new Color(0.4f, 0.7f, 0.95f, 0.18f));
            var itemLayout = item.gameObject.AddComponent<LayoutElement>();
            itemLayout.preferredHeight = 36f;
            var itemToggle = item.gameObject.AddComponent<Toggle>();
            itemToggle.targetGraphic = itemBg;
            itemToggle.transition = Selectable.Transition.ColorTint;
            var itemLabel = AddTMP(item, "Item Label", "Option");
            Stretch(itemLabel.rectTransform, 12, 2, 8, 2);
            itemLabel.fontSize = 22;
            itemLabel.alignment = TextAlignmentOptions.MidlineLeft;
            itemToggle.graphic = null;
            itemToggle.isOn = false;

            scroll.viewport = viewport;
            scroll.content = content;
            dropdown.template = template;
            dropdown.itemText = itemLabel;
            dropdown.options.Clear();
            dropdown.options.Add(new TMP_Dropdown.OptionData("Option A"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("Option B"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("Option C"));
            template.gameObject.SetActive(false);
            Place(go, cmd);
        }

        [MenuItem(Root + "Scroll View", false, 36)]
        private static void CreateScrollView(MenuCommand cmd)
        {
            var go = CreateControl("Glass Scroll View", cmd);
            var bg = AddImage(go.transform, "Glass Surface", new Color(0.78f, 0.89f, 1f, 0.20f));
            bg.gameObject.AddComponent<GlassGraphic>();
            var scroll = go.AddComponent<ScrollRect>();
            var viewport = CreateRect("Viewport", go.transform);
            Stretch(viewport, 8, 8, 8, 8);
            viewport.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var content = CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = new Vector2(0, 500);
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            Place(go, cmd);
        }

        [MenuItem(Root + "Scrollbar", false, 37)]
        private static void CreateScrollbar(MenuCommand cmd)
        {
            var go = CreateControl("Glass Scrollbar", cmd);
            var bg = AddImage(go.transform, "Background", new Color(0.78f, 0.89f, 1f, 0.20f));
            bg.gameObject.AddComponent<GlassGraphic>();
            var bar = go.AddComponent<Scrollbar>();
            var area = CreateRect("Sliding Area", go.transform);
            Stretch(area, 4, 4, 4, 4);
            var handle = AddImage(area, "Handle", new Color(0.8f, 0.92f, 1f, 0.8f));
            Stretch(handle.rectTransform, 0, 0, 0, 0);
            bar.handleRect = handle.rectTransform;
            bar.targetGraphic = handle;
            Place(go, cmd);
        }


        [MenuItem(Root + "Button (Legacy)", false, 40)]
        private static void CreateLegacyButton(MenuCommand cmd) => CreateLegacy(DefaultControls.CreateButton(Resources()), "Glass Button (Legacy)", cmd);

        [MenuItem(Root + "Text (Legacy)", false, 41)]
        private static void CreateLegacyText(MenuCommand cmd) => CreateLegacy(DefaultControls.CreateText(Resources()), "Glass Text (Legacy)", cmd);

        [MenuItem(Root + "Input Field (Legacy)", false, 42)]
        private static void CreateLegacyInput(MenuCommand cmd) => CreateLegacy(DefaultControls.CreateInputField(Resources()), "Glass Input Field (Legacy)", cmd);

        [MenuItem(Root + "Dropdown (Legacy)", false, 43)]
        private static void CreateLegacyDropdown(MenuCommand cmd) => CreateLegacy(DefaultControls.CreateDropdown(Resources()), "Glass Dropdown (Legacy)", cmd);

        private static DefaultControls.Resources Resources()
        {
            return new DefaultControls.Resources
            {
                standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
                knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
                checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
                dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
                mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd")
            };
        }

        private static void CreateLegacy(GameObject go, string name, MenuCommand cmd)
        {
            go.name = name;
            var surface = go.GetComponent<Image>();
            if (!surface)
            {
                var images = go.GetComponentsInChildren<Image>(true);
                if (images.Length > 0) surface = images[0];
            }
            if (surface && !surface.GetComponent<GlassGraphic>())
                surface.gameObject.AddComponent<GlassGraphic>();
            if (go.GetComponent<Selectable>())
                go.AddComponent<GlassPointerFeedback>();
            GameObjectUtility.SetParentAndAlign(go, cmd.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            Selection.activeGameObject = go;
        }

        private static void CreateGlassGraphic(string name, MenuCommand cmd)
        {
            var go = CreateRect(name);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.82f, 0.91f, 1f, 0.25f);
            go.AddComponent<GlassPanel>();
            Place(go, cmd);
        }

        private static GameObject CreateControl(string name, MenuCommand cmd)
        {
            var go = CreateRect(name);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 64);
            return go;
        }

        private static GameObject CreateRect(string name, Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            if (parent) go.transform.SetParent(parent, false);
            return go;
        }

        private static Image AddImage(Transform parent, string name, Color color)
        {
            var go = CreateRect(name, parent);
            Stretch(go.GetComponent<RectTransform>(), 0, 0, 0, 0);
            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static TextMeshProUGUI AddTMP(Transform parent, string name, string value)
        {
            var go = CreateRect(name, parent);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.color = Color.white;
            text.fontSize = 24;
            return text;
        }

        private static void Stretch(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private static void Place(GameObject go, MenuCommand cmd)
        {
            GameObjectUtility.SetParentAndAlign(go, cmd.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeGameObject = go;
            if (!go.GetComponentInParent<Canvas>() && go.GetComponent<Canvas>() == null)
                Debug.LogWarning("Liquid Glass UI element created without a Canvas. Add it under a Canvas or use GameObject > UI > Liquid Glass > Glass Canvas.");
        }

        private static ColorBlock SoftColors()
        {
            var c = ColorBlock.defaultColorBlock;
            c.normalColor = new Color(1, 1, 1, 0.95f);
            c.highlightedColor = new Color(0.78f, 0.91f, 1f, 1f);
            c.pressedColor = new Color(0.55f, 0.78f, 0.95f, 1f);
            c.selectedColor = c.highlightedColor;
            return c;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>()) return;
            var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }
    }
}