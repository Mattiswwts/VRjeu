using UnityEngine;
using UnityEngine.InputSystem;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.Rooms;

namespace ActionGame.Networking
{
    /// <summary>
    /// Affiche un écran de sélection de rôle au démarrage.
    /// Spawne le bon prefab joueur via Ubiq selon le choix.
    ///
    /// Setup requis :
    /// - NetworkSpawnManager sur le NetworkScene (avec PrefabCatalogue)
    /// - PrefabCatalogue[0] = prefab Acteur, PrefabCatalogue[1] = prefab Réalisateur
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [Header("Prefabs joueurs (doivent être dans le PrefabCatalogue)")]
        [SerializeField] private GameObject m_actorPrefab;
        [SerializeField] private GameObject m_directorPrefab;

        [Header("Points de spawn")]
        [SerializeField] private Transform m_actorSpawnPoint;
        [SerializeField] private Transform m_directorSpawnPoint;

        private NetworkSpawnManager m_spawnManager;
        private bool m_roleSelected = false;
        private bool m_showUI = true;

        private void Start()
        {
            m_spawnManager = NetworkScene.Find(this).GetComponentInChildren<NetworkSpawnManager>();

            if (m_spawnManager == null)
                Debug.LogError("[PlayerSpawner] NetworkSpawnManager introuvable sur le NetworkScene !");
        }

        private void Update()
        {
            if (m_roleSelected) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            // Sélection du rôle au clavier
            if (kb.f1Key.wasPressedThisFrame) SpawnPlayer(isActor: true);
            if (kb.f2Key.wasPressedThisFrame) SpawnPlayer(isActor: false);
        }

        private void SpawnPlayer(bool isActor)
        {
            if (m_spawnManager == null) return;

            GameObject prefab = isActor ? m_actorPrefab : m_directorPrefab;
            Transform spawnPoint = isActor ? m_actorSpawnPoint : m_directorSpawnPoint;

            if (prefab == null)
            {
                Debug.LogError($"[PlayerSpawner] Prefab non assigné dans l'Inspector !");
                return;
            }

            // Spawn via Ubiq — visible par tous les peers
            GameObject spawned = m_spawnManager.SpawnWithPeerScope(prefab);

            // Positionne au bon endroit
            if (spawnPoint != null)
                spawned.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            // Marque comme propriétaire local
            var sync = spawned.GetComponent<NetworkedPlayerSync>();
            if (sync != null) sync.SetOwner(true);

            m_roleSelected = true;
            m_showUI = false;

            Debug.Log($"[PlayerSpawner] Rôle choisi : {(isActor ? "ACTEUR" : "RÉALISATEUR")}");
        }

        // ─── UI de sélection de rôle ─────────────────────────────────────────

        private void OnGUI()
        {
            if (!m_showUI) return;

            // Panneau centré à l'écran
            float w = 400f, h = 200f;
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

            GUI.Label(new Rect(x, y + 10, w, 40), "Choisir votre rôle", titleStyle);

            if (GUI.Button(new Rect(x + 30, y + 70, 160, 50), "F1 — Acteur", btnStyle))
                SpawnPlayer(isActor: true);

            if (GUI.Button(new Rect(x + 210, y + 70, 160, 50), "F2 — Réalisateur", btnStyle))
                SpawnPlayer(isActor: false);

            GUIStyle hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(x, y + 140, w, 30), "Ou appuyez sur F1 / F2", hintStyle);
        }
    }
}
