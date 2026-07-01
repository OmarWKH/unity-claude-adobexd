using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nostrivia
{
    /// <summary>
    /// Controller for the Results ("GAME OVER") overlay. All content is
    /// hardcoded per the design; this component only wires the two action
    /// buttons to events the scene/screen manager can subscribe to.
    /// </summary>
    public class ResultsOverlay : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] Button playAgainButton;
        [SerializeField] Button backToHomeButton;

        /// <summary>Raised when the "Play Again" button is tapped.</summary>
        public event Action PlayAgainClicked;
        /// <summary>Raised when the "Back to Home" button is tapped.</summary>
        public event Action BackToHomeClicked;

        void OnEnable()
        {
            if (playAgainButton != null) playAgainButton.onClick.AddListener(RaisePlayAgain);
            if (backToHomeButton != null) backToHomeButton.onClick.AddListener(RaiseBackToHome);
        }

        void OnDisable()
        {
            if (playAgainButton != null) playAgainButton.onClick.RemoveListener(RaisePlayAgain);
            if (backToHomeButton != null) backToHomeButton.onClick.RemoveListener(RaiseBackToHome);
        }

        void RaisePlayAgain() => PlayAgainClicked?.Invoke();
        void RaiseBackToHome() => BackToHomeClicked?.Invoke();
    }
}
