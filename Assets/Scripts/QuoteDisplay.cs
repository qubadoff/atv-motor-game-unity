using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Gokyuzunde asili duran, yavasca geriye kayip solan motivasyon cumlesi.</summary>
public class QuoteDisplay : MonoBehaviour
{
    public Font font;
    public Camera cam;
    public float fadeIn = 0.7f;
    public float hold = 3.2f;
    public float fadeOut = 1.3f;
    public Vector2 startOffset = new Vector2(3.5f, 2.4f);
    public float driftSpeed = 0.8f;

    readonly HashSet<int> used = new HashSet<int>();

    public void ShowRandom()
    {
        Show(Quotes.Random(used));
    }

    public void Show(string text)
    {
        StartCoroutine(Run(text));
    }

    IEnumerator Run(string text)
    {
        var go = new GameObject("Quote");
        var tm = go.AddComponent<TextMesh>();
        tm.font = font;
        tm.text = Wrap(text, 30);
        tm.fontSize = 64;
        tm.characterSize = 0.1f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.lineSpacing = 1.05f;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = font.material;
        mr.sortingOrder = 20;

        Vector2 offset = startOffset;
        float t = 0f, total = fadeIn + hold + fadeOut;
        while (t < total)
        {
            t += Time.deltaTime;
            float a = t < fadeIn ? t / fadeIn : t < fadeIn + hold ? 1f : Mathf.Clamp01(1f - (t - fadeIn - hold) / fadeOut);
            offset.x -= driftSpeed * Time.deltaTime;
            float bob = Mathf.Sin(t * 1.6f) * 0.12f;
            Vector3 c = cam.transform.position;
            go.transform.position = new Vector3(c.x + offset.x, c.y + offset.y + bob, 0f);
            tm.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }
        Destroy(go);
    }

    static string Wrap(string text, int maxChars)
    {
        var words = text.Split(' ');
        var sb = new System.Text.StringBuilder();
        int lineLen = 0;
        foreach (var w in words)
        {
            if (lineLen > 0 && lineLen + 1 + w.Length > maxChars) { sb.Append('\n'); lineLen = 0; }
            else if (lineLen > 0) { sb.Append(' '); lineLen++; }
            sb.Append(w);
            lineLen += w.Length;
        }
        return sb.ToString();
    }
}
