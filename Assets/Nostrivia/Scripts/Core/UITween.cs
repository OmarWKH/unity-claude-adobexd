using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nostrivia {
  /// <summary>
  /// Dependency-free coroutine tweening primitive for UI transitions.
  /// Runs on a hidden persistent runner MonoBehaviour so callers don't need
  /// a MonoBehaviour of their own to host the coroutine. Starting a new
  /// tween on the same target automatically cancels any prior tween on
  /// that target.
  /// </summary>
  public static class UITween {
    class Runner : MonoBehaviour { }

    static Runner _runner;
    static readonly Dictionary<object, Coroutine> _active = new Dictionary<object, Coroutine>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init() {
      _active.Clear();
      var go = new GameObject("UITweenRunner");
      go.hideFlags = HideFlags.HideInHierarchy;
      UnityEngine.Object.DontDestroyOnLoad(go);
      _runner = go.AddComponent<Runner>();
    }

    /// <summary>Cancels the tween identified by the given coroutine handle, if it is still running,
    /// and removes its bookkeeping entry from the active-tween table.</summary>
    public static void Kill(Coroutine handle) {
      if (handle == null || _runner == null) return;
      _runner.StopCoroutine(handle);

      object key = null;
      foreach (var kv in _active) {
        if (kv.Value == handle) {
          key = kv.Key;
          break;
        }
      }
      if (key != null) _active.Remove(key);
    }

    static void CancelExisting(object target) {
      if (target == null) return;
      if (_active.TryGetValue(target, out var existing) && existing != null) {
        if (_runner != null) _runner.StopCoroutine(existing);
      }
      _active.Remove(target);
    }

    static Coroutine Run(object target, IEnumerator routine) {
      CancelExisting(target);
      var handle = _runner.StartCoroutine(routine);
      _active[target] = handle;
      return handle;
    }

    public static Coroutine MoveAnchored(RectTransform rt, Vector2 to, float dur, Func<float, float> ease, Action onDone = null) {
      if (rt == null) return null;
      if (_runner == null) Init();

      if (dur <= 0f) {
        rt.anchoredPosition = to;
        _active.Remove(rt);
        onDone?.Invoke();
        return null;
      }

      return Run(rt, MoveAnchoredRoutine(rt, to, dur, ease, onDone));
    }

    static IEnumerator MoveAnchoredRoutine(RectTransform rt, Vector2 to, float dur, Func<float, float> ease, Action onDone) {
      Vector2 from = rt.anchoredPosition;
      float t = 0f;
      while (t < dur) {
        if (rt == null) {
          _active.Remove(rt);
          yield break;
        }
        t = Mathf.Min(t + Time.unscaledDeltaTime, dur);
        float p = Mathf.Clamp01(t / dur);
        p = ease != null ? ease(p) : p;
        rt.anchoredPosition = Vector2.LerpUnclamped(from, to, p);
        yield return null;
      }
      rt.anchoredPosition = to;
      _active.Remove(rt);
      onDone?.Invoke();
    }

    public static Coroutine Fade(CanvasGroup cg, float toAlpha, float dur, Func<float, float> ease, Action onDone = null) {
      if (cg == null) return null;
      if (_runner == null) Init();

      if (dur <= 0f) {
        cg.alpha = toAlpha;
        _active.Remove(cg);
        onDone?.Invoke();
        return null;
      }

      return Run(cg, FadeRoutine(cg, toAlpha, dur, ease, onDone));
    }

    static IEnumerator FadeRoutine(CanvasGroup cg, float toAlpha, float dur, Func<float, float> ease, Action onDone) {
      float from = cg.alpha;
      float t = 0f;
      while (t < dur) {
        if (cg == null) {
          _active.Remove(cg);
          yield break;
        }
        t = Mathf.Min(t + Time.unscaledDeltaTime, dur);
        float p = Mathf.Clamp01(t / dur);
        p = ease != null ? ease(p) : p;
        cg.alpha = Mathf.LerpUnclamped(from, toAlpha, p);
        yield return null;
      }
      cg.alpha = toAlpha;
      _active.Remove(cg);
      onDone?.Invoke();
    }
  }
}
