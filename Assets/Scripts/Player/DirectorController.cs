using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ActionGame.Player
{
    /// <summary>
    /// Contrôle le Réalisateur en mode desktop.
    /// WASD + souris pour se déplacer dans la cabine.
    /// 1 → Lumière | 2 → Son | 3 → Météo
    /// </summary>
    public class DirectorController : MonoBehaviour
    {
        [Header("Déplacement")]
        [SerializeField] private float m_moveSpeed = 5f;
        [SerializeField] private float m_mouseSensitivity = 0.2f;

        [Header("Événements")]
        public UnityEvent<string> OnEffectTriggered;

        private CharacterController m_charController;
        private Camera m_camera;
        private float m_cameraPitch = 0f;

        private void Awake()
        {
            m_charController = GetComponent<CharacterController>();
            m_camera = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            HandleMouseLook();
            HandleMovement();
            HandleEffects();
        }

        private void HandleMouseLook()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 delta = mouse.delta.ReadValue() * m_mouseSensitivity;
            transform.Rotate(Vector3.up, delta.x);

            m_cameraPitch -= delta.y;
            m_cameraPitch = Mathf.Clamp(m_cameraPitch, -80f, 80f);
            if (m_camera != null)
                m_camera.transform.localRotation = Quaternion.Euler(m_cameraPitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

            Vector3 move = transform.right * h + transform.forward * v;
            m_charController.Move(move * m_moveSpeed * Time.deltaTime);
            m_charController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }

        private void HandleEffects()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame) TriggerEffect("Light");
            if (kb.digit2Key.wasPressedThisFrame) TriggerEffect("Sound");
            if (kb.digit3Key.wasPressedThisFrame) TriggerEffect("Weather");
        }

        private void TriggerEffect(string effectName)
        {
            Debug.Log($"[DirectorController] Effet déclenché : {effectName}");
            OnEffectTriggered?.Invoke(effectName);
        }
    }
}
