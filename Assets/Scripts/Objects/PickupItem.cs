using System.Collections;
using UnityEngine;
using ActionGame.Player;

namespace ActionGame.Objects
{
    /// <summary>
    /// Pose ce script sur un objet à utiliser (tasse café...).
    /// Quand l'acteur clique dessus : l'objet se colle devant sa caméra puis disparaît.
    /// </summary>
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [Tooltip("Distance devant la caméra où l'objet apparaît une fois ramassé")]
        [SerializeField] private float m_holdDistance = 0.4f;
        [Tooltip("Durée pendant laquelle l'objet reste visible après le ramassage")]
        [SerializeField] private float m_holdDuration = 1.5f;

        private ActorController m_actor;
        private Camera m_actorCam;
        private bool m_pickedUp = false;

        private void Start()
        {
            m_actor = FindObjectOfType<ActorController>();
            if (m_actor != null)
                m_actorCam = m_actor.GetComponentInChildren<Camera>();
        }

        public void Interact(ActorController actor)
        {
            if (m_pickedUp) return;
            m_actor = actor;
            m_actorCam = actor.GetComponentInChildren<Camera>();
            m_pickedUp = true;

            foreach (var col in GetComponentsInChildren<Collider>())
                col.enabled = false;

            Debug.Log($"[PickupItem] {gameObject.name} ramassé !");
            actor.OnActionExecuted?.Invoke(gameObject.name);
            StartCoroutine(PickupRoutine());
        }

        private IEnumerator PickupRoutine()
        {
            // Détache de son parent pour qu'il suive la caméra librement
            transform.SetParent(null);

            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            // Déplace l'objet devant la caméra en 0.3s
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                Vector3 target = m_actorCam.transform.position + m_actorCam.transform.forward * m_holdDistance;
                transform.position = Vector3.Lerp(startPos, target, t);
                transform.rotation = Quaternion.Lerp(startRot, m_actorCam.transform.rotation, t);
                yield return null;
            }

            // Reste en main pendant m_holdDuration
            elapsed = 0f;
            while (elapsed < m_holdDuration)
            {
                elapsed += Time.deltaTime;
                transform.position = m_actorCam.transform.position + m_actorCam.transform.forward * m_holdDistance;
                transform.rotation = m_actorCam.transform.rotation;
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
