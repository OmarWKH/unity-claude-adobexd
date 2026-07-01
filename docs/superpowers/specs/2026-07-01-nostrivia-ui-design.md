# Nostrivia UI — Design Spec

**Date:** 2026-07-01
**Status:** Implemented (all 9 screens + transitions Play-verified; pending designer items in `Assets/Nostrivia/_DesignRef/OpenQuestions.md`: background grid asset, AudioMixer asset creation, transition-timing/blink-color confirmation)
**Goal:** Faithfully recreate the designer's Adobe XD prototype for "Nostrivia" in Unity
uGUI — the UI layout and the transitions. Gameplay is out of scope beyond what the prototype
shows (a single hardcoded question).

---

## 1. Context & source of truth

- **Prototype (source of truth):** Adobe XD view link —
  `https://xd.adobe.com/view/3cc8f38a-bbe9-4e7e-8750-c8ef3da2cf5b-699b/`. It is fully viewable
  in a browser **without Adobe sign-in**. Specs mode (`/specs/`) exposes exact px sizes, colors,
  fonts, and per-screen **Interactions** (Trigger / Action / Destination). The designer also left
  per-screen annotation comments. Interactive preview is used to observe transitions.
- **Local design reference:** `Assets/Nostrivia/_DesignRef/` — `DesignRef.md` (screens, flow,
  transitions, dimensions), screen PNGs, and `Results_To_Gameplay.mp4`. Open items are tracked in
  `Assets/Nostrivia/_DesignRef/OpenQuestions.md`.
- **Provided assets:** `Assets/Nostrivia/Fonts/` (BagelFatOne-Regular = display; Nunito
  Black/Bold/ExtraBold/SemiBold = body) and `Assets/Nostrivia/Sprites/` (Logo; ornaments
  Star/Circle/Triangle = pastel-gradient decals; white tintable glyphs Gear/Clock/Check/Cross/
  Circle/Square + outlines).

## 2. Target & constraints

- **Artboard:** 360 × 640 px, mobile **portrait** (viewport == design size).
- **Responsiveness:** Must look good on taller/wider phones (not tablets). Corner/edge elements
  (gear, X, timer, close buttons) stay **pinned** to their corner/edge; content blocks (logo,
  Play, question card, options, dialogs) stay centered/relative. Extra space becomes more
  background, never stretched content.
- **Text:** TextMeshPro. SDF font assets generated from the provided `.ttf` files. Text color
  token `#26233B`.
- **Style:** Y2K — playful chunky Bagel display type over a pastel grid, with popups styled as
  Windows-9x dialog boxes (sharp corners, solid colored title bar with white X, 1px navy border,
  offset drop-shadow on buttons).

### Color tokens (`NostriviaColors`)

| Token | Hex | Use |
|---|---|---|
| White | `#FFFFFF` | option buttons, card fills |
| SkyBlue | `#A9C4F0` | screen background |
| MintGreen | `#A8E0C2` | correct answer, Play Again, "Excellent!" |
| Pink | `#DDA8D2` | Play / Submit(enabled) / Results button |
| Coral | `#E2917F` | wrong answer, WARNING title bar, Leave |
| Periwinkle | `#6F7FE0` | selected option, dialog title bars, "1/10" badge |
| Navy | `#26233B` | all text and outlines |
| LavenderGray | `#D0CBDD` | disabled Submit |
| Gold | `#EFC583` | timer pill, ornament |

## 3. Architecture (Hybrid)

Native, editable Unity **prefabs + scene** are the deliverable (this is a handoff repo). All
**behavior** — navigation, transitions, state, audio — lives in focused runtime `MonoBehaviour`
scripts that the prefabs reference. Visuals are inspector-editable; logic is reviewable and
testable.

### Folder layout (under `Assets/Nostrivia/`)

```
Scenes/Nostrivia.unity            single scene holding everything
Prefabs/Screens/                  Home, Gameplay
Prefabs/Overlays/                 Settings, Results, Leave
Prefabs/Components/               AnswerOption, button, dialog-frame, timer-pill, background
Fonts/TMP/                        generated TMP font assets + material presets
Scripts/Core/                     ScreenManager, UITween, NostriviaColors, ScreenId
Scripts/Screens/                  HomeScreen, GameplayScreen
Scripts/Overlays/                 SettingsOverlay, ResultsOverlay, LeaveOverlay
Scripts/Components/               AnswerOption, Timer, etc.
Editor/                           one-time editor utilities (e.g. TMP font generation)
Audio/                            AudioMixer with SFX + Music groups (exposed volume params)
```

- **Canvas:** one Screen Space – Overlay canvas. `CanvasScaler` = Scale With Screen Size,
  reference **360 × 640**, with match/anchoring chosen to satisfy the responsiveness rule.
- **Sprites:** imported as Sprite (2D and UI). White glyphs are tinted per use (usually Navy).
- **Background grid:** not provided — interim flat `#A9C4F0` Image; real grid to come from the
  designer (see OpenQuestions). No procedurally generated background.

## 4. Screens & navigation

`ScreenManager` (one scene singleton) owns all navigation and models XD's two concepts:

- **Screens** (full-screen, mutually exclusive): **Home**, **Gameplay**. Switching = a transition.
- **Overlays** (on top of the current screen, each with a backdrop): **Settings**, **Results**,
  **Leave**. All overlays include a full-screen backdrop element.

Each screen/overlay is a **prefab with a controller** exposing intent events (e.g.
`HomeScreen.PlayClicked`); the `ScreenManager` subscribes and drives navigation. Controllers do
not reference each other.

### Stage model

The active screen **and any overlays on top of it** live inside a full-screen **Stage**
container. Overlays animate *within* the current stage, so they move *with* their screen — this
is what lets the outgoing screen and its overlay slide out together during the Results→ transitions.

### Navigation graph

| From | Trigger | Action | To |
|---|---|---|---|
| Home | Gear | Overlay (slide up) | Settings |
| Home | Play | Transition (Gameplay slides up) | Gameplay |
| Settings | X | Overlay slide down out | Home |
| Gameplay | X | Overlay (slide up) | Leave |
| Gameplay | Submit | in-place state change | — |
| Gameplay | Results | Overlay (slide up from below) | Results |
| Results | Play Again | Dual-slide screen swap | fresh Gameplay |
| Results | Back to Home | Dual-slide screen swap | Home |
| Leave | Cancel | Overlay slide down out | Gameplay (resume) |
| Leave | Leave | Dual-slide screen swap | Home |

## 5. Transition system

**`UITween`** — dependency-free static helper. Coroutine lerps for `RectTransform.anchoredPosition`,
`CanvasGroup.alpha` (where needed), and scale; each takes duration + easing
(Linear / EaseOutCubic / EaseInOutCubic) and optional `onComplete`. Handles are cancelable so a
new transition interrupts the previous one cleanly. All transitions build from this primitive.

**Transition catalogue:**

1. **Overlay slide-up in** (Home→Settings, Gameplay→Results, Gameplay→Leave): the whole overlay
   (backdrop + window) slides up from below as one unit. **No alpha fade** — backdrops slide,
   they don't fade.
2. **Overlay slide-down out** (Settings X→Home, Leave Cancel→Gameplay): reverse, then deactivate.
3. **Dual-slide screen swap** (Results→Play Again, Results→Back to Home, Leave→Leave): build a new
   stage with the incoming screen positioned above the viewport; animate the old stage
   (screen + overlay) down and the new stage in simultaneously; destroy the old stage on
   completion.
4. **Simple screen transition** (Home→Play→Gameplay): stage transition with Gameplay sliding up;
   exact behavior (whether Home holds) read from the XD spec.

**Timing & easing:** read from the XD interaction spec per transition; the `.mp4` and interactive
browser preview complement. Nothing hardcoded by guess — unknowns go to OpenQuestions and are
resolved by browser test / spec / asking. `ffmpeg` is available locally if frame extraction is
needed (requires a session restart for PATH; ask Omar before use).

## 6. Per-screen behavior

Logic is intentionally thin (focus is UI + transitions).

- **Home** — Play and Gear fire navigation events; otherwise static.
- **GameplayScreen** — the only stateful screen. State machine `Idle → Selected → Revealed`:
  - Select an option → highlight periwinkle (`#6F7FE0`, white text), enable Submit
    (gray `#D0CBDD` → pink `#DDA8D2`). Submit is blocked with no selection.
  - Submit → reveal: chosen option recolors **green if correct / coral if wrong** (single
    hardcoded question; correct answer = Internet Explorer). Submit button morphs into the
    **Results** button.
  - **Timer:** counts down from 30s; at ≤10s the pill background blinks yellow↔red. Exact blink
    hexes and 0-behavior are open questions (treat 0 as "just stops" until told otherwise).
  - X → opens Leave overlay.
- **SettingsOverlay** — SFX & Music sliders drive `AudioMixer` exposed volume params and update
  the `%` labels. Notification toggles and difficulty radios animate visually but are
  **non-functional** by design. X closes.
- **ResultsOverlay** — fully hardcoded ("Excellent! / You got 9 answers out of 10"); Play Again
  and Back to Home fire navigation.
- **LeaveOverlay** — Cancel resumes Gameplay; Leave navigates Home.

Known non-functional (by design): "1 / 10" counter, "✓ 5" badge, notification toggles, difficulty
selector, Results content.

## 7. Testing & verification

- **EditMode unit tests** (run via the Unity bridge `run-tests`): `UITween` easing/lerp math; the
  Gameplay state machine (selection gating, reveal correctness); slider→mixer value mapping.
- **Visual verification** per screen: build via MCP → capture with `Unity_Camera_Capture` /
  `Unity_SceneView_Capture2DScene` → compare against the XD PNG/spec → iterate.
- **Transition verification:** enter Play mode (`Unity_ManageEditor` Play), trigger the
  transition, capture frames, compare against the XD prototype playback.

## 8. Tooling

- **Unity MCP** (`mcp__unity-mcp__*`): `ManageGameObject` (build hierarchy/prefabs/components),
  `ManageScene`, `ManageAsset`, `ManageMenuItem`, `ManageEditor` (Play/Pause/Stop/state),
  `RunCommand` (arbitrary editor C#), capture tools, `ReadConsole`/`GetConsoleLogs`,
  `ValidateScript`, `PackageManager`.
- **Unity bridge** (`unity-bridge` CLI, `com.mxr.claude-bridge` installed): `compile`,
  `run-tests`, `get-console-logs`, `get-status`, `refresh`, `play/pause/step`.
- **Browser** (Playwright MCP): the XD prototype for specs, annotations, and transition preview.
- Unity 6000.3.16f1, running.

## 9. Build sequence

1. **Foundation + Home vertical slice** — folders, scene, CanvasScaler, TMP fonts, color tokens,
   shared building blocks (background placeholder, ornaments, one button style), and a fully
   finished **Home** screen including its Settings slide-in transition. Proves the whole
   build→capture→compare loop end-to-end.
2. Then **screen by screen**: Settings → Gameplay (+ states/timer) → Results → Leave, finishing
   layout + transitions for each, wiring navigation as screens come online.

## 10. Open questions

Tracked and maintained in `Assets/Nostrivia/_DesignRef/OpenQuestions.md` (missing background grid;
per-transition durations/easing; Results fade-vs-slide; Play→Gameplay Home behavior; timer 0
behavior and blink hexes; answer-reveal highlighting). Resolved via browser preview + XD spec +
asking the designer.
