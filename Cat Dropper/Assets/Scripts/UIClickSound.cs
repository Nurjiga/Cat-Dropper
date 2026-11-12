using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Звук нажатия на кнопку. AudioManager сам добавляет этот компонент на все кнопки,
/// вручную вешать не нужно (но можно — дубля не будет).
/// </summary>
[RequireComponent(typeof(Button))]
public class UIClickSound : MonoBehaviour, IPointerDownHandler
{
    public string clipName = "click";

    public void OnPointerDown(PointerEventData eventData)
    {
        Button button = GetComponent<Button>();
        if (button != null && button.interactable) AudioManager.Play(clipName, 0.8f);
    }
}
