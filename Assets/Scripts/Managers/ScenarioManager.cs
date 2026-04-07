using UnityEngine;
using UnityEngine.Events;

namespace ActionGame.GameLogic
{
    /// <summary>
    /// Gère le scénario du jeu : 3 étapes simples pour le prototype.
    /// Avance quand une synchro réussit, reset si ça rate.
    ///
    /// Étapes du scénario prototype :
    /// 1. Acteur marche vers la boîte → Réalisateur allume la lumière
    /// 2. Acteur prend la boîte → Réalisateur joue un son
    /// 3. Acteur pose la boîte → Réalisateur déclenche la météo
    /// </summary>
    public class ScenarioManager : MonoBehaviour
    {
        [System.Serializable]
        public struct ScenarioStep
        {
            public string actorInstruction;    // Affiché dans le HUD Acteur
            public string directorInstruction; // Affiché dans le HUD Réalisateur
        }

        [Header("Scénario")]
        [SerializeField] private ScenarioStep[] m_steps = new ScenarioStep[]
        {
            new ScenarioStep { actorInstruction = "Va vers la boîte",      directorInstruction = "Allume la lumière" },
            new ScenarioStep { actorInstruction = "Interagis avec la boîte", directorInstruction = "Joue un son" },
            new ScenarioStep { actorInstruction = "Recule de la boîte",    directorInstruction = "Déclenche la météo" },
        };

        [Header("Événements")]
        public UnityEvent<string> OnActorInstructionChanged;    // → met à jour le HUD Acteur
        public UnityEvent<string> OnDirectorInstructionChanged; // → met à jour le HUD Réalisateur
        public UnityEvent<int> OnScenarioComplete;              // → fin du scénario (score en paramètre)

        private int m_currentStep = 0;
        private int m_takeCount = 0; // nombre de prises (ratés + 1)

        private void Start()
        {
            ShowCurrentStep();
        }

        // ─── Appelé par SyncChecker.OnSyncSuccess ────────────────────────────

        public void OnSyncSuccess()
        {
            m_currentStep++;
            Debug.Log($"[ScenarioManager] ✅ Étape {m_currentStep}/{m_steps.Length} réussie !");

            if (m_currentStep >= m_steps.Length)
            {
                // Scénario terminé
                m_takeCount++;
                Debug.Log($"[ScenarioManager] 🎬 SCÉNARIO TERMINÉ en {m_takeCount} prise(s) !");
                OnScenarioComplete?.Invoke(m_takeCount);
            }
            else
            {
                ShowCurrentStep();
            }
        }

        // ─── Appelé par SyncChecker.OnSyncFail ───────────────────────────────

        public void OnSyncFail()
        {
            m_takeCount++;
            m_currentStep = 0;
            Debug.Log($"[ScenarioManager] ❌ RATÉ — Prise {m_takeCount}, on recommence depuis le début !");
            ShowCurrentStep();
        }

        // ─── Affiche l'étape courante ─────────────────────────────────────────

        private void ShowCurrentStep()
        {
            if (m_currentStep >= m_steps.Length) return;

            var step = m_steps[m_currentStep];
            Debug.Log($"[ScenarioManager] Étape {m_currentStep + 1} — Acteur : '{step.actorInstruction}' | Réalisateur : '{step.directorInstruction}'");

            OnActorInstructionChanged?.Invoke(step.actorInstruction);
            OnDirectorInstructionChanged?.Invoke(step.directorInstruction);
        }
    }
}
