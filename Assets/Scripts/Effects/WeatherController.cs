using UnityEngine;

namespace ActionGame.Effects
{
    /// <summary>
    /// Contrôle la météo de la scène.
    /// Cycle : Rien → Pluie → Rien → ...
    /// </summary>
    public class WeatherController : MonoBehaviour
    {
        [Header("Pluie — Visuel")]
        [SerializeField] private ParticleSystem m_rainParticles;

        [Header("Pluie — Son (optionnel)")]
        [Tooltip("AudioSource avec le clip de pluie. Laisse vide si géré ailleurs.")]
        [SerializeField] private AudioSource m_rainAudio;

        private bool m_isRaining = false;
        private bool m_busy = false;

        private void Start()
        {
            SetRain(false);

            var effectsSync = FindFirstObjectByType<ActionGame.Networking.EffectsSync>();
            if (effectsSync != null)
            {
                effectsSync.OnEffectReceived.RemoveListener(TriggerWeather);
                effectsSync.OnEffectReceived.AddListener(TriggerWeather);
                Debug.Log("[WeatherController] ✅ Connecté à EffectsSync");
            }
        }

        /// <summary>
        /// Déclenché par le bouton "Weather" du Réalisateur.
        /// Active/désactive les particules ET le son de pluie ensemble.
        /// </summary>
        public void TriggerWeather(string effectName)
        {
            if (effectName != "Weather") return;
            if (m_busy) return;
            m_busy = true;
            StartCoroutine(ResetBusy());

            m_isRaining = !m_isRaining;
            SetRain(m_isRaining);
            Debug.Log($"[WeatherController] {(m_isRaining ? "🌧 Pluie" : "☀ Temps clair")}");
        }

        private System.Collections.IEnumerator ResetBusy()
        {
            yield return null;
            m_busy = false;
        }

        private void SetRain(bool active)
        {
            // Particules
            if (m_rainParticles != null)
            {
                if (active) m_rainParticles.Play();
                else        m_rainParticles.Stop();
            }

            // Son
            if (m_rainAudio != null)
            {
                if (active) m_rainAudio.Play();
                else        m_rainAudio.Stop();
            }
        }
    }
}
