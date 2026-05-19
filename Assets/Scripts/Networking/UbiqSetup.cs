using UnityEngine;
using Ubiq.Rooms;
using Ubiq.Messaging;

namespace ActionGame.Networking
{
    public class UbiqSetup : MonoBehaviour
    {
        private RoomClient m_roomClient;

        private string m_joinCodeInput  = "";
        private string m_displayedCode  = "";
        private bool   m_isConnected    = false;
        private bool   m_isSolo         = false;
        private bool   m_joining        = false;
        private float  m_joinStartTime  = 0f;
        private const float JOIN_TIMEOUT = 6f;

        private string m_statusMsg  = "";
        private Color  m_statusColor = Color.white;

        private void Start()
        {
            // Guard: NetworkScene might be missing from the scene
            GameObject ns = null;
            try
            {
                var scene = NetworkScene.Find(this);
                if (scene != null)
                {
                    ns = scene.gameObject;
                    m_roomClient = scene.GetComponentInChildren<RoomClient>();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[UbiqSetup] NetworkScene introuvable : {e.Message}");
            }

            if (m_roomClient != null)
            {
                m_roomClient.OnJoinedRoom.AddListener(OnJoinedRoom);
                m_roomClient.OnPeerAdded.AddListener(p => Debug.Log($"[UbiqSetup] Peer connecté : {p.uuid}"));
                m_roomClient.OnPeerRemoved.AddListener(p => Debug.Log($"[UbiqSetup] Peer déconnecté : {p.uuid}"));
            }
            else
            {
                Debug.LogWarning("[UbiqSetup] RoomClient introuvable — Solo uniquement.");
            }
        }

        private void Update()
        {
            if (!m_joining) return;

            if (Time.time - m_joinStartTime > JOIN_TIMEOUT)
            {
                m_joining = false;
                SetStatus("Serveur inaccessible. Lance en Solo ou vérifie ta connexion.", Color.yellow);
            }
        }

        private void OnJoinedRoom(IRoom room)
        {
            if (string.IsNullOrEmpty(room.UUID)) return;

            m_joining       = false;
            m_displayedCode = room.JoinCode;
            m_isConnected   = true;
            SetStatus("", Color.white);
            Debug.Log($"[UbiqSetup] Connecté ! Join code : {room.JoinCode}");
        }

        private void SetStatus(string msg, Color color)
        {
            m_statusMsg   = msg;
            m_statusColor = color;
        }

        // ─── Accesseurs ───────────────────────────────────────────────────────

        public RoomClient RoomClient  => m_roomClient;
        public bool       IsConnected => m_isConnected || m_isSolo;

        // ─── UI ──────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            // Solo mode : rien à afficher
            if (m_isSolo) return;

            // Connecté : affiche le join code en haut à gauche
            if (m_isConnected)
            {
                GUI.Box(new Rect(10, 10, 340, 35), "");
                GUI.Label(new Rect(20, 18, 330, 25), $"Join Code : {m_displayedCode}");
                return;
            }

            // Panneau de connexion centré
            float w = 420f, h = 260f;
            float x = (Screen.width  - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), "");

            GUIStyle title = new GUIStyle(GUI.skin.label)
                { fontSize = 20, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUIStyle btn = new GUIStyle(GUI.skin.button) { fontSize = 16 };

            GUI.Label(new Rect(x, y + 10, w, 35), "Connexion réseau", title);

            if (m_roomClient == null)
            {
                GUIStyle err = new GUIStyle(GUI.skin.label)
                    { fontSize = 13, normal = { textColor = Color.red }, alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(x, y + 55, w, 60), "NetworkScene/RoomClient introuvable !\nAjoute le prefab Ubiq NetworkScene dans la hiérarchie.", err);
                DrawSoloButton(x, y + 125, w, btn);
                return;
            }

            // Créer une room (hôte)
            GUI.enabled = !m_joining;
            if (GUI.Button(new Rect(x + 30, y + 60, 360, 40), m_joining ? "Connexion en cours..." : "Créer une room (Hôte)", btn))
            {
                m_joining      = true;
                m_joinStartTime = Time.time;
                SetStatus("", Color.white);
                m_roomClient.Join("ActionGame-Room", publish: true);
            }
            GUI.enabled = true;

            // Rejoindre avec un join code
            GUI.Label(new Rect(x + 30, y + 116, 100, 25), "Join code :");
            m_joinCodeInput = GUI.TextField(new Rect(x + 135, y + 114, 165, 28), m_joinCodeInput);

            if (GUI.Button(new Rect(x + 310, y + 113, 80, 30), "Rejoindre", btn))
            {
                if (!string.IsNullOrEmpty(m_joinCodeInput))
                {
                    m_joining       = true;
                    m_joinStartTime  = Time.time;
                    SetStatus("", Color.white);
                    m_roomClient.Join(m_joinCodeInput.Trim());
                }
            }

            // Message de statut (erreur timeout, etc.)
            if (!string.IsNullOrEmpty(m_statusMsg))
            {
                GUIStyle status = new GUIStyle(GUI.skin.label)
                    { fontSize = 12, alignment = TextAnchor.MiddleCenter, normal = { textColor = m_statusColor }, wordWrap = true };
                GUI.Label(new Rect(x + 10, y + 155, w - 20, 40), m_statusMsg, status);
            }

            DrawSoloButton(x, y + 205, w, btn);
        }

        private void DrawSoloButton(float x, float y, float w, GUIStyle btn)
        {
            GUIStyle solo = new GUIStyle(btn) { fontSize = 12, normal = { textColor = Color.yellow } };
            if (GUI.Button(new Rect(x + 110, y, 200, 28), "Solo (test sans réseau)", solo))
            {
                m_isSolo      = true;
                m_isConnected = true;
                m_joining     = false;
            }
        }
    }
}
