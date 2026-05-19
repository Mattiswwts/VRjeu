using UnityEngine;
using UnityEngine.Events;
using Ubiq.Messaging;

namespace ActionGame.Networking
{
    /// <summary>
    /// Synchronise les effets entre les 2 joueurs via Ubiq.
    /// Le Réalisateur déclenche un effet → message envoyé → tous l'appliquent.
    /// À attacher sur EffectsManager.
    /// </summary>
    public class EffectsSync : MonoBehaviour
    {
        [Header("Événement reçu par les controllers d'effets")]
        [Tooltip("Branché sur LightController, SoundController, WeatherController")]
        public UnityEvent<string> OnEffectReceived;

        private NetworkContext m_context;

        private struct EffectMessage
        {
            public string effectName;
        }

        private void Start()
        {
            m_context = NetworkScene.Register(this);

            // Branche automatiquement tous les boutons de la scène sans passer par l'Inspector
            foreach (var btn in FindObjectsByType<ActionGame.Objects.InteractableButton>(FindObjectsSortMode.None))
            {
                btn.OnPressed.AddListener(SendEffect);
                Debug.Log($"[EffectsSync] Bouton auto-connecté : {btn.effectName}");
            }
        }

        /// <summary>
        /// Appelé par InteractableButton.OnPressed — envoie l'effet à tous via Ubiq.
        /// </summary>
        public void SendEffect(string effectName)
        {
            Debug.Log($"[EffectsSync] Envoi effet : {effectName}");
            m_context.SendJson(new EffectMessage { effectName = effectName });
            OnEffectReceived?.Invoke(effectName);
        }

        /// <summary>
        /// Appelé par Lever.OnToggled — envoie le jour/nuit à tous via Ubiq.
        /// </summary>
        public void SendDayNight(bool isDay)
        {
            string effectName = isDay ? "DayNight_On" : "DayNight_Off";
            Debug.Log($"[EffectsSync] Envoi : {effectName}");
            m_context.SendJson(new EffectMessage { effectName = effectName });
            OnEffectReceived?.Invoke(effectName);
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var msg = message.FromJson<EffectMessage>();
            Debug.Log($"[EffectsSync] Effet reçu : {msg.effectName}");
            OnEffectReceived?.Invoke(msg.effectName);
        }
    }
}
