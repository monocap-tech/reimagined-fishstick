# Relay Yard verification / grading gate

These are test procedures, NOT claims of successful tests.

| Item | Manual acceptance test | Status |
|---|---|---|
| 1 Input System | A/D & arrows move; Space jumps only when grounded; cannot infinitely jump in air | NOT RUN |
| 2 Puzzle | Each relay increases counter once; all three unlock red exit; green exit loads Win | NOT RUN |
| 3 Audio | Original BGM + distinct jump and relay SFX; Music and SFX routed through RelayMixer groups; only one AudioDirector across three scenes; saved menu volume survives relaunch | NOT RUN |
| 4 SO + C# | Inspect cyan/gold RelayDefinition assets and RelayLedger.cs pure C#; both used | NOT RUN |
| 5 Animator | See Locomotion blend tree (Idle/Run), Jump and Fall; parameters change while playing | NOT RUN |
| 6 StateMachineBehaviour | Jump state entry invokes JumpFeedback; feedback audible/visible | NOT RUN |
| 7 Particles | Jump dust and relay activation bursts call ParticleSystem.Play() | NOT RUN |
| 8 Procedural | Change Seed on Procedural Starfield; positions vary deterministically; geometry unaffected | NOT RUN |
| Cohesion | MainMenu > Game > 3 relays > gate > Win > Play again | NOT RUN |
| Hygiene | Zero Console errors in Editor and standalone build, all three scenes in Build Profiles | NOT RUN |
| Repository | Generated scenes, .meta, animator, data, Resources/RelayMixer.mixer and source committed | NOT RUN |
| Documentation | Word/PDF includes script inventory, scenes, rubric coverage, original/third-party disclosures | NOT RUN |
| Canvas | Windows/WebGL archive + source link + document in a single submission; same build in peer review | NOT RUN |

Track actual results and defects here after running Unity. Do not mark PASS without having performed the test.
