using Main;
using UnityEngine;
using TMPro;

namespace Component
{
    public class CountdownUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private GameObject countdownPanel;
        
        private void Start()
        {
            // 初期状態では非表示
            if (countdownPanel != null)
            {
                countdownPanel.SetActive(false);
            }
            else
            {
                Debug.LogError("Countdown Panel is not assigned in the inspector.");
            }
            
            // カウントダウン開始イベントを購読
            if (GamePlayMode.Shared != null)
            {
                GamePlayMode.Shared.OnCountdownStarted += ShowCountdown;
                GamePlayMode.Shared.OnCountdownUpdated += UpdateCountdown;
                GamePlayMode.Shared.OnCountdownFinished += HideCountdown;
            }
            else
            {
                Debug.LogWarning("GamePlayMode.Shared is null. Countdown UI will not function.");
            }
        }

        private void OnDestroy()
        {
            // イベントの購読を解除
            if (GamePlayMode.Shared != null)
            {
                GamePlayMode.Shared.OnCountdownStarted -= ShowCountdown;
                GamePlayMode.Shared.OnCountdownUpdated -= UpdateCountdown;
                GamePlayMode.Shared.OnCountdownFinished -= HideCountdown;
            }
        }

        private void ShowCountdown()
        {
            if (countdownPanel != null)
            {
                countdownPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("Countdown Panel is not assigned in the inspector.");
            }
        }

        private void UpdateCountdown(int count)
        {
            if (countdownPanel != null)
            {
                countdownPanel.SetActive(true);
            }
            if (countdownText != null)
            {
                countdownText.text = count.ToString();
            }
        }

        private void HideCountdown()
        {
            if (countdownPanel != null)
            {
                countdownPanel.SetActive(false);
            }
        }
    }
}
