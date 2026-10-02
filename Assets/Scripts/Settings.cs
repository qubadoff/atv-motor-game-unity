using UnityEngine;

/// <summary>Muzik ve ses acik/kapali ayarlari (PlayerPrefs).</summary>
public static class Settings
{
    public static bool MusicOn
    {
        get => PlayerPrefs.GetInt("music_on", 1) == 1;
        set { PlayerPrefs.SetInt("music_on", value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static bool SoundOn
    {
        get => PlayerPrefs.GetInt("sound_on", 1) == 1;
        set { PlayerPrefs.SetInt("sound_on", value ? 1 : 0); PlayerPrefs.Save(); }
    }
}
