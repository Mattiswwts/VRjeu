using UnityEngine;
using UnityEngine.Events;

namespace ActionGame.Objects
{
    /// <summary>
    /// Levier 3D interactable par raycast.
    /// Se penche en avant/arrière selon son état.
    /// Déclenche un UnityEvent au changement d'état.
    /// </summary>
    public class Lever : MonoBehaviour
    {
        [Header("Animation")]
        [Tooltip("Rotation du levier quand en position ON")]
        [SerializeField] private float m_onAngle = 30f;
        [Tooltip("Rotation du levier quand en position OFF")]
        [SerializeField] private float m_offAngle = -30f;
        [SerializeField] private float m_animSpeed = 5f;

        [Header("Événement")]
        public UnityEvent<bool> OnToggled; // true = ON, false = OFF

        private bool m_isOn = false;
        private float m_targetAngle;

        private void Start()
        {
            m_targetAngle = m_offAngle;
        }

        private void Update()
        {
            // Animation fluide du levier autour de Z
            float current = transform.localEulerAngles.z;
            float newAngle = Mathf.LerpAngle(current, m_targetAngle, Time.deltaTime * m_animSpeed);
            transform.localEulerAngles = new Vector3(0f, 0f, newAngle);
        }

        /// <summary>Appelé par DirectorInteraction quand le raycast touche ce levier.</summary>
        public void Toggle()
        {
            m_isOn = !m_isOn;
            m_targetAngle = m_isOn ? m_onAngle : m_offAngle;
            Debug.Log($"[Lever] Levier {(m_isOn ? "ON" : "OFF")}");
            OnToggled?.Invoke(m_isOn);
        }
    }
}
