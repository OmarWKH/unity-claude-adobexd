using UnityEngine;
namespace Nostrivia {
  public static class Easing {
    public static float Linear(float t){ return Mathf.Clamp01(t); }
    public static float EaseOutCubic(float t){ t = Mathf.Clamp01(t); float u = 1f - t; return 1f - u*u*u; }
    public static float EaseInOutCubic(float t){ t = Mathf.Clamp01(t);
      return t < 0.5f ? 4f*t*t*t : 1f - Mathf.Pow(-2f*t + 2f, 3f)/2f; }
  }
}
