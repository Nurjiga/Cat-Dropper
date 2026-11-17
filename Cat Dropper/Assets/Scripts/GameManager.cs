using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Игровая сессия: спавн и бросок котов, слияния, счёт, пауза, Game Over.
/// Всё, что хранится в этом классе, — временные данные одной игры (сбрасываются при Restart).
/// Постоянные данные (рекорд, коллекция, просмотр инструкции) — в SaveData.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Spawning")]
    public GameObject[] catPrefabs;
    public Transform spawnPoint;
    [Tooltip("Внутренний край левой стенки (X)")]
    public float leftWallX = -1.69f;
    [Tooltip("Внутренний край правой стенки (X)")]
    public float rightWallX = 1.69f;
    [Tooltip("Пауза между броском и появлением следующего кота")]
    public float nextCatDelay = 0.55f;
    [Tooltip("Самый крупный кот, который может выпасть (2 = только первые три уровня)")]
    public int maxSpawnTier = 2;

    [Header("Main UI")]
    public GameObject mainGameUI;
    public TextMeshProUGUI scoreText;
    public Image nextCatImage;

    [Header("Tutorial")]
    [Tooltip("Окно How to play со всеми шагами: само появляется только при первом запуске, дальше — по кнопке Help")]
    public TutorialModal tutorial;
    private int lastSpawnLevel = -1;
    private int secondLastSpawnLevel = -1;

    [Header("Pause UI")]
    public GameObject pausePanel;
    [Tooltip("Сплошной фон под меню паузы, чтобы поле не просвечивало")]
    public GameObject pauseBackdrop;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI bestScoreText;
    [Tooltip("Подпись над рекордом: \"Best score\" или \"New best!\"")]
    public TextMeshProUGUI bestLabelText;
    [Tooltip("Задержка между проигрышем и экраном Game Over (сек)")]
    public float gameOverDelay = 1.8f;
    public Color culpritColor = new Color(1f, 0.35f, 0.35f, 1f);

    // --- Временные данные сессии ---
    private GameObject currentCat;
    private Cat currentCatScript;
    private int nextCatLevel;
    private int score = 0;
    private bool isGameOver = false;
    private bool isAiming = false;
    private float lastDropX;

    private Camera cam;
    private SpriteRenderer aimGuide;
    private float floorY = -4.3f;

    public bool IsGameOver => isGameOver;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1;

        // Каждая игра начинается с нового случайного зерна
        Random.InitState((int)(System.DateTime.Now.Ticks & 0x7FFFFFFF));

        // На телефонах Unity по умолчанию держит 30 FPS — из-за этого игра казалась дёрганой
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        cam = Camera.main;
        FindTutorial();
    }

    void Start()
    {
        UnlockCat(0);
        UnlockCat(1);
        UnlockCat(2);

        lastDropX = spawnPoint.position.x;

        GameObject floor = GameObject.Find("Floor");
        if (floor != null) floorY = floor.transform.position.y + floor.transform.lossyScale.y * 0.5f;

        UpdateScoreText();
        GenerateNextCat();
        SpawnNewCat();
    }

    // ---------------- Управление ----------------

    void Update()
    {
        if (isGameOver || Time.timeScale == 0 || TutorialModal.IsOpen)
        {
            isAiming = false;
            return;
        }

        // Прицеливание начинается только с нажатия по игровому полю, а не по кнопке/панели.
        // Иначе отпускание пальца после закрытия окна сразу роняло кота.
        if (Input.GetMouseButtonDown(0)) isAiming = !IsPointerOverUI();

        if (isAiming && currentCat != null)
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)) MoveHeldCat(Input.mousePosition.x);
            if (Input.GetMouseButtonUp(0)) DropCat();
        }

        if (Input.GetMouseButtonUp(0)) isAiming = false;
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return true;
        return false;
    }

    void MoveHeldCat(float screenX)
    {
        float worldX = cam.ScreenToWorldPoint(new Vector3(screenX, 0f, -cam.transform.position.z)).x;
        PlaceHeldCat(worldX);
    }

    /// <summary>Ставит кота под палец, но не даёт залезть в стенку (с учётом его размера).</summary>
    void PlaceHeldCat(float worldX)
    {
        float radius = currentCatScript != null ? currentCatScript.WorldRadius : 0.3f;
        float minX = leftWallX + radius + 0.02f;
        float maxX = rightWallX - radius - 0.02f;
        float x = Mathf.Clamp(worldX, minX, maxX);

        currentCat.transform.position = new Vector3(x, spawnPoint.position.y, 0f);
        UpdateAimGuide(x, radius);
    }

    // ---------------- Спавн и бросок ----------------

    void GenerateNextCat()
    {
        // Случайный уровень среди тех, что могут выпасть (0..maxSpawnTier).
        // Чистый random любит серии (5 одинаковых подряд) — игроку это кажется "не случайным",
        // поэтому один и тот же уровень не выпадает больше двух раз подряд.
        int pool = Mathf.Clamp(maxSpawnTier + 1, 1, catPrefabs.Length);
        int level;
        do
        {
            level = Random.Range(0, pool);
        }
        while (pool > 1 && level == lastSpawnLevel && level == secondLastSpawnLevel);

        secondLastSpawnLevel = lastSpawnLevel;
        lastSpawnLevel = level;
        nextCatLevel = level;

        if (nextCatImage != null)
        {
            nextCatImage.sprite = catPrefabs[nextCatLevel].GetComponent<SpriteRenderer>().sprite;
        }
    }

    void SpawnNewCat()
    {
        if (isGameOver) return;

        currentCat = Instantiate(catPrefabs[nextCatLevel], spawnPoint.position, Quaternion.identity);
        currentCatScript = currentCat.GetComponent<Cat>();
        currentCatScript.catTier = nextCatLevel;
        currentCatScript.Hold();

        // Новый кот появляется там, куда бросали прошлого, а если палец уже на экране — сразу под пальцем
        if (isAiming && Input.GetMouseButton(0)) MoveHeldCat(Input.mousePosition.x);
        else PlaceHeldCat(lastDropX);

        GenerateNextCat();
    }

    void DropCat()
    {
        if (currentCat == null) return;

        lastDropX = currentCat.transform.position.x;
        currentCatScript.Release();
        currentCat = null;
        currentCatScript = null;
        if (aimGuide != null) aimGuide.enabled = false;

        AudioManager.Play("drop", 0.7f, Random.Range(0.95f, 1.05f));

        Invoke(nameof(SpawnNewCat), nextCatDelay);
    }

    /// <summary>Тонкая линия под котом: показывает, куда он упадёт.</summary>
    void UpdateAimGuide(float x, float catRadius)
    {
        if (aimGuide == null)
        {
            GameObject guide = new GameObject("AimGuide");
            aimGuide = guide.AddComponent<SpriteRenderer>();
            aimGuide.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 1f), 4f);
            aimGuide.color = new Color(1f, 1f, 1f, 0.35f);
            aimGuide.sortingOrder = -1;
            if (currentCatScript != null && currentCatScript.Renderer != null)
                aimGuide.sharedMaterial = currentCatScript.Renderer.sharedMaterial;
        }

        float top = spawnPoint.position.y - catRadius;
        aimGuide.enabled = true;
        aimGuide.transform.position = new Vector3(x, top, 0f);
        aimGuide.transform.localScale = new Vector3(0.03f, Mathf.Max(0.01f, top - floorY), 1f);
    }

    // ---------------- Слияние и счёт ----------------

    public void MergeCats(int level, Vector3 position)
    {
        if (isGameOver) return;

        int nextLevel = level + 1;
        int basePoints = (level + 1) * 10;

        AudioManager.Play("pop", 0.8f, Mathf.Lerp(1.2f, 0.8f, Mathf.Clamp01(level / 6f)));

        if (nextLevel == 7)
        {
            // Legendary cat: удваивает счёт и исчезает через 3 секунды, освобождая место
            GameObject finalCat = Instantiate(catPrefabs[7], position, Quaternion.identity);
            Cat finalCatScript = finalCat.GetComponent<Cat>();
            finalCatScript.catTier = 7;
            finalCatScript.PlaySpawnPop();

            Destroy(finalCat, 3f);

            score *= 2;

            UnlockCat(7);
            AudioManager.Play("legendary");
            AudioManager.Play("meow_happy", 0.9f);
        }
        else if (nextLevel < catPrefabs.Length)
        {
            GameObject mergedCat = Instantiate(catPrefabs[nextLevel], position, Quaternion.identity);
            Cat mergedCatScript = mergedCat.GetComponent<Cat>();
            mergedCatScript.catTier = nextLevel;
            mergedCatScript.PlaySpawnPop();

            score += basePoints;
            UnlockCat(nextLevel);
            AudioManager.PlayMeow(nextLevel);
        }
        else
        {
            // Два Legendary кота встретились: выше уровня нет, оба просто исчезают
            AudioManager.Play("meow_happy", 0.9f);
        }

        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null) scoreText.text = "Score\n" + score;
    }

    public void UnlockCat(int catIndex)
    {
        SaveData.UnlockCat(catIndex);
    }

    // ---------------- Пауза и Help ----------------

    public void TogglePause()
    {
        if (isGameOver || TutorialModal.IsOpen) return;

        bool isPaused = Time.timeScale == 0;

        if (isPaused)
        {
            Time.timeScale = 1;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (pauseBackdrop != null) pauseBackdrop.SetActive(false);
            if (mainGameUI != null) mainGameUI.SetActive(true);
            AudioManager.DuckMusic(false);
        }
        else
        {
            Time.timeScale = 0;
            if (pauseBackdrop != null) pauseBackdrop.SetActive(true);
            if (pausePanel != null) pausePanel.SetActive(true);
            if (mainGameUI != null) mainGameUI.SetActive(false);
            AudioManager.DuckMusic(true);
        }
    }

    /// <summary>Кнопка Help: повторно открыть инструкцию.</summary>
    public void OpenHelp()
    {
        if (isGameOver) return;
        FindTutorial();
        if (tutorial != null) tutorial.Open();
    }

    void FindTutorial()
    {
        if (tutorial != null || mainGameUI == null) return;

        Transform canvas = mainGameUI.transform.parent;
        Transform panel = canvas != null ? canvas.Find("TutorialPanel") : null;
        if (panel == null) return;

        tutorial = panel.GetComponent<TutorialModal>();
        if (tutorial == null) tutorial = panel.gameObject.AddComponent<TutorialModal>();
    }

    // ---------------- Game Over ----------------

    public void GameOver()
    {
        GameOver(null, null);
    }

    public void GameOver(Cat culprit, DeadZone zone)
    {
        if (isGameOver) return; // защита от повторного запуска
        isGameOver = true;

        CancelInvoke(nameof(SpawnNewCat));
        StartCoroutine(GameOverSequence(culprit, zone));
    }

    IEnumerator GameOverSequence(Cat culprit, DeadZone zone)
    {
        // 1. Блокируем управление и убираем кота из "руки"
        isAiming = false;
        if (currentCat != null) Destroy(currentCat);
        currentCat = null;
        currentCatScript = null;
        if (aimGuide != null) aimGuide.enabled = false;

        // 2. Замораживаем поле, чтобы игрок увидел, из-за чего проиграл
        foreach (Cat cat in Cat.All) cat.Freeze();

        AudioManager.DuckMusic(true);
        AudioManager.Play("meow_sad");

        // 3. Обратная связь: тряска камеры, кот-виновник и линия мигают красным
        SpriteRenderer culpritRenderer = culprit != null ? culprit.Renderer : null;
        Vector3 cameraHome = cam.transform.position;

        for (float t = 0f; t < gameOverDelay; t += Time.unscaledDeltaTime)
        {
            float shake = Mathf.Clamp01(1f - t / 0.45f) * 0.12f;
            cam.transform.position = cameraHome + (Vector3)(Random.insideUnitCircle * shake);

            float blink = Mathf.PingPong(t * 5f, 1f);
            if (culpritRenderer != null) culpritRenderer.color = Color.Lerp(Color.white, culpritColor, blink);
            if (zone != null) zone.SetFlash(blink);

            yield return null;
        }

        cam.transform.position = cameraHome;

        // 4. Экран Game Over
        ShowGameOverScreen();
    }

    void ShowGameOverScreen()
    {
        bool newRecord = SaveData.TrySetBestScore(score);

        if (mainGameUI != null) mainGameUI.SetActive(false);

        if (finalScoreText != null) finalScoreText.text = score.ToString();
        // как в макете: подпись "Best score" и под ней число (при новом рекорде подпись "New best!")
        if (bestLabelText != null) bestLabelText.text = newRecord ? "New best!" : "Best score";
        if (bestScoreText != null) bestScoreText.text = SaveData.BestScore.ToString();

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        AudioManager.Play("gameover", 0.9f);
    }

    // ---------------- Сцены ----------------

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
    }
}
