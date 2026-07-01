using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nostrivia
{
    /// <summary>
    /// Controller for the Home screen. Exposes intent events; the ScreenManager
    /// subscribes and drives navigation. Knows nothing about other screens.
    /// </summary>
    public class HomeScreen : MonoBehaviour
    {
        [SerializeField] Button playButton;
        [SerializeField] Button settingsButton;

        /// <summary>Raised when the player taps Play.</summary>
        public event Action PlayClicked;
        /// <summary>Raised when the player taps the settings gear.</summary>
        public event Action SettingsClicked;

        void OnEnable()
        {
            if (playButton != null) playButton.onClick.AddListener(RaisePlay);
            if (settingsButton != null) settingsButton.onClick.AddListener(RaiseSettings);
        }

        void OnDisable()
        {
            if (playButton != null) playButton.onClick.RemoveListener(RaisePlay);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(RaiseSettings);
        }

        void RaisePlay() => PlayClicked?.Invoke();
        void RaiseSettings() => SettingsClicked?.Invoke();
    }
}
