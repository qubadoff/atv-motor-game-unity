using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu ve Oyun sahnelerini, sprite'lari ve materyalleri sifirdan kurar.
/// Menu: BurnGetter > Build Game Scenes  veya  komut satiri: -executeMethod SceneBuilder.Build
/// </summary>
public static class SceneBuilder
{
    const string GameScenePath = "Assets/Scenes/Game.unity";
    const string MenuScenePath = "Assets/Scenes/Menu.unity";
    const string SplashScenePath = "Assets/Scenes/Splash.unity";

    static Font Font => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    static Sprite[] pedalSprites;
    static Sprite pauseSprite, starOn, starOff;

    [MenuItem("BurnGetter/Build Game Scenes")]
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Materials");

        SpriteFactory.GenerateAll();
        Sprite atvSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/atv.png");
        Sprite wheelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/wheel.png");
        Sprite flagSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/flag.png");
        Sprite lockSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/lock.png");
        Sprite cloudSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cloud.png");
        Sprite[] birdSprites =
        {
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bird_0.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bird_1.png"),
        };
        AudioClip[] music = ImportMusic();
        Sprite pedalFill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pedal_fill.png");
        Sprite pedalOutline = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pedal_outline.png");
        Sprite arrow = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/arrow.png");
        pedalSprites = new[] { pedalFill, pedalOutline, arrow };
        pauseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pause.png");
        starOn = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/star_on.png");
        starOff = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/star_off.png");

        Material groundMat = MakeGroundMaterial();
        PhysicsMaterial2D groundPhys = MakePhysicsMaterial("Assets/Materials/GroundPhysics.physicsMaterial2D", 0.9f);
        PhysicsMaterial2D wheelPhys = MakePhysicsMaterial("Assets/Materials/WheelPhysics.physicsMaterial2D", 1.3f);

        Sprite logo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/logo.png");
        BuildSplashScene(logo, music);
        BuildMenuScene(lockSprite, music);
        BuildGameScene(atvSprite, wheelSprite, flagSprite, cloudSprite, birdSprites, groundMat, groundPhys, wheelPhys);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(SplashScenePath, true),
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true),
        };

        PlayerSettings.productName = "Old Atv Motor";
        PlayerSettings.companyName = "BurnGame";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        PlayerSettings.Android.forceSDCardPermission = false;
        PlayerSettings.SplashScreen.show = false;              // Unity 6: kendi acilis sahnemiz var
        PlayerSettings.SplashScreen.showUnityLogo = false;
        var iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/logo.png");
        if (iconTex != null) PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { iconTex }, IconKind.Any);
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        // atv-motor.burngame.org -> ters alan adi. Android paket adinda tire olamaz.
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "org.burngame.atv-motor");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "org.burngame.atvmotor");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "org.burngame.atvmotor");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("BurnGetter: sahneler kuruldu -> " + MenuScenePath + ", " + GameScenePath);
    }

    // ==================================================================
    static AudioClip[] ImportMusic()
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio/Music" });
        var clips = new System.Collections.Generic.List<AudioClip>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.4f;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
            clips.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(path));
        }
        return clips.ToArray();
    }

    static void BuildSplashScene(Sprite logo, AudioClip[] music)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.white;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        Canvas canvas = MakeCanvas();
        var groupGo = new GameObject("Group");
        groupGo.transform.SetParent(canvas.transform, false);
        var groupRt = groupGo.AddComponent<RectTransform>();
        groupRt.anchorMin = Vector2.zero; groupRt.anchorMax = Vector2.one;
        groupRt.offsetMin = groupRt.offsetMax = Vector2.zero;
        var group = groupGo.AddComponent<CanvasGroup>();

        Image logoImg = MakeImage(groupGo.transform, "Logo", logo, new Vector2(380f, 380f), new Vector2(0f, 120f));
        logoImg.preserveAspect = true;
        Text title = MakeText(groupGo.transform, "Title", 120, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -170f), new Vector2(1600f, 150f), Color.black);
        title.fontStyle = FontStyle.Bold;
        title.text = "OLD ATV MOTOR";
        Text madeBy = MakeText(groupGo.transform, "MadeBy", 40, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -270f), new Vector2(1400f, 60f), new Color(0f, 0f, 0f, 0.6f));
        madeBy.text = "Made by BurnGame";

        var splash = new GameObject("Splash").AddComponent<SplashController>();
        splash.group = group;
        splash.logo = logoImg.rectTransform;
        splash.title = title;
        splash.madeBy = madeBy;

        var musicGo = new GameObject("MusicPlayer");
        musicGo.AddComponent<MusicPlayer>().tracks = music;

        EditorSceneManager.SaveScene(scene, SplashScenePath);
    }

    static void BuildMenuScene(Sprite lockSprite, AudioClip[] music)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.white;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        Canvas canvas = MakeCanvas();
        MakeEventSystem();

        var menuGo = new GameObject("Menu");
        var menu = menuGo.AddComponent<MenuController>();
        menu.canvas = canvas;
        menu.font = Font;
        menu.lockSprite = lockSprite;
        menu.starOn = starOn;
        menu.starOff = starOff;

        var musicGo = new GameObject("MusicPlayer");
        musicGo.AddComponent<MusicPlayer>().tracks = music;

        EditorSceneManager.SaveScene(scene, MenuScenePath);
    }

    // ==================================================================
    static void BuildGameScene(Sprite atvSprite, Sprite wheelSprite, Sprite flagSprite, Sprite cloudSprite, Sprite[] birdSprites,
        Material groundMat, PhysicsMaterial2D groundPhys, PhysicsMaterial2D wheelPhys)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---------- Kamera ----------
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.white;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        camGo.transform.position = new Vector3(3f, 2f, -10f);
        camGo.AddComponent<AudioListener>();
        var follow = camGo.AddComponent<CameraFollow>();

        // ---------- ATV ----------
        int w = SpriteFactory.AtvW, h = SpriteFactory.AtvH;
        Vector2 rearPos = SpriteFactory.PxToLocal(SpriteFactory.RearWheelPx, w, h);
        Vector2 frontPos = SpriteFactory.PxToLocal(SpriteFactory.FrontWheelPx, w, h);
        Vector2 helmetPos = SpriteFactory.PxToLocal(SpriteFactory.HelmetPx, w, h);
        float wheelRadius = SpriteFactory.WheelRadiusPx / (float)SpriteFactory.PPU;

        var atvGo = new GameObject("ATV");
        atvGo.transform.position = new Vector3(2f, wheelRadius - rearPos.y + 0.1f, 0f);

        var body = atvGo.AddComponent<Rigidbody2D>();
        body.mass = 5f;
        body.linearDamping = 0.05f;
        body.angularDamping = 1.0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        var bodySr = atvGo.AddComponent<SpriteRenderer>();
        bodySr.sprite = atvSprite;
        bodySr.sortingOrder = 10;

        // Govde carpisma kutulari (piksel -> yerel birim)
        AddBox(atvGo, PxRect(8, 66, 132, 120, w, h), groundPhys);    // arka camurluk
        AddBox(atvGo, PxRect(158, 66, 292, 132, w, h), groundPhys);  // on camurluk + burun
        AddBox(atvGo, PxRect(98, 50, 200, 62, w, h), groundPhys);    // ayak tabani
        AddBox(atvGo, PxRect(96, 118, 216, 156, w, h), null);        // sele + depo
        AddBox(atvGo, PxRect(96, 132, 134, 198, w, h), null);        // surucu govdesi

        // Kafa (oyun sonu tetigi)
        var head = new GameObject("Head");
        head.transform.SetParent(atvGo.transform, false);
        head.transform.localPosition = helmetPos;
        var headCol = head.AddComponent<CircleCollider2D>();
        headCol.radius = SpriteFactory.HelmetRadiusPx / (float)SpriteFactory.PPU;
        headCol.isTrigger = true;
        head.AddComponent<HeadTrigger>();

        var rear = MakeWheel("RearWheel", atvGo.transform, rearPos, wheelSprite, wheelPhys, wheelRadius);
        var front = MakeWheel("FrontWheel", atvGo.transform, frontPos, wheelSprite, wheelPhys, wheelRadius);
        var rearJoint = MakeWheelJoint(body, rear, rearPos);
        var frontJoint = MakeWheelJoint(body, front, frontPos);

        var controller = atvGo.AddComponent<AtvController>();
        controller.body = body;
        controller.rearWheel = rearJoint;
        controller.frontWheel = frontJoint;

        var audioSource = atvGo.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        var engine = atvGo.AddComponent<EngineAudio>();
        engine.atv = controller;

        follow.target = atvGo.transform;
        follow.targetBody = body;

        // ---------- Gokyuzu suslemeleri ve sozler ----------
        var skyGo = new GameObject("Sky");
        var sky = skyGo.AddComponent<SkyDecor>();
        sky.cam = cam;
        sky.cloudSprite = cloudSprite;
        sky.birdFrames = birdSprites;

        var quotesGo = new GameObject("Quotes");
        var quotes = quotesGo.AddComponent<QuoteDisplay>();
        quotes.cam = cam;
        quotes.font = Font;

        // ---------- Arazi ----------
        var terrainGo = new GameObject("Terrain");
        var gen = terrainGo.AddComponent<TerrainGenerator>();
        gen.target = atvGo.transform;
        gen.groundMaterial = groundMat;
        gen.groundPhysics = groundPhys;

        // ---------- UI ----------
        Canvas canvas = MakeCanvas();
        MakeEventSystem();
        var safeGo = new GameObject("SafeArea");
        safeGo.transform.SetParent(canvas.transform, false);
        var safeRt = safeGo.AddComponent<RectTransform>();
        safeRt.anchorMin = Vector2.zero; safeRt.anchorMax = Vector2.one;
        safeRt.offsetMin = safeRt.offsetMax = Vector2.zero;
        safeGo.AddComponent<SafeArea>();
        Transform ui = safeGo.transform;

        Text level = MakeText(ui, "Level", 56, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(40f, -24f), new Vector2(420f, 70f), Color.black);
        level.fontStyle = FontStyle.Bold;
        level.text = "LEVEL 1";
        Text distance = MakeText(ui, "Distance", 40, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(40f, -92f), new Vector2(420f, 56f), new Color(0f, 0f, 0f, 0.7f));
        distance.text = "0 / 0 m";
        Text info = MakeText(ui, "Info", 32, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(-170f, -36f), new Vector2(360f, 50f), new Color(0f, 0f, 0f, 0.6f));

        Text timer = MakeText(ui, "Timer", 84, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(300f, 100f), Color.black);
        timer.fontStyle = FontStyle.Bold;
        timer.text = "0:00";
        Image[] goalStarImgs;
        Text goalTime = MakeStarRow(ui, "Goal", 3, 34f, 32, new Vector2(0.5f, 1f), new Vector2(0f, -112f), out goalStarImgs);
        goalTime.color = new Color(0f, 0f, 0f, 0.6f);

        // Duraklat dugmesi (sag ust)
        var pauseGo = new GameObject("PauseButton");
        pauseGo.transform.SetParent(ui, false);
        var pauseRt = pauseGo.AddComponent<RectTransform>();
        pauseRt.anchorMin = pauseRt.anchorMax = pauseRt.pivot = new Vector2(1f, 1f);
        pauseRt.anchoredPosition = new Vector2(-30f, -24f);
        pauseRt.sizeDelta = new Vector2(112f, 112f);
        var pauseImg = pauseGo.AddComponent<Image>();
        pauseImg.sprite = pauseSprite;
        pauseImg.preserveAspect = true;
        var pauseBtn = pauseGo.AddComponent<Button>();
        var pauseColors = pauseBtn.colors;
        pauseColors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        pauseBtn.colors = pauseColors;

        GameObject pausePanel = MakePanel(canvas.transform, "PausePanel");
        Text pauseText = MakeText(pausePanel.transform, "Text", 96, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1600f, 200f), Color.black);
        pauseText.fontStyle = FontStyle.Bold;
        Button resume = MakeButton(pausePanel.transform, "Resume", "RESUME", new Vector2(-220f, -120f), new Vector2(400f, 100f));
        Button exit = MakeButton(pausePanel.transform, "Exit", "EXIT", new Vector2(220f, -120f), new Vector2(400f, 100f));

        PedalButton brakePedal = MakePedal(ui, "BrakePedal", false, new Vector2(0f, 0f), new Vector2(40f, 40f));
        PedalButton gasPedal = MakePedal(ui, "GasPedal", true, new Vector2(1f, 0f), new Vector2(-40f, 40f));
        brakePedal.atv = controller;
        gasPedal.atv = controller;
        Text brakeHint = brakePedal.label;
        Text gasHint = gasPedal.label;

        GameObject overPanel = MakePanel(canvas.transform, "GameOverPanel");
        Text overText = MakeText(overPanel.transform, "Text", 96, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1600f, 300f), Color.black);
        overText.fontStyle = FontStyle.Bold;
        Button retry = MakeButton(overPanel.transform, "Retry", "TEKRAR DENE", new Vector2(-220f, -120f), new Vector2(400f, 100f));
        Button overMenu = MakeButton(overPanel.transform, "Menu", "MENU", new Vector2(220f, -120f), new Vector2(400f, 100f));

        GameObject donePanel = MakePanel(canvas.transform, "LevelCompletePanel");
        Text doneText = MakeText(donePanel.transform, "Text", 76, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(1700f, 190f), Color.black);
        doneText.fontStyle = FontStyle.Bold;
        Image[] resultStarImgs;
        MakeStarRow(donePanel.transform, "ResultStars", 3, 150f, 0, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), out resultStarImgs);
        Text resultTime = MakeText(donePanel.transform, "ResultTime", 48, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(1600f, 70f), Color.black);
        Image[] g3; Image[] g2;
        Text goal3 = MakeStarRow(donePanel.transform, "Goal3", 3, 36f, 34, new Vector2(0.5f, 0.5f), new Vector2(-260f, -70f), out g3);
        Text goal2 = MakeStarRow(donePanel.transform, "Goal2", 2, 36f, 34, new Vector2(0.5f, 0.5f), new Vector2(240f, -70f), out g2);
        goal3.color = goal2.color = new Color(0f, 0f, 0f, 0.6f);
        Button doneRetry = MakeButton(donePanel.transform, "Retry", "RETRY", new Vector2(-440f, -190f), new Vector2(400f, 100f));
        Button next = MakeButton(donePanel.transform, "Next", "NEXT", new Vector2(0f, -190f), new Vector2(400f, 100f));
        Button doneMenu = MakeButton(donePanel.transform, "Menu", "MENU", new Vector2(440f, -190f), new Vector2(400f, 100f));

        // ---------- Oyun yoneticisi ----------
        var gmGo = new GameObject("GameManager");
        var gm = gmGo.AddComponent<GameManager>();
        gm.atv = controller;
        gm.terrain = gen;
        gm.quotes = quotes;
        gm.starOn = starOn;
        gm.starOff = starOff;
        gm.timerText = timer;
        gm.goalStars = goalStarImgs;
        gm.goalTimeText = goalTime;
        gm.resultStars = resultStarImgs;
        gm.resultTimeText = resultTime;
        gm.goal3Text = goal3;
        gm.goal2Text = goal2;
        gm.completeRetryButton = doneRetry;
        gm.pauseButton = pauseBtn;
        gm.pausePanel = pausePanel;
        gm.pauseText = pauseText;
        gm.resumeButton = resume;
        gm.exitButton = exit;
        gm.brakeHint = brakeHint;
        gm.gasHint = gasHint;
        gm.flagSprite = flagSprite;
        gm.levelText = level;
        gm.distanceText = distance;
        gm.infoText = info;
        gm.gameOverPanel = overPanel;
        gm.gameOverText = overText;
        gm.retryButton = retry;
        gm.gameOverMenuButton = overMenu;
        gm.levelCompletePanel = donePanel;
        gm.levelCompleteText = doneText;
        gm.nextButton = next;
        gm.completeMenuButton = doneMenu;

        EditorSceneManager.SaveScene(scene, GameScenePath);
    }

    // ==================================================================
    static Rect PxRect(int x0, int y0, int x1, int y1, int w, int h)
    {
        Vector2 a = SpriteFactory.PxToLocal(new Vector2(x0, y0), w, h);
        Vector2 b = SpriteFactory.PxToLocal(new Vector2(x1, y1), w, h);
        return new Rect(a.x, a.y, b.x - a.x, b.y - a.y);
    }

    static void AddBox(GameObject go, Rect r, PhysicsMaterial2D phys)
    {
        var box = go.AddComponent<BoxCollider2D>();
        box.offset = r.center;
        box.size = r.size;
        box.sharedMaterial = phys;
    }

    static GameObject MakeWheel(string name, Transform parent, Vector2 localPos, Sprite sprite, PhysicsMaterial2D phys, float radius)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.mass = 0.7f;
        rb.angularDamping = 0.05f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = radius;
        col.sharedMaterial = phys;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 9; // camurluklarin arkasinda
        return go;
    }

    static WheelJoint2D MakeWheelJoint(Rigidbody2D body, GameObject wheel, Vector2 anchor)
    {
        var joint = body.gameObject.AddComponent<WheelJoint2D>();
        joint.connectedBody = wheel.GetComponent<Rigidbody2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = anchor;
        joint.connectedAnchor = Vector2.zero;
        joint.enableCollision = false;
        var sus = joint.suspension;
        sus.angle = 90f;
        sus.frequency = 5f;
        sus.dampingRatio = 0.7f;
        joint.suspension = sus;
        joint.useMotor = false;
        return joint;
    }

    /// <summary>Yan yana yildiz ikonlari + (istege bagli) metin. Metin fontSize 0 ise metin yok.</summary>
    static Text MakeStarRow(Transform parent, string name, int count, float starSize, int fontSize, Vector2 anchor, Vector2 pos, out Image[] stars)
    {
        var row = new GameObject(name);
        row.transform.SetParent(parent, false);
        var rt = row.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(count * (starSize + 8f) + (fontSize > 0 ? 160f : 0f), starSize + 8f);
        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        stars = new Image[count];
        for (int i = 0; i < count; i++)
        {
            var st = new GameObject("Star" + i);
            st.transform.SetParent(row.transform, false);
            st.AddComponent<RectTransform>().sizeDelta = new Vector2(starSize, starSize);
            var img = st.AddComponent<Image>();
            img.sprite = starOff;
            img.raycastTarget = false;
            stars[i] = img;
        }
        if (fontSize <= 0) return null;
        Text t = MakeText(row.transform, "Text", fontSize, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150f, starSize + 8f), Color.black);
        t.fontStyle = FontStyle.Bold;
        return t;
    }

    static PedalButton MakePedal(Transform parent, string name, bool isGas, Vector2 anchor, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(320f, 240f);

        var fill = MakeImage(go.transform, "Fill", pedalSprites[0], new Vector2(320f, 240f), Vector2.zero);
        fill.color = new Color(1f, 1f, 1f, 0.75f);
        fill.raycastTarget = false;
        var outline = MakeImage(go.transform, "Outline", pedalSprites[1], new Vector2(320f, 240f), Vector2.zero);
        outline.raycastTarget = false;
        var icon = MakeImage(go.transform, "Icon", pedalSprites[2], new Vector2(110f, 110f), new Vector2(0f, 28f));
        icon.color = Color.black;
        icon.raycastTarget = false;
        if (!isGas) icon.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
        Text label = MakeText(go.transform, "Label", 40, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(300f, 60f), Color.black);
        label.fontStyle = FontStyle.Bold;

        var pedal = go.AddComponent<PedalButton>();
        pedal.isGas = isGas;
        pedal.fill = fill;
        pedal.icon = icon;
        pedal.label = label;
        return pedal;
    }

    static Image MakeImage(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        return img;
    }

    static Canvas MakeCanvas()
    {
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void MakeEventSystem()
    {
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    static GameObject MakePanel(Transform parent, string name)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var img = panel.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.9f);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        panel.SetActive(false);
        return panel;
    }

    static Text MakeText(Transform parent, string name, int size, TextAnchor align,
        Vector2 anchor, Vector2 pos, Vector2 sizeDelta, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        var t = go.AddComponent<Text>();
        t.font = Font;
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static Button MakeButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = Color.white;
        var outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(4f, -4f);
        outline.useGraphicAlpha = false;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
        btn.colors = colors;
        Text t = MakeText(go.transform, "Label", 40, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, size, Color.black);
        t.fontStyle = FontStyle.Bold;
        t.text = label;
        return btn;
    }

    static Material MakeGroundMaterial()
    {
        const string path = "Assets/Materials/Ground.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = Color.black;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static PhysicsMaterial2D MakePhysicsMaterial(string path, float friction)
    {
        var pm = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (pm == null)
        {
            pm = new PhysicsMaterial2D(Path.GetFileNameWithoutExtension(path));
            AssetDatabase.CreateAsset(pm, path);
        }
        pm.friction = friction;
        pm.bounciness = 0f;
        EditorUtility.SetDirty(pm);
        return pm;
    }
}
