using UnityEngine;
using UnityEngine.Audio;

namespace Nostrivia
{
    /// <summary>
    /// Bridges the Settings sliders to the AudioMixer: each slider's percent
    /// value is converted (via <see cref="AudioSettings.PercentToDecibels"/>)
    /// and written to the mixer's exposed volume parameters.
    ///
    /// Null-safe: if no mixer is assigned (e.g. before the .mixer asset exists),
    /// the slider labels still update and the app runs — the writes are skipped.
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        [SerializeField] SettingsOverlay settings;
        [SerializeField] AudioMixer mixer;
        [SerializeField] string sfxParam = "SFXVolume";
        [SerializeField] string musicParam = "MusicVolume";

        void OnEnable()
        {
            if (settings == null) return;
            settings.SfxChanged += ApplySfx;
            settings.MusicChanged += ApplyMusic;
        }

        void OnDisable()
        {
            if (settings == null) return;
            settings.SfxChanged -= ApplySfx;
            settings.MusicChanged -= ApplyMusic;
        }

        void ApplySfx(float pct) => Apply(sfxParam, pct);
        void ApplyMusic(float pct) => Apply(musicParam, pct);

        void Apply(string param, float pct)
        {
            if (mixer == null || string.IsNullOrEmpty(param)) return;
            mixer.SetFloat(param, AudioSettings.PercentToDecibels(pct));
        }
    }
}
