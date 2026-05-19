using UnityEngine;
using ActionGame.Player;

namespace ActionGame.Objects
{
    /// <summary>
    /// Lance le rouage de la boîte à musique et joue la musique
    /// quand le Réalisateur envoie l'effet "Music".
    /// Brancher EffectsSync.OnEffectReceived → TriggerMusicBox(string).
    /// </summary>
    public class MusicBoxController : MonoBehaviour, IInteractable
    {
        [Header("Rouage")]
        [Tooltip("Le Transform du rouage à faire tourner")]
        [SerializeField] private Transform m_gear;
        [SerializeField] private float m_rotationSpeed = 180f;
        [SerializeField] private Vector3 m_rotationAxis = Vector3.forward;

        [Header("Musique")]
        [SerializeField] private AudioSource m_audioSource;

        private bool m_isPlaying = false;
        private bool m_cdInserted = false;

        private ActorController m_actor;

        private void Awake()
        {
            if (m_audioSource == null)
                m_audioSource = GetComponent<AudioSource>();

            if (m_audioSource != null)
            {
                m_audioSource.playOnAwake = false;
                m_audioSource.spatialBlend = 0f;
                m_audioSource.Stop();
            }
        }

        private void Start()
        {
            var effectsSync = FindObjectOfType<ActionGame.Networking.EffectsSync>();
            if (effectsSync != null)
                effectsSync.OnEffectReceived.AddListener(TriggerMusicBox);

            m_actor = FindObjectOfType<ActorController>();
            if (m_actor != null)
                m_actor.OnActionExecuted.AddListener(OnActorAction);
        }

        private void OnDestroy()
        {
            if (m_actor != null)
                m_actor.OnActionExecuted.RemoveListener(OnActorAction);
        }

        private void OnActorAction(string actionName)
        {
            // fallback non utilisé — l'interaction passe par IInteractable
        }

        public void Interact(ActorController actor)
        {
            if (m_cdInserted) return;

            if (actor.HeldItem != "CD")
            {
                Debug.Log("[MusicBoxController] Il faut d'abord récupérer le CD !");
                return;
            }

            m_cdInserted = true;
            var cd = FindObjectOfType<HoldableItem>();
            cd?.Place();
            Debug.Log("[MusicBoxController] CD inséré !");
            FindObjectOfType<ActionGame.GameLogic.ScenarioManager>()?.RegisterActorAction(gameObject.name);
        }

        private void Update()
        {
            if (!m_isPlaying || m_gear == null) return;
            m_gear.Rotate(m_rotationAxis, m_rotationSpeed * Time.deltaTime, Space.Self);
        }

        // Brancher sur EffectsSync.OnEffectReceived
        public void TriggerMusicBox(string effectName)
        {
            Debug.Log($"[MusicBoxController] TriggerMusicBox appelé avec : '{effectName}'");
            if (!string.Equals(effectName, "Music", System.StringComparison.OrdinalIgnoreCase)) return;
            m_isPlaying = true;
            if (m_audioSource != null)
            {
                Debug.Log($"[MusicBoxController] Play ! clip={m_audioSource.clip?.name ?? "NULL"}");
                m_audioSource.Play();
            }
            else
                Debug.LogError("[MusicBoxController] AudioSource NULL au moment de jouer !");
        }

        public void StopMusicBox()
        {
            m_isPlaying = false;
            if (m_audioSource != null)
                m_audioSource.Stop();
        }
    }
}
