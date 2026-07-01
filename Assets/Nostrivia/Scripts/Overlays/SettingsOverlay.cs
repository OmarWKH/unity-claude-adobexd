using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nostrivia
{
    /// <summary>
    /// Controller for the Settings overlay. Sliders drive audio (via events the
    /// scene wires to the mixer) and update their percent labels. Notification
    /// toggles and difficulty radios are visual-only per the design.
    /// </summary>
    public class SettingsOverlay : MonoBehaviour
    {
        [Header("Close")]
        [SerializeField] Button closeButton;

        [Header("Sound sliders")]
        [SerializeField] Slider sfxSlider;
        [SerializeField] TMPro.TMP_Text sfxLabel;
        [SerializeField] Slider musicSlider;
        [SerializeField] TMPro.TMP_Text musicLabel;

        /// <summary>Raised when the close (X) button is tapped.</summary>
        public event Action CloseClicked;
        /// <summary>Raised with the new SFX volume in percent [0..100].</summary>
        public event Action<float> SfxChanged;
        /// <summary>Raised with the new Music volume in percent [0..100].</summary>
        public event Action<float> MusicChanged;

        void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(RaiseClose);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfx);
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusic);
            // Reflect current slider values into the labels on show.
            if (sfxSlider != null) OnSfx(sfxSlider.value);
            if (musicSlider != null) OnMusic(musicSlider.value);
        }

        void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(RaiseClose);
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfx);
            if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusic);
        }

        void OnSfx(float v)
        {
            if (sfxLabel != null) sfxLabel.text = Mathf.RoundToInt(v) + "%";
            SfxChanged?.Invoke(v);
        }

        void OnMusic(float v)
        {
            if (musicLabel != null) musicLabel.text = Mathf.RoundToInt(v) + "%";
            MusicChanged?.Invoke(v);
        }

        void RaiseClose() => CloseClicked?.Invoke();
    }
}
