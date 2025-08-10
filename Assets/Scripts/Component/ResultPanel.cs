using System.Collections;
using System.Threading.Tasks;
using Main;
using Model;
using naichilab;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using unityroom.Api;

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

        private Coroutine sendScoreCoroutine;

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

            StartCoroutine(InitializeAsync());
        }

        private IEnumerator InitializeAsync()
        {
            // 非同期処理のために少し待機
            yield return new WaitForSeconds(3f);
            // ゲーム終了イベントを購読
            GamePlayMode.Shared.OnGameEnded += ShowResult;
        }

        private void OnDestroy()
        {
            // イベントの購読を解除
            if (GamePlayMode.Shared != null)
                GamePlayMode.Shared.OnGameEnded -= ShowResult;

            if (sendScoreCoroutine != null)
            {
                StopCoroutine(sendScoreCoroutine);
            }
        }

        private void ShowResult()
        {
            if (resultPanel != null)
                resultPanel.SetActive(true);
            
            // スコア情報を表示
            UpdateResultDisplay();
            if (sendScoreCoroutine != null)
            {
                StopCoroutine(sendScoreCoroutine);
            }

            sendScoreCoroutine = StartCoroutine(SendScoreCoroutine());
        }

        private IEnumerator SendScoreCoroutine()
        {
            yield return new WaitForSeconds(0.2f);
            UnityroomApiClient.Instance.SendScore(1, GamePlayMode.Shared.PlayerState.Score, ScoreboardWriteMode.HighScoreDesc);
        }

        private void UpdateResultDisplay()
        {
            PlayerState playerState = GamePlayMode.Shared.PlayerState;
            
            if (scoreText != null)
                scoreText.text = $"Score: {playerState.Score}";
            
            if (groundBeansText != null)
                groundBeansText.text = $"Beans: {playerState.Result.GroundBeans}";
            
            if (groundCoffeeText != null)
                groundCoffeeText.text = $"Coffee: {playerState.Result.MakeCoffee}";
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
            UnityRoomTweet.Tweet(GamePlayMode.Shared.GameId, text, "unityroom", "unity1week", "轢乃珈琲店");
        }
    }
} 