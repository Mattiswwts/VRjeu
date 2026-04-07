using UnityEngine;
using Ubiq.Messaging;

namespace ActionGame.Networking
{
    /// <summary>
    /// Synchronise la position d'un joueur via Ubiq.
    /// À attacher sur Player_Actor ET Player_Director.
    ///
    /// Si c'est le joueur local → envoie sa position aux autres.
    /// Si c'est un joueur distant → reçoit et applique la position.
    /// </summary>
    public class PlayerSync : MonoBehaviour
    {
        private NetworkContext m_context;
        private bool m_isLocal = false;

        private struct TransformMessage
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        private void Start()
        {
            m_context = NetworkScene.Register(this);
        }

        /// <summary>Appelé par RoleSelector pour marquer ce joueur comme local.</summary>
        public void SetLocal(bool isLocal)
        {
            m_isLocal = isLocal;
            Debug.Log($"[PlayerSync] {gameObject.name} — local : {isLocal}");
        }

        private void Update()
        {
            if (!m_isLocal) return;

            m_context.SendJson(new TransformMessage
            {
                position = transform.position,
                rotation = transform.rotation
            });
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            if (m_isLocal) return;

            var msg = message.FromJson<TransformMessage>();
            transform.position = msg.position;
            transform.rotation = msg.rotation;
        }
    }
}
