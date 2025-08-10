using Main;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Component
{
    public class OverlayController : MonoBehaviour
    {
        private static readonly int LiquidPosition = Shader.PropertyToID("_LiquidPosition");
        [SerializeField]
        private Image beansSliderFill;

        [SerializeField]
        private Material coffeeSliderFill;

        [SerializeField]
        private TextMeshProUGUI scoreText;

        private void Start()
        {
            if (beansSliderFill == null || coffeeSliderFill == null || scoreText == null)
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

            if (beansSliderFill != null)
            {
                beansSliderFill.fillAmount = playerState.GroundBeans / (float)GamePlayMode.Shared.Settings.MaxGroundBeans;
            }

            if (coffeeSliderFill != null)
            {
                var rate = playerState.GroundCoffee / (float)GamePlayMode.Shared.Settings.MaxGroundCoffee;
                coffeeSliderFill.SetFloat(LiquidPosition, rate);
            }

            if (scoreText != null)
            {
                scoreText.text = $"Score: {playerState.Score}";
            }
        }
    }
}