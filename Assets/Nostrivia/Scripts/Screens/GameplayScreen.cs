using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nostrivia
{
    /// <summary>
    /// Controller for the Gameplay screen (question + 4 answers + timer + submit).
    /// Owns a <see cref="GameplayState"/> state machine and drives the visuals of the
    /// answer options and the submit/results button off it. Knows nothing about other
    /// screens; the ScreenManager subscribes to the intent events for navigation.
    /// </summary>
    public class GameplayScreen : MonoBehaviour
    {
        static readonly Color SubmitDisabledColor = NostriviaColors.LavenderGray;
        static readonly Color SubmitEnabledColor = NostriviaColors.Pink;
        const string SubmitLabelText = "Submit";
        const string ResultsLabelText = "Results";

        [SerializeField] CountdownTimer timer;
        [SerializeField] AnswerOption[] options;
        [SerializeField] Button submitButton;
        [SerializeField] Image submitFill;
        [SerializeField] TMP_Text submitLabel;
        [SerializeField] Button exitButton;

        GameplayState state;

        /// <summary>Raised when the player taps the exit (X) button.</summary>
        public event Action ExitClicked;
        /// <summary>Raised when the player taps the Results button (after reveal).</summary>
        public event Action ResultsClicked;

        void OnEnable()
        {
            state = new GameplayState();

            if (options != null)
            {
                foreach (var option in options)
                {
                    if (option == null) continue;
                    option.SetVisualState(OptionVisual.Default);
                    option.Clicked += OnOption;
                }
            }

            SetSubmitVisual(SubmitDisabledColor, SubmitLabelText);

            if (submitButton != null) submitButton.onClick.AddListener(OnSubmitOrResults);
            if (exitButton != null) exitButton.onClick.AddListener(RaiseExit);
            // Timer.Elapsed is intentionally NOT subscribed: per the design, reaching 0 just stops
            // the timer (no auto-advance). Wire timer.Elapsed here if time-out navigation is added.
            if (timer != null) timer.StartFrom(30f);
        }

        void OnDisable()
        {
            if (options != null)
            {
                foreach (var option in options)
                {
                    if (option == null) continue;
                    option.Clicked -= OnOption;
                }
            }

            if (submitButton != null) submitButton.onClick.RemoveListener(OnSubmitOrResults);
            if (exitButton != null) exitButton.onClick.RemoveListener(RaiseExit);
        }

        void OnOption(int i)
        {
            if (state.Phase == QuestionPhase.Revealed) return;

            state.Select(i);

            if (options != null)
            {
                for (int j = 0; j < options.Length; j++)
                {
                    if (options[j] == null) continue;
                    options[j].SetVisualState(j == i ? OptionVisual.Selected : OptionVisual.Default);
                }
            }

            SetSubmitVisual(SubmitEnabledColor, SubmitLabelText);
        }

        void OnSubmitOrResults()
        {
            if (state.Phase == QuestionPhase.Selected)
            {
                if (!state.Submit()) return;

                if (options != null && state.Selected.HasValue)
                {
                    var chosen = options[state.Selected.Value];
                    if (chosen != null)
                    {
                        chosen.SetVisualState(state.IsSelectedCorrect ? OptionVisual.Correct : OptionVisual.Wrong);
                    }
                }

                SetSubmitVisual(SubmitEnabledColor, ResultsLabelText);
            }
            else if (state.Phase == QuestionPhase.Revealed)
            {
                ResultsClicked?.Invoke();
            }
        }

        void SetSubmitVisual(Color fill, string label)
        {
            if (submitFill != null) submitFill.color = fill;
            if (submitLabel != null) submitLabel.text = label;
        }

        void RaiseExit() => ExitClicked?.Invoke();
    }
}
