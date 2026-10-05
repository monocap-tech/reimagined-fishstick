# Neon Range — Project 1

A small 2D target shooter for CPSC 386, covering the eight Weeks 1–6 requirements. Move around a tiled arena, shoot upward, hit the pink targets, and beat your saved record. Each target relocates within its own lane when hit. The run continues until you return to the menu.

**Status: source scaffold ready for import. Not yet compiled or play-tested in Unity; no playable build has been produced.** This repository is an overlay for your existing Unity 6 LTS project, not a complete Hub-openable project yet. Its Editor helper creates real saved scenes, a prefab, art, input references, and a material inside Unity.

## Fast start

1. Download this repository with **Code → Download ZIP** (or clone it).
2. Copy its `Assets` contents into your existing Unity project's `Assets` folder. Keep your current `Packages` and `ProjectSettings`.
3. In Unity's Package Manager, ensure **Input System**, **Unity UI (uGUI)**, and **2D Tilemap Editor** are installed. Use versions compatible with your installed Unity 6 LTS. Set **Active Input Handling** to **Input System Package (New)** and restart if prompted.
4. Wait for compilation, then select **Tools → Neon Range → Assemble Prototype**.
5. Check the Physics 2D collision pairs in [ASSEMBLY.md](ASSEMBLY.md), set Game view to 16:9, and press Play from `MainMenu`.
6. Complete [TESTING.md](TESTING.md) before exporting or submitting.

The helper leaves your existing scenes intact, creates its output in `Assets/NeonRangeGenerated`, and places MainMenu first in the shared Build Profiles Scene List. It adds the `Enemy` and `PlayerBullet` tags and four named layers using free layer slots. It refuses to overwrite a previous generated folder. Check your active Build Profile if it overrides the shared Scene List.

## Controls

| Input | Action |
|---|---|
| WASD / arrows | Move with Rigidbody2D |
| Hold Space | Fire upward |
| Escape | Pause / resume |
| Pause → Quit to Menu | Return to MainMenu |
| MainMenu → Quit Game | Exit desktop build; stop Play mode in Editor |

## Contents

- `Assets/Scripts`: eight runtime scripts, with serialized Inspector references.
- `Assets/Input/PlayerControls.inputactions`: Move, Fire, and Pause, backed by keyboard bindings.
- `Assets/Materials/NeonPulse.shader`: a visible pulsing material effect on targets.
- `Assets/Editor/NeonRangeBuilder.cs`: optional editor-only scene assembly.
- [ASSEMBLY.md](ASSEMBLY.md): import, generated hierarchy, Inspector wiring, and manual fallback.
- [PROJECT1_DOCUMENTATION.md](PROJECT1_DOCUMENTATION.md): script, scene, requirement, and assistance disclosures.
- [TESTING.md](TESTING.md): Editor/build checks and submission packaging.

## Repository handoff

Once imported and tested, commit the **actual Unity project** here: `Assets` (including generated assets and every `.meta`), `Packages`, and `ProjectSettings`. Do not commit `Library`, `Temp`, or build output. Retain this repository's documentation and gitignore. Unity writes the correct editor version and package lockfile on your PC; those are deliberately not guessed in this overlay.

The GitHub source is not the Canvas playable-build submission. Export and test a separate build, zip its entire output folder, and submit the documentation with it.
