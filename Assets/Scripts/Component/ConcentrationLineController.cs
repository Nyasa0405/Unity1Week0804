using Main;
using UnityEngine;

namespace Component
{
    public class ConcentrationLineController: MonoBehaviour
    {
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");
        private static readonly int SpeedLimit = Shader.PropertyToID("_SpeedLimit");
        private static readonly int Speed = Shader.PropertyToID("_Speed");
        [SerializeField] private Material lineMaterial;

        private void Start()
        {
            lineMaterial.SetFloat(SpeedLimit, 1f);
            lineMaterial.SetFloat(Speed, 0.2f);
            lineMaterial.SetFloat(Alpha, 0f);
        }
        private void FixedUpdate()
        {
            float rate = 0f;
            if (GamePlayMode.Shared == null || GamePlayMode.Shared.Player == null)
            {
                return;
            }
            if (GamePlayMode.Shared.Player.Speed > 0.01f)
            {
                rate = GamePlayMode.Shared.Player.Speed / GamePlayMode.Shared.Player.MaxSpeed;
            }

            lineMaterial.SetFloat(Alpha, Mathf.Max(0f, rate - 0.8f));
            //
            // if (rate > 0.99f)
            // {
            //     lineMaterial.SetFloat(Speed, 5f);
            // }
            // else
            // {
            //     lineMaterial.SetFloat(Speed, 0.2f);
            // }
        }
    }
}