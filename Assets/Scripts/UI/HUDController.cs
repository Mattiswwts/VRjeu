using UnityEngine;
using TMPro;
using System.Collections;
using ActionGame.GameLogic;

namespace ActionGame.UI
{
    /// <summary>
    /// Gère 2 écrans physiques dans la scène (World Space Canvas sur Quad).
    ///
    /// Ecran_Acteur  → dans la maison (ex: note sur le bureau)
    /// Ecran_Realisateur → dans la cabine (moniteur face au réalisateur)
    ///
    /// Setup d'un écran physique :
    ///   1. Créer un Quad à l'emplacement voulu dans la scène
    ///   2. Créer un Canvas enfant du Quad → Render Mode : World Space
    ///      Régler sa taille pour qu'elle colle au Quad (ex: W=1, H=0.6, Scale=0.01)
    ///   3. Ajouter TextMeshPro dedans pour l'instruction + le compteur
    ///   4. Optionnel : fond sombre sur le Quad (Material Unlit/Color noir)
    ///
    /// Branchements Inspector :
    ///   ScenarioManager.OnActorInstructionChanged    → SetActorInstruction(string)
    ///   ScenarioManager.OnDirectorInstructionChanged → SetDirectorInstruction(string)
    ///   ScenarioManager.OnStepIndexChanged           → UpdateStepCounter(int)
    ///   SyncChecker.OnSyncSuccess                    → ShowSuccess()
    ///   SyncChecker.OnSyncFail                       → ShowFail()
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Écran Acteur (dans la maison)")]
        [SerializeField] private TextMeshProUGUI m_actorInstruction;
        [SerializeField] private TextMeshProUGUI m_actorStepCounter;
        [SerializeField] private TextMeshProUGUI m_actorFeedback;

        [Header("Écran Réalisateur (dans la cabine)")]
        [SerializeField] private TextMeshProUGUI m_directorInstruction;
        [SerializeField] private TextMeshProUGUI m_directorStepCounter;
        [SerializeField] private TextMeshProUGUI m_directorFeedback;

        [Header("Durée du flash de feedback (secondes)")]
        [SerializeField] private float m_feedbackDuration = 1.5f;

        [Header("Scénario (pour le total d'étapes)")]
        [SerializeField] private ScenarioManager m_scenarioManager;

        private int m_totalSteps;
        private Coroutine m_feedbackCoroutine;

        private void Start()
        {
            if (m_scenarioManager == null)
                m_scenarioManager = FindFirstObjectByType<ActionGame.GameLogic.ScenarioManager>();

            if (m_scenarioManager != null)
            {
                m_totalSteps = m_scenarioManager.StepCount;
                m_scenarioManager.OnActorInstructionChanged.AddListener(SetActorInstruction);
                m_scenarioManager.OnDirectorInstructionChanged.AddListener(SetDirectorInstruction);
                m_scenarioManager.OnStepIndexChanged.AddListener(UpdateStepCounter);
                m_scenarioManager.OnSyncSuccess.AddListener(ShowSuccess);
                m_scenarioManager.OnSyncFail.AddListener(ShowFail);

                // ScenarioManager démarre peut-être avant nous → on force un refresh
                m_scenarioManager.RefreshHUD();
            }

            SetFeedbackVisible(false);
        }

        private void OnDestroy()
        {
            if (m_scenarioManager != null)
            {
                m_scenarioManager.OnActorInstructionChanged.RemoveListener(SetActorInstruction);
                m_scenarioManager.OnDirectorInstructionChanged.RemoveListener(SetDirectorInstruction);
                m_scenarioManager.OnStepIndexChanged.RemoveListener(UpdateStepCounter);
                m_scenarioManager.OnSyncSuccess.RemoveListener(ShowSuccess);
                m_scenarioManager.OnSyncFail.RemoveListener(ShowFail);
            }
        }

        // ─── Instruction Acteur ───────────────────────────────────────────────

        public void SetActorInstruction(string instruction)
        {
            if (m_actorInstruction != null)
                m_actorInstruction.text = instruction;
        }

        // ─── Instruction Réalisateur ──────────────────────────────────────────

        public void SetDirectorInstruction(string instruction)
        {
            if (m_directorInstruction != null)
                m_directorInstruction.text = instruction;
        }

        // ─── Compteur d'étape (aparaît sur les 2 écrans) ─────────────────────

        public void UpdateStepCounter(int stepIndex)
        {
            string label = $"{stepIndex + 1} / {m_totalSteps}";
            if (m_actorStepCounter != null)    m_actorStepCounter.text    = label;
            if (m_directorStepCounter != null) m_directorStepCounter.text = label;
        }

        // ─── Feedback synchro (flash sur les 2 écrans) ───────────────────────

        public void ShowSuccess()
        {
            ShowFeedback("SYNCHRO !", new Color(0.2f, 0.85f, 0.3f));
        }

        public void ShowFail()
        {
            ShowFeedback("RATE !", new Color(0.9f, 0.2f, 0.2f));
        }

        // ─── Interne ─────────────────────────────────────────────────────────

        private void ShowFeedback(string message, Color color)
        {
            if (m_actorFeedback != null)
            {
                m_actorFeedback.text  = message;
                m_actorFeedback.color = color;
            }
            if (m_directorFeedback != null)
            {
                m_directorFeedback.text  = message;
                m_directorFeedback.color = color;
            }

            SetFeedbackVisible(true);

            if (m_feedbackCoroutine != null)
                StopCoroutine(m_feedbackCoroutine);
            m_feedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay());
        }

        private void SetFeedbackVisible(bool visible)
        {
            if (m_actorFeedback != null)    m_actorFeedback.gameObject.SetActive(visible);
            if (m_directorFeedback != null) m_directorFeedback.gameObject.SetActive(visible);
        }

        private IEnumerator HideFeedbackAfterDelay()
        {
            yield return new WaitForSeconds(m_feedbackDuration);
            SetFeedbackVisible(false);
        }
    }
}
