using UnityEngine;
using Ubiq.Messaging;

namespace ActionGame.Networking
{
    /// <summary>
    /// Synchronise la visibilité (SetActive) d'un objet entre les 2 clients via Ubiq.
    ///
    /// Setup (par objet ramassable : tasse café, CD...) :
    ///   1. Ajouter ce composant sur le GameObject de l'objet
    ///   2. PickupItem et HoldableItem appellent SyncHide() automatiquement
    ///      à la place de gameObject.SetActive(false)
    ///
    /// Résultat : quand l'acteur ramasse/cache l'objet, il disparaît aussi
    /// côté Réalisateur.
    /// </summary>
    public class NetworkObjectState : MonoBehaviour
    {
        private NetworkContext m_context;

        private struct StateMessage
        {
            public bool active;
        }

        private void Start()
        {
            m_context = NetworkScene.Register(this);
        }

        /// <summary>
        /// Cache l'objet localement ET envoie le message à l'autre client.
        /// Appeler à la place de gameObject.SetActive(false).
        /// </summary>
        public void SyncHide()
        {
            m_context.SendJson(new StateMessage { active = false });
            gameObject.SetActive(false);
            Debug.Log($"[NetworkObjectState] {gameObject.name} → caché (local + réseau)");
        }

        /// <summary>
        /// Rend l'objet visible localement ET sur l'autre client.
        /// Utile si on veut reset la scène.
        /// </summary>
        public void SyncShow()
        {
            m_context.SendJson(new StateMessage { active = true });
            gameObject.SetActive(true);
            Debug.Log($"[NetworkObjectState] {gameObject.name} → visible (local + réseau)");
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var msg = message.FromJson<StateMessage>();
            gameObject.SetActive(msg.active);
            Debug.Log($"[NetworkObjectState] {gameObject.name} → {(msg.active ? "visible" : "caché")} (réseau)");
        }
    }
}
