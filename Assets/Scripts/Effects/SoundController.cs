using UnityEngine;

namespace ActionGame.Effects
{
    /// <summary>
    /// Joue des sons déclenchés par le Réalisateur.
    /// Appelé par DirectorController quand il appuie sur 2.
    /// </summary>
    public class SoundController : MonoBehaviour
    {
        [Header("Sons disponibles")]
        [SerializeField] private AudioClip[] m_clips;

        private AudioSource m_audioSource;
        private int m_clipIndex = 0;

        private void Awake()
        {
            m_audioSource = GetComponent<AudioSource>();
            if (m_audioSource == null)
                m_audioSource = gameObject.AddComponent<AudioSource>();
        }

        /// <summary>Appelé par DirectorController via OnEffectTriggered.</summary>
        public void TriggerSound(string effectName)
        {
            if (effectName != "Sound") return;

            if (m_clips == null || m_clips.Length == 0)
            {
                Debug.LogWarning("[SoundController] Aucun clip audio assigné !");
                return;
            }

            // Joue le son suivant dans la liste (cycle)
            var clip = m_clips[m_clipIndex];
            m_clipIndex = (m_clipIndex + 1) % m_clips.Length;

            m_audioSource.PlayOneShot(clip);
            Debug.Log($"[SoundController] Son joué : {clip.name}");
        }
    }
}
