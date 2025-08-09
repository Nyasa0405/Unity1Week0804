using Main;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Component
{
    public class OverlayController : MonoBehaviour
    {
        [SerializeField]
        private Slider beansSlider;

        [SerializeField]
        private Slider coffeeSlider;

        [SerializeField]
        private TextMeshProUGUI scoreText;

        private void Start()
        {
            if (beansSlider == null || coffeeSlider == null || scoreText == null)
            {
                Debug.LogError("OverlayController: Missing UI components.");
                return;
            }

            UpdateUI();
        }

        private void Update()
        {
            if (GamePlayMode.Shared == null || GamePlayMode.Shared.PlayerState == null)
            {
                return;
            }

            // ゲームが開始されていない場合はUIを更新しない
            if (!GamePlayMode.Shared.IsGameActive)
            {
                return;
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            PlayerState playerState = GamePlayMode.Shared.PlayerState;

            if (beansSlider != null)
            {
                beansSlider.value = playerState.GroundBeans;
                beansSlider.maxValue = GamePlayMode.Shared.Settings.MaxGroundBeans;
            }

            if (coffeeSlider != null)
            {
                coffeeSlider.value = playerState.GroundCoffee;
                coffeeSlider.maxValue = GamePlayMode.Shared.Settings.MaxGroundCoffee;
            }

            if (scoreText != null)
            {
                scoreText.text = $"Score: {playerState.Score}";
            }
        }
    }
}