using UnityEngine;
using UnityEngine.Events;

namespace ActionGame.Objects
{
    /// <summary>
    /// Bouton 3D cliquable par raycast.
    /// S'enfonce visuellement quand on clique dessus.
    /// Déclenche un UnityEvent — branché sur LightController, SoundController, etc.
    /// </summary>
    public class InteractableButton : MonoBehaviour
    {
        [Header("Effet visuel")]
        [Tooltip("Distance d'enfoncement du bouton quand appuyé")]
        [SerializeField] private float m_pressDepth = 0.05f;
        [Tooltip("Vitesse de l'animation d'enfoncement")]
        [SerializeField] private float m_pressSpeed = 10f;

        [Header("Effet déclenché")]
        public string effectName; // "Light", "Sound", "Weather"
        public UnityEvent<string> OnPressed;

        private Vector3 m_restPosition;
        private Vector3 m_pressedPosition;
        private bool m_isPressed = false;

        private void Start()
        {
            m_restPosition    = transform.localPosition;
            m_pressedPosition = m_restPosition - new Vector3(0, m_pressDepth, 0);
        }

        private void Update()
        {
            // Animation : enfonce ou remonte selon l'état
            Vector3 target = m_isPressed ? m_pressedPosition : m_restPosition;
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * m_pressSpeed);

            // Relâche automatiquement après 0.2s
            if (m_isPressed)
            {
                m_pressTimer += Time.deltaTime;
                if (m_pressTimer > 0.2f)
                {
                    m_isPressed = false;
                    m_pressTimer = 0f;
                }
            }
        }

        private float m_pressTimer = 0f;

        /// <summary>Appelé par DirectorInteraction quand le raycast touche ce bouton.</summary>
        public void Press()
        {
            if (m_isPressed) return; // évite le double-clic

            m_isPressed = true;
            m_pressTimer = 0f;
            Debug.Log($"[InteractableButton] Bouton pressé : {effectName}");
            OnPressed?.Invoke(effectName);
        }
    }
}
