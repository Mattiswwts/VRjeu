using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using ActionGame.Player;

namespace ActionGame.Networking
{
    /// <summary>
    /// Affiche un écran de sélection de rôle au démarrage.
    /// F1 = Acteur, F2 = Réalisateur.
    ///
    /// Active les bons scripts de contrôle selon le rôle choisi,
    /// et marque le bon PlayerSync comme local.
    /// </summary>
    public class RoleSelector : MonoBehaviour
    {
        [Header("Références joueurs")]
        [SerializeField] private GameObject m_actorPlayer;
        [SerializeField] private GameObject m_directorPlayer;

        [Header("Événements")]
        public UnityEvent<bool> OnRoleSelected; // true = Acteur, false = Réalisateur

        private bool m_roleSelected = false;

        private void Start()
        {
            // Auto-trouve les joueurs par composant si les références Inspector sont cassées
            if (m_actorPlayer == null)
            {
                var ctrl = FindFirstObjectByType<ActorController>();
                if (ctrl != null) m_actorPlayer = ctrl.gameObject;
            }
            if (m_directorPlayer == null)
            {
                var ctrl = FindFirstObjectByType<DirectorController>();
                if (ctrl != null) m_directorPlayer = ctrl.gameObject;
            }

            // Ubiq VoipSpeechIndicator a besoin de Camera.main dès le départ
            if (Camera.main == null)
            {
                var cam = m_actorPlayer?.GetComponentInChildren<Camera>()
                       ?? m_directorPlayer?.GetComponentInChildren<Camera>();
                if (cam != null) cam.tag = "MainCamera";
            }
        }

        private void Update()
        {
            if (m_roleSelected) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.f1Key.wasPressedThisFrame) SelectRole(isActor: true);
            if (kb.f2Key.wasPressedThisFrame) SelectRole(isActor: false);
        }

        private void SelectRole(bool isActor)
        {
            // Joueur local et distant
            GameObject localPlayer  = isActor ? m_actorPlayer    : m_directorPlayer;
            GameObject remotePlayer = isActor ? m_directorPlayer  : m_actorPlayer;

            if (localPlayer == null || remotePlayer == null)
            {
                Debug.LogError("[RoleSelector] Joueur introuvable — vérifie Actor/Director dans la scène !");
                return;
            }

            // Marque les PlayerSync
            localPlayer.GetComponent<PlayerSync>()?.SetLocal(true);
            remotePlayer.GetComponent<PlayerSync>()?.SetLocal(false);

            // Active les contrôles du joueur local uniquement
            var actorCtrl    = m_actorPlayer.GetComponent<ActorController>();
            var directorCtrl = m_directorPlayer.GetComponent<DirectorController>();

            if (actorCtrl != null)    actorCtrl.enabled    = isActor;
            if (directorCtrl != null) directorCtrl.enabled = !isActor;

            // Désactive la caméra du joueur distant, plein écran pour le local
            var localCam  = localPlayer.GetComponentInChildren<Camera>();
            var remoteCam = remotePlayer.GetComponentInChildren<Camera>();
            if (remoteCam != null)
            {
                var remoteListener = remoteCam.GetComponent<AudioListener>();
                if (remoteListener != null) remoteListener.enabled = false;
                remoteCam.gameObject.SetActive(false);
            }
            if (localCam != null)
            {
                localCam.rect = new Rect(0, 0, 1, 1);
                localCam.tag = "MainCamera";
            }

            // Lock le curseur pour les 2 rôles (mouse look)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            m_roleSelected = true;
            OnRoleSelected?.Invoke(isActor);
            Debug.Log($"[RoleSelector] Rôle choisi : {(isActor ? "ACTEUR" : "RÉALISATEUR")}");
        }

        // ─── UI ──────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            if (m_roleSelected) return;

            float w = 400f, h = 180f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), "");

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            GUIStyle hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.Label(new Rect(x, y + 10, w, 40), "Choisir votre rôle", titleStyle);

            if (GUI.Button(new Rect(x + 30,  y + 65, 160, 50), "F1 — Acteur",      btnStyle)) SelectRole(isActor: true);
            if (GUI.Button(new Rect(x + 210, y + 65, 160, 50), "F2 — Réalisateur", btnStyle)) SelectRole(isActor: false);

            GUI.Label(new Rect(x, y + 130, w, 30), "Ou appuyez sur F1 / F2", hintStyle);
        }
    }
}
