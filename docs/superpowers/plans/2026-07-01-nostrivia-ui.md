# Nostrivia UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Faithfully recreate the designer's Adobe XD "Nostrivia" prototype (9 screens + transitions) in Unity uGUI + TextMeshPro.

**Architecture:** Hybrid — native editable prefabs + one scene hold the visuals; behavior (navigation, transitions, state, audio) lives in focused runtime `MonoBehaviour`s referenced by the prefabs. A `ScreenManager` singleton owns navigation using a **Stage** model (active screen + its overlays live in one container so they animate together). All motion is built on a dependency-free `UITween` coroutine helper.

**Tech Stack:** Unity 6000.3.16f1, URP (active but irrelevant to the Screen-Space-Overlay canvas), uGUI 2.0.0 (TextMeshPro built in), Input System 1.19.0, Unity Test Framework 1.6.0. No external packages (no DOTween).

## Global Constraints

- **Artboard / reference resolution:** 360 × 640 px, portrait. `CanvasScaler` = Scale With Screen Size, reference 360×640.
- **Responsiveness:** Corner/edge elements pinned to their corner/edge; content centered/relative. Extra space → more background, never stretched content. No tablet support.
- **Text:** TextMeshPro only. Text color `#26233B`.
- **Input:** `activeInputHandler: 1` (Input System only) — `EventSystem` MUST use `InputSystemUIInputModule`, never `StandaloneInputModule`.
- **Color tokens** (verbatim, define once in `NostriviaColors`): White `#FFFFFF`, SkyBlue `#A9C4F0`, MintGreen `#A8E0C2`, Pink `#DDA8D2`, Coral `#E2917F`, Periwinkle `#6F7FE0`, Navy `#26233B`, LavenderGray `#D0CBDD`, Gold `#EFC583`.
- **Source of truth for exact values:** XD Specs mode at `https://xd.adobe.com/view/3cc8f38a-bbe9-4e7e-8750-c8ef3da2cf5b-699b/specs/` (no login). Read px sizes/positions, colors, font sizes, and per-screen Interactions (durations/easing) at execution. Unknowns → `Assets/Nostrivia/_DesignRef/OpenQuestions.md`, resolved by browser preview / spec / asking. Never hardcode a guessed value.
- **Branch:** `nostrivia-ui`. Commit after every task.
- **Design reference:** spec at `docs/superpowers/specs/2026-07-01-nostrivia-ui-design.md`; screen PNGs + `Results_To_Gameplay.mp4` in `Assets/Nostrivia/_DesignRef/`.

---

## Tooling notes (how to execute steps)

- **Build UI / prefabs:** `Unity_ManageGameObject` (create objects, add components incl. `Image`, `TextMeshProUGUI`, `RectTransform`, custom scripts; set properties; `save_as_prefab`), `Unity_ManageScene`, `Unity_ManageAsset`.
- **Arbitrary editor C#:** `Unity_RunCommand` (class `CommandScript : IRunCommand`) — used for TMP import, TMP font-asset generation, sprite reimport, AudioMixer creation.
- **Compile / errors:** `unity-bridge compile`, `Unity_ReadConsole`, `Unity_ValidateScript`.
- **Run tests:** `unity-bridge run-tests --mode EditMode --filter <Name>`.
- **Visual verify (static):** `Unity_Camera_Capture` (no cameraID → current Scene View; frame it on the canvas). Compare the returned image to the named XD PNG/screen.
- **Visual verify (transitions):** `Unity_ManageEditor` Play → capture frames → compare to XD preview. `ffmpeg` is available for frame extraction but **ask Omar to restart the session first** (PATH); prefer XD spec + browser preview.

---

## File Structure

**Scripts (runtime) — `Assets/Nostrivia/Scripts/`**
- `Core/NostriviaColors.cs` — static color tokens (the 9 hexes as `Color`).
- `Core/UITween.cs` — static easing + coroutine lerp helpers.
- `Core/Easing.cs` — pure easing functions (separated so they're trivially unit-testable).
- `Core/ScreenManager.cs` — navigation singleton + Stage orchestration.
- `Core/ScreenId.cs` — enum of screens/overlays.
- `Core/AudioSettings.cs` — `PercentToDecibels` mapping + mixer param setters.
- `Screens/HomeScreen.cs`, `Screens/GameplayScreen.cs`
- `Overlays/SettingsOverlay.cs`, `Overlays/ResultsOverlay.cs`, `Overlays/LeaveOverlay.cs`
- `Components/AnswerOption.cs`, `Components/CountdownTimer.cs`
- `Nostrivia.asmdef` — runtime assembly (refs `Unity.TextMeshPro`, `Unity.InputSystem`).

**Tests (EditMode) — `Assets/Nostrivia/Tests/`**
- `Nostrivia.Tests.asmdef` (refs `Nostrivia`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`; `"editorOnly"`/EditMode).
- `EasingTests.cs`, `GameplayStateMachineTests.cs`, `AudioSettingsTests.cs`.

**Prefabs — `Assets/Nostrivia/Prefabs/`**
- `Components/` — `Background.prefab`, `Y2KButton.prefab`, `DialogFrame.prefab`, `AnswerOption.prefab`, `TimerPill.prefab`.
- `Screens/` — `HomeScreen.prefab`, `GameplayScreen.prefab`.
- `Overlays/` — `SettingsOverlay.prefab`, `ResultsOverlay.prefab`, `LeaveOverlay.prefab`.

**Other**
- `Assets/Nostrivia/Scenes/Nostrivia.unity` — the single scene.
- `Assets/Nostrivia/Fonts/TMP/` — generated TMP font assets + outline material presets.
- `Assets/Nostrivia/Audio/NostriviaMixer.mixer` — SFX + Music groups, exposed `SFXVolume`/`MusicVolume`.

---

## Phase 0 — Prerequisites & project setup

### Task 1: Import TMP Essential Resources

**Files:** none authored; imports into `Assets/TextMesh Pro/`.

- [ ] **Step 1: Import via RunCommand.** `Unity_RunCommand` with a `CommandScript` that locates the TMP essentials package and imports it silently:

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;
internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        // uGUI 2.0.0 ships TMP; essentials .unitypackage lives in the package cache.
        string root = Path.GetFullPath("Packages/com.unity.ugui");
        string[] hits = Directory.GetFiles(root, "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);
        if (hits.Length == 0) { result.LogError("TMP essentials package not found under {0}", root); return; }
        AssetDatabase.ImportPackage(hits[0], false); // false = no interactive dialog
        result.Log("Imported TMP essentials from {0}", hits[0]);
    }
}
```

- [ ] **Step 2: Refresh + verify.** `unity-bridge refresh`, then confirm `Assets/TextMesh Pro/Resources/TMP Settings.asset` exists (Glob `Assets/TextMesh Pro/**/TMP Settings.asset`). Expected: one match.
- [ ] **Step 3: Commit.** `git add "Assets/TextMesh Pro" && git commit -m "chore: import TMP essential resources"`

### Task 2: Reimport Nostrivia sprites as Sprite (2D and UI)

**Files:** modifies the 12 `Assets/Nostrivia/Sprites/*.png.meta`.

- [ ] **Step 1: Reimport via RunCommand.** Set each PNG's importer to Sprite, single mode, no mipmaps, alphaIsTransparency on:

```csharp
using UnityEngine; using UnityEditor; using System.IO;
internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[]{"Assets/Nostrivia/Sprites"}))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.SaveAndReimport();
            result.Log("Sprite: {0}", path);
        }
    }
}
```

- [ ] **Step 2: Verify.** Grep a `.png.meta` (e.g. `Logo.png.meta`) for `textureType: 8` and `spriteMode: 1`. Expected: present.
- [ ] **Step 3: Commit.** `git add Assets/Nostrivia/Sprites && git commit -m "chore: import Nostrivia sprites as UI sprites"`

### Task 3: Create the scene with Canvas + EventSystem

**Files:** Create `Assets/Nostrivia/Scenes/Nostrivia.unity`.

- [ ] **Step 1:** `Unity_ManageScene` Create `Nostrivia` under `Assets/Nostrivia/Scenes/`, then Load it.
- [ ] **Step 2:** `Unity_ManageGameObject` create `Canvas` (add `Canvas`, `CanvasScaler`, `GraphicRaycaster`). Set `Canvas.renderMode = ScreenSpaceOverlay`; `CanvasScaler.uiScaleMode = ScaleWithScreenSize`, `referenceResolution = (360,640)`, `screenMatchMode = MatchWidthOrHeight`, `matchWidthOrHeight = 0.5` (revisit per responsiveness in Task 23).
- [ ] **Step 3:** Create `EventSystem` GameObject; add `EventSystem` + `InputSystemUIInputModule` (NOT StandaloneInputModule — `activeInputHandler:1`).
- [ ] **Step 4:** Add the scene to Build Settings (index 0). Save scene (`Unity_ManageScene` Save).
- [ ] **Step 5: Verify compile clean** (`unity-bridge compile`) and capture the empty scene (`Unity_Camera_Capture`) — expect an empty canvas, no errors.
- [ ] **Step 6: Commit.** `git add Assets/Nostrivia/Scenes && git commit -m "feat: add Nostrivia scene with overlay canvas + input-system EventSystem"`

### Task 4: Assembly definitions + color tokens

**Files:** Create `Assets/Nostrivia/Scripts/Nostrivia.asmdef`, `Assets/Nostrivia/Scripts/Core/NostriviaColors.cs`, `Assets/Nostrivia/Tests/Nostrivia.Tests.asmdef`.

**Interfaces — Produces:** `NostriviaColors.SkyBlue/White/MintGreen/Pink/Coral/Periwinkle/Navy/LavenderGray/Gold` (static `Color`); `NostriviaColors.Hex(string)` helper.

- [ ] **Step 1:** Create `Nostrivia.asmdef`:

```json
{ "name": "Nostrivia", "references": ["Unity.TextMeshPro", "Unity.InputSystem"], "autoReferenced": true }
```

- [ ] **Step 2:** Create `NostriviaColors.cs`:

```csharp
using UnityEngine;
namespace Nostrivia {
  public static class NostriviaColors {
    public static Color Hex(string h){ ColorUtility.TryParseHtmlString(h, out var c); return c; }
    public static readonly Color White        = Hex("#FFFFFF");
    public static readonly Color SkyBlue       = Hex("#A9C4F0");
    public static readonly Color MintGreen     = Hex("#A8E0C2");
    public static readonly Color Pink          = Hex("#DDA8D2");
    public static readonly Color Coral         = Hex("#E2917F");
    public static readonly Color Periwinkle    = Hex("#6F7FE0");
    public static readonly Color Navy          = Hex("#26233B");
    public static readonly Color LavenderGray  = Hex("#D0CBDD");
    public static readonly Color Gold          = Hex("#EFC583");
  }
}
```

- [ ] **Step 3:** Create `Nostrivia.Tests.asmdef`:

```json
{ "name": "Nostrivia.Tests", "references": ["Nostrivia", "UnityEngine.TestRunner", "UnityEditor.TestRunner"],
  "includePlatforms": ["Editor"], "optionalUnityReferences": ["TestAssemblies"] }
```

- [ ] **Step 4: Verify.** `unity-bridge compile` → success. `unity-bridge run-tests --mode EditMode` → runs (0 tests OK).
- [ ] **Step 5: Commit.** `git add Assets/Nostrivia/Scripts Assets/Nostrivia/Tests && git commit -m "feat: add Nostrivia asmdefs and color tokens"`

---

## Phase 1 — Foundation logic & shared pieces

### Task 5: Easing functions (TDD)

**Files:** Create `Scripts/Core/Easing.cs`, `Tests/EasingTests.cs`.

**Interfaces — Produces:** `Nostrivia.Easing.Linear(float t)`, `EaseOutCubic(float t)`, `EaseInOutCubic(float t)` — all take/return `float`, clamp `t` to `[0,1]`.

- [ ] **Step 1: Failing test** (`EasingTests.cs`):

```csharp
using NUnit.Framework; using Nostrivia;
public class EasingTests {
  [Test] public void Linear_endpoints(){ Assert.AreEqual(0f, Easing.Linear(0f), 1e-4f); Assert.AreEqual(1f, Easing.Linear(1f), 1e-4f); }
  [Test] public void EaseOutCubic_midpoint_is_fast_start(){ Assert.Greater(Easing.EaseOutCubic(0.5f), 0.5f); }
  [Test] public void EaseOutCubic_endpoints(){ Assert.AreEqual(0f, Easing.EaseOutCubic(0f),1e-4f); Assert.AreEqual(1f, Easing.EaseOutCubic(1f),1e-4f); }
  [Test] public void EaseInOutCubic_symmetric(){ Assert.AreEqual(0.5f, Easing.EaseInOutCubic(0.5f),1e-4f); }
  [Test] public void Clamps_out_of_range(){ Assert.AreEqual(1f, Easing.EaseOutCubic(2f),1e-4f); Assert.AreEqual(0f, Easing.EaseOutCubic(-1f),1e-4f); }
}
```

- [ ] **Step 2: Run, expect FAIL** (`Easing` undefined): `unity-bridge run-tests --mode EditMode --filter EasingTests`
- [ ] **Step 3: Implement** (`Easing.cs`):

```csharp
using UnityEngine;
namespace Nostrivia {
  public static class Easing {
    public static float Linear(float t){ return Mathf.Clamp01(t); }
    public static float EaseOutCubic(float t){ t = Mathf.Clamp01(t); float u = 1f - t; return 1f - u*u*u; }
    public static float EaseInOutCubic(float t){ t = Mathf.Clamp01(t);
      return t < 0.5f ? 4f*t*t*t : 1f - Mathf.Pow(-2f*t + 2f, 3f)/2f; }
  }
}
```

- [ ] **Step 4: Run, expect PASS.**
- [ ] **Step 5: Commit.** `git commit -am "feat: easing functions with tests"`

### Task 6: UITween coroutine helpers

**Files:** Create `Scripts/Core/UITween.cs`. (Coroutine motion is verified in Play mode later; the math is already covered by Task 5.)

**Interfaces — Produces:**
- `UITween.MoveAnchored(RectTransform rt, Vector2 to, float dur, System.Func<float,float> ease, System.Action onDone=null) : Coroutine` (runs on a shared runner).
- `UITween.Fade(CanvasGroup cg, float toAlpha, float dur, System.Func<float,float> ease, System.Action onDone=null) : Coroutine`
- `UITween.Kill(Coroutine handle)` / per-target cancellation so a new tween on a target cancels the prior one.

- [ ] **Step 1:** Implement `UITween` with a hidden persistent runner MonoBehaviour (`[RuntimeInitializeOnLoadMethod]`-created), a `Dictionary<object,Coroutine>` keyed by target for auto-cancel, and IEnumerator lerps using `Easing`. Use unscaled time (`Time.unscaledDeltaTime`) so transitions are framerate/timescale independent.
- [ ] **Step 2:** `unity-bridge compile` → success; `Unity_ValidateScript` on the file → no errors.
- [ ] **Step 3: Commit.** `git commit -am "feat: UITween coroutine helpers"`

### Task 7: Generate TMP font assets + outline material presets

**Files:** creates `Assets/Nostrivia/Fonts/TMP/*.asset`.

- [ ] **Step 1:** `Unity_RunCommand` — for each `.ttf` in `Assets/Nostrivia/Fonts/`, create an SDF font asset and save it:

```csharp
using UnityEngine; using UnityEditor; using TMPro; using System.IO;
internal class CommandScript : IRunCommand {
  public void Execute(ExecutionResult result){
    Directory.CreateDirectory("Assets/Nostrivia/Fonts/TMP");
    foreach (string guid in AssetDatabase.FindAssets("t:Font", new[]{"Assets/Nostrivia/Fonts"})){
      string p = AssetDatabase.GUIDToAssetPath(guid);
      var font = AssetDatabase.LoadAssetAtPath<Font>(p);
      var fa = TMP_FontAsset.CreateFontAsset(font); // dynamic SDF
      string outPath = "Assets/Nostrivia/Fonts/TMP/" + Path.GetFileNameWithoutExtension(p) + " SDF.asset";
      AssetDatabase.CreateAsset(fa, outPath);
      foreach (var o in fa.material ? new Object[]{fa.material} : new Object[0]) AssetDatabase.AddObjectToAsset(o, fa);
      result.Log("TMP font: {0}", outPath);
    }
    AssetDatabase.SaveAssets();
  }
}
```

- [ ] **Step 2:** Create an **outline material preset** for the question text and "Excellent!" title (navy outline, colored face): duplicate the BagelFatOne SDF material, set `_OutlineColor` = Navy, `_OutlineWidth` per XD (read the stroke width from the Question screen spec). Save under `Fonts/TMP/`.
- [ ] **Step 3: Verify.** Glob `Assets/Nostrivia/Fonts/TMP/*SDF.asset` → 5 matches. Compile clean.
- [ ] **Step 4: Commit.** `git add Assets/Nostrivia/Fonts/TMP && git commit -m "feat: generate TMP font assets + outline preset"`

### Task 8: ScreenManager + Stage skeleton

**Files:** Create `Scripts/Core/ScreenId.cs`, `Scripts/Core/ScreenManager.cs`.

**Interfaces — Produces:**
- `enum ScreenId { Home, Gameplay }`; `enum OverlayId { Settings, Results, Leave }`.
- `ScreenManager.Instance` (scene singleton).
- `RegisterScreen(ScreenId, GameObject)`, `RegisterOverlay(OverlayId, GameObject)`.
- `ShowOverlay(OverlayId)` / `HideOverlay(OverlayId)` — slide up/down within current Stage.
- `TransitionToScreen(ScreenId, TransitionKind)` where `enum TransitionKind { SlideUp, DualSlide }`.
- A `Stage` inner concept: full-screen `RectTransform` holding the active screen + its overlays.

- [ ] **Step 1:** Implement the singleton + registries + Stage container creation. Overlay show = instantiate/activate under current Stage at `anchoredPosition.y = -H`, `UITween.MoveAnchored` to `0`. Hide = reverse then deactivate. `TransitionToScreen(DualSlide)` = create new Stage above (`y=+H`), move old Stage to `-H` and new to `0` simultaneously, destroy old on done. Durations/easing are serialized fields (set from XD in the transition tasks).
- [ ] **Step 2:** `unity-bridge compile` → success.
- [ ] **Step 3: Commit.** `git commit -am "feat: ScreenManager + Stage navigation skeleton"`

### Task 9: Shared component prefabs (background, Y2K button, dialog frame)

**Files:** Create `Prefabs/Components/Background.prefab`, `Y2KButton.prefab`, `DialogFrame.prefab`.

- [ ] **Step 1: Background** — `Unity_ManageGameObject`: full-screen `Image` tinted SkyBlue (`#A9C4F0`) as the grid placeholder (grid asset pending — see OpenQuestions), plus child ornament `Image`s (Star/Circle/Triangle) positioned per the Home screen spec. Save as prefab.
- [ ] **Step 2: Y2KButton** — a reusable button: `Image` (fill color param) + navy 1px border + offset drop-shadow child + `TextMeshProUGUI` label (BagelFatOne SDF) + `Button`. Save as prefab. Read padding/shadow offset from the Play button spec on Home.
- [ ] **Step 3: DialogFrame** — the Windows-9x popup shell: white body `Image`, 1px navy border, a colored title-bar child (color param) with a title `TextMeshProUGUI` + white X `Button` (Cross sprite tinted white). Save as prefab.
- [ ] **Step 4: Verify.** `Unity_Camera_Capture` each prefab instance in-scene; compare button/dialog styling to the XD (Home Play button; Settings dialog). Iterate values from spec.
- [ ] **Step 5: Commit.** `git add Assets/Nostrivia/Prefabs/Components && git commit -m "feat: shared background, button, dialog-frame prefabs"`

---

## Phase 2 — Home vertical slice (Home + Settings + first transition)

### Task 10: Home screen prefab + controller

**Files:** Create `Prefabs/Screens/HomeScreen.prefab`, `Scripts/Screens/HomeScreen.cs`.

**Interfaces — Produces:** `HomeScreen.PlayClicked : event Action`, `HomeScreen.SettingsClicked : event Action`.

- [ ] **Step 1:** Build hierarchy under a Home root: Background (instance of Task 9), `Logo` Image (centered per spec), tagline `TextMeshProUGUI` "Fun Y2K Trivia!" (Nunito Black 16px, Navy), Play `Y2KButton` (Pink fill, label "Play"), Gear `Button` (Gear sprite tinted Navy, pinned top-right). Read every position/size from the Home spec (`/specs/`, Home screen). Pin gear to top-right anchor; center logo/Play.
- [ ] **Step 2:** `HomeScreen.cs` wires Play→`PlayClicked`, Gear→`SettingsClicked`. Save prefab, place instance in scene, register with ScreenManager.
- [ ] **Step 3: Verify.** `Unity_Camera_Capture` → compare to `_DesignRef/Home.png` and the live XD Home screen. Iterate until faithful.
- [ ] **Step 4: Commit.** `git add -A && git commit -m "feat: Home screen"`

### Task 11: Settings overlay prefab + controller

**Files:** Create `Prefabs/Overlays/SettingsOverlay.prefab`, `Scripts/Overlays/SettingsOverlay.cs`.

**Interfaces — Produces:** `SettingsOverlay.CloseClicked : event Action`; `SettingsOverlay.SetSfx(float pct)`, `SetMusic(float pct)` (drive labels + events `SfxChanged`/`MusicChanged`).

- [ ] **Step 1:** Build backdrop (full-screen Image, tint/opacity read from XD) + DialogFrame (title bar Periwinkle `#6F7FE0`, "SETTINGS", white X). Sections: **Sounds** (SFX + Music `Slider`s with `%` labels), **Notifications** (New/Event trivia toggle graphics), **Difficulty** (Easy/Medium/Difficult radio graphics, Easy selected). Read all metrics from the Settings spec.
- [ ] **Step 2:** `SettingsOverlay.cs`: X→`CloseClicked`; sliders update `%` labels + fire change events; toggles/radios are visual-only groups (radios mutually exclusive visually, no persistence).
- [ ] **Step 3: Verify.** Capture; compare to `_DesignRef/Settings.png`.
- [ ] **Step 4: Commit.** `git add -A && git commit -m "feat: Settings overlay (visual)"`

### Task 12: Home↔Settings transition

**Files:** Modify `ScreenManager.cs` wiring in the scene; no new files.

- [ ] **Step 1:** Read the Home→Settings interaction from XD Specs (Trigger Tap, Action Overlay → Settings): note **duration + easing**. Set ScreenManager's overlay-in duration/easing to those values. Wire `HomeScreen.SettingsClicked → ShowOverlay(Settings)` and `SettingsOverlay.CloseClicked → HideOverlay(Settings)`.
- [ ] **Step 2: Verify in Play mode.** `Unity_ManageEditor` Play → trigger settings → capture ~3 frames of the slide → compare motion to XD preview (whole overlay incl. backdrop slides up, **no fade**). Confirm close slides down. Log final duration/easing in OpenQuestions if adjusted.
- [ ] **Step 3: Commit.** `git commit -am "feat: Home<->Settings slide transition"`

### Task 13: Settings audio wiring (TDD for the mapping)

**Files:** Create `Scripts/Core/AudioSettings.cs`, `Tests/AudioSettingsTests.cs`, `Assets/Nostrivia/Audio/NostriviaMixer.mixer`.

**Interfaces — Produces:** `AudioSettings.PercentToDecibels(float pct0to100) : float` (0%→-80dB mute; 100%→0dB; log curve).

- [ ] **Step 1: Failing test:**

```csharp
using NUnit.Framework; using Nostrivia;
public class AudioSettingsTests {
  [Test] public void Zero_is_muted(){ Assert.AreEqual(-80f, AudioSettings.PercentToDecibels(0f), 0.01f); }
  [Test] public void Full_is_zero_dB(){ Assert.AreEqual(0f, AudioSettings.PercentToDecibels(100f), 0.01f); }
  [Test] public void Half_is_negative(){ Assert.Less(AudioSettings.PercentToDecibels(50f), 0f); }
  [Test] public void Monotonic(){ Assert.Less(AudioSettings.PercentToDecibels(25f), AudioSettings.PercentToDecibels(60f)); }
}
```

- [ ] **Step 2: Run, expect FAIL.** `unity-bridge run-tests --mode EditMode --filter AudioSettingsTests`
- [ ] **Step 3: Implement:**

```csharp
using UnityEngine;
namespace Nostrivia {
  public static class AudioSettings {
    public static float PercentToDecibels(float pct){
      float n = Mathf.Clamp01(pct/100f);
      if (n <= 0.0001f) return -80f;
      return Mathf.Log10(n) * 20f;
    }
  }
}
```

- [ ] **Step 4: Run, expect PASS.**
- [ ] **Step 5:** Create `NostriviaMixer` (RunCommand or `Unity_ManageAsset`) with child groups **SFX** and **Music**, expose their volumes as `SFXVolume` / `MusicVolume`. Wire `SettingsOverlay.SfxChanged/MusicChanged → mixer.SetFloat(param, PercentToDecibels(pct))`. Default SFX 60%, Music 25% (from the Settings art).
- [ ] **Step 6: Verify.** Play, drag sliders, confirm labels update and `GetFloat` reflects mapping (log via RunCommand).
- [ ] **Step 7: Commit.** `git add -A && git commit -m "feat: Settings audio mixer wiring with tested dB mapping"`

**✅ Milestone: vertical slice complete** — build→capture→compare loop and one transition proven end-to-end. Reassess with Omar before Phase 3.

---

## Phase 3 — Gameplay

### Task 14: Gameplay state machine (TDD)

**Files:** Create `Scripts/Core/GameplayState.cs`, `Tests/GameplayStateMachineTests.cs`.

**Interfaces — Produces:** `enum QuestionPhase { Idle, Selected, Revealed }`; class `GameplayState` with `int CorrectIndex` (=1, Internet Explorer), `int? Selected`, `QuestionPhase Phase`, `bool CanSubmit`, `void Select(int i)`, `bool Submit()` (returns false if not Selected), `bool IsSelectedCorrect`.

- [ ] **Step 1: Failing tests:**

```csharp
using NUnit.Framework; using Nostrivia;
public class GameplayStateMachineTests {
  [Test] public void Starts_idle_cannot_submit(){ var s=new GameplayState(); Assert.AreEqual(QuestionPhase.Idle, s.Phase); Assert.IsFalse(s.CanSubmit); }
  [Test] public void Select_enables_submit(){ var s=new GameplayState(); s.Select(0); Assert.AreEqual(QuestionPhase.Selected,s.Phase); Assert.IsTrue(s.CanSubmit); }
  [Test] public void Submit_without_selection_fails(){ var s=new GameplayState(); Assert.IsFalse(s.Submit()); Assert.AreEqual(QuestionPhase.Idle,s.Phase); }
  [Test] public void Submit_reveals(){ var s=new GameplayState(); s.Select(1); Assert.IsTrue(s.Submit()); Assert.AreEqual(QuestionPhase.Revealed,s.Phase); }
  [Test] public void Correct_index_is_IE(){ var s=new GameplayState(); s.Select(1); s.Submit(); Assert.IsTrue(s.IsSelectedCorrect); }
  [Test] public void Wrong_selection(){ var s=new GameplayState(); s.Select(0); s.Submit(); Assert.IsFalse(s.IsSelectedCorrect); }
}
```

- [ ] **Step 2: Run, expect FAIL.**
- [ ] **Step 3: Implement `GameplayState.cs`** to satisfy the tests (CorrectIndex default 1; Select sets Selected+Phase=Selected; Submit only from Selected → Revealed).
- [ ] **Step 4: Run, expect PASS.**
- [ ] **Step 5: Commit.** `git commit -am "feat: gameplay state machine with tests"`

### Task 15: AnswerOption component + prefab

**Files:** Create `Scripts/Components/AnswerOption.cs`, `Prefabs/Components/AnswerOption.prefab`.

**Interfaces — Produces:** `AnswerOption.SetVisualState(enum OptionVisual { Default, Selected, Correct, Wrong })`; `AnswerOption.Clicked : event Action<int>`; `int Index`.

- [ ] **Step 1:** Prefab: white `Image` + navy border + `TextMeshProUGUI` (Nunito). `SetVisualState` maps Default=White/Navy text, Selected=Periwinkle/White text, Correct=MintGreen, Wrong=Coral (from Gameplay_Selection/Correct/Wrong PNGs).
- [ ] **Step 2: Verify** each state via capture vs the three answer-state PNGs.
- [ ] **Step 3: Commit.** `git add -A && git commit -m "feat: AnswerOption component"`

### Task 16: Countdown timer component

**Files:** Create `Scripts/Components/CountdownTimer.cs`, `Prefabs/Components/TimerPill.prefab`.

**Interfaces — Produces:** `CountdownTimer.StartFrom(float seconds)`, `event Action Elapsed`, serialized blink threshold (10s) + blink colors (Gold + red — read exact hexes from XD/OpenQuestions).

- [ ] **Step 1:** TimerPill prefab: Gold `Image` pill + Clock sprite (Navy) + `TextMeshProUGUI` "30 s". Component counts down (unscaled), updates label; at ≤10s blinks background between the two colors; fires `Elapsed` at 0 (behavior at 0 = stop, per OpenQuestions).
- [ ] **Step 2: Verify** default + blinking state via Play + capture.
- [ ] **Step 3: Commit.** `git add -A && git commit -m "feat: countdown timer with blink"`

### Task 17: Gameplay screen prefab + controller

**Files:** Create `Prefabs/Screens/GameplayScreen.prefab`, `Scripts/Screens/GameplayScreen.cs`.

**Interfaces — Produces:** `GameplayScreen.ExitClicked`, `GameplayScreen.ResultsClicked : event Action`.

- [ ] **Step 1:** Build: Background, X `Button` (top-left, →`ExitClicked`), TimerPill, question card (Periwinkle "1 / 10" badge, outlined question `TextMeshProUGUI` in BagelFatOne, MintGreen "✓ 5" badge), four `AnswerOption`s (Opera/Internet Explorer/Netscape Navigator/Mozilla Firefox), Submit `Y2KButton`. Read layout from the Question spec.
- [ ] **Step 2:** Controller owns a `GameplayState`: option click → `state.Select` + refresh visuals + enable Submit (LavenderGray→Pink); Submit → `state.Submit()`, recolor chosen option Correct/Wrong, morph Submit button into **Results** (label+color swap) → subsequent click fires `ResultsClicked`. Start timer on enable.
- [ ] **Step 3: Verify** all four states vs PNGs (`Gameplay.png`, `Gameplay_Selection.png`, `Gamepaly_Correct.png`, `Gameplay_Wrong.png`).
- [ ] **Step 4: Commit.** `git add -A && git commit -m "feat: Gameplay screen with states"`

### Task 18: Home→Gameplay transition

- [ ] **Step 1:** Read Play→Question interaction from XD Specs (duration/easing; confirm Gameplay-slides-up + whether Home holds — OpenQuestions). Wire `HomeScreen.PlayClicked → TransitionToScreen(Gameplay, SlideUp)`.
- [ ] **Step 2: Verify** in Play; compare to XD preview.
- [ ] **Step 3: Commit.** `git commit -am "feat: Home->Gameplay transition"`

---

## Phase 4 — Results, Leave & dual-slide

### Task 19: Results overlay + Gameplay→Results transition

**Files:** Create `Prefabs/Overlays/ResultsOverlay.prefab`, `Scripts/Overlays/ResultsOverlay.cs`.

**Interfaces — Produces:** `ResultsOverlay.PlayAgainClicked`, `ResultsOverlay.BackToHomeClicked : event Action`.

- [ ] **Step 1:** Backdrop + DialogFrame (Periwinkle "GAME OVER" bar), outlined MintGreen "Excellent!" (BagelFatOne outline preset), "You got 9 answers out of 10." (bold 9/10), Back-to-Home (white) + Play-Again (MintGreen) buttons. Hardcoded. Read from `Results.png`.
- [ ] **Step 2:** Wire `GameplayScreen.ResultsClicked → ShowOverlay(Results)` with slide-up (verify whether Results has any fade — OpenQuestions).
- [ ] **Step 3: Verify** vs `Results.png` + transition preview.
- [ ] **Step 4: Commit.** `git add -A && git commit -m "feat: Results overlay + transition"`

### Task 20: Leave overlay + Gameplay→Leave transition

**Files:** Create `Prefabs/Overlays/LeaveOverlay.prefab`, `Scripts/Overlays/LeaveOverlay.cs`.

**Interfaces — Produces:** `LeaveOverlay.CancelClicked`, `LeaveOverlay.LeaveClicked : event Action`.

- [ ] **Step 1:** Backdrop + DialogFrame (Coral "WARNING" bar), "Are you sure you want to leave?", Cancel (white) + Leave (Coral) buttons. Read from `Leave.png`.
- [ ] **Step 2:** Wire `GameplayScreen.ExitClicked → ShowOverlay(Leave)`; `CancelClicked → HideOverlay(Leave)` (slide down, resume).
- [ ] **Step 3: Verify** vs `Leave.png`.
- [ ] **Step 4: Commit.** `git add -A && git commit -m "feat: Leave overlay + transition"`

### Task 21: Dual-slide screen swaps

Covers Results→Play Again (fresh Gameplay), Results→Back to Home, Leave→Leave (Home).

- [ ] **Step 1:** Study `Results_To_Gameplay.mp4` + XD preview for the dual-slide (new screen drops from top while old screen+overlay slide out bottom, simultaneously). Extract exact duration/easing from spec; if frame timing is needed, ask Omar to restart for `ffmpeg`.
- [ ] **Step 2:** Wire via `TransitionToScreen(target, DualSlide)`: `ResultsOverlay.PlayAgainClicked → DualSlide to a fresh Gameplay`; `ResultsOverlay.BackToHomeClicked → DualSlide to Home`; `LeaveOverlay.LeaveClicked → DualSlide to Home`. Ensure the outgoing Stage carries both the old screen AND its overlay (Stage model already does this).
- [ ] **Step 3: Verify** each dual-slide in Play vs XD preview; the Play-Again path must reset Gameplay to `Idle`.
- [ ] **Step 4: Commit.** `git add -A && git commit -m "feat: dual-slide screen swaps"`

---

## Phase 5 — Full-flow verification & responsiveness

### Task 22: End-to-end flow + responsive pass

- [ ] **Step 1:** Play-mode walkthrough of the entire graph (Home→Settings→Home→Play→Gameplay→select→submit→Results→Play Again→…; Gameplay→X→Leave→Cancel/Leave). Capture a frame per screen; compare to PNGs.
- [ ] **Step 2:** Test at 3 resolutions (360×640 baseline, a taller 360×780, a wider 400×640). Confirm corner/edge elements stay pinned and content stays centered (tune `CanvasScaler.matchWidthOrHeight` + anchors). Fix any drift.
- [ ] **Step 3:** `unity-bridge run-tests --mode EditMode` (all logic green); `Unity_ReadConsole` clean (no errors/warnings).
- [ ] **Step 4: Commit.** `git commit -am "test: full-flow + responsive verification"`

### Task 23: Finalize

- [ ] **Step 1:** Review `OpenQuestions.md`; ensure every resolved item is annotated and unresolved items are clearly flagged for the designer (esp. the grid background asset).
- [ ] **Step 2:** Update the spec's status to "Implemented (pending designer items)".
- [ ] **Step 3: Commit.** `git commit -am "docs: finalize open questions + spec status"`

---

## Verification summary (how to prove it works)

- **Logic:** `unity-bridge run-tests --mode EditMode` — Easing, GameplayStateMachine, AudioSettings all green.
- **Per-screen fidelity:** `Unity_Camera_Capture` of each screen vs the matching `_DesignRef/*.png` and the live XD `/specs/` screen.
- **Transitions:** `Unity_ManageEditor` Play → trigger each transition → capture frames → compare to the XD interactive preview (durations/easing sourced from the XD interaction spec).
- **No regressions:** `Unity_ReadConsole` shows zero errors after the full-flow walkthrough.

## Open questions (live)

Maintained in `Assets/Nostrivia/_DesignRef/OpenQuestions.md`: background grid asset (missing); per-transition durations/easing; Results fade-vs-slide; Home-holds-vs-moves on Play; timer 0-behavior + blink hexes; answer-reveal highlighting. Resolve via XD preview/spec or ask the designer — never guess.
