# Verification and submission

No Unity Editor is available in the preparation environment. Source/JSON checks do not establish Unity compilation, shader support, or playability. Complete these checks on the assembled project and record the actual results in PROJECT1_DOCUMENTATION.md.

## Play-mode acceptance

- [ ] Launch MainMenu; click Start; Gameplay loads with no Console errors.
- [ ] WASD and arrow keys move the player; diagonal speed is not faster.
- [ ] Push against all four tilemap boundaries. The player cannot leave the arena.
- [ ] Hold Space. Bullets fire upward and score exactly once per hit. All three targets relocate inside their lanes.
- [ ] Expand BulletPool in the Hierarchy. It starts with 12 bullet children; fire for at least 30 seconds and confirm those same instances activate/deactivate without a steadily increasing object count. Optional: set poolSize to 1 to see exhaustion grow the pool, then restore 12.
- [ ] Fire into a wall; bullets deactivate. Miss a target; a shot returns by wall contact or its two-second lifetime.
- [ ] Escape pauses motion, firing, target hits, and projectile lifetimes. Score stays fixed. Canvas buttons still respond.
- [ ] Escape and Resume both resume correctly. Repeat pause/resume several times.
- [ ] Quit to Menu while paused. Menu loads and Start begins a fresh run with normal movement; saved best stays visible.
- [ ] Targets visibly pulse through the custom material. Pause freezes the time-based pulse.
- [ ] MainMenu Quit Game stops Play mode. In a desktop build, it closes the application.
- [ ] Test at 1280×720 and another 16:9 size; buttons and score remain readable.
- [ ] Confirm no Console errors on launch, normal play, pause, menu return, or restarting gameplay.

## Standalone build

1. File → Build Profiles: MainMenu enabled at index 0, Gameplay enabled next. Remove unrelated scenes from the submission Scene List. Check any active-profile override.
2. Select Windows (or macOS) build support matching your installed Unity 6 LTS; install the module through Hub if necessary.
3. Build into an **empty output folder** such as `Build/NeonRange-Windows`, separate from the project's Assets. Never select the Unity project root as a build destination.
4. Launch the build. Repeat the acceptance checks that apply outside the Editor.
5. Set a new best score, quit the application completely, and relaunch **the same build**. Confirm the saved best appears before Start. Keep company/product names unchanged for this check, because PlayerPrefs storage depends on them.
6. Inspect the player log for errors. On Windows it is normally under `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\Player.log`.
7. Zip the **entire output folder**, including the executable, its `_Data` folder, UnityPlayer.dll, and every other generated supporting file. For macOS, preserve the complete `.app` bundle.
8. Extract the archive to a different folder and run the extracted build; repeat menu, gameplay, pause, and persistence checks.
9. Update the verification record in PROJECT1_DOCUMENTATION.md with real results. Commit the assembled project's Assets plus every `.meta`, Packages, and ProjectSettings to this repository. Confirm the public GitHub link opens when signed out.
10. Submit the playable archive and short documentation to Canvas. Post the same build to the Project 1 discussion board by **Friday, October 9, 2026, 11:59 PM**. Play and review at least three classmates by **Friday, October 16**.

WebGL is an allowed fallback per the supplied project instructions, but do not double-click its index.html for testing: it requires a web server. Browser persistence and quitting behave differently from desktop builds; the MainMenu return path remains available. Desktop is the shortest path for this prototype.
