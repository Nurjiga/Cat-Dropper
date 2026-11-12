using UnityEditor;
using UnityEngine;

/// <summary>
/// Сбрасывает сохранения игры в редакторе: открытых котов, рекорд, просмотр туториала, настройки звука.
/// Меню: Tools > Cat Dropper > Reset Progress (PlayerPrefs)
/// </summary>
public static class ResetProgress
{
    [MenuItem("Tools/Cat Dropper/Reset Progress (PlayerPrefs)")]
    public static void Run()
    {
        if (!EditorUtility.DisplayDialog("Reset Progress",
                "Сбросить открытых котов, Best Score, туториал и настройки звука?", "Сбросить", "Отмена"))
            return;

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Cat Dropper: PlayerPrefs cleared. Cats 4-8 are locked again, tutorial will show on next launch.");
    }
}
