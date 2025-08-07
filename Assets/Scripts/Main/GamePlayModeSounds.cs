using Component;
using UnityEngine;

namespace Main
{
    public partial class GamePlayMode
    {
        [SerializeField] private SoundSettings soundSettings;
        [SerializeField] private GameObject singleAudioSourcePrefab;
        #region Sound Methods

        /// <summary>
        /// コーヒー豆を轢いた時の音を再生（連続再生対応）
        /// </summary>
        public void PlayBeansBreakSound(Transform _transform)
        {
            if (soundSettings == null)
            {
                Debug.LogWarning("SoundSettings is not assigned.");
                return;
            }
            var audioSource = Instantiate(singleAudioSourcePrefab, _transform.position, Quaternion.identity).GetComponent<SingleAudioSource>();
            if (soundSettings != null && audioSource)
            {
                var clip = soundSettings.BeansBreak[Random.Range(0, soundSettings.BeansBreak.Count)];
                audioSource.PlaySound(clip, true);
            }
            else
            {
                Debug.LogWarning("SingleAudioSource component not found on the instantiated prefab.");
            }
        }

        /// <summary>
        /// 障害物にぶつかった時の音を再生
        /// </summary>
        public void PlayHitObstacleSound(Transform _transform)
        {
            if (soundSettings != null)
            {
                var audioSource = Instantiate(singleAudioSourcePrefab, _transform.position, Quaternion.identity).GetComponent<SingleAudioSource>();
                if (audioSource != null)
                {
                    var clip = soundSettings.HitObstacle[Random.Range(0, soundSettings.HitObstacle.Count)];
                    audioSource.PlaySound(clip, true);
                }
                else
                {
                    Debug.LogWarning("SingleAudioSource component not found on the instantiated prefab.");
                }
            }
            else
            {
                Debug.LogWarning("SoundSettings is not assigned.");
            }
        }

        /// <summary>
        /// コーヒー完成時の音を再生
        /// </summary>
        public void PlayMakeCoffeeSound(AudioSource _source)
        {
            if (soundSettings != null)
            {
                soundSettings.PlayMakeCoffeeSound(_source);
            }
        }

        /// <summary>
        /// ミル回転音の開始
        /// </summary>
        public void StartMillSound(AudioSource _source)
        {
            if (soundSettings != null && _source != null)
            {
                var millSound = soundSettings.Mill[Random.Range(0, soundSettings.Mill.Count)];
                _source.clip = millSound;
                _source.loop = true;
                if (!_source.isPlaying)
                {
                    _source.Play();
                }
            }
        }

        /// <summary>
        /// ミル回転音の停止
        /// </summary>
        public void StopMillSound(AudioSource _source)
        {
            if (soundSettings != null && _source != null && _source.isPlaying)
            {
                _source.Stop();
            }
        }

        #endregion
    }
}