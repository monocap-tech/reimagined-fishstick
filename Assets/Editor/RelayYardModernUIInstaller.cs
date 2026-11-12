#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RelayYard.EditorTools
{
    /// <summary>
    /// In-place UI revision. Preserves puzzle, URP lighting, colliders, audio,
    /// and existing Canvas; deactivates (does not delete) legacy UI.
    /// Creates complete backups of all three saved scenes before editing.
    /// </summary>
    public static class RelayYardModernUIInstaller
    {
        private const string Scenes = "Assets/Scenes/";
        private const string Backups = "Assets/InterfaceBackups";
        private const string ModernCanvasName = "Relay Yard Modern UI";
        private const string ControlsPath = "Assets/Input/RelayControls.inputactions";
        private const string RingPath = "Assets/Sprites/ResetRing.png";

        private static readonly Color Base = new Color(0.065f, 0.098f, 0.15f, 0.96f);
        private static readonly Color PanelColor = new Color(0.086f, 0.133f, 0.19f, 0.97f);
        private static readonly Color TextColor = new Color(0.90f, 0.93f, 0.94f);
        private static readonly Color Dim = new Color(0.62f, 0.72f, 0.78f);
        private static readonly Color Cyan = new Color(0.32f, 0.80f, 0.82f);
        private static readonly Color Amber = new Color(0.91f, 0.73f, 0.36f);

        [MenuItem("Tools/Relay Yard/Upgrade to Final HUD and Audio")]
        public static void Install()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Exit Play Mode",
                    "Stop Play before modifying the saved scenes.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            if (input == null || input.FindAction("Gameplay/ResetHold", false) == null)
            {
                Debug.LogError("[Relay UI] Missing Gameplay/ResetHold. Wait for Unity to reimport " + ControlsPath);
                return;
            }

            foreach (string sceneName in new[] { "MainMenu", "Game", "Win" })
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Scenes + sceneName + ".unity") == null)
                {
                    Debug.LogError("[Relay UI] Required scene missing: " + Scenes + sceneName + ".unity");
                    return;
                }
            }

            if (!EditorUtility.DisplayDialog("Relay Yard: final interface",
                "Back up all three scenes, preserve the URP puzzle environment and " +
                "deactivate the previous Canvas. Install objective/time top band, " +
                "controls/reset bottom band, audio settings, and time results?",
                "Back Up and Upgrade", "Cancel"))
                return;

            if (!AssetDatabase.IsValidFolder(Backups))
                AssetDatabase.CreateFolder("Assets", "InterfaceBackups");

            // Backups happen before any saved scene is edited.
            foreach (string sceneName in new[] { "MainMenu", "Game", "Win" })
            {
                string source = Scenes + sceneName + ".unity";
                string backup = AssetDatabase.GenerateUniqueAssetPath(
                    Backups + "/" + sceneName + "-BeforeModernUI.unity");
                if (!AssetDatabase.CopyAsset(source, backup))
                {
                    Debug.LogError("[Relay UI] Backup failed for " + source +
                                   ". Conversion canceled before editing any scenes.");
                    return;
                }
            }
            AssetDatabase.SaveAssets();

            string previousScene = SceneManager.GetActiveScene().path;
            try
            {
                Sprite ring = CreateRingSprite();
                if (ring == null)
                    throw new InvalidOperationException("Reset ring sprite could not be imported.");

                foreach (string sceneName in new[] { "MainMenu", "Game", "Win" })
                {
                    Scene scene = EditorSceneManager.OpenScene(Scenes + sceneName + ".unity");
                    if (Find(scene, ModernCanvasName) != null)
                    {
                        Debug.Log("[Relay UI] Already modernized: " + sceneName + ". Leaving it intact.");
                        continue;
                    }

                    SceneUI ui = FindComponent<SceneUI>(scene);
                    if (ui == null)
                        throw new InvalidOperationException("SceneUI missing from " + sceneName);
                    SceneLoader loader = ui.GetComponent<SceneLoader>();
                    if (loader == null)
                        loader = ui.gameObject.AddComponent<SceneLoader>();

                    var canvas = CreateCanvas();
                    EnsureEventSystem(scene);

                    if (sceneName == "MainMenu")
                        BuildMainMenu(canvas.transform, ui, loader);
                    else if (sceneName == "Game")
                        BuildGame(scene, canvas.transform, ui, loader, input, ring);
                    else
                        BuildWin(canvas.transform, ui, loader);

                    // Keep old UI serialized as a rollback, but remove it
                    // from rendering, input hit-testing and navigation.
                    Transform oldCanvas = Find(scene, "Relay Yard Canvas");
                    if (oldCanvas != null) oldCanvas.gameObject.SetActive(false);

                    EditorUtility.SetDirty(ui);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log("[Relay UI] Updated " + sceneName +
                              " without rebuilding gameplay objects.");
                }

                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Relay Yard",
                    "All three scenes upgraded. Existing Canvases are retained inactive. " +
                    "Review MainMenu, Game, Pause, Audio and Win in Play mode. " +
                    "Original scenes are in Assets/InterfaceBackups.", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Relay UI] Upgrade interrupted: " + ex +
                               ". Restore scene backups if needed.");
            }
            finally
            {
                if (!string.IsNullOrEmpty(previousScene) &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(previousScene) != null)
                    EditorSceneManager.OpenScene(previousScene);
            }
        }

        private static Transform Find(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                    if (item.name == name)
                        return item;
            return null;
        }

        private static T FindComponent<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T item = root.GetComponentInChildren<T>(true);
                if (item != null) return item;
            }
            return null;
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject(ModernCanvasName,
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void EnsureEventSystem(Scene scene)
        {
            EventSystem eventSystem = FindComponent<EventSystem>(scene);
            if (eventSystem == null)
            {
                var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            else if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                foreach (BaseInputModule module in eventSystem.GetComponents<BaseInputModule>())
                    module.enabled = false;
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
        }

        private static Image Panel(Transform parent, string name,
            Vector2 position, Vector2 size, Color color, bool blocksRaycast = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = blocksRaycast;
            image.rectTransform.anchoredPosition = position;
            image.rectTransform.sizeDelta = size;
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void AnchorTop(RectTransform rect, Vector2 offset, Vector2 size, bool right)
        {
            Vector2 anchor = right ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = right ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;
        }

        private static void AnchorBottom(RectTransform rect, Vector2 offset, Vector2 size, bool right)
        {
            Vector2 anchor = right ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = right ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;
        }

        private static Text Label(Transform parent, string name, string value,
            Vector2 position, Vector2 size, int fontSize, Color color,
            TextAnchor align = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = color;
            text.text = value;
            text.raycastTarget = false;
            text.rectTransform.anchoredPosition = position;
            text.rectTransform.sizeDelta = size;
            return text;
        }

        private enum Tone { Primary, Standard, Quiet }

        private static Button MakeButton(Transform parent, string label,
            Vector2 position, Vector2 size, UnityAction click,
            Tone tone = Tone.Standard, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var image = go.GetComponent<Image>();
            image.color = Color.white;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            Color baseColor = tone == Tone.Primary ? new Color(0.12f, 0.41f, 0.47f) :
                tone == Tone.Quiet ? new Color(0.13f, 0.19f, 0.27f) :
                new Color(0.16f, 0.26f, 0.34f);
            colors.normalColor = baseColor;
            colors.highlightedColor = Color.Lerp(baseColor, Cyan, 0.38f);
            colors.selectedColor = Color.Lerp(baseColor, Cyan, 0.28f);
            colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.25f);
            colors.disabledColor = new Color(0.19f, 0.23f, 0.29f, 0.45f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            UnityEventTools.AddPersistentListener(button.onClick, click);
            Label(go.transform, "Label", label, Vector2.zero,
                size - new Vector2(16f, 6f), 21, TextColor, alignment);
            return button;
        }

        private static Slider MakeSlider(Transform parent, string name,
            Vector2 position, UnityAction<float> onValueChanged)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(312f, 36f);

            var track = Panel(go.transform, "Track", Vector2.zero,
                new Vector2(290f, 8f), new Color(0.16f, 0.23f, 0.31f));
            track.raycastTarget = false;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.5f);
            fillAreaRect.sizeDelta = new Vector2(-22f, 8f);
            fillAreaRect.anchoredPosition = Vector2.zero;
            var fill = Panel(fillArea.transform, "Fill", Vector2.zero, Vector2.zero, Cyan, false);
            Stretch(fill.rectTransform);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            Stretch(handleAreaRect);
            handleAreaRect.offsetMin = new Vector2(11f, 0f);
            handleAreaRect.offsetMax = new Vector2(-11f, 0f);
            var handle = Panel(handleArea.transform, "Handle", Vector2.zero,
                new Vector2(16f, 27f), Amber);
            var slider = go.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.direction = Slider.Direction.LeftToRight;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.SetValueWithoutNotify(0.5f);
            UnityEventTools.AddPersistentListener(slider.onValueChanged, onValueChanged);
            return slider;
        }

        private static AudioSettingsPanel MakeAudioOverlay(Transform parent)
        {
            // The controller lives on the always-active Canvas, even when the
            // overlay is closed. Its Open() callback can therefore activate it.
            var controller = parent.GetComponent<AudioSettingsPanel>();
            if (controller == null)
                controller = parent.gameObject.AddComponent<AudioSettingsPanel>();

            var overlay = Panel(parent, "Audio Settings Overlay",
                Vector2.zero, Vector2.zero, new Color(0.025f, 0.042f, 0.069f, 0.96f));
            Stretch(overlay.rectTransform);

            var card = Panel(overlay.transform, "Audio Settings",
                Vector2.zero, new Vector2(665f, 480f), PanelColor);
            Label(card.transform, "Title", "AUDIO",
                new Vector2(-255f, 176f), new Vector2(450f, 50f), 42,
                TextColor, TextAnchor.MiddleLeft);
            Label(card.transform, "Music", "MUSIC",
                new Vector2(-245f, 95f), new Vector2(240f, 32f), 19,
                TextColor, TextAnchor.MiddleLeft);
            Label(card.transform, "SFX", "SOUND EFFECTS",
                new Vector2(-245f, -4f), new Vector2(240f, 32f), 19,
                TextColor, TextAnchor.MiddleLeft);

            Slider music = MakeSlider(card.transform, "Music Slider",
                new Vector2(-80f, 56f), controller.ChangeMusic);
            Slider sfx = MakeSlider(card.transform, "SFX Slider",
                new Vector2(-80f, -43f), controller.ChangeSfx);

            Text musicValue = Label(card.transform, "Music Percent", "55%",
                new Vector2(270f, 56f), new Vector2(70f, 40f), 18, Dim);
            Text sfxValue = Label(card.transform, "SFX Percent", "75%",
                new Vector2(270f, -43f), new Vector2(70f, 40f), 18, Dim);

            Button muteMusic = MakeButton(card.transform, "MUTE",
                new Vector2(245f, 101f), new Vector2(130f, 39f),
                controller.ToggleMusicMute, Tone.Quiet);
            Button muteSfx = MakeButton(card.transform, "MUTE",
                new Vector2(245f, 2f), new Vector2(130f, 39f),
                controller.ToggleSfxMute, Tone.Quiet);

            Text musicMute = muteMusic.transform.Find("Label").GetComponent<Text>();
            Text sfxMute = muteSfx.transform.Find("Label").GetComponent<Text>();

            MakeButton(card.transform, "TEST SFX", new Vector2(-206f, -130f),
                new Vector2(177f, 44f), controller.PreviewEffect, Tone.Quiet);
            MakeButton(card.transform, "RESET AUDIO", new Vector2(28f, -130f),
                new Vector2(210f, 44f), controller.ResetAudio, Tone.Quiet);
            MakeButton(card.transform, "BACK", new Vector2(0f, -196f),
                new Vector2(268f, 46f), controller.Close, Tone.Primary);

            controller.EditorConfigure(overlay.gameObject, music, sfx,
                musicValue, sfxValue, musicMute, sfxMute);
            overlay.gameObject.SetActive(false);
            return controller;
        }

        private static void BuildMainMenu(Transform root, SceneUI ui, SceneLoader loader)
        {
            var wash = Panel(root, "Menu Wash", Vector2.zero, Vector2.zero, Base, false);
            Stretch(wash.rectTransform);

            Label(root, "Title", "RELAY YARD",
                new Vector2(-325f, 150f), new Vector2(640f, 100f), 64,
                TextColor, TextAnchor.MiddleLeft);
            var accent = Panel(root, "Accent Line",
                new Vector2(-515f, 83f), new Vector2(60f, 4f), Cyan, false);
            Label(root, "Rule", "Stepping on a plate also toggles its neighbors.",
                new Vector2(-287f, 40f), new Vector2(700f, 50f), 24,
                Dim, TextAnchor.MiddleLeft);

            MakeButton(root, "START", new Vector2(-334f, -59f),
                new Vector2(360f, 56f), loader.LoadGame, Tone.Primary);
            AudioSettingsPanel audio = MakeAudioOverlay(root);
            MakeButton(root, "AUDIO", new Vector2(-334f, -132f),
                new Vector2(360f, 52f), audio.Open);
            MakeButton(root, "QUIT", new Vector2(-334f, -202f),
                new Vector2(360f, 52f), loader.QuitGame, Tone.Quiet);
            ui.EditorBindModernUI(null, null, null, null, null);
        }

        private static void BuildWin(Transform root, SceneUI ui, SceneLoader loader)
        {
            var wash = Panel(root, "Result Wash", Vector2.zero,
                Vector2.zero, Base, false);
            Stretch(wash.rectTransform);

            Label(root, "Result Title", "CIRCUIT RESTORED",
                new Vector2(0f, 146f), new Vector2(790f, 70f), 49, Cyan);
            Label(root, "Clear Label", "CLEAR TIME",
                new Vector2(-185f, 53f), new Vector2(250f, 34f), 18, Dim);
            Text clear = Label(root, "Clear Time", "--:--.--",
                new Vector2(-185f, 10f), new Vector2(280f, 53f), 34, TextColor);
            Label(root, "Best Label", "BEST TIME",
                new Vector2(185f, 53f), new Vector2(250f, 34f), 18, Dim);
            Text best = Label(root, "Best Time", "--:--.--",
                new Vector2(185f, 10f), new Vector2(280f, 53f), 34, TextColor);
            Text record = Label(root, "Record", "",
                new Vector2(0f, -45f), new Vector2(490f, 37f), 24, Amber);

            MakeButton(root, "PLAY AGAIN", new Vector2(-195f, -150f),
                new Vector2(328f, 56f), loader.LoadGame, Tone.Primary);
            MakeButton(root, "MAIN MENU", new Vector2(195f, -150f),
                new Vector2(328f, 56f), loader.LoadMenu);
            ui.EditorBindModernUI(null, null, best, clear, record);
        }

        private static void BuildGame(Scene scene, Transform root, SceneUI ui,
            SceneLoader loader, InputActionAsset input, Sprite ring)
        {
            var runClock = FindComponent<RelayRunClock>(scene);
            if (runClock == null)
                runClock = new GameObject("Relay Run Clock").AddComponent<RelayRunClock>();

            var top = Panel(root, "Objective / Time Band", Vector2.zero,
                new Vector2(0f, 78f), new Color(0.06f, 0.10f, 0.16f, 0.93f), false);
            var topRect = top.rectTransform;
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.anchoredPosition = Vector2.zero;

            Text objective = Label(top.transform, "Objective",
                "Light all six indicators",
                Vector2.zero, new Vector2(540f, 34f), 22, TextColor,
                TextAnchor.MiddleLeft);
            AnchorTop(objective.rectTransform, new Vector2(22f, -9f),
                new Vector2(540f, 34f), false);
            Text status = Label(top.transform, "Circuit Status",
                "CIRCUITS 02 / 06", Vector2.zero, new Vector2(470f, 24f), 16,
                Cyan, TextAnchor.MiddleLeft);
            AnchorTop(status.rectTransform, new Vector2(22f, -43f),
                new Vector2(470f, 26f), false);
            Text timer = Label(top.transform, "Current Time", "00:00.00",
                Vector2.zero, new Vector2(175f, 40f), 32, TextColor,
                TextAnchor.MiddleRight);
            AnchorTop(timer.rectTransform, new Vector2(-22f, -3f),
                new Vector2(175f, 40f), true);
            Text best = Label(top.transform, "Best Time", "BEST --:--.--",
                Vector2.zero, new Vector2(210f, 26f), 17, Dim,
                TextAnchor.MiddleRight);
            AnchorTop(best.rectTransform, new Vector2(-22f, -44f),
                new Vector2(210f, 26f), true);

            var bottom = Panel(root, "Controls Band", Vector2.zero,
                new Vector2(0f, 73f), new Color(0.06f, 0.10f, 0.16f, 0.93f), false);
            var bottomRect = bottom.rectTransform;
            bottomRect.anchorMin = new Vector2(0f, 0f);
            bottomRect.anchorMax = new Vector2(1f, 0f);
            bottomRect.pivot = new Vector2(0.5f, 0f);
            bottomRect.anchoredPosition = Vector2.zero;

            Text move = Label(bottom.transform, "Move Command", "A / D     MOVE",
                Vector2.zero, new Vector2(240f, 47f), 20, TextColor,
                TextAnchor.MiddleLeft);
            AnchorBottom(move.rectTransform, new Vector2(20f, 15f),
                new Vector2(240f, 47f), false);
            Text jump = Label(bottom.transform, "Jump Command", "SPACE     JUMP",
                Vector2.zero, new Vector2(250f, 47f), 20, TextColor,
                TextAnchor.MiddleLeft);
            AnchorBottom(jump.rectTransform, new Vector2(266f, 15f),
                new Vector2(250f, 47f), false);
            Text pause = Label(bottom.transform, "Pause Command", "ESC     PAUSE",
                Vector2.zero, new Vector2(210f, 47f), 20, TextColor,
                TextAnchor.MiddleLeft);
            AnchorBottom(pause.rectTransform, new Vector2(550f, 15f),
                new Vector2(210f, 47f), false);

            var reset = FindComponent<ResetHoldController>(scene);
            if (reset == null)
                reset = new GameObject("Reset Hold Controller").AddComponent<ResetHoldController>();

            // A clickable area contains the 360-degree R hold indicator.
            var resetButton = MakeButton(bottom.transform, "RESET",
                Vector2.zero, new Vector2(206f, 61f),
                reset.ClickRestart, Tone.Quiet);
            AnchorBottom(resetButton.GetComponent<RectTransform>(),
                new Vector2(-16f, 5f), new Vector2(206f, 61f), true);
            var title = resetButton.transform.Find("Label").GetComponent<Text>();
            title.alignment = TextAnchor.MiddleRight;
            title.fontSize = 18;
            title.rectTransform.anchoredPosition = new Vector2(-4f, 0f);
            title.text = "RESET";

            Image underRing = Panel(resetButton.transform, "Reset Ring Track",
                new Vector2(-71f, 0f), new Vector2(47f, 47f),
                new Color(0.28f, 0.38f, 0.44f, 0.75f), false);
            underRing.sprite = ring;
            underRing.type = Image.Type.Simple;

            Image progress = Panel(resetButton.transform, "Hold Progress",
                new Vector2(-71f, 0f), new Vector2(47f, 47f),
                Amber, false);
            progress.sprite = ring;
            progress.type = Image.Type.Filled;
            progress.fillMethod = Image.FillMethod.Radial360;
            progress.fillOrigin = (int)Image.Origin360.Top;
            progress.fillClockwise = true;
            progress.fillAmount = 0f;
            Label(resetButton.transform, "R Symbol", "R",
                new Vector2(-71f, 0f), new Vector2(36f, 36f),
                21, TextColor);
            reset.EditorConfigure(input, loader, progress);

            var pauseOverlay = Panel(root, "Pause Overlay", Vector2.zero,
                Vector2.zero, new Color(0.015f, 0.028f, 0.05f, 0.84f));
            Stretch(pauseOverlay.rectTransform);
            var card = Panel(pauseOverlay.transform, "Pause Actions",
                Vector2.zero, new Vector2(540f, 500f), PanelColor);
            Label(card.transform, "Paused Title", "PAUSED",
                new Vector2(0f, 170f), new Vector2(450f, 60f), 44, TextColor);

            var manager = FindComponent<PauseManager>(scene);
            if (manager == null)
                manager = new GameObject("Pause Manager").AddComponent<PauseManager>();

            MakeButton(card.transform, "RESUME", new Vector2(0f, 93f),
                new Vector2(340f, 52f), manager.Resume, Tone.Primary);
            MakeButton(card.transform, "RESTART", new Vector2(0f, 26f),
                new Vector2(340f, 52f), manager.RestartPuzzle);
            AudioSettingsPanel audio = MakeAudioOverlay(pauseOverlay.transform);
            MakeButton(card.transform, "AUDIO", new Vector2(0f, -41f),
                new Vector2(340f, 52f), audio.Open);
            MakeButton(card.transform, "MAIN MENU", new Vector2(0f, -108f),
                new Vector2(340f, 52f), manager.QuitToMenu, Tone.Quiet);
            Label(card.transform, "Back Hint", "ESC  -  RESUME",
                new Vector2(0f, -180f), new Vector2(320f, 35f), 17, Dim);

            manager.EditorConfigure(input, pauseOverlay.gameObject, loader, audio);
            pauseOverlay.gameObject.SetActive(false);
            ui.EditorBindModernUI(status, timer, best, null, null);
        }

        private static Sprite CreateRingSprite()
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(RingPath);
            if (existing != null) return existing;

            if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
                AssetDatabase.CreateFolder("Assets", "Sprites");

            const int dimension = 96;
            var tex = new Texture2D(dimension, dimension, TextureFormat.RGBA32, false);
            var pixels = new Color[dimension * dimension];
            float radius = dimension / 2f - 3f;
            for (int y = 0; y < dimension; y++)
                for (int x = 0; x < dimension; x++)
                {
                    float dx = x - dimension / 2f + 0.5f;
                    float dy = y - dimension / 2f + 0.5f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(radius - distance + 0.5f) *
                                  Mathf.Clamp01(distance - (radius - 7f) + 0.5f);
                    pixels[y * dimension + x] = new Color(1f, 1f, 1f, alpha);
                }
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(RingPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(RingPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(RingPath) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException("Cannot import ring texture.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = dimension;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(RingPath);
        }
    }
}
#endif
