using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using ActionGame.GameLogic;

namespace ActionGame.UI
{
    public class CheatButton : MonoBehaviour
    {
        [SerializeField] private ScenarioManager m_scenarioManager;

        private XRSimpleInteractable m_xrInteractable;

        private void Start()
        {
            if (m_scenarioManager == null)
                m_scenarioManager = FindFirstObjectByType<ScenarioManager>();

            m_xrInteractable = GetComponent<XRSimpleInteractable>();
            if (m_xrInteractable != null)
                m_xrInteractable.selectEntered.AddListener(OnSelectEntered);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            Press();
            StartCoroutine(ReleaseNextFrame(m_xrInteractable));
        }

        private IEnumerator ReleaseNextFrame(XRBaseInteractable interactable)
        {
            yield return null;
            yield return null;
            var manager = FindFirstObjectByType<XRInteractionManager>();
            if (manager == null) yield break;
            var selecting = new List<IXRSelectInteractor>(interactable.interactorsSelecting);
            foreach (var i in selecting)
                manager.SelectExit(i, interactable);
        }

        public void Press()
        {
            if (m_scenarioManager != null)
                m_scenarioManager.ForceNextStep();
            else
                Debug.LogWarning("[CheatButton] ScenarioManager introuvable !");
        }

        private void OnMouseDown() => Press();
    }
}
