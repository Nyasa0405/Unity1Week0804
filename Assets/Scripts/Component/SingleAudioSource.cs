using System.Collections;
using UnityEngine;

namespace Component
{
    public class SingleAudioSource: MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        /// <summary>
        /// 音声を再生
        /// </summary>
        /// <param name="_clip">再生する音声クリップ</param>
        public void PlaySound(AudioClip _clip, bool _destroyOnFinish = false)
        {
            if (audioSource == null || _clip == null)
                return;

            // 既に再生中の音声を停止
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.clip = _clip;
            audioSource.Play();

            // 再生後にオブジェクトを破棄する場合
            if (_destroyOnFinish)
            {
                StartCoroutine(DestroyAfterClipEnds(_clip.length + 1f));
            }
        }

        private IEnumerator DestroyAfterClipEnds(float _duration)
        {
            yield return new WaitForSeconds(_duration);
            Destroy(gameObject);
        }
    }
}