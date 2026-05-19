using UnityEngine;

namespace ActionGame.Effects
{
    /// <summary>
    /// Contrôle le cycle jour/nuit.
    /// Branché sur le levier de la cabine Réalisateur.
    /// Jour : lumière soleil forte, ciel clair.
    /// Nuit : lumière soleil faible, ambiance sombre.
    /// </summary>
    public class DayNightController : MonoBehaviour
    {
        [Header("Lumière directionnelle (Soleil)")]
        [SerializeField] private Light m_sunLight;

        [Header("Paramètres Jour")]
        [SerializeField] private Color m_dayColor    = new Color(1f, 0.95f, 0.8f);
        [SerializeField] private float m_dayIntensity = 1.2f;
        [SerializeField] private Vector3 m_dayRotation = new Vector3(50f, -30f, 0f);

        [Header("Paramètres Nuit")]
        [SerializeField] private Color m_nightColor    = new Color(0.1f, 0.1f, 0.3f);
        [SerializeField] private float m_nightIntensity = 0.05f;
        [SerializeField] private Vector3 m_nightRotation = new Vector3(50f, -30f, 0f);

        [Header("Transition")]
        [SerializeField] private float m_transitionSpeed = 1f;

        private bool m_isDay = true;
        private Color m_targetColor;
        private float m_targetIntensity;
        private Vector3 m_targetRotation;

        private void Start()
        {
            if (m_sunLight == null)
                m_sunLight = FindObjectOfType<Light>();

            ApplyImmediate(isDay: true);
        }

        private void Update()
        {
            if (m_sunLight == null) return;

            // Transition fluide vers la cible
            m_sunLight.color     = Color.Lerp(m_sunLight.color, m_targetColor, Time.deltaTime * m_transitionSpeed);
            m_sunLight.intensity = Mathf.Lerp(m_sunLight.intensity, m_targetIntensity, Time.deltaTime * m_transitionSpeed);
            m_sunLight.transform.rotation = Quaternion.Slerp(
                m_sunLight.transform.rotation,
                Quaternion.Euler(m_targetRotation),
                Time.deltaTime * m_transitionSpeed
            );
        }

        /// <summary>Appelé par EffectsSync.OnEffectReceived — reçoit "DayNight_On" ou "DayNight_Off".</summary>
        public void TriggerDayNight(string effectName)
        {
            if (effectName == "DayNight_On")  SetDayNight(true);
            if (effectName == "DayNight_Off") SetDayNight(false);
        }

        /// <summary>Appelé par Lever.OnToggled — true = jour, false = nuit.</summary>
        public void SetDayNight(bool isDay)
        {
            m_isDay = isDay;
            m_targetColor     = isDay ? m_dayColor     : m_nightColor;
            m_targetIntensity = isDay ? m_dayIntensity : m_nightIntensity;
            m_targetRotation  = isDay ? m_dayRotation  : m_nightRotation;

            Debug.Log($"[DayNightController] {(isDay ? "☀️ Jour" : "🌙 Nuit")}");
        }

        private void ApplyImmediate(bool isDay)
        {
            m_targetColor     = isDay ? m_dayColor     : m_nightColor;
            m_targetIntensity = isDay ? m_dayIntensity : m_nightIntensity;
            m_targetRotation  = isDay ? m_dayRotation  : m_nightRotation;
        }
    }
}
