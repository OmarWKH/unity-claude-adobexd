using UnityEngine;

namespace Nostrivia
{
    /// <summary>
    /// Scene composition root. Registers screens/overlays with the ScreenManager
    /// and wires the controllers' C# intent events to navigation actions.
    /// (C# events can't be wired in the inspector, so this is done in code.)
    /// Grows as more screens come online.
    /// </summary>
    public class NostriviaApp : MonoBehaviour
    {
        [SerializeField] ScreenManager screenManager;

        [Header("Screens")]
        [SerializeField] HomeScreen home;
        [SerializeField] GameplayScreen gameplay;

        [Header("Overlays")]
        [SerializeField] SettingsOverlay settings;

        void Start()
        {
            // Register with the manager.
            if (home != null) screenManager.RegisterScreen(ScreenId.Home, home.gameObject);
            if (gameplay != null) screenManager.RegisterScreen(ScreenId.Gameplay, gameplay.gameObject);
            if (settings != null) screenManager.RegisterOverlay(OverlayId.Settings, settings.gameObject);

            // Wire navigation.
            if (home != null)
            {
                home.SettingsClicked += () => screenManager.ShowOverlay(OverlayId.Settings);
                home.PlayClicked += () => screenManager.TransitionToScreen(ScreenId.Gameplay, TransitionKind.SlideUp);
            }
            if (settings != null)
                settings.CloseClicked += () => screenManager.HideOverlay(OverlayId.Settings);

            // Home is the entry screen.
            if (home != null) screenManager.ShowScreen(ScreenId.Home);
        }
    }
}
