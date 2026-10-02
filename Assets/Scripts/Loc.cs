using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>Dort dilli metinler (az, en, ru, tr). Cihaz dili otomatik secilir, ayarlardan degistirilebilir.</summary>
public static class Loc
{
    public static readonly string[] Languages = { "az", "en", "ru", "tr" };
    public static readonly string[] LanguageNames = { "Azərbaycan", "English", "Русский", "Türkçe" };

    public const string Auto = "auto";
    const string Key = "language";
    static string current;
    static string detected;

    /// <summary>Cihaz dilinden tespit edilen dil (az/en/ru/tr).</summary>
    public static string Detected => detected ?? (detected = DetectDeviceLanguage());

    /// <summary>Kullanici ayarlardan dil secmediyse cihaz dili kullanilir.</summary>
    public static bool IsAuto => PlayerPrefs.GetString(Key, Auto) == Auto;

    public static string Language
    {
        get
        {
            if (current == null)
            {
                string saved = PlayerPrefs.GetString(Key, Auto);
                current = System.Array.IndexOf(Languages, saved) >= 0 ? saved : Detected;
            }
            return current;
        }
        set
        {
            if (value == Auto || System.Array.IndexOf(Languages, value) < 0)
            {
                PlayerPrefs.SetString(Key, Auto);
                current = Detected;
            }
            else
            {
                PlayerPrefs.SetString(Key, value);
                current = value;
            }
            PlayerPrefs.Save();
        }
    }

    public static string Get(string key)
    {
        if (table.TryGetValue(key, out var row))
        {
            int i = System.Array.IndexOf(Languages, Language);
            if (i >= 0 && i < row.Length && !string.IsNullOrEmpty(row[i])) return row[i];
            return row[1];
        }
        return key;
    }

    public static string Get(string key, params object[] args) => string.Format(Get(key), args);

    static string DetectDeviceLanguage()
    {
        // Once isletim sistemi yerel ayari (Unity Azerbaycancayi tanimaz, burada yakalanir)
        string iso = "";
        try { iso = CultureInfo.CurrentCulture.TwoLetterISOLanguageName; } catch { }
        try { if (string.IsNullOrEmpty(iso) || iso == "iv") iso = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName; } catch { }
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var locale = new AndroidJavaClass("java.util.Locale"))
            using (var def = locale.CallStatic<AndroidJavaObject>("getDefault"))
                iso = def.Call<string>("getLanguage");
        }
        catch { }
#endif
        if (iso == "az" || iso == "tr" || iso == "ru") return iso;

        switch (Application.systemLanguage)
        {
            case SystemLanguage.Turkish: return "tr";
            case SystemLanguage.Russian: return "ru";
        }
        return "en";
    }

    // sira: az, en, ru, tr
    static readonly Dictionary<string, string[]> table = new Dictionary<string, string[]>
    {
        ["title"]          = new[] { "OLD ATV MOTOR", "OLD ATV MOTOR", "OLD ATV MOTOR", "OLD ATV MOTOR" },
        ["made_by"]        = new[] { "BurnGame tərəfindən hazırlanıb", "Made by BurnGame", "Сделано BurnGame", "BurnGame tarafından hazırlandı" },
        ["copyright"]      = new[] { "© 2026 BurnGame", "© 2026 BurnGame", "© 2026 BurnGame", "© 2026 BurnGame" },
        ["ask_name"]       = new[] { "Adın nədir?", "What's your name?", "Как тебя зовут?", "Adın ne?" },
        ["name_hint"]      = new[] { "bura yaz...", "type here...", "введи здесь...", "buraya yaz..." },
        ["start"]          = new[] { "BAŞLA", "START", "СТАРТ", "BAŞLA" },
        ["tab_play"]       = new[] { "OYNA", "PLAY", "ИГРАТЬ", "OYNA" },
        ["tab_settings"]   = new[] { "AYARLAR", "SETTINGS", "НАСТРОЙКИ", "AYARLAR" },
        ["choose_level"]   = new[] { "SƏVİYYƏ SEÇ", "CHOOSE LEVEL", "ВЫБЕРИ УРОВЕНЬ", "SEVİYE SEÇ" },
        ["hello"]          = new[] { "Salam, {0}", "Hello, {0}", "Привет, {0}", "Merhaba, {0}" },
        ["change_name"]    = new[] { "ADI DƏYİŞ", "CHANGE NAME", "СМЕНИТЬ ИМЯ", "İSMİ DEĞİŞTİR" },
        ["language"]       = new[] { "Dil", "Language", "Язык", "Dil" },
        ["auto"]           = new[] { "Avto", "Auto", "Авто", "Otomatik" },
        ["music"]          = new[] { "Musiqi", "Music", "Музыка", "Müzik" },
        ["sound"]          = new[] { "Səs", "Sound", "Звук", "Ses" },
        ["on"]             = new[] { "AÇIQ", "ON", "ВКЛ", "AÇIK" },
        ["off"]            = new[] { "BAĞLI", "OFF", "ВЫКЛ", "KAPALI" },
        ["level"]          = new[] { "SƏVİYYƏ {0}", "LEVEL {0}", "УРОВЕНЬ {0}", "SEVİYE {0}" },
        ["brake"]          = new[] { "ƏYLƏC", "BRAKE", "ТОРМОЗ", "FREN" },
        ["gas"]            = new[] { "QAZ", "GAS", "ГАЗ", "GAZ" },
        ["crashed"]        = new[] { "AŞDIN", "YOU CRASHED", "ТЫ ПЕРЕВЕРНУЛСЯ", "DEVRİLDİN" },
        ["retry"]          = new[] { "YENİDƏN", "RETRY", "ЗАНОВО", "TEKRAR DENE" },
        ["menu"]           = new[] { "MENYU", "MENU", "МЕНЮ", "MENÜ" },
        ["level_complete"] = new[] { "SƏVİYYƏ {0} TAMAMLANDI", "LEVEL {0} COMPLETE", "УРОВЕНЬ {0} ПРОЙДЕН", "SEVİYE {0} TAMAMLANDI" },
        ["new_record"]     = new[] { "YENİ REKORD", "NEW RECORD", "НОВЫЙ РЕКОРД", "YENİ REKOR" },
        ["best"]           = new[] { "ən yaxşı {0}", "best {0}", "лучшее {0}", "en iyi {0}" },
        ["next_level"]     = new[] { "NÖVBƏTİ", "NEXT LEVEL", "СЛЕДУЮЩИЙ", "SONRAKİ SEVİYE" },
        ["all_done"]       = new[] { "BÜTÜN SƏVİYYƏLƏR TAMAMLANDI!", "ALL LEVELS COMPLETE!", "ВСЕ УРОВНИ ПРОЙДЕНЫ!", "TÜM SEVİYELER TAMAMLANDI!" },
        ["credits"]        = new[] { "Musiqi: Kevin MacLeod (incompetech.com), CC BY 4.0", "Music: Kevin MacLeod (incompetech.com), CC BY 4.0", "Музыка: Kevin MacLeod (incompetech.com), CC BY 4.0", "Müzik: Kevin MacLeod (incompetech.com), CC BY 4.0" },
        ["paused"]         = new[] { "FASİLƏ", "PAUSED", "ПАУЗА", "DURAKLATILDI" },
        ["resume"]         = new[] { "DAVAM ET", "RESUME", "ПРОДОЛЖИТЬ", "DEVAM ET" },
        ["exit"]           = new[] { "ÇIXIŞ", "EXIT", "ВЫХОД", "ÇIKIŞ" },
        ["time"]           = new[] { "VAXT", "TIME", "ВРЕМЯ", "SÜRE" },
        ["goal"]           = new[] { "hədəf", "goal", "цель", "hedef" },
        ["your_time"]      = new[] { "Sənin vaxtın", "Your time", "Твоё время", "Senin süren" },
        ["tap_restart"]    = new[] { "davam üçün toxun", "tap to continue", "нажми, чтобы продолжить", "devam için dokun" },
    };
}
