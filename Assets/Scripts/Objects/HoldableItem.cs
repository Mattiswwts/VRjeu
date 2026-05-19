using System.Collections;
using UnityEngine;
using ActionGame.Player;

namespace ActionGame.Objects
{
    /// <summary>
    /// Pour le CD : quand l'acteur clique dessus, il le tient devant sa caméra.
    /// L'objet reste en main jusqu'à ce qu'un autre script appelle Place().
    /// </summary>
    public class HoldableItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private float m_holdDistance = 0.4f;

        private ActorController m_actor;
        private Camera m_actorCam;
        private bool m_isHeld = false;

        public bool IsHeld => m_isHeld;

        private void Start()
        {
            m_actor = FindFirstObjectByType<ActorController>();
            if (m_actor != null)
                m_actorCam = m_actor.GetComponentInChildren<Camera>();
        }

        public void Interact(ActorController actor)
        {
            if (m_isHeld) return;
            m_actor = actor;
            m_actorCam = actor.GetComponentInChildren<Camera>();
            m_isHeld = true;

            foreach (var col in GetComponentsInChildren<Collider>())
                col.enabled = false;

            m_actor.PickUp(gameObject.name);
            Debug.Log($"[HoldableItem] {gameObject.name} ramassé !");
            StartCoroutine(MoveToHand());
        }

        private IEnumerator MoveToHand()
        {
            float elapsed = 0f;
            Vector3 start = transform.position;

            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                Vector3 target = m_actorCam.transform.position + m_actorCam.transform.forward * m_holdDistance;
                transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }
        }

        private void Update()
        {
            if (!m_isHeld) return;
            transform.position = m_actorCam.transform.position + m_actorCam.transform.forward * m_holdDistance;
            transform.rotation = m_actorCam.transform.rotation;
        }

        /// <summary>Appelé par MusicBoxController quand le CD est placé dans la boîte.</summary>
        public void Place()
        {
            m_isHeld = false;
            m_actor?.DropHeld();
            gameObject.SetActive(false);
        }
    }
}
