using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nostrivia
{
    /// <summary>
    /// Visual state of an AnswerOption button, mapped to design tokens by SetVisualState.
    /// </summary>
    public enum OptionVisual
    {
        Default,
        Selected,
        Correct,
        Wrong
    }

    /// <summary>
    /// A single answer button used in the Gameplay screen's answer list. Displays a label,
    /// recolors on state change (Default/Selected/Correct/Wrong), and raises Clicked(Index)
    /// when tapped.
    /// </summary>
    public class AnswerOption : MonoBehaviour
    {
        public int Index;

        public event Action<int> Clicked;

        [SerializeField] private Image fillImage;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
            }
        }

        private void HandleClicked()
        {
            Clicked?.Invoke(Index);
        }

        public void SetVisualState(OptionVisual v)
        {
            Color fill;
            Color text;
            switch (v)
            {
                case OptionVisual.Selected:
                    fill = NostriviaColors.Periwinkle;
                    text = NostriviaColors.White;
                    break;
                case OptionVisual.Correct:
                    fill = NostriviaColors.MintGreen;
                    text = NostriviaColors.Navy;
                    break;
                case OptionVisual.Wrong:
                    fill = NostriviaColors.Coral;
                    text = NostriviaColors.Navy;
                    break;
                default:
                    fill = NostriviaColors.White;
                    text = NostriviaColors.Navy;
                    break;
            }

            if (fillImage != null)
            {
                fillImage.color = fill;
            }

            if (label != null)
            {
                label.color = text;
            }
        }

        public void SetText(string t)
        {
            if (label != null)
            {
                label.text = t;
            }
        }

        public void SetIndex(int i)
        {
            Index = i;
        }
    }
}
