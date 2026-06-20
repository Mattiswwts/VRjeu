using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using ActionGame.GameLogic;
using ActionGame.Player;

namespace ActionGame.Objects
{
    /// <summary>
    /// Pose ce script sur doorM et doorJ.
    /// Desktop : clic souris → Interact(actor)
    /// VR      : XRSimpleInteractable.selectEntered → InteractVR()
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

            // Branchement VR : écoute directement le XRSimpleInteractable
            var xrInteractable = GetComponent<XRSimpleInteractable>();
            if (xrInteractable != null)
                xrInteractable.selectEntered.AddListener(_ => InteractVR());
        }

        // Appelé directement par XRSimpleInteractable en VR
        private void InteractVR()
        {
            if (m_targetPoint == null) return;

            var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                var cc = xrOrigin.GetComponent<CharacterController>();
                if (cc != null) { cc.enabled = false; xrOrigin.transform.position = m_targetPoint.position; cc.enabled = true; }
                else xrOrigin.transform.position = m_targetPoint.position;
            }

            Debug.Log($"[DoorTeleport] VR : téléporté via {gameObject.name}");
            NotifyScenario();

            // Relâche l'interacteur immédiatement (sinon la porte reste "tenue")
            StartCoroutine(ReleaseNextFrame());
        }

        private IEnumerator ReleaseNextFrame()
        {
            yield return null;
            var xri = GetComponent<XRSimpleInteractable>();
            if (xri == null) yield break;
            var manager = FindFirstObjectByType<XRInteractionManager>();
            if (manager == null) yield break;
            var selecting = new List<IXRSelectInteractor>(xri.interactorsSelecting);
            foreach (var interactor in selecting)
                manager.SelectExit(interactor, xri);
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
                // Desktop — déplace le CharacterController
                var cc = actor.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                    cc.transform.position = m_targetPoint.position;
                    cc.enabled = true;
                }
            }
            else
            {
                // VR — déplace le XR Origin
                var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
                if (xrOrigin != null)
                {
                    var cc = xrOrigin.GetComponent<CharacterController>();
                    if (cc != null)
                    {
                        cc.enabled = false;
                        xrOrigin.transform.position = m_targetPoint.position;
                        cc.enabled = true;
                    }
                    else
                    {
                        xrOrigin.transform.position = m_targetPoint.position;
                    }
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
                        || other.GetComponent<ActorControllerXR>() != null
                        || other.GetComponentInParent<ActorControllerXR>() != null
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
