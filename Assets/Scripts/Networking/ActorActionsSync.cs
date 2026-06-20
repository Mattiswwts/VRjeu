using UnityEngine;
using UnityEngine.Events;
using Ubiq.Messaging;

namespace ActionGame.Networking
{
    // Synchronise les actions de l'Acteur vers tous les clients via Ubiq.
    // Même pattern que EffectsSync mais dans l'autre sens.
    // À placer sur le même GameObject que EffectsSync (EffectsManager).
    public class ActorActionsSync : MonoBehaviour
    {
        public UnityEvent<string> OnActionReceived;

        private NetworkContext m_context;

        private struct ActionMessage { public string actionName; }

        private void Start()
        {
            m_context = NetworkScene.Register(this);

            // Desktop
            var actorDesktop = FindFirstObjectByType<ActionGame.Player.ActorController>();
            if (actorDesktop != null)
            {
                actorDesktop.OnActionExecuted.AddListener(SendAction);
                Debug.Log("[ActorActionsSync] ✅ ActorController (desktop) connecté");
            }

            // VR — cherche même si le composant est désactivé (activé après sélection de rôle)
            var actorXR = FindFirstObjectByType<ActionGame.Player.ActorControllerXR>(FindObjectsInactive.Include);
            if (actorXR != null)
            {
                actorXR.OnActionExecuted.AddListener(SendAction);
                Debug.Log("[ActorActionsSync] ✅ ActorControllerXR (VR) connecté");
            }

            if (actorDesktop == null && actorXR == null)
                Debug.Log("[ActorActionsSync] Pas d'ActorController local (normal côté Réalisateur)");
        }

        // Appelé par ActorController.OnActionExecuted et ScenarioTrigger
        public void SendAction(string actionName)
        {
            Debug.Log($"[ActorActionsSync] → Envoi : {actionName}");
            m_context.SendJson(new ActionMessage { actionName = actionName });
            OnActionReceived?.Invoke(actionName);
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var msg = message.FromJson<ActionMessage>();
            Debug.Log($"[ActorActionsSync] ← Réseau : {msg.actionName}");
            OnActionReceived?.Invoke(msg.actionName);
        }
    }
}
