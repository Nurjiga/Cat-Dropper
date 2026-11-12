using UnityEngine;

/// <summary>
/// Держит "дизайн-кадр" (фиксированный макет вроде 390x844 из Figma) целиком на экране:
/// масштабирует его так, чтобы он вписался в родителя без искажений, на любом телефоне.
/// Вешается на корень панели, кадр — её дочерний объект.
/// </summary>
[ExecuteAlways]
public class FitToCanvas : MonoBehaviour
{
    public RectTransform frame;

    private void OnEnable() { Apply(); }
    private void OnRectTransformDimensionsChange() { Apply(); }

    public void Apply()
    {
        if (frame == null) return;
        Rect parent = ((RectTransform)transform).rect;
        if (parent.width <= 0f || parent.height <= 0f || frame.rect.width <= 0f) return;

        float scale = Mathf.Min(parent.width / frame.rect.width, parent.height / frame.rect.height);
        frame.localScale = new Vector3(scale, scale, 1f);
    }
}
