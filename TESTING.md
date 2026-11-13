# Relay Yard — release verification

Do not mark a test as passed unless it was observed in the relevant environment. The user reported the finished URP puzzle and UI **functionally working in the Unity Editor on November 12, 2026**. This does not prove a clean release import or standalone build.

| Area | Required behavior | Current evidence |
|---|---|---|
| Main menu | Start, Audio, Quit; single neighbor-toggle rule | Screenshots and reported Editor playtest |
| Puzzle mechanics | Six plates toggle self and neighbors; 6/6 unlocks exit | Reported working in Editor |
| Gate and goal | Gate rises gradually; player already in goal when solved still completes | Reported working after latest trigger correction |
| Movement | Keyboard movement/jump, plate bypass, wall collision | Reported functional in Editor |
| HUD | Objective/status, clock/best time, lower controls | Screenshots and Editor playtest |
| R hold reset | Continuous 0.9 s hold, radial fill, cancellation on release | Reported functional in Editor; dedicated cancellation test recommended |
| Mouse reset | Immediate restart with new timer | Editor verification recommended |
| Pause | Time freezes, resumes; navigation works | Screenshots and reported playtest |
| Audio | Music/SFX slider, mute, reset, persistence | Settings screenshots; persistence across app restart still needs explicit test |
| Imported puzzle-clear cue | Resources/PuzzleClear.wav plays once on 6/6 via SFX mixer channel, without affecting the gate animation or exit | **PENDING — WAV must be imported locally** |
| Best time | Stops at goal, retains fastest success only | Results screenshots; persistence across app restart still needs explicit test |
| URP lighting | 2D point light falloff, no missing/pink materials | Gameplay screenshots |
| Release cleanup | No missing scene/script/assets after removal | **NOT TESTED** |
| AudioMixer groups | Music and SFX mixer groups assigned as required | **NOT TESTED — mixer asset missing from remote** |
| Clean Unity import | No compile errors or missing references | **NOT TESTED on release branch** |
| Standalone Windows build | External launch, full playthrough, audio, restart, win, quit | **NOT TESTED** |
| Source/disclosure | Complete `.meta` set, source commit, reviewed report and AI citation | **PENDING** |

## Final manual acceptance sequence

1. Open MainMenu and verify Audio controls and Reset Audio.
2. Play Game; verify all switch interactions, jump, URP lighting and controls. Confirm the imported puzzle-clear jingle plays on 6/6, obeys SFX volume/mute, and is heard through the SFX mixer group once configured.
3. Enter the exit flag before the final board state where possible. Solve the board while overlapping; verify Win triggers without leaving/re-entering.
4. Confirm the gate visually rises gradually; no snap.
5. Hold and release R early; ring drains and the board does not reset. Hold past threshold; board and timer reset. Click RESET; same state reset.
6. Pause and resume; confirm game clock freezes while paused.
7. Clear the game; compare Clear Time and Best Time. Restart and clear more slowly to verify only a faster result updates Best Time.
8. Test Music/SFX volume and mute independently, then quit/relaunch and verify persistence.
9. Build a Windows player and repeat the core sequence outside the Editor.

## Delivery evidence

- Unity version: **6000.6.4f1**.
- Tested release source commit: **PENDING**.
- Clean release import result: **PENDING**.
- AudioMixer verification: **PENDING**.
- Standalone build folder/ZIP: **PENDING**.
- Report PDF/DOCX: **PENDING**.
