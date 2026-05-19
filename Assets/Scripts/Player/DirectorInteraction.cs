using UnityEngine;
using UnityEngine.InputSystem;
using ActionGame.Objects;

namespace ActionGame.Player
{
    /// <summary>
    /// Gère l'interaction du Réalisateur avec les boutons 3D.
    /// Raycast depuis la caméra → clic gauche → appuie sur le bouton.
    /// À attacher sur Player_Director.
    /// </summary>
    public class DirectorInteraction : MonoBehaviour
    {
        [SerializeField] private float m_interactRange = 5f;

        private Camera m_camera;

        private void Awake()
        {
            m_camera = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

            Ray ray = m_camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

            if (Physics.Raycast(ray, out RaycastHit hit, m_interactRange))
            {
                var button = hit.collider.GetComponent<InteractableButton>();
                if (button != null) { button.Press(); return; }

                var lever = hit.collider.GetComponent<Lever>()
                         ?? hit.collider.GetComponentInParent<Lever>();
                if (lever != null) lever.Toggle();
            }
        }
    }
}
