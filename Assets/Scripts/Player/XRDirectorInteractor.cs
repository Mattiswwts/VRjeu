using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using ActionGame.Objects;

namespace ActionGame.Player
{
    /// <summary>
    /// À attacher sur la main droite du Réalisateur (XR rig).
    /// Détecte les boutons/leviers via selectEntered et les actionne,
    /// puis force-relâche immédiatement pour que la main ne reste pas collée.
    /// </summary>
    [RequireComponent(typeof(XRBaseInteractor))]
    public class XRDirectorInteractor : MonoBehaviour
    {
        private XRBaseInteractor m_interactor;

        private void Awake()
        {
            m_interactor = GetComponent<XRBaseInteractor>();
        }

        private void OnEnable()
        {
            m_interactor.selectEntered.AddListener(OnSelectEntered);
        }

        private void OnDisable()
        {
            m_interactor.selectEntered.RemoveListener(OnSelectEntered);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            var go = args.interactableObject.transform.gameObject;

            var button = go.GetComponent<InteractableButton>()
                      ?? go.GetComponentInParent<InteractableButton>();
            if (button != null)
            {
                button.Press();
                StartCoroutine(ReleaseNextFrame(args.interactableObject as XRBaseInteractable));
                return;
            }

            var lever = go.GetComponent<Lever>()
                     ?? go.GetComponentInParent<Lever>();
            if (lever != null)
            {
                lever.Toggle();
                StartCoroutine(ReleaseNextFrame(args.interactableObject as XRBaseInteractable));
            }
        }

        private IEnumerator ReleaseNextFrame(XRBaseInteractable interactable)
        {
            if (interactable == null) yield break;

            yield return null;
            yield return null;

            var manager = FindFirstObjectByType<XRInteractionManager>();
            if (manager == null) yield break;

            var selecting = new List<IXRSelectInteractor>(interactable.interactorsSelecting);
            foreach (var i in selecting)
                manager.SelectExit(i, interactable);
        }
    }
}
