using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nostrivia
{
    /// <summary>
    /// Controller for the Leave ("WARNING") confirmation overlay. All content
    /// is hardcoded per the design; this component only wires the two action
    /// buttons to events the scene/screen manager can subscribe to.
    /// </summary>
    public class LeaveOverlay : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] Button cancelButton;
        [SerializeField] Button leaveButton;

        /// <summary>Raised when the "Cancel" button is tapped.</summary>
        public event Action CancelClicked;
        /// <summary>Raised when the "Leave" button is tapped.</summary>
        public event Action LeaveClicked;

        void OnEnable()
        {
            if (cancelButton != null) cancelButton.onClick.AddListener(RaiseCancel);
            if (leaveButton != null) leaveButton.onClick.AddListener(RaiseLeave);
        }

        void OnDisable()
        {
            if (cancelButton != null) cancelButton.onClick.RemoveListener(RaiseCancel);
            if (leaveButton != null) leaveButton.onClick.RemoveListener(RaiseLeave);
        }

        void RaiseCancel() => CancelClicked?.Invoke();
        void RaiseLeave() => LeaveClicked?.Invoke();
    }
}
