# Relay Yard six-switch verification checklist

These are **acceptance tests, not claims of a passing Unity run**. Run them in Unity Editor, then repeat the core game loop in an unpacked standalone build.

| Area | Acceptance criterion | Status |
|---|---|---|
| Unity import | Input System enabled, assets compile without red errors | NOT RUN |
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

Known potential defects to test first: plate trigger overlap at takeoff/landing; ability to make the prescribed jumps; Visual Animator parameter transitions; halo appearance with Built-In; particle rendering; and physical gate collision while lifting.
