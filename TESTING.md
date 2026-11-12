# Relay Yard six-switch verification checklist

These are **acceptance tests, not claims of a passing Unity run**. Run them in Unity Editor, then repeat the core game loop in an unpacked standalone build.

| Area | Acceptance criterion | Status |
|---|---|---|
| Unity import | Input System and Unity UI packages enabled; all scripts compile without red errors | NOT RUN |
| Scene preservation | Before running the in-place UI upgrader, original Game/MainMenu/Win scenes are copied into Assets/SceneBackups and remain recoverable | NOT RUN |
| Native Canvas UI | MainMenu/Game/Win each contain Relay Yard Canvas and EventSystem with InputSystemUIInputModule; no duplicate OnGUI overlay | NOT RUN |
| Pause / Resume | Escape pauses time, movement, gate animation and procedural waveform; Escape again and Resume button restore timeScale=1 | NOT RUN |
| Pause navigation | Restart Puzzle and Quit to Menu work while paused; following scene runs at timeScale=1; replay and pause repeatedly | NOT RUN |
| Menu controls | Start, Quit Game, and saved music-volume slider work; quit stops a desktop player (or Play Mode in Editor) | NOT RUN |
| Responsive UI | Test 1280x720 and another 16:9 resolution; status, waveform, pause card and indicators stay legible | NOT RUN |
| New scene | Regenerated Game has six distinct plates and six distinct lights, safe gaps, a single floor, solid gate plus separate exit | NOT RUN |
| Logic-only test | Tools > Relay Yard > Validate Six-Switch Logic logs PASS for solution 2-4-6, all 64 configurations, reversibility | NOT RUN |
| Puzzle correctness | Press i toggles i, i-1, i+1 (clamped to ends); step-on once, standing still does not retrigger; leaving and returning does | NOT RUN |
| Initial state | Lights start OFF OFF ON OFF ON OFF; 2, 4, 6 solves in that order | NOT RUN |
| Platformer fairness | Player can jump over 1 to 2, over 3 to 4, and over 5 to 6 without landing on skipped plates | NOT RUN |
| Grounded motion | A/D and arrows move, Space jumps when grounded, cannot jump repeatedly in air; zero-friction material avoids wall adhesion | NOT RUN |
| Gate and goal | Gate is solid while locked; brighter when close; lifts on solved; separate goal enters Win only once clear | NOT RUN |
| Feedback | Each plate press gives short ParticleSystem.Emit sparks and SFX, even for rapid/repeated presses; Jump state gives dust and sound | NOT RUN |
| Audio | Music + jump/plate/unlock SFX; AudioMixer Music/SFX groups actually assigned, volume persists, single AudioDirector across scene changes | NOT RUN |
| ScriptableObjects | CyanSwitch.asset and GoldSwitch.asset exist and are both referenced by plates | NOT RUN |
| Pure C# | LightsOutBoard contains no Unity scene dependency; controller uses it | SOURCE-REVIEW ONLY |
| Animator | Locomotion/Jump/Fall states; visible animated clips and Idle/Walk/Run 1D blend tree driven by script | NOT RUN |
| Procedural | Seeded Perlin signal visible; altering seed or noiseScale changes the generated curve, puzzle remains fixed and beatable | NOT RUN |
| Scene switching | Dedicated SceneLoader is attached to Menu/Game/Win UI and goal; MainMenu → Game → Win → Game works and gate blocks early goal entry | NOT RUN |
| Build | Assets/Scenes/MainMenu, Game, Win in build list, Windows/WebGL build launches OUTSIDE Unity, all game states tested | NOT RUN |
| Repo | Standard Assets folders retained; all generated scenes, .meta files, Resources/RelayMixer.mixer and source committed | NOT RUN |
| Submission folder | Build ZIP, source branch URL, PDF/DOCX are distinct deliverables per SUBMISSION.md | NOT RUN |
| Docs | Individually reviewed DOCX/PDF with script inventory, scenes, design decisions, accurate AI/third-party disclosures | NOT RUN |
| Canvas | Standalone ZIP, source link, documentation submitted; same build posted in Peer Review 2 | NOT RUN |

Known potential defects to test first: UI package import and Canvas button wiring; pause toggling after scene changes; plate trigger overlap at takeoff/landing; prescribed jumps; Animator transitions; halo appearance with Built-In; particle rendering; and gate collision while lifting. None are marked passed without actual testing.

## Additional checks on cpsc386-project2-urp-test

These are not yet verified on a Unity workstation:

| Test | Expected result | Status |
|---|---|---|
| URP package | Universal RP 17.6.0 installed, Editor compiles | NOT RUN |
| Pipeline | Graphics and current Quality use URP Asset with 2D Renderer | NOT RUN |
| Scene backup | Assets/URPBackups/Game-BeforeURP2D.unity exists | NOT RUN |
| Player lantern | Player/Lantern Light 2D follows player; physical circular attenuation | NOT RUN |
| Gate receives light | Gate Lit shader illuminates partially on approach, dims on retreat | NOT RUN |
| No fake halo | Gate Proximity Halo disabled; use2DLighting on ExitGate | NOT RUN |
| Room boundaries | Left/right solid walls prevent escaping; goal still reachable | NOT RUN |
| Visual compatibility | Player/lamps remain readable, panels Lit, line and particles visible, no pink materials | NOT RUN |
| Functional regression | Lights Out, gate lift, victory, pause, AudioDirector, restart unaffected | NOT RUN |
| Windows player | Standalone build launches and above features pass outside Editor | NOT RUN |
