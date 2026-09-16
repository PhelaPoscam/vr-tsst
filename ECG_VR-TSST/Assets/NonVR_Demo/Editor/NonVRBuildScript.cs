using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NonVRDemo.Editor
{
    /// <summary>
    /// Editor utilities for playing and building the isolated Non-VR demo.
    /// Does not alter existing project settings or build pipelines.
    /// </summary>
    [InitializeOnLoad]
    public static class NonVRBuildScript
    {
        private const string NonVRModePrefKey = "ECG_VR_TSST_NonVR_Mode_Enabled";
        private const string LauncherScenePath = "Assets/NonVR_Demo/Scenes/NonVR_Launcher.unity";
        private const string MainLogicScenePath = "Assets/Scenes/MainLogic.unity";
        private const string RoomsScenePath = "Assets/Scenes/Rooms.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        static NonVRBuildScript()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (EditorPrefs.GetBool(NonVRModePrefKey, false))
                {
                    Debug.Log("[NonVR] Non-VR Mode active for Play Mode session.");
                    NonVRBootstrap.IsNonVRModeActive = true;
                }
            }
        }

        [MenuItem("Tools/ECG VR-TSST/Start Non-VR Demo (Direct to 3D Room)", false, 10)]
        public static void PlayNonVRDemoDirect()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorPrefs.SetBool(NonVRModePrefKey, true);
                EditorSceneManager.OpenScene(MainLogicScenePath);
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("Tools/ECG VR-TSST/Start Non-VR Demo (via Main Menu)", false, 11)]
        public static void PlayNonVRDemoMainMenu()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorPrefs.SetBool(NonVRModePrefKey, true);
                EditorSceneManager.OpenScene(MainMenuScenePath);
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("Tools/ECG VR-TSST/Start Non-VR Demo (via Launcher Menu)", false, 12)]
        public static void PlayNonVRDemoLauncher()
        {
            EnsureLauncherSceneExists();

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorPrefs.SetBool(NonVRModePrefKey, true);
                EditorSceneManager.OpenScene(LauncherScenePath);
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("Tools/ECG VR-TSST/Toggle Non-VR Mode for Play Mode", false, 15)]
        public static void ToggleNonVRMode()
        {
            bool current = EditorPrefs.GetBool(NonVRModePrefKey, false);
            bool newState = !current;
            EditorPrefs.SetBool(NonVRModePrefKey, newState);
            Debug.Log($"[NonVR] Non-VR Mode for Play Mode is now: {(newState ? "ENABLED (Runs on Desktop)" : "DISABLED (Runs in VR)")}");
        }

        [MenuItem("Tools/ECG VR-TSST/Toggle Non-VR Mode for Play Mode", true)]
        public static bool ToggleNonVRModeValidate()
        {
            Menu.SetChecked("Tools/ECG VR-TSST/Toggle Non-VR Mode for Play Mode", EditorPrefs.GetBool(NonVRModePrefKey, false));
            return true;
        }

        [MenuItem("Tools/ECG VR-TSST/Generate or Refresh Non-VR Launcher Scene", false, 25)]
        public static void GenerateLauncherScene()
        {
            EnsureLauncherSceneExists(true);
        }

        [MenuItem("Tools/ECG VR-TSST/Build Standalone Non-VR Demo (Windows)", false, 30)]
        public static void BuildStandaloneNonVRDemo()
        {
            EnsureLauncherSceneExists();

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string buildFolder = Path.Combine(projectRoot, "Build_NonVR_Demo");
            string exePath = Path.Combine(buildFolder, "ECG_VR-TSST_NonVR.exe");

            if (!Directory.Exists(buildFolder))
            {
                Directory.CreateDirectory(buildFolder);
            }

            string[] scenesToBuild = new string[]
            {
                LauncherScenePath,
                MainLogicScenePath,
                RoomsScenePath,
                MainMenuScenePath
            };

            // Verify scenes exist
            foreach (string scenePath in scenesToBuild)
            {
                if (!File.Exists(scenePath))
                {
                    EditorUtility.DisplayDialog("Build Error", $"Required scene not found: {scenePath}", "OK");
                    return;
                }
            }

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenesToBuild,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"[NonVRBuildScript] Starting Non-VR standalone build to: {exePath}");
            var report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[NonVRBuildScript] Build Succeeded! Output: {exePath}");
                EditorUtility.DisplayDialog("Build Completed",
                    $"Non-VR Demo built successfully!\n\nLocation:\n{exePath}", "Open Folder");
                EditorUtility.RevealInFinder(exePath);
            }
            else
            {
                Debug.LogError($"[NonVRBuildScript] Build failed with {report.summary.totalErrors} errors.");
                EditorUtility.DisplayDialog("Build Failed", "Non-VR Demo build encountered errors. Check Console for details.", "OK");
            }
        }

        public static void EnsureLauncherSceneExists(bool forceRecreate = false)
        {
            string dir = Path.GetDirectoryName(LauncherScenePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            if (!File.Exists(LauncherScenePath) || forceRecreate)
            {
                Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

                // Add launcher manager object
                GameObject launcherGo = new GameObject("NonVR_Demo_Manager");
                launcherGo.AddComponent<NonVRBootstrap>();
                launcherGo.AddComponent<NonVRLauncher>();

                EditorSceneManager.SaveScene(newScene, LauncherScenePath);
                AssetDatabase.Refresh();
                Debug.Log($"[NonVRBuildScript] Created Non-VR Launcher scene at: {LauncherScenePath}");
            }
        }
    }
}
