using System.Collections;
using Interface;
using Main;
using UnityEngine;
using UnityEngine.VFX;

namespace Component
{
    public class CrushBeanEffectPlayer: MonoBehaviour
    {
        [SerializeField] private VisualEffect crushEffect;

        private float playerSpeedRate;
        public void Play(ICoffeeBean _bean, IPlayer _player)
        {
            transform.position = _bean.Transform.position;
            if (crushEffect != null)
            {
                playerSpeedRate = _player.Speed / _player.MaxSpeed;
                StartCoroutine(Start());
            }

        }

        private IEnumerator Start()
        {
            var baseVelocitySpeed = GamePlayMode.Shared.Settings.CrushBaseDiffusion;
            var rateVelocitySpeed = GamePlayMode.Shared.Settings.CrushDiffusionByPlayerSpeedRate;
            crushEffect.SetFloat("SpawnRate", Mathf.Clamp((int)(100 * playerSpeedRate), 0, 100));
            crushEffect.SetFloat("MinVelocitySpeed", baseVelocitySpeed + playerSpeedRate * rateVelocitySpeed);
            crushEffect.SetFloat("MaxVelocitySpeed", baseVelocitySpeed + 1.2f + playerSpeedRate * rateVelocitySpeed);
            crushEffect.SetFloat("BaseSize", (int)(10 * playerSpeedRate));
            crushEffect.SendEvent("OnPlay");
            yield return new WaitForSeconds(1f);
            crushEffect.SendEvent("OnStop");
            Destroy(gameObject);
        }
    }
}