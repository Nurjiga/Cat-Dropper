using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Дизайн кнопок и экрана Game Over по макету Figma (кадр 390x844). Размеры, шрифты и цвета — как в макете:
///   Главная:   Play 144x84 (жирный 48), Collection / Settings 144x49 (обычный 24), скругление 20
///   Game Over: фон #FFE5BB, кот, "Game over" (жирный 40), карточка #FFC067 со Score / 1200 / Best score,
///              кнопки "Play again" и "Main menu" 144x55, скругление 10, жирный 24
///   Пауза:     в макете нет — кнопки как на Game Over (тот же размер, шрифт и скругление)
/// Primary — золотая #F6C47D, Secondary — кремовая #FFF8EF.
/// Меню: Tools > Cat Dropper > Apply Button Style. Можно запускать повторно.
/// </summary>
public static class ApplyButtonStyle
{
    private const float FramePtW = 390f, FramePtH = 844f;
    private const string SpriteFolder = "Assets/Sprites/Buttons/";
    private const string CatPath = "Assets/Sprites/GameOver/cat_gameover.png";
    private const string RegularFontGuid = "3f4fa28a6766c4dcb8d0b43a9fe3ac5f";                  // Baloo Tamma 2 (обычный)
    private const string HeavyFontPath = "Assets/Fonts/BalooPaaji2-ExtraBold SDF.asset";        // жирный (в макете Baloo Tamma / Tammudu / Paaji)

    private static readonly Color Black = Color.black;
    private static readonly Color TitleColor = new Color(0.196f, 0.180f, 0.149f, 1f);          // #322E26
    private static readonly Color PageColor = new Color(1f, 0.898f, 0.733f, 1f);               // #FFE5BB

    // Эталонный телефон 1284 x 2778 (iPhone 12 Pro Max)
    private const float RefW = 1284f, RefH = 2778f;

    private struct Look { public float w, h, radius, font; public bool heavy; }
    private static readonly Look HomeHero = new Look { w = 144f, h = 84f, radius = 20f, font = 48f, heavy = true };
    private static readonly Look HomeSmall = new Look { w = 144f, h = 49f, radius = 20f, font = 24f, heavy = false };
    private static readonly Look Overlay = new Look { w = 144f, h = 55f, radius = 10f, font = 24f, heavy = true };   // Game Over и пауза

    [MenuItem("Tools/Cat Dropper/Apply Button Style")]
    public static void Run()
    {
        ImportSprites();

        // ---------------- главное меню ----------------
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Main Menu.unity", OpenSceneMode.Single);
        Transform cv = RootCanvas(scene).transform;
        float menuScale = Mathf.Sqrt((RefW / 1080f) * (RefH / 1920f));
        float u = (RefW / menuScale) / FramePtW;                  // единиц холста на 1 пункт макета
        // верх кнопок в макете: Play 500, Collection 595, Settings 651
        Style(cv.Find("Play"), true, "Play", HomeHero, 500f, u);
        Style(cv.Find("Collection"), false, "Collection", HomeSmall, 595f, u);
        Style(cv.Find("Settings"), false, "Settings", HomeSmall, 651f, u);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);

        // ---------------- игра: пауза и Game Over ----------------
        scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
        cv = RootCanvas(scene).transform;
        u = RefW / FramePtW;                                       // холст игры: ширина всегда 1284 единиц

        // Пауза: три кнопки через тот же шаг, что на Game Over (629 -> 702)
        float pitch = 73f, pauseTop = 489f;
        Style(cv.Find("PausePanel/Resume"), true, "Resume", Overlay, pauseTop, u);
        Style(cv.Find("PausePanel/Main Menu"), false, "Main menu", Overlay, pauseTop + pitch, u);
        Style(cv.Find("PausePanel/Help"), false, "Help", Overlay, pauseTop + 2 * pitch, u);

        BuildGameOver(cv, scene, u);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);

        AssetDatabase.SaveAssets();
        Debug.Log("ApplyButtonStyle: DONE");
    }

    // ---------------- экран Game Over по макету ----------------
    private static void BuildGameOver(Transform cv, UnityEngine.SceneManagement.Scene scene, float u)
    {
        Transform panel = cv.Find("Panel");
        if (panel == null) throw new System.Exception("ApplyButtonStyle: Panel (Game Over) не найден");

        GameManager gm = null;
        foreach (var m in Resources.FindObjectsOfTypeAll<GameManager>()) if (m.gameObject.scene == scene) gm = m;

        // фон: сплошной #FFE5BB вместо картинки
        Image bg = panel.GetComponent<Image>();
        bg.sprite = null; bg.type = Image.Type.Simple; bg.color = PageColor;

        // убрать прошлую сборку
        foreach (string n in new[] { "Cat", "ScoreCard", "ScoreLabel", "BestLabel" })
        {
            Transform old = panel.Find(n);
            if (old != null) Object.DestroyImmediate(old.gameObject);
        }

        // кот
        Image cat = NewImage(panel, "Cat", LoadSprite(CatPath), Image.Type.Simple, Color.white);
        cat.preserveAspect = true; cat.raycastTarget = false;
        PlaceCenter(cat.rectTransform, 172.5f, 200f, 261f, 136f, u);
        cat.transform.SetSiblingIndex(0);

        // жёлтая карточка #FFC067, скругление 20, 306x265
        Image card = NewImage(panel, "ScoreCard", LoadSprite(SpriteFolder + "card_gold.png"), Image.Type.Sliced, Color.white);   // цвет #FFC067 уже в самом спрайте
        card.raycastTarget = false; card.pixelsPerUnitMultiplier = 64f / (20f * u);
        PlaceCenter(card.rectTransform, 195f, 479.5f, 306f, 265f, u);
        card.transform.SetSiblingIndex(1);

        TMP_FontAsset regular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(RegularFontGuid));
        TMP_FontAsset heavy = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(HeavyFontPath);

        // заголовок "Game over"
        TextMeshProUGUI title = panel.Find("Game Over").GetComponent<TextMeshProUGUI>();
        FormatText(title, "Game over", heavy, 40f * u, TitleColor);
        PlaceCenter(title.rectTransform, 195f, 312f, 196f, 70f, u);

        // Score / число / Best score / рекорд
        TextMeshProUGUI scoreLabel = NewText(panel, "ScoreLabel", "Score", regular, 32f * u, Black);
        PlaceCenter(scoreLabel.rectTransform, 195f, 386f, 120f, 54f, u);
        FormatText(gm.finalScoreText, gm.finalScoreText.text, heavy, 48f * u, Black);
        PlaceCenter(gm.finalScoreText.rectTransform, 194.5f, 435.5f, 200f, 83f, u);
        TextMeshProUGUI bestLabel = NewText(panel, "BestLabel", "Best score", regular, 24f * u, Black);
        PlaceCenter(bestLabel.rectTransform, 194.5f, 498f, 200f, 42f, u);
        FormatText(gm.bestScoreText, "2000", regular, 24f * u, Black);
        PlaceCenter(gm.bestScoreText.rectTransform, 195f, 521f, 200f, 30f, u);
        gm.bestLabelText = bestLabel;

        // кнопки 144x55: верх 629 и 702
        Style(panel.Find("Restart"), true, "Play again", Overlay, 629f, u);
        Style(panel.Find("Main Menu"), false, "Main menu", Overlay, 702f, u);

        // порядок: фон -> кот, карточка -> тексты -> кнопки
        panel.Find("Restart").SetAsLastSibling();
        panel.Find("Main Menu").SetAsLastSibling();

        if (gm.finalScoreText != null) EditorUtility.SetDirty(gm.finalScoreText);
        EditorUtility.SetDirty(gm);
    }

    // ---------------- кнопка ----------------
    private static void Style(Transform t, bool primary, string label, Look look, float topPt, float u)
    {
        if (t == null) throw new System.Exception("ApplyButtonStyle: кнопка не найдена (" + label + ")");

        RectTransform rt = (RectTransform)t;
        PlaceCenter(rt, 195f, topPt + look.h * 0.5f, look.w, look.h, u);

        Image img = t.GetComponent<Image>();
        img.sprite = LoadSprite(SpriteFolder + (primary ? "btn_primary.png" : "btn_secondary.png"));
        img.type = Image.Type.Sliced;
        img.color = Color.white;
        img.raycastTarget = true;
        img.pixelsPerUnitMultiplier = 64f / (look.radius * u);          // углы спрайта 64 px -> radius пунктов макета

        Button b = t.GetComponent<Button>();
        b.targetGraphic = img;
        b.transition = Selectable.Transition.ColorTint;
        var colors = b.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.selectedColor = Color.white;
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.disabledColor = new Color(1f, 1f, 1f, 0.5f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        b.colors = colors;
        var nav = b.navigation; nav.mode = Navigation.Mode.None; b.navigation = nav;
        EditorUtility.SetDirty(b);

        TextMeshProUGUI tmp = t.GetComponentInChildren<TextMeshProUGUI>(true);
        RectTransform trt = (RectTransform)tmp.transform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = trt.offsetMax = Vector2.zero; trt.localScale = Vector3.one;
        TMP_FontAsset font = look.heavy
            ? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(HeavyFontPath)
            : AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(RegularFontGuid));
        FormatText(tmp, label, font, look.font * u, Black);
        EditorUtility.SetDirty(tmp); EditorUtility.SetDirty(img); EditorUtility.SetDirty(rt);
    }

    // ---------------- помощники ----------------
    /// <summary>Центр (cx, cy) и размер (w, h) в пунктах макета, отсчёт от верхнего левого угла кадра 390x844.</summary>
    private static void PlaceCenter(RectTransform rt, float cx, float cy, float w, float h, float u)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;
        rt.sizeDelta = new Vector2(w * u, h * u);
        rt.anchoredPosition = new Vector2((cx - FramePtW * 0.5f) * u, -(cy - FramePtH * 0.5f) * u);
    }

    private static void FormatText(TextMeshProUGUI tmp, string text, TMP_FontAsset font, float sizePx, Color color)
    {
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = sizePx;
        tmp.enableAutoSizing = false;
        tmp.fontStyle = FontStyles.Normal;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        tmp.lineSpacing = 0f;
        tmp.margin = Vector4.zero;          // у старых текстов были внутренние поля — они сдвигали текст от центра
    }

    private static Image NewImage(Transform parent, string name, Sprite sprite, Image.Type type, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");
        Image img = go.AddComponent<Image>();
        img.sprite = sprite; img.type = type; img.color = color;
        return img;
    }

    private static TextMeshProUGUI NewText(Transform parent, string name, string text, TMP_FontAsset font, float sizePx, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        FormatText(tmp, text, font, sizePx, color);
        return tmp;
    }

    private static void ImportSprites()
    {
        AssetDatabase.Refresh();
        foreach (string file in Directory.GetFiles(SpriteFolder, "*.png"))
        {
            string path = file.Replace('\\', '/');
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) continue;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 100f;
            imp.spriteBorder = new Vector4(64f, 64f, 64f, 64f);
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();
        }
        var cat = AssetImporter.GetAtPath(CatPath) as TextureImporter;
        if (cat != null)
        {
            cat.textureType = TextureImporterType.Sprite;
            cat.spriteImportMode = SpriteImportMode.Single;
            cat.spritePixelsPerUnit = 100f;
            cat.alphaIsTransparency = true;
            cat.mipmapEnabled = false;
            cat.filterMode = FilterMode.Bilinear;
            cat.maxTextureSize = 2048;
            cat.textureCompression = TextureImporterCompression.Uncompressed;
            cat.SaveAndReimport();
        }
    }

    private static Canvas RootCanvas(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (var c in Resources.FindObjectsOfTypeAll<Canvas>())
            if (c.gameObject.scene == scene && c.isRootCanvas) return c;
        throw new System.Exception("ApplyButtonStyle: Canvas not found");
    }

    private static Sprite LoadSprite(string path)
    {
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path)) if (o is Sprite s) return s;
        throw new System.Exception("ApplyButtonStyle: sprite not found " + path);
    }
}
