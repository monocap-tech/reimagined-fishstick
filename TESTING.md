# Relay Yard — release verification

**Evidence boundary:** The developer reports both Windows x64 and WebGL builds **validated**. This platform-level report is not an individual pass record for every test below. Exact browser/OS versions, build ZIP names, hosted URL, build-source SHA and per-test logs have not been supplied.

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
| Imported puzzle-clear cue | Resources/PuzzleClear.wav plays once on 6/6 via SFX mixer channel, without affecting the gate animation or exit | **ASSET COMMITTED — standalone test pending** |
| Best time | Stops at goal, retains fastest success only | Results screenshots; persistence across app restart still needs explicit test |
| URP lighting | 2D point light falloff, no missing/pink materials | Gameplay screenshots |
| Release cleanup | No missing scene/script/assets after removal | **NOT TESTED** |
| AudioMixer groups | Music and SFX mixer groups assigned as required | **ASSET PRESENT — runtime routing not yet confirmed** |
| BGM transitions | MainMenu plays 172; Game plays 134; Win continues 134; returning to MainMenu restores 172 | **Reported working in Unity Editor on November 15, 2026; standalone test pending** |
| Clean Unity import | No compile errors or missing references | **NOT TESTED on release branch** |
| Windows x64 build | Build created and validated by developer | **PASS — reported by developer; detailed test log not provided** |
| WebGL build | Browser build created and validated by developer | **PASS — reported by developer; browser/version and hosted URL not provided** |
| Individual cross-platform regression | Confirm all puzzle, reset, pause, audio, UI, and persistence cases on both platforms | **NOT RECORDED AT CASE LEVEL** |
| Source/disclosure | Complete `.meta` set, source commit, reviewed report, third-party credits and AI citation | **Meta audit passed; notices drafted; report pending** |

## Final manual acceptance sequence

Record individual test outcomes here when available; platform-level validation alone does not establish a pass for each acceptance case.

1. Open MainMenu; verify track 172, Audio controls and Reset Audio. Enter Game and verify track 134. Win should continue Game BGM without restarting, and returning to MainMenu should restore track 172.
2. Play Game; verify all switch interactions, jump, URP lighting and controls. Confirm the imported puzzle-clear jingle plays on 6/6, obeys SFX volume/mute, and is heard through the SFX mixer group once configured.
3. Enter the exit flag before the final board state where possible. Solve the board while overlapping; verify Win triggers without leaving/re-entering.
4. Confirm the gate visually rises gradually; no snap.
5. Hold and release R early; ring drains and the board does not reset. Hold past threshold; board and timer reset. Click RESET; same state reset.
6. Pause and resume; confirm game clock freezes while paused.
7. Clear the game; compare Clear Time and Best Time. Restart and clear more slowly to verify only a faster result updates Best Time.
8. Test Music/SFX volume and mute independently, then quit/relaunch and verify persistence.
9. Build a Windows player and repeat the core sequence **outside** the Editor; record Windows version, build folder, full playthrough and any errors.
10. Build WebGL, serve over HTTP or publish to Unity Play, and repeat the game/UI/audio sequence, including browser autoplay and quit-button behavior.

## Delivery evidence

- Unity version: **6000.6.4f1**.
- Tested release source commit: **PENDING — record the SHA used for the binaries, not a subsequent docs-only commit**.
- Source import into the grader's Unity 6 LTS version: **NOT VERIFIED**.
- AudioMixer verification: **PENDING**.
- Windows x64 build: **developer-reported validated; ZIP name, build SHA, and per-test log outstanding**.
- WebGL build: **developer-reported validated; browser version, ZIP/URL, and per-test log outstanding**.
- Build archives/hosted URL: **NOT YET RECORDED**.
- Report PDF/DOCX: **PENDING**.
