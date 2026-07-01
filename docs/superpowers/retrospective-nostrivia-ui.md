# Nostrivia UI — Workflow Retrospective

Lessons from implementing the Adobe XD prototype in Unity uGUI via Claude Code + the Unity-MCP
bridge. Written for the next person (or agent) doing similar design-to-Unity handoff work.

---

## 1. Communicating requirements (design intake)

**What worked well**
- **Three complementary sources beat any one.** Screen PNGs (what it looks like), a written
  `DesignRef.md` (flow + transitions + intent), and the **live XD prototype in a browser** (exact
  specs) together left almost nothing to guess. The XD **Specs mode** (`/specs/` URL) exposed exact
  color tokens, font sizes, and per-screen **Interactions** (Trigger/Action/Destination) — this is
  gold; use it.
- **"Ask for specs as you go" instead of a giant upfront dump.** Getting the artboard size + palette
  + flow first, then pulling per-element details when actually building each screen, avoided both
  analysis-paralysis and guessing.
- **Capture-compare is the faithfulness mechanism.** Every screen was verified by rendering it and
  diffing against the reference PNG by eye. Without a tight visual loop you're building blind.

**Friction / what to improve**
- **The XD share link is a JS app — `WebFetch` sees a blank shell.** You need a real browser
  (Playwright MCP) to read it. Worth establishing browser access on day one.
- **Some values don't extract cleanly from XD:** transition durations/easing and the display-text
  outline width had to be defaulted and flagged for confirmation. If the designer can state these
  explicitly (e.g. "settings sheet: 0.35 s ease-out"), that removes a whole class of open questions.
- **Missing/again-needed assets surfaced mid-build:** the background grid was never provided (still a
  flat-color placeholder), and the body font (Nunito) lacks a `✓` glyph so the "✓ 5" badge needed an
  icon fallback. Ask designers up front for: every non-photographic background, and any text using
  glyphs outside the chosen font.
- **Keep a running `OpenQuestions.md`** in the repo. It was invaluable — every deferred decision,
  missing asset, and "confirm vs XD" item lived there instead of getting lost.

## 2. Planning & process

**What worked**
- **Spec → plan before code paid for itself.** The architecture decision made during brainstorming —
  a "Stage" model where overlays are children of the current screen's container — made the hardest
  transition (old screen + popup slide out together) *fall out for free*. Good architecture up front
  beats fighting it later.
- **Build sequence mattered: foundation + one full vertical slice first.** Proving the entire
  pipeline (fonts, tokens, a screen, an overlay, a transition, audio) on the simplest screen before
  scaling caught the hard problems (see the capture loop, below) early and cheaply.
- **A milestone checkpoint** after the vertical slice was the right place to confirm direction with
  the human before building four more screens.

**What to improve**
- **The plan under-specified the "composition root."** Controllers expose **C# events** (great for
  decoupling), but C# events can't be wired in the Inspector — so a code-level `NostriviaApp` that
  registers everything and wires events was required and wasn't in the plan. When a design uses C#
  events, plan the wiring object explicitly.

## 3. Subagent-driven execution

**What worked**
- **Fresh subagent per task + independent review caught real bugs** that "it compiles + looks right"
  would have missed: two `UITween` lifecycle bugs (leak + destroyed-target NRE) and the final
  navigation re-entrancy. The adversarial review step is worth it.
- **Delegating visual builds to subagents kept the controller's context lean.** A single screen build
  burned 100k–190k tokens in the *subagent's* context (the capture/iterate loop is heavy); absorbing
  that away from the coordinator is the whole point.
- **A shared "playbook" file** (`.superpowers/sdd/visual-build-playbook.md`) that every visual
  subagent reads — build patterns, the capture helper, and the MCP foot-guns — gave consistent
  results without a giant per-dispatch prompt. Hand artifacts as files, not pasted text.
- **A durable progress ledger** survived an auth interruption *and* context compaction. When memory
  is unreliable, the ledger + `git log` are the source of truth.

**What to improve**
- **Playbook instructions must evolve with world state.** An early cleanup rule ("remove all Canvas
  children") was safe when the scene was empty but became *dangerous* once the scene held a live
  composition — a subagent nearly wiped it. Re-audit shared instructions whenever the environment
  they assume changes.
- **The single shared Unity editor forces serial execution** for anything touching it. Subagent
  builds had to run one at a time; only pure-logic (test) tasks and read-only reviews can overlap.

## 4. Unity + MCP workflow (the technical foot-guns) — the most reusable section

These cost real time to discover. If you're driving Unity through this MCP, read these first.

- **Screenshotting a Screen Space–Overlay canvas is not obvious.** The MCP capture tools render the
  *scene view* (landscape, with skybox) — useless for portrait UI. A camera capture can't see an
  Overlay canvas at all, and fails outright if the camera has a `targetTexture`. **Working approach:**
  a self-contained `RunCommand` that *transiently* flips the canvas to Screen Space–Camera with a
  throwaway camera + `RenderTexture`, renders portrait (e.g. 1080×1920), writes a PNG to `Temp/`,
  then restores Overlay — all in one command. Non-destructive, deterministic, keeps the deliverable
  as Overlay. (Saved as `.superpowers/sdd/capture-helper.txt`.)
- **Do NOT touch the Game View from `RunCommand`.** `GetWindow(GameView)`, setting its size, or
  `Repaint()` **crashes the MCP bridge** ("UNEXPECTED_ERROR: Object reference not set"). This is why
  the RenderTexture route above exists instead of just setting a portrait Game View resolution.
- **Do NOT create an `AudioMixerController` programmatically from `RunCommand`** — it triggers editor
  UI and crashes the bridge the same way. Some editor operations must stay manual; detect these and
  hand them to the human (the AudioMixer asset is a documented 30-second manual step).
- **`AssetDatabase.DeleteAsset` on a folder pops a confirmation modal that hangs the bridge.** Delete
  files via the shell (`rm`) instead. In general, any editor call that can show a modal will hang a
  non-interactive command.
- **Scene persistence is subtle and bit us hard.** `EditorSceneManager.SaveOpenScenes()` in a
  `RunCommand`, followed immediately by entering Play mode, **did not persist prefab-instance
  additions to disk** — three screen/overlay instances worked in Play mode for the whole build but
  were never in the committed scene (0 references). **Reliable pattern:**
  `MarkSceneDirty(scene)` → `SaveScene(scene)` → `AssetDatabase.SaveAssets()`, then **verify on disk**
  (grep the prefab's GUID in the `.unity` file) *before* entering Play mode, and **commit the scene
  before Play-testing**. Verify persistence, don't assume it.
- **The EditMode test runner pops a "Scene(s) Have Been Modified" modal if the scene is dirty**,
  which hangs `run-tests` (times out at 30 s/90 s). **Always save the scene before running tests.**
- **`RunCommand` C# quirks:** the injected namespace makes `Image` **ambiguous** with `Unity.AI.Image`
  — alias UI types (`using UIImage = UnityEngine.UI.Image;`). Don't pass non-Unity objects (Type,
  ConstructorInfo) to the result logger — it can NRE the harness. Set component properties in real C#
  (typed enums, `Vector2`, `Color`); the `ManageGameObject` property-map silently no-ops on enums.
- **`GameObject.Find` / `FindFirstObjectByType` skip inactive objects.** Parked/hidden UI needs
  `FindObjectsByType<T>(FindObjectsInactive.Include, ...)`.
- **The bridge CLI wasn't on PATH** — run it as a module with UTF-8 forced (Windows console chokes on
  the `✓` in its output): `PYTHONIOENCODING=utf-8 PYTHONUTF8=1 python -m claude_unity_bridge.cli …`.
- **Play-mode transition testing works** because tweens run in real time: trigger via
  `ScreenManager` or a `Button.onClick.Invoke()`, and the normal command round-trip latency
  (seconds) is longer than a 0.3 s tween, so the *end state* has settled by the time you capture. To
  catch a mid-transition frame you'd capture immediately (0 frames elapsed).

## 5. Implementation & architecture lessons

- **Build prefabs via one `RunCommand` C# builder** (create the hierarchy, set typed properties,
  `PrefabUtility.SaveAsPrefabAsset`, instantiate child prefabs with `PrefabUtility.InstantiatePrefab`,
  wire private `[SerializeField]` refs via `SerializedObject.FindProperty(...).objectReferenceValue`).
  This is far more token-efficient and deterministic than dozens of `ManageGameObject` calls that
  each echo huge JSON.
- **A tiny code tween utility + the Stage model handled every transition** with no external
  dependency (no DOTween). The one genuinely hard case — Results→Play-Again, where a *fresh* gameplay
  slides in as the *old* gameplay + results slide out — needed a dedicated "replace current screen
  with a fresh instance and swap the registry" method; a single reused instance can't be in two
  places at once.
- **Add a global "transition in progress" guard from the start.** Navigation re-entrancy (opening an
  overlay during a screen slide) can orphan UI into a stuck state. It was flagged early, deferred, and
  became the final review's one real finding. Cheap to add up front, annoying to retrofit.
- **Dynamic TMP fonts bake glyphs into their atlas on use**, so font `.asset` files show as
  "modified" after captures — expected, and fine to commit (pre-baked common glyphs).

## 6. If I did it again

1. **Stand up the capture rig and the save-verify-commit scene pattern before any visual work.** Both
   were discovered mid-stream at real cost. They're foundational infrastructure, not incidental.
2. **Verify scene composition persisted to disk immediately after every scene-mutating step** (grep
   the GUID). This would have caught the non-persisted instances at the moment they were added, not
   at final review.
3. **Extract exact transition timings/easing from XD up front** rather than defaulting and deferring.
4. **Front-load the "manual-only" list** (things that crash the bridge: Game View, AudioMixer,
   modal-popping asset ops) so they're planned as human steps from the beginning.

---

*Net:* the design-intake + spec/plan + subagent-with-review + capture-compare loop produced a
faithful, well-architected result. Nearly all the pain was in the Unity-MCP plumbing (capture,
persistence, bridge fragility) — now documented above and in the SDD scratch files so the next run
starts from a much better place.
