using Main;
using Model;
using naichilab;
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
        [SerializeField] private Button tweetButton;

        private void Start()
        {
            // 初期状態では非表示
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
            
            // ボタンのイベントを設定
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }

            if (titleButton != null)
            {
                titleButton.onClick.AddListener(OnTitleButtonClicked);
            }

            if (tweetButton != null)
            {
                tweetButton.onClick.AddListener(OnTweetButtonClicked);
            }
            
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

        private void OnTweetButtonClicked()
        {
            PlayerState playerState = GamePlayMode.Shared.PlayerState;
            var text = $"轢乃珈琲店で{playerState.Score}点獲得！\n";
            // ツイート機能を呼び出す
            UnityRoomTweet.Tweet(GamePlayMode.Shared.GameId, text, "unityroom", "unity1week");
        }
    }
} 