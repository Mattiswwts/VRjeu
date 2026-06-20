using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using ActionGame.Objects;

namespace ActionGame.Player
{
    /// <summary>
    /// À attacher sur l'XRRayInteractor de la main droite de l'Acteur.
    ///
    /// Deux modes de détection :
    ///   1. XRI (selectEntered) — pour les objets avec XRSimpleInteractable ou XRGrabInteractable
    ///   2. Physics.Raycast (fallback) — pour TOUS les objets avec collider (TV, zones, etc.)
    ///      Le joueur vise l'objet et appuie sur le trigger → ExecuteAction(nom)
    /// </summary>
    [RequireComponent(typeof(XRBaseInteractor))]
    public class XRActorInteractor : MonoBehaviour
    {
        [Header("Portée du rayon (fallback Physics)")]
        [SerializeField] private float m_rayDistance = 8f;

        [Header("Cooldown entre 2 actions (évite les doubles)")]
        [SerializeField] private float m_actionCooldown = 0.5f;

        private XRBaseInteractor m_interactor;
        private ActorControllerXR m_actor;

        // Input XR — on lit la valeur du trigger directement
        private UnityEngine.XR.InputDevice m_rightHand;
        private bool m_triggerWasPressed = false;
        private float m_lastActionTime = -99f;

        private void Awake()
        {
            m_interactor = GetComponent<XRBaseInteractor>();
            m_actor = GetComponentInParent<ActorControllerXR>();
            if (m_actor == null)
                Debug.LogWarning("[XRActorInteractor] ActorControllerXR introuvable dans les parents.");
        }

        private void OnEnable()
        {
            m_interactor.selectEntered.AddListener(OnSelectEntered);
        }

        private void OnDisable()
        {
            m_interactor.selectEntered.RemoveListener(OnSelectEntered);
        }

        // ─── Mode 1 : XRI (objets avec XRSimpleInteractable) ─────────────────

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (m_actor == null) return;
            if (Time.time - m_lastActionTime < m_actionCooldown) return;

            var go = args.interactableObject.transform.gameObject;

            // Priorité 1 : IInteractable (PickupItem, HoldableItem, etc.)
            var interactable = go.GetComponent<IInteractable>()
                            ?? go.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact(null);
                TriggerAction(go.name);
                return;
            }

            // Priorité 2 : nom de l'objet → action scénario
            Debug.Log($"[XRActorInteractor] XRI interaction : {go.name}");
            TriggerAction(go.name);
        }

        // ─── Mode 2 : Physics.Raycast (fallback — fonctionne sans XRSimpleInteractable) ──

        private void Update()
        {
            if (m_actor == null) return;

            // Récupère le device main droite si pas encore fait
            if (!m_rightHand.isValid)
            {
                var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, devices);
                if (devices.Count > 0) m_rightHand = devices[0];
                return;
            }

            // Lit l'état du trigger
            bool triggerPressed = false;
            m_rightHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerPressed);

            bool justPressed = triggerPressed && !m_triggerWasPressed;
            m_triggerWasPressed = triggerPressed;

            if (!justPressed) return;
            if (Time.time - m_lastActionTime < m_actionCooldown) return;

            // Raycast depuis la position/direction du controller
            var ray = new Ray(transform.position, transform.forward);
            Debug.DrawRay(transform.position, transform.forward * m_rayDistance, Color.yellow, 0.3f);

            if (!Physics.Raycast(ray, out RaycastHit hit, m_rayDistance)) return;

            var hitGO = hit.collider.gameObject;
            Debug.Log($"[XRActorInteractor] Raycast hit : {hitGO.name} (tag:{hitGO.tag})");

            // Ignore les objets sans intérêt scénario (sol, murs, plafond...)
            // Accepte tout objet nommé ou tagué "Interactable"
            if (hitGO.CompareTag("Untagged") &&
                hitGO.name == "Floor") return;

            // IInteractable en priorité
            var interactable = hitGO.GetComponent<IInteractable>()
                            ?? hitGO.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(null);
                TriggerAction(hitGO.name);
                return;
            }

            // Tout objet avec collider → envoie son nom comme action
            TriggerAction(hitGO.name);
        }

        private void TriggerAction(string objectName)
        {
            m_lastActionTime = Time.time;
            Debug.Log($"[XRActorInteractor] → Action : {objectName}");
            m_actor.ExecuteAction(objectName);
        }
    }
}
