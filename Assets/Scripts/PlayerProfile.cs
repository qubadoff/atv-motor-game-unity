using UnityEngine;

/// <summary>Oyuncu adi ve ilerleme (PlayerPrefs).</summary>
public static class PlayerProfile
{
    const string NameKey = "player_name";
    const string UnlockedKey = "unlocked_level";

    public static string Name
    {
        get => PlayerPrefs.GetString(NameKey, "");
        set { PlayerPrefs.SetString(NameKey, value.Trim()); PlayerPrefs.Save(); }
    }

    public static bool HasName => !string.IsNullOrWhiteSpace(Name);

    /// <summary>Acik olan en yuksek seviye (1..50).</summary>
    public static int UnlockedLevel
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(UnlockedKey, 1), 1, Levels.Count);
        set { PlayerPrefs.SetInt(UnlockedKey, Mathf.Clamp(value, 1, Levels.Count)); PlayerPrefs.Save(); }
    }

    public static void UnlockUpTo(int level)
    {
        if (level > UnlockedLevel) UnlockedLevel = level;
    }

    public static int Stars(int level) => PlayerPrefs.GetInt("stars_" + level, 0);

    /// <summary>Daha fazla yildiz ise kaydeder.</summary>
    public static void ReportStars(int level, int stars)
    {
        if (stars > Stars(level)) { PlayerPrefs.SetInt("stars_" + level, stars); PlayerPrefs.Save(); }
    }

    public static float BestTime(int level) => PlayerPrefs.GetFloat("best_time_" + level, 0f);

    /// <summary>Daha iyi bir sure ise kaydeder; rekor kirildiysa true doner.</summary>
    public static bool ReportTime(int level, float seconds)
    {
        float best = BestTime(level);
        if (best > 0f && seconds >= best) return false;
        PlayerPrefs.SetFloat("best_time_" + level, seconds);
        PlayerPrefs.Save();
        return true;
    }
}
