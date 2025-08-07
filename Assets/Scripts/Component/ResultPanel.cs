using Main;
using Model;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Component
{
    public class ResultPanel : MonoBehaviour
    {
        [Header("UI References"), SerializeField]
        private GameObject resultPanel;
        
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI groundBeansText;
        [SerializeField] private TextMeshProUGUI groundCoffeeText;
        
        [SerializeField] private Button restartButton;
        [SerializeField] private Button titleButton;

        private void Start()
        {
            // 初期状態では非表示
            if (resultPanel != null)
                resultPanel.SetActive(false);
            
            // ボタンのイベントを設定
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            
            if (titleButton != null)
                titleButton.onClick.AddListener(OnTitleButtonClicked);
            
            // ゲーム終了イベントを購読
            GamePlayMode.Shared.OnGameEnded += ShowResult;
        }

        private void OnDestroy()
        {
            // イベントの購読を解除
            if (GamePlayMode.Shared != null)
                GamePlayMode.Shared.OnGameEnded -= ShowResult;
        }

        private void ShowResult()
        {
            if (resultPanel != null)
                resultPanel.SetActive(true);
            
            // スコア情報を表示
            UpdateResultDisplay();
        }

        private void UpdateResultDisplay()
        {
            PlayerState playerState = GamePlayMode.Shared.PlayerState;
            
            if (scoreText != null)
                scoreText.text = $"Score: {playerState.Score}";
            
            if (groundBeansText != null)
                groundBeansText.text = $"Beans: {playerState.GroundBeans}";
            
            if (groundCoffeeText != null)
                groundCoffeeText.text = $"Coffee: {playerState.GroundCoffee}";
        }

        private void OnRestartButtonClicked()
        {
            GamePlayMode.Shared.RestartGame();
        }

        private void OnTitleButtonClicked()
        {
            GamePlayMode.Shared.ReturnToTitle();
        }
    }
} 