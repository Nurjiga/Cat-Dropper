using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "SampleScene"; 

    [Header("Panels")]
    public GameObject collectionsPanel;
    public GameObject settingsPanel;

    [Header("Collections (Locked Overlays 0 to 7)")]
    // Сюда перетаскиваются 8 плашек "Locked!" от каждого кота
    public GameObject[] lockedOverlays;

    private void Start()
    {
        // Проверяем статус котов при запуске главного меню
        CheckUnlockedCats();
    }

    // --- Запуск игры ---
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName); 
    }

    // --- Управление панелью Collections ---
    public void OpenCollections()
    {
        if (collectionsPanel != null)
        {
            collectionsPanel.SetActive(true);
            CheckUnlockedCats(); // Обновляем состояние замков при открытии
        }
    }

    public void CloseCollections()
    {
        if (collectionsPanel != null)
        {
            collectionsPanel.SetActive(false);
        }
    }

    // --- Управление панелью Settings ---
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // --- Проверка открытых котов ---
    public void CheckUnlockedCats()
    {
        if (lockedOverlays == null || lockedOverlays.Length == 0) return;

        for (int i = 0; i < lockedOverlays.Length; i++)
        {
            if (lockedOverlays[i] == null) continue;

            // Читаем из памяти телефона (1 = открыт, 0 = заблокирован)
            int isUnlocked = PlayerPrefs.GetInt("CatUnlocked_" + i, 0);

            // Первые 3 кота (индексы 0, 1, 2) всегда доступны по умолчанию
            if (i <= 2) isUnlocked = 1;

            // Если кот открыт (isUnlocked == 1) — скрываем плашку "Locked!", иначе показываем ее
            lockedOverlays[i].SetActive(isUnlocked == 0);
        }
    }
}