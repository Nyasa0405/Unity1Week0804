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
        [Header("System Settings"), SerializeField]
        private string gameId = "";
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
        
        // ゲーム状態管理
        public bool IsGameActive { get; private set; } = true;
        public float RemainingTime { get; private set; }
        
        // イベント
        public event Action<float> OnTimeChanged;
        public event Action OnGameEnded;
        
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
            spawnCoroutine = StartCoroutine(SpawnBeansRoutine());
            gameTimerCoroutine = StartCoroutine(GameTimerRoutine());
        }

        private void OnDestroy()
        {
            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
            if (gameTimerCoroutine != null)
                StopCoroutine(gameTimerCoroutine);
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
            Time.timeScale = 0f; // ゲームを一時停止
            OnGameEnded?.Invoke();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToTitle()
        {
            Time.timeScale = 1f;
            // タイトルシーンに戻る（シーン名は適宜変更してください）
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
            while (true)
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