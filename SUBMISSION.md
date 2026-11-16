# Relay Yard — Project 2 submission checklist

Submit **three distinct deliverables together in one Canvas submission** (only the latest submitted version counts), as required by the course syllabus:

1. **Playable build:** ZIP the complete tested Windows standalone build directory (the executable **and** its data/runtime files); a WebGL build/archive or public hosted URL is also accepted. Both platforms may be supplied, but the course only requires one working build.
2. **Source:** Link directly to [the Relay Yard release branch](https://github.com/monocap-tech/reimagined-fishstick/tree/cpsc386-project2-release) plus the exact tested commit SHA (the default `main` is a different game), or submit a source ZIP containing `Assets/`, `Packages/`, `ProjectSettings/`, and all required Unity `.meta` files. Do not include `Library/`, `Temp/`, `obj/`, `.vs/`, or disposable build caches.
3. **Documentation:** individually reviewed DOCX or PDF technical report covering mechanics/design, scripts, scenes, rubric mapping, testing, asset sources, and AI assistance.

Proposed hand-in folder (local only; not committed to source Git):

```text
Project2-Submission/
  RelayYard-Windows-x64.zip
  RelayYard-WebGL.zip            # optional alternate; or public hosting URL
  RelayYard-Documentation.pdf
  SOURCE.txt
```

## Release preparation

- [ ] Confirm third-party asset **prior instructor consent**, as required by the syllabus, and document sources, copyright, license and extent of use for the MusMus audio, modified DotGothic16 font, and puzzle-clear jingle. See `THIRD_PARTY_NOTICES.md`.
- [ ] Confirm the instructor's Unity 6 LTS grading environment can open the project recorded as Unity **6000.6.4f1** (in `ProjectSettings/ProjectVersion.txt`).

- [x] Preserve the URP development checkpoint on `cpsc386-project2-urp-test`.
- [ ] Open **`cpsc386-project2-release`** in Unity and allow a clean reimport.
- [ ] Verify the current three scenes, their scripts, animations and shader assets are intact after removing generator scripts/unused prototype assets.
- [x] Commit `Assets/Resources/RelayMixer.mixer`, its `.meta`, and Music/SFX groups. **Runtime assignment of the AudioSources to those groups still needs explicit verification.**
- [x] Commit `Assets/Resources/PuzzleClear.wav` and its `.meta`. **Still verify the cue in the final player and resolve its copyright/distribution status.**
- [ ] Run full gameplay and audio regression tests, including hold R, pause, gate animation, goal trigger overlap and best-time persistence.
- [x] Produce an initial Windows Build Profiles build (reported successful by the developer; standalone gameplay **not yet confirmed**).
- [ ] Unzip the Windows archive to a separate folder and launch the player **outside Unity**; verify an actual full playthrough.
- [ ] Finish WebGL build and test in a browser, then optionally publish to Unity Play.
- [ ] Verify no missing references/Console errors, no development Editor utilities, and a clean source tree.
- [ ] Finish the DOCX/PDF technical report, attach valid screenshots/testing record, cite `THIRD_PARTY_NOTICES.md`, and **disclose ChatGPT assistance** in the manner required by the syllabus.
- [ ] Prepare the source URL and tested commit ID, then submit all three deliverables in one Canvas submission.
- [ ] Post the **same graded build** to Project 2 Peer Review; syllabus lists review deadline **Wednesday, November 18, 2026** and requires playing/reviewing at least three classmates' builds.

**Original Project 2 due date in the syllabus:** Friday, November 13, 2026 (now past). Confirm the Canvas submission portal's current availability/policy; the peer-review milestone is separate.
