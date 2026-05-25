using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using ActionGame.Objects;

namespace ActionGame.Player
{
    /// <summary>
    /// À attacher sur l'XRRayInteractor de la main droite de l'Acteur.
    ///
    /// Traduit les événements XRI (pointer + trigger) vers notre système :
    ///   - Si l'objet a IInteractable → Interact()
    ///   - Sinon → ActorControllerXR.ExecuteAction(nom de l'objet)
    ///
    /// Setup :
    ///   1. Sur XROrigin de l'Acteur → RightHand Controller → ajoute ce script
    ///   2. L'objet doit aussi avoir XRSimpleInteractable pour être détectable par XRI
    /// </summary>
    [RequireComponent(typeof(XRBaseInteractor))]
    public class XRActorInteractor : MonoBehaviour
    {
        private XRBaseInteractor m_interactor;
        private ActorControllerXR m_actor;

        private void Awake()
        {
            m_interactor = GetComponent<XRBaseInteractor>();
            // Cherche ActorControllerXR sur le parent (XROrigin)
            m_actor = GetComponentInParent<ActorControllerXR>();
            if (m_actor == null)
                Debug.LogWarning("[XRActorInteractor] ActorControllerXR introuvable dans les parents.");
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
            if (m_actor == null) return;

            var go = args.interactableObject.transform.gameObject;

            // Priorité 1 : IInteractable (PickupItem, HoldableItem, etc.)
            var interactable = go.GetComponent<IInteractable>()
                            ?? go.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                // On passe null car en VR l'objet gère sa position via XRI
                // Les scripts IInteractable vérifient null avant d'utiliser ActorController
                interactable.Interact(null);
                // On envoie quand même l'action au réseau
                m_actor.ExecuteAction(go.name);
                return;
            }

            // Priorité 2 : nom de l'objet → action scénario
            Debug.Log($"[XRActorInteractor] Interaction : {go.name}");
            m_actor.ExecuteAction(go.name);
        }
    }
}
