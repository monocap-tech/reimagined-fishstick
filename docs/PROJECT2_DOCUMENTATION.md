# Relay Yard — Project 2 technical report (review draft)

**Course:** CPSC 386, Fall 2026  
**Engine:** Unity 6.6 (6000.6.4f1), URP 2D Renderer  
**Status:** Source-based technical draft. Review and personalize before DOCX/PDF submission; complete testing blanks with actual evidence.

## 1. Project overview and design

Relay Yard is a one-room 2D puzzle-platformer inspired by the reversible-state structure of Lights Out. The player walks over six separated pressure plates; contacting plate *i* flips the state of *i* and its immediate neighbors. The initial six-light state is OFF, OFF, ON, OFF, ON, OFF. The player can jump to bypass a plate, turning platform traversal into a way to select puzzle inputs.

When all six indicators are ON, the puzzle is solved, an unlock sound and particles play, and the exit gate begins rising. The goal becomes eligible immediately on the solved event. The player still needs to reach the goal, including when already overlapping its trigger at the moment of unlock. A successful goal entry loads the Win screen.

A clear-time clock continues until the goal is reached; the fastest successful clear persists locally as the personal best. The game does **not** count attempts.

## 2. Scenes and user interface

- **MainMenu:** title, brief neighbor-toggle rule, Start, Audio and Quit.
- **Game:** player, floor/walls, six plates and indicators, gate, goal trigger, procedural signal, particle effects, player-carried Light 2D, ambient Light 2D, top objective/time band and bottom control legend.
- **Win:** clear time, best time, New Best when applicable, Replay and Main Menu.

An Escape-driven pause panel provides Resume, Restart, Audio and Main Menu. The audio panel is available from MainMenu and Pause. Music and SFX have independent sliders and mute buttons; Reset Audio restores default levels. The keyboard's **R** command requires approximately 0.9 seconds of uninterrupted holding to reset the puzzle. A clockwise radial gauge fills during the hold and visually drains when released before confirmation. The HUD reset button also supports an immediate mouse click.

## 3. Key scripts and architecture

| Script | Responsibility |
|---|---|
| `PlayerMotor.cs` | Input System movement/jump, Rigidbody2D motion, grounded probe, Animator parameters |
| `LightsOutBoard.cs` | Pure C# reversible six-bit board, neighborhood toggling, solved-state test |
| `LightsOutController.cs` | Reacts to plate presses, updates visuals, sends unlock event, triggers solve particles/audio |
| `PressurePlate.cs` | Trigger-enter plate detection and local feedback |
| `SwitchStyle.cs` | ScriptableObject palette definition for plate/indicator states |
| `ExitGate.cs` | Immediate unlock flag and collision clearance; gradual visible ascent |
| `SceneLoader.cs` | Menu/game/win transitions; goal trigger accepts both entry and continued overlap |
| `RelayRunClock.cs` | Gameplay timer, completed clear time and persisted fastest clear |
| `ResetHoldController.cs` | R hold duration, release cancellation, radial progress and click reset |
| `PauseManager.cs` | Time scale, Escape toggling and pause navigation |
| `SceneUI.cs` | HUD status and timer/results text updates |
| `AudioDirector.cs` | Persistent generated audio, separate Music/SFX volume/mutes, PlayerPrefs |
| `AudioSettingsPanel.cs` | Audio sliders/buttons and synchronized labels |
| `SeededSignal.cs` | Inspector-configured Perlin/sine procedural waveform |
| `JumpFeedback.cs` | Animator StateMachineBehaviour for jump feedback |

## 4. Unity features and rubric correspondence

- **Input/physics:** New Input System action asset for A/D/arrow movement, Space, Esc and R; Dynamic Rigidbody2D, colliders, grounded jump logic and zero-friction PhysicsMaterial2D.
- **State and events:** Pure C# LightsOutBoard plus MonoBehaviour controller. Switch contacts call Press, and solved state unlocks the gate.
- **ScriptableObjects:** Distinct `CyanSwitch.asset` and `GoldSwitch.asset` store switch presentation options.
- **Animation:** Animator with idle, walk, run, jump and fall animation clips; state-based jump feedback.
- **Particles/audio:** Short scripted particle bursts for plates/jump/solve and original procedurally synthesized sounds/music.
- **Procedural behavior:** Perlin-driven animated signal visualizer, independent of logical puzzle state.
- **Rendering/UI:** URP 2D Renderer with Lit/Unlit sprites and player-mounted Light 2D; native Canvas UI with interactive audio settings and radial reset indication.
- **Persistence:** Best clear time and audio preferences stored locally through PlayerPrefs.

**AudioMixer verification pending:** The code can route through an optional Music/SFX AudioMixer, but the source branch does not yet include a `RelayMixer.mixer` asset. Confirm and supply it if the rubric requires mixer-based routing.

## 5. Testing evidence (complete with observed results)

- Editor version and test date: Unity 6000.6.4f1, November 12, 2026.
- Editor gameplay/UI: Reported working; screenshots show menu, game HUD, pause/audio and result displays.
- Gate/goal trigger correction: Reported working after implementing `OnTriggerStay2D` alongside `OnTriggerEnter2D` and restoring gradual gate motion.
- Cleaned release branch import: **Not yet verified.**
- AudioMixer group routing: **Not yet verified.**
- Windows standalone build and full playthrough: **Not yet verified.**
- Tested release commit: **To fill in.**

## 6. Asset sources, attribution and assistance

The project uses Unity and its official packages. Core geometric art and the reset-ring graphic are generated locally; `AudioDirector` synthesizes its background music and effect tones at runtime rather than distributing prerecorded third-party sounds. Review the final `Assets/` contents to confirm whether any additional imported asset requires attribution.

**AI-assistance disclosure (must be retained and personally reviewed):** ChatGPT was used substantially during design iteration, research, C# scripting, project structuring, troubleshooting, URP migration and UI tooling, acceptance-test planning, and drafting this documentation outline. The author reviewed, integrated, debugged and tested the resulting work in Unity. The final report should identify assistance in the form required by the instructor, and independently explain the author's actual decisions and testing.

## 7. Submission record

- Source URL and commit: **To fill in.**
- Built executable folder and ZIP: **To fill in.**
- Documentation PDF/DOCX: **To fill in.**
- Platform/build test date: **To fill in.**
- Peer review / upload confirmation: **To fill in.**
