using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;

namespace ActionGame.Networking
{
    /// <summary>
    /// Synchronise la position et rotation d'un joueur via Ubiq.
    /// Implémente INetworkSpawnable pour s'intégrer au NetworkSpawnManager.
    /// Seul le propriétaire envoie sa position — les autres reçoivent.
    /// </summary>
    public class NetworkedPlayerSync : MonoBehaviour, INetworkSpawnable
    {
        // INetworkSpawnable : ID assigné automatiquement par NetworkSpawnManager
        public NetworkId NetworkId { get; set; }

        private NetworkContext m_context;
        private bool m_isOwner = false;

        private struct TransformMessage
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        private void Start()
        {
            // Enregistre ce composant avec son NetworkId (assigné par le SpawnManager)
            m_context = NetworkScene.Register(this, NetworkId);
        }

        /// <summary>Appelé par PlayerSpawner après le spawn pour définir le propriétaire.</summary>
        public void SetOwner(bool isOwner)
        {
            m_isOwner = isOwner;
            Debug.Log($"[NetworkedPlayerSync] {gameObject.name} — propriétaire : {isOwner}");
        }

        private void Update()
        {
            // Seul le propriétaire envoie sa position
            if (!m_isOwner) return;

            m_context.SendJson(new TransformMessage
            {
                position = transform.position,
                rotation = transform.rotation
            });
        }

        // ─── Réception des positions des autres joueurs ──────────────────────

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            if (m_isOwner) return;

            var msg = message.FromJson<TransformMessage>();
            transform.position = msg.position;
            transform.rotation = msg.rotation;
        }
    }
}
