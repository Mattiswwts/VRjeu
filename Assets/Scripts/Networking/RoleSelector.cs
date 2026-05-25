using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using ActionGame.Player;

namespace ActionGame.Networking
{
    /// <summary>
    /// Sélection de rôle au démarrage — compatible Desktop ET VR.
    ///
    /// Desktop : F1 = Acteur, F2 = Réalisateur
    /// VR      : 2 boutons 3D flottants (VRRoleMenu les crée automatiquement)
    ///
    /// Setup Inspector :
    ///   m_actorPlayer        → GameObject Player_Actor   (desktop)
    ///   m_directorPlayer     → GameObject Player_Director (desktop)
    ///   m_actorPlayerXR      → GameObject Player_Actor_XR (VR, XROrigin)
    ///   m_directorPlayerXR   → GameObject Player_Director_XR (VR, XROrigin)
    ///
    /// Tous ces GameObjects doivent être désactivés au départ (SetActive false).
    /// </summary>
    public class RoleSelector : MonoBehaviour
    {
        [Header("Joueurs Desktop")]
        [SerializeField] private GameObject m_actorPlayer;
        [SerializeField] private GameObject m_directorPlayer;

        [Header("Joueurs VR (XR Origin)")]
        [SerializeField] private GameObject m_actorPlayerXR;
        [SerializeField] private GameObject m_directorPlayerXR;

        [Header("Événements")]
        public UnityEvent<bool> OnRoleSelected; // true = Acteur, false = Réalisateur

        public bool RoleSelected => m_roleSelected;

        private bool m_roleSelected = false;

        private void Start()
        {
            AutoFindPlayers();

            // Désactive tout au départ — RoleSelector active le bon au choix
            SetActiveIfExists(m_actorPlayer, false);
            SetActiveIfExists(m_directorPlayer, false);
            SetActiveIfExists(m_actorPlayerXR, false);
            SetActiveIfExists(m_directorPlayerXR, false);
        }

        private void Update()
        {
            if (m_roleSelected) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.f1Key.wasPressedThisFrame) SelectRole(isActor: true);
            if (kb.f2Key.wasPressedThisFrame) SelectRole(isActor: false);
        }

        // ─── API publique (appelée par VRRoleMenu) ────────────────────────────

        public void SelectActor()    => SelectRole(isActor: true);
        public void SelectDirector() => SelectRole(isActor: false);

        // ─── Logique principale ───────────────────────────────────────────────

        public void SelectRole(bool isActor)
        {
            if (m_roleSelected) return;
            m_roleSelected = true;

            bool useVR = IsVRActive();
            Debug.Log($"[RoleSelector] Rôle : {(isActor ? "ACTEUR" : "RÉALISATEUR")} | Mode : {(useVR ? "VR" : "Desktop")}");

            if (useVR)
                SetupVR(isActor);
            else
                SetupDesktop(isActor);

            OnRoleSelected?.Invoke(isActor);
        }

        // ─── Setup Desktop ────────────────────────────────────────────────────

        private void SetupDesktop(bool isActor)
        {
            SetActiveIfExists(m_actorPlayer,    isActor);
            SetActiveIfExists(m_directorPlayer, !isActor);

            if (m_actorPlayer != null)
            {
                m_actorPlayer.GetComponent<PlayerSync>()?.SetLocal(isActor);
                var ctrl = m_actorPlayer.GetComponent<ActorController>();
                if (ctrl != null) ctrl.enabled = isActor;
            }

            if (m_directorPlayer != null)
            {
                m_directorPlayer.GetComponent<PlayerSync>()?.SetLocal(!isActor);
                var ctrl = m_directorPlayer.GetComponent<DirectorController>();
                if (ctrl != null) ctrl.enabled = !isActor;
            }

            // Camera locale plein écran, camera distante désactivée
            SetupCameras(
                localPlayer:  isActor ? m_actorPlayer  : m_directorPlayer,
                remotePlayer: isActor ? m_directorPlayer : m_actorPlayer
            );

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        // ─── Setup VR ─────────────────────────────────────────────────────────

        private void SetupVR(bool isActor)
        {
            GameObject localXR  = isActor ? m_actorPlayerXR    : m_directorPlayerXR;
            GameObject remoteXR = isActor ? m_directorPlayerXR : m_actorPlayerXR;

            // Active le bon XR Rig
            SetActiveIfExists(localXR,  true);
            SetActiveIfExists(remoteXR, false); // l'autre joueur est sur son propre Quest

            if (localXR == null)
            {
                Debug.LogError("[RoleSelector] XR Rig local introuvable ! Vérifie les références dans l'Inspector.");
                return;
            }

            // PlayerSync — marque le local
            localXR.GetComponent<PlayerSync>()?.SetLocal(true);

            // Contrôleurs XR — active le bon
            var actorXR    = m_actorPlayerXR   ?.GetComponent<ActorControllerXR>();
            var directorXR = m_directorPlayerXR?.GetComponent<DirectorControllerXR>();
            if (actorXR    != null) actorXR.enabled    = isActor;
            if (directorXR != null) directorXR.enabled = !isActor;

            // Avatar Ubiq — active HeadAndHandsAvatarInputXRI uniquement sur le rig local
            SetAvatarInput(m_actorPlayerXR,    isActor);
            SetAvatarInput(m_directorPlayerXR, !isActor);

            // Tag MainCamera sur la caméra XR locale (requis par Ubiq VoIP)
            var xrCam = localXR.GetComponentInChildren<Camera>();
            if (xrCam != null) xrCam.tag = "MainCamera";
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static bool IsVRActive()
        {
#if UNITY_EDITOR
            return UnityEngine.XR.XRSettings.isDeviceActive;
#else
            return true; // Sur Quest, toujours en VR
#endif
        }

        private static void SetAvatarInput(GameObject xrRig, bool active)
        {
            if (xrRig == null) return;
            // HeadAndHandsAvatarInputXRI est dans le namespace Ubiq.XRI
            var input = xrRig.GetComponentInChildren<Ubiq.XRI.HeadAndHandsAvatarInputXRI>(includeInactive: true);
            if (input != null) input.enabled = active;
        }

        private static void SetupCameras(GameObject localPlayer, GameObject remotePlayer)
        {
            if (remotePlayer != null)
            {
                var remoteCam = remotePlayer.GetComponentInChildren<Camera>();
                if (remoteCam != null)
                {
                    var listener = remoteCam.GetComponent<AudioListener>();
                    if (listener != null) listener.enabled = false;
                    remoteCam.gameObject.SetActive(false);
                }
            }

            if (localPlayer != null)
            {
                var localCam = localPlayer.GetComponentInChildren<Camera>();
                if (localCam != null)
                {
                    localCam.rect = new Rect(0, 0, 1, 1);
                    localCam.tag  = "MainCamera";
                }
            }
        }

        private static void SetActiveIfExists(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }

        private void AutoFindPlayers()
        {
            if (m_actorPlayer == null)
            {
                var c = FindFirstObjectByType<ActorController>();
                if (c != null) m_actorPlayer = c.gameObject;
            }
            if (m_directorPlayer == null)
            {
                var c = FindFirstObjectByType<DirectorController>();
                if (c != null) m_directorPlayer = c.gameObject;
            }
            if (m_actorPlayerXR == null)
            {
                var c = FindFirstObjectByType<ActorControllerXR>();
                if (c != null) m_actorPlayerXR = c.gameObject;
            }
            if (m_directorPlayerXR == null)
            {
                var c = FindFirstObjectByType<DirectorControllerXR>();
                if (c != null) m_directorPlayerXR = c.gameObject;
            }
        }

        // ─── UI Desktop (inchangée) ───────────────────────────────────────────

        private void OnGUI()
        {
            if (m_roleSelected) return;
            if (IsVRActive()) return; // VRRoleMenu gère l'UI en VR

            float w = 400f, h = 180f;
            float x = (Screen.width  - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), "");

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)  { fontSize = 22, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUIStyle btnStyle   = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            GUIStyle hintStyle  = new GUIStyle(GUI.skin.label)  { fontSize = 13, alignment = TextAnchor.MiddleCenter };

            GUI.Label(new Rect(x,       y + 10,  w,   40), "Choisir votre rôle", titleStyle);
            if (GUI.Button(new Rect(x + 30,  y + 65, 160, 50), "F1 — Acteur",      btnStyle)) SelectRole(isActor: true);
            if (GUI.Button(new Rect(x + 210, y + 65, 160, 50), "F2 — Réalisateur", btnStyle)) SelectRole(isActor: false);
            GUI.Label(new Rect(x, y + 130, w, 30), "Ou appuyez sur F1 / F2", hintStyle);
        }
    }
}
