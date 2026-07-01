using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nostrivia {
  /// <summary>
  /// Scene singleton that owns navigation between full-screen Screens and the
  /// Overlays that appear on top of them.
  ///
  /// The core model is the "Stage": a full-screen RectTransform container that
  /// holds the currently active screen plus any overlays currently shown on
  /// top of it. Overlays are reparented into the current Stage when shown, so
  /// they always slide as children of it. That single fact is what makes
  /// DualSlide transitions "just work": when the old Stage is animated off
  /// screen, every overlay riding inside it goes along for the ride with no
  /// special-casing.
  ///
  /// Registered screen/overlay GameObjects are never destroyed by this class —
  /// they are reparented between a neutral pool (directly under the canvas)
  /// and whichever Stage is currently displaying them. Only the transient
  /// Stage *container* objects are created/destroyed as transitions happen,
  /// so a Screen or Overlay registered once via RegisterScreen/RegisterOverlay
  /// can be shown again any number of times later.
  /// </summary>
  public class ScreenManager : MonoBehaviour {
    /// <summary>Easing curve choices exposed in the inspector; mapped to <see cref="Easing"/> methods.</summary>
    public enum EaseKind {
      Linear,
      EaseOutCubic,
      EaseInOutCubic
    }

    public static ScreenManager Instance { get; private set; }

    [Header("Canvas")]
    [Tooltip("RectTransform that Stages are parented under. Defaults to the nearest parent Canvas if left empty.")]
    [SerializeField] RectTransform canvasRect;
    [Tooltip("Used only if no Canvas RectTransform can be found at runtime.")]
    [SerializeField] float fallbackCanvasHeight = 640f;

    [Header("Overlay transition")]
    [SerializeField] float overlaySlideDuration = 0.3f;
    [SerializeField] EaseKind overlaySlideEase = EaseKind.EaseOutCubic;

    [Header("Screen transition")]
    [SerializeField] float screenTransitionDuration = 0.3f;
    [SerializeField] EaseKind screenTransitionEase = EaseKind.EaseOutCubic;

    readonly Dictionary<ScreenId, GameObject> _screens = new Dictionary<ScreenId, GameObject>();
    readonly Dictionary<OverlayId, GameObject> _overlays = new Dictionary<OverlayId, GameObject>();

    Stage _currentStage;

    /// <summary>A full-screen RectTransform holding one screen and zero or more overlays on top of it.</summary>
    class Stage {
      public RectTransform Root;
      public ScreenId? ScreenId;
      public GameObject ScreenRoot;
      public readonly HashSet<OverlayId> OverlayIds = new HashSet<OverlayId>();
    }

    /// <summary>Current canvas height in UI units, used as the off-screen slide distance (H).</summary>
    float CanvasHeight => canvasRect != null ? canvasRect.rect.height : fallbackCanvasHeight;

    void Awake() {
      if (Instance != null && Instance != this) {
        Destroy(gameObject);
        return;
      }
      Instance = this;

      if (canvasRect == null) {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null) canvasRect = canvas.transform as RectTransform;
      }
      if (canvasRect == null) {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null) canvasRect = canvas.transform as RectTransform;
      }

      _currentStage = CreateStage();
    }

    void OnDestroy() {
      if (Instance == this) Instance = null;
    }

    // ---------------------------------------------------------------
    // Registration
    // ---------------------------------------------------------------

    public void RegisterScreen(ScreenId id, GameObject root) {
      if (root == null) {
        Debug.LogError($"[ScreenManager] RegisterScreen({id}) called with a null root.");
        return;
      }
      if (!(root.transform is RectTransform)) {
        Debug.LogError($"[ScreenManager] RegisterScreen({id}): root has no RectTransform.");
        return;
      }
      _screens[id] = root;
      ParkInPool(root);
    }

    public void RegisterOverlay(OverlayId id, GameObject root) {
      if (root == null) {
        Debug.LogError($"[ScreenManager] RegisterOverlay({id}) called with a null root.");
        return;
      }
      if (!(root.transform is RectTransform)) {
        Debug.LogError($"[ScreenManager] RegisterOverlay({id}): root has no RectTransform.");
        return;
      }
      _overlays[id] = root;
      ParkInPool(root);
    }

    /// <summary>Deactivates and reparents an object into the neutral pool (directly under the canvas, outside any Stage).</summary>
    void ParkInPool(GameObject go) {
      go.SetActive(false);
      go.transform.SetParent(canvasRect, false);
    }

    // ---------------------------------------------------------------
    // Overlays
    // ---------------------------------------------------------------

    /// <summary>
    /// Activates the overlay under the current Stage and slides it up into view
    /// (whole overlay incl. backdrop moves as one unit — no alpha fade).
    /// </summary>
    public void ShowOverlay(OverlayId id) {
      if (!_overlays.TryGetValue(id, out var root)) {
        Debug.LogError($"[ScreenManager] ShowOverlay({id}): overlay not registered.");
        return;
      }
      if (_currentStage.OverlayIds.Contains(id)) return; // already shown

      var rt = (RectTransform)root.transform;
      rt.SetParent(_currentStage.Root, false);
      rt.SetAsLastSibling(); // overlays render above the screen
      rt.anchoredPosition = new Vector2(0f, -CanvasHeight);
      root.SetActive(true);
      _currentStage.OverlayIds.Add(id);

      UITween.MoveAnchored(rt, Vector2.zero, overlaySlideDuration, Resolve(overlaySlideEase));
    }

    /// <summary>Slides the overlay back down off-screen, then deactivates and parks it for reuse.</summary>
    public void HideOverlay(OverlayId id) {
      if (!_overlays.TryGetValue(id, out var root)) {
        Debug.LogError($"[ScreenManager] HideOverlay({id}): overlay not registered.");
        return;
      }
      if (!_currentStage.OverlayIds.Contains(id)) return; // not currently shown

      var rt = (RectTransform)root.transform;
      var stage = _currentStage;
      UITween.MoveAnchored(rt, new Vector2(0f, -CanvasHeight), overlaySlideDuration, Resolve(overlaySlideEase), () => {
        stage.OverlayIds.Remove(id);
        ParkInPool(root);
      });
    }

    // ---------------------------------------------------------------
    // Screen transitions
    // ---------------------------------------------------------------

    public void TransitionToScreen(ScreenId id, TransitionKind kind) {
      if (!_screens.TryGetValue(id, out var incoming)) {
        Debug.LogError($"[ScreenManager] TransitionToScreen({id}): screen not registered.");
        return;
      }

      switch (kind) {
        case TransitionKind.SlideUp:
          DoSlideUp(id, incoming);
          break;
        case TransitionKind.DualSlide:
          DoDualSlide(id, incoming);
          break;
      }
    }

    /// <summary>Incoming screen slides up from below into the current Stage, covering (and then retiring) the outgoing screen.</summary>
    void DoSlideUp(ScreenId id, GameObject incoming) {
      var stage = _currentStage;
      var outgoingRoot = stage.ScreenRoot;
      var outgoingOverlays = new List<OverlayId>(stage.OverlayIds);

      var incomingRt = (RectTransform)incoming.transform;
      incomingRt.SetParent(stage.Root, false);
      incomingRt.SetAsLastSibling(); // render above the outgoing screen + its overlays while sliding up
      incomingRt.anchoredPosition = new Vector2(0f, -CanvasHeight);
      incoming.SetActive(true);

      UITween.MoveAnchored(incomingRt, Vector2.zero, screenTransitionDuration, Resolve(screenTransitionEase), () => {
        if (outgoingRoot != null && outgoingRoot != incoming) ParkInPool(outgoingRoot);
        foreach (var oid in outgoingOverlays) {
          if (_overlays.TryGetValue(oid, out var overlayRoot)) ParkInPool(overlayRoot);
        }
        stage.OverlayIds.Clear();
        stage.ScreenId = id;
        stage.ScreenRoot = incoming;
        incomingRt.SetAsFirstSibling(); // back to the normal bottom-of-stage position for future overlays
      });
    }

    /// <summary>
    /// Creates a new Stage above the viewport holding the incoming screen, then
    /// simultaneously slides the old Stage (screen + its overlays) down and off
    /// while the new Stage slides down into place. The old Stage is torn down
    /// once its slide-out completes.
    /// </summary>
    void DoDualSlide(ScreenId id, GameObject incoming) {
      float h = CanvasHeight;
      var oldStage = _currentStage;

      var newStage = CreateStage();
      var incomingRt = (RectTransform)incoming.transform;
      incomingRt.SetParent(newStage.Root, false);
      incomingRt.anchoredPosition = Vector2.zero;
      incoming.SetActive(true);
      newStage.ScreenId = id;
      newStage.ScreenRoot = incoming;
      newStage.Root.anchoredPosition = new Vector2(0f, h);

      // Future ShowOverlay/HideOverlay calls should target the new Stage immediately.
      _currentStage = newStage;

      UITween.MoveAnchored(oldStage.Root, new Vector2(0f, -h), screenTransitionDuration, Resolve(screenTransitionEase), () => {
        RetireStage(oldStage);
      });
      UITween.MoveAnchored(newStage.Root, Vector2.zero, screenTransitionDuration, Resolve(screenTransitionEase));
    }

    // ---------------------------------------------------------------
    // Stage plumbing
    // ---------------------------------------------------------------

    Stage CreateStage() {
      var go = new GameObject("Stage", typeof(RectTransform));
      var rt = (RectTransform)go.transform;
      rt.SetParent(canvasRect, false);
      rt.anchorMin = Vector2.zero;
      rt.anchorMax = Vector2.one;
      rt.pivot = new Vector2(0.5f, 0.5f);
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;
      rt.anchoredPosition = Vector2.zero;
      return new Stage { Root = rt };
    }

    /// <summary>
    /// Preserves the Stage's contents (screen + any overlays) by parking them
    /// back in the neutral pool, then destroys the now-empty Stage container.
    /// </summary>
    void RetireStage(Stage stage) {
      if (stage?.Root == null) return;
      for (int i = stage.Root.childCount - 1; i >= 0; i--) {
        ParkInPool(stage.Root.GetChild(i).gameObject);
      }
      Destroy(stage.Root.gameObject);
    }

    static Func<float, float> Resolve(EaseKind kind) {
      switch (kind) {
        case EaseKind.Linear: return Easing.Linear;
        case EaseKind.EaseInOutCubic: return Easing.EaseInOutCubic;
        default: return Easing.EaseOutCubic;
      }
    }
  }
}
