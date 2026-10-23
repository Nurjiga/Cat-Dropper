using UnityEngine;

public class Cat : MonoBehaviour
{
    [Header("Cat Settings")]
    public int catTier = 0; // Уровень кота

    public bool isMerged = false; // Защита от двойного слияния (теперь видна в Inspector)

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Cat otherCat = collision.gameObject.GetComponent<Cat>();

        // Если коснулись другого кота, и оба еще НЕ сливались
        if (otherCat != null && !this.isMerged && !otherCat.isMerged)
        {
            // Если уровни совпадают
            if (otherCat.catTier == this.catTier)
            {
                // 1. Мгновенно блокируем обоих котов от любых других столкновений в этом кадре
                this.isMerged = true;
                otherCat.isMerged = true;

                // 2. Вычисляем точку ровно посередине
                Vector3 spawnPosition = (transform.position + otherCat.transform.position) / 2f;

                // 3. Сообщаем менеджеру о слиянии
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.MergeCats(this.catTier, spawnPosition);
                }

                // 4. Уничтожаем старых котов
                Destroy(otherCat.gameObject);
                Destroy(this.gameObject);
            }
        }
    }
}