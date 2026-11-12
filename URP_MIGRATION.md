# Relay Yard: URP 2D test branch — execution and review

Branch: `cpsc386-project2-urp-test`
Stable playable checkpoint: `cpsc386-project2-relay-yard` at `df248b0`

**Do NOT run Tools > Relay Yard > Assemble Lights Out Room on this project.** That generator replaces all three saved scenes. The UI upgrade has already run and the original MainMenu/Game/Win scenes are committed.

## What was changed in source

- The test branch explicitly requires `com.unity.render-pipelines.universal` **17.6.0** in `Packages/manifest.json`. Unity 6000.6.4f1 resolves package dependencies and regenerates the package lock.
- `Assets/Editor/RelayYardURP2DMigrator.cs` provides an Editor-only conversion command. **It must be run once inside Unity** to actually write the migrated .unity, .asset, .mat and .meta files.
- `ExitGate.cs` has a serialized URP lighting flag; the migration enables it and deactivates the legacy halo, keeping the gate's collider, unlock and lift.
- `SeededSignal.cs` prefers the URP 2D unlit shader at runtime, with fallbacks.
- The stable Project 2 branch is not modified by this experiment.

## On Windows, after pulling the test branch

1. Save and close Unity. Check `git status --short` for uncommitted local files and preserve them before pulling (do not use `git clean` or `reset --hard`).
2. In the existing RelayYard clone:

   ```bat
   cd /d "C:\Users\user\Documents\Project 2\RelayYard"
   git branch --show-current
   git pull --ff-only origin cpsc386-project2-urp-test
   ```

3. Open the project in **Unity 6000.6.4f1**. Allow URP 17.6.0 to import completely. Stop if the Console has red errors. Unity will regenerate `Packages/packages-lock.json`.
4. Stop Play mode. Choose **Tools > Relay Yard > URP Test > Convert Saved Game Scene**. Confirm backup/conversion. The tool:
   - copies the saved `Assets/Scenes/Game.unity` to `Assets/URPBackups/Game-BeforeURP2D.unity` (once);
   - creates `Assets/Settings/RelayYardURP.asset` and `RelayYard2DRenderer.asset` if missing;
   - assigns the new URP pipeline in Graphics and the *current* Quality level;
   - creates URP Lit/Unlit materials in `Assets/Materials`;
   - keeps scene objects, puzzle, animator, audio, UI and trigger references untouched;
   - changes gate/floor/background to Lit and player/indicators/plates to Unlit;
   - disables `Gate Proximity Halo`, and activates the `ExitGate` 2D lighting mode;
   - adds `Player/Lantern Light 2D` (warm 5.8-unit circular falloff);
   - adds `Room Ambient Light 2D` (dim global fill);
   - adds left/right physical boundary walls and background panels (visual-only).
5. Open `Assets/Scenes/Game.unity` and enter Play mode. Confirm the lantern is attached to Player; the gate brightens gradually as the player approaches, darkens as the player retreats, and the walls/floor receive the same light. Reconfirm gate lifting and crossing the exit.
6. Check Graphics and Quality Project Settings. The **active** URP asset must point to the **2D Renderer**. Unity may have platform- or quality-specific overrides; correct any that are not using the same URP asset.
7. Test Canvas buttons, pause, gameplay, switch contacts, gate/goal, waveform, particles, AudioDirector and AudioMixer. Then build and launch a Windows standalone executable in an unpacked folder.
8. Save all scenes and `ProjectSettings`. Check `git status --short`, then commit the converted `.unity`, generated `.mat`/`.asset` and `.meta` files, and package-lock changes on **the URP test branch**. Preserve `Assets/URPBackups` for the experiment.

## Lighting acceptance criteria

- The light source is `Player/Lantern Light 2D`, not a scripted approximation at the gate.
- `Exit Gate` uses `RelaySpriteLit`, and the rectangular `Gate Proximity Halo` is inactive.
- The distance-based tint branch of `ExitGate` is disabled via its serialized `use2DLighting` field.
- The player and indicator sprites remain legible outside the lantern's range.
- Moving the player near the gate makes the gate **partially** brighter; moving away makes it dim.
- A low-intensity Global Light 2D keeps architectural features visible.
- The physical side walls constrain the player's movement without blocking access to `Goal Beyond Gate`.
- UI and gameplay remain unchanged. No pink/missing materials, missing shader warnings, or Unity Console errors.

## Troubleshooting

- **Cannot compile the migration Editor script:** Confirm `Packages/manifest.json` contains `com.unity.render-pipelines.universal: 17.6.0` and package import finished. Do not attempt scene conversion until all C# errors are resolved.
- **Pink or black sprites:** Check active Graphics **and Quality** pipelines, 2D Renderer Data, and the materials' shader. Lit = `Universal Render Pipeline/2D/Sprite-Lit-Default`; Unlit = `Universal Render Pipeline/2D/Sprite-Unlit-Default`.
- **Gate doesn't respond to lantern:** Check the gate material, sorting layer filtering on `Lantern Light 2D`, and that the legacy `Gate Proximity Halo` is inactive.
- **Waveform invisible:** The LineRenderer uses the URP Sprite-Unlit shader; check Console and LineRenderer sorting/material. The waveform is decorative and must not affect logic.
- **Particles invisible:** Check `RelayParticlesUnlit` shader and ParticleSystemRenderer sorting order.
- **URP settings invalid after conversion:** Open the assets in `Assets/Settings`, verify RelayYardURP's default Renderer is RelayYard2DRenderer. If needed, use `Assets > Create > Rendering > URP Asset (with 2D Renderer)` and assign it in Graphics/Quality.
- **Need to revert:** The untouched `cpsc386-project2-relay-yard` branch is the baseline. The separate `Assets/URPBackups/Game-BeforeURP2D.unity` is an additional scene backup, but reverting the scene alone does NOT revert Graphics/Quality or the URP package. Switch back to the stable branch after preserving any test work.

## Limitations

This code has been reviewed at the source level, but **Unity Editor compilation, on-screen appearance, package import, and standalone build have not been executed by ChatGPT**. Record their real outcomes in `TESTING.md`; report any exact compile errors rather than guessing. The supplied course midterm used a 3D Point Light with URP/Lit material, whereas this migration uses the dedicated **URP 2D** Renderer and Light 2D—the correct adaptation for Relay Yard's sprites.
