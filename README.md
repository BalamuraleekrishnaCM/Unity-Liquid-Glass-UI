# Unity Liquid Glass UI

A Unity Package Manager package for Liquid Glass-style uGUI and TextMeshPro interfaces.

## Install from Git

**Window → Package Manager → + → Add package from git URL…**

`https://github.com/BalamuraleekrishnaCM/Unity-Liquid-Glass-UI.git`

Package ID: `com.balu.unity-liquid-glass`

## Create UI from Hierarchy

Use **GameObject → UI → Liquid Glass**:

- Glass Canvas, Panel, Image, Raw Image
- Button (TMP), Text (TMP), Input Field (TMP), Dropdown (TMP)
- Toggle, Slider, Scroll View, Scrollbar
- Button (Legacy), Text (Legacy), Input Field (Legacy), Dropdown (Legacy)

Generated objects use native uGUI/TMP components and preserve Unity's standard interaction and event pathways. Text keeps its normal SDF/legacy font material; the glass shader is applied to surface graphics.

## Enable live backdrop blur

### Built-in Render Pipeline
1. Add a Camera to the scene.
2. Add **Glass Background Capture (Built-in RP)** to that camera.
3. Adjust resolution scale and blur radius on the component.
4. Add Liquid Glass UI elements to a Canvas rendered after the camera.

### Universal Render Pipeline (URP 14+)
1. Open the active Universal Renderer Data asset.
2. Select **Add Renderer Feature → Liquid Glass Renderer Feature**.
3. Set the injection point to **Before Rendering Transparents**.
4. Tune blur resolution and radius in the feature settings.

URP integration is isolated in an optional assembly and does not add URP as a mandatory package dependency. It uses URP's Blitter path and XR texture macros. Confirm compatibility with the exact URP version and XR mode used by your project.

## Presets and interaction feedback

Create reusable presets with **Assets → Create → Liquid Glass → Glass Preset**. Assign a preset from code:

```csharp
using Balu.LiquidGlass;
using UnityEngine;

public class ApplyGlassPreset : MonoBehaviour
{
    [SerializeField] private GlassGraphic target;
    [SerializeField] private GlassPreset preset;

    private void Start()
    {
        if (target && preset) target.ApplyPreset(preset);
    }
}
```

Add **Glass Pointer Feedback** to a control to animate its glass intensity on pointer hover and press. It works with mouse/touch and compatible XR UI input modules.

## XR and platform support

- World-space Canvas is supported as a uGUI layout approach.
- For XR, configure the Canvas with the Tracked Device Graphic Raycaster and XR UI Input Module (or equivalent input module).
- The glass UI shader uses Unity stereo screen-texture macros; the URP blur pass uses Blitter and XR texture-array sampling. Validate single-pass instanced/multiview stereo behaviour on each target headset and GPU.
- Mobile, WebGL, desktop and XR support depends on render pipeline, shader model, graphics API, GPU memory and device capability. Reduce blur resolution/radius on constrained devices.
- visionOS requires project-specific Unity PolySpatial/visionOS integration and is not guaranteed by this uGUI package alone.
- Other Scriptable Render Pipelines need an equivalent capture/blur renderer feature.

## Rendering model and limitations

The shader provides a blurred backdrop sample, glass tint, frost, rim highlight and subtle screen-space refraction. It is a stylized real-time UI effect, not a physically based optical simulation. Blur is captured once per camera frame and shared by glass surfaces; per-surface blur radius adds a small local sample blur.

## Validation status

Source files are published, but this package has not been compiled or run inside Unity as part of this release. Before production use, validate clean imports and compilation in Unity 2022.3 LTS and Unity 6, verify every control's runtime interaction, and profile Built-in RP/URP on target mobile, desktop and XR devices.
