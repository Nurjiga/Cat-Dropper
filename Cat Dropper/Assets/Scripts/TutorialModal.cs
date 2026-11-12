using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Инструкция "How to play" из нескольких окон, которые появляются одно за другим:
/// 1) шаги игры (кнопка Next), 2) Legendary-кот (кнопка Got it!).
/// Сама показывается только при первом запуске игры (флаг TutorialSeen в PlayerPrefs),
/// дальше открывается только вручную через Help.
/// Висит на корне панели; окна — её дочерние объекты (pages) с кнопками (nextButtons).
/// </summary>
public class TutorialModal : MonoBehaviour
{
    /// <summary>Пока инструкция открыта, GameManager не даёт управлять котами.</summary>
    public static bool IsOpen { get; private set; }

    /// <summary>Главное меню ставит true, чтобы показать инструкцию при следующем входе в игру.</summary>
    public static bool ForceShowOnce;

    [Tooltip("Окна инструкции по порядку")]
    public GameObject[] pages;

    [Tooltip("Кнопка каждого окна: на всех, кроме последнего — Next, на последнем — Got it! (закрывает)")]
    public Button[] nextButtons;

    private bool initialized;
    private bool openedManually;
    private int current;

    private void Awake()
    {
        Initialize();
        if (openedManually) return; // панель была выключена в сцене и её открыли через Help

        bool show = !SaveData.TutorialSeen || ForceShowOnce;
        ForceShowOnce = false;

        if (show) Open();
        else gameObject.SetActive(false);
    }

    private void Initialize()
    {
        if (initialized) return;
        initialized = true;

        if (nextButtons == null) return;
        for (int i = 0; i < nextButtons.Length; i++)
        {
            if (nextButtons[i] != null) nextButtons[i].onClick.AddListener(Next);
        }
    }

    /// <summary>Открыть инструкцию с первого окна (кнопка Help).</summary>
    public void Open()
    {
        Initialize();
        openedManually = true;
        transform.SetAsLastSibling(); // поверх остальных панелей
        gameObject.SetActive(true);
        ShowPage(0);
        IsOpen = true;
    }

    /// <summary>Следующее окно; после последнего — закрыть.</summary>
    public void Next()
    {
        if (pages != null && current < pages.Length - 1) ShowPage(current + 1);
        else Close();
    }

    /// <summary>Закрыть и запомнить, что игрок инструкцию прошёл.</summary>
    public void Close()
    {
        if (!SaveData.TutorialSeen) SaveData.TutorialSeen = true;
        IsOpen = false;
        gameObject.SetActive(false);
    }

    private void ShowPage(int index)
    {
        current = index;
        if (pages == null) return;
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null) pages[i].SetActive(i == index);
        }
    }

    private void OnDisable()
    {
        IsOpen = false;
    }
}
