using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Interface;
using Main;
using Random = UnityEngine.Random;

namespace Component
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class CoffeeBeanAgent : MonoBehaviour, ICoffeeBean
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Transform Transform => transform;

        [Header("Roaming Settings")]
        [SerializeField] private float roamRadius = 10f;        // Radius for roaming wander
        [SerializeField] private float roamInterval = 3f;       // Time between roam destinations
        [SerializeField] private float roamSpeed = 3f;          // Speed while roaming

        [Header("Flee Settings")]
        [SerializeField] private float fleeDistance = 5f;       // Distance to trigger flee
        [SerializeField] private float fleeSpeed = 6f;          // Speed while fleeing

        [Header("Cohesion Settings")]
        [SerializeField] private float cohesionRadius = 3f;     // Radius to find group
        [SerializeField] private int maxGroupSize = 5;          // Max neighbors to join
        [SerializeField] private float cohesionSpeed = 4f;      // Speed for cohesion
        [SerializeField] private float minGroupDuration = 3f;   // Min time in group
        [SerializeField] private float maxGroupDuration = 6f;   // Max time in group

        private enum State { Roaming, Fleeing, Cohesion }
        private State currentState = State.Roaming;

        private NavMeshAgent agent;
        private Coroutine roamCoroutine;
        private Coroutine cohesionCoroutine;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            GamePlayMode.Shared.Beans.Add(this);
        }

        private void OnDestroy()
        {
            GamePlayMode.Shared.Beans.Remove(this);
            StopAllCoroutines();
        }

        private void Start()
        {
            agent.speed = roamSpeed;
            roamCoroutine = StartCoroutine(RoamRoutine());
        }

        private void Update()
        {
            float distToPlayer = Vector3.Distance(transform.position, GamePlayMode.Shared.Player.Transform.position);
            if (distToPlayer < fleeDistance)
            {
                if (currentState != State.Fleeing)
                {
                    TransitionToFlee();
                }
            }
            else if (currentState != State.Cohesion)
            {
                var neighbors = GetNeighbors();
                if (neighbors.Count > 0 && neighbors.Count < maxGroupSize)
                {
                    TransitionToCohesion();
                }
                else if (currentState != State.Roaming)
                {
                    TransitionToRoam();
                }
            }
        }

        private IEnumerator RoamRoutine()
        {
            while (true)
            {
                Vector3 randomPos = Random.insideUnitSphere * roamRadius + transform.position;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPos, out hit, roamRadius, NavMesh.AllAreas))
                {
                    agent.speed = roamSpeed;
                    agent.SetDestination(hit.position);
                }
                yield return new WaitForSeconds(roamInterval);
            }
        }

        private void TransitionToFlee()
        {
            currentState = State.Fleeing;
            StopCoroutine(roamCoroutine);
            if (cohesionCoroutine != null) StopCoroutine(cohesionCoroutine);
            StartCoroutine(FleeRoutine());
        }

        private IEnumerator FleeRoutine()
        {
            while (Vector3.Distance(transform.position, GamePlayMode.Shared.Player.Transform.position) < fleeDistance * 1.2f)
            {
                Vector3 dir = (transform.position - GamePlayMode.Shared.Player.Transform.position).normalized;
                Vector3 fleeTarget = transform.position + dir * fleeDistance;
                agent.speed = fleeSpeed;
                agent.SetDestination(fleeTarget);
                yield return new WaitForSeconds(0.5f);
            }
            TransitionToRoam();
        }

        private void TransitionToCohesion()
        {
            currentState = State.Cohesion;
            StopCoroutine(roamCoroutine);
            if (cohesionCoroutine != null) StopCoroutine(cohesionCoroutine);
            cohesionCoroutine = StartCoroutine(CohesionRoutine());
        }

        private IEnumerator CohesionRoutine()
        {
            float duration = Random.Range(minGroupDuration, maxGroupDuration);
            float endTime = Time.time + duration;
            while (Time.time < endTime)
            {
                var center = GetGroupCenter();
                agent.speed = cohesionSpeed;
                agent.SetDestination(center);
                yield return new WaitForSeconds(1f);
            }
            TransitionToRoam();
        }

        private List<ICoffeeBean> GetNeighbors()
        {
            var list = new List<ICoffeeBean>();
            foreach (var bean in GamePlayMode.Shared.Beans)
            {
                if (bean.Id == Id) continue;
                if (Vector3.Distance(transform.position, bean.Transform.position) <= cohesionRadius)
                    list.Add(bean);
            }
            return list;
        }

        private Vector3 GetGroupCenter()
        {
            var neighbors = GetNeighbors();
            Vector3 sum = transform.position;
            foreach (var bean in neighbors) sum += bean.Transform.position;
            return sum / (neighbors.Count + 1);
        }

        /// <summary>
        /// 状態を Roaming に切り替え、
        /// 他の Coroutine を停止してランダム徘徊を再開する
        /// </summary>
        private void TransitionToRoam()
        {
            currentState = State.Roaming;
            // もし既に徘徊 Coroutine が回っていれば停止
            if (roamCoroutine != null)
            {
                StopCoroutine(roamCoroutine);
            }
            // 集団行動中の Coroutine もあれば停止
            if (cohesionCoroutine != null)
            {
                StopCoroutine(cohesionCoroutine);
                cohesionCoroutine = null;
            }
            // 徘徊 Coroutine を開始
            roamCoroutine = StartCoroutine(RoamRoutine());
        }
    }
}