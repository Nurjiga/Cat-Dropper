using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Собирает инструкцию из двух окон подряд (шаги -> Legendary-кот) в играх и в меню.
/// Меню: Tools > Cat Dropper > Build Tutorial Windows. Можно запускать повторно: старые окна пересоздаются.
/// </summary>
public static class BuildTutorialWindows
{
    private const string GameScene = "Assets/Scenes/SampleScene.unity";
    private const string MenuScene = "Assets/Scenes/Main Menu.unity";
    private const string Folder = "Assets/Sprites/instruc/";

    [MenuItem("Tools/Cat Dropper/Build Tutorial Windows")]
    public static void Run()
    {
        ImportNewSprites();

        // ---- игра ----
        var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
        Canvas canvas = RootCanvas(scene);
        GameManager gm = null;
        foreach (var m in Resources.FindObjectsOfTypeAll<GameManager>()) if (m.gameObject.scene == scene) gm = m;
        Destroy(canvas.transform, "TutorialPanel");
        TutorialModal modal = Build(canvas.transform, "TutorialPanel");
        gm.tutorial = modal;
        EditorUtility.SetDirty(gm);
        // в игре окно показывается само при первом запуске (TutorialModal.Awake решает по TutorialSeen)
        modal.gameObject.SetActive(true);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);

        // ---- меню ----
        scene = EditorSceneManager.OpenScene(MenuScene, OpenSceneMode.Single);
        canvas = RootCanvas(scene);
        MainMenuScript menu = null;
        foreach (var m in Resources.FindObjectsOfTypeAll<MainMenuScript>()) if (m.gameObject.scene == scene) menu = m;
        Destroy(canvas.transform, "MenuTutorialPanel");
        modal = Build(canvas.transform, "MenuTutorialPanel");
        menu.tutorial = modal;
        EditorUtility.SetDirty(menu);
        modal.gameObject.SetActive(false);   // в меню открывается только по Help
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("BuildTutorialWindows: DONE");
    }

    private static TutorialModal Build(Transform canvas, string name)
    {
        GameObject root = new GameObject(name, typeof(RectTransform));
        root.transform.SetParent(canvas, false);
        root.layer = LayerMask.NameToLayer("UI");
        Stretch((RectTransform)root.transform);
        TutorialModal modal = root.AddComponent<TutorialModal>();

        // окно 1: шаги (кнопка Next). Кнопка по центру на высоте 1160/1688 картинки
        Button next1;
        GameObject p1 = Page(root.transform, "Page1_Steps", Folder + "Steps.png", Folder + "ButtonNext.png", 0.287f, 0.339f, out next1);
        // окно 2: Legendary-кот (кнопка Got it!). Кнопка на высоте 1087/1688 картинки
        Button next2;
        GameObject p2 = Page(root.transform, "Page2_Legendary", Folder + "Group 3.png", Folder + "Button.png", 0.330f, 0.382f, out next2);

        modal.pages = new[] { p1, p2 };
        modal.nextButtons = new[] { next1, next2 };
        root.transform.SetAsLastSibling();
        p2.SetActive(false);
        return modal;
    }

    private static GameObject Page(Transform parent, string name, string bgPath, string btnPath, float yMin, float yMax, out Button button)
    {
        GameObject page = new GameObject(name, typeof(RectTransform));
        page.transform.SetParent(parent, false);
        page.layer = LayerMask.NameToLayer("UI");
        Stretch((RectTransform)page.transform);
        Image bg = page.AddComponent<Image>();
        bg.sprite = LoadSprite(bgPath);
        bg.color = Color.white;
        bg.raycastTarget = true;   // блокирует нажатия по игре под окном

        GameObject b = new GameObject("Button", typeof(RectTransform));
        b.transform.SetParent(page.transform, false);
        b.layer = LayerMask.NameToLayer("UI");
        RectTransform rt = (RectTransform)b.transform;
        rt.anchorMin = new Vector2(0.276f, yMin);
        rt.anchorMax = new Vector2(0.724f, yMax);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image img = b.AddComponent<Image>();
        img.sprite = LoadSprite(btnPath);
        button = b.AddComponent<Button>();
        button.targetGraphic = img;
        return page;
    }

    private static void ImportNewSprites()
    {
        AssetDatabase.Refresh();
        foreach (string file in new[] { "Steps.png", "ButtonNext.png" })
        {
            string path = Folder + file;
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) continue;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 100f;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.maxTextureSize = 4096;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();
        }
    }

    private static Canvas RootCanvas(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (var c in Resources.FindObjectsOfTypeAll<Canvas>())
            if (c.gameObject.scene == scene && c.isRootCanvas) return c;
        throw new System.Exception("BuildTutorialWindows: Canvas not found");
    }

    private static void Destroy(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject);
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero; rt.localScale = Vector3.one;
    }

    private static Sprite LoadSprite(string path)
    {
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path)) if (o is Sprite s) return s;
        Debug.LogWarning("BuildTutorialWindows: sprite not found " + path);
        return null;
    }
}
