# Project 2 technical documentation — Relay Yard (WORKING DRAFT)

CPSC 386-05 | Fall 2026 | Individual Unity project

**IMPORTANT:** This is a project-generated source inventory and planning outline, not an independently authored submission document. Verify it, write or revise the actual design discussion yourself, disclose AI assistance as required, and record actual Unity/build tests before exporting to Word/PDF.

## Overview and intended experience

Relay Yard is a single-room six-switch Lights Out puzzle-platformer. Floor plates are separated by safe ground. Walking onto a plate toggles its indicator and its immediate left/right neighbors; jumping allows a player to bypass plates. All six indicator lights must be ON simultaneously to raise a physical gate. The player then reaches a separate goal trigger to change to the Win scene.

Starting pattern left-to-right: OFF OFF ON OFF ON OFF. A mathematically verified solution is 2, 4, 6 (switch labels are 1-based). Whether the player can physically execute the intended jump sequence still requires playtesting.

The design uses physical traversal to select puzzle inputs, event-driven state to implement reversible toggling, and immediate light/color/sound/particle feedback to make cause and effect visible.

## Scenes and GameObjects

- **MainMenu**: Main Camera / AudioListener, persistent AudioDirector instance, SceneUI menu/start and saved music-volume control.
- **Game**: Player (Dynamic Rigidbody2D, BoxCollider2D with Slick friction=0, Ground Probe, Input Action asset; child Visual with Animator); Solid Floor (RelayGround layer); Pressure Plate 1..6 (trigger collider, SwitchStyle, ParticleSystem); Indicator Light 1..6; Lights Out Controller; Exit Gate (solid collider + proximity halo + upward movement); separate Goal Beyond Gate; Seeded Perlin Signal (LineRenderer); Completion Burst; SceneUI HUD.
- **Win**: Main Camera / AudioListener, duplicate-guarded AudioDirector, SceneUI victory/replay.

## Source script inventory

- **PlayerMotor.cs**: InputActions Gameplay/Move/Jump, physics body movement, grounded jump eligibility, Animator Speed/VerticalSpeed/Grounded, Jump state feedback.
- **LightsOutBoard.cs**: pure C# board model: initial bitmask, reversible toggling of self and immediate neighbors, lit count and solved state.
- **PressurePlate.cs**: per-plate OnTriggerEnter2D sends input to controller; repeated entry is allowed; ParticleSystem.Emit and plate SFX; reads SwitchStyle.
- **LightsOutController.cs**: MonoBehaviour owns LightsOutBoard, updates indicator/plate colors on input, unlocks gate and fires solved event once.
- **SwitchStyle.cs**: ScriptableObject type defining on/off colors and a label; CyanSwitch and GoldSwitch are two distinct assets used by alternating plates.
- **ExitGate.cs**: solid sliding gate, updates a proximity halo and gate appearance, rises after solved.
- **ExitGoal.cs**: goal trigger beyond gate loads Win only when the gate has opened.
- **AudioDirector.cs**: singleton/DontDestroyOnLoad sound, distinct generated SFX and looping music, AudioMixer Music/SFX groups, PlayerPrefs volume.
- **JumpFeedback.cs**: StateMachineBehaviour triggers jump feedback upon Animator state entry.
- **SeededSignal.cs**: deterministic Perlin noise plus sine-wave visualizer drawn by a LineRenderer. Inspector exposes seed/noiseScale/noiseStrength/amplitude/speed.
- **SceneUI.cs**: menu/game/win HUD, restart, music-volume UI.
- **RelayYardAssembler.cs**: Editor-only scene/data/Animator generator, and setup audit (not in player build).
- **LightsOutPuzzleSelfTest.cs**: Editor-only known-solution and state-reachability checks.

The prior prototype's RelayLedger/RelayController/RelayBeacon/RelayDefinition/SeededBackdrop remain legacy source files, not used in the new six-switch scene, until the final cleanup pass.

## Rubric mapping

1. **2D input/physics/jump**: RelayControls.inputactions with Gameplay/Move and Jump, Rigidbody2D, zero-friction BoxCollider2D, Physics2D.OverlapCircle Ground Probe.
2. **Event-driven puzzle state**: pressure plate trigger sends Press(index); pure C# model updates 3-bit neighborhood; MonoBehaviour refreshes presentation and unlocks gate on solved transition.
3. **Audio**: original synthesized music and jump/plate/unlock SFX, AudioDirector survives scene transitions, Music/SFX AudioMixer groups (MUST CREATE and VERIFY), saved music volume via PlayerPrefs.
4. **ScriptableObjects and pure C#**: CyanSwitch and GoldSwitch are separate SwitchStyle assets; LightsOutBoard is non-MonoBehaviour and instantiated by LightsOutController.
5. **Animator and blend**: Locomotion/Jump/Fall states; 1D Idle/Walk/Run blend and animated .anim curves controlled by Speed, VerticalSpeed, Grounded.
6. **Animation-driven moment**: JumpFeedback.StateMachineBehaviour.OnStateEnter calls PlayerMotor.OnJumpAnimationStarted() to emit jump dust and play jump audio.
7. **Particles triggered by code**: ParticleSystem.Emit(n) for jump, each toggle, and puzzle solution.
8. **Procedural**: SeededSignal uses Inspector-controlled seed and Mathf.PerlinNoise, updated at runtime as a decorative line waveform. The puzzle remains deterministic and reachable.

## Distinction from studied examples

- **Assignment 9**: Perlin noise seed/scale and scripted particle bursts, adapted to a 2D background trace and switch feedback rather than a cube terrain.
- **Assignment 10**: event-driven switch puzzle and grounded jumping, adapted from permanent latches to reversible neighbor toggles and physically spaced plates.
- **Lantern Dash midterm study guide**: zero-friction PhysicsMaterial2D, solid gate and separate goal concept, midterm-style player-near-door glow. The current Built-In version approximates proximity illumination with a sprite halo; it does NOT contain a URP/Lit shader/Point Light reproduction.
- **Assignment 8**: actual keyframed animation states and smoothed Idle/Walk/Run blend, plus JumpFeedback StateMachineBehaviour.

## Dependencies, sources and disclosure

- Unity Engine and official Unity Input System package are project dependencies.
- Square graphic generated locally by Editor utility; audio synthesized in AudioDirector; no third-party artwork or prerecorded audio currently included.
- The source implementation, scene assembler, proposed solution and this outline were developed with substantial ChatGPT assistance. **Course policy permits AI on projects only with disclosure and citation.** Disclose the actual help received in your independently written submission. Do not submit AI-generated explanatory prose as your original writing.
- Verify whether any third-party code/assets need instructor permission and attribution. Existing Project 1 files on the repo's main branch are a separate project and not part of this Project 2 build.

## Actual testing and delivery record — fill in only after verification

- Unity version:
- Regenerated scene date:
- Unity Console (warnings/errors):
- LightsOutPuzzleSelfTest result:
- Physical jump-over-plate sequence:
- AudioMixer group verification:
- Animator and particle result:
- Gate/exit and restart result:
- Standalone platform, file name and tested location:
- Build ZIP:
- Source commit/branch:
- Final document prepared and AI disclosures confirmed:
- Submission date:

The detailed rubric and syllabus use **November 13, 2026, 11:59 PM** as the operational deadline, with peer-review build posting.
