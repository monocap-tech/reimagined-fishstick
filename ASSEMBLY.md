# Relay Yard — Unity assembly procedure

## Preconditions

1. Unity 6 LTS (the previous course project used 6000.6.4f1); Input System package must resolve. Use a regular Unity 6 2D project if opening the source scaffold directly does not produce normal ProjectSettings.
2. Clone the cpsc386-project2-relay-yard branch and open the project in Unity Hub. Alternatively create a new Unity 6 2D Built-In project and copy Assets, Packages and ProjectSettings/ProjectVersion.txt into it. Wait for asset import and C# compilation.
3. Set Project Settings > Player > Active Input Handling to Input System Package (New) or Both. Restart Unity if prompted.
4. Fix any red Console errors before proceeding.

## Generate the scene assets

5. In Unity's menu run Tools > Relay Yard > Assemble Playable Prototype. This creates:
   - MainMenu, Game, Win scenes in Assets/RelayYardGenerated/Scenes
   - Player with Rigidbody2D, BoxCollider2D, Input Actions, ground probe and Animator
   - Floor, one raised platform, three trigger relays, exit gate
   - 1D locomotion blend tree, Jump/Fall states and JumpFeedback StateMachineBehaviour
   - two different RelayDefinition ScriptableObject instances
   - jump and relay particles, seeded decorative starfield
   - Build Profiles scene list: MainMenu, Game, Win
6. Open Game scene and inspect RelayController, the relays, Player and ground layer. The assembler can overwrite generated scenes on repeated runs; save custom changes separately.

## REQUIRED manual AudioMixer setup — do not skip

7. Create folder Assets/Resources if it does not exist. In that folder create an Audio Mixer asset named EXACTLY RelayMixer.mixer (Assets > Create > Audio Mixer).
8. Open RelayMixer and add two child groups under Master, named EXACTLY Music and SFX. Save asset.
9. On launch AudioDirector loads Resources/RelayMixer and routes its Music and SFX AudioSources to the corresponding groups. It carries over between MainMenu, Game and Win and restores the saved music-source volume from PlayerPrefs.
10. Check AudioDirector.MixerConnected in a debugger or add a temporary Inspector check; if false, verify the mixer name, Resources location and group spellings. Do not submit without actual Music/SFX routing.

The assignment does NOT require an exposed AudioMixer volume parameter: it requires a saved volume setting. Relay Yard persists the music AudioSource volume using PlayerPrefs.

## Controls and test

11. Enter Play in MainMenu. Move with A/D or left/right arrows. Jump with Space.
12. Collect Relays A, B and C by touching them. One is on the elevated platform; use jump. Gate changes from red to green only after all three relays. Touch green gate to enter Win.
13. Return to Menu or Win and move the Music volume slider; leave Play mode, start Play again, and verify restored value.
14. Confirm music is continuous across Menu > Game > Win, and that only one AudioDirector survives transitions.
15. Change Seed on Procedural Starfield to a different integer, run Game and inspect the generated pattern. The fixed platforms and relays should not move.

## Finalize for GitHub/Canvas

16. Commit Assets/** including generated scenes, ScriptableObjects, Animator, generated PNG and ALL .meta files, alongside Packages, ProjectSettings and documentation. The branch's present source-only state is not final.
17. Produce a Windows build from File > Build Profiles into Build/RelayYard-Windows. Zip the entire build output, then unzip it elsewhere and launch the .exe. If using WebGL, serve or host the output; opening index.html directly does not reliably work.
18. Export your completed documentation from docs/PROJECT2_DOCUMENTATION.md to PDF/Word. Attach build ZIP, source link and document in one Canvas submission. Peer-review post must use the exact same build.
