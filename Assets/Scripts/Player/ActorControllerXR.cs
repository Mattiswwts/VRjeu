using UnityEngine;
using UnityEngine.Events;

namespace ActionGame.Player
{
    /// <summary>
    /// Composant minimal pour l'Acteur VR.
    /// À placer sur l'XROrigin de l'Acteur (à côté de LocomotionSystem, etc.)
    ///
    /// La locomotion (téléportation, rotation) est entièrement gérée par
    /// les composants XRI standard (TeleportationProvider, SnapTurnProvider).
    /// Ce script gère uniquement les événements de gameplay.
    /// </summary>
    public class ActorControllerXR : MonoBehaviour
    {
        [Header("Événements gameplay")]
        public UnityEvent<string> OnActionExecuted;

        public string HeldItem { get; private set; } = "";
        public void PickUp(string itemName) => HeldItem = itemName;
        public void DropHeld()             => HeldItem = "";

        /// <summary>
        /// Appelé par XRActorInteractor ou XRPickupHelper quand l'acteur
        /// interagit avec un objet en VR.
        /// </summary>
        public void ExecuteAction(string actionName)
        {
            Debug.Log($"[ActorControllerXR] Action : {actionName}");
            OnActionExecuted?.Invoke(actionName);
        }
    }
}
