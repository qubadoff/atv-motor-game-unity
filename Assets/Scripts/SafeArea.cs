using UnityEngine;

/// <summary>RectTransform'u cihazin guvenli alanina (centik, yuvarlak kose) sigdirir.</summary>
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    Rect applied = new Rect(-1, -1, -1, -1);

    void Update()
    {
        Rect area = Screen.safeArea;
        if (area == applied) return;
        applied = area;
        var rt = GetComponent<RectTransform>();
        Vector2 min = area.position;
        Vector2 max = area.position + area.size;
        min.x /= Screen.width; min.y /= Screen.height;
        max.x /= Screen.width; max.y /= Screen.height;
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
