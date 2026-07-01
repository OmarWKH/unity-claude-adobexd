using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nostrivia
{
    /// <summary>
    /// Counts down from a given number of seconds using unscaled time, updating a TMP label
    /// ("N s") and blinking the pill background between two colors once remaining time drops
    /// to or below blinkThreshold. Fires Elapsed exactly once when the countdown reaches 0.
    /// </summary>
    public class CountdownTimer : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image pillBackground;

        [SerializeField] private float blinkThreshold = 10f;
        [SerializeField] private Color blinkColorA = NostriviaColors.Gold;
        [SerializeField] private Color blinkColorB = NostriviaColors.Coral;
        [SerializeField] private Color normalColor = NostriviaColors.Gold;

        [SerializeField] private float blinkRate = 2.5f; // toggles per second

        public event Action Elapsed;

        private float remaining;
        private bool running;
        private bool blinkStateA = true;
        private float blinkTimer;

        private void Awake()
        {
            ApplyBackground(normalColor);
        }

        public void StartFrom(float seconds)
        {
            remaining = Mathf.Max(0f, seconds);
            running = remaining > 0f;
            blinkTimer = 0f;
            blinkStateA = true;
            UpdateLabel();
            ApplyBackground(remaining <= blinkThreshold ? blinkColorA : normalColor);

            if (!running)
            {
                Elapsed?.Invoke();
            }
        }

        private void Update()
        {
            if (!running)
            {
                return;
            }

            remaining -= Time.unscaledDeltaTime;

            if (remaining <= 0f)
            {
                remaining = 0f;
                running = false;
                UpdateLabel();
                ApplyBackground(normalColor);
                Elapsed?.Invoke();
                return;
            }

            UpdateLabel();

            if (remaining <= blinkThreshold)
            {
                blinkTimer += Time.unscaledDeltaTime;
                if (blinkTimer >= 1f / blinkRate)
                {
                    blinkTimer = 0f;
                    blinkStateA = !blinkStateA;
                    ApplyBackground(blinkStateA ? blinkColorA : blinkColorB);
                }
            }
        }

        private void UpdateLabel()
        {
            if (label != null)
            {
                label.text = Mathf.CeilToInt(remaining) + " s";
            }
        }

        private void ApplyBackground(Color color)
        {
            if (pillBackground != null)
            {
                pillBackground.color = color;
            }
        }
    }
}
