# Visual-Build Playbook (Nostrivia) — READ FULLY before building any prefab/screen

Repo root: `C:\Users\Aurelia Aurita\Offline\Random\gravity-explosion\unity\Raghad-UI-Handoff`.
Branch: `nostrivia-ui` (do NOT create/switch branches). Unity 6000.3.16f1 is running with a
Unity-MCP bridge. You build UI by executing C# in the editor via `Unity_RunCommand`, then verify
visually by capturing a portrait PNG and comparing it to the XD reference. Reference PNGs are in
`Assets/Nostrivia/_DesignRef/` (Home.png, Settings.png, Gameplay.png, Gameplay_Selection.png,
Gamepaly_Correct.png [sic], Gameplay_Wrong.png, Results.png, Leave.png, Overall.png).

## The one true way to build (do this, not ManageGameObject piece-by-piece)

Write ONE `Unity_RunCommand` whose `Code` is a `internal class CommandScript : IRunCommand` that
constructs the whole hierarchy in C# and saves a prefab. This is deterministic and token-light.

CRITICAL RunCommand gotchas (WILL bite you):
- The harness injects a namespace, making `Image` AMBIGUOUS (collides with Unity.AI.Image). ALWAYS
  alias UI types: `using UIImage = UnityEngine.UI.Image; using UIButton = UnityEngine.UI.Button;
  using UISlider = UnityEngine.UI.Slider;`. TMP types (`TextMeshProUGUI`, `TMP_FontAsset`) are fine
  via `using TMPro;`.
- Class MUST be named `CommandScript`, `internal`, implement `IRunCommand`, method `Execute(ExecutionResult result)`.
- Register changes: `result.RegisterObjectCreation(go)` after creating; `result.Log("...", primitiveArgs)`.
  Do NOT pass Type/ConstructorInfo/etc to result.Log — non-Unity objects can NRE the harness.
- Set component properties in C# directly (enums, Vector2, Color). Do NOT rely on ManageGameObject's
  component_properties map (it silently no-ops on typed enums).
- NEVER touch the Game View from RunCommand (GetWindow(GameView)/selectedSizeIndex/Repaint) → breaks
  the MCP harness. NEVER `AssetDatabase.DeleteAsset` a folder (prompts a modal → hangs). Delete files
  via the Bash `rm` tool instead.
- `GameObject.Find` and `FindFirstObjectByType` SKIP inactive objects. To find hidden ones use
  `Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)`.

## Helpers you can paste into your builder (proven)

```csharp
static Color C(string h){ ColorUtility.TryParseHtmlString(h, out var c); return c; }
// Color tokens: White #FFFFFF, SkyBlue #A9C4F0, MintGreen #A8E0C2, Pink #DDA8D2, Coral #E2917F,
//               Periwinkle #6F7FE0, Navy #26233B, LavenderGray #D0CBDD, Gold #EFC583. Text=Navy.
// TMP fonts: Assets/Nostrivia/Fonts/TMP/{BagelFatOne-Regular SDF | Nunito-Black SDF | Nunito-Bold SDF |
//   Nunito-SemiBold SDF | Nunito-ExtraBold SDF}.asset . Bagel = chunky display (logo/Play/Submit/
//   question/Excellent). Nunito = body/labels. Outline preset: "BagelFatOne SDF - Navy Outline.mat".
// Sprites (already Sprite type): Assets/Nostrivia/Sprites/{Logo,Ornament-Circle,Ornament-Star,
//   Ornament-Triangle,Gear,Clock,Check,Cross,Circle,Circle-Outline,Square,Square-Outline}.png
```
Reusable component prefabs to instantiate via
`(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path), parent)`:
- `Assets/Nostrivia/Prefabs/Components/Background.prefab` (sky-blue + 3 ornaments, full-screen stretch)
- `Assets/Nostrivia/Prefabs/Components/Y2KButton.prefab` (children: Shadow, Border, Fill[set color],
  Label[set text]; Button.targetGraphic=Fill). Resize the instance's RectTransform as needed.
- `Assets/Nostrivia/Prefabs/Components/DialogFrame.prefab` (children: Body>{TitleBar>{Title, CloseButton}, Content}).
  Set Title text + TitleBar Image color; add your content under Body/Content.

Wire private `[SerializeField]` refs after AddComponent:
```csharp
var so = new SerializedObject(component);
so.FindProperty("fieldName").objectReferenceValue = someUnityObjectOrComponent;
so.ApplyModifiedPropertiesWithoutUndo();
```
Save a prefab: `PrefabUtility.SaveAsPrefabAsset(rootGO, "Assets/Nostrivia/Prefabs/<Screens|Overlays|Components>/<Name>.prefab");`
(Create the folder first with `AssetDatabase.CreateFolder` if missing — CreateFolder is fine, only DeleteAsset prompts.)

## Capture → compare loop (portrait, reliable)

To SEE your UI: instantiate the prefab under the "Canvas" GameObject (parent), then run the capture
helper in `docs/superpowers/sdd/capture-helper.txt` VERBATIM as a `Unity_RunCommand` — it writes
`Temp/uicap.png` (540x960 portrait) and restores the canvas. Then `Read` the file
`<repoRoot>/Temp/uicap.png` (it's an image; the Read tool shows it). Compare to the XD reference PNG.
Iterate positions/sizes/colors until faithful. The canvas is 360x640 (center origin, y-up); pin
corner/edge elements to their corner anchor, center content.

## The committed scene is LIVE — do not clobber it

`Assets/Nostrivia/Scenes/Nostrivia.unity` already contains the composed app:
`Canvas > { HomeScreen (active), SettingsOverlay (inactive), <later screens/overlays> }`, plus
`EventSystem` and `App` (ScreenManager + NostriviaApp + AudioController). A PREFAB task must leave
this scene BYTE-IDENTICAL. Therefore:
- **NEVER `EditorSceneManager.SaveOpenScenes()` / `SaveScene()` in a prefab task.** (Only a scene-
  composition/wiring task saves — and the brief will say so explicitly.)
- **NEVER "remove all Canvas children."** That would delete HomeScreen/etc. Instead, keep a C#
  reference to each object YOU instantiate and `Object.DestroyImmediate` exactly those at the end.
- For a CLEAN capture (so Home isn't behind your prefab), hide the existing screen first and restore
  it after:
  ```csharp
  var home = GameObject.Find("HomeScreen"); if (home) home.SetActive(false);
  // ... instantiate your test objects under Canvas, capture ...
  // ... DestroyImmediate your own instances ...
  if (home) home.SetActive(true); // restore
  ```
  Do NOT save the scene afterward — the in-memory hide/restore leaves disk untouched.

## Finishing a task

1. Verify capture matches the reference. Then destroy ONLY your own instantiated test objects
   (tracked by reference) and restore anything you hid. Do NOT SaveOpenScenes (prefab task).
2. Check console: `Unity_ReadConsole` Types=["Error"] — must be empty (ignore any stale
   "Unity.Camera.Capture ... Instance ID" errors from earlier sessions).
3. Commit ONLY your task's files (`git status` to see; include Unity-generated `.meta`s and any
   new folder `.meta`). Message per your brief + trailer
   `Co-Authored-By: Claude Opus 4.8 (1M context) <noreply@anthropic.com>`.

## If you write a runtime C# script (controller/component)

Put runtime scripts in `Assets/Nostrivia/Scripts/<Screens|Overlays|Components|Core>/`, `namespace
Nostrivia`. After writing a `.cs`, compile via the bridge (NOT on PATH — module form, utf-8):
`PYTHONIOENCODING=utf-8 PYTHONUTF8=1 python -m claude_unity_bridge.cli refresh` then poll
`... get-status` until `Compilation: ✓ Ready` then `... compile`. A component must exist+compile
BEFORE a RunCommand can AddComponent it. If you run EditMode tests, SAVE THE SCENE FIRST
(SaveOpenScenes) or a modal hangs the runner; if run-tests times out at 30s, a modal is blocking —
report BLOCKED, don't loop.

## Report contract

Write your full report (what you built, values used, the reference you compared to, console-clean
confirmation, commit hash) to the report file named in your dispatch. Return only: status
(DONE/DONE_WITH_CONCERNS/BLOCKED/NEEDS_CONTEXT), commit hash, one-line verify summary, concerns.
If you hit a harness break (UNEXPECTED_ERROR) or a modal hang you can't clear, STOP and report
BLOCKED with the exact command — do not retry in a loop.
