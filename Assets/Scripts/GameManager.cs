using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Seviye akisi: zamana karsi yaris, HUD, bitis, yildizlar, oyun sonu, duraklatma.</summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referanslar")]
    public AtvController atv;
    public TerrainGenerator terrain;
    public QuoteDisplay quotes;
    public Sprite flagSprite;
    public Sprite starOn;
    public Sprite starOff;

    [Header("HUD")]
    public Text levelText;
    public Text distanceText;
    public Text timerText;
    public Image[] goalStars;
    public Text goalTimeText;
    public Text infoText;
    public Text brakeHint;
    public Text gasHint;

    [Header("Oyun sonu paneli")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Button retryButton;
    public Button gameOverMenuButton;

    [Header("Seviye tamamlandi paneli")]
    public GameObject levelCompletePanel;
    public Text levelCompleteText;
    public Image[] resultStars;
    public Text resultTimeText;
    public Text goal3Text;
    public Text goal2Text;
    public Button completeRetryButton;
    public Button nextButton;
    public Button completeMenuButton;

    [Header("Duraklatma")]
    public Button pauseButton;
    public GameObject pausePanel;
    public Text pauseText;
    public Button resumeButton;
    public Button exitButton;

    public float restartDelay = 0.8f;
    public bool IsPaused { get; private set; }
    public bool IsGameOver => state == State.GameOver;
    public bool IsLevelComplete => state == State.Complete;
    public float Distance => distance;
    public float Elapsed => elapsed;
    public int Level => config.level;
    public LevelConfig Config => config;

    enum State { Playing, GameOver, Complete }
    State state = State.Playing;

    LevelConfig config;
    float startX, distance, elapsed, endedAt;
    static readonly float[] quoteAt = { 0.10f, 0.33f, 0.56f, 0.80f };
    int nextQuote;

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        Time.timeScale = 1f;
    }

    void Start()
    {
        config = Levels.Get(LevelSession.Current);
        terrain.ApplyLevel(config);
        startX = atv.transform.position.x;
        SpawnFinish();

        SetActive(gameOverPanel, false);
        SetActive(levelCompletePanel, false);
        SetActive(pausePanel, false);

        SetText(levelText, Loc.Get("level", config.level));
        SetText(brakeHint, Loc.Get("brake"));
        SetText(gasHint, Loc.Get("gas"));
        SetText(goalTimeText, FormatTime(config.ThreeStarTime));
        SetText(pauseText, Loc.Get("paused"));
        SetText(goal3Text, FormatTime(config.ThreeStarTime));
        SetText(goal2Text, FormatTime(config.TwoStarTime));
        SetLabel(retryButton, Loc.Get("retry"));
        SetLabel(gameOverMenuButton, Loc.Get("menu"));
        SetLabel(completeRetryButton, Loc.Get("retry"));
        SetLabel(nextButton, Loc.Get("next_level"));
        SetLabel(completeMenuButton, Loc.Get("menu"));
        SetLabel(resumeButton, Loc.Get("resume"));
        SetLabel(exitButton, Loc.Get("exit"));

        Listen(retryButton, Retry);
        Listen(gameOverMenuButton, GoToMenu);
        Listen(completeRetryButton, Retry);
        Listen(nextButton, NextLevel);
        Listen(completeMenuButton, GoToMenu);
        Listen(pauseButton, Pause);
        Listen(resumeButton, Resume);
        Listen(exitButton, GoToMenu);
        UpdateHud();
    }

    void SpawnFinish()
    {
        float x = config.length;
        float y = terrain.HeightAt(x);
        var flag = new GameObject("FinishFlag");
        flag.transform.position = new Vector3(x, y, 0f);
        var sr = flag.AddComponent<SpriteRenderer>();
        sr.sprite = flagSprite;
        sr.sortingOrder = 5;

        var trigger = new GameObject("FinishTrigger");
        trigger.transform.position = new Vector3(x, y + 5f, 0f);
        var box = trigger.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(0.5f, 30f);
        trigger.AddComponent<FinishLine>();
    }

    void Update()
    {
        if (IsPaused)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Resume();
            return;
        }
        if (state == State.Playing)
        {
            elapsed += Time.deltaTime;
            distance = Mathf.Clamp(atv.transform.position.x - startX, 0f, config.length);
            UpdateHud();

            if (quotes != null && nextQuote < quoteAt.Length && distance >= config.length * quoteAt[nextQuote])
            {
                nextQuote++;
                quotes.ShowRandom();
            }
            if (atv.transform.position.y < -25f) GameOver();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) Pause();
        }
        else if (Time.unscaledTime - endedAt > restartDelay)
        {
            if (state == State.GameOver && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return))) Retry();
            if (state == State.Complete && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))) NextLevel();
            if (state == State.Complete && Input.GetKeyDown(KeyCode.R)) Retry();
            if (Input.GetKeyDown(KeyCode.Escape)) GoToMenu();
        }
    }

    void UpdateHud()
    {
        SetText(distanceText, Mathf.FloorToInt(distance) + " / " + Mathf.FloorToInt(config.length) + " m");
        SetText(timerText, FormatTime(elapsed));
        SetText(infoText, PlayerProfile.Name);
        // Canli hedef: sure gectikce yildizlar soner
        int onPace = elapsed <= config.ThreeStarTime ? 3 : elapsed <= config.TwoStarTime ? 2 : 1;
        SetStars(goalStars, onPace);
        SetText(goalTimeText, onPace == 3 ? FormatTime(config.ThreeStarTime) : onPace == 2 ? FormatTime(config.TwoStarTime) : "");
    }

    public static string FormatTime(float s)
    {
        int m = Mathf.FloorToInt(s / 60f);
        return m + ":" + Mathf.FloorToInt(s % 60f).ToString("00");
    }

    public void GameOver()
    {
        if (state != State.Playing) return;
        state = State.GameOver;
        endedAt = Time.unscaledTime;
        atv.ControlsEnabled = false;
        SetActive(pauseButton != null ? pauseButton.gameObject : null, false);
        SetActive(gameOverPanel, true);
        SetText(gameOverText, Loc.Get("crashed") + "\n<size=44>" + Mathf.FloorToInt(distance) + " / " + Mathf.FloorToInt(config.length) + " m   " + FormatTime(elapsed) + "</size>");
        Time.timeScale = 0.35f;
    }

    public void LevelComplete()
    {
        if (state != State.Playing) return;
        state = State.Complete;
        endedAt = Time.unscaledTime;
        atv.ControlsEnabled = false;
        SetActive(pauseButton != null ? pauseButton.gameObject : null, false);

        int stars = config.StarsFor(elapsed);
        bool record = PlayerProfile.ReportTime(config.level, elapsed);
        PlayerProfile.ReportStars(config.level, stars);
        if (config.level < Levels.Count) PlayerProfile.UnlockUpTo(config.level + 1);

        SetActive(levelCompletePanel, true);
        string title = config.level >= Levels.Count ? Loc.Get("all_done") : Loc.Get("level_complete", config.level);
        SetText(levelCompleteText, title + (record ? "\n<size=40>" + Loc.Get("new_record") + "</size>" : ""));
        SetText(resultTimeText, Loc.Get("your_time") + "  " + FormatTime(elapsed)
            + "   <color=#00000099>" + Loc.Get("best", FormatTime(PlayerProfile.BestTime(config.level))) + "</color>");
        if (nextButton != null) nextButton.gameObject.SetActive(config.level < Levels.Count);
        SetStars(resultStars, 0);
        StartCoroutine(PopStars(stars));
        Time.timeScale = 0.35f;
    }

    IEnumerator PopStars(int count)
    {
        if (resultStars == null) yield break;
        yield return new WaitForSecondsRealtime(0.3f);
        for (int i = 0; i < resultStars.Length; i++)
        {
            var img = resultStars[i];
            if (img == null) continue;
            bool on = i < count;
            img.sprite = on ? starOn : starOff;
            if (!on) continue;
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / 0.35f);
                float scale = 1f + Mathf.Sin(k * Mathf.PI) * 0.5f;   // pat diye buyu-kucul
                img.rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }
            img.rectTransform.localScale = Vector3.one;
            yield return new WaitForSecondsRealtime(0.15f);
        }
    }

    void SetStars(Image[] stars, int count)
    {
        if (stars == null) return;
        for (int i = 0; i < stars.Length; i++)
            if (stars[i] != null) stars[i].sprite = i < count ? starOn : starOff;
    }

    public void Pause()
    {
        if (state != State.Playing || IsPaused) return;
        IsPaused = true;
        atv.ControlsEnabled = false;
        Time.timeScale = 0f;
        SetActive(pausePanel, true);
        SetActive(pauseButton != null ? pauseButton.gameObject : null, false);
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        SetActive(pausePanel, false);
        SetActive(pauseButton != null ? pauseButton.gameObject : null, true);
        Time.timeScale = 1f;
        StartCoroutine(EnableControlsNextFrame());
    }

    IEnumerator EnableControlsNextFrame()
    {
        yield return null;
        if (!IsPaused && state == State.Playing) atv.ControlsEnabled = true;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void NextLevel()
    {
        if (config.level >= Levels.Count) { GoToMenu(); return; }
        LevelSession.Current = config.level + 1;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    static void SetText(Text t, string value) { if (t != null) t.text = value; }
    static void SetActive(GameObject go, bool on) { if (go != null) go.SetActive(on); }
    static void SetLabel(Button b, string value) { if (b != null) { var t = b.GetComponentInChildren<Text>(); if (t != null) t.text = value; } }
    static void Listen(Button b, UnityEngine.Events.UnityAction a) { if (b != null) b.onClick.AddListener(a); }
}
