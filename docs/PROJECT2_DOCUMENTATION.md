# Project 2 documentation — Relay Yard
CPSC 386-05 | Fall 2026 | Individual Unity game project

**Project status: draft based on source implementation. Verify against assembled Unity project and finished build before exporting to PDF/Word.**

## Game description and design decisions

Relay Yard is a one-level 2D puzzle-platformer. Move left/right and jump to activate three colored relays by contact; this unlocks a previously closed exit. The design intentionally uses fixed, easily testable platforms and seeded decorative starfield generation to preserve beatability. The goal is to demonstrate the course's technical patterns cohesively rather than build a large content-heavy game.

## Scenes

- MainMenu: orthographic camera, one AudioListener, persistent AudioDirector (if none already exists), SceneUI with Start and music volume slider.
- Game: camera, AudioDirector duplicate guard, Player, solid floor and raised platform, three RelayBeacon triggers, RelayController, ExitGate, SeededBackdrop, gameplay HUD.
- Win: victory camera, AudioDirector duplicate guard, SceneUI with replay and volume slider.

## Scripts

- PlayerMotor.cs — reads InputActionAsset, uses Rigidbody2D velocity, gates jumps through an OverlapCircle ground check, drives Animator parameters.
- RelayLedger.cs — pure C# HashSet model of expected and activated relay IDs; exposes counts and solved state.
- RelayController.cs — MonoBehaviour adapter, initialized with three relay IDs; uses RelayLedger on trigger events and unlocks exit when complete.
- RelayBeacon.cs — trigger-activated relay with ScriptableObject-defined active color, particle burst, and audio response.
- RelayDefinition.cs — CreateAssetMenu ScriptableObject defining relay visual data; CyanRelay and GoldRelay are separate generated instances.
- ExitGate.cs — initially locked exit trigger; changes color when unlocked and switches to Win scene on player contact.
- AudioDirector.cs — singleton DontDestroyOnLoad audio manager, generated original audio clips, two AudioMixer groups after RelayMixer creation, saved volume through PlayerPrefs.
- JumpFeedback.cs — StateMachineBehaviour executes PlayerMotor feedback when Animator enters Jump state.
- SeededBackdrop.cs — deterministic System.Random background generator, configurable Inspector seed and star count.
- SceneUI.cs — minimal start, win, restart and volume HUD.
- RelayYardAssembler.cs — Editor-only reproducible scene, data, animation and Build Profiles generator (not included in runtime build).

## Requirements compliance map

1. Input System: RelayControls.inputactions Gameplay/Move and Gameplay/Jump, Rigidbody2D on Player, Physics2D.OverlapCircle ground probe against RelayGround layer.
2. Puzzle tracking: RelayLedger HashSet updated only on RelayBeacon.OnTriggerEnter2D; RelayController unlocks ExitGate when all three distinct relays are activated.
3. Audio: AudioDirector maintains music plus three distinct SFX (jump, relay, unlock); loads Resources/RelayMixer.mixer with Music and SFX groups; DontDestroyOnLoad/duplicate guard; persists RelayYard.MusicVolume in PlayerPrefs. **Verify mixer asset exists before submitting.**
4. Data and logic separation: RelayDefinition CreateAssetMenu with two instantiated data assets (CyanRelay, GoldRelay); RelayLedger is pure C# and is used by RelayController.
5. Animator: Player.controller contains 3 Animator states (Locomotion, Jump, Fall) plus 1D Idle/Run blend tree, driven by Speed/VerticalSpeed/Grounded script parameters.
6. StateMachineBehaviour: JumpFeedback.OnStateEnter invokes PlayerMotor.OnJumpAnimationStarted.
7. Particles: PlayerMotor and RelayBeacon call ParticleSystem.Play at jump and activation.
8. Procedure: SeededBackdrop generates randomized decorative stars from exposed seed and count; collision platforms are fixed, so level remains beatable.

## Third-party assets / packages / tutorials

- Unity engine built-in primitives and components, plus official Unity Input System package.
- Graphics: square texture generated in Editor by our assembler; no third-party art.
- Audio: generated algorithmically by AudioDirector; no third-party music or recordings.
- Techniques: standard Unity public Input System, Animator, physics and AudioMixer APIs; no copied third-party project or asset.
- **After implementation, amend this section to list any additional assets, code, packages, tutorials or sources actually used.**

## Build / verification record — complete after testing

- Unity Editor version: [verify]
- Platform and build file name: [verify]
- Build runs outside the Editor: [not yet tested]
- Console errors during play: [not yet tested]
- All eight requirements tested: [not yet tested]
- Date tested: [enter after testing]

## Repository

Source branch: https://github.com/monocap-tech/reimagined-fishstick/tree/cpsc386-project2-relay-yard

**Before submission:** export this content to PDF or Word, replace bracketed fields and remove all pending-test language after tests have actually passed. Include any public WebGL URL if used.
