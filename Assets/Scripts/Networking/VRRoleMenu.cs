using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace ActionGame.Networking
{
    /// <summary>
    /// Crée deux boutons 3D flottants pour la sélection de rôle en VR.
    ///
    /// Les boutons n'apparaissent QUE LORSQUE Ubiq est connecté (room rejointe).
    /// Cela garantit que les deux joueurs sont dans la même room avant de choisir.
    ///
    /// Setup :
    ///   Ajouter sur un GameObject de la scène (ex: GameManager).
    ///   Trouve RoleSelector et UbiqSetup automatiquement.
    ///
    /// Visuel :
    ///   Bouton ACTEUR      → vert,  à gauche
    ///   Bouton RÉALISATEUR → bleu,  à droite
    /// </summary>
    public class VRRoleMenu : MonoBehaviour
    {
        [Header("Position des boutons")]
        [Tooltip("Distance devant le joueur")]
        [SerializeField] private float m_distance = 2f;
        [Tooltip("Hauteur (0 = niveau yeux)")]
        [SerializeField] private float m_height = 0f;
        [Tooltip("Écartement gauche/droite entre les deux boutons")]
        [SerializeField] private float m_spread = 0.6f;

        [Header("Apparence")]
        [SerializeField] private Vector3 m_buttonSize = new Vector3(0.4f, 0.4f, 0.1f);

        private RoleSelector m_roleSelector;
        private UbiqSetup    m_ubiqSetup;
        private GameObject   m_menuRoot;
        private bool         m_menuCreated = false;

        private void Start()
        {
            m_roleSelector = FindFirstObjectByType<RoleSelector>();
            m_ubiqSetup    = FindFirstObjectByType<UbiqSetup>();

            if (m_roleSelector == null)
            {
                Debug.LogWarning("[VRRoleMenu] RoleSelector introuvable — menu VR désactivé.");
                return;
            }

            if (!XRCheck.IsVR())
            {
                Debug.Log("[VRRoleMenu] Pas de casque VR détecté — menu clavier actif (F1/F2).");
                return;
            }

            // On cache le menu dès que le rôle est choisi
            m_roleSelector.OnRoleSelected.AddListener(_ => HideMenu());
        }

        private void Update()
        {
            // Menu déjà créé ou rôle déjà choisi
            if (m_menuCreated) return;
            if (m_roleSelector == null || m_roleSelector.RoleSelected) return;
            if (!XRCheck.IsVR()) return;

            // Attend que la connexion Ubiq soit établie
            // (IsConnected = true quand room rejointe OU mode Solo activé)
            if (m_ubiqSetup != null && !m_ubiqSetup.IsConnected) return;

            CreateMenu();
            m_menuCreated = true;
        }

        // ─── Création du menu ─────────────────────────────────────────────────

        private void CreateMenu()
        {
            // S'assure que les NearFarInteractors peuvent interagir avec l'UI Canvas
            foreach (var i in FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
                i.enableUIInteraction = true;

            m_menuRoot = new GameObject("VRRoleMenu_Root");

            // Placer le menu devant la caméra du joueur au moment de la connexion
            Camera cam = Camera.main;
            Vector3 origin  = cam != null ? cam.transform.position  : Vector3.zero;
            Vector3 forward = cam != null ? cam.transform.forward    : Vector3.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, -forward).normalized;

            Vector3 center = origin + forward * m_distance + Vector3.up * m_height;

            CreateButton(
                parent:    m_menuRoot.transform,
                localPos:  center - right * m_spread,
                label:     "ACTEUR",
                color:     new Color(0.2f, 0.8f, 0.3f),
                onSelect:  () => m_roleSelector.SelectActor()
            );

            CreateButton(
                parent:    m_menuRoot.transform,
                localPos:  center + right * m_spread,
                label:     "REALISATEUR",
                color:     new Color(0.2f, 0.5f, 0.9f),
                onSelect:  () => m_roleSelector.SelectDirector()
            );

            Debug.Log("[VRRoleMenu] Menu VR créé — pointez un bouton et appuyez sur le trigger.");
        }

        private void CreateButton(Transform parent, Vector3 localPos, string label,
                                   Color color, System.Action onSelect)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"RoleButton_{label}";
            go.transform.SetParent(parent, worldPositionStays: false);
            go.transform.localPosition = localPos;
            go.transform.localScale    = m_buttonSize;

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = color;
                renderer.material = mat;
            }

            var interactable = go.AddComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(_ => StartCoroutine(SafeSelect(interactable, onSelect)));

            AddLabel(go.transform, label, color);
        }

        private static void AddLabel(Transform parent, string text, Color color)
        {
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(parent, worldPositionStays: false);
            labelGo.transform.localPosition = new Vector3(0, 0, -0.6f);
            labelGo.transform.localScale    = new Vector3(0.1f, 0.1f, 0.1f);

            var tm = labelGo.AddComponent<TextMesh>();
            tm.text      = text;
            tm.fontSize  = 60;
            tm.color     = Color.white;
            tm.anchor    = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
        }

        // ─── Sélection sécurisée (force-relâche avant d'appeler SelectRole) ───

        private IEnumerator SafeSelect(XRSimpleInteractable cube, System.Action action)
        {
            // 1) Force-relâche le cube pour que l'interacteur ne soit plus actif
            var manager = FindFirstObjectByType<XRInteractionManager>();
            if (manager != null)
            {
                var selecting = new List<IXRSelectInteractor>(cube.interactorsSelecting);
                foreach (var interactor in selecting)
                    manager.SelectExit(interactor, cube);
            }

            // 2) Attendre 2 frames que XRI finalise le exit
            yield return null;
            yield return null;

            // 3) Switcher le rôle
            action();
        }

        private void HideMenu()
        {
            if (m_menuRoot != null)
                m_menuRoot.SetActive(false);
        }
    }
}
