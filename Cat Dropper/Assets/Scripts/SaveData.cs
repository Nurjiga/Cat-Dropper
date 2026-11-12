using UnityEngine;

/// <summary>
/// Постоянные данные игрока (живут между запусками игры, хранятся в PlayerPrefs).
/// Временные данные сессии (текущий счёт, коты на поле) сюда НЕ попадают —
/// они лежат в GameManager и сбрасываются при каждой новой игре.
/// </summary>
public static class SaveData
{
    private const string TutorialSeenKey = "TutorialSeen";
    private const string BestScoreKey = "BestScore";
    private const string CatUnlockedPrefix = "CatUnlocked_";
    private const string SoundOnKey = "SoundOn";
    private const string MusicOnKey = "MusicOn";

    // --- Tutorial ---
    public static bool TutorialSeen
    {
        get => PlayerPrefs.GetInt(TutorialSeenKey, 0) == 1;
        set => SetInt(TutorialSeenKey, value ? 1 : 0);
    }

    // --- Best score ---
    public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);

    /// <summary>Сохраняет рекорд, если он побит. Возвращает true, если это новый рекорд.</summary>
    public static bool TrySetBestScore(int score)
    {
        if (score <= BestScore) return false;
        SetInt(BestScoreKey, score);
        return true;
    }

    // --- Коллекция котов ---
    public static bool IsCatUnlocked(int catIndex)
    {
        return PlayerPrefs.GetInt(CatUnlockedPrefix + catIndex, 0) == 1;
    }

    /// <summary>Открывает кота. Пишет на диск только один раз (раньше запись шла при каждом слиянии и давала подлагивания).</summary>
    public static bool UnlockCat(int catIndex)
    {
        if (IsCatUnlocked(catIndex)) return false;
        SetInt(CatUnlockedPrefix + catIndex, 1);
        return true;
    }

    // --- Настройки звука ---
    public static bool SoundOn
    {
        get => PlayerPrefs.GetInt(SoundOnKey, 1) == 1;
        set => SetInt(SoundOnKey, value ? 1 : 0);
    }

    public static bool MusicOn
    {
        get => PlayerPrefs.GetInt(MusicOnKey, 1) == 1;
        set => SetInt(MusicOnKey, value ? 1 : 0);
    }

    private static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }
}
