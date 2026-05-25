using UnityEngine;

namespace ActionGame.Effects
{
    /// <summary>
    /// Contrôle la météo de la scène.
    /// Cycle : Rien → Pluie → Rien → ...
    /// </summary>
    public class WeatherController : MonoBehaviour
    {
        [Header("Pluie")]
        [SerializeField] private ParticleSystem m_rainParticles;

        private bool m_isRaining = false;

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

        private bool m_busy = false;

        /// <summary>Appelé par EffectsSync via OnEffectReceived.</summary>
        public void TriggerWeather(string effectName)
        {
            if (effectName != "Weather") return;
            if (m_busy) return;
            m_busy = true;
            StartCoroutine(ResetBusy());

            m_isRaining = !m_isRaining;
            SetRain(m_isRaining);
            Debug.Log($"[WeatherController] {(m_isRaining ? "Pluie" : "Temps clair")}");
        }

        private System.Collections.IEnumerator ResetBusy()
        {
            yield return null;
            m_busy = false;
        }

        private void SetRain(bool active)
        {
            if (m_rainParticles == null) return;
            if (active) m_rainParticles.Play();
            else m_rainParticles.Stop();
        }
    }
}
