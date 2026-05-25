using UnityEngine;
using ActionGame.GameLogic;
using ActionGame.Player;

namespace ActionGame.Objects
{
    /// <summary>
    /// Zone trigger qui notifie le ScenarioManager quand l'Acteur entre dedans.
    /// Utilisation :
    ///   1. Créer un GameObject avec un Collider (Is Trigger = true)
    ///   2. Régler m_zoneName sur la valeur attendue par ScenarioManager
    ///      ("Zone_Jardin" ou "Zone_Maison")
    ///   3. Assigner le ScenarioManager dans l'Inspector
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ScenarioTrigger : MonoBehaviour
    {
        [Tooltip("Doit correspondre exactement à expectedActorAction dans ScenarioManager")]
        [SerializeField] private string m_zoneName;
        [SerializeField] private ScenarioManager m_scenarioManager;

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Accepte l'Acteur desktop (ActorController) ou VR (tag "Actor")
            bool isActor = other.GetComponent<ActorController>() != null
                        || other.GetComponentInParent<ActorController>() != null
                        || other.CompareTag("Actor")
                        || other.transform.root.CompareTag("Actor");
            if (!isActor) return;

            Debug.Log($"[ScenarioTrigger] Zone '{m_zoneName}' atteinte");

            // Passe par ActorActionsSync pour synchroniser sur le réseau
            var actorSync = FindFirstObjectByType<ActionGame.Networking.ActorActionsSync>();
            if (actorSync != null)
                actorSync.SendAction(m_zoneName);
            else
                m_scenarioManager?.RegisterActorAction(m_zoneName);
        }
    }
}
