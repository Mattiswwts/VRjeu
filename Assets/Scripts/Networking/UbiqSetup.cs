using UnityEngine;
using Ubiq.Rooms;
using Ubiq.Messaging;
using Ubiq.Networking;

namespace ActionGame.Networking
{
    public class UbiqSetup : MonoBehaviour
    {
        private RoomClient m_roomClient;

        private string m_serverInput   = "nexus.cs.ucl.ac.uk:8009";
        private string m_joinCodeInput = "";
        private string m_displayedCode = "";
        private bool   m_isConnected   = false;
        private bool   m_isSolo        = false;
        private bool   m_joining       = false;
        private float  m_joinStartTime = 0f;
        private const float JOIN_TIMEOUT = 8f;

        private string m_statusMsg   = "";
        private Color  m_statusColor = Color.white;

        private void Start()
        {
            try
            {
                var scene = NetworkScene.Find(this);
                if (scene != null)
                    m_roomClient = scene.GetComponentInChildren<RoomClient>();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[UbiqSetup] NetworkScene introuvable : {e.Message}");
            }

            if (m_roomClient != null)
            {
                m_roomClient.OnJoinedRoom.AddListener(OnJoinedRoom);
                m_roomClient.OnPeerAdded.AddListener(p   => Debug.Log($"[UbiqSetup] Peer connecté : {p.uuid}"));
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
                SetStatus($"Impossible de joindre {m_serverInput}\nVérifie l'IP ou ta connexion.", Color.yellow);
            }
        }

        private void ConnectAndJoin(bool createNew)
        {
            if (m_roomClient == null) return;

            m_joining       = true;
            m_joinStartTime = Time.time;
            SetStatus("", Color.white);

            // Force la connexion au serveur spécifié
            var parts = m_serverInput.Trim().Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[1], out _))
            {
                SetStatus("Format invalide — utilise : ip:port  (ex: nexus.cs.ucl.ac.uk:8009)", Color.red);
                m_joining = false;
                return;
            }

            var def = ScriptableObject.CreateInstance<ConnectionDefinition>();
            def.sendToIp   = parts[0];
            def.sendToPort = parts[1];
            def.type       = ConnectionType.TcpClient;
            m_roomClient.Connect(def);

            if (createNew)
                m_roomClient.Join("ActionGame-Room", publish: true);
            else
                m_roomClient.Join(m_joinCodeInput.Trim());
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

        public RoomClient RoomClient  => m_roomClient;
        public bool       IsConnected => m_isConnected || m_isSolo;

        private void OnGUI()
        {
            if (m_isSolo) return;

            if (m_isConnected)
            {
                GUI.Box(new Rect(10, 10, 340, 35), "");
                GUI.Label(new Rect(20, 18, 330, 25), $"Join Code : {m_displayedCode}");
                return;
            }

            float w = 440f, h = 300f;
            float x = (Screen.width  - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), "");

            GUIStyle title = new GUIStyle(GUI.skin.label)  { fontSize = 20, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUIStyle btn   = new GUIStyle(GUI.skin.button) { fontSize = 15 };
            GUIStyle lbl   = new GUIStyle(GUI.skin.label)  { fontSize = 13 };

            GUI.Label(new Rect(x, y + 10, w, 35), "Connexion réseau", title);

            // Serveur
            GUI.Label(new Rect(x + 20, y + 55, 80, 25), "Serveur :", lbl);
            m_serverInput = GUI.TextField(new Rect(x + 105, y + 53, 315, 26), m_serverInput);

            if (m_roomClient == null)
            {
                GUIStyle err = new GUIStyle(GUI.skin.label) { fontSize = 13, normal = { textColor = Color.red }, alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(x, y + 90, w, 50), "NetworkScene/RoomClient introuvable !\nAjoute le prefab Ubiq NetworkScene dans la hiérarchie.", err);
                DrawSoloButton(x, y + 250, w, btn);
                return;
            }

            // Créer une room
            GUI.enabled = !m_joining;
            if (GUI.Button(new Rect(x + 20, y + 100, 400, 40), m_joining ? "Connexion en cours..." : "Créer une room (Hôte)", btn))
                ConnectAndJoin(createNew: true);
            GUI.enabled = true;

            // Rejoindre
            GUI.Label(new Rect(x + 20, y + 158, 80, 25), "Join code :", lbl);
            m_joinCodeInput = GUI.TextField(new Rect(x + 105, y + 156, 200, 26), m_joinCodeInput);
            GUI.enabled = !m_joining;
            if (GUI.Button(new Rect(x + 315, y + 155, 105, 30), "Rejoindre", btn))
            {
                if (!string.IsNullOrEmpty(m_joinCodeInput))
                    ConnectAndJoin(createNew: false);
            }
            GUI.enabled = true;

            // Status
            if (!string.IsNullOrEmpty(m_statusMsg))
            {
                GUIStyle status = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleCenter, normal = { textColor = m_statusColor }, wordWrap = true };
                GUI.Label(new Rect(x + 10, y + 200, w - 20, 45), m_statusMsg, status);
            }

            DrawSoloButton(x, y + 255, w, btn);
        }

        private void DrawSoloButton(float x, float y, float w, GUIStyle btn)
        {
            GUIStyle solo = new GUIStyle(btn) { fontSize = 12, normal = { textColor = Color.yellow } };
            if (GUI.Button(new Rect(x + 110, y, 220, 28), "Solo (test sans réseau)", solo))
            {
                m_isSolo      = true;
                m_isConnected = true;
                m_joining     = false;
            }
        }
    }
}
