using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Ana menu: ilk aciliste isim sorar; sonra OYNA (seviye secimi) ve AYARLAR (dil, muzik, ses) sekmeleri.
/// Tum arayuz kodla kurulur ve dil degisince yeniden cizilir.
/// </summary>
public class MenuController : MonoBehaviour
{
    public Canvas canvas;
    public Font font;
    public Sprite lockSprite;
    public Sprite starOn;
    public Sprite starOff;

    static readonly Color Ink = Color.black;
    static readonly Color Paper = Color.white;
    static readonly Color Faded = new Color(0f, 0f, 0f, 0.4f);

    public enum Tab { Play, Settings }
    Tab tab = Tab.Play;

    /// <summary>Test ve dis erisim: sekmeyi secip arayuzu yeniden kurar.</summary>
    public void SetTab(Tab t) { tab = t; Rebuild(); }
    public void ShowNamePanel() { ShowNamePrompt(); }
    public void RebuildUI() { Rebuild(); }

    GameObject namePanel;
    GameObject mainPanel;
    InputField nameInput;

    void Start()
    {
        Time.timeScale = 1f;
        Rebuild();
    }

    void Update()
    {
        if (namePanel != null && namePanel.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            ConfirmName();
    }

    void Rebuild()
    {
        for (int i = canvas.transform.childCount - 1; i >= 0; i--) Destroy(canvas.transform.GetChild(i).gameObject);
        BuildNamePanel();
        BuildMainPanel();
        if (PlayerProfile.HasName) ShowMain(); else ShowNamePrompt();
    }

    // ------------------------------------------------------------ isim
    void BuildNamePanel()
    {
        namePanel = Panel("NamePanel");
        var title = MakeText(namePanel.transform, "Title", Loc.Get("title"), 120, new Vector2(0.5f, 0.5f), new Vector2(0f, 230f), new Vector2(1400f, 150f));
        title.fontStyle = FontStyle.Bold;
        MakeText(namePanel.transform, "Question", Loc.Get("ask_name"), 54, new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(1000f, 80f));

        var inputGo = new GameObject("NameInput");
        inputGo.transform.SetParent(namePanel.transform, false);
        var inputRt = inputGo.AddComponent<RectTransform>();
        inputRt.anchorMin = inputRt.anchorMax = inputRt.pivot = new Vector2(0.5f, 0.5f);
        inputRt.anchoredPosition = Vector2.zero;
        inputRt.sizeDelta = new Vector2(700f, 100f);
        inputGo.AddComponent<Image>().color = Paper;
        AddOutline(inputGo, 4f);

        var placeholder = MakeText(inputGo.transform, "Placeholder", Loc.Get("name_hint"), 48, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660f, 90f));
        placeholder.color = Faded;
        placeholder.fontStyle = FontStyle.Italic;
        var text = MakeText(inputGo.transform, "Text", "", 48, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660f, 90f));
        text.supportRichText = false;

        nameInput = inputGo.AddComponent<InputField>();
        nameInput.textComponent = text;
        nameInput.placeholder = placeholder;
        nameInput.characterLimit = 16;
        nameInput.caretColor = Ink;
        nameInput.selectionColor = new Color(0f, 0f, 0f, 0.2f);

        MakeButton(namePanel.transform, "StartButton", Loc.Get("start"), 52, new Vector2(0f, -120f), new Vector2(360f, 96f)).onClick.AddListener(ConfirmName);

        // Isim ekraninda da dil secilebilsin
        BuildLanguageRow(namePanel.transform, new Vector2(0f, -270f), 28);
        MakeText(namePanel.transform, "MadeBy", Loc.Get("made_by"), 26, new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(1000f, 36f)).color = Faded;
    }

    void ShowNamePrompt()
    {
        mainPanel.SetActive(false);
        namePanel.SetActive(true);
        nameInput.text = PlayerProfile.Name;
        nameInput.ActivateInputField();
        nameInput.Select();
    }

    void ConfirmName()
    {
        string name = nameInput.text.Trim();
        if (name.Length == 0) { nameInput.ActivateInputField(); return; }
        PlayerProfile.Name = name;
        ShowMain();
    }

    // ------------------------------------------------------------ ana panel + sekmeler
    void BuildMainPanel()
    {
        mainPanel = Panel("MainPanel");

        var title = MakeText(mainPanel.transform, "Title", Loc.Get("title"), 64, new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(900f, 76f));
        title.fontStyle = FontStyle.Bold;

        var greeting = MakeText(mainPanel.transform, "Greeting", Loc.Get("hello", PlayerProfile.Name), 32, new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(900f, 44f));
        greeting.color = Faded;

        // Sekmeler (ortada, baslik altinda)
        var playTab = MakeButton(mainPanel.transform, "TabPlay", Loc.Get("tab_play"), 38, new Vector2(-170f, -150f), new Vector2(320f, 80f));
        var settingsTab = MakeButton(mainPanel.transform, "TabSettings", Loc.Get("tab_settings"), 38, new Vector2(170f, -150f), new Vector2(320f, 80f));
        foreach (var b in new[] { playTab, settingsTab })
        {
            var rt = b.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
        }
        StyleTab(playTab, tab == Tab.Play);
        StyleTab(settingsTab, tab == Tab.Settings);
        playTab.onClick.AddListener(() => { tab = Tab.Play; Rebuild(); });
        settingsTab.onClick.AddListener(() => { tab = Tab.Settings; Rebuild(); });

        var change = MakeButton(mainPanel.transform, "ChangeName", Loc.Get("change_name"), 26, new Vector2(-30f, -30f), new Vector2(300f, 60f));
        var changeRt = change.GetComponent<RectTransform>();
        changeRt.anchorMin = changeRt.anchorMax = changeRt.pivot = new Vector2(1f, 1f);
        change.onClick.AddListener(ShowNamePrompt);

        if (tab == Tab.Play) BuildLevelGrid(mainPanel.transform);
        else BuildSettings(mainPanel.transform);

        var madeBy = MakeText(mainPanel.transform, "MadeBy", Loc.Get("made_by") + "   " + Loc.Get("copyright"), 22, new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(1100f, 30f));
        madeBy.color = Faded;
    }

    void StyleTab(Button b, bool active)
    {
        b.GetComponent<Image>().color = active ? Ink : Paper;
        b.GetComponentInChildren<Text>().color = active ? Paper : Ink;
        b.interactable = !active;
    }

    void ShowMain()
    {
        namePanel.SetActive(false);
        mainPanel.SetActive(true);
        var g = mainPanel.transform.Find("Greeting");
        if (g != null) g.GetComponent<Text>().text = Loc.Get("hello", PlayerProfile.Name);
    }

    // ------------------------------------------------------------ seviyeler
    void BuildLevelGrid(Transform parent)
    {
        var sub = MakeText(parent, "Subtitle", Loc.Get("choose_level"), 34, new Vector2(0.5f, 1f), new Vector2(0f, -246f), new Vector2(1000f, 46f));
        sub.color = Faded;

        var gridGo = new GameObject("Grid");
        gridGo.transform.SetParent(parent, false);
        var gridRt = gridGo.AddComponent<RectTransform>();
        gridRt.anchorMin = gridRt.anchorMax = gridRt.pivot = new Vector2(0.5f, 0.5f);
        gridRt.anchorMin = gridRt.anchorMax = gridRt.pivot = new Vector2(0.5f, 1f);
        gridRt.anchoredPosition = new Vector2(0f, -300f);
        gridRt.sizeDelta = new Vector2(1400f, 600f);
        var layout = gridGo.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(122f, 96f);
        layout.spacing = new Vector2(16f, 18f);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;

        for (int i = 1; i <= Levels.Count; i++) BuildLevelButton(gridGo.transform, i);
    }

    void BuildLevelButton(Transform grid, int level)
    {
        var go = new GameObject("Level" + level);
        go.transform.SetParent(grid, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        var outline = AddOutline(go, 4f);
        var btn = go.AddComponent<Button>();
        var btnColors = btn.colors;
        btnColors.disabledColor = Color.white;
        btnColors.highlightedColor = new Color(0.88f, 0.88f, 0.88f);
        btnColors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = btnColors;
        var label = MakeText(go.transform, "Label", level.ToString(), 40, new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(120f, 48f));
        label.fontStyle = FontStyle.Bold;
        var best = MakeText(go.transform, "Best", "", 18, new Vector2(0.5f, 0.5f), new Vector2(0f, -34f), new Vector2(120f, 24f));

        int stars = PlayerProfile.Stars(level);
        var starsGo = new GameObject("Stars");
        starsGo.transform.SetParent(go.transform, false);
        var starsRt = starsGo.AddComponent<RectTransform>();
        starsRt.anchorMin = starsRt.anchorMax = starsRt.pivot = new Vector2(0.5f, 0.5f);
        starsRt.anchoredPosition = new Vector2(0f, -10f);
        starsRt.sizeDelta = new Vector2(84f, 24f);
        var starsLayout = starsGo.AddComponent<HorizontalLayoutGroup>();
        starsLayout.spacing = 4f;
        starsLayout.childAlignment = TextAnchor.MiddleCenter;
        starsLayout.childControlWidth = starsLayout.childControlHeight = false;
        starsLayout.childForceExpandWidth = starsLayout.childForceExpandHeight = false;
        for (int i = 0; i < 3; i++)
        {
            var st = new GameObject("Star" + i);
            st.transform.SetParent(starsGo.transform, false);
            st.AddComponent<RectTransform>().sizeDelta = new Vector2(24f, 24f);
            var si = st.AddComponent<Image>();
            si.sprite = i < stars ? starOn : starOff;
            si.raycastTarget = false;
        }

        var lockGo = new GameObject("Lock");
        lockGo.transform.SetParent(go.transform, false);
        var lockRt = lockGo.AddComponent<RectTransform>();
        lockRt.anchorMin = lockRt.anchorMax = lockRt.pivot = new Vector2(0.5f, 0.5f);
        lockRt.anchoredPosition = new Vector2(0f, -2f);
        lockRt.sizeDelta = new Vector2(40f, 48f);
        var lockImg = lockGo.AddComponent<Image>();
        lockImg.sprite = lockSprite;
        lockImg.preserveAspect = true;
        lockImg.raycastTarget = false;

        bool unlocked = level <= PlayerProfile.UnlockedLevel;
        img.color = unlocked ? Paper : new Color(0.94f, 0.94f, 0.94f);
        outline.effectColor = unlocked ? Ink : new Color(0f, 0f, 0f, 0.25f);
        label.gameObject.SetActive(unlocked);
        starsGo.SetActive(unlocked);
        lockGo.SetActive(!unlocked);
        btn.interactable = unlocked;

        float bestTime = PlayerProfile.BestTime(level);
        best.text = unlocked && bestTime > 0f ? GameManager.FormatTime(bestTime) : "";
        best.color = Faded;

        int captured = level;
        btn.onClick.AddListener(() => StartLevel(captured));
    }

    void StartLevel(int level)
    {
        LevelSession.Current = level;
        SceneManager.LoadScene("Game");
    }

    // ------------------------------------------------------------ ayarlar
    void BuildSettings(Transform parent)
    {
        float y = -262f;
        MakeText(parent, "LangLabel", Loc.Get("language"), 36, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(800f, 44f)).color = Faded;
        BuildLanguageRow(parent, new Vector2(0f, y - 52f), 28, new Vector2(0.5f, 1f));

        y -= 190f;
        MakeText(parent, "MusicLabel", Loc.Get("music"), 36, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(800f, 44f)).color = Faded;
        var music = MakeToggle(parent, "MusicToggle", Settings.MusicOn, new Vector2(0f, y - 52f));
        music.onClick.AddListener(() => { Settings.MusicOn = !Settings.MusicOn; Rebuild(); });

        y -= 170f;
        MakeText(parent, "SoundLabel", Loc.Get("sound"), 36, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(800f, 44f)).color = Faded;
        var sound = MakeToggle(parent, "SoundToggle", Settings.SoundOn, new Vector2(0f, y - 52f));
        sound.onClick.AddListener(() => { Settings.SoundOn = !Settings.SoundOn; Rebuild(); });

        var credits = MakeText(parent, "Credits", Loc.Get("credits"), 22, new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(1100f, 30f));
        credits.color = Faded;
    }

    void BuildLanguageRow(Transform parent, Vector2 pos, int fontSize, Vector2? anchor = null)
    {
        var row = new GameObject("Languages");
        row.transform.SetParent(parent, false);
        var rt = row.AddComponent<RectTransform>();
        var a = anchor ?? new Vector2(0.5f, 0.5f);
        rt.anchorMin = rt.anchorMax = a;
        rt.pivot = new Vector2(0.5f, anchor.HasValue ? 1f : 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(1200f, 72f);
        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        // Auto (cihaz dili) + 4 dil
        var auto = MakeButton(row.transform, "Lang_auto", Loc.Get("auto"), fontSize, Vector2.zero, new Vector2(220f, 64f));
        Highlight(auto, Loc.IsAuto);
        auto.onClick.AddListener(() => { Loc.Language = Loc.Auto; Rebuild(); });

        for (int i = 0; i < Loc.Languages.Length; i++)
        {
            string code = Loc.Languages[i];
            var b = MakeButton(row.transform, "Lang_" + code, Loc.LanguageNames[i], fontSize, Vector2.zero, new Vector2(220f, 64f));
            Highlight(b, !Loc.IsAuto && Loc.Language == code);
            b.onClick.AddListener(() => { Loc.Language = code; Rebuild(); });
        }
    }

    static void Highlight(Button b, bool active)
    {
        b.GetComponent<Image>().color = active ? Ink : Paper;
        b.GetComponentInChildren<Text>().color = active ? Paper : Ink;
    }

    Button MakeToggle(Transform parent, string name, bool on, Vector2 pos)
    {
        var b = MakeButton(parent, name, Loc.Get(on ? "on" : "off"), 32, pos, new Vector2(240f, 64f));
        var rt = b.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        Highlight(b, on);
        return b;
    }

    // ------------------------------------------------------------ yardimcilar
    GameObject Panel(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = Paper;
        go.SetActive(false);
        return go;
    }

    Text MakeText(Transform parent, string name, string content, int size, Vector2 anchor, Vector2 pos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        var t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.text = content;
        t.color = Ink;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    Button MakeButton(Transform parent, string name, string label, int fontSize, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = Paper;
        AddOutline(go, 4f);
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.88f, 0.88f, 0.88f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        colors.disabledColor = Color.white;
        btn.colors = colors;
        var t = MakeText(go.transform, "Label", label, fontSize, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        t.fontStyle = FontStyle.Bold;
        return btn;
    }

    static Outline AddOutline(GameObject go, float width)
    {
        var o = go.AddComponent<Outline>();
        o.effectColor = Ink;
        o.effectDistance = new Vector2(width, -width);
        o.useGraphicAlpha = false;
        return o;
    }
}
