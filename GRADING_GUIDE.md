# Relay Yard — Grader's guide

**CPSC 386 · Project 2 · Fall 2026**  
**Source branch:** [cpsc386-project2-release](https://github.com/monocap-tech/reimagined-fishstick/tree/cpsc386-project2-release)  
**Project editor version:** Unity **6000.6.4f1** (recorded in [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt)).

> **Build status:** A Windows build has been reported successful, but launch and a full run outside Unity still need to be recorded. WebGL is in progress. The course syllabus calls for Unity 6 LTS; verify compatibility with the instructor's exact grading editor version before relying on the source project alone.

## Open and play

1. Use this **release branch**, **not** the repository's `main` branch (which is a different, older project).
2. Open the repository folder containing `Assets/`, `Packages/`, and `ProjectSettings/` as a Unity project.
3. Open [Assets/Scenes/MainMenu.unity](Assets/Scenes/MainMenu.unity). It is also scene **0** in the checked-in Build Profiles scene list; Game is **1**, Win is **2**.
4. Press Play in the Editor, or launch the separately supplied Windows/Web build.

**Controls:** A/D or arrows to move; Space to jump; Esc to pause/resume; hold R ~0.9 seconds to reset; mouse to operate the UI.

**Mechanics:** Six floor switches toggle themselves and their immediate neighbors. The initial pattern from left to right is **OFF, OFF, ON, OFF, ON, OFF**. Light all six switches to unlock the rising gate, then touch the exit to reach Win. To demonstrate a solution quickly, activate switches **2, 4, and 6** (counting from the left, once each), using jumps to avoid unwanted switches.

## Scenes and key GameObjects

| Scene | Key GameObjects/components and interactions |
|---|---|
| [MainMenu](Assets/Scenes/MainMenu.unity) | Canvas menu and Audio Settings controls load Game; persistent AudioDirector plays MusMus-BGM-172 |
| [Game](Assets/Scenes/Game.unity) | Player Rigidbody2D/Animator with ground probe; six PressurePlate triggers and indicators; LightsOutController/board; ExitGate and goal trigger; procedural SeededSignal, particles, URP 2D lighting, HUD, Pause Manager and reset controls |
| [Win](Assets/Scenes/Win.unity) | Clear/best-time results and Replay/Main Menu controls; game BGM continues until returning to menu |

## Project 2 feature evidence

This is an implementation index, not a claim that the game has been independently graded or fully tested.

| # | Feature | Source and scene evidence |
|---:|---|---|
| 1 | Input Actions, physics movement and grounded jump | [RelayControls.inputactions](Assets/Input/RelayControls.inputactions), [PlayerMotor.cs](Assets/Scripts/PlayerMotor.cs); Rigidbody2D and Physics2D ground probe |
| 2 | Event-driven puzzle tracking and win | [PressurePlate.cs](Assets/Scripts/PressurePlate.cs) → [LightsOutController.cs](Assets/Scripts/LightsOutController.cs) → [LightsOutBoard.cs](Assets/Scripts/LightsOutBoard.cs) → [ExitGate.cs](Assets/Scripts/ExitGate.cs) and [SceneLoader.cs](Assets/Scripts/SceneLoader.cs) |
| 3 | Persistent music and SFX, AudioMixer, saved audio controls | [AudioDirector.cs](Assets/Scripts/AudioDirector.cs), [AudioSettingsPanel.cs](Assets/Scripts/AudioSettingsPanel.cs), [RelayMixer.mixer](Assets/Resources/RelayMixer.mixer); Music/SFX routing and PlayerPrefs |
| 4 | ScriptableObjects plus a plain C# logic class | [SwitchStyle.cs](Assets/Scripts/SwitchStyle.cs), [CyanSwitch.asset](Assets/Data/CyanSwitch.asset), [GoldSwitch.asset](Assets/Data/GoldSwitch.asset); [LightsOutBoard.cs](Assets/Scripts/LightsOutBoard.cs) has no MonoBehaviour dependency |
| 5 | Animator states and blend tree | [Player.controller](Assets/Animations/Player.controller) with Locomotion / Jump / Fall; Locomotion uses the **Idle Walk Run Blend** tree; animation clips under [Assets/Animations](Assets/Animations) |
| 6 | Animation-driven feedback | [JumpFeedback.cs](Assets/Scripts/JumpFeedback.cs) is a StateMachineBehaviour on the Jump state that calls back into PlayerMotor |
| 7 | Code-triggered particle effects | [PlayerMotor.cs](Assets/Scripts/PlayerMotor.cs) emits takeoff particles; [LightsOutController.cs](Assets/Scripts/LightsOutController.cs) triggers the solve burst |
| 8 | Inspector-configurable procedural/noise effect | [SeededSignal.cs](Assets/Scripts/SeededSignal.cs) uses seeded Perlin noise plus a traveling sine wave, with editable seed/scale/strength/speed parameters; purely decorative |

**Other relevant work:** [RelayRunClock.cs](Assets/Scripts/RelayRunClock.cs) saves fastest completed runs; [ResetHoldController.cs](Assets/Scripts/ResetHoldController.cs) manages cancelable hold-to-reset and UI feedback; the renderer uses URP 2D Lights and lit/unlit materials.

## Build, validation, and disclosure

- [TESTING.md](TESTING.md) distinguishes Editor observations from independently tested standalone-player behavior.
- [SUBMISSION.md](SUBMISSION.md) tracks the required playable build, source link, and Word/PDF document.
- [docs/PROJECT2_DOCUMENTATION.md](docs/PROJECT2_DOCUMENTATION.md) is a **draft** technical report; do not mistake the old external Word draft (three relays, generated stars) for current game behavior.
- [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) lists imported music, font, sound and license sources. **Course policy separately requires prior instructor consent to third-party assets, including free assets. Consent has not been documented here.**
- The final personally reviewed report must identify substantial ChatGPT assistance in design, coding, troubleshooting, testing support and documentation.

**Submitted build URL, archive name, source commit SHA, tested platform, and course approval status must be filled in with actual evidence before final submission.**
