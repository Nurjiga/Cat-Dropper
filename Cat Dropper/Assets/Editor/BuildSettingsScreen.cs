using System.IO;
using UnityEngine.TextCore.LowLevel;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Собирает экран Settings (макет из Figma, кадр 390x844) и окно "How to play" в сцене Main Menu.
/// Меню: Tools > Cat Dropper > Build Settings Screen.
/// Можно запускать повторно: старые SettingsPanel и MenuTutorialPanel пересоздаются.
/// </summary>
public static class BuildSettingsScreen
{
    private const string MenuScenePath = "Assets/Scenes/Main Menu.unity";
    private const string SpriteFolder = "Assets/Sprites/Settings";

    // Макет в Figma — 390x844 пунктов. Внутри кадра 1 пункт = U единиц.
    private const float U = 3f;
    private const float FrameW = 390f * U;
    private const float FrameH = 844f * U;

    private const string TitleFontGuid = "bdacd581acd5943db9f0792d29b92433"; // cattie (как у "Cat collection")
    private const string LabelFontGuid = "0ac34c51ba99c46d092f495a33468277"; // Baloo Paaji 2 (как у названий котов)

    private const string BoldFontTtf = "Assets/Fonts/BalooPaaji2-ExtraBold.ttf";
    private const string BoldFontAsset = "Assets/Fonts/BalooPaaji2-ExtraBold SDF.asset";

    private static readonly Color Ink = new Color(0.1f, 0.08f, 0.06f, 1f);
    private static readonly Color DividerColor = new Color(0.78f, 0.67f, 0.5f, 1f);

    [MenuItem("Tools/Cat Dropper/Build Settings Screen")]
    public static void Run()
    {
        ImportSprites();
        var scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);

        MainMenuScript menu = null;
        foreach (MainMenuScript m in Resources.FindObjectsOfTypeAll<MainMenuScript>())
            if (m.gameObject.scene == scene) menu = m;
        if (menu == null) throw new System.Exception("BuildSettingsScreen: MainMenuScript not found");

        Canvas canvas = null;
        foreach (Canvas c in Resources.FindObjectsOfTypeAll<Canvas>())
            if (c.gameObject.scene == scene && c.isRootCanvas) canvas = c;
        if (canvas == null) throw new System.Exception("BuildSettingsScreen: Canvas not found");
        Transform cv = canvas.transform;

        // убираем прошлую сборку
        DestroyChild(cv, "SettingsPanel");

        // шаблон для кнопки "назад" — кнопка из Collections (тот же вид)
        Transform backTemplate = cv.Find("CollectionsPanel/BackButton");
        if (backTemplate == null) throw new System.Exception("BuildSettingsScreen: CollectionsPanel/BackButton not found");

        TMP_FontAsset titleFont = LoadFont(TitleFontGuid);
        TMP_FontAsset labelFont = EnsureBoldFont() ?? LoadFont(LabelFontGuid);

        Sprite background = LoadSprite("Assets/Sprites/back 2.png");
        Sprite card = LoadSprite(SpriteFolder + "/settings_card.png");
        Sprite pillSprite = LoadSprite(SpriteFolder + "/settings_pill.png");
        Sprite knobSprite = LoadSprite(SpriteFolder + "/settings_knob.png");
        Sprite helpSprite = LoadSprite(SpriteFolder + "/settings_help.png");
        Sprite speakerSprite = LoadSprite(SpriteFolder + "/settings_speaker.png");

        // ---------------- Панель Settings ----------------
        GameObject panelGo = NewUI("SettingsPanel", cv);
        RectTransform panel = (RectTransform)panelGo.transform;
        Stretch(panel);
        Image bg = panelGo.AddComponent<Image>();
        bg.sprite = background;
        bg.color = Color.white;

        // Кадр 390x844 внутри панели: сам подгоняется под экран (FitToCanvas)
        GameObject frameGo = NewUI("Frame", panel);
        RectTransform frame = (RectTransform)frameGo.transform;
        frame.anchorMin = frame.anchorMax = new Vector2(0.5f, 0.5f);
        frame.pivot = new Vector2(0.5f, 0.5f);
        frame.sizeDelta = new Vector2(FrameW, FrameH);
        frame.anchoredPosition = Vector2.zero;
        FitToCanvas fit = panelGo.AddComponent<FitToCanvas>();
        fit.frame = frame;

        // Заголовок
        TextMeshProUGUI title = MakeText(frame, "Title", "Settings", titleFont, 120f, Ink, TextAlignmentOptions.Center, FontStyles.Normal);
        Place(title.rectTransform, 195f, 100f, 300f, 80f);

        // Кнопка "назад"
        GameObject back = Object.Instantiate(backTemplate.gameObject, frame);
        back.name = "BackButton";
        RectTransform backRt = (RectTransform)back.transform;
        backRt.localScale = Vector3.one;
        Place(backRt, 42f, 97f, 50f, 41f);
        Button backBtn = back.GetComponent<Button>();
        ClearListeners(backBtn);
        UnityEventTools.AddPersistentListener(backBtn.onClick, menu.CloseSettings);

        // Карточка
        Image cardImg = MakeImage(frame, "Card", card, Image.Type.Sliced, Color.white);
        Place(cardImg.rectTransform, 192.5f, 388.5f, 333f, 501f);
        cardImg.raycastTarget = false;

        // Строки
        Button sfxOn, sfxOff, musicOn, musicOff;
        RectTransform sfxKnob, musicKnob;
        Image sfxIcon, musicIcon;
        BuildToggleRow(frame, "SFX", 287f, labelFont, speakerSprite, pillSprite, knobSprite,
            out sfxOn, out sfxOff, out sfxKnob, out sfxIcon);
        BuildToggleRow(frame, "Music", 344f, labelFont, speakerSprite, pillSprite, knobSprite,
            out musicOn, out musicOff, out musicKnob, out musicIcon);

        // Разделители между строками
        MakeDivider(frame, "Divider1", 315.5f);
        MakeDivider(frame, "Divider2", 372.5f);

        // Help
        TextMeshProUGUI helpLabel = MakeText(frame, "HelpLabel", "Help", labelFont, 66f, Ink, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        PlaceLeft(helpLabel.rectTransform, 62f, 401f, 140f, 44f);

        Image helpImg = MakeImage(frame, "HelpButton", helpSprite, Image.Type.Simple, Color.white);
        Place(helpImg.rectTransform, 297f, 403f, 31f, 31f);
        helpImg.raycastTarget = false;
        TextMeshProUGUI q = MakeText(helpImg.transform, "Q", "?", labelFont, 60f, Color.white, TextAlignmentOptions.Center, FontStyles.Normal);
        Stretch(q.rectTransform);

        // кнопку удобнее нажимать, чем она выглядит: невидимая зона побольше
        Image helpHit = MakeImage(frame, "HelpHitArea", null, Image.Type.Simple, new Color(1f, 1f, 1f, 0f));
        Place(helpHit.rectTransform, 297f, 403f, 60f, 60f);
        Button helpBtn = helpHit.gameObject.AddComponent<Button>();
        helpBtn.targetGraphic = helpHit;
        helpBtn.transition = Selectable.Transition.None;

        // Компонент панели
        SettingsPanel sp = panelGo.AddComponent<SettingsPanel>();
        sp.sfxOnButton = sfxOn; sp.sfxOffButton = sfxOff; sp.sfxKnob = sfxKnob; sp.sfxIcon = sfxIcon;
        sp.musicOnButton = musicOn; sp.musicOffButton = musicOff; sp.musicKnob = musicKnob; sp.musicIcon = musicIcon;
        sp.helpButton = helpBtn; sp.menu = menu;
        fit.Apply();
        sp.ShowState(true, true, true);

        // ---------------- Связки ----------------
        menu.settingsPanel = panelGo;

        Transform settingsButton = cv.Find("Settings");
        if (settingsButton != null && settingsButton.GetComponent<Button>() != null)
        {
            Button b = settingsButton.GetComponent<Button>();
            ClearListeners(b);
            UnityEventTools.AddPersistentListener(b.onClick, menu.OpenSettings);
        }
        else Debug.LogWarning("BuildSettingsScreen: кнопка Settings в главном меню не найдена");

        // порядок: Settings над Collections, окно инструкции — самым верхним
        panel.SetAsLastSibling();
        Transform tutorialPanel = cv.Find("MenuTutorialPanel");
        if (tutorialPanel != null) tutorialPanel.SetAsLastSibling(); // окно инструкции — поверх всего

        panelGo.SetActive(false);

        EditorUtility.SetDirty(menu);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("BuildSettingsScreen: DONE");
    }

    // ---------------- Предпросмотр (для проверки без запуска игры) ----------------

    /// <summary>Рендерит панели в PNG-файлы. Папка берётся из переменной окружения CD_PREVIEW_DIR.</summary>
    public static void RenderPreview()
    {
        string dir = System.Environment.GetEnvironmentVariable("CD_PREVIEW_DIR");
        if (string.IsNullOrEmpty(dir)) dir = Path.Combine(Application.dataPath, "..", "Preview");
        Directory.CreateDirectory(dir);

        var scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);

        Canvas canvas = null;
        foreach (Canvas c in Resources.FindObjectsOfTypeAll<Canvas>())
            if (c.gameObject.scene == scene && c.isRootCanvas) canvas = c;
        Camera cam = null;
        foreach (Camera c in Resources.FindObjectsOfTypeAll<Camera>())
            if (c.gameObject.scene == scene) cam = c;

        const int W = 642, H = 1389; // iPhone 12 Pro Max / 2
        var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null) scaler.enabled = false;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 10f;
        // CanvasScaler: ScaleWithScreenSize 1080x1920, match 0.5 -> sqrt((W/1080)*(H/1920))
        canvas.scaleFactor = Mathf.Sqrt((W / 1080f) * (H / 1920f));

        Transform cv = canvas.transform;
        GameObject panel = cv.Find("SettingsPanel").gameObject;
        Transform tutT = cv.Find("MenuTutorialPanel");
        GameObject tut = tutT != null ? tutT.gameObject : null;
        SettingsPanel sp = panel.GetComponent<SettingsPanel>();

        Shoot(cam, rt, Path.Combine(dir, "menu.png"));

        panel.SetActive(true);
        sp.ShowState(true, true, true);
        Shoot(cam, rt, Path.Combine(dir, "settings_on.png"));

        sp.ShowState(false, true, true);
        Shoot(cam, rt, Path.Combine(dir, "settings_sfx_off.png"));
        sp.ShowState(true, true, true);

        if (tut != null) { tut.SetActive(true); Shoot(cam, rt, Path.Combine(dir, "tutorial.png")); }
        Debug.Log("BuildSettingsScreen: preview written to " + dir);
    }

    private static void Shoot(Camera cam, RenderTexture rt, string path)
    {
        Canvas.ForceUpdateCanvases();
        foreach (var fit in Resources.FindObjectsOfTypeAll<FitToCanvas>()) fit.Apply();
        Canvas.ForceUpdateCanvases();

        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    // ---------------- Строка с переключателем ----------------

    private static void BuildToggleRow(RectTransform frame, string label, float y, TMP_FontAsset font,
        Sprite speaker, Sprite pillSprite, Sprite knobSprite,
        out Button onBtn, out Button offBtn, out RectTransform knob, out Image icon)
    {
        TextMeshProUGUI text = MakeText(frame, label + "Label", label, font, 66f, Ink, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        PlaceLeft(text.rectTransform, 62f, y - 1f, 140f, 44f);

        icon = MakeImage(frame, label + "Icon", speaker, Image.Type.Simple, Color.white);
        Place(icon.rectTransform, 160f, y - 1f, 28f, 28f);
        icon.raycastTarget = false;

        const float pillW = 85f, pillH = 38.5f, knobW = 46f;

        Image pill = MakeImage(frame, label + "Toggle", pillSprite, Image.Type.Sliced, Color.white);
        Place(pill.rectTransform, 300.5f, y, pillW, pillH);
        pill.raycastTarget = false;

        // ползунок: anchor слева по центру, двигает SettingsPanel
        Image knobImg = MakeImage(pill.transform, "Knob", knobSprite, Image.Type.Sliced, Color.white);
        knob = knobImg.rectTransform;
        knob.anchorMin = knob.anchorMax = new Vector2(0f, 0.5f);
        knob.pivot = new Vector2(0.5f, 0.5f);
        knob.sizeDelta = new Vector2(knobW * U, pillH * U);
        knob.anchoredPosition = new Vector2(knobW * U * 0.5f, 0f);
        knobImg.raycastTarget = false;

        TextMeshProUGUI on = MakeText(pill.transform, "On", "On", font, 54f, Ink, TextAlignmentOptions.Center, FontStyles.Normal);
        TextMeshProUGUI off = MakeText(pill.transform, "Off", "Off", font, 54f, Ink, TextAlignmentOptions.Center, FontStyles.Normal);
        SetInPill(on.rectTransform, pillW, pillH, 0.5f * knobW);
        SetInPill(off.rectTransform, pillW, pillH, pillW - 0.5f * knobW);
        on.transform.SetAsLastSibling();
        off.transform.SetAsLastSibling();

        // две невидимые зоны нажатия: левая половина — On, правая — Off
        onBtn = MakeHitZone(pill.transform, "OnButton", 0f, 0.5f);
        offBtn = MakeHitZone(pill.transform, "OffButton", 0.5f, 1f);
    }

    private static void MakeDivider(Transform parent, string name, float y)
    {
        Image line = MakeImage(parent, name, null, Image.Type.Simple, DividerColor);
        Place(line.rectTransform, 192.5f, y, 273f, 2.5f);
        line.raycastTarget = false;
    }

    private static void SetInPill(RectTransform rt, float pillW, float pillH, float centerX)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(46f * U, pillH * U);
        rt.anchoredPosition = new Vector2(centerX * U, 0f);
    }

    private static Button MakeHitZone(Transform parent, string name, float xMin, float xMax)
    {
        Image img = MakeImage(parent, name, null, Image.Type.Simple, new Color(1f, 1f, 1f, 0f));
        RectTransform rt = img.rectTransform;
        rt.anchorMin = new Vector2(xMin, 0f);
        rt.anchorMax = new Vector2(xMax, 1f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Button b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.transition = Selectable.Transition.None;
        return b;
    }

    // ---------------- Помощники ----------------

    private static GameObject NewUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");
        return go;
    }

    private static Image MakeImage(Transform parent, string name, Sprite sprite, Image.Type type, Color color)
    {
        GameObject go = NewUI(name, parent);
        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.type = type;
        img.color = color;
        img.raycastTarget = true;
        return img;
    }

    private static TextMeshProUGUI MakeText(Transform parent, string name, string text, TMP_FontAsset font,
        float size, Color color, TextAlignmentOptions align, FontStyles style)
    {
        GameObject go = NewUI(name, parent);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.fontStyle = style;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        t.raycastTarget = false;
        return t;
    }

    /// <summary>Центр элемента (cx, cy) и размер (w, h) в пунктах макета, отсчёт от левого верхнего угла кадра.</summary>
    private static void Place(RectTransform rt, float cx, float cy, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w * U, h * U);
        rt.anchoredPosition = new Vector2(cx * U, -cy * U);
    }

    /// <summary>Как Place, но x — левый край элемента.</summary>
    private static void PlaceLeft(RectTransform rt, float left, float cy, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(w * U, h * U);
        rt.anchoredPosition = new Vector2(left * U, -cy * U);
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    private static void ClearListeners(Button b)
    {
        while (b.onClick.GetPersistentEventCount() > 0)
            UnityEventTools.RemovePersistentListener(b.onClick, 0);
    }

    private static void DestroyChild(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject);
    }

    private static TMP_FontAsset LoadFont(string guid)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (font == null) Debug.LogWarning("BuildSettingsScreen: font not found " + guid);
        return font;
    }

    /// <summary>Создаёт TMP-шрифт из жирного (ExtraBold) начертания Baloo Paaji 2, если его ещё нет.</summary>
    private static TMP_FontAsset EnsureBoldFont()
    {
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontAsset);
        if (existing != null) return existing;

        AssetDatabase.ImportAsset(BoldFontTtf, ImportAssetOptions.ForceSynchronousImport);
        Font font = AssetDatabase.LoadAssetAtPath<Font>(BoldFontTtf);
        if (font == null)
        {
            Debug.LogWarning("BuildSettingsScreen: " + BoldFontTtf + " не найден, беру обычный шрифт");
            return null;
        }

        TMP_FontAsset fa = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
            AtlasPopulationMode.Dynamic, true);
        fa.name = "BalooPaaji2-ExtraBold SDF";
        AssetDatabase.CreateAsset(fa, BoldFontAsset);
        fa.material.name = "BalooPaaji2-ExtraBold SDF Material";
        AssetDatabase.AddObjectToAsset(fa.material, fa);
        for (int i = 0; i < fa.atlasTextures.Length; i++)
        {
            fa.atlasTextures[i].name = "BalooPaaji2-ExtraBold SDF Atlas " + i;
            AssetDatabase.AddObjectToAsset(fa.atlasTextures[i], fa);
        }
        fa.TryAddCharacters("SFXMusicHelpOnf?abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,!:-");
        EditorUtility.SetDirty(fa);
        AssetDatabase.SaveAssets();
        Debug.Log("BuildSettingsScreen: created bold font " + BoldFontAsset);
        return fa;
    }

    private static Sprite LoadSprite(string path)
    {
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
            if (o is Sprite s) return s;
        Debug.LogWarning("BuildSettingsScreen: sprite not found " + path);
        return null;
    }

    private static void ImportSprites()
    {
        AssetDatabase.Refresh();
        foreach (string file in Directory.GetFiles(SpriteFolder, "*.png"))
        {
            string path = file.Replace('\\', '/');
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            string n = Path.GetFileNameWithoutExtension(path);
            if (n == "settings_card") importer.spriteBorder = new Vector4(128f, 128f, 128f, 128f);
            else if (n == "settings_pill" || n == "settings_knob") importer.spriteBorder = new Vector4(64f, 64f, 64f, 64f);
            else importer.spriteBorder = Vector4.zero;

            importer.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }
}
