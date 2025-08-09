using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    [CreateAssetMenu(fileName = "SoundSettings", menuName = "Coffee Game/Sound Settings")]
    public class SoundSettings : ScriptableObject
    {
        [Header("Audio Clips")]
        [SerializeField] private List<AudioClip> beansBreak;
        [SerializeField] private List<AudioClip> hitObstacle;
        [SerializeField] private List<AudioClip> makeCoffee;
        [SerializeField] private List<AudioClip> mill;
        [SerializeField] private AudioClip click;
        [SerializeField] private AudioClip countdown;
        [SerializeField] private AudioClip startAndFinish;
        [Header("BGM")]
        [SerializeField] private AudioClip gameBGM;

        public List<AudioClip> BeansBreak => beansBreak;
        public List<AudioClip> HitObstacle => hitObstacle;
        public List<AudioClip> MakeCoffee => makeCoffee;
        public List<AudioClip> Mill => mill;
        public AudioClip Click => click;
        public AudioClip Countdown => countdown;
        public AudioClip StartAndFinish => startAndFinish;
        public AudioClip GameBGM => gameBGM;

        /// <summary>
        /// 障害物にぶつかった時の音をランダムに再生
        /// </summary>
        public void PlayHitObstacleSound(AudioSource _audioSource)
        {
            PlayRandomSound(_audioSource, hitObstacle);
        }

        /// <summary>
        /// コーヒー完成時の音をランダムに再生
        /// </summary>
        public void PlayMakeCoffeeSound(AudioSource _audioSource)
        {
            PlayRandomSound(_audioSource, makeCoffee);
        }

        /// <summary>
        /// 指定されたリストからランダムに音を選択して再生
        /// </summary>
        private void PlayRandomSound(AudioSource _audioSource, List<AudioClip> _clips)
        {
            if (_audioSource == null || _clips == null || _clips.Count == 0)
                return;

            // ランダムに音を選択
            AudioClip randomClip = _clips[Random.Range(0, _clips.Count)];
            
            // 音を再生
            _audioSource.PlayOneShot(randomClip);
        }
    }
}