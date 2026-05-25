using UnityEngine;
using UnityEngine.Events;

namespace ActionGame.Player
{
    /// <summary>
    /// Composant minimal pour le Réalisateur VR.
    /// À placer sur l'XROrigin du Réalisateur.
    ///
    /// La rotation est gérée par SnapTurnProvider (XRI).
    /// Pas de locomotion : le réalisateur reste dans sa cabine.
    /// Les interactions (boutons/leviers) sont gérées par XRDirectorInteractor.
    /// </summary>
    public class DirectorControllerXR : MonoBehaviour
    {
        [Header("Événements")]
        public UnityEvent<string> OnEffectTriggered;

        /// <summary>Appelé par XRDirectorInteractor si besoin d'event centralisé.</summary>
        public void TriggerEffect(string effectName)
        {
            Debug.Log($"[DirectorControllerXR] Effet : {effectName}");
            OnEffectTriggered?.Invoke(effectName);
        }
    }
}
