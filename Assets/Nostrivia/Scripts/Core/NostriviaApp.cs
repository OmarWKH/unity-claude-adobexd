using UnityEngine;

namespace Nostrivia
{
    /// <summary>
    /// Scene composition root. Registers screens/overlays with the ScreenManager
    /// and wires the controllers' C# intent events to navigation actions.
    /// (C# events can't be wired in the inspector, so this is done in code.)
    /// </summary>
    public class NostriviaApp : MonoBehaviour
    {
        [SerializeField] ScreenManager screenManager;

        [Header("Screens")]
        [SerializeField] HomeScreen home;
        [Tooltip("The INITIAL gameplay instance. Registered at Start; read only in Start. After a " +
                 "Play Again this instance is destroyed and replaced — the ScreenManager registry, " +
                 "not this field, is the source of truth thereafter, so do not read it post-Start.")]
        [SerializeField] GameplayScreen gameplay;
        [Tooltip("Prefab used to spawn a fresh gameplay instance for the Results -> Play Again dual-slide.")]
        [SerializeField] GameObject gameplayPrefab;
        [Tooltip("Parent (the Canvas RectTransform) that freshly-spawned screens are placed under.")]
        [SerializeField] RectTransform screenParent;

        [Header("Overlays")]
        [SerializeField] SettingsOverlay settings;
        [SerializeField] ResultsOverlay results;
        [SerializeField] LeaveOverlay leave;

        void Start()
        {
            if (screenManager == null)
            {
                Debug.LogError("[NostriviaApp] screenManager is not assigned; navigation will not work.");
                return;
            }

            // Register with the manager.
            if (home != null) screenManager.RegisterScreen(ScreenId.Home, home.gameObject);
            if (gameplay != null) screenManager.RegisterScreen(ScreenId.Gameplay, gameplay.gameObject);
            if (settings != null) screenManager.RegisterOverlay(OverlayId.Settings, settings.gameObject);
            if (results != null) screenManager.RegisterOverlay(OverlayId.Results, results.gameObject);
            if (leave != null) screenManager.RegisterOverlay(OverlayId.Leave, leave.gameObject);

            // Home.
            if (home != null)
            {
                home.SettingsClicked += () => screenManager.ShowOverlay(OverlayId.Settings);
                home.PlayClicked += () => screenManager.TransitionToScreen(ScreenId.Gameplay, TransitionKind.SlideUp);
            }
            if (settings != null)
                settings.CloseClicked += () => screenManager.HideOverlay(OverlayId.Settings);

            // Gameplay (the registered instance; fresh ones are wired in PlayAgain).
            if (gameplay != null) WireGameplay(gameplay);

            // Results.
            if (results != null)
            {
                results.PlayAgainClicked += PlayAgain;
                results.BackToHomeClicked += () => screenManager.TransitionToScreen(ScreenId.Home, TransitionKind.DualSlide);
            }

            // Leave / Warning.
            if (leave != null)
            {
                leave.CancelClicked += () => screenManager.HideOverlay(OverlayId.Leave);
                leave.LeaveClicked += () => screenManager.TransitionToScreen(ScreenId.Home, TransitionKind.DualSlide);
            }

            // Home is the entry screen.
            if (home != null) screenManager.ShowScreen(ScreenId.Home);
        }

        /// <summary>Wires a gameplay screen's intent events to overlay navigation. Called for the registered instance and any fresh one.</summary>
        void WireGameplay(GameplayScreen gp)
        {
            gp.ResultsClicked += () => screenManager.ShowOverlay(OverlayId.Results);
            gp.ExitClicked += () => screenManager.ShowOverlay(OverlayId.Leave);
        }

        /// <summary>
        /// Results -> Play Again: spawn a FRESH gameplay window and dual-slide it
        /// in while the old gameplay + results popup slide out together. The fresh
        /// instance resets itself via its own OnEnable when activated.
        /// </summary>
        void PlayAgain()
        {
            if (gameplayPrefab == null || screenParent == null)
            {
                // Fallback if the prefab wasn't assigned: reuse the registered instance.
                screenManager.TransitionToScreen(ScreenId.Gameplay, TransitionKind.SlideUp);
                return;
            }
            var fresh = Instantiate(gameplayPrefab, screenParent);
            fresh.SetActive(false);
            var gp = fresh.GetComponent<GameplayScreen>();
            if (gp != null) WireGameplay(gp);
            screenManager.ReplaceCurrentScreenDualSlide(ScreenId.Gameplay, fresh);
        }
    }
}
