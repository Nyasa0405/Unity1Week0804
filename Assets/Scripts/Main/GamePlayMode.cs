using System;
using System.Collections;
using System.Collections.Generic;
using Interface;
using Model;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Main
{
    public class GamePlayMode : MonoBehaviour
    {

        [Header("Game Settings"), SerializeField]
         private GameSettings settings;

        [Header("Spawn Settings"), SerializeField]
         private NavMeshSurface navMeshSurface;
        [SerializeField] private LayerMask spawnLayerMask = 1;

        public List<ICoffeeBean> Beans = new List<ICoffeeBean>();

        private Transform spawnCenter;

        private Coroutine spawnCoroutine;
        public static GamePlayMode Shared { get; private set; }
        public IPlayer Player { get; private set; }
        public PlayerState PlayerState { get; private set; }
        public GameSettings Settings => settings;

        private void Awake()
        {
            if (Shared == null)
            {
                Shared = this;
                PlayerState = new PlayerState();
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

            spawnCoroutine = StartCoroutine(SpawnBeansRoutine());
        }

        private void OnDestroy()
        {
            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
        }

        public void OnPlayerSpawn(IPlayer _player)
        {
            if (Player != null)
            {
                throw new Exception("Player is already spawned");
            }
            Player = _player;
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
            if (settings.BeanPrefab == null) return;

            Vector3 spawnPosition = GetRandomNavMeshPosition();
            if (spawnPosition != Vector3.zero)
            {
                GameObject beanObject = Instantiate(settings.BeanPrefab, spawnPosition, Quaternion.identity);

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

        public void OnBeanDestroyed(ICoffeeBean _bean)
        {
            Beans.Remove(_bean);
        }
    }
}