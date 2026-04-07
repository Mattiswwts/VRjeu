using UnityEngine;
using UnityEngine.Events;

namespace ActionGame.GameLogic
{
    /// <summary>
    /// Vérifie si l'action de l'Acteur et l'effet du Réalisateur
    /// arrivent dans la fenêtre de synchronisation.
    ///
    /// Logique :
    /// - L'Acteur fait une action → on enregistre le timestamp
    /// - Le Réalisateur déclenche un effet → on compare les timestamps
    /// - Si la différence est dans m_syncWindow → ✅ SYNCHRO
    /// - Sinon → ❌ RATÉ
    /// </summary>
    public class SyncChecker : MonoBehaviour
    {
        [Header("Fenêtre de synchronisation")]
        [Tooltip("Temps max (en secondes) entre l'action et l'effet pour que ce soit synchro")]
        [SerializeField] private float m_syncWindow = 2f;

        [Header("Événements")]
        public UnityEvent OnSyncSuccess; // ✅ Synchro réussie
        public UnityEvent OnSyncFail;    // ❌ Raté

        // Timestamp de la dernière action de l'Acteur (-1 = pas encore d'action)
        private float m_lastActionTime = -1f;
        private string m_lastActionName = "";

        // ─── Appelé par ActorController via OnActionExecuted ─────────────────

        public void RegisterAction(string actionName)
        {
            m_lastActionTime = Time.time;
            m_lastActionName = actionName;
            Debug.Log($"[SyncChecker] Action enregistrée : {actionName} à t={m_lastActionTime:F2}s");
        }

        // ─── Appelé par DirectorController via OnEffectTriggered ─────────────

        public void RegisterEffect(string effectName)
        {
            Debug.Log($"[SyncChecker] Effet reçu : {effectName} à t={Time.time:F2}s");

            // Pas encore d'action enregistrée
            if (m_lastActionTime < 0f)
            {
                Debug.Log("[SyncChecker] ❌ RATÉ — effet déclenché sans action préalable");
                OnSyncFail?.Invoke();
                return;
            }

            float delta = Mathf.Abs(Time.time - m_lastActionTime);
            Debug.Log($"[SyncChecker] Delta = {delta:F2}s | Fenêtre = {m_syncWindow}s");

            if (delta <= m_syncWindow)
            {
                Debug.Log($"[SyncChecker] ✅ SYNCHRO ! ({m_lastActionName} + {effectName})");
                OnSyncSuccess?.Invoke();
            }
            else
            {
                Debug.Log($"[SyncChecker] ❌ RATÉ — trop lent ({delta:F2}s > {m_syncWindow}s)");
                OnSyncFail?.Invoke();
            }

            // Reset après chaque tentative
            m_lastActionTime = -1f;
        }
    }
}
