using System;
using UnityEngine;
using UnityEngine.UI;

namespace Component
{
    public class HowToPlayController: MonoBehaviour
    {
        public event Action OnClose;

        [SerializeField]
        private Button closeButton;

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
            else
            {
                Debug.LogError("Close Button is not assigned in the inspector.");
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }
        }

        private void OnCloseButtonClicked()
        {
            // Close the How To Play panel
            OnClose?.Invoke();
        }
    }
}