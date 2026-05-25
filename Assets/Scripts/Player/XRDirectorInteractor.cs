using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using ActionGame.Objects;

namespace ActionGame.Player
{
    /// <summary>
    /// À attacher sur l'XRRayInteractor de la main droite du Réalisateur.
    ///
    /// Quand il pointe vers un bouton/levier et appuie sur trigger :
    ///   - InteractableButton → Press()
    ///   - Lever → Toggle()
    ///
    /// Setup :
    ///   1. Sur XROrigin du Réalisateur → RightHand Controller → ajoute ce script
    ///   2. Les boutons et leviers doivent avoir XRSimpleInteractable
    /// </summary>
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor))]
    public class XRDirectorInteractor : MonoBehaviour
    {
        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor m_interactor;

        private void Awake()
        {
            m_interactor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();
        }

        private void OnEnable()
        {
            m_interactor.selectEntered.AddListener(OnSelectEntered);
        }

        private void OnDisable()
        {
            m_interactor.selectEntered.RemoveListener(OnSelectEntered);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            var go = args.interactableObject.transform.gameObject;

            // Bouton 3D
            var button = go.GetComponent<InteractableButton>()
                      ?? go.GetComponentInParent<InteractableButton>();
            if (button != null)
            {
                button.Press();
                return;
            }

            // Levier
            var lever = go.GetComponent<Lever>()
                     ?? go.GetComponentInParent<Lever>();
            if (lever != null)
            {
                lever.Toggle();
            }
        }
    }
}
