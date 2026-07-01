using UnityEngine;

namespace Nostrivia
{
    /// <summary>
    /// Maps a 0..100 percent volume (from the Settings sliders) to a decibel
    /// value suitable for an AudioMixer exposed volume parameter.
    /// </summary>
    public static class AudioSettings
    {
        /// <summary>
        /// 0% → -80 dB (effectively muted), 100% → 0 dB, using a perceptual
        /// log curve in between (20*log10 of the normalized linear value).
        /// </summary>
        public static float PercentToDecibels(float pct)
        {
            float n = Mathf.Clamp01(pct / 100f);
            if (n <= 0.0001f) return -80f;
            return Mathf.Log10(n) * 20f;
        }
    }
}
