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
            // 0 — Réveil
            new ScenarioStep {
                actorInstruction    = "Le personnage se réveille doucement...",
                directorInstruction = "ACTION !\nUne journée banale commence.",
                expectedActorAction    = "",
                expectedDirectorEffect = "",
                requiresSync = false
            },
            // 1 — TV
            new ScenarioStep {
                actorInstruction    = "Le personnage s'approche de la TV et l'allume.",
                directorInstruction = "Il allume la TV...\nLes infos du matin. Lance le son.",
                expectedActorAction    = "TV",
                expectedDirectorEffect = "Sound",
                requiresSync = true
            },
            // 2 — Note
            new ScenarioStep {
                actorInstruction    = "Le personnage s'approche du bureau et lit la feuille.",
                directorInstruction = "Il lit un mot laissé sur le bureau.\n\"Café dehors à 11h\" — il est temps. Règle l'heure.",
                expectedActorAction    = "feuille",
                expectedDirectorEffect = "DayNight_On",
                requiresSync = false
            },
            // 3 — Jardin
            new ScenarioStep {
                actorInstruction    = "Le personnage sort prendre l'air.",
                directorInstruction = "Il sort dans le jardin.\nObserve. Laisse-le respirer.",
                expectedActorAction    = "doorM",
                expectedDirectorEffect = "",
                requiresSync = false
            },
            // 4 — Café
            new ScenarioStep {
                actorInstruction    = "Le personnage savoure son café en silence.",
                directorInstruction = "Il boit son café.\nUn moment de calme. Profites-en.",
                expectedActorAction    = "tasse café",
                expectedDirectorEffect = "",
                requiresSync = false
            },
            // 5 — Boîte à musique
            new ScenarioStep {
                actorInstruction    = "Le personnage remarque quelque chose près de l'arbre...",
                directorInstruction = "Il découvre la boîte à musique.\nQuand il la prend — déclenche la mélodie.",
                expectedActorAction    = "musicbox",
                expectedDirectorEffect = "Music",
                requiresSync = true
            },
            // 6 — Pluie + retour maison
            new ScenarioStep {
                actorInstruction    = "Des gouttes commencent à tomber... le personnage rentre.",
                directorInstruction = "Les nuages arrivent.\nDéclenche la pluie — fais-le rentrer.",
                expectedActorAction    = "doorJ",
                expectedDirectorEffect = "Weather",
                requiresSync = false
            },
            // 7 — Interrupteur
            new ScenarioStep {
                actorInstruction    = "Le personnage essaie d'allumer la lumière...",
                directorInstruction = "L'interrupteur est cassé, il ne le sait pas encore.\nAllume la lumière depuis la régie.",
                expectedActorAction    = "interrupteur",
                expectedDirectorEffect = "Light",
                requiresSync = true
            },
            // 8 — Dodo
            new ScenarioStep {
                actorInstruction    = "Une journée ordinaire se termine. Le personnage va se coucher.",
                directorInstruction = "Il s'endort.\nFin de journée — passe en mode nuit.",
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

        // Appelé par HUDController après son abonnement pour afficher l'étape courante
        public void RefreshHUD() => ShowCurrentStep();

        private int   m_currentStep = 0;
        private int   m_takeCount   = 0;
        private float m_actorTime   = -1f;
        private float m_directorTime = -1f;
        private bool  m_actorDone   = false;
        private bool  m_directorDone = false;

        private ScenarioStep CurrentStep => m_steps[m_currentStep];

        private void Start()
        {
            // Actions Acteur via réseau (fonctionne sur les 2 clients)
            var actorSync = FindFirstObjectByType<ActionGame.Networking.ActorActionsSync>();
            if (actorSync != null)
                actorSync.OnActionReceived.AddListener(RegisterActorAction);
            else
            {
                // Fallback solo sans ActorActionsSync dans la scène
                var actor = FindFirstObjectByType<ActionGame.Player.ActorController>();
                if (actor != null) actor.OnActionExecuted.AddListener(RegisterActorAction);
            }

            // Effets Réalisateur via réseau
            var effectsSync = FindFirstObjectByType<ActionGame.Networking.EffectsSync>();
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

            // Préfixe "Étape X / Y" sur le prompteur du Réalisateur
            string dirText = $"Étape {m_currentStep + 1} / {m_steps.Length}\n\n{step.directorInstruction}";
            OnDirectorInstructionChanged?.Invoke(dirText);
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
