using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        /// <summary>
        /// 車の最大前方速度（メートル/秒）。
        /// </summary>
        [Header("Settings"), SerializeField]
        private float maxSpeed = 20f; // in m/s

        /// <summary>
        /// 加速時に適用される力。値が大きいほど加速が強くなる。
        /// </summary>
        [SerializeField]
        private float accelerationForce = 1500f; // Force applied when accelerating

        /// <summary>
        /// 1秒あたりの最大ステアリング角度（度）。
        /// </summary>
        [SerializeField]
        private float maxSteerAngle = 45f; // degrees per second

        /// <summary>
        /// スロットル入力時のステアリング減衰率。
        /// 範囲: 0.0（減衰なし）〜1.0（完全減衰）。
        /// 0.0 → アクセル時にステアリング減衰なし
        /// 0.5 → 全開時に半分のステアリング性能
        /// 1.0 → 全開時にステアリング無効
        /// </summary>
        [SerializeField, Range(0f, 1f)]
        private float throttleSteerReduction = 0.5f;

        /// <summary>
        /// ドリフト時の横方向速度保持率。
        /// 範囲: 0.0（完全横滑り）〜1.0（横滑りなし）。
        /// 0.0 → 横滑り100%
        /// 1.0 → ドリフトなし
        /// </summary>
        [SerializeField, Range(0f, 1f)]
        private float driftFactor = 0.85f;

        private Rigidbody rb;
        private float steeringInput;
        private float throttleInput;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            // X軸とZ軸の回転を固定し、車が横転するのを防止
            // リニアダンピングを設定し、入力がないときに徐々に減速
            rb.linearDamping = 0.14f;
            // 角ダンピングを設定し、回転の変化を滑らかに制御
            rb.angularDamping = 0.8f;
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

        /// <summary>
        /// 前後方向に力を加え、前方速度をmaxSpeed以下に制限する
        /// </summary>
        private void HandleAcceleration()
        {
            Vector3 forward = transform.forward;
            // 現在の速度を前方向に投影して取得
            float forwardVel = Vector3.Dot(rb.linearVelocity, forward);

            if (throttleInput > 0 && forwardVel < maxSpeed)
            {
                // 前方に力を加える
                rb.AddForce(forward * throttleInput * accelerationForce * Time.fixedDeltaTime);
            }
            else if (throttleInput < 0)
            {
                // 後退またはブレーキ時に力を加える
                rb.AddForce(forward * throttleInput * accelerationForce * Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Y軸回りに車を回転させる。速度とスロットル入力に応じて操舵性能を減衰する
        /// </summary>
        private void HandleSteering()
        {
            // 速度比でステアリング感度を減衰
            float speedRatio = rb.linearVelocity.magnitude / maxSpeed;
            // スロットル入力時のステアリング減衰率を適用
            float steerFactor = 1f - Mathf.Abs(throttleInput) * throttleSteerReduction;
            // 回転量を計算
            float steerAmount = steeringInput * maxSteerAngle * (1f - speedRatio) * steerFactor;
            Quaternion deltaRot = Quaternion.Euler(0f, steerAmount * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * deltaRot);
        }

        /// <summary>
        /// 横方向の速度成分を分離し、driftFactorで減衰させてドリフトをシミュレート
        /// </summary>
        private void ApplyDrift()
        {
            Vector3 forwardVel = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
            Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
            // 横方向の速度をdriftFactor分だけ残す
            rb.linearVelocity = forwardVel + lateralVel * driftFactor;
        }
    }
}
