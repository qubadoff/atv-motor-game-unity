using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Acilis ekrani: logo ve BurnGame yazisi belirir, sonra menuye gecer.</summary>
public class SplashController : MonoBehaviour
{
    public CanvasGroup group;
    public RectTransform logo;
    public UnityEngine.UI.Text title;
    public UnityEngine.UI.Text madeBy;
    public float fadeIn = 0.7f;
    public float hold = 1.6f;
    public float fadeOut = 0.5f;

    IEnumerator Start()
    {
        Time.timeScale = 1f;
        if (title != null) title.text = Loc.Get("title");
        if (madeBy != null) madeBy.text = Loc.Get("made_by");
        float t = 0f;
        group.alpha = 0f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeIn);
            group.alpha = k;
            if (logo != null) logo.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, 1f - (1f - k) * (1f - k));
            yield return null;
        }
        group.alpha = 1f;
        float held = 0f;
        while (held < hold)
        {
            held += Time.deltaTime;
            if (held > 0.3f && (Input.anyKeyDown || Input.touchCount > 0)) break;   // dokununca gec
            yield return null;
        }
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            group.alpha = 1f - Mathf.Clamp01(t / fadeOut);
            yield return null;
        }
        SceneManager.LoadScene("Menu");
    }
}
