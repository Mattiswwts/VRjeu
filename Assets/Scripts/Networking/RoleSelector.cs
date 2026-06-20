using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using ActionGame.Player;

namespace ActionGame.Networking
{
    /// <summary>
    /// Sélection de rôle — Desktop (F1/F2) et VR (cubes 3D via VRRoleMenu).
    ///
    /// En VR : UN SEUL XR rig (Player_Actor_XR) utilisé pour les deux rôles.
    /// On le téléporte au bon spawn et on swap les scripts — jamais de SetActive(false)
    /// sur un rig actif pour éviter les crashes OpenXR.
    ///
    /// Setup Inspector :
    ///   m_actorPlayer    → Player_Actor    (desktop + spawn Actor en VR)
    ///   m_directorPlayer → Player_Director (desktop + spawn Director en VR)
    ///   m_actorPlayerXR  → Player_Actor_XR (le seul XR rig)
    ///
    /// Sur Player_Actor_XR il faut :
    ///   - ActorControllerXR   (activé par défaut)
    ///   - DirectorControllerXR (désactivé par défaut)
    ///   - Sur Right Hand : XRActorInteractor   (activé)
    ///                      XRDirectorInteractor (désactivé)
    /// </summary>
    public class RoleSelector : MonoBehaviour
    {
        [Header("Joueurs Desktop (aussi utilisés comme spawn points VR)")]
        [SerializeField] private GameObject m_actorPlayer;
        [SerializeField] private GameObject m_directorPlayer;

        [Header("XR Rig unique")]
        [SerializeField] private GameObject m_actorPlayerXR;


        [Header("Événements")]
        public UnityEvent<bool> OnRoleSelected; // true = Acteur, false = Réalisateur

        public bool RoleSelected => m_roleSelected;
        private bool m_roleSelected = false;

        private UbiqSetup m_ubiqSetup;

        private bool UbiqReady =>
            m_ubiqSetup == null || m_ubiqSetup.IsConnected;

        private void Start()
        {
            AutoFindPlayers();
            m_ubiqSetup = FindFirstObjectByType<UbiqSetup>();

#if UNITY_EDITOR
            // Editor : desktop actif, XR rig désactivé
            if (m_actorPlayerXR != null) m_actorPlayerXR.SetActive(false);
#else
            // Quest : désactiver desktop, activer le XR rig dans le lobby
            if (m_actorPlayer    != null) m_actorPlayer.SetActive(false);
            if (m_directorPlayer != null) m_directorPlayer.SetActive(false);
            if (m_actorPlayerXR  != null) m_actorPlayerXR.SetActive(true);

            // Désactiver les contrôleurs de jeu — ils seront activés après la sélection
            SetControllerActive(isActor: false, enable: false);
            SetControllerActive(isActor: true,  enable: false);
#endif
        }

        private void Update()
        {
            if (m_roleSelected) return;
            if (!UbiqReady) return;          // attend la connexion Ubiq
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.f1Key.wasPressedThisFrame) SelectRole(isActor: true);
            if (kb.f2Key.wasPressedThisFrame) SelectRole(isActor: false);
        }

        // ─── API publique ─────────────────────────────────────────────────────

        public void SelectActor()    => SelectRole(isActor: true);
        public void SelectDirector() => SelectRole(isActor: false);

        public void SelectRole(bool isActor)
        {
            if (m_roleSelected) return;
            m_roleSelected = true;

            Debug.Log($"[RoleSelector] Rôle : {(isActor ? "ACTEUR" : "RÉALISATEUR")} | VR : {IsVRActive()}");

            if (IsVRActive())
                SetupVR(isActor);
            else
                SetupDesktop(isActor);

            OnRoleSelected?.Invoke(isActor);
        }

        // ─── Setup VR ─────────────────────────────────────────────────────────

        private void SetupVR(bool isActor)
        {
            if (m_actorPlayerXR == null)
            {
                Debug.LogError("[RoleSelector] Player_Actor_XR introuvable !");
                return;
            }

            // 1. Téléporter le rig au bon spawn
            //    On réutilise la position du joueur desktop comme point d'arrivée
            GameObject spawnRef = isActor ? m_actorPlayer : m_directorPlayer;
            if (spawnRef != null)
            {
                Vector3 target = spawnRef.transform.position;
                var cc = m_actorPlayerXR.GetComponent<CharacterController>();
                if (cc != null) { cc.enabled = false; m_actorPlayerXR.transform.position = target; cc.enabled = true; }
                else m_actorPlayerXR.transform.position = target;
            }

            // 2. Activer les bons scripts de contrôle
            SetControllerActive(isActor: true,  enable: isActor);
            SetControllerActive(isActor: false, enable: !isActor);

            // 3. PlayerSync — non utilisé en VR (avatars Ubiq gèrent la visualisation distante)

            // 4. Avatar Ubiq
            var avatar = m_actorPlayerXR.GetComponentInChildren<Ubiq.XRI.HeadAndHandsAvatarInputXRI>(true);
            if (avatar != null) avatar.enabled = true;

            // 5. Caméra taguée MainCamera
            var cam = m_actorPlayerXR.GetComponentInChildren<Camera>();
            if (cam != null) cam.tag = "MainCamera";

            Debug.Log($"[RoleSelector] VR prêt — {(isActor ? "ACTEUR" : "RÉALISATEUR")}");
        }

        // ─── Setup Desktop ────────────────────────────────────────────────────

        private void SetupDesktop(bool isActor)
        {
            if (m_actorPlayer    != null) m_actorPlayer.SetActive(isActor);
            if (m_directorPlayer != null) m_directorPlayer.SetActive(!isActor);

            m_actorPlayer   ?.GetComponent<PlayerSync>()?.SetLocal(isActor);
            m_directorPlayer?.GetComponent<PlayerSync>()?.SetLocal(!isActor);

            var actorCtrl    = m_actorPlayer   ?.GetComponent<ActorController>();
            var directorCtrl = m_directorPlayer?.GetComponent<DirectorController>();
            if (actorCtrl    != null) actorCtrl.enabled    = isActor;
            if (directorCtrl != null) directorCtrl.enabled = !isActor;

            SetupCameras(
                localPlayer:  isActor ? m_actorPlayer  : m_directorPlayer,
                remotePlayer: isActor ? m_directorPlayer : m_actorPlayer
            );

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private void SetControllerActive(bool isActor, bool enable)
        {
            if (m_actorPlayerXR == null) return;

            if (isActor)
            {
                var c = m_actorPlayerXR.GetComponentInChildren<ActorControllerXR>(true);
                if (c != null) c.enabled = enable;
                var i = m_actorPlayerXR.GetComponentInChildren<XRActorInteractor>(true);
                if (i != null) i.enabled = enable;
            }
            else
            {
                var c = m_actorPlayerXR.GetComponentInChildren<DirectorControllerXR>(true);
                if (c != null) c.enabled = enable;
                var i = m_actorPlayerXR.GetComponentInChildren<XRDirectorInteractor>(true);
                if (i != null) i.enabled = enable;
            }
        }

        private static bool IsVRActive() => XRCheck.IsVR();

        private static void SetupCameras(GameObject localPlayer, GameObject remotePlayer)
        {
            if (remotePlayer != null)
            {
                var cam = remotePlayer.GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    var listener = cam.GetComponent<AudioListener>();
                    if (listener != null) listener.enabled = false;
                    cam.gameObject.SetActive(false);
                }
            }
            if (localPlayer != null)
            {
                var cam = localPlayer.GetComponentInChildren<Camera>();
                if (cam != null) { cam.rect = new Rect(0, 0, 1, 1); cam.tag = "MainCamera"; }
            }
        }

        private void AutoFindPlayers()
        {
            if (m_actorPlayer == null)
            { var c = FindFirstObjectByType<ActorController>(); if (c) m_actorPlayer = c.gameObject; }

            if (m_directorPlayer == null)
            { var c = FindFirstObjectByType<DirectorController>(); if (c) m_directorPlayer = c.gameObject; }

            if (m_actorPlayerXR == null)
            { var c = FindFirstObjectByType<ActorControllerXR>(); if (c) m_actorPlayerXR = c.gameObject; }
        }

        // ─── UI Desktop ───────────────────────────────────────────────────────

        private void OnGUI()
        {
            if (m_roleSelected) return;
            if (IsVRActive()) return;
            if (!UbiqReady) return;          // attend la connexion Ubiq

            float w = 400f, h = 180f;
            float x = (Screen.width  - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), "");
            GUIStyle title = new GUIStyle(GUI.skin.label)  { fontSize = 22, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUIStyle btn   = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            GUIStyle hint  = new GUIStyle(GUI.skin.label)  { fontSize = 13, alignment = TextAnchor.MiddleCenter };

            GUI.Label(new Rect(x,       y + 10,  w,   40), "Choisir votre rôle", title);
            if (GUI.Button(new Rect(x + 30,  y + 65, 160, 50), "F1 — Acteur",      btn)) SelectRole(isActor: true);
            if (GUI.Button(new Rect(x + 210, y + 65, 160, 50), "F2 — Réalisateur", btn)) SelectRole(isActor: false);
            GUI.Label(new Rect(x, y + 130, w, 30), "Ou appuyez sur F1 / F2", hint);
        }
    }
}
