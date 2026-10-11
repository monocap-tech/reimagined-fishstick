using System.IO;
using NeonRange;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>Editor-only assembly helper; creates native, editable assets and scenes once.</summary>
public static class NeonRangeBuilder
{
    private const string Generated = "Assets/NeonRangeGenerated";
    private static Material spriteMaterial;
    private static readonly Color Cyan = new Color(0.2f, 0.95f, 1f);
    private static readonly Color Pink = new Color(1f, 0.3f, 0.65f);

    [MenuItem("Tools/Neon Range/Assemble Prototype")]
    public static void Assemble()
    {
        if (EditorApplication.isPlaying) return;
        if (AssetDatabase.IsValidFolder(Generated))
        {
            EditorUtility.DisplayDialog("Prototype already assembled",
                "Open the scenes in Assets/NeonRangeGenerated/Scenes. This helper never overwrites them. See ASSEMBLY.md for rebuilding.", "OK");
            return;
        }
        InputActionAsset input = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Input/PlayerControls.inputactions");
        Shader shader = Shader.Find("NeonRange/NeonPulse");
        if (input == null || shader == null)
        {
            EditorUtility.DisplayDialog("Import needed", "Import all repository Assets, install Input System and Unity UI, and resolve Console errors first.", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        AssetDatabase.CreateFolder("Assets", "NeonRangeGenerated");
        foreach (string folder in new[] { "Scenes", "Art", "Prefabs", "Input" })
            AssetDatabase.CreateFolder(Generated, folder);

        EnsureTag("Enemy");
        EnsureTag("PlayerBullet");
        int playerLayer = EnsureLayer("Player");
        int bulletLayer = EnsureLayer("PlayerBullet");
        int enemyLayer = EnsureLayer("Enemy");
        int wallLayer = EnsureLayer("ArenaWall");
        Sprite square = CreateSquare();
        Shader spriteShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
        if (spriteShader == null) throw new System.InvalidOperationException("No compatible unlit sprite shader found.");
        spriteMaterial = new Material(spriteShader);
        AssetDatabase.CreateAsset(spriteMaterial, Generated + "/Art/SquareUnlit.mat");
        Material pulse = new Material(shader);
        AssetDatabase.CreateAsset(pulse, Generated + "/Art/NeonPulse.mat");
        Tile wallTile = ScriptableObject.CreateInstance<Tile>();
        wallTile.sprite = square;
        wallTile.color = new Color(0.12f, 0.45f, 0.6f);
        wallTile.colliderType = Tile.ColliderType.Grid;
        AssetDatabase.CreateAsset(wallTile, Generated + "/Art/WallTile.asset");
        InputActionReference move = ActionReference(input, "Move");
        InputActionReference fire = ActionReference(input, "Fire");
        InputActionReference pause = ActionReference(input, "Pause");

        Scene gameplay = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        Canvas canvas = CreateCanvas();
        Text scoreText = Label(canvas.transform, "Score", "Score: 0", new Vector2(-430, 305), new Vector2(250, 55), 28);
        Text bestText = Label(canvas.transform, "Best", "Best: 0", new Vector2(430, 305), new Vector2(250, 55), 28);
        Label(canvas.transform, "Controls", "WASD / Arrows: Move    Space: Fire    Esc: Pause", new Vector2(0, -320), new Vector2(1050, 40), 22);
        GameObject managers = new GameObject("GameManager");
        ScoreManager score = managers.AddComponent<ScoreManager>();
        Assign(score, "scoreText", scoreText);
        Assign(score, "bestScoreText", bestText);

        GameObject grid = new GameObject("ArenaGrid", typeof(Grid));
        GameObject walls = new GameObject("Walls", typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D));
        walls.transform.SetParent(grid.transform, false);
        walls.layer = wallLayer;
        walls.GetComponent<TilemapRenderer>().sharedMaterial = spriteMaterial;
        Tilemap map = walls.GetComponent<Tilemap>();
        for (int x = -8; x <= 7; x++)
            for (int y = -5; y <= 4; y++)
                if (x == -8 || x == 7 || y == -5 || y == 4)
                    map.SetTile(new Vector3Int(x, y, 0), wallTile);

        GameObject bulletObject = SpriteObject("Bullet", square, Cyan, new Vector2(0.16f, 0.38f));
        bulletObject.layer = bulletLayer;
        bulletObject.tag = "PlayerBullet";
        Rigidbody2D bulletBody = bulletObject.AddComponent<Rigidbody2D>();
        bulletBody.gravityScale = 0;
        bulletBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        bulletBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        bulletObject.AddComponent<BoxCollider2D>().isTrigger = true;
        bulletObject.AddComponent<Bullet>();
        GameObject bulletPrefab = PrefabUtility.SaveAsPrefabAsset(bulletObject, Generated + "/Prefabs/Bullet.prefab");
        Object.DestroyImmediate(bulletObject);
        BulletPool pool = new GameObject("BulletPool").AddComponent<BulletPool>();
        Assign(pool, "bulletPrefab", bulletPrefab.GetComponent<Bullet>());

        GameObject player = SpriteObject("Player", square, Cyan, new Vector2(0.65f, 0.65f));
        player.transform.position = new Vector3(0, -2.8f, 0);
        player.layer = playerLayer;
        player.tag = "Player";
        Rigidbody2D playerBody = player.AddComponent<Rigidbody2D>();
        playerBody.gravityScale = 0;
        playerBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        playerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        player.AddComponent<BoxCollider2D>();
        Transform muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(player.transform, false);
        // Local scale is inherited; this places the muzzle 0.65 world units above the player.
        muzzle.localPosition = Vector3.up;
        PlayerController controller = player.AddComponent<PlayerController>();
        Assign(controller, "moveAction", move);
        Assign(controller, "fireAction", fire);
        Assign(controller, "bulletPool", pool);
        Assign(controller, "muzzle", muzzle);
        for (int i = 0; i < 3; i++)
        {
            GameObject target = SpriteObject("Target " + (i + 1), square, Pink, new Vector2(0.85f, 0.85f));
            target.transform.position = new Vector3(-4f + i * 4f, 2f, 0f);
            target.layer = enemyLayer;
            target.tag = "Enemy";
            target.GetComponent<SpriteRenderer>().sharedMaterial = pulse;
            target.AddComponent<BoxCollider2D>().isTrigger = true;
            EnemyTarget enemy = target.AddComponent<EnemyTarget>();
            Assign(enemy, "scoreManager", score);
            SerializedObject settings = new SerializedObject(enemy);
            settings.FindProperty("minimum").vector2Value = new Vector2(-5.5f + i * 4f, 0.8f);
            settings.FindProperty("maximum").vector2Value = new Vector2(-2.5f + i * 4f, 2.8f);
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
        GameObject panel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.02f, 0.025f, 0.08f, 0.94f);
        PauseManager pauseManager = managers.AddComponent<PauseManager>();
        Assign(pauseManager, "pauseAction", pause);
        Assign(pauseManager, "pauseMenuUI", panel);
        Label(panel.transform, "Title", "PAUSED", new Vector2(0, 120), new Vector2(550, 80), 44);
        MakeButton(panel.transform, "Resume", new Vector2(0, 20), pauseManager.Resume);
        MakeButton(panel.transform, "Quit to Menu", new Vector2(0, -65), pauseManager.QuitToMenu);
        panel.SetActive(false);
        EditorSceneManager.SaveScene(gameplay, Generated + "/Scenes/Gameplay.unity");

        Scene menu = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        Canvas menuCanvas = CreateCanvas();
        MainMenu menuManager = new GameObject("MenuManager").AddComponent<MainMenu>();
        Label(menuCanvas.transform, "Title", "NEON RANGE", new Vector2(0, 195), new Vector2(900, 100), 64);
        Label(menuCanvas.transform, "Description", "Move. Aim upward. Hit the pink targets. Beat your record.", new Vector2(0, 115), new Vector2(1050, 55), 24);
        Text menuBest = Label(menuCanvas.transform, "Best", "Best: 0", new Vector2(0, 60), new Vector2(400, 45), 28);
        MenuBestScore display = menuManager.gameObject.AddComponent<MenuBestScore>();
        Assign(display, "bestScoreText", menuBest);
        MakeButton(menuCanvas.transform, "Start", new Vector2(0, -35), menuManager.StartGame);
        MakeButton(menuCanvas.transform, "Quit Game", new Vector2(0, -120), menuManager.QuitGame);
        Label(menuCanvas.transform, "Controls", "WASD / Arrows: Move    Space: Fire    Esc: Pause", new Vector2(0, -230), new Vector2(1050, 50), 24);
        EditorSceneManager.SaveScene(menu, Generated + "/Scenes/MainMenu.unity");
        var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(Generated + "/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene(Generated + "/Scenes/Gameplay.unity", true)
        };
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            if (scene.path != sceneList[0].path && scene.path != sceneList[1].path) sceneList.Add(scene);
        EditorBuildSettings.scenes = sceneList.ToArray();
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Neon Range assembled", "Both scenes are saved. Main Menu is first in the shared Scene List. Read TESTING.md, press Play, and check your active Build Profile before exporting.", "OK");
    }

    private static void Assign(Object target, string property, Object value)
    {
        SerializedObject settings = new SerializedObject(target);
        settings.FindProperty(property).objectReferenceValue = value;
        settings.ApplyModifiedPropertiesWithoutUndo();
    }
    private static InputActionReference ActionReference(InputActionAsset input, string name)
    {
        InputActionReference reference = InputActionReference.Create(input.FindAction("Gameplay/" + name, true));
        AssetDatabase.CreateAsset(reference, Generated + "/Input/" + name + ".asset");
        return reference;
    }
    private static Sprite CreateSquare()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        texture.SetPixels(pixels);
        texture.Apply();
        string path = Generated + "/Art/Square.png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    private static GameObject SpriteObject(string name, Sprite sprite, Color color, Vector2 size)
    {
        GameObject result = new GameObject(name, typeof(SpriteRenderer));
        result.GetComponent<SpriteRenderer>().sprite = sprite;
        result.GetComponent<SpriteRenderer>().sharedMaterial = spriteMaterial;
        result.GetComponent<SpriteRenderer>().color = color;
        result.transform.localScale = new Vector3(size.x, size.y, 1);
        return result;
    }
    private static void CreateCamera()
    {
        Camera camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 0, -10);
        camera.orthographic = true;
        camera.orthographicSize = 6;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.025f, 0.03f, 0.065f);
    }
    private static Canvas CreateCanvas()
    {
        Canvas canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        GameObject events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        return canvas;
    }
    private static Text Label(Transform parent, string name, string content, Vector2 position, Vector2 size, int fontSize)
    {
        Text text = new GameObject(name, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        text.transform.SetParent(parent, false);
        text.rectTransform.sizeDelta = size;
        text.rectTransform.anchoredPosition = position;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = content;
        text.raycastTarget = false;
        return text;
    }
    private static void MakeButton(Transform parent, string title, Vector2 position, UnityAction callback)
    {
        GameObject obj = new GameObject(title + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(320, 64);
        rect.anchoredPosition = position;
        obj.GetComponent<Image>().color = new Color(0.09f, 0.33f, 0.43f);
        Button button = obj.GetComponent<Button>();
        button.targetGraphic = obj.GetComponent<Image>();
        UnityEventTools.AddPersistentListener(button.onClick, callback);
        Label(obj.transform, "Label", title, Vector2.zero, new Vector2(310, 60), 28);
    }
    private static void EnsureTag(string tag)
    {
        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = manager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++) if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        int index = tags.arraySize;
        tags.InsertArrayElementAtIndex(index);
        tags.GetArrayElementAtIndex(index).stringValue = tag;
        manager.ApplyModifiedPropertiesWithoutUndo();
    }
    private static int EnsureLayer(string name)
    {
        int existing = LayerMask.NameToLayer(name);
        if (existing >= 0) return existing;
        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = manager.FindProperty("layers");
        for (int i = 8; i < 32; i++)
        {
            if (!string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)) continue;
            layers.GetArrayElementAtIndex(i).stringValue = name;
            manager.ApplyModifiedPropertiesWithoutUndo();
            return i;
        }
        throw new System.InvalidOperationException("No free layer slot for " + name);
    }
}
