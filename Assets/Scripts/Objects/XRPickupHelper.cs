using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ActionGame.Objects
{
    /// <summary>
    /// À ajouter sur les objets ramassables (qui ont déjà PickupItem ou HoldableItem).
    /// Nécessite aussi un XRGrabInteractable sur le même objet.
    ///
    /// Quand l'acteur attrape l'objet en VR → envoie l'action au ScenarioManager
    /// via ActorActionsSync (synchronisé sur le réseau).
    ///
    /// Setup par objet (ex: tasse café, musicbox) :
    ///   1. Add Component → XRGrabInteractable
    ///   2. Add Component → XRPickupHelper
    ///   3. Sur XRGrabInteractable : Throw On Detach = false (optionnel)
    /// </summary>
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable))]
    public class XRPickupHelper : MonoBehaviour
    {
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable m_interactable;

        private void Awake()
        {
            m_interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        }

        private void OnEnable()
        {
            m_interactable.selectEntered.AddListener(OnGrabbed);
        }

        private void OnDisable()
        {
            m_interactable.selectEntered.RemoveListener(OnGrabbed);
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            // Envoie le nom de l'objet comme action acteur (via réseau)
            var actorSync = FindFirstObjectByType<ActionGame.Networking.ActorActionsSync>();
            if (actorSync != null)
            {
                actorSync.SendAction(gameObject.name);
                Debug.Log($"[XRPickupHelper] Objet ramassé en VR : {gameObject.name}");
            }
        }
    }
}
