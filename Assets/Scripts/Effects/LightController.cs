using UnityEngine;

namespace ActionGame.Effects
{
    /// <summary>
    /// Contrôle les lumières de la scène.
    /// Appelé par DirectorController quand le Réalisateur appuie sur 1.
    /// Toggle allumé/éteint + changement de couleur aléatoire.
    /// </summary>
    public class LightController : MonoBehaviour
    {
        [Header("Lumières à contrôler")]
        [SerializeField] private Light[] m_lights;

        [Header("Couleurs disponibles")]
        [SerializeField] private Color[] m_colors = new Color[]
        {
            Color.white,
            Color.red,
            Color.blue,
            Color.green,
            new Color(1f, 0.5f, 0f), // orange
        };

        private bool m_isOn = true;
        private int m_colorIndex = 0;

        /// <summary>Appelé par DirectorController via OnEffectTriggered.</summary>
        public void TriggerLight(string effectName)
        {
            if (effectName != "Light") return;

            m_isOn = !m_isOn;

            foreach (var light in m_lights)
            {
                if (light == null) continue;
                light.enabled = m_isOn;

                if (m_isOn)
                {
                    // Change de couleur à chaque allumage
                    m_colorIndex = (m_colorIndex + 1) % m_colors.Length;
                    light.color = m_colors[m_colorIndex];
                }
            }

            Debug.Log($"[LightController] Lumières {(m_isOn ? "allumées" : "éteintes")} — couleur : {m_colors[m_colorIndex]}");
        }
    }
}
