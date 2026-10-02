using UnityEngine;

/// <summary>Bir seviyenin arazi, uzunluk ve hedef sure ayarlari.</summary>
[System.Serializable]
public struct LevelConfig
{
    public int level;
    public int seed;
    public float length;            // bitis cizgisi (metre)
    public float baseAmplitude;
    public float amplitudeGrowth;
    public float maxAmplitude;
    public float noiseScale;
    public float detailAmplitude;
    public float featureMinGap;     // tumsek/cukur araligi
    public float featureMaxGap;
    public float bumpMinHeight;
    public float bumpMaxHeight;
    public float bumpWidth;
    public float pitChance;         // ozelligin cukur olma olasiligi
    public float pitMinDepth;
    public float pitMaxDepth;
    public float pitWidth;
    public float parSpeed;          // iyi bir surusun ortalama hizi (m/s)

    /// <summary>3 yildiz icin ust sure.</summary>
    public float ThreeStarTime => length / parSpeed;
    /// <summary>2 yildiz icin ust sure.</summary>
    public float TwoStarTime => length / parSpeed * 1.45f;

    public int StarsFor(float seconds) => seconds <= ThreeStarTime ? 3 : seconds <= TwoStarTime ? 2 : 1;
}

/// <summary>50 seviye; ilk seviyeden itibaren zorlu, 50'ye dogru daha dik ve daha sik engel.</summary>
public static class Levels
{
    public const int Count = 50;

    public static LevelConfig Get(int level)
    {
        level = Mathf.Clamp(level, 1, Count);
        float t = (level - 1) / (float)(Count - 1);
        float ease = t * t * 0.5f + t * 0.5f;

        return new LevelConfig
        {
            level = level,
            seed = 1000 + level * 7919,
            length = Mathf.Lerp(420f, 700f, t),
            baseAmplitude = Mathf.Lerp(1.8f, 2.7f, ease),
            amplitudeGrowth = 0.015f,
            maxAmplitude = Mathf.Lerp(3.0f, 4.5f, ease),
            noiseScale = Mathf.Lerp(0.045f, 0.052f, ease),
            detailAmplitude = Mathf.Lerp(0.3f, 0.42f, ease),
            featureMinGap = Mathf.Lerp(11f, 7f, ease),
            featureMaxGap = Mathf.Lerp(20f, 12f, ease),
            // Tumsek egimi ~ yukseklik / (0.7 * genislik): 40 dereceyi gecmesin (duvar olmasin), ama firlatsin
            bumpMinHeight = Mathf.Lerp(1.0f, 1.3f, ease),
            bumpMaxHeight = Mathf.Lerp(1.7f, 1.75f, ease),
            bumpWidth = Mathf.Lerp(3.2f, 3.5f, ease),
            pitChance = Mathf.Lerp(0.25f, 0.4f, ease),
            pitMinDepth = Mathf.Lerp(1.5f, 2.0f, ease),
            pitMaxDepth = Mathf.Lerp(2.2f, 2.8f, ease),
            pitWidth = Mathf.Lerp(6.5f, 7.0f, ease),        // ATV'den genis: icine dusulur ama cikilir
            parSpeed = Mathf.Lerp(6.6f, 5.3f, ease),
        };
    }
}

/// <summary>Sahneler arasi tasinan secili seviye.</summary>
public static class LevelSession
{
    public static int Current = 1;
}
