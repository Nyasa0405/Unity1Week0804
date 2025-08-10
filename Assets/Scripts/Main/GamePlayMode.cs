using System;
using System.Collections;
using System.Collections.Generic;
using Component;
using Interface;
using Model;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Main
{
    public partial class GamePlayMode: MonoBehaviour
    {
        private string gameId = "hikinocoffee";
        public string GameId => gameId;

        [Header("Game Settings")]
        [SerializeField] private GameSettings settings;

        [Header("Spawn Settings")]
        [SerializeField] private NavMeshSurface navMeshSurface;
        [SerializeField] private LayerMask spawnLayerMask = 1;
		[SerializeField] private List<Transform> spawnPoints;

        public List<ICoffeeBean> Beans = new List<ICoffeeBean>();

        private Transform spawnCenter;
        private Coroutine spawnCoroutine;
        private Coroutine gameTimerCoroutine;
        private Coroutine countdownCoroutine;
        
        // ゲーム状態管理
        public bool IsGameActive { get; private set; } = false; // 初期状態をfalseに変更
        public float RemainingTime { get; private set; }
        
        // イベント
        public event Action<float> OnTimeChanged;
        public event Action OnGameEnded;
        public event Action OnCountdownStarted;
        public event Action<int> OnCountdownUpdated;
        public event Action OnCountdownFinished;
        
        public static GamePlayMode Shared { get; private set; }
        public IPlayer Player { get; private set; }
        public PlayerState PlayerState { get; } = new PlayerState();
        public GameSettings Settings => settings;
        public SoundSettings SoundSettings => soundSettings;

        private void Awake()
        {
            if (Shared == null)
            {
                Shared = this;
            }
            else
            {
                throw new Exception("GamePlayMode is already initialized");
            }
        }

        private void Start()
        {
            if (spawnCenter == null)
                spawnCenter = transform;

            RemainingTime = settings.GameTimeSec;
            
            // カウントダウンを開始
            StartCountdown();
        }

        private void OnDestroy()
        {
            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
            if (gameTimerCoroutine != null)
                StopCoroutine(gameTimerCoroutine);
            if (countdownCoroutine != null)
                StopCoroutine(countdownCoroutine);
            Shared = null;
        }

        private void StartCountdown()
        {
            // ゲームを一時停止状態にする
            Time.timeScale = 0f;
            IsGameActive = false;
            
            // カウントダウンコルーチンを開始
            countdownCoroutine = StartCoroutine(CountdownRoutine());
        }

        private IEnumerator CountdownRoutine()
        {
            yield return new WaitForSecondsRealtime(1f);
            // カウントダウン開始イベントを発火
            OnCountdownStarted?.Invoke();
            // 3,2,1のカウントダウン
            for (int i = 3; i > 0; i--)
            {
                OnCountdownUpdated?.Invoke(i);
                PlayCountdownSound();
                yield return new WaitForSecondsRealtime(1f);
            }
            
            // カウントダウン終了
            OnCountdownFinished?.Invoke();
            PlayStartAndFinishSound();
            
            // ゲームを開始
            StartGame();
        }

        private void StartGame()
        {
            // ゲームを再開
            Time.timeScale = 1f;
            IsGameActive = true;
            
            // ゲーム開始処理
            spawnCoroutine = StartCoroutine(SpawnBeansRoutine());
            gameTimerCoroutine = StartCoroutine(GameTimerRoutine());
            if (soundSettings.GameBGM)
            {
                bgmAudioSource.clip = soundSettings.GameBGM;
                bgmAudioSource.loop = true;
                bgmAudioSource.Play();
            }
        }

        private IEnumerator GameTimerRoutine()
        {
            while (IsGameActive && RemainingTime > 0f)
            {
                yield return new WaitForSeconds(1f);
                RemainingTime -= 1f;
                OnTimeChanged?.Invoke(RemainingTime);
                
                if (RemainingTime <= 0f)
                {
                    EndGame();
                }
            }
        }

        private void EndGame()
        {
            IsGameActive = false;
            if (gameTimerCoroutine != null)
            {
                StopCoroutine(gameTimerCoroutine);
            }
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
            if (bgmAudioSource != null && bgmAudioSource.isPlaying)
            {
                bgmAudioSource.Stop();
            }
            PlayStartAndFinishSound();
            Time.timeScale = 0f; // ゲームを一時停止
            OnGameEnded?.Invoke();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            Player = null;
            Beans.Clear();
            
            // コルーチンを停止
            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
            if (gameTimerCoroutine != null)
                StopCoroutine(gameTimerCoroutine);
            if (countdownCoroutine != null)
                StopCoroutine(countdownCoroutine);
            
            // Sharedをnullに設定（OnDestroy()の代わり）
            Shared = null;
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToTitle()
        {
            Time.timeScale = 1f;
            Player = null;
            Beans.Clear();
            
            // コルーチンを停止
            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
            if (gameTimerCoroutine != null)
                StopCoroutine(gameTimerCoroutine);
            if (countdownCoroutine != null)
                StopCoroutine(countdownCoroutine);
            
            // Sharedをnullに設定（OnDestroy()の代わり）
            Shared = null;
            
            SceneManager.LoadScene("Title");
        }

        public void LoadGameScene()
        {
            if (settings.GameSceneName != null)
            {
                SceneManager.LoadScene(settings.GameSceneName);
            }
            else
            {
                Debug.LogError("Game scene is not set in the GamePlayMode settings.");
            }
        }

        public void OnPlayerSpawn(IPlayer _player)
        {
            if (Player != null)
            {
                throw new Exception("Player is already spawned");
            }
            Player = _player;
        }

        public void OnPlayerDestroyed(IPlayer _player)
        {
            if (Player == _player)
            {
                Player = null;
            }
            else
            {
                throw new Exception("Trying to destroy a player that is not the current player");
            }
        }

        private IEnumerator SpawnBeansRoutine()
        {
            while (IsGameActive) // IsGameActiveチェックを追加
            {
                if (Beans.Count < settings.MaxBeanCount)
                {
                    SpawnBean();
                }
                yield return new WaitForSeconds(settings.BeanSpawnInterval);
            }
        }

        private void SpawnBean()
        {
            if (settings.BeanPrefabs == null || settings.BeanPrefabs.Count == 0) return;

            Vector3 spawnPosition = GetRandomNavMeshPosition();
            var beanPrefab = settings.BeanPrefabs[Random.Range(0, settings.BeanPrefabs.Count)];
            if (spawnPosition != Vector3.zero)
            {
                GameObject beanObject = Instantiate(beanPrefab, spawnPosition, Quaternion.identity);

                ICoffeeBean coffeeBean = beanObject.GetComponent<ICoffeeBean>();
                if (coffeeBean != null)
                {
                    Beans.Add(coffeeBean);
                }
            }
        }

        private Vector3 GetRandomNavMeshPosition()
        {
            Vector3? spawnPoint;

            if (spawnPoints == null || spawnPoints.Count == 0)
            {
                // Spawn pointsが設定されていない場合は中心位置を使用
                return navMeshSurface.center;
            }
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)]?.position;

            if (spawnPoint == null)
            {
                // Debug.LogWarning("No valid spawn point found.");
                return navMeshSurface.center;
            }
            return spawnPoint.Value;
        }

        public void OnCrushBeanEffect(ICoffeeBean _bean)
        {
            var obj = Instantiate(settings.CrushBeanEffectPrefab, _bean.Transform.position, Quaternion.identity);
            CrushBeanEffectPlayer effectPlayer = obj.GetComponent<CrushBeanEffectPlayer>();
            if (effectPlayer != null)
            {
                effectPlayer.Play(_bean, Player);
            }
            else
            {
                Debug.LogWarning("CrushBeanEffectPlayer component not found on the effect prefab.");
            }
        }

        public void OnBeanDestroyed(ICoffeeBean _bean)
        {
            Beans.Remove(_bean);
        }
    }
}