using Interface;
using Main;
using Model.Generated;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour, IPlayer
    {
        private static readonly int RotatePowerRef = Shader.PropertyToID("_RotatePower");
        /// <summary>
        ///     車の最大前方速度（メートル/秒）。
        /// </summary>
        [Header("Settings"), SerializeField]
        private float maxSpeed = 10f; // 車の最大前方速度（m/s）

        /// <summary>
        ///     加速時に適用される力。値が大きいほど加速が強くなる。
        /// </summary>
        [SerializeField]
        private float accelerationForce = 1100f; // 加速時に適用される力

        /// <summary>
        ///     1秒あたりの最大ステアリング角度（度）。
        /// </summary>
        [SerializeField]
        private float maxSteerAngle = 60f; // 最大ステアリング角度（度/秒）

        [Tooltip("低速時に適用するステアリング倍率"), SerializeField]
        private float lowSpeedSteerMultiplier = 1.5f; // 低速時のステアリング倍率（>1）

        [Tooltip("低速と判断する速度のしきい値"), SerializeField]
        private float lowSpeedThreshold = 2f; // 低速しきい値（m/s）

        [Tooltip("最高速到達時の最小ステアリング倍率"), SerializeField, Range(0f, 1f)]
        private float minSteerAtMaxSpeed = 0.2f; // 高速時の最小ステアリング倍率

        [Tooltip("アクセル入力時のステアリング減衰率"), SerializeField, Range(0f, 1f)]
        private float throttleSteerReduction = 0.5f;

        [Header("Realistic Physics"), Tooltip("空気抵抗係数"), SerializeField]
        private float dragCoefficient = 0.3f; // 空気抵抗係数

        [Tooltip("エンジンブレーキの強さ"), SerializeField]
        private float engineBrakeForce = 2f; // エンジンブレーキ

        [Tooltip("静止時の最小ステアリング速度"), SerializeField]
        private float minSteeringSpeed = 0.5f; // 静止時の最小ステアリング速度

        [Tooltip("速度に応じた減速係数（高速時は減速が遅い）"), SerializeField]
        private float speedBasedDecay = 0.8f; // 速度に応じた減速係数

        [Header("Drift Settings"), Tooltip("ドリフト開始の速度しきい値"), SerializeField]
        private float driftSpeedThreshold = 3f; // ドリフト開始速度

        [Tooltip("ドリフト時の横方向速度保持率"), SerializeField, Range(0f, 1f)]
        private float driftFactor = 0.85f; // ドリフト時の横滑り量

        [Tooltip("ドリフト時の後輪スリップ係数"), SerializeField, Range(0f, 1f)]
        private float rearWheelSlip = 0.3f; // 後輪スリップ

        [Tooltip("ドリフト時の前輪グリップ係数"), SerializeField, Range(0f, 1f)]
        private float frontWheelGrip = 0.8f; // 前輪グリップ

        [Header("Mill Rotation Settings"), SerializeField]
        private Material millMaterial; // ミルのマテリアル

        [Tooltip("前進時のミル回転速度（0-1）"), SerializeField, Range(0f, 1f)]
        private float forwardMillRotationPower = 0.1f;

        [Tooltip("旋回時のミル回転速度（0-1）"), SerializeField, Range(0f, 1f)]
        private float steeringMillRotationPower = 0.4f;

        [Tooltip("ドリフト時のミル回転速度（0-1）"), SerializeField, Range(0f, 1f)]
        private float driftMillRotationPower = 0.6f;

        [Tooltip("ミル回転の減衰速度"), SerializeField]
        private float millRotationDecay = 5f;
        private float currentMillRotationPower;
        private float currentSpeed;
        private float grindTimer;
        private bool isDrifting;
        private Vector3 lastVelocity;

        private Rigidbody rb;
        private float spillTimer;
        private float steeringInput;
        private float throttleInput;
        private InputSystem_Actions inputActions;
        private InputAction moveAction;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            inputActions = new InputSystem_Actions();
            // X軸とZ軸の回転を固定し、車が横転するのを防止
            // リニアダンピングを設定し、入力がないときに徐々に減速
            rb.linearDamping = 0.14f;
            // 角ダンピングを設定し、回転の変化を滑らかに制御
            rb.angularDamping = 0.8f;
            moveAction = inputActions.Player.Move;

            inputActions.Enable();
            GamePlayMode.Shared.OnPlayerSpawn(this);
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
            // 入力を取得: 縦(forward/back)と横(left/right)
            var moveValue = moveAction.ReadValue<Vector2>();
            throttleInput = moveValue.y; // 縦軸の入力（前進/後退）
            steeringInput = moveValue.x; // 横軸の入力（左/右）
            currentSpeed = rb.linearVelocity.magnitude;

            HandleSteering();
            HandleAcceleration();
            HandleDeceleration();
            HandleDrift();
            UpdateMillRotation();
            HandleGrinding();
            HandleSpilling();
        }

        private void OnCollisionEnter(Collision _collision)
        {
            // 高速衝突時のこぼれ判定
            if (currentSpeed > GamePlayMode.Shared.Settings.SpillSpeedThreshold)
            {
                GamePlayMode.Shared.PlayerState.IsSpilling = true;
            }
        }

        private void OnCollisionExit(Collision _collision)
        {
            // 衝突終了時はこぼれを停止
            GamePlayMode.Shared.PlayerState.IsSpilling = false;
        }

        private void OnTriggerEnter(Collider _other)
        {
            // 豆を轢く判定
            ICoffeeBean coffeeBean = _other.GetComponent<ICoffeeBean>();
            if (coffeeBean != null)
            {
                // 轢いた豆を削除
                GamePlayMode.Shared.OnBeanDestroyed(coffeeBean);
                Destroy(_other.gameObject);

                // 豆を轢いた処理（スコア加算なし）
                GamePlayMode.Shared.PlayerState.CrushBean();
            }
        }
        public Transform Transform => transform;

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

                // 前進している場合のみエンジンブレーキを適用
                if (forwardVel > 0.1f)
                {
                    Vector3 engineBrakeForceVector = -forward * (engineBrakeForce * Time.fixedDeltaTime);
                    rb.AddForce(engineBrakeForceVector);
                }
            }

            // 空気抵抗（速度の2乗に比例）
            if (currentSpeed > 0.1f)
            {
                Vector3 dragForce = -rb.linearVelocity.normalized * (dragCoefficient * currentSpeed * currentSpeed * Time.fixedDeltaTime);
                rb.AddForce(dragForce);
            }
        }

        private void HandleSteering()
        {
            float speed = rb.linearVelocity.magnitude;
            float speedSteerFactor;

            // 静止時または極低速時のステアリング制限
            if (speed < minSteeringSpeed)
            {
                // 速度が低すぎる場合はステアリングを無効化
                return;
            }

            // 低速時：速度がしきい値以下なら倍率を lowSpeedSteerMultiplier から 1 へ補間
            if (speed <= lowSpeedThreshold)
            {
                float t = speed / lowSpeedThreshold; // 0～1
                speedSteerFactor = Mathf.Lerp(lowSpeedSteerMultiplier, 1f, t);
            }
            else
            {
                // しきい値以上：速度に応じて 1 から minSteerAtMaxSpeed へ補間
                float t = (speed - lowSpeedThreshold) / (maxSpeed - lowSpeedThreshold);
                speedSteerFactor = Mathf.Lerp(1f, minSteerAtMaxSpeed, Mathf.Clamp01(t));
            }

            // アクセル入力時の追加減衰
            float throttleFactor = 1f - Mathf.Abs(throttleInput) * throttleSteerReduction;

            // 最終的なステアリング角度
            float steerAmount = steeringInput * maxSteerAngle * speedSteerFactor * throttleFactor;
            Quaternion deltaRot = Quaternion.Euler(0f, steerAmount * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * deltaRot);
        }

        private void HandleDrift()
        {
            float speed = rb.linearVelocity.magnitude;
            float lateralSpeed = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.right));
            float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

            // ドリフト条件の判定
            bool shouldDrift = speed > driftSpeedThreshold &&
                               lateralSpeed > speed * 0.3f &&
                               Mathf.Abs(steeringInput) > 0.1f &&
                               throttleInput > 0.1f;

            isDrifting = shouldDrift;

            if (isDrifting)
            {
                // ドリフト時の挙動
                Vector3 forwardVel = transform.forward * forwardSpeed;
                Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

                // 後輪スリップをシミュレート
                Vector3 rearWheelForce = -lateralVel * (rearWheelSlip * accelerationForce * Time.fixedDeltaTime);
                rb.AddForce(rearWheelForce);

                // 前輪グリップをシミュレート
                Vector3 frontWheelForce = lateralVel * ((1f - frontWheelGrip) * accelerationForce * Time.fixedDeltaTime);
                rb.AddForce(frontWheelForce);

                // 横方向速度の保持
                rb.linearVelocity = forwardVel + lateralVel * driftFactor;
            }
            else
            {
                // 通常走行時の横方向速度減衰（速度に応じて変化）
                Vector3 forwardVel = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
                Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

                // 速度が高いほど横方向速度を保持
                float lateralDecay = Mathf.Lerp(0.95f, 0.98f, Mathf.Clamp01(speed / maxSpeed));
                Vector3 vel = forwardVel + lateralVel * lateralDecay;
                vel.y = 0f;
                rb.linearVelocity = vel;
            }
        }

        private void UpdateMillRotation()
        {
            float targetRotationPower = 0f;
            float speed = rb.linearVelocity.magnitude;

            if (isDrifting)
            {
                // ドリフト時は最高速度で回転
                targetRotationPower = driftMillRotationPower;
            }
            else if (Mathf.Abs(steeringInput) > 0.1f && speed > 1f)
            {
                // 旋回時
                targetRotationPower = steeringMillRotationPower * Mathf.Abs(steeringInput);
            }
            else if (throttleInput > 0.1f && speed > 0.5f)
            {
                // 前進時
                targetRotationPower = forwardMillRotationPower * throttleInput;
            }

            // 回転パワーの滑らかな変化
            currentMillRotationPower = Mathf.Lerp(currentMillRotationPower, targetRotationPower, Time.fixedDeltaTime * millRotationDecay);
            
            // 極小値を0に丸める（浮動小数点数の精度問題を解決）
            if (Mathf.Abs(currentMillRotationPower) < 0.001f)
            {
                currentMillRotationPower = 0f;
            }
            
            // 小数点第3位以下を切り捨て
            currentMillRotationPower = Mathf.Floor(currentMillRotationPower * 100f) / 100f;

            // シェーダーのRotatePowerプロパティに値を設定
            if (millMaterial != null)
            {
                millMaterial.SetFloat(RotatePowerRef, currentMillRotationPower);
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
                float grindSpeedMultiplier = currentMillRotationPower; // 0-1の値をそのまま使用
                float grindInterval = 1f / (GamePlayMode.Shared.Settings.BaseMillGrindSpeed * grindSpeedMultiplier);

                if (grindTimer >= grindInterval)
                {
                    int beansToGrind = Mathf.Min(1, GamePlayMode.Shared.PlayerState.GroundBeans);
                    GamePlayMode.Shared.PlayerState.ConsumeGroundBeans(beansToGrind);
                    GamePlayMode.Shared.PlayerState.AddGroundCoffee(beansToGrind);
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
            if (GamePlayMode.Shared.PlayerState.IsSpilling)
            {
                spillTimer += Time.fixedDeltaTime;
                float spillInterval = 1f / GamePlayMode.Shared.Settings.SpillSpeed;

                if (spillTimer >= spillInterval)
                {
                    GamePlayMode.Shared.PlayerState.RemoveGroundCoffee(1);
                    spillTimer = 0f;
                }
            }
            else
            {
                spillTimer = 0f;
            }
        }
    }
}