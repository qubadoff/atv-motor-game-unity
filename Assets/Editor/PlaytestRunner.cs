using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Komut satirindan otomatik oynanis testi:
/// -executeMethod PlaytestRunner.Run  (quit vermeden). ATV 12 sn gaz verir, sonuc loglanir, editor kapanir.
/// </summary>
public static class PlaytestRunner
{
    const string Flag = "burngetter_playtest";
    const float Duration = 170f;

    public static void Run()
    {
        SessionState.SetBool(Flag, true);
        string lv = System.Environment.GetEnvironmentVariable("BURN_LEVEL");
        SessionState.SetInt("burngetter_level", string.IsNullOrEmpty(lv) ? 1 : int.Parse(lv));
        SessionState.SetBool("burngetter_dumb", System.Environment.GetEnvironmentVariable("BURN_BOT") == "dumb");
        string scene = System.Environment.GetEnvironmentVariable("BURN_SCENE");
        SessionState.SetBool("burngetter_menu", scene == "Menu");
        EditorSceneManager.OpenScene(scene == "Menu" ? "Assets/Scenes/Menu.unity" : "Assets/Scenes/Game.unity");
        EditorApplication.EnterPlaymode();
    }

    [InitializeOnLoadMethod]
    static void Init()
    {
        if (!SessionState.GetBool(Flag, false)) return;
        LevelSession.Current = SessionState.GetInt("burngetter_level", 1);
        EditorApplication.update += Tick;
        Application.logMessageReceived += OnLog;
    }

    static float startTime = -1f;
    static AtvController atv;
    static float maxX, minY = float.MaxValue;
    static bool failed;
    static float nextReport = 2f;
    static float gameOverAt = -1f;
    static float stuckSince = -1f, reverseUntil = -1f;
    static bool hudChecked; static int gameProblems;
    static int pausePhase; static float pauseX; static float finishedAt = -1f; static int stuckCount;

    static float menuStart = -1f;
    static int menuStep = -1, menuProblems;
    static bool testNameSet;

    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;

        if (SessionState.GetBool("burngetter_menu", false))
        {
            if (menuStart < 0f)
            {
                menuStart = Time.realtimeSinceStartup;
                if (!PlayerProfile.HasName) { PlayerProfile.Name = "Test"; testNameSet = true; }
                Debug.Log("PLAYTEST: menu basladi, dil=" + Loc.Language + " tespit=" + Loc.Detected);
            }
            float e = Time.realtimeSinceStartup - menuStart;
            var menu = Object.FindFirstObjectByType<MenuController>();
            // Her 0.5 sn'de bir adim: her dil x (isim paneli, oyna, ayarlar)
            int step = Mathf.FloorToInt(e / 0.5f);
            if (menu != null && step > menuStep && step < Loc.Languages.Length * 3 + 1)
            {
                if (menuStep >= 0)
                {
                    int prev = menuStep;
                    string lang = Loc.Languages[prev / 3];
                    string page = new[] { "isim", "oyna", "ayarlar" }[prev % 3];
                    menuProblems += UiAudit.Check(lang + "/" + page);
                }
                menuStep = step;
                if (step < Loc.Languages.Length * 3)
                {
                    Loc.Language = Loc.Languages[step / 3];
                    switch (step % 3)
                    {
                        case 0: menu.RebuildUI(); menu.ShowNamePanel(); break;
                        case 1: menu.SetTab(MenuController.Tab.Play); break;
                        case 2: menu.SetTab(MenuController.Tab.Settings); break;
                    }
                }
            }
            if (e > Loc.Languages.Length * 1.5f + 1.5f)
            {
                if (menu != null) menu.SetTab(MenuController.Tab.Play);
                Canvas.ForceUpdateCanvases();
                var canvas = Object.FindFirstObjectByType<UnityEngine.Canvas>();
                int buttons = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Length;
                var music = Object.FindFirstObjectByType<MusicPlayer>();
                Debug.Log("PLAYTEST: menu canvasCocuk=" + (canvas != null ? canvas.transform.childCount : -1) + " dugme=" + buttons
                    + " muzik=" + (music != null && music.tracks != null ? music.tracks.Length : -1) + " uiSorun=" + menuProblems);
                bool ok = !failed && buttons >= 50 && menuProblems == 0;
                Debug.Log(ok ? "PLAYTEST: OK" : "PLAYTEST: FAIL");
                Loc.Language = Loc.Auto;
                if (testNameSet) PlayerPrefs.DeleteKey("player_name");
                PlayerPrefs.Save();
                SessionState.EraseBool(Flag);
                EditorApplication.Exit(ok ? 0 : 1);
            }
            return;
        }

        if (atv == null)
        {
            atv = Object.FindFirstObjectByType<AtvController>();
            if (atv == null) return;
            Debug.Log("PLAYTEST: seviye " + LevelSession.Current);
            atv.useAutoThrottle = true;
            atv.autoThrottle = 1f;
            startTime = Time.realtimeSinceStartup;
            Debug.Log("PLAYTEST: basladi");
        }

        // Basit surucu: burun cok kalkarsa fren; takilip kalirsa geri cekilip hiz al
        float tilt = Mathf.DeltaAngle(0f, atv.body.rotation);
        float now = Time.realtimeSinceStartup;
        if (Mathf.Abs(atv.body.linearVelocity.x) < 0.6f && atv.IsGrounded) { if (stuckSince < 0f) stuckSince = now; }
        else stuckSince = -1f;
        if (reverseUntil < 0f && stuckSince > 0f && now - stuckSince > 1.5f) { reverseUntil = now + 3.5f; stuckSince = -1f; stuckCount++; if (stuckCount <= 3) Debug.Log("PLAYTEST: takildi, geri cekiliyor x=" + atv.transform.position.x.ToString("F1")); }
        bool dumb = SessionState.GetBool("burngetter_dumb", false);
        if (dumb) atv.autoThrottle = 1f;                       // sadece gaz: bunun basarisiz olmasi gerekir
        else if (now < reverseUntil) atv.autoThrottle = -1f;
        else { reverseUntil = -1f; atv.autoThrottle = tilt > 40f ? -0.6f : 1f; }

        maxX = Mathf.Max(maxX, atv.transform.position.x);
        if (gameOverAt < 0f && GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            gameOverAt = Time.realtimeSinceStartup - startTime;
            Debug.Log("PLAYTEST: oyun bitti t=" + gameOverAt.ToString("F1") + " x=" + atv.transform.position.x.ToString("F1"));
        }
        minY = Mathf.Min(minY, atv.transform.position.y);

        float elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed >= nextReport)
        {
            nextReport += 2f;
            Debug.Log("PLAYTEST: t=" + elapsed.ToString("F1") + " pos=" + atv.transform.position + " hiz=" + atv.body.linearVelocity
                + " aci=" + atv.body.rotation.ToString("F0") + " yerde=" + atv.IsGrounded
                + " oyunBitti=" + (GameManager.Instance != null && GameManager.Instance.IsGameOver));
        }
        if (elapsed > 3f && !hudChecked) { hudChecked = true; gameProblems += UiAudit.Check("oyun/hud"); }
        // Duraklatma testi: 5. saniyede duraklat, 6.'da devam et; arada ATV ilerlememeli
        var gmp = GameManager.Instance;
        if (gmp != null && elapsed > 5f && pausePhase == 0) { gmp.Pause(); pauseX = atv.transform.position.x; pausePhase = 1; Debug.Log("PLAYTEST: duraklatildi x=" + pauseX.ToString("F1") + " timeScale=" + Time.timeScale + " panel=" + gmp.pausePanel.activeSelf); }
        if (gmp != null && elapsed > 6f && pausePhase == 1)
        {
            float moved = Mathf.Abs(atv.transform.position.x - pauseX);
            Debug.Log("PLAYTEST: duraklamada hareket=" + moved.ToString("F2") + (moved < 0.05f ? " (OK)" : " (HATA)"));
            if (moved >= 0.05f) gameProblems++;
            gmp.Resume(); pausePhase = 2;
            Debug.Log("PLAYTEST: devam edildi timeScale=" + Time.timeScale + " panel=" + gmp.pausePanel.activeSelf);
        }
        var gmr = GameManager.Instance;
        bool finished = gmr != null && (gmr.IsLevelComplete || gmr.IsGameOver);
        if (finished && finishedAt < 0f) { finishedAt = elapsed; Debug.Log("PLAYTEST: bitis t=" + gmr.Elapsed.ToString("F1") + " tamamlandi=" + gmr.IsLevelComplete + " yildiz=" + (gmr.IsLevelComplete ? gmr.Config.StarsFor(gmr.Elapsed) : 0) + " hedef3=" + gmr.Config.ThreeStarTime.ToString("F0") + " hedef2=" + gmr.Config.TwoStarTime.ToString("F0")); }
        if (elapsed >= Duration || (finishedAt > 0f && elapsed - finishedAt > 1.5f))
        {
            // Panelleri acip metinleri denetle
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.levelCompletePanel.SetActive(false); gm.gameOverPanel.SetActive(false); gm.pausePanel.SetActive(false);
                gm.gameOverPanel.SetActive(true); gm.gameOverText.text = Loc.Get("crashed") + "\n<size=44>123 / 750 m</size>";
                Canvas.ForceUpdateCanvases();
                gameProblems += UiAudit.Check("oyun/devrildi");
                gm.gameOverPanel.SetActive(false);
                gm.levelCompletePanel.SetActive(true); gm.levelCompleteText.text = Loc.Get("level_complete", 12) + "\n<size=44>1:23   " + Loc.Get("new_record") + "</size>";
                Canvas.ForceUpdateCanvases();
                gameProblems += UiAudit.Check("oyun/tamamlandi");
                gm.levelCompletePanel.SetActive(false);
                gm.pausePanel.SetActive(true);
                Canvas.ForceUpdateCanvases();
                gameProblems += UiAudit.Check("oyun/duraklat");
                gm.pausePanel.SetActive(false);
            }
            Debug.Log("PLAYTEST: oyun uiSorun=" + gameProblems + " takilma=" + stuckCount);
            bool over = GameManager.Instance != null && GameManager.Instance.IsGameOver;
            Debug.Log("PLAYTEST: bitti maxX=" + maxX.ToString("F1") + " minY=" + minY.ToString("F1")
                + " son pos=" + atv.transform.position + " oyunBitti=" + over + " timeScale=" + Time.timeScale);
            bool ok = !failed && maxX > 15f && gameProblems == 0;
            Debug.Log("PLAYTEST: seviye=" + (GameManager.Instance != null ? GameManager.Instance.Level : -1)
                + " mesafe=" + (GameManager.Instance != null ? GameManager.Instance.Distance.ToString("F0") : "?"));
            Debug.Log(ok ? "PLAYTEST: OK" : "PLAYTEST: FAIL");
            SessionState.EraseBool(Flag);
            EditorApplication.Exit(ok ? 0 : 1);
        }
    }

    static void OnLog(string msg, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            // Sadece bizim kodumuzdan gelen hatalar testi dusurur (editor ic hatalari haric)
            if (!stack.Contains("Assets/") && !msg.Contains("Assets/")) return;
            failed = true;
            Debug.Log("PLAYTEST: hata yakalandi: " + msg);
        }
    }
}
