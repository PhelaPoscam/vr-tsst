# ECG VR-TSST  Standalone Non-VR Desktop Demo

This module provides a **100% separated, non-VR desktop demo** for the ECG VR-TSST project. It allows researchers, developers, and evaluators to run and demonstrate the full Trier Social Stress Test experiment on any regular PC or laptop screen without requiring a VR headset (HTC Vive, Meta Quest, OpenXR runtime, etc.).

---

## Key Features

1. **First-Person Mouse Look (360)**:
   - Rotates the camera with the mouse (pitch and yaw with angle clamps) to simulate the participant's head orientation in both the Waiting Room and the Auditor / Evaluation room.
2. **Dual Interaction Modes (Look Mode vs. Free Cursor)**:
   - **Look Mode**: Mouse moves the camera view.
   - **Free Cursor Mode**: Mouse cursor is freed to click on-screen researcher control buttons.
   - Seamlessly toggle between modes using **`Tab`**, **`Escape`**, or by **holding Right-Click**.
3. **Full Protocol Control**:
   - Keys **`1` through `9`** trigger standard TSST test actions (Next, Repeat, Wrong, Right, etc.) via `KeyboardInputManager` at all times.
4. **Zero Original Code Changes**:
   - All assets, scripts, and tools are located in `Assets/NonVR_Demo/`.
   - Existing VR scenes (`MainMenu.unity`, `MainLogic.unity`, `Rooms.unity`) and scripts are completely untouched.

---

## How to Run in Unity Editor

You have three convenient ways to run the demo:

### Method 1: Direct to 3D Room (Recommended)
From the Unity top menu bar, click:
> **Tools** > **ECG VR-TSST** > **Start Non-VR Demo (Direct to 3D Room)**
- Jumps straight into the 3D experiment scene (`MainLogic` + `Rooms`), placing you directly inside the 3D Waiting Room with 360° mouse-look ready.

### Method 2: Via Main Menu / Configuration
From the Unity top menu bar, click:
> **Tools** > **ECG VR-TSST** > **Start Non-VR Demo (via Main Menu)**
- Launches the study settings/configuration menu first.

### Method 3: Via Launcher Menu
From the Unity top menu bar, click:
> **Tools** > **ECG VR-TSST** > **Start Non-VR Demo (via Launcher Menu)**
- Opens the standalone launcher menu scene.

### Method 4: Global Play Mode Toggle
From the Unity top menu bar, click:
> **Tools** > **ECG VR-TSST** > **Toggle Non-VR Mode for Play Mode**
- When checked (`ENABLED`), hitting **Play** on **ANY scene** (`MainMenu.unity`, `MainLogic.unity`, etc.) will run in Non-VR mode with mouse-look enabled and VR display suppressed.
- When unchecked (`DISABLED`), the project reverts to standard VR mode.

---

## Controls Reference

| Input | Action |
| :--- | :--- |
| **Mouse Move** | Rotate camera (360° yaw, pitch clamped between -80° and +80°) |
| **F** | **Face Wall Instructions** (Instantly/smoothly turns camera to look directly at the active wall text) |
| **Tab** or **Escape** | Toggle between **Look Mode** and **Free Cursor Mode** |
| **Right-Click (Hold)** | Temporarily look around while holding |
| **Left-Click (Screen)** | Re-locks cursor to enter Look Mode (if not clicking on a UI button) |
| **C** | Toggle posture between **Standing (1.68m)** and **Sitting (1.25m)** |
| **Q / E** | Fine-tune camera height up (`E`) or down (`Q`) |
| **WASD** / **Arrow Keys** | Walk / nudge position around the room |
| **R** | Reset camera position to initial viewpoint |
| **1 – 9** | Researcher TSST action hotkeys (Next, Repeat, Wrong, Right, etc.) |
| **H** | Hide / Show the on-screen help HUD |

---

## How to Build a Standalone Executable

To export a standalone Windows `.exe` that runs without VR:
1. In Unity, go to **Tools** > **ECG VR-TSST** > **Build Standalone Non-VR Demo (Windows)**.
2. The build will be placed in `<ProjectRoot>/Build_NonVR_Demo/ECG_VR-TSST_NonVR.exe`.
3. The build folder will open automatically in Windows Explorer when completed.
4. Run `ECG_VR-TSST_NonVR.exe` on any Windows PC.
