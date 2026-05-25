using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace ActionGame.Networking
{
    /// <summary>
    /// Crée deux boutons 3D flottants dans la scène pour sélectionner le rôle en VR.
    ///
    /// Les boutons apparaissent devant le joueur à l'angle de vue initial.
    /// Le joueur pointe son rayon et appuie sur le trigger pour choisir.
    ///
    /// Setup :
    ///   Ajouter ce script sur n'importe quel GameObject de la scène (ex: GameManager).
    ///   Il trouve RoleSelector automatiquement.
    ///   Les boutons se cachent automatiquement une fois le rôle choisi.
    ///
    /// Visuel :
    ///   Bouton ACTEUR     → vert,  à gauche
    ///   Bouton RÉALISATEUR → bleu, à droite
    /// </summary>
    public class VRRoleMenu : MonoBehaviour
    {
        [Header("Position des boutons")]
        [Tooltip("Distance devant le point de départ")]
        [SerializeField] private float m_distance = 2f;
        [Tooltip("Hauteur des boutons (0 = niveau yeux)")]
        [SerializeField] private float m_height = 0f;
        [Tooltip("Écartement gauche/droite entre les deux boutons")]
        [SerializeField] private float m_spread = 0.6f;

        [Header("Apparence")]
        [SerializeField] private Vector3 m_buttonSize = new Vector3(0.4f, 0.4f, 0.1f);

        private RoleSelector m_roleSelector;
        private GameObject   m_menuRoot;

        private void Start()
        {
            m_roleSelector = FindFirstObjectByType<RoleSelector>();
            if (m_roleSelector == null)
            {
                Debug.LogWarning("[VRRoleMenu] RoleSelector introuvable — menu VR désactivé.");
                return;
            }

            // N'affiche le menu VR que si un casque est connecté
            if (!UnityEngine.XR.XRSettings.isDeviceActive)
            {
                Debug.Log("[VRRoleMenu] Pas de casque VR détecté — menu clavier actif (F1/F2).");
                return;
            }

            CreateMenu();

            // Cacher le menu une fois le rôle sélectionné
            m_roleSelector.OnRoleSelected.AddListener(_ => HideMenu());
        }

        // ─── Création du menu ─────────────────────────────────────────────────

        private void CreateMenu()
        {
            m_menuRoot = new GameObject("VRRoleMenu_Root");

            Vector3 center = Vector3.forward * m_distance + Vector3.up * m_height;

            CreateButton(
                parent:    m_menuRoot.transform,
                localPos:  center + Vector3.left  * m_spread,
                label:     "ACTEUR",
                color:     new Color(0.2f, 0.8f, 0.3f),
                onSelect:  () => m_roleSelector.SelectActor()
            );

            CreateButton(
                parent:    m_menuRoot.transform,
                localPos:  center + Vector3.right * m_spread,
                label:     "RÉALISATEUR",
                color:     new Color(0.2f, 0.5f, 0.9f),
                onSelect:  () => m_roleSelector.SelectDirector()
            );

            Debug.Log("[VRRoleMenu] Menu VR créé — pointez un bouton et appuyez sur le trigger.");
        }

        private void CreateButton(Transform parent, Vector3 localPos, string label,
                                   Color color, System.Action onSelect)
        {
            // Corps du bouton
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"RoleButton_{label}";
            go.transform.SetParent(parent, worldPositionStays: false);
            go.transform.localPosition = localPos;
            go.transform.localScale    = m_buttonSize;

            // Couleur
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = color;
                renderer.material = mat;
            }

            // XRSimpleInteractable pour que le ray interactor le détecte
            var interactable = go.AddComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(_ => onSelect());

            // Label texte 3D
            AddLabel(go.transform, label, color);
        }

        private static void AddLabel(Transform parent, string text, Color color)
        {
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(parent, worldPositionStays: false);
            labelGo.transform.localPosition = new Vector3(0, 0, -0.6f); // devant le cube
            labelGo.transform.localScale    = new Vector3(0.1f, 0.1f, 0.1f);

            var tm = labelGo.AddComponent<TextMesh>();
            tm.text      = text;
            tm.fontSize  = 60;
            tm.color     = Color.white;
            tm.anchor    = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
        }

        private void HideMenu()
        {
            if (m_menuRoot != null)
                m_menuRoot.SetActive(false);
        }
    }
}
