# Relay Yard

**CPSC 386 — Project 2 | Unity 6.6 (6000.6.4f1)**

Relay Yard is a one-room 2D puzzle-platformer. Walking onto a floor plate toggles that circuit and its immediate neighbors. All six indicators must be lit together to unlock the exit. Jumping lets the player cross plates without activating them.

The project uses Unity's Universal Render Pipeline with a 2D Renderer and a player-carried Light 2D.

## Run

Open this repository as a Unity project, let its packages import, then open `Assets/Scenes/MainMenu.unity` and press Play. The three playable scenes are **MainMenu**, **Game**, and **Win**. They must appear in the Build Profiles scene list in that order for a standalone build.

### Controls

| Control | Action |
|---|---|
| A / D or Left / Right | Move |
| Space | Jump |
| Esc | Pause / Resume |
| Hold R (~0.9 s) | Restart puzzle, with clockwise progress indicator |
| Click RESET | Restart immediately |
| Mouse | Menu, pause, and audio controls |

Main Menu and Pause both open **Audio** settings with independent Music/SFX volume sliders, mute controls, Test SFX, and Reset Audio. Audio preferences persist locally. A timer measures a clear until the player reaches the exit; only the fastest successful clear is retained as the best time. No attempt count is recorded.

## Program structure

- `Assets/Scenes/`: the three playable scenes and the finished Canvas UI.
- `Assets/Scripts/`: movement, puzzle rules, scene and exit logic, audio, pause, timer, and UI behaviors.
- `Assets/Input/RelayControls.inputactions`: movement, jump, pause, and R reset bindings.
- `Assets/Animations/`: player Animator and animation clips.
- `Assets/Data/`: the cyan and gold SwitchStyle ScriptableObject assets.
- `Assets/Materials/`, `Assets/Settings/`: physics and URP sprite materials, URP pipeline and 2D renderer.
- `Assets/Sprites/`: shared source sprite and the reset-ring graphic.
- `Assets/Font/`: a modified copy of DotGothic16 under the SIL Open Font License 1.1. See the notices and bundled OFL text.

The source excludes historical scene backups, unused prototype assets, and one-time Editor generation/migration tools. The working development history remains on branch `cpsc386-project2-urp-test`. This release branch contains the working game source and draft documentation; the final build and technical-report deliverables remain unverified.

## Technical notes

- The six-switch board logic is a pure C# model (`LightsOutBoard`). The MonoBehaviour controller updates visible indicators and unlocks the gate when the model is solved.
- The gate rises gradually after unlock. Exit eligibility changes immediately at 6/6; `SceneLoader` handles both trigger entry and continued overlap, so a player already standing in the goal can complete the level.
- The player Light 2D provides spatial illumination to URP-lit surfaces. Gameplay sprites that need constant legibility use appropriate unlit materials.
- The persistent `AudioDirector` plays `MusMus-BGM-172.ogg` on **MainMenu**, then `MusMus-BGM-134.ogg` on **Game**. The gameplay track continues through **Win**, and the title track returns on **MainMenu**. Missing BGM assets fall back to a synthesized loop. Jump and plate cues remain synthesized; `Assets/Resources/PuzzleClear.wav` is loaded for the clear cue, with a synthesized fallback. The clear cue uses the SFX AudioSource and responds to SFX volume/mute controls.
- `Assets/Resources/RelayMixer.mixer` is committed with Music and SFX groups. `AudioDirector` assigns the two sources to their matching groups. This routing still needs a runtime Unity/standalone verification.
- The puzzle-clear WAV is **already committed** and originated from a supplied commercial jingle. Its distribution rights are unverified. The two MusMus OGG files are also committed; check their prohibition on standalone music redistribution. See `THIRD_PARTY_NOTICES.md` and `licenses/DotGothic16-OFL.txt`.

## Submission

See `SUBMISSION.md` for the three deliverables and `TESTING.md` for the remaining release checks. The technical report draft is `docs/PROJECT2_DOCUMENTATION.md`.

**Verification boundary:** Editor gameplay has been reported working; this cleaned release source still needs a Unity import/build check and independent Windows-player test. The final report must include accurate asset credits and disclosure of substantial ChatGPT assistance.
