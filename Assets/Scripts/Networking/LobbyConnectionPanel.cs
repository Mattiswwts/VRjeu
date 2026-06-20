using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace ActionGame.Networking
{
    public class LobbyConnectionPanel : MonoBehaviour
    {
        [SerializeField] private float m_distance     = 1.6f;
        [SerializeField] private float m_heightOffset = -0.3f; // légèrement sous les yeux

        static readonly Color C_BG     = new Color(0.05f, 0.05f, 0.09f);
        static readonly Color C_CREATE = new Color(0.10f, 0.55f, 0.22f);
        static readonly Color C_JOIN   = new Color(0.12f, 0.32f, 0.78f);
        static readonly Color C_KEY    = new Color(0.18f, 0.18f, 0.32f);
        static readonly Color C_DEL    = new Color(0.60f, 0.12f, 0.12f);
        static readonly Color C_OK     = new Color(0.10f, 0.55f, 0.22f);
        static readonly Color C_SOLO   = new Color(0.18f, 0.12f, 0.05f);
        static readonly Color C_GOLD   = new Color(1.00f, 0.82f, 0.15f);

        private UbiqSetup  m_setup;
        private bool       m_built = false;
        private GameObject m_root;
        private GameObject m_screenMain;
        private GameObject m_screenCode;
        private GameObject m_screenKb;

        private string   m_input    = "";
        private bool     m_shift    = false;   // false = minuscules, true = majuscules
        private TextMesh m_inputTM;
        private TextMesh m_statusTM;
        private TextMesh m_codeTM;
        private TextMesh m_kbStatusTM;
        private TextMesh m_shiftTM;
        private List<TextMesh> m_letterKeys = new List<TextMesh>();

        private string m_codeDisplay = "";
        private string m_codeDesktop = "";
        private string m_statusMsg   = "";
        private Color  m_statusColor = Color.white;

        private void Start()
        {
            m_setup = FindFirstObjectByType<UbiqSetup>();
            if (m_setup == null) return;
            m_setup.OnConnected     += OnConnected;
            m_setup.OnStatusChanged += OnStatus;
        }

        private void Update()
        {
            if (m_setup == null || m_setup.IsConnected) return;
            if (XRCheck.IsVR() && !m_built) { StartCoroutine(Build()); m_built = true; }
        }

        // ─── Construction ─────────────────────────────────────────────────────

        private IEnumerator Build()
        {
            yield return null;
            Camera  cam  = Camera.main;
            Vector3 orig = cam ? cam.transform.position : Vector3.zero;
            Vector3 fwd  = cam ? cam.transform.forward  : Vector3.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < .001f) fwd = Vector3.forward;
            fwd.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

            // Centre du panneau — devant le joueur, légèrement bas
            Vector3 c = orig + fwd * m_distance + Vector3.up * m_heightOffset;

            m_root       = new GameObject("Lobby");
            m_screenMain = BuildMain(c, fwd, right);
            m_screenCode = BuildCodeDisplay(c, fwd, right);
            m_screenKb   = BuildKeyboard(c, fwd, right);

            m_screenCode.SetActive(false);
            m_screenKb.SetActive(false);
        }

        private Quaternion Face(Vector3 fwd) => Quaternion.LookRotation(-fwd, Vector3.up);

        // ── Écran principal ───────────────────────────────────────────────────

        private GameObject BuildMain(Vector3 c, Vector3 fwd, Vector3 R)
        {
            var s = Child("Main");
            Bg(s, c, fwd, 0.88f, 0.58f);
            Txt(s, "VR MIRROR WORLD",       c + V(0,.21f,0), fwd, 0.028f, Color.white, true);
            Txt(s, "Connexion multijoueur",  c + V(0,.12f,0), fwd, 0.015f, new Color(.5f,.5f,.8f));
            Line(s, c + V(0,.06f,0), fwd, 0.80f);

            Btn(s, c + V(0,-.02f,0) - R*.22f, fwd, "CREER",     "Joueur 1", C_CREATE, 0.36f, 0.14f,
                () => { m_setup.CreateRoom(); Show(m_screenCode); });
            Btn(s, c + V(0,-.02f,0) + R*.22f, fwd, "REJOINDRE", "Joueur 2", C_JOIN,   0.36f, 0.14f,
                () => Show(m_screenKb));

            m_statusTM = Txt(s, "Choisissez une action", c + V(0,-.19f,0), fwd, 0.013f, new Color(.5f,.5f,.8f));
            Line(s, c + V(0,-.25f,0), fwd, 0.80f);
            Btn(s, c + V(0,-.33f,0), fwd, "SOLO", "Sans reseau", C_SOLO, 0.24f, 0.09f,
                () => m_setup.SetSolo());
            return s;
        }

        // ── Écran code J1 ─────────────────────────────────────────────────────

        private GameObject BuildCodeDisplay(Vector3 c, Vector3 fwd, Vector3 R)
        {
            var s = Child("CodeDisplay");
            Bg(s, c, fwd, 0.65f, 0.42f);
            Txt(s, "VOTRE CODE",                    c + V(0,.14f,0), fwd, 0.022f, Color.white, true);
            Txt(s, "Dictez-le a l'autre joueur",    c + V(0,.05f,0), fwd, 0.013f, new Color(.6f,.6f,.9f));
            m_codeTM = Txt(s, "...",                c + V(0,-.06f,0), fwd, 0.042f, C_GOLD, true);
            Txt(s, "Connexion en cours...",          c + V(0,-.17f,0), fwd, 0.012f, Color.cyan);
            return s;
        }

        // ── Clavier VR ────────────────────────────────────────────────────────

        private GameObject BuildKeyboard(Vector3 c, Vector3 fwd, Vector3 R)
        {
            var s = Child("Keyboard");
            Bg(s, c + V(0,.08f,0), fwd, 1.10f, 0.95f);

            Txt(s, "ENTREZ LE CODE", c + V(0,.50f,0), fwd, 0.022f, Color.white, true);
            m_inputTM    = Txt(s, "_", c + V(0,.39f,0), fwd, 0.036f, C_GOLD, true);
            m_kbStatusTM = Txt(s, "Appuyez sur les touches puis OK", c + V(0,.30f,0), fwd, 0.013f, new Color(.5f,.5f,.8f));

            float kw = 0.10f, kh = 0.088f, kd = 0.05f, sx = 0.114f;

            BuildRow(s, "1234567890", c + V(0,.18f,0), fwd, R, kw, kh, kd, sx);
            BuildRow(s, "abcdefghij", c + V(0,.07f,0), fwd, R, kw, kh, kd, sx);
            BuildRow(s, "klmnopqrst", c + V(0,-.04f,0), fwd, R, kw, kh, kd, sx);
            BuildRow(s, "uvwxyz",     c + V(0,-.15f,0), fwd, R, kw, kh, kd, sx);

            // SHIFT — bascule maj/min
            float delX = 6 * sx * .5f + sx * .3f;
            var shiftGo = Key(s, c + V(0,-.15f,0) - R * delX, fwd, "min", new Color(.3f,.3f,.5f), 0.14f, kh, kd, false, ToggleShift);
            m_shiftTM = shiftGo.GetComponentInChildren<TextMesh>();

            Key(s, c + V(0,-.15f,0) + R *  delX,       fwd, "DEL", C_DEL, 0.14f, kh, kd, false, DeleteChar);
            Key(s, c + V(0,-.15f,0) + R * (delX+.17f), fwd, "OK",  C_OK,  0.14f, kh, kd, true,  Confirm);

            return s;
        }

        private void BuildRow(GameObject parent, string chars, Vector3 rowCenter,
                              Vector3 fwd, Vector3 R, float kw, float kh, float kd, float sx)
        {
            int n = chars.Length;
            for (int i = 0; i < n; i++)
            {
                string lower = chars[i].ToString().ToLower();
                float  x     = (i - (n - 1) / 2f) * sx;
                var    tm    = Key(parent, rowCenter + R * x, fwd, lower, C_KEY, kw, kh, kd, false,
                                      () => TypeChar(m_shift ? lower.ToUpper() : lower))
                               .GetComponentInChildren<TextMesh>();
                // Ignore les chiffres (pas de shift)
                if (char.IsLetter(chars[i]))
                    m_letterKeys.Add(tm);
            }
        }

        private void ToggleShift()
        {
            m_shift = !m_shift;
            // Met à jour les labels des touches lettres
            foreach (var tm in m_letterKeys)
                tm.text = m_shift ? tm.text.ToUpper() : tm.text.ToLower();
            if (m_shiftTM)
            {
                m_shiftTM.text  = m_shift ? "MAJ" : "min";
                m_shiftTM.color = m_shift ? C_GOLD : Color.white;
            }
        }

        // ─── Saisie ───────────────────────────────────────────────────────────

        private void TypeChar(string ch)
        {
            m_input += ch;
            RefreshInput();
        }

        private void DeleteChar()
        {
            if (m_input.Length == 0) return;
            m_input = m_input.Substring(0, m_input.Length - 1);
            RefreshInput();
        }

        private void RefreshInput()
        {
            if (m_inputTM) m_inputTM.text = m_input.Length > 0 ? m_input + "_" : "_";
        }

        private void Confirm()
        {
            if (m_input.Length == 0)
            {
                if (m_kbStatusTM) { m_kbStatusTM.text = "Code vide !"; m_kbStatusTM.color = Color.red; }
                return;
            }
            if (m_kbStatusTM) { m_kbStatusTM.text = $"Connexion avec '{m_input}'..."; m_kbStatusTM.color = Color.cyan; }
            m_setup.JoinWithCode(m_input);
        }

        private void Show(GameObject screen)
        {
            m_screenMain.SetActive(false);
            m_screenCode.SetActive(false);
            m_screenKb.SetActive(false);
            screen.SetActive(true);
        }

        // ─── Callbacks ────────────────────────────────────────────────────────

        private void OnConnected(string code)
        {
            m_codeDisplay = code;
            if (!string.IsNullOrEmpty(code) && m_codeTM) m_codeTM.text = code;
            SetStatus("Connecte ! Choisissez votre role.", Color.green);
            StartCoroutine(HideDelay(7f));
        }

        private void OnStatus(string msg, Color col) => SetStatus(msg, col);

        private void SetStatus(string msg, Color col)
        {
            m_statusMsg = msg; m_statusColor = col;
            if (m_statusTM) { m_statusTM.text = msg; m_statusTM.color = col; }
            if (m_kbStatusTM) { m_kbStatusTM.text = msg; m_kbStatusTM.color = col; }
        }

        private IEnumerator HideDelay(float t)
        {
            yield return new WaitForSeconds(t);
            if (m_root) m_root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (!m_setup) return;
            m_setup.OnConnected     -= OnConnected;
            m_setup.OnStatusChanged -= OnStatus;
        }

        // ─── Desktop OnGUI ────────────────────────────────────────────────────

        private void OnGUI()
        {
            if (XRCheck.IsVR()) return;
            if (!m_setup || m_setup.IsConnected) return;

            float w = 440f, h = 250f, x = (Screen.width-w)/2f, y = (Screen.height-h)/2f;
            GUI.Box(new Rect(x, y, w, h), "");
            var T  = new GUIStyle(GUI.skin.label)  { fontSize = 18, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            var B  = new GUIStyle(GUI.skin.button) { fontSize = 14 };
            var Cd = new GUIStyle(GUI.skin.label)  { fontSize = 22, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = C_GOLD } };
            var St = new GUIStyle(GUI.skin.label)  { fontSize = 12, alignment = TextAnchor.MiddleCenter, normal = { textColor = m_statusColor }, wordWrap = true };
            var Lb = new GUIStyle(GUI.skin.label)  { fontSize = 13 };

            GUI.Label(new Rect(x, y+8, w, 30), "VR Mirror World — Connexion", T);
            if (GUI.Button(new Rect(x+20, y+44, w-40, 36), "CREER (Joueur 1)", B)) m_setup.CreateRoom();
            if (!string.IsNullOrEmpty(m_codeDisplay))
                GUI.Label(new Rect(x, y+88, w, 28), $"Code : {m_codeDisplay}", Cd);
            GUI.Label(new Rect(x+20, y+124, 80, 24), "Code :", Lb);
            m_codeDesktop = GUI.TextField(new Rect(x+105, y+122, 180, 26), m_codeDesktop);
            if (GUI.Button(new Rect(x+296, y+121, 110, 28), "Rejoindre", B)) m_setup.JoinWithCode(m_codeDesktop);
            if (!string.IsNullOrEmpty(m_statusMsg))
                GUI.Label(new Rect(x+10, y+160, w-20, 40), m_statusMsg, St);
            if (GUI.Button(new Rect(x+110, y+208, 220, 24), "Solo (test)", new GUIStyle(B){fontSize=11, normal={textColor=Color.yellow}}))
                m_setup.SetSolo();
        }

        // ─── Factories ────────────────────────────────────────────────────────

        // Key : reactivable = true pour les boutons uniques (OK/DEL one-shot),
        //       false pour les touches du clavier (réutilisables)
        private GameObject Key(GameObject parent, Vector3 pos, Vector3 fwd,
                         string label, Color col, float w, float h, float d,
                         bool oneShot, System.Action action)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"K_{label}";
            go.transform.SetParent(parent.transform, true);
            go.transform.position   = pos;
            go.transform.localScale = new Vector3(w, h, d);
            go.transform.rotation   = Face(fwd);
            Paint(go, col);

            var xi = go.AddComponent<XRSimpleInteractable>();
            if (oneShot)
                xi.selectEntered.AddListener(_ => StartCoroutine(SafeOnce(xi, action)));
            else
                xi.selectEntered.AddListener(_ => StartCoroutine(SafeReuse(xi, action)));

            var tgo = new GameObject("L");
            tgo.transform.SetParent(go.transform, false);
            tgo.transform.localPosition = new Vector3(0, 0, -1.1f);
            tgo.transform.localScale    = Vector3.one * (.016f / h);
            var tm = tgo.AddComponent<TextMesh>();
            tm.text = label; tm.fontSize = 60; tm.color = Color.white;
            tm.fontStyle = FontStyle.Bold;
            tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center;
            return go;
        }

        private void Btn(GameObject parent, Vector3 pos, Vector3 fwd,
                         string title, string sub, Color col, float w, float h, System.Action action)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Btn_{title}";
            go.transform.SetParent(parent.transform, true);
            go.transform.position   = pos;
            go.transform.localScale = new Vector3(w, h, 0.05f);
            go.transform.rotation   = Face(fwd);
            Paint(go, col);
            var xi = go.AddComponent<XRSimpleInteractable>();
            xi.selectEntered.AddListener(_ => StartCoroutine(SafeOnce(xi, action)));
            Label(go.transform, title, new Vector3(0, .10f, -1.1f), .018f / h, true);
            Label(go.transform, sub,   new Vector3(0,-.14f, -1.1f), .011f / h, false);
        }

        private static void Label(Transform parent, string txt, Vector3 lp, float scale, bool bold)
        {
            var go = new GameObject("L"); go.transform.SetParent(parent, false);
            go.transform.localPosition = lp;
            go.transform.localScale    = Vector3.one * scale;
            var tm = go.AddComponent<TextMesh>();
            tm.text = txt; tm.fontSize = 60;
            tm.color = bold ? Color.white : new Color(1,1,1,.65f);
            tm.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center;
        }

        private TextMesh Txt(GameObject parent, string txt, Vector3 pos, Vector3 fwd,
                             float scale, Color col, bool bold = false)
        {
            var go = new GameObject("T"); go.transform.SetParent(parent.transform, true);
            go.transform.position   = pos;
            go.transform.rotation   = Face(fwd);
            go.transform.localScale = Vector3.one * scale;
            var tm = go.AddComponent<TextMesh>();
            tm.text = txt; tm.fontSize = 60; tm.color = col;
            tm.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center;
            return tm;
        }

        private void Bg(GameObject parent, Vector3 pos, Vector3 fwd, float w, float h)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "BG"; go.transform.SetParent(parent.transform, true);
            go.transform.position   = pos;
            go.transform.rotation   = Face(fwd);
            go.transform.localScale = new Vector3(w, h, 0.005f);
            Destroy(go.GetComponent<Collider>()); Paint(go, C_BG);
        }

        private void Line(GameObject parent, Vector3 pos, Vector3 fwd, float w)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Line"; go.transform.SetParent(parent.transform, true);
            go.transform.position   = pos;
            go.transform.rotation   = Face(fwd);
            go.transform.localScale = new Vector3(w, 0.003f, 0.004f);
            Destroy(go.GetComponent<Collider>()); Paint(go, new Color(.25f,.25f,.45f));
        }

        private static void Paint(GameObject go, Color col)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = col; go.GetComponent<Renderer>().material = mat;
        }

        private GameObject Child(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(m_root.transform, true);
            return go;
        }

        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);

        // Bouton principal — désactivé après clic (évite double-clic)
        private IEnumerator SafeOnce(XRSimpleInteractable xi, System.Action action)
        {
            Release(xi); xi.enabled = false;
            yield return null; yield return null;
            action();
        }

        // Touche clavier — réactivée après 0.2s pour pouvoir retaper
        private IEnumerator SafeReuse(XRSimpleInteractable xi, System.Action action)
        {
            Release(xi); xi.enabled = false;
            action();
            yield return new WaitForSeconds(0.2f);
            xi.enabled = true;
        }

        private void Release(XRSimpleInteractable xi)
        {
            var mgr = FindFirstObjectByType<XRInteractionManager>();
            if (mgr == null) return;
            var sel = new List<IXRSelectInteractor>(xi.interactorsSelecting);
            foreach (var i in sel) mgr.SelectExit(i, xi);
        }
    }
}
