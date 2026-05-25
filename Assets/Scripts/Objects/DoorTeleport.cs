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

        // ─── Clic souris (desktop) ───────────────────────────────────────────
        public void Interact(ActorController actor)
        {
            if (m_targetPoint == null)
            {
                Debug.LogError($"[DoorTeleport] {gameObject.name} : Target Point non assigné !");
                return;
            }

            if (actor != null)
            {
                var cc = actor.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                    cc.transform.position = m_targetPoint.position;
                    cc.enabled = true;
                }
            }

            Debug.Log($"[DoorTeleport] Acteur téléporté via {gameObject.name}");
            NotifyScenario();
        }

        // ─── Traversée physique (VR ou marche vers la porte) ─────────────────
        private void OnTriggerEnter(Collider other)
        {
            bool isActor = other.GetComponent<ActorController>() != null
                        || other.GetComponentInParent<ActorController>() != null
                        || other.CompareTag("Actor")
                        || other.transform.root.CompareTag("Actor");
            if (!isActor) return;

            Debug.Log($"[DoorTeleport] Traversée détectée : {gameObject.name}");
            NotifyScenario();
        }

        // ─── Commun ───────────────────────────────────────────────────────────
        private void NotifyScenario()
        {
            var actorSync = FindFirstObjectByType<ActionGame.Networking.ActorActionsSync>();
            if (actorSync != null)
                actorSync.SendAction(gameObject.name);
            else
                m_scenarioManager?.RegisterActorAction(gameObject.name);
        }
    }
}
