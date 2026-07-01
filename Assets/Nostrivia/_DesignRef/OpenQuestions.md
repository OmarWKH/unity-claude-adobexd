# Open Questions & Missing Assets — for the designer

Running list of things to confirm or obtain. Resolve items by noting the answer inline.
Primary way to resolve transition questions: interactively test the prototype in the browser
(XD preview mode) + read the XD interaction spec; ask the designer when still unclear.

## Implementation status (as of 2026-07-01)

**All 9 screens + all transitions are implemented and Play-verified in Unity** (scene:
`Assets/Nostrivia/Scenes/Nostrivia.unity` — press Play). Home, Settings, Gameplay (4 answer
states + live timer), Results, Leave, and every navigation transition (overlay slide-ups,
Home→Gameplay slide, and the Results→Play-Again / Back-to-Home / Leave→Home dual-slides) work.
17/17 EditMode logic tests pass. Faithful to the XD PNGs via capture-compare.

**Two things need YOU (can't be automated safely — they crash the Unity-MCP bridge):**
1. **AudioMixer asset** — see "Manual step needed" below.
2. Nothing else blocking.

**Items below are polish/confirmation, none blocking.** Highest value: the background grid asset,
and confirming transition timings + timer blink colors against XD.

## Missing assets

- **Background grid** — The sky-blue grid background (`#A9C4F0` with white grid lines) seen on
  Home / Gameplay is not in `Sprites/`. Need one of: (a) export of the background from XD, or
  (b) a tileable grid-cell sprite plus exact cell size, line width, and line color. Interim:
  flat `#A9C4F0` color Image placeholder (no grid lines).

## Transitions — implemented with default timings (CONFIRM vs XD)

All transitions are implemented and Play-verified. Timings are serialized fields on the `App`
object's `ScreenManager` component, easily tunable: **overlay slide 0.35s, screen transition 0.30s,
EaseOutCubic**. These are reasonable defaults chosen for feel — confirm/adjust against the XD
interaction spec (durations + easing) when convenient. The video `Results_To_Gameplay.mp4` can
complement.

- **Durations & easing** for every transition — read from XD interaction spec; video complements.
- **Results overlay in** — doc says "fades in from below," but backdrops slide without fading
  (per Omar). Confirm whether Results has any alpha fade or is a pure slide-up.
- **Home → Gameplay (Play)** — stage transition, Gameplay slides up. Confirm whether Home holds
  in place or also moves.
- **Gameplay → Leave (X)** — confirm it's an overlay slide-up like the others.

## Implementation follow-ups (self-resolve via capture-compare)

- **Display-text outline width** — `BagelFatOne SDF - Navy Outline.mat` created with a
  placeholder `_OutlineWidth = 0.2`. Tune to match XD's question text + "Excellent!" stroke
  during the Gameplay screen build (plan Task 17) via capture→compare. Face color is set
  per-usage (white for the question, mint-green for "Excellent!").

## Manual step needed (automation blocked)

- **AudioMixer asset** — `NostriviaMixer.mixer` with **SFX** and **Music** groups, each exposing
  its volume as `SFXVolume` / `MusicVolume`, then assign it to the `AudioController.mixer` field on
  the scene's `App` object. Must be done by hand (Assets > Create > Audio Mixer): creating an
  AudioMixerController programmatically crashes the Unity-MCP bridge. All the code wiring + the
  tested dB mapping are already in place; only the asset + one field assignment remain. No audible
  effect anyway (the prototype has no audio clips).

## Minor fidelity gaps (implementation)

- **Settings toggles** are rectangular; the XD design shows rounded "pill" toggles. No rounded
  sprite was provided — a true pill needs a rounded sprite (from designer) or a rounded-rect
  shader/sprite. Currently a plain rect Image. Low priority; flag to designer if pill shape matters.
- **Home logo** sits slightly higher than Home.png — minor vertical tuning candidate.

## Behavior to confirm

- **Timer at 0** — countdown starts at 30s, blinks between yellow and red at ≤10s remaining.
  What happens when it hits 0? (auto-submit / time-out screen / just stops?) Unspecified.
- **Timer blink colors** — exact yellow and red hex. Gold token `#EFC583`; red likely coral
  `#E2917F` — confirm.
- **Answer reveal** — when submitted, does ONLY the selected option recolor (green if right /
  coral if wrong), or is the correct option always highlighted too? (PNGs show only the
  selected option changing.)

## Known non-functional (per spec, no action needed)

- "1 / 10" question counter, the "✓ 5" badge, Notifications toggles, Difficulty selector,
  and Results content (always "Excellent! / 9 out of 10") are all hardcoded / non-functional.
