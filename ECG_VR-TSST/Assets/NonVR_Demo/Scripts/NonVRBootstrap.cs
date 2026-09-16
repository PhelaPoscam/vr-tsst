using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NonVRDemo
{
    /// <summary>
    /// Bootstraps and manages Non-VR mode for the ECG VR-TSST project.
    /// Automatically manages camera look, suppresses VR display subsystems,
    /// and renders an optional on-screen control helper HUD.
    /// </summary>
    public class NonVRBootstrap : MonoBehaviour
    {
        private static NonVRBootstrap s_instance;
        public static NonVRBootstrap Instance => s_instance;

        private static bool s_isNonVRModeActive = false;
        public static bool IsNonVRModeActive
        {
            get => s_isNonVRModeActive;
            set
            {
                s_isNonVRModeActive = value;
                if (s_isNonVRModeActive && Application.isPlaying)
                {
                    EnsureInstance();
                }
            }
        }

        [Header("HUD Settings")]
        [SerializeField] private bool showHelpOverlay = true;
        [SerializeField] private bool showInstructionBanner = true;

        private Camera _lastActiveCamera;
        private NonVRCameraController _currentController;
        private GUIStyle _panelStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _keyStyle;

        private GUIStyle _bannerPanelStyle;
        private GUIStyle _bannerHeaderStyle;
        private GUIStyle _bannerTextStyle;
        private GUIStyle _bannerSubStyle;
        private bool _guiStylesInitialized = false;

        private string _activeInstructionText;
        private string _activeInstructionTranslation;
        private Transform _activeInstructionTransform;
        private bool _hasAutoOrientedToWall = false;
        private string _lastActiveInstructionText;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnApplicationStart()
        {
            // Check command-line flags (e.g., -nonvr or -desktop)
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i].ToLowerInvariant();
                if (a == "-nonvr" || a == "-novr" || a == "-desktop")
                {
                    IsNonVRModeActive = true;
                    break;
                }
            }
        }

        public static void EnsureInstance()
        {
            if (!Application.isPlaying) return;
            if (s_instance != null) return;

            GameObject go = new GameObject("[NonVR_Bootstrap]");
            s_instance = go.AddComponent<NonVRBootstrap>();
            DontDestroyOnLoad(go);
            s_isNonVRModeActive = true;
        }

        private void Awake()
        {
            if (!Application.isPlaying) return;

            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            s_isNonVRModeActive = true;
            DontDestroyOnLoad(gameObject);

            DeactivateXRSubsystems();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (s_instance == this) s_instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            DeactivateXRSubsystems();
            UpdateCameraRigControllers();
        }

        private void Update()
        {
            // Toggle help HUD with 'H'
            if (Input.GetKeyDown(KeyCode.H))
            {
                showHelpOverlay = !showHelpOverlay;
            }

            // Continuously check if active camera changed (e.g. room switch)
            if (Camera.main != _lastActiveCamera)
            {
                UpdateCameraRigControllers();
            }

            UpdateActiveInstructions();
        }

        private void UpdateActiveInstructions()
        {
#if UNITY_2023_1_OR_NEWER
            var waitingRoom = UnityEngine.Object.FindAnyObjectByType<WaitingRoomTask>();
#else
            var waitingRoom = UnityEngine.Object.FindObjectOfType<WaitingRoomTask>();
#endif
            UnityEngine.UI.Text activeComp = null;
            if (waitingRoom != null)
            {
                if (waitingRoom._initalText != null && waitingRoom._initalText.enabled && !string.IsNullOrEmpty(waitingRoom._initalText.text))
                {
                    activeComp = waitingRoom._initalText;
                }
                else if (waitingRoom._surveillanceText != null && waitingRoom._surveillanceText.enabled && !string.IsNullOrEmpty(waitingRoom._surveillanceText.text))
                {
                    activeComp = waitingRoom._surveillanceText;
                }
                else if (waitingRoom._readyText != null && waitingRoom._readyText.enabled && !string.IsNullOrEmpty(waitingRoom._readyText.text))
                {
                    activeComp = waitingRoom._readyText;
                }
            }

            if (activeComp != null)
            {
                _activeInstructionText = activeComp.text.Trim();
                _activeInstructionTransform = activeComp.transform;
                _activeInstructionTranslation = GetTranslation(_activeInstructionText);

                // Ensure 3D text in the room is 100% visible:
                Canvas canvas = activeComp.GetComponentInParent<Canvas>(true);
                if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
                {
                    if (canvas.worldCamera == null && Camera.main != null)
                    {
                        canvas.worldCamera = Camera.main;
                    }
                }

                if (activeComp.color.a < 0.95f)
                {
                    Color c = activeComp.color;
                    c.a = 1.0f;
                    activeComp.color = c;
                }

                if (_lastActiveInstructionText != _activeInstructionText)
                {
                    _lastActiveInstructionText = _activeInstructionText;
                    _hasAutoOrientedToWall = false;
                    Debug.Log($"[NonVRBootstrap] Active Wall Instruction: '{_activeInstructionText}' (at: {_activeInstructionTransform.position})");
                }

                // Smoothly orient camera to face the wall text on initial appearance
                if (!_hasAutoOrientedToWall && _currentController != null)
                {
                    _currentController.LookAtPosition(_activeInstructionTransform.position);
                    _hasAutoOrientedToWall = true;
                    Debug.Log($"[NonVRBootstrap] Auto-oriented view to wall instructions at {_activeInstructionTransform.position}");
                }
            }
            else
            {
                _activeInstructionText = null;
                _activeInstructionTranslation = null;
                _activeInstructionTransform = null;
            }

            // Press F to face wall instructions anytime
            if (Input.GetKeyDown(KeyCode.F) && _currentController != null)
            {
                if (_activeInstructionTransform != null)
                {
                    _currentController.LookAtPosition(_activeInstructionTransform.position);
                    Debug.Log($"[NonVRBootstrap] Turned camera to face wall instruction at {_activeInstructionTransform.position}");
                }
                else
                {
                    // Fallback: If in waiting room (x < -3), wall is at yaw ~ 125
                    if (_currentController.transform.position.x < -3f)
                    {
                        _currentController.SetTargetAngles(125f, 0f);
                        Debug.Log("[NonVRBootstrap] Turned camera to face Waiting Room wall (yaw 125)");
                    }
                    else
                    {
                        _currentController.SetTargetAngles(0f, 0f);
                    }
                }
            }
        }

        private string GetTranslation(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            if (text.Contains("Bitte haben Sie noch einen Moment Geduld"))
            {
                return "Please be patient for a moment! You will soon be able to enter the examiner's office to start the tasks.";
            }
            if (text.Contains("aufgezeichnet") || text.Contains("Bild als auch Ton"))
            {
                return "In the subsequent conversation, both image and sound will be recorded!";
            }
            if (text.Contains("Bitte stehen Sie auf"))
            {
                return "Please stand up. The examiner(s) are now ready for you.";
            }

            if (text.Contains("Please be patient"))
            {
                return "Bitte haben Sie noch einen Moment Geduld! Sie können gleich zu dem/den Prüfer(n) ins Büro, um mit den Aufgaben zu starten.";
            }
            if (text.Contains("both picture and sound will be recorded"))
            {
                return "Im später folgenden Gespräch werden sowohl Bild als auch Ton aufgezeichnet werden!";
            }
            if (text.Contains("Please stand up"))
            {
                return "Bitte stehen Sie auf. Der/Die Prüfer ist/sind jetzt für Sie bereit.";
            }

            return null;
        }

        /// <summary>
        /// Stops any active XR display subsystems so Unity renders to the standard desktop monitor.
        /// </summary>
        public void DeactivateXRSubsystems()
        {
            try
            {
                List<UnityEngine.XR.XRDisplaySubsystem> displays = new List<UnityEngine.XR.XRDisplaySubsystem>();
                SubsystemManager.GetSubsystems(displays);
                foreach (var display in displays)
                {
                    if (display.running)
                    {
                        display.Stop();
                        Debug.Log($"[NonVRBootstrap] Stopped XR display subsystem: {display.subsystemDescriptor?.id}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NonVRBootstrap] Notice during XR subsystem check: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds cameras inside known rigs (Waiting Room XRRig, Main XRRig) or Camera.main
        /// and ensures NonVRCameraController is attached.
        /// </summary>
        public void UpdateCameraRigControllers()
        {
            // Scan for all cameras in the scene
#if UNITY_2023_1_OR_NEWER
            Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            Camera[] allCameras = FindObjectsOfType<Camera>(true);
#endif
            foreach (Camera cam in allCameras)
            {
                // Only attach to main camera or cameras inside rigs
                Transform parent = cam.transform;
                bool isTSSTCamera = false;

                while (parent != null)
                {
                    string pName = parent.name.ToLower();
                    if (pName.Contains("xrrig") || pName.Contains("rig") || pName.Contains("office") || pName.Contains("waiting"))
                    {
                        isTSSTCamera = true;
                        break;
                    }
                    parent = parent.parent;
                }

                if (isTSSTCamera || cam.CompareTag("MainCamera"))
                {
                    NonVRCameraController controller = cam.GetComponent<NonVRCameraController>();
                    if (controller == null)
                    {
                        controller = cam.gameObject.AddComponent<NonVRCameraController>();
                        Debug.Log($"[NonVRBootstrap] Attached NonVRCameraController to camera: {cam.gameObject.name}");
                    }
                }
            }

            _lastActiveCamera = Camera.main;
            if (_lastActiveCamera != null)
            {
                _currentController = _lastActiveCamera.GetComponent<NonVRCameraController>();
            }
        }

        private void InitGUIStyles()
        {
            if (_guiStylesInitialized) return;

            Texture2D bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.08f, 0.09f, 0.12f, 0.88f));
            bgTex.Apply();

            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = bgTex },
                padding = new RectOffset(12, 12, 10, 10)
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.35f, 0.8f, 1f) }
            };

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = Color.white }
            };

            _keyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.3f) }
            };

            Texture2D bannerBg = new Texture2D(1, 1);
            bannerBg.SetPixel(0, 0, new Color(0.05f, 0.07f, 0.11f, 0.94f));
            bannerBg.Apply();

            _bannerPanelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = bannerBg },
                padding = new RectOffset(16, 16, 10, 10)
            };

            _bannerHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.82f, 0.25f) }
            };

            _bannerTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            _bannerSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Italic,
                wordWrap = true,
                normal = { textColor = new Color(0.45f, 0.85f, 1f) }
            };

            _guiStylesInitialized = true;
        }

        private void OnGUI()
        {
            InitGUIStyles();

            // 1. Render Top Wall Instruction Banner if active
            if (showInstructionBanner && !string.IsNullOrEmpty(_activeInstructionText))
            {
                float bannerWidth = Mathf.Min(Screen.width - 60f, 750f);
                float bannerX = (Screen.width - bannerWidth) * 0.5f;

                GUILayout.BeginArea(new Rect(bannerX, 16f, bannerWidth, 150f), _bannerPanelStyle);
                {
                    GUILayout.Label("ROOM INSTRUCTIONS  [Press 'F' to Face Wall]", _bannerHeaderStyle);
                    GUILayout.Space(2);
                    GUILayout.Label(_activeInstructionText, _bannerTextStyle);

                    if (!string.IsNullOrEmpty(_activeInstructionTranslation))
                    {
                        GUILayout.Space(2);
                        GUILayout.Label("Translation: " + _activeInstructionTranslation, _bannerSubStyle);
                    }
                }
                GUILayout.EndArea();
            }

            if (!showHelpOverlay) return;

            // 2. Render bottom-left corner HUD
            float width = 330f;
            float height = 230f;
            float margin = 15f;
            Rect rect = new Rect(margin, Screen.height - height - margin, width, height);

            GUILayout.BeginArea(rect, _panelStyle);
            {
                GUILayout.Label("ECG VR-TSST  Non-VR Demo", _headerStyle);
                GUILayout.Space(3);

                string modeText = (_currentController != null && _currentController.IsLookModeActive)
                    ? "<color=#55ff55>LOOK MODE (Active)</color>"
                    : "<color=#ffaa33>CURSOR MODE (Free)</color>";

                string postureText = (_currentController != null && _currentController.IsSitting)
                    ? "<color=#88ccff>Sitting (1.25m)</color>"
                    : "<color=#88ccff>Standing (1.68m)</color>";

                GUILayout.Label($"Camera: {modeText} | {postureText}", _labelStyle);
                GUILayout.Space(5);

                GUILayout.Label("Controls:", _headerStyle);
                GUILayout.Label("[F]: Face Wall Instructions (Auto-turn)", _keyStyle);
                GUILayout.Label("[Tab / Esc / Right-Click]: Toggle Look / Cursor", _keyStyle);
                GUILayout.Label("[Mouse]: Look around 360", _labelStyle);
                GUILayout.Label("[C]: Toggle Stand / Sit | [Q / E]: Height +/-", _keyStyle);
                GUILayout.Label("[WASD / Arrows]: Move around | [R]: Reset", _labelStyle);
                GUILayout.Label("[1  9]: TSST Researcher Actions", _keyStyle);
                GUILayout.Label("[H]: Hide / Show this help overlay", _labelStyle);
            }
            GUILayout.EndArea();
        }
    }
}
