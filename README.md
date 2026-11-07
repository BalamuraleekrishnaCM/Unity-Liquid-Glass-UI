# Unity Liquid Glass UI

A Unity Package Manager package providing native uGUI and TextMeshPro controls with a Liquid Glass-style translucent surface treatment.

## Install from Git

In Unity, open **Window → Package Manager → + → Add package from git URL…** and enter:

`https://github.com/BalamuraleekrishnaCM/Unity-Liquid-Glass-UI.git`

For a pinned release:

`https://github.com/BalamuraleekrishnaCM/Unity-Liquid-Glass-UI.git#v2.0.0`

## Hierarchy menu

Use **GameObject → UI → Liquid Glass** (also available from the Hierarchy context menu):

- Glass Canvas
- Panel
- Image / Raw Image
- Button (TMP)
- Text (TMP)
- Input Field (TMP)
- Dropdown (TMP)
- Toggle
- Slider
- Scroll View
- Scrollbar

Generated controls use native Unity uGUI and TextMeshPro components, retaining their normal interaction/event systems. Glass is applied to the control's surface; TMP text retains its normal SDF material for crisp text.

## Compatibility

- Unity 2022.3 LTS and Unity 6
- uGUI and TextMeshPro
- XR world-space Canvas workflows; configure EventSystem and XR UI input modules for your XR stack.

## Rendering scope

v2.0.0 supplies editor creation workflows and translucent glass surface styling. It does **not** yet ship production-grade universal backdrop blur/refraction. Live blur needs a render-pipeline-specific capture/blur path (for example, a URP Renderer Feature) and validation for mobile GPUs, WebGL, stereo XR and visionOS. The optional camera capture helper is single-camera and not stereo-correct.

Validate controls, input, layout, accessibility and performance in each target Unity version/device before production use.
