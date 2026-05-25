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

            var actor = FindFirstObjectByType<ActionGame.Player.ActorController>();
            if (actor != null)
            {
                actor.OnActionExecuted.AddListener(SendAction);
                Debug.Log("[ActorActionsSync] ✅ ActorController connecté");
            }
            else
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
