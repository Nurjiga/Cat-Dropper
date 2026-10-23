using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // Обязательно для блокировки кликов по UI

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Spawning")]
    public GameObject[] catPrefabs; 
    public Transform spawnPoint; 
    private float leftWallX = -1.76f; 
    private float rightWallX = 1.76f;

    [Header("Main UI")]
    public GameObject mainGameUI; 
    public TextMeshProUGUI scoreText;
    public Image nextCatImage;

    [Header("Tutorial & Hints")]
    public TextMeshProUGUI hintText; 
    private int dropCount = 0;

    [Header("Pause UI")]
    public GameObject pausePanel; 

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    private GameObject currentCat;
    private int nextCatLevel;
    private int score = 0;
    private bool isGameOver = false;

    void Awake() 
    { 
        Instance = this; 
    }

    void Start() 
    { 
        // Первые 3 кота всегда открыты в коллекции по умолчанию
        UnlockCat(0);
        UnlockCat(1);
        UnlockCat(2);

        GenerateNextCat();
        SpawnNewCat(); 
    }

    void Update()
    {
        if (isGameOver) return;
        
        // Блокируем управление пальцем, если игра на паузе
        if (Time.timeScale == 0) return;

        // Блокируем сброс кота, если курсор мыши или палец находится над кнопкой
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;

        if (currentCat != null)
        {
            // Прицеливание
            if (Input.GetMouseButton(0)) 
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                float clampedX = Mathf.Clamp(mousePos.x, leftWallX, rightWallX); 
                currentCat.transform.position = new Vector3(clampedX, spawnPoint.position.y, 0);
            }

            // Сброс кота
            if (Input.GetMouseButtonUp(0)) 
            {
                DropCat();
            }
        }
    }

    void GenerateNextCat()
    {
        // Выбираем случайного кота из первых 3 уровней (0, 1, 2)
        nextCatLevel = Random.Range(0, 3); 
        
        if (nextCatImage != null)
        {
            nextCatImage.sprite = catPrefabs[nextCatLevel].GetComponent<SpriteRenderer>().sprite;
        }
    }

    void SpawnNewCat()
    {
        currentCat = Instantiate(catPrefabs[nextCatLevel], spawnPoint.position, Quaternion.identity);
        
        // ПРИНУДИТЕЛЬНО задаем уровень кота (спасает, если забыли настроить Префабы)
        currentCat.GetComponent<Cat>().catTier = nextCatLevel;
        
        Rigidbody2D rb = currentCat.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; 
        rb.linearVelocity = Vector2.zero; 
        
        GenerateNextCat();
    }

    void DropCat()
    {
        if (currentCat != null)
        {
            currentCat.GetComponent<Rigidbody2D>().gravityScale = 1; 
            currentCat = null;
            
            ShowNextHint(); // Показываем следующую подсказку
            
            Invoke("SpawnNewCat", 1.2f); 
        }
    }

    void ShowNextHint()
    {
        if (hintText == null) return;

        dropCount++;
        if (dropCount == 1) hintText.text = "Move your finger to drop the cat!";
        else if (dropCount == 2) hintText.text = "Same cats merge into a bigger one!";
        else if (dropCount == 3) hintText.text = "Don’t let the cats fall out the line!";
        else hintText.transform.parent.gameObject.SetActive(false);
    }

    // Порядок аргументов: (1) level (int), (2) position (Vector3) — совпадает с Cat.cs
    public void MergeCats(int level, Vector3 position)
    {
        int nextLevel = level + 1;
        int basePoints = (level + 1) * 10;

        if (nextLevel == 7) // Если это кот 8-го уровня (индекс 7)
        {
            // 1. Спавним финального кота, чтобы игроки его увидели
            GameObject finalCat = Instantiate(catPrefabs[7], position, Quaternion.identity);
            
            // 2. ПРИНУДИТЕЛЬНО задаем ему уровень (для безопасности)
            finalCat.GetComponent<Cat>().catTier = 7;
            
            // 3. Уничтожаем этот объект ровно через 3 секунды (3f)
            Destroy(finalCat, 3f);

            // Начисляем очки и сохраняем в коллекцию (как у вас и было)
            score += basePoints * 2; 
            UnlockCat(7); 
        }
        else if (nextLevel < catPrefabs.Length)
        {
            // Спавним нового кота
            GameObject mergedCat = Instantiate(catPrefabs[nextLevel], position, Quaternion.identity);
            
            // ПРИНУДИТЕЛЬНО задаем уровень новому коту
            mergedCat.GetComponent<Cat>().catTier = nextLevel;
            
            score += basePoints;
            UnlockCat(nextLevel);
        }
        
        if (scoreText != null)
        {
            scoreText.text = "Score\n" + score;
        }
    }

    // Сохранение статуса кота в память устройства
    public void UnlockCat(int catIndex)
    {
        PlayerPrefs.SetInt("CatUnlocked_" + catIndex, 1);
        PlayerPrefs.Save();
    }

    public void TogglePause()
    {
        bool isPaused = Time.timeScale == 0;
        
        if (isPaused)
        {
            Time.timeScale = 1; 
            if (pausePanel != null) pausePanel.SetActive(false);
            if (mainGameUI != null) mainGameUI.SetActive(true); // Возвращаем Score и Next
        }
        else
        {
            Time.timeScale = 0; 
            if (pausePanel != null) pausePanel.SetActive(true);
            if (mainGameUI != null) mainGameUI.SetActive(false); // Прячем Score и Next
        }
    }

    public void GameOver()
    {
        if (!isGameOver)
        {
            isGameOver = true;
            
            if (mainGameUI != null) 
            {
                mainGameUI.SetActive(false); 
            }
            
            if (finalScoreText != null)
            {
                finalScoreText.text = score.ToString(); 
            }
            
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1; // Обязательно возвращаем время в норму
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1; // Обязательно возвращаем время в норму
        SceneManager.LoadScene("Main Menu"); 
    }
}