using UnityEngine;
using UnityEngine.SceneManagement;

namespace NonVRDemo
{
    /// <summary>
    /// UI Controller and entry point for the Non-VR Demo Launcher scene.
    /// Provides an immediate GUI and button callbacks for starting the test.
    /// </summary>
    public class NonVRLauncher : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [Tooltip("Index or name of the MainLogic scene")]
        [SerializeField] private string mainLogicSceneName = "MainLogic";

        [Tooltip("Index or name of the MainMenu configuration scene")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private GUIStyle _cardStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _bodyStyle;
        private bool _stylesInitialized = false;

        private void Awake()
        {
            NonVRBootstrap.EnsureInstance();
        }

        public void StartDemo()
        {
            NonVRBootstrap.EnsureInstance();
            SceneManager.LoadScene(mainLogicSceneName);
        }

        public void OpenConfigMenu()
        {
            NonVRBootstrap.EnsureInstance();
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            Texture2D cardTex = new Texture2D(1, 1);
            cardTex.SetPixel(0, 0, new Color(0.11f, 0.12f, 0.16f, 0.95f));
            cardTex.Apply();

            _cardStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = cardTex },
                padding = new RectOffset(30, 30, 25, 25)
            };

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.35f, 0.8f, 1f) }
            };

            _subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.75f, 0.8f, 0.85f) }
            };

            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = new Color(0.85f, 0.88f, 0.92f) }
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                fixedHeight = 44,
                margin = new RectOffset(0, 0, 8, 8)
            };

            _stylesInitialized = true;
        }

        private void Update()
        {
            // Keep cursor free while in the launcher menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnGUI()
        {
            // Do not draw launcher menu if loaded into main experiment scenes
            string activeScene = SceneManager.GetActiveScene().name;
            if (activeScene == "MainLogic" || activeScene == "Rooms" || activeScene == "MainMenu")
            {
                return;
            }

            InitStyles();

            float cardWidth = 520f;
            float cardHeight = 460f;
            float posX = (Screen.width - cardWidth) * 0.5f;
            float posY = (Screen.height - cardHeight) * 0.5f;

            Rect cardRect = new Rect(posX, posY, cardWidth, cardHeight);

            GUILayout.BeginArea(cardRect, _cardStyle);
            {
                GUILayout.Label("ECG VR-TSST", _titleStyle);
                GUILayout.Label("Trier Social Stress Test  Desktop Non-VR Demo", _subtitleStyle);
                GUILayout.Space(15);

                GUILayout.Label(
                    "This standalone demo allows you to inspect and run the complete VR-TSST experiment without a VR headset on standard PC/laptop displays.",
                    _bodyStyle);

                GUILayout.Space(10);
                GUILayout.Label("<b>Non-VR Controls:</b>\n" +
                                "  [Tab / Esc / Right-Click]  Toggle between Mouse Look and Free Cursor\n" +
                                "  [Mouse]  360 Head-Look rotation (simulates VR headset)\n" +
                                "  [1 to 9]  Researcher Test Action Shortcuts (Next, Repeat, Wrong, etc.)\n" +
                                "  [WASD / Arrows]  Fine vantage point nudge | [R] Reset view",
                                _bodyStyle);

                GUILayout.Space(18);

                GUI.backgroundColor = new Color(0.2f, 0.65f, 0.95f);
                if (GUILayout.Button("  Start Non-VR Experiment (Default Settings)", _buttonStyle))
                {
                    StartDemo();
                }

                GUI.backgroundColor = new Color(0.3f, 0.45f, 0.6f);
                if (GUILayout.Button("  Open Configuration Menu (MainMenu)", _buttonStyle))
                {
                    OpenConfigMenu();
                }

                GUI.backgroundColor = new Color(0.45f, 0.25f, 0.25f);
                if (GUILayout.Button("  Quit Demo", _buttonStyle))
                {
                    QuitApplication();
                }

                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndArea();
        }
    }
}
