using Interface;
using Main;
using Model.Generated;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class CoffeeMillController : MonoBehaviour, IPlayer
    {
        [Header("Basic Settings")]
        [SerializeField] private float maxSpeed = 14f;
        [SerializeField] private float accelerationForce = 1300f;
        [SerializeField] private float maxSteerAngle = 100f;

        [Header("Mill Settings")]
        [SerializeField] private Transform millTransform;
        [SerializeField] private float millRotationSpeed = 360f; // ミルの最大回転速度（度/秒）
        [SerializeField] private float millResponseSpeed = 5f; // ミル回転の応答速度
        [SerializeField, Range(0f, 1f)] private float forwardMillPower = 0.1f; // 前進時のミル回転パワー（0-1）

        [Header("Audio Settings")]
        [SerializeField] private AudioSource mainAudioSource;
        [SerializeField] private AudioSource millAudioSource;

        // 内部計算用の変数
        private float currentMillRotationPower;
        private float currentSpeed;
        private float lastSpeed;
        private float grindTimer;
        private bool isDrifting;
        private bool wasMillRotating = false;
        private float lastSteeringInput;
        private float steeringSpeed; // 旋回速度

        private Rigidbody rb;
        private float steeringInput;
        private float throttleInput;
        private InputSystem_Actions inputActions;
        private InputAction moveAction;

        // 自動計算される値
        private float lowSpeedThreshold => maxSpeed * 0.2f; // 最大速度の20%
        private float driftSpeedThreshold => maxSpeed * 0.3f; // 最大速度の30%
        private float minSteeringSpeed => maxSpeed * 0.05f; // 最大速度の5%

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            inputActions = new InputSystem_Actions();
            
            // 物理設定
            rb.linearDamping = 0.14f;
            rb.angularDamping = 0.8f;
            
            // 入力設定
            moveAction = inputActions.Player.Move;
            inputActions.Enable();
            
            // ゲーム管理
            GamePlayMode.Shared.OnPlayerSpawn(this);
            GamePlayMode.Shared.OnGameEnded += () =>
            {
                if (millAudioSource != null)
                {
                    GamePlayMode.Shared.StopMillSound(millAudioSource);
                }
            };

            // AudioSource設定
            SetupAudioSources();
        }

        private void SetupAudioSources()
        {
            // メインAudioSource（効果音用）
            if (mainAudioSource == null)
            {
                mainAudioSource = GetComponent<AudioSource>();
                if (mainAudioSource == null)
                {
                    throw new System.Exception("Main AudioSource is not assigned and no AudioSource component found on the player.");
                }
            }

            // ミル専用AudioSource
            if (millAudioSource == null)
            {
                millAudioSource = gameObject.AddComponent<AudioSource>();
                if (millAudioSource == null)
                {
                    throw new System.Exception("Mill AudioSource is not assigned and could not be created.");
                }
                millAudioSource.loop = false;
                millAudioSource.playOnAwake = false;
            }
        }

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Disable();
                inputActions.Dispose();
            }
            GamePlayMode.Shared.OnPlayerDestroyed(this);
        }

        private void FixedUpdate()
        {
            // ゲームが終了している場合は入力を無効化
            if (!GamePlayMode.Shared.IsGameActive)
            {
                throttleInput = 0f;
                steeringInput = 0f;
                return;
            }
            
            // 入力を取得
            var moveValue = moveAction.ReadValue<Vector2>();
            throttleInput = moveValue.y;
            steeringInput = moveValue.x;

            // 旋回速度を計算
            steeringSpeed = Mathf.Abs(steeringInput - lastSteeringInput) / Time.fixedDeltaTime;
            lastSteeringInput = steeringInput;

            lastSpeed = currentSpeed;
            currentSpeed = rb.linearVelocity.magnitude;

            // 各処理を実行
            HandleMovement();
            UpdateMillRotation();
            HandleMillSound();
            HandleGrinding();
            HandleSpilling();
        }

        private void HandleMovement()
        {
            HandleAcceleration();
            HandleDeceleration();
            HandleSteering();
            HandleDrift();
        }

        private void HandleAcceleration()
        {
            Vector3 forward = transform.forward;
            float forwardVel = Vector3.Dot(rb.linearVelocity, forward);

            if (throttleInput > 0 && forwardVel < maxSpeed)
            {
                rb.AddForce(forward * (throttleInput * accelerationForce * Time.fixedDeltaTime));
            }
            else if (throttleInput < 0)
            {
                rb.AddForce(forward * (throttleInput * accelerationForce * Time.fixedDeltaTime));
            }
        }

        private void HandleDeceleration()
        {
            // エンジンブレーキ（アクセルを離した時の減速）
            if (Mathf.Abs(throttleInput) < 0.1f && currentSpeed > 0.1f)
            {
                Vector3 forward = transform.forward;
                float forwardVel = Vector3.Dot(rb.linearVelocity, forward);

                if (forwardVel > 0.1f)
                {
                    Vector3 engineBrakeForceVector = -forward * (2f * Time.fixedDeltaTime);
                    rb.AddForce(engineBrakeForceVector);
                }
            }

            // 空気抵抗（速度の2乗に比例）
            if (currentSpeed > 0.1f)
            {
                Vector3 dragForce = -rb.linearVelocity.normalized * (0.3f * currentSpeed * currentSpeed * Time.fixedDeltaTime);
                rb.AddForce(dragForce);
            }
        }

        private void HandleSteering()
        {
            // 静止時または極低速時のステアリング制限
            if (currentSpeed < minSteeringSpeed)
            {
                return;
            }

            // 速度に応じたステアリング調整
            float speedFactor = Mathf.Lerp(1.5f, 0.2f, currentSpeed / maxSpeed);
            float throttleFactor = 1f - Mathf.Abs(throttleInput) * 0.5f;

            float steerAmount = steeringInput * maxSteerAngle * speedFactor * throttleFactor;
            Quaternion deltaRot = Quaternion.Euler(0f, steerAmount * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * deltaRot);
        }

        private void HandleDrift()
        {
            float lateralSpeed = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.right));
            float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

            // ドリフト条件の判定（より厳密に）
            bool shouldDrift = currentSpeed > driftSpeedThreshold &&
                               lateralSpeed > currentSpeed * 0.05f && // 閾値を下げる
                               Mathf.Abs(steeringInput) > 0.3f && // ステアリング閾値を上げる
                               throttleInput > 0.2f; // アクセル閾値を上げる

            isDrifting = shouldDrift;

            if (isDrifting)
            {
                // ドリフト時の挙動（より強く）
                Vector3 forwardVel = transform.forward * forwardSpeed;
                Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

                // 後輪スリップを強くする
                Vector3 rearWheelForce = -lateralVel * (0.5f * accelerationForce * Time.fixedDeltaTime);
                Vector3 frontWheelForce = lateralVel * (0.1f * accelerationForce * Time.fixedDeltaTime);
                rb.AddForce(rearWheelForce + frontWheelForce);

                // 横方向速度をより保持
                rb.linearVelocity = forwardVel + lateralVel * 0.9f;
            }
            else
            {
                // 通常走行時の横方向速度減衰
                Vector3 forwardVel = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
                Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
                
                float lateralDecay = Mathf.Lerp(0.95f, 0.98f, currentSpeed / maxSpeed);
                Vector3 vel = forwardVel + lateralVel * lateralDecay;
                vel.y = 0f;
                rb.linearVelocity = vel;
            }
        }

        private void UpdateMillRotation()
        {
            float targetRotationPower = 0f;

            if (isDrifting)
            {
                // ドリフト時は最高速度で回転
                targetRotationPower = 1f;
            }
            else if (Mathf.Abs(steeringInput) > 0.1f && currentSpeed > 1f)
            {
                // 旋回時：旋回速度に応じて回転
                float steeringPower = Mathf.Abs(steeringInput);
                float speedPower = Mathf.Clamp01(currentSpeed / maxSpeed);
                targetRotationPower = steeringPower * speedPower;
            }
            else if (throttleInput > 0.1f && currentSpeed > 0.5f)
            {
                // 前進時：ゆっくり回転
                targetRotationPower = forwardMillPower * throttleInput;
            }

            // 回転パワーの滑らかな変化
            currentMillRotationPower = Mathf.Lerp(currentMillRotationPower, targetRotationPower, Time.fixedDeltaTime * millResponseSpeed);
            
            // 極小値を0に丸める
            if (Mathf.Abs(currentMillRotationPower) < 0.001f)
            {
                currentMillRotationPower = 0f;
            }

            // ミルの回転を更新
            if (millTransform != null)
            {
                float rotationSpeed = currentMillRotationPower * millRotationSpeed;
                millTransform.Rotate(Vector3.up, rotationSpeed * Time.fixedDeltaTime, Space.Self);
            }
        }

        private void HandleMillSound()
        {
            bool isCurrentlyRotating = currentMillRotationPower > 0.001f;
            
            // ミル回転状態が変わった時のみ音を制御
            if (isCurrentlyRotating && !wasMillRotating)
            {
                GamePlayMode.Shared.StartMillSound(millAudioSource);
            }
            else if (!isCurrentlyRotating && wasMillRotating)
            {
                GamePlayMode.Shared.StopMillSound(millAudioSource);
            }
            
            wasMillRotating = isCurrentlyRotating;

            // 速度に応じたピッチ調整
            var rate = currentSpeed / maxSpeed;
            var addPitch = Mathf.Clamp((rate - 0.7f) / 0.3f, 0f, 1f);
            if (millAudioSource != null)
            {
                millAudioSource.pitch = 1f + addPitch * 0.7f;
            }
        }

        private void HandleGrinding()
        {
            // ミルが回転している時は豆を挽く
            if (currentMillRotationPower > GamePlayMode.Shared.Settings.MinMillRotationForGrinding &&
                GamePlayMode.Shared.PlayerState.GroundBeans > 0)
            {
                grindTimer += Time.fixedDeltaTime;

                // ミルの回転パワーに応じて挽くスピードを変える
                float grindSpeedMultiplier = currentMillRotationPower;
                float grindInterval = 1f / (GamePlayMode.Shared.Settings.BaseMillGrindSpeed * grindSpeedMultiplier);

                if (grindTimer >= grindInterval)
                {
                    int beansToGrind = Mathf.Min(1, GamePlayMode.Shared.PlayerState.GroundBeans);
                    GamePlayMode.Shared.PlayerState.ConsumeGroundBeans(beansToGrind);
                    GamePlayMode.Shared.PlayerState.AddGroundCoffee(beansToGrind, mainAudioSource);
                    grindTimer = 0f;
                }
            }
            else
            {
                grindTimer = 0f;
            }
        }

        private void HandleSpilling()
        {
            // 高速衝突時はコーヒーがこぼれる
            if (lastSpeed > GamePlayMode.Shared.Settings.SpillSpeedThreshold
                && currentSpeed <= 4f)
            {
                GamePlayMode.Shared.PlayHitObstacleSound(transform);
                GamePlayMode.Shared.PlayerState.RemoveGroundCoffee(1);
            }
        }

        public Transform Transform => transform;
        public float Speed => currentSpeed;
        public float MaxSpeed => maxSpeed;
    }
}