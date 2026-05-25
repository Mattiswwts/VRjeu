using UnityEngine;

namespace ActionGame.Effects
{
    public class LightController : MonoBehaviour
    {
        private Light[] m_lights;
        private bool m_isOn = false;

        private void Start()
        {
            m_lights = System.Array.FindAll(
                FindObjectsByType<Light>(FindObjectsSortMode.None),
                l => l.type == LightType.Spot
            );

            var effectsSync = FindFirstObjectByType<ActionGame.Networking.EffectsSync>();
            if (effectsSync != null)
            {
                // RemoveListener avant AddListener = garantit une seule souscription
                // (évite le double-fire si aussi câblé dans l'Inspector)
                effectsSync.OnEffectReceived.RemoveListener(TriggerLight);
                effectsSync.OnEffectReceived.AddListener(TriggerLight);
                Debug.Log("[LightController] ✅ Connecté à EffectsSync");
            }
        }

        public void TriggerLight(string effectName)
        {
            if (effectName != "Light") return;

            m_isOn = !m_isOn;

            foreach (var light in m_lights)
            {
                if (light == null) continue;
                light.color = Color.white;
                light.enabled = m_isOn;
            }

            Debug.Log($"[LightController] Lumières {(m_isOn ? "allumées" : "éteintes")}");
        }
    }
}
