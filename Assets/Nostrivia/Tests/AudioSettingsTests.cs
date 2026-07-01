using NUnit.Framework;
using Nostrivia;

public class AudioSettingsTests
{
    [Test] public void Zero_is_muted() => Assert.AreEqual(-80f, AudioSettings.PercentToDecibels(0f), 0.01f);
    [Test] public void Full_is_zero_dB() => Assert.AreEqual(0f, AudioSettings.PercentToDecibels(100f), 0.01f);
    [Test] public void Half_is_negative() => Assert.Less(AudioSettings.PercentToDecibels(50f), 0f);
    [Test] public void Monotonic() => Assert.Less(AudioSettings.PercentToDecibels(25f), AudioSettings.PercentToDecibels(60f));
    [Test] public void Below_zero_clamps_to_muted() => Assert.AreEqual(-80f, AudioSettings.PercentToDecibels(-10f), 0.01f);
    [Test] public void Above_full_clamps_to_zero() => Assert.AreEqual(0f, AudioSettings.PercentToDecibels(150f), 0.01f);
}
