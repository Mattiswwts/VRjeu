using System.Collections;
using UnityEngine;
using Ubiq.Rooms;
using Ubiq.Messaging;
using Ubiq.Networking;

namespace ActionGame.Networking
{
    public class UbiqSetup : MonoBehaviour
    {
        [Header("Serveur Ubiq")]
        [SerializeField] private string m_server = "nexus.cs.ucl.ac.uk:8009";

        public event System.Action<string>        OnConnected;
        public event System.Action<string, Color> OnStatusChanged;

        private RoomClient           m_roomClient;
        private ConnectionDefinition m_connectionDef;
        private bool                 m_isConnected = false;
        private bool                 m_isSolo      = false;
        private bool                 m_joining     = false;
        private float                m_joinStartTime;
        private const float          JOIN_TIMEOUT  = 15f;

        public bool IsConnected => m_isConnected || m_isSolo;

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
                m_roomClient.OnPeerAdded.AddListener(p => Debug.Log($"[UbiqSetup] Peer : {p.uuid}"));
            }
            else
            {
                OnStatusChanged?.Invoke("RoomClient introuvable.", Color.red);
            }
        }

        private void Update()
        {
            if (!m_joining) return;
            if (Time.time - m_joinStartTime > JOIN_TIMEOUT)
            {
                m_joining = false;
                OnStatusChanged?.Invoke("Timeout — vérifie ta connexion.", Color.yellow);
            }
        }

        public void CreateRoom()
        {
            if (!Connect()) return;
            StartCoroutine(DoAfter(0.6f, () =>
            {
                OnStatusChanged?.Invoke("Création de la room...", Color.cyan);
                m_roomClient.Join("VRMirrorWorld", publish: true);
            }));
        }

        public void JoinWithCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) { OnStatusChanged?.Invoke("Code vide.", Color.yellow); return; }
            if (!Connect()) return;
            StartCoroutine(DoAfter(0.6f, () =>
            {
                OnStatusChanged?.Invoke($"Connexion avec {code}...", Color.cyan);
                m_roomClient.Join(code.Trim());
            }));
        }

        public void SetSolo()
        {
            m_isSolo = true; m_isConnected = true; m_joining = false;
            OnConnected?.Invoke("");
        }

        private IEnumerator DoAfter(float t, System.Action action)
        {
            yield return new WaitForSeconds(t);
            action();
        }

        private bool Connect()
        {
            if (m_roomClient == null) { OnStatusChanged?.Invoke("RoomClient introuvable !", Color.red); return false; }
            if (m_joining) return true;

            m_joining = true;
            m_joinStartTime = Time.time;
            OnStatusChanged?.Invoke("Connexion...", Color.cyan);

            var parts = m_server.Trim().Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int port))
            {
                OnStatusChanged?.Invoke("Serveur invalide.", Color.red);
                m_joining = false; return false;
            }

            string ip = parts[0];

            m_connectionDef            = ScriptableObject.CreateInstance<ConnectionDefinition>();
            m_connectionDef.sendToIp   = ip;
            m_connectionDef.sendToPort = port.ToString();
            m_connectionDef.type       = ConnectionType.TcpClient;
            Debug.Log($"[UbiqSetup] TCP → {ip}:{port}");
            m_roomClient.Connect(m_connectionDef);
            return true;
        }

        private void OnJoinedRoom(IRoom room)
        {
            if (string.IsNullOrEmpty(room.UUID)) return;
            m_joining = false; m_isConnected = true;
            Debug.Log($"[UbiqSetup] ✅ {room.Name} — code : {room.JoinCode}");
            OnStatusChanged?.Invoke("", Color.white);
            OnConnected?.Invoke(room.JoinCode);
        }
    }
}
