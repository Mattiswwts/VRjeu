using UnityEngine;
using Ubiq.Rooms;
using Ubiq.Messaging;

namespace ActionGame.Networking
{
    /// <summary>
    /// Point d'entrée réseau du jeu.
    /// Instance 1 : crée une room → affiche le join code à l'écran.
    /// Instance 2 : entre le join code → rejoint la même room.
    /// </summary>
    public class UbiqSetup : MonoBehaviour
    {
        private RoomClient m_roomClient;

        // UI join code
        private string m_joinCodeInput = "";
        private string m_displayedJoinCode = "";
        private bool m_isConnected = false;

        private void Awake()
        {
            Debug.Log("[UbiqSetup] Awake — script actif");
        }

        private void Start()
        {
            m_roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();

            if (m_roomClient == null)
            {
                Debug.LogError("[UbiqSetup] RoomClient introuvable !");
                return;
            }

            m_roomClient.OnJoinedRoom.AddListener(OnJoinedRoom);
            m_roomClient.OnPeerAdded.AddListener(OnPeerAdded);
            m_roomClient.OnPeerRemoved.AddListener(OnPeerRemoved);
        }

        // ─── Callbacks réseau ────────────────────────────────────────────────

        private void OnJoinedRoom(IRoom room)
        {
            if (string.IsNullOrEmpty(room.UUID)) return; // ignore le Reconnect vide

            m_displayedJoinCode = room.JoinCode;
            m_isConnected = true;
            Debug.Log($"[UbiqSetup] ✅ Connecté ! Join code : {room.JoinCode}");
        }

        private void OnPeerAdded(IPeer peer)
        {
            Debug.Log($"[UbiqSetup] 👤 Joueur connecté — uuid : {peer.uuid}");
        }

        private void OnPeerRemoved(IPeer peer)
        {
            Debug.Log($"[UbiqSetup] 👤 Joueur déconnecté — uuid : {peer.uuid}");
        }

        // ─── Accesseurs publics ───────────────────────────────────────────────

        public RoomClient RoomClient => m_roomClient;
        public bool IsConnected => m_isConnected;

        // ─── UI ──────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            if (m_isConnected)
            {
                // Affiche le join code en haut de l'écran pour que l'autre instance puisse le copier
                GUI.Box(new Rect(10, 10, 320, 35), "");
                GUI.Label(new Rect(20, 18, 310, 25), $"Join Code : {m_displayedJoinCode}");
                return;
            }

            // Panneau de connexion centré
            float w = 400f, h = 200f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), "");

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 16 };

            GUI.Label(new Rect(x, y + 10, w, 35), "Connexion réseau", titleStyle);

            // Créer une nouvelle room
            if (GUI.Button(new Rect(x + 30, y + 60, 340, 40), "Créer une room (Hôte)", btnStyle))
            {
                m_roomClient.Join("ActionGame-Room", publish: true);
            }

            // Rejoindre avec un join code
            GUI.Label(new Rect(x + 30, y + 115, 100, 25), "Join code :");
            m_joinCodeInput = GUI.TextField(new Rect(x + 130, y + 113, 160, 28), m_joinCodeInput);

            if (GUI.Button(new Rect(x + 300, y + 112, 70, 30), "Rejoindre", btnStyle))
            {
                if (!string.IsNullOrEmpty(m_joinCodeInput))
                    m_roomClient.Join(m_joinCodeInput.Trim());
            }
        }
    }
}
