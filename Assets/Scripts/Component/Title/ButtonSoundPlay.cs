using UnityEngine;
using UnityEngine.UI;

namespace Component.Title
{
    [RequireComponent(typeof(Button))]
    public class ButtonSoundPlay: MonoBehaviour
    {
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private GameObject singleAudioSourcePrefab;

        private Button button;
        private void Awake()
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("Button component not found on this GameObject.");
                return;
            }
            button.onClick.AddListener(OnButtonClick);
        }
        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }
        }
        private void OnButtonClick()
        {
            if (buttonClickSound == null || singleAudioSourcePrefab == null)
            {
                Debug.LogWarning("Button click sound or SingleAudioSource prefab is not assigned.");
                return;
            }

            // SingleAudioSourceをインスタンス化して音声を再生
            var audioSourceObject = Instantiate(singleAudioSourcePrefab, transform.position, Quaternion.identity);
            var audioSource = audioSourceObject.GetComponent<SingleAudioSource>();
            if (audioSource != null)
            {
                audioSource.PlaySound(buttonClickSound, true);
            }
            else
            {
                Debug.LogWarning("SingleAudioSource component not found on the instantiated prefab.");
            }
        }
    }
}