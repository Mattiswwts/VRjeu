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
            // Accepte uniquement l'Acteur (a un ActorController)
            if (other.GetComponent<ActorController>() == null &&
                other.GetComponentInParent<ActorController>() == null) return;

            Debug.Log($"[ScenarioTrigger] Zone '{m_zoneName}' atteinte");
            m_scenarioManager?.RegisterActorAction(m_zoneName);
        }
    }
}
