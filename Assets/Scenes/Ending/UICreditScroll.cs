using UnityEngine;

public class UICreditScroll : MonoBehaviour
{
    public float speed;
    public float stopY;
    public RectTransform rect;

    void Update()
    {
        if (rect.anchoredPosition.y < stopY)
            rect.anchoredPosition += Vector2.up * speed * Time.unscaledDeltaTime;
    }
}
