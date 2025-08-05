using Interface;
using Main;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour, IPlayer
    {
        /// <summary>
        /// 車の最大前方速度（メートル/秒）。
        /// </summary>
        [Header("Settings"), SerializeField]
        private float maxSpeed = 10f; // 車の最大前方速度（m/s）

        /// <summary>
        /// 加速時に適用される力。値が大きいほど加速が強くなる。
        /// </summary>
        [SerializeField]
        private float accelerationForce = 1100f; // 加速時に適用される力

        /// <summary>
        /// 1秒あたりの最大ステアリング角度（度）。
        /// </summary>
        [SerializeField]
        private float maxSteerAngle = 60f; // 最大ステアリング角度（度/秒）

        [Tooltip("低速時に適用するステアリング倍率")]
        [SerializeField]
        private float lowSpeedSteerMultiplier = 1.5f; // 低速時のステアリング倍率（>1）

        [Tooltip("低速と判断する速度のしきい値")]
        [SerializeField]
        private float lowSpeedThreshold = 2f; // 低速しきい値（m/s）

        [Tooltip("最高速到達時の最小ステアリング倍率")]
        [SerializeField, Range(0f, 1f)]
        private float minSteerAtMaxSpeed = 0.2f; // 高速時の最小ステアリング倍率

        [Tooltip("アクセル入力時のステアリング減衰率")]
        [SerializeField, Range(0f, 1f)]
        private float throttleSteerReduction = 0.5f;

        [Tooltip("ドリフト時の横方向速度保持率")]
        [SerializeField, Range(0f, 1f)]
        private float driftFactor = 0.85f; // ドリフト時の横滑り量

        private Rigidbody rb;
        private float steeringInput;
        private float throttleInput;
        public Transform Transform => transform;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            // X軸とZ軸の回転を固定し、車が横転するのを防止
            // リニアダンピングを設定し、入力がないときに徐々に減速
            rb.linearDamping = 0.14f;
            // 角ダンピングを設定し、回転の変化を滑らかに制御
            rb.angularDamping = 0.8f;
            GamePlayMode.Shared.OnPlayerSpawn(this);
        }

        private void FixedUpdate()
        {
            // 入力を取得: 縦(forward/back)と横(left/right)
            throttleInput = Input.GetAxis("Vertical");
            steeringInput = Input.GetAxis("Horizontal");

            HandleSteering();
            HandleAcceleration();
            ApplyDrift();
        }

        private void HandleAcceleration()
        {
            Vector3 forward = transform.forward;
            float forwardVel = Vector3.Dot(rb.linearVelocity, forward);

            if (throttleInput > 0 && forwardVel < maxSpeed)
            {
                rb.AddForce(forward * throttleInput * accelerationForce * Time.fixedDeltaTime);
            }
            else if (throttleInput < 0)
            {
                rb.AddForce(forward * throttleInput * accelerationForce * Time.fixedDeltaTime);
            }
        }

        private void HandleSteering()
        {
            float speed = rb.linearVelocity.magnitude;
            float speedSteerFactor;

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

        private void ApplyDrift()
        {
            Vector3 forwardVel = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
            Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
            rb.linearVelocity = forwardVel + lateralVel * driftFactor;
        }
    }
}
