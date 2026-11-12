using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Одноразовая настройка игровой сцены: вешает TutorialModal, добавляет кнопку Help в паузу
/// и текст рекорда на экран Game Over. Можно запускать повторно — дубли не создаются.
/// Меню: Tools > Cat Dropper > Setup Game Scene
/// </summary>
public static class CatDropperSetup
{
    private const string GameScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Cat Dropper/Setup Game Scene")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

        GameManager gm = null;
        foreach (GameManager candidate in Resources.FindObjectsOfTypeAll<GameManager>())
            if (candidate.gameObject.scene == scene) gm = candidate;
        if (gm == null) throw new System.Exception("CatDropperSetup: GameManager not found in " + GameScenePath);

        Transform canvas = gm.mainGameUI.transform.parent;

        // --- Help button in the pause panel ---
        if (gm.pausePanel != null && gm.pausePanel.transform.Find("Help") == null)
        {
            Transform pause = gm.pausePanel.transform;
            RectTransform resume = pause.Find("Resume") as RectTransform;
            RectTransform menu = pause.Find("Main Menu") as RectTransform;
            if (resume != null && menu != null)
            {
                GameObject help = Object.Instantiate(menu.gameObject, pause);
                help.name = "Help";
                RectTransform rt = (RectTransform)help.transform;
                rt.anchoredPosition = menu.anchoredPosition + (menu.anchoredPosition - resume.anchoredPosition);

                TextMeshProUGUI label = help.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null) label.text = "Help";

                Button button = help.GetComponent<Button>();
                while (button.onClick.GetPersistentEventCount() > 0)
                    UnityEventTools.RemovePersistentListener(button.onClick, 0);
                UnityEventTools.AddPersistentListener(button.onClick, gm.OpenHelp);
                Debug.Log("CatDropperSetup: Help button created at " + rt.anchoredPosition);
            }
        }

        // --- Best score label on the Game Over panel ---
        if (gm.bestScoreText == null && gm.finalScoreText != null)
        {
            GameObject best = Object.Instantiate(gm.finalScoreText.gameObject, gm.finalScoreText.transform.parent);
            best.name = "BestScoreText";
            RectTransform src = gm.finalScoreText.rectTransform;
            RectTransform rt = (RectTransform)best.transform;
            TextMeshProUGUI text = best.GetComponent<TextMeshProUGUI>();
            text.enableAutoSizing = false;
            text.fontSize = gm.finalScoreText.fontSize * 0.5f;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.text = "Best: 0";
            rt.sizeDelta = new Vector2(600f, src.sizeDelta.y);
            rt.anchoredPosition = new Vector2(0f, src.anchoredPosition.y - gm.finalScoreText.fontSize * 0.95f);
            gm.bestScoreText = text;
            Debug.Log("CatDropperSetup: BestScoreText created at " + rt.anchoredPosition + " font " + text.fontSize);
        }

        EditorUtility.SetDirty(gm);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("CatDropperSetup: DONE");
    }
}
