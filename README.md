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

The source excludes historical scene backups, unused prototype assets, and one-time Editor generation/migration tools. The working development history remains on branch `cpsc386-project2-urp-test`. This release branch contains only the current game source and final documentation.

## Technical notes

- The six-switch board logic is a pure C# model (`LightsOutBoard`). The MonoBehaviour controller updates visible indicators and unlocks the gate when the model is solved.
- The gate rises gradually after unlock. Exit eligibility changes immediately at 6/6; `SceneLoader` handles both trigger entry and continued overlap, so a player already standing in the goal can complete the level.
- The player Light 2D provides spatial illumination to URP-lit surfaces. Gameplay sprites that need constant legibility use appropriate unlit materials.
- The persistent `AudioDirector` generates original music and sounds procedurally, with separate Music and SFX channels. It uses Music/SFX AudioMixer groups when a compatible `Assets/Resources/RelayMixer.mixer` is present. **The mixer asset is not currently committed and must be checked against the assignment requirements before hand-in.**

## Submission

See `SUBMISSION.md` for the three deliverables and `TESTING.md` for the remaining release checks. The technical report draft is `docs/PROJECT2_DOCUMENTATION.md`.

**Verification boundary:** Editor gameplay has been reported working; this cleaned release source still needs a Unity import/build check and independent Windows-player test. The final report must include accurate asset credits and disclosure of substantial ChatGPT assistance.
