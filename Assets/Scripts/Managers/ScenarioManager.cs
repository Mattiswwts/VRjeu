using UnityEngine;
using UnityEngine.Events;

namespace ActionGame.GameLogic
{
    public class ScenarioManager : MonoBehaviour
    {
        [System.Serializable]
        public struct ScenarioStep
        {
            public string actorInstruction;
            public string directorInstruction;
            [Tooltip("Nom de l'objet/zone attendu côté Acteur. Vide = pas d'action acteur.")]
            public string expectedActorAction;
            [Tooltip("Nom de l'effet attendu côté Réalisateur. Vide = pas d'action réalisateur.")]
            public string expectedDirectorEffect;
            [Tooltip("Les deux actions doivent arriver dans m_syncWindow secondes l'une de l'autre.")]
            public bool requiresSync;
        }

        [Header("Scénario — Une journée banale")]
        [SerializeField] private ScenarioStep[] m_steps = new ScenarioStep[]
        {
            // 0 — Réveil (intro, avance automatiquement)
            new ScenarioStep {
                actorInstruction    = "Le personnage se réveille doucement...",
                directorInstruction = "Action !",
                expectedActorAction    = "",
                expectedDirectorEffect = "",
                requiresSync = false
            },
            // 1 — TV
            new ScenarioStep {
                actorInstruction    = "Le personnage s'approche de la TV et l'allume.",
                directorInstruction = "Le personnage allume la TV — active le son des infos.",
                expectedActorAction    = "TV",
                expectedDirectorEffect = "Sound",
                requiresSync = true
            },
            // 2 — Note
            new ScenarioStep {
                actorInstruction    = "Le personnage s'approche du bureau et lit la feuille.",
                directorInstruction = "Il lit un mot : \"Café dehors à 11h\" — règle l'heure sur 11h !",
                expectedActorAction    = "feuille",
                expectedDirectorEffect = "DayNight_On",
                requiresSync = false
            },
            // 3 — Jardin
            new ScenarioStep {
                actorInstruction    = "Le personnage sort dans le jardin.",
                directorInstruction = "Observe...",
                expectedActorAction    = "doorM",
                expectedDirectorEffect = "",
                requiresSync = false
            },
            // 4 — Café
            new ScenarioStep {
                actorInstruction    = "Le personnage savoure son café.",
                directorInstruction = "Laisse-le profiter du moment...",
                expectedActorAction    = "tasse café",
                expectedDirectorEffect = "",
                requiresSync = false
            },
            // 5 — Boîte à musique
            new ScenarioStep {
                actorInstruction    = "Le personnage ramasse la boîte à musique près de l'arbre.",
                directorInstruction = "Il trouve la boîte à musique — déclenche la mélodie !",
                expectedActorAction    = "musicbox",
                expectedDirectorEffect = "Music",
                requiresSync = true
            },
            // 6 — Pluie + retour maison
            new ScenarioStep {
                actorInstruction    = "Il commence à pleuvoir — le personnage rentre vite !",
                directorInstruction = "Déclenche la pluie pour le faire rentrer.",
                expectedActorAction    = "doorJ",
                expectedDirectorEffect = "Weather",
                requiresSync = false
            },
            // 7 — Interrupteur
            new ScenarioStep {
                actorInstruction    = "Le personnage essaie d'allumer la lumière... elle ne marche pas.",
                directorInstruction = "L'interrupteur est cassé — allume la lumière depuis la régie !",
                expectedActorAction    = "interrupteur",
                expectedDirectorEffect = "Light",
                requiresSync = true
            },
            // 8 — Dodo
            new ScenarioStep {
                actorInstruction    = "Le personnage va se coucher. Bonne nuit.",
                directorInstruction = "Il s'endort — passe en mode nuit.",
                expectedActorAction    = "lit",
                expectedDirectorEffect = "DayNight_Off",
                requiresSync = true
            },
        };

        [Header("Fenêtre de synchronisation (secondes)")]
        [SerializeField] private float m_syncWindow = 2f;

        [Header("Événements")]
        public UnityEvent<string> OnActorInstructionChanged;
        public UnityEvent<string> OnDirectorInstructionChanged;
        public UnityEvent<int>    OnStepIndexChanged;
        public UnityEvent         OnSyncSuccess;
        public UnityEvent         OnSyncFail;
        public UnityEvent<int>    OnScenarioComplete; // paramètre = nombre de prises

        public int StepCount => m_steps.Length;

        private int   m_currentStep = 0;
        private int   m_takeCount   = 0;
        private float m_actorTime   = -1f;
        private float m_directorTime = -1f;
        private bool  m_actorDone   = false;
        private bool  m_directorDone = false;

        private ScenarioStep CurrentStep => m_steps[m_currentStep];

        private void Start()
        {
            // Auto-connexions sans Inspector
            var actor = FindObjectOfType<ActionGame.Player.ActorController>();
            if (actor != null) actor.OnActionExecuted.AddListener(RegisterActorAction);

            var effectsSync = FindObjectOfType<ActionGame.Networking.EffectsSync>();
            if (effectsSync != null) effectsSync.OnEffectReceived.AddListener(RegisterDirectorEffect);

            ShowCurrentStep();
            TryAutoAdvance();
        }

        // ─── API publique ─────────────────────────────────────────────────────

        // Brancher sur : ActorController.OnActionExecuted + ScenarioTrigger (via code)
        public void RegisterActorAction(string actionName)
        {
            if (m_currentStep >= m_steps.Length) return;
            if (CurrentStep.expectedActorAction == "" || actionName != CurrentStep.expectedActorAction) return;

            m_actorDone = true;
            m_actorTime = Time.time;
            Debug.Log($"[ScenarioManager] ✅ Action acteur : {actionName}");
            TryAdvance();
        }

        // Brancher sur : EffectsSync.OnEffectReceived
        public void RegisterDirectorEffect(string effectName)
        {
            if (m_currentStep >= m_steps.Length) return;
            if (CurrentStep.expectedDirectorEffect == "" || effectName != CurrentStep.expectedDirectorEffect) return;

            m_directorDone = true;
            m_directorTime = Time.time;
            Debug.Log($"[ScenarioManager] 🎬 Effet réalisateur : {effectName}");
            TryAdvance();
        }

        // ─── Logique d'avancement ─────────────────────────────────────────────

        private void TryAdvance()
        {
            bool needsActor    = CurrentStep.expectedActorAction    != "";
            bool needsDirector = CurrentStep.expectedDirectorEffect != "";

            if (needsActor && !needsDirector)
            {
                if (m_actorDone) AdvanceStep();
                return;
            }

            if (!needsActor && needsDirector)
            {
                if (m_directorDone) AdvanceStep();
                return;
            }

            if (needsActor && needsDirector)
            {
                if (!m_actorDone || !m_directorDone) return;

                if (!CurrentStep.requiresSync)
                {
                    AdvanceStep();
                    return;
                }

                float delta = Mathf.Abs(m_actorTime - m_directorTime);
                if (delta <= m_syncWindow)
                {
                    Debug.Log($"[ScenarioManager] ✅ SYNCHRO ! delta={delta:F2}s");
                    OnSyncSuccess?.Invoke();
                    AdvanceStep();
                }
                else
                {
                    m_takeCount++;
                    Debug.Log($"[ScenarioManager] ❌ RATÉ — delta={delta:F2}s > {m_syncWindow}s (prise {m_takeCount})");
                    OnSyncFail?.Invoke();
                    ResetStepState();
                }
            }
        }

        private void AdvanceStep()
        {
            ResetStepState();
            m_currentStep++;

            if (m_currentStep >= m_steps.Length)
            {
                m_takeCount++;
                Debug.Log($"[ScenarioManager] 🎬 SCÉNARIO TERMINÉ en {m_takeCount} prise(s) !");
                OnScenarioComplete?.Invoke(m_takeCount);
                return;
            }

            ShowCurrentStep();
            TryAutoAdvance();
        }

        private void ResetStepState()
        {
            m_actorDone   = false;
            m_directorDone = false;
            m_actorTime   = -1f;
            m_directorTime = -1f;
        }

        private void ShowCurrentStep()
        {
            if (m_currentStep >= m_steps.Length) return;
            var step = m_steps[m_currentStep];
            Debug.Log($"[ScenarioManager] Étape {m_currentStep + 1}/{m_steps.Length} — '{step.actorInstruction}' | '{step.directorInstruction}'");
            OnActorInstructionChanged?.Invoke(step.actorInstruction);
            OnDirectorInstructionChanged?.Invoke(step.directorInstruction);
            OnStepIndexChanged?.Invoke(m_currentStep);
        }

        // Avance automatiquement si l'étape ne nécessite aucune action
        private void TryAutoAdvance()
        {
            if (m_currentStep >= m_steps.Length) return;
            if (CurrentStep.expectedActorAction == "" && CurrentStep.expectedDirectorEffect == "")
                Invoke(nameof(AdvanceStep), 2f);
        }
    }
}
