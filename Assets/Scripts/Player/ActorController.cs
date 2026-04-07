using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ActionGame.Player
{
    /// <summary>
    /// Contrôle l'Acteur en mode desktop.
    /// WASD pour se déplacer, clic gauche pour interagir avec les objets.
    /// </summary>
    public class ActorController : MonoBehaviour
    {
        [Header("Déplacement")]
        [SerializeField] private float m_moveSpeed = 5f;

        [Header("Caméra")]
        [SerializeField] private float m_mouseSensitivity = 0.2f;

        [Header("Interaction")]
        [Tooltip("Distance max pour interagir avec un objet")]
        [SerializeField] private float m_interactRange = 10f;
        [SerializeField] private LayerMask m_interactableLayer;

        [Header("Événements")]
        [Tooltip("Déclenché quand l'acteur effectue une action (envoyé au SyncChecker)")]
        public UnityEvent<string> OnActionExecuted;

        private CharacterController m_charController;
        private Camera m_camera;
        private float m_cameraPitch = 0f; // rotation verticale de la caméra

        private void Awake()
        {
            m_charController = GetComponent<CharacterController>();
            // Cherche la caméra enfant du joueur
            m_camera = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            HandleMouseLook();
            HandleMovement();
            HandleInteraction();
        }

        // ─── Rotation caméra (souris) ────────────────────────────────────────

        private void HandleMouseLook()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 delta = mouse.delta.ReadValue() * m_mouseSensitivity;

            // Rotation horizontale : tourne le corps du joueur
            transform.Rotate(Vector3.up, delta.x);

            // Rotation verticale : tourne seulement la caméra (clampée)
            m_cameraPitch -= delta.y;
            m_cameraPitch = Mathf.Clamp(m_cameraPitch, -80f, 80f);
            if (m_camera != null)
                m_camera.transform.localRotation = Quaternion.Euler(m_cameraPitch, 0f, 0f);
        }

        // ─── Déplacement WASD ────────────────────────────────────────────────

        private void HandleMovement()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

            Vector3 move = transform.right * h + transform.forward * v;
            m_charController.Move(move * m_moveSpeed * Time.deltaTime);

            // Gravité simple
            m_charController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }

        // ─── Interaction clic gauche ─────────────────────────────────────────

        private void HandleInteraction()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

            // Raycast depuis le centre de la caméra
            Ray ray = m_camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

            // Si aucun layer configuré, on prend tout
            int mask = m_interactableLayer == 0 ? ~0 : (int)m_interactableLayer;

            Debug.Log($"[ActorController] Clic détecté — raycast depuis caméra : {m_camera?.name}");

            if (Physics.Raycast(ray, out RaycastHit hit, m_interactRange, mask))
            {
                string objectName = hit.collider.gameObject.name;
                Debug.Log($"[ActorController] Action exécutée sur : {objectName}");
                OnActionExecuted?.Invoke(objectName);
            }
            else
            {
                Debug.Log($"[ActorController] Raycast : rien touché (range={m_interactRange})");
            }
        }
    }
}
