using UnityEngine;
public class DeadZone : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null && rb.gravityScale > 0 && rb.linearVelocity.magnitude < 0.1f)
        {
            GameManager.Instance.GameOver();
        }
    }
}