using NUnit.Framework; using Nostrivia;
public class EasingTests {
  [Test] public void Linear_endpoints(){ Assert.AreEqual(0f, Easing.Linear(0f), 1e-4f); Assert.AreEqual(1f, Easing.Linear(1f), 1e-4f); }
  [Test] public void EaseOutCubic_midpoint_is_fast_start(){ Assert.Greater(Easing.EaseOutCubic(0.5f), 0.5f); }
  [Test] public void EaseOutCubic_endpoints(){ Assert.AreEqual(0f, Easing.EaseOutCubic(0f),1e-4f); Assert.AreEqual(1f, Easing.EaseOutCubic(1f),1e-4f); }
  [Test] public void EaseInOutCubic_symmetric(){ Assert.AreEqual(0.5f, Easing.EaseInOutCubic(0.5f),1e-4f); }
  [Test] public void Clamps_out_of_range(){ Assert.AreEqual(1f, Easing.EaseOutCubic(2f),1e-4f); Assert.AreEqual(0f, Easing.EaseOutCubic(-1f),1e-4f); }
}
