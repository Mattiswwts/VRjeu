using UnityEngine;
using ActionGame.GameLogic;
using ActionGame.Player;

namespace ActionGame.Objects
{
    /// <summary>
    /// Pose ce script sur doorM et doorJ.
    /// L'acteur clique sur la porte → téléportation + notification ScenarioManager.
    /// Le GameObject doit avoir un Collider (non-trigger) pour que le raycast le détecte.
    /// </summary>
    public class DoorTeleport : MonoBehaviour, IInteractable
    {
        [Tooltip("Où l'acteur apparaît après la porte")]
        [SerializeField] private Transform m_targetPoint;

        private ScenarioManager m_scenarioManager;
        private ActorController m_actor;

        private void Start()
        {
            m_scenarioManager = FindFirstObjectByType<ScenarioManager>();
        }

        public void Interact(ActorController actor)
        {
            if (m_targetPoint == null)
            {
                Debug.LogError($"[DoorTeleport] {gameObject.name} : Target Point non assigné !");
                return;
            }

            var cc = actor.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                cc.transform.position = m_targetPoint.position;
                cc.enabled = true;
            }

            Debug.Log($"[DoorTeleport] Acteur téléporté via {gameObject.name}");
            m_scenarioManager?.RegisterActorAction(gameObject.name);
        }
    }
}
