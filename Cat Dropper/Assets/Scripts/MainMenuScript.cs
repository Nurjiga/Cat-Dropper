using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "SampleScene"; 

    [Header("Panels")]
    public GameObject collectionsPanel;
    public GameObject settingsPanel;
    public TutorialModal tutorial; // окно How to play для кнопки Help

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

            // Первые 3 кота (индексы 0, 1, 2) всегда доступны по умолчанию
            bool isUnlocked = i <= 2 || SaveData.IsCatUnlocked(i);

            // Если кот открыт — скрываем плашку "Locked!", иначе показываем ее
            lockedOverlays[i].SetActive(!isUnlocked);

            // Пока кот закрыт, прячем и саму цветную карточку: плашка Locked! полупрозрачная по краям,
            // и цвет/силуэт кота просвечивал сквозь неё
            Image card = lockedOverlays[i].transform.parent.GetComponent<Image>();
            if (card != null) card.enabled = isUnlocked;
        }
    }

    // --- Help: инструкция по игре (окно "How to play" прямо в меню) ---
    public void OpenHelp()
    {
        if (tutorial != null)
        {
            tutorial.Open();
            return;
        }

        // Запасной вариант, если окна в меню нет: показать инструкцию при входе в игру
        TutorialModal.ForceShowOnce = true;
        StartGame();
    }

    // --- Сброс прогресса (для будущей кнопки в Settings) ---
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        CheckUnlockedCats();
    }

    // --- Звук: методы для кнопок будущего экрана Settings ---
    public void ToggleSound()
    {
        AudioManager.SoundOn = !AudioManager.SoundOn;
    }

    public void ToggleMusic()
    {
        AudioManager.MusicOn = !AudioManager.MusicOn;
    }

    public void SetSound(bool isOn)
    {
        AudioManager.SoundOn = isOn;
    }

    public void SetMusic(bool isOn)
    {
        AudioManager.MusicOn = isOn;
    }
}