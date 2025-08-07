using Main;
using UnityEngine;
using TMPro;

namespace Component
{
    public class GameTimerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        private readonly string timeFormat = "Time: {0:D2}:{1:D2}";

        private void Start()
        {
            // 初期時間を表示
            UpdateTimerDisplay(GamePlayMode.Shared.RemainingTime);
            
            // 時間変更イベントを購読
            GamePlayMode.Shared.OnTimeChanged += UpdateTimerDisplay;
        }

        private void OnDestroy()
        {
            // イベントの購読を解除
            if (GamePlayMode.Shared != null)
                GamePlayMode.Shared.OnTimeChanged -= UpdateTimerDisplay;
        }

        private void UpdateTimerDisplay(float remainingTime)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                timerText.text = string.Format(timeFormat, minutes, seconds);
            }
        }
    }
} 