using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using ActionGame.Player;

namespace ActionGame.Objects
{
    /// <summary>
    /// Boîte à musique — détecte le CD de deux façons :
    ///   Desktop : clic sur la boîte pendant que l'acteur tient le CD (IInteractable)
    ///   VR      : le CD entre dans la zone trigger de la boîte (OnTriggerEnter)
    ///
    /// Setup scène :
    ///   - Ajoute un second Collider sur la boîte (ou le même), IS TRIGGER = true
    ///   - Branche EffectsSync.OnEffectReceived → TriggerMusicBox(string)
    /// </summary>
    public class MusicBoxController : MonoBehaviour, IInteractable
    {
        [Header("Rouage")]
        [SerializeField] private Transform m_gear;
        [SerializeField] private float     m_rotationSpeed = 180f;
        [SerializeField] private Vector3   m_rotationAxis  = Vector3.forward;

        [Header("Musique")]
        [SerializeField] private AudioSource m_audioSource;

        private bool m_isPlaying  = false;
        private bool m_cdInserted = false;

        private ActorController m_actor;

        private void Awake()
        {
            if (m_audioSource == null)
                m_audioSource = GetComponent<AudioSource>();
            if (m_audioSource != null)
            {
                m_audioSource.playOnAwake  = false;
                m_audioSource.spatialBlend = 0f;
                m_audioSource.Stop();
            }
        }

        private void Start()
        {
            var effectsSync = FindFirstObjectByType<ActionGame.Networking.EffectsSync>();
            if (effectsSync != null)
            {
                effectsSync.OnEffectReceived.RemoveListener(TriggerMusicBox);
                effectsSync.OnEffectReceived.AddListener(TriggerMusicBox);
            }

            m_actor = FindFirstObjectByType<ActorController>();
            if (m_actor != null)
                m_actor.OnActionExecuted.AddListener(OnActorAction);
        }

        private void OnDestroy()
        {
            if (m_actor != null)
                m_actor.OnActionExecuted.RemoveListener(OnActorAction);
        }

        private void OnActorAction(string actionName) { }

        // ─── Desktop : clic IInteractable ────────────────────────────────────

        public void Interact(ActorController actor)
        {
            if (m_cdInserted) return;

            bool hasCD = actor != null && actor.HeldItem == "CD";
            if (actor != null && !hasCD)
            {
                Debug.Log("[MusicBoxController] Il faut d'abord récupérer le CD !");
                return;
            }

            if (hasCD)
            {
                var cd = FindFirstObjectByType<HoldableItem>();
                cd?.Place();
            }

            InsertCD();
        }

        // ─── VR : le CD entre physiquement dans la zone ───────────────────────

        private void OnTriggerEnter(Collider other)
        {
            if (m_cdInserted) return;

            var cd = other.GetComponent<HoldableItem>()
                  ?? other.GetComponentInParent<HoldableItem>();
            if (cd == null) return;

            Debug.Log("[MusicBoxController] CD détecté dans la zone (VR) !");

            // Force-relâche la main VR avant de placer le CD
            var grab = cd.GetComponent<XRGrabInteractable>();
            if (grab != null && grab.isSelected)
                StartCoroutine(ReleaseAndPlace(cd, grab));
            else
                PlaceCD(cd);
        }

        private IEnumerator ReleaseAndPlace(HoldableItem cd, XRGrabInteractable grab)
        {
            // Force-relâche
            var manager = FindFirstObjectByType<XRInteractionManager>();
            if (manager != null)
            {
                var selecting = new List<IXRSelectInteractor>(grab.interactorsSelecting);
                foreach (var i in selecting)
                    manager.SelectExit(i, grab);
            }
            yield return null;
            yield return null;
            PlaceCD(cd);
        }

        private void PlaceCD(HoldableItem cd)
        {
            cd.Place();
            InsertCD();
        }

        // ─── Commun ───────────────────────────────────────────────────────────

        private void InsertCD()
        {
            m_cdInserted = true;
            Debug.Log("[MusicBoxController] CD inséré — boîte activée !");

            var actorSync = FindFirstObjectByType<ActionGame.Networking.ActorActionsSync>();
            if (actorSync != null)
                actorSync.SendAction(gameObject.name);
            else
                FindFirstObjectByType<ActionGame.GameLogic.ScenarioManager>()
                    ?.RegisterActorAction(gameObject.name);
        }

        private void Update()
        {
            if (!m_isPlaying || m_gear == null) return;
            m_gear.Rotate(m_rotationAxis, m_rotationSpeed * Time.deltaTime, Space.Self);
        }

        public void TriggerMusicBox(string effectName)
        {
            if (!string.Equals(effectName, "Music", System.StringComparison.OrdinalIgnoreCase)) return;
            m_isPlaying = true;
            if (m_audioSource != null)
            {
                Debug.Log($"[MusicBoxController] Play ! clip={m_audioSource.clip?.name ?? "NULL"}");
                m_audioSource.Play();
            }
            else
                Debug.LogError("[MusicBoxController] AudioSource NULL !");
        }

        public void StopMusicBox()
        {
            m_isPlaying = false;
            m_audioSource?.Stop();
        }
    }
}
