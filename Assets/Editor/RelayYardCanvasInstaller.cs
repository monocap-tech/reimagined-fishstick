#if UNITY_EDITOR
using System;
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
    /// Adds native Canvas UI to the EXISTING assembled scenes without rebuilding
    /// or touching the puzzle, character, gate, animation, or audio objects.
    /// Adapted from Project 1's NeonRangeBuilder Canvas/EventSystem pattern.
    /// </summary>
    public static class RelayYardCanvasInstaller
    {
        private const string SceneDirectory = "Assets/Scenes/";
        private const string InputPath = "Assets/Input/RelayControls.inputactions";
        private const string CanvasName = "Relay Yard Canvas";

        [MenuItem("Tools/Relay Yard/Upgrade UI In Existing Scenes")]
        public static void UpgradeExistingScenes()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Exit Play Mode", "Stop Play before editing scene assets.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (!EditorUtility.DisplayDialog("Add Canvas UI and Pause",
                "Back up MainMenu, Game and Win to Assets/SceneBackups first, then add " +
                "native Canvas controls and a pause overlay without recreating the game objects. " +
                "Existing 'Relay Yard Canvas' objects will be left unchanged. Continue?",
                "Back Up and Upgrade", "Cancel"))
                return;

            if (UpgradeScenes(true))
                EditorUtility.DisplayDialog("Relay Yard UI",
                    "Scenes upgraded and saved. Open Assets/Scenes/MainMenu.unity and Play. " +
                    "Escape pauses in Game. Backups are under Assets/SceneBackups. " +
                    "Verify the Console and TESTING.md before building.", "OK");
        }

        // Called by the full assembler only after it has generated new scenes.
        // Use UpgradeExistingScenes for an already playable locally edited project.
        public static bool InstallForNewScenes() => UpgradeScenes(false);

        private static bool UpgradeScenes(bool makeBackups)
        {
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            if (actions == null || actions.FindAction("Gameplay/Pause", false) == null)
            {
                Debug.LogError("[Relay Yard UI] Pause action is missing from " + InputPath +
                               ". Allow Unity to finish importing after pulling.");
                return false;
            }

            foreach (string name in new[] { "MainMenu", "Game", "Win" })
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneDirectory + name + ".unity") == null)
                {
                    Debug.LogError("[Relay Yard UI] Missing Assets/Scenes/" + name +
                                   ".unity. The non-destructive upgrade requires the assembled scenes.");
                    return false;
                }
            }

            if (makeBackups)
            {
                if (!AssetDatabase.IsValidFolder("Assets/SceneBackups"))
                    AssetDatabase.CreateFolder("Assets", "SceneBackups");

                foreach (string name in new[] { "MainMenu", "Game", "Win" })
                {
                    string source = SceneDirectory + name + ".unity";
                    string backup = AssetDatabase.GenerateUniqueAssetPath(
                        "Assets/SceneBackups/" + name + "-BeforeCanvasUpgrade.unity");
                    if (!AssetDatabase.CopyAsset(source, backup))
                    {
                        Debug.LogError("[Relay Yard UI] Failed to back up " + source +
                                       ". No scenes have been modified.");
                        return false;
                    }
                }
                AssetDatabase.SaveAssets();
            }

            string previous = SceneManager.GetActiveScene().path;
            try
            {
                foreach (string name in new[] { "MainMenu", "Game", "Win" })
                {
                    var scene = EditorSceneManager.OpenScene(SceneDirectory + name + ".unity");
                    var oldCanvas = GameObject.Find(CanvasName);
                    if (oldCanvas != null && oldCanvas.GetComponent<Canvas>() != null)
                    {
                        Debug.Log("[Relay Yard UI] Existing Canvas kept in " + name + ".");
                        continue;
                    }

                    SceneUI ui = UnityEngine.Object.FindAnyObjectByType<SceneUI>();
                    if (ui == null)
                        throw new InvalidOperationException(
                            name + " must contain the SceneUI component from the current prototype.");

                    SceneLoader loader = ui.GetComponent<SceneLoader>();
                    if (loader == null) loader = ui.gameObject.AddComponent<SceneLoader>();

                    Canvas canvas = CreateCanvas();
                    EnsureEventSystem();

                    if (name == "Game")
                        BuildGame(canvas.transform, ui, loader, actions);
                    else if (name == "MainMenu")
                        BuildMenu(canvas.transform, ui, loader);
                    else
                        BuildWin(canvas.transform, ui, loader);

                    EditorUtility.SetDirty(ui);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log("[Relay Yard UI] Installed native Canvas in " + name + ".");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[Relay Yard UI] Upgrade interrupted: " + ex);
                return false;
            }
            finally
            {
                if (!string.IsNullOrEmpty(previous) &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(previous) != null)
                    EditorSceneManager.OpenScene(previous);
            }

            AssetDatabase.SaveAssets();
            return true;
        }

        private static Canvas CreateCanvas()
        {
            var root = new GameObject(CanvasName, typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 25;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            else if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                foreach (BaseInputModule old in eventSystem.GetComponents<BaseInputModule>())
                    old.enabled = false;
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
        }

        private static Image Panel(Transform parent, string name, Vector2 at,
            Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            var rect = image.rectTransform;
            rect.sizeDelta = size;
            rect.anchoredPosition = at;
            return image;
        }

        private static Text Label(Transform parent, string name, string value,
            Vector2 at, Vector2 size, int fontSize, Color color,
            TextAnchor align = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<Text>();
            label.rectTransform.anchoredPosition = at;
            label.rectTransform.sizeDelta = size;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = align;
            label.color = color;
            label.text = value;
            label.raycastTarget = false;
            return label;
        }

        private static Button MakeButton(Transform parent, string title,
            Vector2 at, Vector2 size, UnityAction action)
        {
            var go = new GameObject(title + " Button", typeof(RectTransform),
                typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = at;
            rect.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.12f, 0.27f, 0.37f, 1f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            UnityEventTools.AddPersistentListener(button.onClick, action);
            Label(go.transform, "Label", title, Vector2.zero,
                size - new Vector2(12f, 6f), 22, Color.white);
            return button;
        }

        private static Slider MakeMusicSlider(Transform parent, Vector2 at,
            SceneUI controller)
        {
            var go = new GameObject("Music Volume Slider", typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = at;
            rect.sizeDelta = new Vector2(380f, 30f);

            var track = Panel(go.transform, "Track", Vector2.zero,
                new Vector2(360f, 9f), new Color(0.10f, 0.17f, 0.24f));
            track.raycastTarget = false;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fillRect = fillArea.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(1f, 0.5f);
            fillRect.sizeDelta = new Vector2(-20f, 9f);
            fillRect.anchoredPosition = Vector2.zero;
            var fill = Panel(fillArea.transform, "Fill", Vector2.zero,
                Vector2.zero, new Color(0.28f, 0.85f, 0.81f));
            fill.raycastTarget = false;
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var handleRect = handleArea.GetComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = new Vector2(10f, 0f);
            handleRect.offsetMax = new Vector2(-10f, 0f);
            var handle = Panel(handleArea.transform, "Handle", Vector2.zero,
                new Vector2(22f, 25f), new Color(0.96f, 0.82f, 0.40f));
            handle.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            handle.rectTransform.anchorMax = new Vector2(0f, 0.5f);

            var slider = go.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.direction = Slider.Direction.LeftToRight;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.SetValueWithoutNotify(0.55f);
            UnityEventTools.AddPersistentListener(slider.onValueChanged,
                new UnityAction<float>(controller.OnMusicVolumeChanged));
            return slider;
        }

        private static void BuildMenu(Transform root, SceneUI ui, SceneLoader loader)
        {
            var card = Panel(root, "Main Menu Panel", Vector2.zero,
                new Vector2(785f, 490f), new Color(0.025f, 0.045f, 0.09f, 0.94f));
            Label(card.transform, "Title", "RELAY YARD",
                new Vector2(0f, 175f), new Vector2(720f, 55f), 42,
                new Color(0.55f, 0.93f, 0.98f));
            Label(card.transform, "Instructions",
                "Light all six indicators.\nStepping toggles a plate and its neighbors. Jump to skip.",
                new Vector2(0f, 92f), new Vector2(720f, 75f), 21, Color.white);
            MakeButton(card.transform, "Start", new Vector2(0f, -8f),
                new Vector2(360f, 54f), loader.LoadGame);
            MakeButton(card.transform, "Quit Game", new Vector2(0f, -77f),
                new Vector2(360f, 54f), loader.QuitGame);
            Label(card.transform, "Music Label", "Music volume",
                new Vector2(0f, -148f), new Vector2(410f, 33f), 20, Color.white);
            Slider volume = MakeMusicSlider(card.transform, new Vector2(0f, -191f), ui);
            ui.EditorUseCanvasUI(null, volume);
        }

        private static void BuildWin(Transform root, SceneUI ui, SceneLoader loader)
        {
            var card = Panel(root, "Victory Panel", Vector2.zero,
                new Vector2(760f, 440f), new Color(0.035f, 0.11f, 0.10f, 0.95f));
            Label(card.transform, "Title", "CIRCUIT RESTORED",
                new Vector2(0f, 130f), new Vector2(700f, 62f), 38,
                new Color(0.45f, 1f, 0.77f));
            Label(card.transform, "Message", "All six lights are on. The gate is open.",
                new Vector2(0f, 66f), new Vector2(700f, 46f), 22, Color.white);
            MakeButton(card.transform, "Play Again", new Vector2(0f, -16f),
                new Vector2(360f, 54f), loader.LoadGame);
            MakeButton(card.transform, "Main Menu", new Vector2(0f, -86f),
                new Vector2(360f, 54f), loader.LoadMenu);
            Label(card.transform, "Music Label", "Music volume",
                new Vector2(0f, -144f), new Vector2(410f, 30f), 20, Color.white);
            Slider volume = MakeMusicSlider(card.transform, new Vector2(0f, -182f), ui);
            ui.EditorUseCanvasUI(null, volume);
        }

        private static void BuildGame(Transform root, SceneUI ui, SceneLoader loader,
            InputActionAsset actions)
        {
            var top = Panel(root, "HUD Strip", Vector2.zero,
                new Vector2(0f, 62f), new Color(0.018f, 0.044f, 0.074f, 0.88f));
            var bar = top.rectTransform;
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0.5f, 1f);
            bar.anchoredPosition = Vector2.zero;
            top.raycastTarget = false;

            Text status = Label(bar, "Lights Status",
                "LIGHTS 2 / 6   |   STEP TO TOGGLE • JUMP TO SKIP",
                Vector2.zero, new Vector2(820f, 52f), 20, Color.white,
                TextAnchor.MiddleLeft);
            status.rectTransform.anchorMin = new Vector2(0f, 1f);
            status.rectTransform.anchorMax = new Vector2(0f, 1f);
            status.rectTransform.pivot = new Vector2(0f, 1f);
            status.rectTransform.anchoredPosition = new Vector2(18f, -5f);

            Button restart = MakeButton(bar, "Restart", Vector2.zero,
                new Vector2(142f, 44f), loader.RestartGame);
            var restartRect = restart.GetComponent<RectTransform>();
            restartRect.anchorMin = restartRect.anchorMax = new Vector2(1f, 1f);
            restartRect.pivot = new Vector2(1f, 1f);
            restartRect.anchoredPosition = new Vector2(-14f, -8f);

            var overlay = Panel(root, "PausePanel", Vector2.zero,
                Vector2.zero, new Color(0.01f, 0.02f, 0.05f, 0.88f));
            var full = overlay.rectTransform;
            full.anchorMin = Vector2.zero;
            full.anchorMax = Vector2.one;
            full.offsetMin = Vector2.zero;
            full.offsetMax = Vector2.zero;

            var pauseCard = Panel(full, "Pause Card", Vector2.zero,
                new Vector2(475f, 390f), new Color(0.025f, 0.085f, 0.12f, 0.98f));
            Label(pauseCard.transform, "Pause Heading", "PAUSED",
                new Vector2(0f, 130f), new Vector2(420f, 60f), 38,
                new Color(0.58f, 0.92f, 0.98f));

            var manager = new GameObject("Pause Manager").AddComponent<PauseManager>();
            MakeButton(pauseCard.transform, "Resume", new Vector2(0f, 47f),
                new Vector2(310f, 55f), manager.Resume);
            MakeButton(pauseCard.transform, "Restart Puzzle", new Vector2(0f, -27f),
                new Vector2(310f, 55f), manager.RestartPuzzle);
            MakeButton(pauseCard.transform, "Quit to Menu", new Vector2(0f, -101f),
                new Vector2(310f, 55f), manager.QuitToMenu);
            Label(pauseCard.transform, "Help", "Escape to resume",
                new Vector2(0f, -162f), new Vector2(390f, 32f), 17,
                new Color(0.66f, 0.84f, 0.91f));
            manager.EditorConfigure(actions, overlay.gameObject, loader);
            overlay.gameObject.SetActive(false);
            ui.EditorUseCanvasUI(status, null);
        }
    }
}
#endif
