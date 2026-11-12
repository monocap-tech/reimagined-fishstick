# Relay Yard — Six-Switch Room: Unity assembly and build guide

## Safety and preparation

1. Keep using your short local project path: C:\Users\user\Documents\Project 2\RelayYard.
2. Pull only branch cpsc386-project2-relay-yard using git pull --ff-only origin cpsc386-project2-relay-yard. If Git reports that untracked Unity files would be overwritten, **do not reset/clean**. Preserve the local generated files and resolve the conflict explicitly.
3. Open in Unity 6; let the Editor import assets. In Project Settings > Player > Other Settings, set Active Input Handling to Input System Package (New) or Both. Resolve red compile errors before proceeding.
4. Keep the current Built-In pipeline for this iteration. URP migration is optional and deferred; the midterm-style gate proximity feedback is a sprite halo rather than 3D lighting.

## Regenerate the NEW game, not the old three-relay prototype

5. Stop Play mode. Use **Tools > Relay Yard > Assemble Lights Out Room**. The older menu name **Assemble Playable Prototype** is an alias for the same updated generator.
6. Read the confirmation: this action **overwrites** Assets/Scenes/MainMenu.unity, Game.unity, and Win.unity, plus Assets/Animations/Player.controller. Do not run it again after hand-editing those scenes unless you intend to replace them. The Editor opens MainMenu afterward.
7. Generated Game contains:
   - Player root with zero-friction BoxCollider2D, Rigidbody2D, ground probe, Visual child with Animator, jump particle system.
   - One continuous solid ground floor, six *separated* pressure plates (each with trigger and sparks), six indicator lights.
   - LightsOutController; initial lights OFF OFF ON OFF ON OFF.
   - Exit Gate as a **solid BoxCollider2D** with proximity halo, and a separate Goal Beyond Gate with SceneLoader trigger (only active once gate clears).
   - Seeded Perlin Signal procedural visualizer, victory burst, AudioDirector, SceneUI.
   - CyanSwitch and GoldSwitch ScriptableObject assets, animated Idle/Walk/Run/Jump/Fall clips, 1D blend tree.
   - Build Profiles scenes Assets/Scenes/MainMenu (0), Game (1), Win (2); standard course folders under Assets.
8. The generated scene is not yet committed by the GitHub changes: Unity creates the .unity, .asset, .anim, .png and .meta files locally. Check the Project window and save.

## REQUIRED AudioMixer (manual Unity task)

9. Create Assets/Resources if necessary. Right-click folder > Create > Audio Mixer. Name the asset **RelayMixer** (file should be Assets/Resources/RelayMixer.mixer).
10. Open the mixer, add two groups under Master named exactly **Music** and **SFX**. The AudioDirector finds them by name at runtime; audio won't be routed to them if the asset/groups are missing.
11. Run **Tools > Relay Yard > Validate Six-Switch Logic**; it checks the expected 2-4-6 solution, all 64 patterns' reachability and reversibility. It is a logic-only test, not a physics playtest.
12. Run **Tools > Relay Yard > Audit Generated Project** for assets, Input Actions, build list and mixer. It cannot prove gameplay and build quality.

## Manual gameplay check

13. Open MainMenu and Play. Start game, hold A/D or arrows, jump with Space. Step onto a plate and verify that its indicator plus immediate neighboring indicators toggle together. Merely standing on a plate must not generate repeated presses.
14. From the starting point, jump over plate 1 to land on plate 2. Then jump over plate 3 to activate plate 4, and over plate 5 to activate plate 6. That logical sequence should light all six. Tune PlayerMotor speed/jump velocity and plate spacing in the Editor if needed; verify the physical solution rather than assuming it works.
15. Gate must glow more strongly near the player while locked; all six lights on must start gate ascent; when the gate clears, enter the distinct goal to reach Win.
16. Press Restart from Game and verify initial state resets. Test re-entering an already pressed plate; it must toggle again. Run back and forth and try jumping just before, over and onto plates. Ensure no accidental multiple contact from a single landing.
17. Verify jump and plate particle bursts, audio, Menu -> Game -> Win continuity, singleton duplicate guard, volume saved across a new Play session, and deterministic Perlin signal with alternate Inspector seed.
18. In Game, verify Animator has Locomotion, Jump, Fall and a working Idle/Walk/Run blend tree. Use the Animator window while playing. Adjust any visible animation defects.
19. Build using File > Build Profiles into Build/RelayYard-Windows. Zip all output files. Unzip to a new folder and launch the .exe OUTSIDE Unity. Repeat the full playthrough and verify no crashes or missing shader effects.

## Git / Canvas finish

20. After verification, commit ALL Unity-generated .unity, .asset, .anim, image assets, .meta files, Resources/RelayMixer.mixer and ProjectSettings files to the same branch. Retain required third-party/AI assistance documentation per syllabus; Editor utilities can be cleaned up after generated assets are stable but are not in the player build.
21. Update docs/PROJECT2_DOCUMENTATION.md with truthful test results and your own design rationale, then export to DOCX/PDF. Submit tested ZIP, complete source, document and peer-review build.

## Submission folder layout

Read [SUBMISSION.md](SUBMISSION.md) for the required build ZIP, source link and documentation separation. The new assembler uses standard `Assets/Scenes`, `Assets/Sprites`, `Assets/Materials`, `Assets/Data`, `Assets/Animations`, `Assets/Audio`, `Assets/Prefabs`, `Assets/Tiles`, `Assets/Input`, `Assets/Resources` and `Assets/Scripts` folders; it does not use `Assets/RelayYardGenerated` anymore. Existing prototype assets there can be preserved until the replacement build passes.
