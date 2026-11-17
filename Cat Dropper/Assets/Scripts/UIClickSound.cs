using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Нажатие на кнопку: звук клика + лёгкое "вдавливание" (кнопка чуть уменьшается, пока её держат).
/// AudioManager сам добавляет этот компонент на все кнопки, вручную вешать не нужно (но можно — дубля не будет).
/// </summary>
[RequireComponent(typeof(Button))]
public class UIClickSound : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public string clipName = "click";

    [Tooltip("Во сколько раз кнопка уменьшается, пока на неё нажимают (1 = без эффекта)")]
    public float pressedScale = 0.96f;

    private Vector3 baseScale;
    private bool pressed;

    public void OnPointerDown(PointerEventData eventData)
    {
        Button button = GetComponent<Button>();
        if (button == null || !button.interactable) return;

        AudioManager.Play(clipName, 0.8f);

        if (!pressed)
        {
            pressed = true;
            baseScale = transform.localScale;
            transform.localScale = baseScale * pressedScale;
        }
    }

    public void OnPointerUp(PointerEventData eventData) { Release(); }
    public void OnPointerExit(PointerEventData eventData) { Release(); }

    // Кнопка могла выключиться по клику (например, Resume прячет панель паузы) — вернуть размер
    private void OnDisable() { Release(); }

    private void Release()
    {
        if (!pressed) return;
        pressed = false;
        transform.localScale = baseScale;
    }
}
