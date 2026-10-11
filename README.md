# SHMUPtest — Project 1

A small 2D target shooter for CPSC 386, covering the eight Weeks 1–6 requirements. Move around a tiled arena, shoot upward, hit the pink targets, and beat the record. Each target relocates within its own lane when hit. The run continues until you return to the menu.


## Controls

| Input | Action |
|---|---|
| WASD / arrows | Move with Rigidbody2D |
| Hold Space | Fire upward |
| Escape | Pause / resume |
| Pause → Quit to Menu | Return to MainMenu |
| MainMenu → Quit Game | Exit desktop build |

## Contents

- `Assets/Scripts`: eight runtime scripts, with serialized Inspector references.
- `Assets/Input/PlayerControls.inputactions`: Move, Fire, and Pause, backed by keyboard bindings.
- `Assets/Materials/NeonPulse.shader`: a visible pulsing material effect on targets.
- `Assets/Editor/NeonRangeBuilder.cs`: optional editor-only scene assembly.
- [ASSEMBLY.md](ASSEMBLY.md): import, generated hierarchy, Inspector wiring, and manual fallback.
- [PROJECT1_DOCUMENTATION.md](PROJECT1_DOCUMENTATION.md): script, scene, requirement, and assistance disclosures.
- [TESTING.md](TESTING.md): Editor/build checks and submission packaging.
