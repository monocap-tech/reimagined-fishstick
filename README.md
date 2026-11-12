# Relay Yard — CPSC 386 Project 2

A one-room 2D Lights Out puzzle-platformer. Walk across six **separated** pressure plates or jump over plates to avoid triggering them. Touching plate i toggles indicator i and its immediate horizontal neighbors. Illuminate **all six** simultaneously to raise the gate and reach the exit.

**Status:** The source code and Editor scene assembler are committed. Generated Unity scenes, import-generated .meta files, compilation checks, gameplay testing, AudioMixer, and a standalone build **still need verification on the Unity workstation**. Do not represent the branch as a complete tested submission yet.

## Game design

- Six floor plates with safe ground between them; the player physically chooses which to press or bypass.
- All switches reversible: a new contact after leaving the plate toggles its light neighborhood again. No latch or point accumulation.
- Initial indicators from left: OFF, OFF, ON, OFF, ON, OFF (mask 20). One verified logical solution is plates 2, 4, 6 (1-based indices); physical movement must still be playtested.
- All six lights on triggers a completion burst, sound and upward-moving solid gate. A **separate** goal beyond the gate loads Win only after the gate clears.
- Jumping is grounded and responsive. A zero-friction PhysicsMaterial2D (as in Lantern Dash midterm) avoids undesired wall sticking.
- Near the gate, a sprite halo brightens to evoke the midterm's lantern-lit gate. This is a **visual approximation**, not a URP physically lit material, because the project is still on Built-In.

## Technical requirements

1. New Input System asset with 1D Move A/D/arrows and Jump Space, Rigidbody2D, Ground Probe + Physics2D.OverlapCircle.
2. PressurePlate.OnTriggerEnter2D reports switch presses to LightsOutController; LightsOutBoard is an event-driven reversible state machine; a gate reacts only to the solved transition.
3. AudioDirector singleton survives Menu/Game/Win, plays synthesized original background music and distinct jump/plate/unlock effects; routes through Resources/RelayMixer Music/SFX groups when created; music volume saved with PlayerPrefs.
4. SwitchStyle ScriptableObject type with two generated assets (CyanSwitch and GoldSwitch); LightsOutBoard is pure C# logic instantiated by LightsOutController.
5. Animator: Locomotion, Jump, Fall states; 1D Idle/Walk/Run blend tree driven by smoothed Speed, VerticalSpeed, Grounded; actual keyframed scaling motion.
6. JumpFeedback StateMachineBehaviour fires the jump moment's particles/audio on Jump entry.
7. Jump and switch effects use scripted ParticleSystem.Emit(n), with no continuous emission; completion burst uses Emit as well.
8. SeededSignal makes a decorative, animated Perlin-noise/sine-line visualizer with Inspector seed, scale, amplitude and speed parameters. It never changes puzzle solvability.

## How to run

Read **ASSEMBLY.md**, **TESTING.md**, and **SUBMISSION.md**. **For the already working local scenes, use Tools > Relay Yard > Upgrade UI In Existing Scenes**, which saves backups and adds Canvas menus plus pause without regenerating gameplay. Use Assemble Lights Out Room ONLY for a brand-new project, because it overwrites all three scenes and their generated assets. Commit manually generated scenes and all .meta files after successful testing.

## Source and submission

Project 2 is on branch [cpsc386-project2-relay-yard](https://github.com/monocap-tech/reimagined-fishstick/tree/cpsc386-project2-relay-yard); Project 1 remains untouched on main.

Submit (a) independently tested Windows/WebGL build ZIP, (b) completed source branch or source archive, and (c) your own completed DOCX/PDF technical documentation, with disclosures and credits. Keep the separate hand-in files in the structure described in SUBMISSION.md, and preserve standard course-style Assets folders. Post the same build for peer review.

The detailed assignment and syllabus list **Friday, Nov 13, 2026, 11:59 PM** as the due date; use that rather than relying on the module's later overview date.

## Project 1 UI features restored

- Gameplay/Pause is an Input Actions binding on Escape; PauseManager uses Time.timeScale=0, Resume restores 1, and scene changes cannot carry a paused state.
- Native Canvas/CanvasScaler and InputSystemUIInputModule-based menu, HUD, victory and pause interface, replacing OnGUI after scene upgrade.
- Gameplay pause has Resume, Restart Puzzle and Quit to Menu; MainMenu has Start and Quit Game; victory has Play Again and Main Menu; music volume slider remains persisted.
- The upgrade command is **non-destructive to gameplay** and backs up `Assets/Scenes/*.unity` to `Assets/SceneBackups/`. Existing prior `Relay Yard Canvas` objects are kept on repeated runs.
- Unity UI package `com.unity.ugui` is explicitly installed via Packages/manifest.json, like Project 1.

## URP 2D experiment (test branch only)

For the isolated renderer migration, see [URP_MIGRATION.md](URP_MIGRATION.md). It includes the non-destructive **Tools > Relay Yard > URP Test > Convert Saved Game Scene** step and post-conversion verification. The stable Project 2 branch remains unchanged. **Do not run the destructive scene assembler merely to acquire URP lights.**

## Final UI / timed-run revision (URP test branch)

After the URP scene conversion, run **Tools > Relay Yard > Upgrade to Final HUD and Audio** on the saved scenes. The tool backs them up to `Assets/InterfaceBackups`, adds separate modern Canvases, and deactivates the original ones. It adds an objective/time top band, a keyboard control and hold-R reset bottom band, completion best time, independent Music/SFX audio settings on MainMenu/Pause, and updated Win results. See [FINAL_UI_UPGRADE.md](FINAL_UI_UPGRADE.md) for behavior, safe execution steps and acceptance tests. This does not rebuild the puzzle or URP environment.
