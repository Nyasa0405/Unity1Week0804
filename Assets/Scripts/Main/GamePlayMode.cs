using System;
using System.Collections;
using System.Collections.Generic;
using Component;
using Interface;
using Model;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Main
{
    public partial class GamePlayMode: MonoBehaviour
    {

        [Header("Game Settings")]
        [SerializeField] private GameSettings settings;

        [Header("Spawn Settings")]
        [SerializeField] private NavMeshSurface navMeshSurface;
        [SerializeField] private LayerMask spawnLayerMask = 1;

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
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void ReturnToTitle()
        {
            Time.timeScale = 1f;
            // タイトルシーンに戻る（シーン名は適宜変更してください）
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
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
            Vector3 center = navMeshSurface.center;
            Vector3 randomPos;
            int attempts = 0;
            const int maxAttempts = 30;

            do
            {
                // 円形の範囲内でランダムな位置を生成
                Vector2 randomCircle = Random.insideUnitCircle * settings.BeanSpawnRadius;
                randomPos = center + new Vector3(randomCircle.x, 100f, randomCircle.y); // 高い位置から開始
                attempts++;
            } while (attempts < maxAttempts && !IsValidSpawnPosition(randomPos));

            // 有効な位置が見つからない場合は中心位置を返す
            if (attempts >= maxAttempts)
            {
                // Debug.LogWarning("Valid spawn position not found, using center position");
                return center;
            }

            return randomPos;
        }

        private bool IsValidSpawnPosition(Vector3 _position)
        {
            // NavMesh上にあるかチェック
            NavMeshHit hit;
            if (NavMesh.SamplePosition(_position, out hit, 100f, NavMesh.AllAreas))
            {
                // 他のオブジェクトとの重複をチェック
                if (!Physics.CheckSphere(hit.position, 0.5f, spawnLayerMask))
                {
                    return true;
                }
            }
            return false;
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