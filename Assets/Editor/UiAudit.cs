using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Aktif Text'lerin tasma ve ust uste binme kontrolu (oynanis testinden cagrilir).</summary>
public static class UiAudit
{
    public static int Check(string label)
    {
        var texts = Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
        var list = new List<(Text t, Rect r)>();
        int problems = 0;
        foreach (var t in texts)
        {
            if (!t.gameObject.activeInHierarchy || string.IsNullOrEmpty(t.text)) continue;
            var rt = t.rectTransform;
            Rect r = ScreenRect(rt);
            float pw = t.preferredWidth * rt.lossyScale.x;
            float ph = t.preferredHeight * rt.lossyScale.y;
            if (pw > r.width + 2f)
            {
                problems++;
                Debug.Log($"UIAUDIT[{label}] TASMA: '{Path(t.transform)}' metin={pw:F0}px kutu={r.width:F0}px  \"{t.text.Replace("\n", " ")}\"");
                r = new Rect(r.center.x - pw / 2f, r.y, pw, r.height);   // gercek kaplanan alan
            }
            if (ph > r.height + 2f && t.verticalOverflow == VerticalWrapMode.Overflow)
                r = new Rect(r.x, r.center.y - ph / 2f, r.width, ph);
            list.Add((t, r));
        }
        for (int i = 0; i < list.Count; i++)
            for (int j = i + 1; j < list.Count; j++)
            {
                var a = list[i]; var b = list[j];
                if (a.t.transform.IsChildOf(b.t.transform) || b.t.transform.IsChildOf(a.t.transform)) continue;
                if (a.t.transform.parent == b.t.transform.parent && a.t.transform.parent.GetComponent<InputField>() != null) continue;
                Rect x = Intersect(a.r, b.r);
                if (x.width > 4f && x.height > 4f)
                {
                    problems++;
                    Debug.Log($"UIAUDIT[{label}] BINME: '{Path(a.t.transform)}' x '{Path(b.t.transform)}' kesisim={x.width:F0}x{x.height:F0}");
                }
            }
        Debug.Log($"UIAUDIT[{label}] metin={list.Count} sorun={problems}");
        return problems;
    }

    static Rect ScreenRect(RectTransform rt)
    {
        var corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        foreach (var c in corners)
        {
            minX = Mathf.Min(minX, c.x); maxX = Mathf.Max(maxX, c.x);
            minY = Mathf.Min(minY, c.y); maxY = Mathf.Max(maxY, c.y);
        }
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    static Rect Intersect(Rect a, Rect b)
    {
        float x0 = Mathf.Max(a.xMin, b.xMin), y0 = Mathf.Max(a.yMin, b.yMin);
        float x1 = Mathf.Min(a.xMax, b.xMax), y1 = Mathf.Min(a.yMax, b.yMax);
        return new Rect(x0, y0, x1 - x0, y1 - y0);
    }

    static string Path(Transform t)
    {
        string s = t.name;
        while (t.parent != null && t.parent.parent != null) { t = t.parent; s = t.name + "/" + s; }
        return s;
    }
}
