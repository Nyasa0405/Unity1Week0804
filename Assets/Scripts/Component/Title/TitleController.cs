using System.Collections;
using Main;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Component.Title
{
    public class TitleController : MonoBehaviour
    {
        [SerializeField]
        private Button startButton;

        [SerializeField]
        private Button howToPlayButton;

        [SerializeField]
        private HowToPlayController howToPlayPanel;

        [SerializeField]
        private GameSettings gameSettings;

        private void Start()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }
            else
            {
                Debug.LogError("Start Button is not assigned in the inspector.");
            }

            if (howToPlayButton != null)
            {
                howToPlayButton.onClick.AddListener(OnHowToPlayButtonClicked);
            }
            else
            {
                Debug.LogError("How To Play Button is not assigned in the inspector.");
            }

            if (howToPlayPanel != null)
            {
                howToPlayPanel.OnClose += OnHowToPlayPanelClosed;
            }
            else
            {
                Debug.LogError("How To Play Panel is not assigned in the inspector.");
            }
        }

        private void OnDestroy()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartButtonClicked);
            }

            if (howToPlayButton != null)
            {
                howToPlayButton.onClick.RemoveListener(OnHowToPlayButtonClicked);
            }

            if (howToPlayPanel != null)
            {
                howToPlayPanel.OnClose -= OnHowToPlayPanelClosed;
            }
        }

        private void OnStartButtonClicked()
        {
            // Start the game
            if (gameSettings == null)
            {
                Debug.LogError("GameSettings is not assigned in the inspector.");
                return;
            }
            StartCoroutine(DelayStartGame());
        }
        private IEnumerator DelayStartGame()
        {
            yield return new WaitForSeconds(0.5f); // Delay for 0.5 seconds before starting the game
            SceneManager.LoadScene(gameSettings.GameSceneName);
        }

        private void OnHowToPlayButtonClicked()
        {
            // Show the How To Play panel
            if (howToPlayPanel != null)
            {
                howToPlayPanel.gameObject.SetActive(true);

            }
            else
            {
                Debug.LogError("How To Play Panel is not assigned in the inspector.");
            }
        }

        private void OnHowToPlayPanelClosed()
        {
            // Hide the How To Play panel
            if (howToPlayPanel != null)
            {
                howToPlayPanel.gameObject.SetActive(false);
            }
        }
    }
}