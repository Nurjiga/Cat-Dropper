using UnityEngine;

/// <summary>
/// Линия проигрыша. Если кот лежит на ней неподвижно дольше graceTime — игра окончена.
/// (Раньше хватало одного кадра, поэтому кот, просто подпрыгнувший до линии, сразу давал Game Over.)
/// </summary>
public class DeadZone : MonoBehaviour
{
    [Tooltip("Сколько секунд кот должен пролежать на линии, чтобы засчитался проигрыш")]
    public float graceTime = 0.8f;

    [Tooltip("Ниже этой скорости кот считается лежащим")]
    public float restSpeed = 0.2f;

    public Color flashColor = new Color(1f, 0.25f, 0.25f, 1f);

    [Tooltip("Видимая пунктирная линия (мигает красным при проигрыше). Если пусто — берётся SpriteRenderer этого объекта.")]
    public SpriteRenderer visual;

    private SpriteRenderer line;
    private Color baseColor;

    private void Awake()
    {
        line = visual != null ? visual : GetComponent<SpriteRenderer>();
        if (line != null) baseColor = line.color;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        GameManager manager = GameManager.Instance;
        if (manager == null || manager.IsGameOver) return;

        Cat cat = collision.GetComponent<Cat>();
        if (cat == null || cat.IsHeld || cat.isMerged) return;

        Rigidbody2D rb = collision.attachedRigidbody;
        if (rb == null) return;

        rb.WakeUp(); // уснувшее тело перестаёт слать OnTriggerStay — не даём уснуть на линии

        if (rb.linearVelocity.sqrMagnitude < restSpeed * restSpeed)
        {
            cat.dangerTimer += Time.fixedDeltaTime;
            if (cat.dangerTimer >= graceTime) manager.GameOver(cat, this);
        }
        else
        {
            cat.dangerTimer = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Cat cat = collision.GetComponent<Cat>();
        if (cat != null) cat.dangerTimer = 0f;
    }

    /// <summary>Мигание линии при проигрыше (0 — обычный цвет, 1 — красный).</summary>
    public void SetFlash(float amount)
    {
        if (line != null) line.color = Color.Lerp(baseColor, flashColor, amount);
    }
}
