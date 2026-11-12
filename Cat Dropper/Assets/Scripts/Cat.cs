using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Один кот на поле: знает свой уровень, сливается с таким же котом и издаёт звук приземления.
/// </summary>
public class Cat : MonoBehaviour
{
    /// <summary>Все живые коты на поле (нужно GameManager, чтобы заморозить поле при проигрыше).</summary>
    public static readonly List<Cat> All = new List<Cat>();

    [Header("Cat Settings")]
    public int catTier = 0; // Уровень кота

    public bool isMerged = false; // Защита от двойного слияния

    /// <summary>Кот ещё в "руке" игрока и не сброшен.</summary>
    public bool IsHeld { get; private set; }

    /// <summary>Сколько секунд кот неподвижно лежит на линии проигрыша (считает DeadZone).</summary>
    [HideInInspector] public float dangerTimer;

    private Rigidbody2D rb;
    private CircleCollider2D circle;
    private SpriteRenderer spriteRenderer;
    private bool hasLanded;

    public SpriteRenderer Renderer => spriteRenderer;

    /// <summary>Радиус кота в мировых единицах.</summary>
    public float WorldRadius => circle != null ? circle.radius * Mathf.Abs(transform.lossyScale.x) : 0.3f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circle = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (rb != null) rb.interpolation = RigidbodyInterpolation2D.Interpolate; // плавное движение между шагами физики
    }

    private void OnEnable() { All.Add(this); }
    private void OnDisable() { All.Remove(this); }

    // ---------------- Состояния ----------------

    /// <summary>Кот висит наверху и ждёт броска: физика выключена, его нельзя задеть или сдвинуть.</summary>
    public void Hold()
    {
        IsHeld = true;
        rb.simulated = false;
    }

    /// <summary>Бросок: включаем физику, кот падает строго вниз.</summary>
    public void Release()
    {
        IsHeld = false;
        rb.simulated = true;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    /// <summary>Остановить кота на месте (Game Over).</summary>
    public void Freeze()
    {
        if (rb != null) rb.simulated = false;
    }

    /// <summary>Анимация появления после слияния.</summary>
    public void PlaySpawnPop()
    {
        StartCoroutine(SpawnPop());
    }

    private IEnumerator SpawnPop()
    {
        Vector3 fullScale = transform.localScale;
        const float duration = 0.14f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            // 0.6 -> 1.08 -> 1.0
            float s = k < 0.7f ? Mathf.Lerp(0.6f, 1.08f, k / 0.7f) : Mathf.Lerp(1.08f, 1f, (k - 0.7f) / 0.3f);
            transform.localScale = fullScale * s;
            yield return null;
        }
        transform.localScale = fullScale;
    }

    // ---------------- Столкновения ----------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Звук приземления — один раз на кота, при первом касании
        if (!hasLanded)
        {
            hasLanded = true;
            float impact = collision.relativeVelocity.magnitude;
            if (impact > 0.6f) AudioManager.Play("land", Mathf.Clamp01(impact / 7f) * 0.8f + 0.2f, Random.Range(0.9f, 1.1f));
        }

        Cat otherCat = collision.gameObject.GetComponent<Cat>();

        // Если коснулись другого кота, и оба еще НЕ сливались
        if (otherCat != null && !isMerged && !otherCat.isMerged && otherCat.catTier == catTier)
        {
            // 1. Мгновенно блокируем обоих котов от любых других столкновений в этом кадре
            isMerged = true;
            otherCat.isMerged = true;

            // 2. Вычисляем точку ровно посередине
            Vector3 spawnPosition = (transform.position + otherCat.transform.position) / 2f;

            // 3. Сообщаем менеджеру о слиянии
            if (GameManager.Instance != null)
            {
                GameManager.Instance.MergeCats(catTier, spawnPosition);
            }

            // 4. Уничтожаем старых котов
            Destroy(otherCat.gameObject);
            Destroy(gameObject);
        }
    }
}
