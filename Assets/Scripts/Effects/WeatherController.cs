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
        }

        /// <summary>Appelé par EffectsSync via OnEffectReceived.</summary>
        public void TriggerWeather(string effectName)
        {
            if (effectName != "Weather") return;

            m_isRaining = !m_isRaining;
            SetRain(m_isRaining);
            Debug.Log($"[WeatherController] {(m_isRaining ? "🌧️ Pluie" : "☀️ Temps clair")}");
        }

        private void SetRain(bool active)
        {
            if (m_rainParticles == null) return;
            if (active) m_rainParticles.Play();
            else m_rainParticles.Stop();
        }
    }
}
