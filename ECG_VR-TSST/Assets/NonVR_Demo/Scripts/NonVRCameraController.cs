using UnityEngine;

namespace NonVRDemo
{
    /// <summary>
    /// Provides first-person mouse-look rotation and natural human eye-height for desktop/non-VR demonstration.
    /// Simulates the participant's head orientation in 360 degrees and elevates the camera from floor level.
    /// </summary>
    public class NonVRCameraController : MonoBehaviour
    {
        [Header("Look Settings")]
        [Tooltip("Mouse sensitivity multiplier")]
        [SerializeField] private float mouseSensitivity = 2.0f;

        [Tooltip("Minimum vertical angle (looking down)")]
        [SerializeField] private float minPitch = -80f;

        [Tooltip("Maximum vertical angle (looking up)")]
        [SerializeField] private float maxPitch = 80f;

        [Tooltip("Smooth mouse movement")]
        [SerializeField] private bool smooth = true;

        [SerializeField] private float smoothTime = 15f;

        [Header("Height & Vantage Settings")]
        [Tooltip("Standard standing eye height above the floor (meters)")]
        [SerializeField] private float standingEyeHeight = 1.68f;

        [Tooltip("Standard sitting eye height above the floor (meters)")]
        [SerializeField] private float sittingEyeHeight = 1.25f;

        [SerializeField] private bool isSitting = false;

        [Header("Movement & Positioning")]
        [Tooltip("Allow gentle WASD/Arrow/QE position nudging to adjust vantage point")]
        [SerializeField] private bool allowPositionNudge = true;

        [SerializeField] private float moveSpeed = 1.5f;

        [Header("State")]
        [SerializeField] private bool isLookModeActive = true;

        private float targetYaw;
        private float targetPitch;
        private float currentYaw;
        private float currentPitch;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        public bool IsLookModeActive => isLookModeActive;
        public bool IsSitting => isSitting;

        private void Awake()
        {
            EnsureHumanEyeHeight();
            initialPosition = transform.position;
            initialRotation = transform.localRotation;
            ResetAngles();
            DisableAnyVRTrackingDrivers();
        }

        private void OnEnable()
        {
            EnsureHumanEyeHeight();
            ResetAngles();
            DisableAnyVRTrackingDrivers();
            SetLookMode(true);
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// In VR, the HMD hardware automatically elevates the camera above floor level.
        /// Without VR, the camera defaults to local Y = 0 (on the floor), making everything look giant.
        /// This ensures the camera is placed at natural human eye level.
        /// </summary>
        public void EnsureHumanEyeHeight()
        {
            if (transform.localPosition.y < 0.5f)
            {
                Vector3 pos = transform.localPosition;
                pos.y = isSitting ? sittingEyeHeight : standingEyeHeight;
                transform.localPosition = pos;
            }

            // Expand FOV on flat monitor for comfortable viewing
            Camera cam = GetComponent<Camera>();
            if (cam != null && cam.fieldOfView < 70f)
            {
                cam.fieldOfView = 75f;
            }
        }

        private void ResetAngles()
        {
            Vector3 euler = transform.localEulerAngles;
            targetYaw = euler.y;
            currentYaw = euler.y;

            // Normalize pitch to -180..180
            targetPitch = (euler.x > 180f) ? euler.x - 360f : euler.x;
            currentPitch = targetPitch;
        }

        /// <summary>
        /// Smoothly turns the camera to face a world position (e.g. wall instructions).
        /// </summary>
        public void LookAtPosition(Vector3 worldTarget)
        {
            Vector3 dir = worldTarget - transform.position;
            if (dir.sqrMagnitude < 0.001f) return;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            Vector3 euler = lookRot.eulerAngles;
            targetYaw = euler.y;
            targetPitch = (euler.x > 180f) ? euler.x - 360f : euler.x;
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
        }

        /// <summary>
        /// Sets the target yaw and pitch angles directly.
        /// </summary>
        public void SetTargetAngles(float yaw, float pitch)
        {
            targetYaw = yaw;
            targetPitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        /// <summary>
        /// Disables TrackedPoseDriver if present on this GameObject to prevent VR tracking overrides.
        /// </summary>
        private void DisableAnyVRTrackingDrivers()
        {
            foreach (var comp in GetComponents<MonoBehaviour>())
            {
                if (comp != null && comp != this)
                {
                    string typeName = comp.GetType().Name.ToLower();
                    if (typeName.Contains("trackedpose") || typeName.Contains("cameratrack") || typeName.Contains("xrnode"))
                    {
                        comp.enabled = false;
                        Debug.Log($"[NonVRCameraController] Disabled VR tracker: {comp.GetType().Name} on {gameObject.name}");
                    }
                }
            }
        }

        private void Update()
        {
            HandleCursorAndModeToggles();

            if (isLookModeActive)
            {
                HandleMouseLook();
            }

            if (allowPositionNudge)
            {
                HandlePositionNudge();
            }

            // Press C to toggle Sitting / Standing height
            if (Input.GetKeyDown(KeyCode.C))
            {
                isSitting = !isSitting;
                Vector3 pos = transform.localPosition;
                pos.y = isSitting ? sittingEyeHeight : standingEyeHeight;
                transform.localPosition = pos;
                Debug.Log($"[NonVRCameraController] Posture switched: {(isSitting ? "Sitting (1.25m)" : "Standing (1.68m)")}");
            }

            // Press R to reset camera position to original room vantage point
            if (Input.GetKeyDown(KeyCode.R))
            {
                transform.position = initialPosition;
                EnsureHumanEyeHeight();
                ResetAngles();
                transform.localRotation = initialRotation;
            }
        }

        private void HandleCursorAndModeToggles()
        {
            // Pressing Tab or Escape toggles between Look Mode and Free Cursor Mode
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
            {
                SetLookMode(!isLookModeActive);
            }

            // Holding Right Mouse Button temporarily enters Look Mode if currently free
            if (Input.GetMouseButtonDown(1) && !isLookModeActive)
            {
                SetLookMode(true);
            }

            // Clicking left mouse button on the screen when free re-engages Look Mode (unless clicking UI)
            if (Input.GetMouseButtonDown(0) && !isLookModeActive)
            {
                if (!UnityEngine.EventSystems.EventSystem.current || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    SetLookMode(true);
                }
            }
        }

        public void SetLookMode(bool active)
        {
            isLookModeActive = active;
            if (isLookModeActive)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            targetYaw += mouseX;
            targetPitch -= mouseY;
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

            if (smooth)
            {
                currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * smoothTime);
                currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * smoothTime);
            }
            else
            {
                currentYaw = targetYaw;
                currentPitch = targetPitch;
            }

            transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        }

        private void HandlePositionNudge()
        {
            float h = 0f;
            float v = 0f;
            float y = 0f;

            // Support WASD and Arrow keys
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;

            // E / Q to raise / lower camera height
            if (Input.GetKey(KeyCode.E)) y += 1f;
            if (Input.GetKey(KeyCode.Q)) y -= 1f;

            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f || Mathf.Abs(y) > 0.01f)
            {
                Vector3 forward = transform.forward;
                Vector3 right = transform.right;
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();

                Vector3 direction = forward * v + right * h + Vector3.up * y;
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
        }
    }
}
