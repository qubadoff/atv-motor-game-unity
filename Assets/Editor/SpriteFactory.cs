using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Siyah-beyaz sprite'lari kodla cizer (kontur + dolgu). Menu: BurnGetter > Regenerate Sprites
/// Komut satiri: -executeMethod SpriteFactory.GenerateAll
/// </summary>
public static class SpriteFactory
{
    public const int PPU = 80;
    public const int AtvW = 320, AtvH = 240;
    public const int WheelSize = 96;

    // ATV sprite'i icindeki onemli noktalar (piksel, sol-alt kose 0,0)
    public static readonly Vector2 RearWheelPx = new Vector2(62f, 44f);
    public static readonly Vector2 FrontWheelPx = new Vector2(266f, 44f);
    public static readonly Vector2 HelmetPx = new Vector2(144f, 212f);
    public const float HelmetRadiusPx = 20f;
    public const float WheelRadiusPx = 40f;

    static readonly Color32 Black = new Color32(0, 0, 0, 255);
    static readonly Color32 White = new Color32(255, 255, 255, 255);
    static readonly Color32 Gray = new Color32(120, 120, 120, 255);

    [MenuItem("BurnGetter/Regenerate Sprites")]
    public static void GenerateAll()
    {
        Directory.CreateDirectory("Assets/Sprites");
        MakeAtv();
        MakeWheel();
        MakeFlag();
        MakeLock();
        MakeCloud();
        MakeBirds();
        MakePedal();
        MakeArrow();
        MakeLogo();
        MakePauseIcon();
        MakeStars();
        AssetDatabase.Refresh();
        Debug.Log("BurnGetter: sprite'lar uretildi");
    }

    public static Vector2 PxToLocal(Vector2 px, int w, int h)
    {
        return new Vector2((px.x - w * 0.5f) / PPU, (px.y - h * 0.5f) / PPU);
    }

    // ------------------------------------------------------------------
    public static Sprite MakeAtv()
    {
        var c = new PixelCanvas(AtvW, AtvH);
        const int ol = 3;
        float rx = RearWheelPx.x, ry = RearWheelPx.y, fx = FrontWheelPx.x, fy = FrontWheelPx.y;
        const float arch = WheelRadiusPx + 8f;

        // --- Amortisorler (yaylar): tekerlek arkasinda, camurluktan once cizilir
        DrawSpring(c, 98, 64, 90, 104, 6f);
        DrawSpring(c, 232, 64, 242, 104, 6f);

        // --- Alt sasi / basamak cercevesi (siyah, iki teker arasi)
        var frame = new Mask(AtvW, AtvH);
        frame.Polygon(new[] { new Vector2(96, 50), new Vector2(236, 50), new Vector2(240, 66), new Vector2(92, 66) });
        c.Paint(frame, Black, Black, 1);
        c.Line(102, 58, 230, 58, 1.5f, White);
        var board = new Mask(AtvW, AtvH);                 // ayak tabani (basamak)
        board.Rect(108, 60, 214, 70);
        c.Paint(board, White, Black, 2);
        for (int x = 114; x < 210; x += 8) c.Line(x, 63, x, 67, 1.5f, Black);

        // --- Motor bolgesi (siyah alt panel, ortada)
        var engine = new Mask(AtvW, AtvH);
        engine.Polygon(new[] { new Vector2(112, 66), new Vector2(216, 66), new Vector2(218, 96), new Vector2(110, 96) });
        c.Paint(engine, Black, Black, 1);
        c.Circle(150, 80, 9f, White); c.Circle(150, 80, 6f, Black);   // motor kapagi
        c.Line(166, 74, 206, 74, 2f, White);
        c.Line(166, 86, 200, 86, 2f, White);

        // --- Arka camurluk (koseli, ust duz)
        var rearFender = new Mask(AtvW, AtvH);
        rearFender.Polygon(new[] {
            new Vector2(6, 62), new Vector2(6, 98), new Vector2(14, 112), new Vector2(60, 118),
            new Vector2(116, 118), new Vector2(120, 100), new Vector2(116, 70), new Vector2(110, 60) });
        rearFender.SubtractCircle(rx, ry, arch);
        c.Paint(rearFender, White, Black, ol);
        c.Line(14, 104, 112, 108, 2f, Black);           // camurluk ust kenar
        c.Line(20, 74, 34, 96, 2f, Black);              // koseli kivrim
        var rearLower = new Mask(AtvW, AtvH);           // camurluk alt siyah plastik seridi
        rearLower.Polygon(new[] { new Vector2(6, 62), new Vector2(6, 74), new Vector2(24, 74), new Vector2(30, 62) });
        c.Paint(rearLower, Black, Black, 1);

        // --- On camurluk (uzun, sivri burun)
        var frontFender = new Mask(AtvW, AtvH);
        frontFender.Polygon(new[] {
            new Vector2(212, 60), new Vector2(216, 74), new Vector2(214, 100), new Vector2(224, 116),
            new Vector2(266, 120), new Vector2(292, 116), new Vector2(310, 104), new Vector2(320, 84),
            new Vector2(316, 66), new Vector2(300, 56), new Vector2(282, 56) });
        frontFender.SubtractCircle(fx, fy, arch);
        c.Paint(frontFender, White, Black, ol);
        c.Line(230, 110, 300, 108, 2f, Black);          // kaporta ust cizgisi
        c.Line(286, 64, 314, 80, 2f, Black);            // burun kivrimi
        var frontLower = new Mask(AtvW, AtvH);          // on tampon / alt siyah
        frontLower.Polygon(new[] { new Vector2(296, 52), new Vector2(320, 52), new Vector2(320, 70), new Vector2(304, 62) });
        c.Paint(frontLower, Black, Black, 1);

        // --- Orta govde (sele alti yan panel, beyaz) ve depo
        var body = new Mask(AtvW, AtvH);
        body.Polygon(new[] { new Vector2(112, 96), new Vector2(218, 96), new Vector2(218, 120), new Vector2(112, 120) });
        c.Paint(body, White, Black, ol);
        c.Line(126, 104, 196, 112, 2f, Black);          // yan panel cizgisi
        var tank = new Mask(AtvW, AtvH);
        tank.Polygon(new[] {
            new Vector2(198, 118), new Vector2(204, 142), new Vector2(232, 152),
            new Vector2(258, 146), new Vector2(264, 122), new Vector2(258, 116) });
        c.Paint(tank, White, Black, ol);
        c.Line(214, 138, 246, 132, 2f, Black);

        // --- Sele (siyah, uzun) ve yolcu sirtligi
        var seat = new Mask(AtvW, AtvH);
        seat.Polygon(new[] {
            new Vector2(64, 116), new Vector2(70, 134), new Vector2(200, 134),
            new Vector2(210, 124), new Vector2(204, 116) });
        c.Paint(seat, Black, White, 2);
        c.Line(80, 128, 186, 128, 1.5f, White);
        var backrest = new Mask(AtvW, AtvH);
        backrest.Polygon(new[] { new Vector2(62, 130), new Vector2(66, 176), new Vector2(84, 180), new Vector2(98, 174), new Vector2(102, 130) });
        backrest.Circle(84, 176, 12f);
        c.Paint(backrest, Black, White, 2);
        c.Line(74, 140, 78, 168, 1.5f, White);          // sirtlik dikisi
        c.Line(88, 140, 92, 168, 1.5f, White);
        var grab = new Mask(AtvW, AtvH);                // tutma kolu
        grab.Capsule(100, 150, 124, 148, 3.5f);
        c.Paint(grab, Black, White, 1);

        // --- Arka ve on tasiyicilar
        var rearRack = new Mask(AtvW, AtvH);
        rearRack.Rect(8, 118, 64, 126);
        rearRack.Rect(14, 110, 20, 120);
        rearRack.Rect(56, 112, 62, 120);
        c.Paint(rearRack, White, Black, 2);
        for (int x = 16; x < 60; x += 9) c.Line(x, 120, x, 124, 1.5f, Black);
        var frontRack = new Mask(AtvW, AtvH);
        frontRack.Rect(236, 120, 300, 128);
        frontRack.Rect(244, 114, 250, 122);
        frontRack.Rect(292, 110, 298, 122);
        c.Paint(frontRack, White, Black, 2);
        for (int x = 244; x < 296; x += 9) c.Line(x, 122, x, 126, 1.5f, Black);

        // --- Far (on camurluk burnu)
        var lamp = new Mask(AtvW, AtvH);
        lamp.Polygon(new[] { new Vector2(292, 88), new Vector2(312, 92), new Vector2(314, 102), new Vector2(294, 100) });
        c.Paint(lamp, White, Black, 2);
        c.Line(298, 94, 308, 96, 2f, Black);

        // --- Gidon, elcik, aynalar
        var stem = new Mask(AtvW, AtvH);
        stem.Capsule(238, 150, 232, 172, 4f);
        c.Paint(stem, Black, Black, 1);
        var bar = new Mask(AtvW, AtvH);
        bar.Capsule(214, 176, 262, 170, 3.5f);
        c.Paint(bar, Black, Black, 1);
        c.Circle(214, 176, 5.5f, Black);
        c.Circle(214, 176, 3f, White);
        c.Line(252, 172, 258, 190, 3f, Black);          // ayna sapi
        var mirror = new Mask(AtvW, AtvH);
        mirror.Polygon(new[] { new Vector2(250, 188), new Vector2(268, 190), new Vector2(270, 204), new Vector2(252, 202) });
        c.Paint(mirror, White, Black, 2);

        // --- Surucu (dik oturus)
        var thigh = new Mask(AtvW, AtvH);
        thigh.Capsule(132, 130, 176, 132, 11f);
        c.Paint(thigh, Black, Black, 1);
        var shin = new Mask(AtvW, AtvH);
        shin.Capsule(176, 130, 166, 76, 8f);
        c.Paint(shin, Black, White, 2);
        var boot = new Mask(AtvW, AtvH);
        boot.Capsule(156, 70, 178, 70, 6f);
        c.Paint(boot, Black, White, 2);

        var torso = new Mask(AtvW, AtvH);
        torso.Polygon(new[] { new Vector2(118, 126), new Vector2(148, 126), new Vector2(156, 190), new Vector2(126, 194) });
        c.Paint(torso, Black, Black, 1);
        c.Line(134, 134, 142, 186, 1.5f, White);
        c.Line(122, 154, 130, 152, 1.5f, White);

        var upperArm = new Mask(AtvW, AtvH);
        upperArm.Capsule(148, 184, 178, 166, 7f);
        c.Paint(upperArm, Black, White, 2);
        var foreArm = new Mask(AtvW, AtvH);
        foreArm.Capsule(178, 166, 210, 176, 6f);
        c.Paint(foreArm, Black, White, 2);
        c.Circle(214, 176, 5.5f, Black);
        c.Circle(214, 176, 3f, White);
        c.Circle(214, 176, 1.5f, Black);

        var neck = new Mask(AtvW, AtvH);
        neck.Capsule(140, 192, 142, 200, 5f);
        c.Paint(neck, Black, Black, 1);
        var helmet = new Mask(AtvW, AtvH);
        helmet.Circle(HelmetPx.x, HelmetPx.y, HelmetRadiusPx);
        helmet.Rect((int)HelmetPx.x - 12, (int)HelmetPx.y - 18, (int)HelmetPx.x + 20, (int)HelmetPx.y - 6);
        c.Paint(helmet, White, Black, ol);
        var visor = new Mask(AtvW, AtvH);
        visor.Circle(HelmetPx.x, HelmetPx.y, HelmetRadiusPx - 3);
        visor.ClipBelowY((int)HelmetPx.y - 4);
        visor.ClipAboveY((int)HelmetPx.y + 9);
        visor.ClipLeftX((int)HelmetPx.x - 2);
        c.Paint(visor, Black, Black, 1);
        c.Line(HelmetPx.x + 6, HelmetPx.y + 6, HelmetPx.x + 12, HelmetPx.y + 2, 1.5f, White);
        c.Line(HelmetPx.x - 12, HelmetPx.y + 12, HelmetPx.x - 2, HelmetPx.y + 17, 2f, Black);

        var sprite = c.Save("Assets/Sprites/atv.png", PPU, new Vector2(0.5f, 0.5f));
        WritePreview(c);
        return sprite;
    }

    /// <summary>Amortisor yayi: iki nokta arasinda zikzak (siyah) + govde cubugu.</summary>
    static void DrawSpring(PixelCanvas c, float x0, float y0, float x1, float y1, float width)
    {
        c.Line(x0, y0, x1, y1, 5f, Black);
        const int coils = 6;
        Vector2 a = new Vector2(x0, y0), b = new Vector2(x1, y1);
        Vector2 dir = (b - a).normalized;
        Vector2 n = new Vector2(-dir.y, dir.x) * width;
        Vector2 prev = a;
        for (int i = 1; i <= coils * 2; i++)
        {
            float t = i / (coils * 2f);
            Vector2 p = Vector2.Lerp(a, b, t) + (i % 2 == 0 ? n : -n);
            c.Line(prev.x, prev.y, p.x, p.y, 2.5f, Black);
            prev = p;
        }
    }

    /// <summary>Tekerlekleri bindirilmis onizleme (Previews/atv_preview.png), sadece gozle kontrol icin.</summary>
    static void WritePreview(PixelCanvas atv)
    {
        var preview = new PixelCanvas(AtvW, AtvH);
        preview.Fill(new Color32(255, 255, 255, 255));
        var wheel = DrawWheel();
        preview.Blit(wheel, (int)(RearWheelPx.x - WheelSize / 2f), (int)(RearWheelPx.y - WheelSize / 2f));
        preview.Blit(wheel, (int)(FrontWheelPx.x - WheelSize / 2f), (int)(FrontWheelPx.y - WheelSize / 2f));
        preview.Blit(atv, 0, 0);
        preview.Rect(0, 0, AtvW, 4, Black); // zemin cizgisi
        Directory.CreateDirectory("Previews");
        preview.WritePng("Previews/atv_preview.png");
    }

    public static Sprite MakeWheel()
    {
        return DrawWheel().Save("Assets/Sprites/wheel.png", PPU, new Vector2(0.5f, 0.5f));
    }

    static PixelCanvas DrawWheel()
    {
        int s = WheelSize;
        var c = new PixelCanvas(s, s);
        float cx = s / 2f, cy = s / 2f;
        float r = WheelRadiusPx;

        c.Circle(cx, cy, r, Black);                              // lastik
        const int knobs = 14;
        for (int i = 0; i < knobs; i++)                          // iri dugmeler arasi oluklar
        {
            float a = (i + 0.5f) * Mathf.PI * 2f / knobs;
            c.Line(cx + Mathf.Cos(a) * (r - 13f), cy + Mathf.Sin(a) * (r - 13f),
                   cx + Mathf.Cos(a) * (r - 1f), cy + Mathf.Sin(a) * (r - 1f), 3f, White);
            float a2 = i * Mathf.PI * 2f / knobs;                // dugme ustu kucuk kertik
            c.Line(cx + Mathf.Cos(a2) * (r - 9f), cy + Mathf.Sin(a2) * (r - 9f),
                   cx + Mathf.Cos(a2) * (r - 5f), cy + Mathf.Sin(a2) * (r - 5f), 2f, White);
        }
        c.Circle(cx, cy, r - 14f, White);                        // lastik yanak cizgisi
        c.Circle(cx, cy, r - 16f, Black);
        c.Circle(cx, cy, r - 18f, White);                        // jant (alasim)
        c.Circle(cx, cy, r - 20f, Black);
        c.Circle(cx, cy, r - 22f, White);
        for (int i = 0; i < 5; i++)                              // 5 kol arasi bosluklar (siyah)
        {
            float a = (i + 0.5f) * Mathf.PI * 2f / 5f;
            var gap = new Mask(s, s);
            gap.Polygon(new[] {
                new Vector2(cx + Mathf.Cos(a - 0.36f) * 8f, cy + Mathf.Sin(a - 0.36f) * 8f),
                new Vector2(cx + Mathf.Cos(a - 0.30f) * 17f, cy + Mathf.Sin(a - 0.30f) * 17f),
                new Vector2(cx + Mathf.Cos(a) * 19f, cy + Mathf.Sin(a) * 19f),
                new Vector2(cx + Mathf.Cos(a + 0.30f) * 17f, cy + Mathf.Sin(a + 0.30f) * 17f),
                new Vector2(cx + Mathf.Cos(a + 0.36f) * 8f, cy + Mathf.Sin(a + 0.36f) * 8f) });
            c.Paint(gap, Black, Black, 1);
        }
        c.Circle(cx, cy, 7f, Black);                             // gobek
        c.Circle(cx, cy, 5f, White);
        for (int i = 0; i < 5; i++)                              // bijon
        {
            float a = i * Mathf.PI * 2f / 5f;
            c.Circle(cx + Mathf.Cos(a) * 3f, cy + Mathf.Sin(a) * 3f, 1f, Black);
        }
        return c;
    }

    /// <summary>Pedal arka plani (beyaz yuvarlak dikdortgen, renk tonuyla siyaha cevrilebilir) ve siyah konturu.</summary>
    public static void MakePedal()
    {
        const int w = 160, h = 120, r = 24;
        var m = new Mask(w, h);
        m.Rect(r, 0, w - r, h);
        m.Rect(0, r, w, h - r);
        m.Circle(r, r, r); m.Circle(w - r, r, r); m.Circle(r, h - r, r); m.Circle(w - r, h - r, r);

        var fill = new PixelCanvas(w, h);
        fill.Paint(m, White, White, 1);
        fill.Save("Assets/Sprites/pedal_fill.png", PPU, new Vector2(0.5f, 0.5f));

        var outline = new PixelCanvas(w, h);
        outline.Paint(m, new Color32(0, 0, 0, 0), Black, 4);
        outline.Save("Assets/Sprites/pedal_outline.png", PPU, new Vector2(0.5f, 0.5f));
    }

    /// <summary>Beyaz ok (Image.color ile siyah/beyaz tonlanir). Saga bakar; sol icin X olcegi -1.</summary>
    public static Sprite MakeArrow()
    {
        const int w = 96, h = 96;
        var c = new PixelCanvas(w, h);
        var m = new Mask(w, h);
        m.Polygon(new[] {
            new Vector2(8, 34), new Vector2(52, 34), new Vector2(52, 12), new Vector2(92, 48),
            new Vector2(52, 84), new Vector2(52, 62), new Vector2(8, 62) });
        c.Paint(m, White, White, 1);
        return c.Save("Assets/Sprites/arrow.png", PPU, new Vector2(0.5f, 0.5f));
    }

    /// <summary>BurnGame logosu: lastik gorunumlu siyah rozet icinde alev. Uygulama ikonu olarak da kullanilir.</summary>
    public static Sprite MakeLogo()
    {
        const int size = 512;
        var c = new PixelCanvas(size, size);
        float cx = size / 2f, cy = size / 2f;

        c.Circle(cx, cy, 240f, Black);                            // lastik
        const int knobs = 24;
        for (int i = 0; i < knobs; i++)                            // lastik disleri
        {
            float a = i * Mathf.PI * 2f / knobs;
            c.Line(cx + Mathf.Cos(a) * 214f, cy + Mathf.Sin(a) * 214f, cx + Mathf.Cos(a) * 238f, cy + Mathf.Sin(a) * 238f, 9f, White);
        }
        c.Circle(cx, cy, 206f, White);                            // jant kenari
        c.Circle(cx, cy, 196f, Black);                            // ic zemin

        var flame = new Mask(size, size);                         // dis alev (uc yukari)
        flame.Polygon(new[] {
            new Vector2(262, 438), new Vector2(236, 356), new Vector2(214, 392), new Vector2(166, 320),
            new Vector2(140, 236), new Vector2(160, 150), new Vector2(256, 98), new Vector2(352, 150),
            new Vector2(374, 236), new Vector2(356, 316), new Vector2(322, 372), new Vector2(300, 340) });
        flame.Circle(256, 186, 94f);
        c.Paint(flame, White, White, 1);

        var inner = new Mask(size, size);                         // ic alev (siyah)
        inner.Polygon(new[] {
            new Vector2(260, 344), new Vector2(240, 288), new Vector2(226, 306), new Vector2(200, 250),
            new Vector2(196, 196), new Vector2(256, 142), new Vector2(316, 196), new Vector2(314, 246),
            new Vector2(296, 292), new Vector2(282, 274) });
        inner.Circle(256, 188, 46f);
        c.Paint(inner, Black, Black, 1);
        c.Circle(256, 182, 17f, White);                           // alev cekirdegi

        return c.Save("Assets/Sprites/logo.png", 128, new Vector2(0.5f, 0.5f));
    }

    /// <summary>Duraklat dugmesi: yuvarlak beyaz dugme, siyah kontur, iki dikey cubuk.</summary>
    public static Sprite MakePauseIcon()
    {
        const int size = 112;
        var c = new PixelCanvas(size, size);
        var m = new Mask(size, size);
        m.Circle(56, 56, 52f);
        c.Paint(m, White, Black, 5);
        c.Rect(36, 32, 50, 80, Black);
        c.Rect(62, 32, 76, 80, Black);
        return c.Save("Assets/Sprites/pause.png", PPU, new Vector2(0.5f, 0.5f));
    }

    /// <summary>Yildiz: dolu (siyah) ve bos (kontur).</summary>
    public static void MakeStars()
    {
        const int size = 64;
        var pts = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float a = Mathf.PI / 2f + i * Mathf.PI / 5f;
            float r = i % 2 == 0 ? 29f : 12.5f;
            pts[i] = new Vector2(32f + Mathf.Cos(a) * r, 33f + Mathf.Sin(a) * r);
        }
        var on = new PixelCanvas(size, size);
        var m = new Mask(size, size); m.Polygon(pts);
        on.Paint(m, Black, Black, 1);
        on.Save("Assets/Sprites/star_on.png", PPU, new Vector2(0.5f, 0.5f));
        var off = new PixelCanvas(size, size);
        off.Paint(m, new Color32(0, 0, 0, 0), Black, 3);
        off.Save("Assets/Sprites/star_off.png", PPU, new Vector2(0.5f, 0.5f));
    }

    public static Sprite MakeCloud()
    {
        const int w = 128, h = 64;
        var c = new PixelCanvas(w, h);
        var m = new Mask(w, h);
        m.Circle(30, 26, 18f);
        m.Circle(56, 36, 24f);
        m.Circle(86, 32, 20f);
        m.Circle(108, 24, 14f);
        m.Rect(14, 10, 118, 28);
        c.Paint(m, White, Black, 3);
        return c.Save("Assets/Sprites/cloud.png", PPU, new Vector2(0.5f, 0.5f));
    }

    public static void MakeBirds()
    {
        for (int f = 0; f < 2; f++)
        {
            const int w = 48, h = 28;
            var c = new PixelCanvas(w, h);
            float tipY = f == 0 ? 24f : 6f;   // kanat yukari / asagi
            c.Line(24, 12, 6, tipY, 3f, Black);
            c.Line(24, 12, 42, tipY, 3f, Black);
            c.Circle(24, 12, 3.5f, Black);
            c.Save("Assets/Sprites/bird_" + f + ".png", PPU, new Vector2(0.5f, 0.5f));
        }
    }

    public static Sprite MakeLock()
    {
        const int w = 40, h = 48;
        var c = new PixelCanvas(w, h);
        var shackle = new Mask(w, h);
        shackle.Circle(20, 30, 13f);
        shackle.SubtractCircle(20, 30, 8f);
        shackle.ClipBelowY(28);
        c.Paint(shackle, White, Black, 2);
        var body = new Mask(w, h);
        body.Rect(4, 4, 36, 30);
        c.Paint(body, White, Black, 3);
        c.Circle(20, 19, 3.5f, Black);
        c.Rect(18, 8, 22, 18, Black);
        return c.Save("Assets/Sprites/lock.png", PPU, new Vector2(0.5f, 0.5f));
    }

    public static Sprite MakeFlag()
    {
        const int w = 64, h = 160;
        var c = new PixelCanvas(w, h);
        c.Rect(6, 0, 12, 160, Black);                   // direk
        c.Circle(9, 158, 4f, Black);
        var flag = new Mask(w, h);
        flag.Rect(12, 110, 62, 150);
        c.Paint(flag, White, Black, 2);
        for (int y = 112; y < 148; y += 9)              // damali desen
            for (int x = 14; x < 60; x += 9)
                if (((x - 14) / 9 + (y - 112) / 9) % 2 == 0) c.Rect(x, y, Mathf.Min(x + 9, 60), Mathf.Min(y + 9, 148), Black);
        return c.Save("Assets/Sprites/flag.png", PPU, new Vector2(0.15f, 0f));
    }

    // ------------------------------------------------------------------
    /// <summary>Sekil maskesi: bir parcanin kapladigi pikseller.</summary>
    public class Mask
    {
        public readonly int w, h;
        public readonly bool[] px;

        public Mask(int width, int height) { w = width; h = height; px = new bool[w * h]; }
        public bool Get(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && px[y * w + x];

        public void Rect(int x0, int y0, int x1, int y1)
        {
            for (int y = Mathf.Max(0, y0); y < Mathf.Min(h, y1); y++)
                for (int x = Mathf.Max(0, x0); x < Mathf.Min(w, x1); x++) px[y * w + x] = true;
        }

        public void Circle(float cx, float cy, float r) => CircleSet(cx, cy, r, true);
        public void SubtractCircle(float cx, float cy, float r) => CircleSet(cx, cy, r, false);

        void CircleSet(float cx, float cy, float r, bool v)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - r) - 1), x1 = Mathf.Min(w - 1, Mathf.CeilToInt(cx + r) + 1);
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - r) - 1), y1 = Mathf.Min(h - 1, Mathf.CeilToInt(cy + r) + 1);
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                    if (dx * dx + dy * dy <= r * r) px[y * w + x] = v;
                }
        }

        public void ClipAboveY(int maxY)
        {
            for (int y = Mathf.Max(0, maxY); y < h; y++)
                for (int x = 0; x < w; x++) px[y * w + x] = false;
        }

        public void ClipLeftX(int minX)
        {
            for (int y = 0; y < h; y++)
                for (int x = 0; x < Mathf.Min(w, minX); x++) px[y * w + x] = false;
        }

        public void ClipBelowY(int minY)
        {
            for (int y = 0; y < Mathf.Min(h, minY); y++)
                for (int x = 0; x < w; x++) px[y * w + x] = false;
        }

        public void Capsule(float x0, float y0, float x1, float y1, float radius)
        {
            float len = Vector2.Distance(new Vector2(x0, y0), new Vector2(x1, y1));
            int steps = Mathf.Max(1, Mathf.CeilToInt(len * 2f));
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Circle(Mathf.Lerp(x0, x1, t), Mathf.Lerp(y0, y1, t), radius);
            }
        }

        public void Polygon(Vector2[] pts)
        {
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            foreach (var p in pts) { minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x); minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y); }
            for (int y = Mathf.Max(0, (int)minY); y <= Mathf.Min(h - 1, (int)maxY); y++)
                for (int x = Mathf.Max(0, (int)minX); x <= Mathf.Min(w - 1, (int)maxX); x++)
                    if (Inside(pts, x + 0.5f, y + 0.5f)) px[y * w + x] = true;
        }

        static bool Inside(Vector2[] p, float x, float y)
        {
            bool inside = false;
            for (int i = 0, j = p.Length - 1; i < p.Length; j = i++)
            {
                if ((p[i].y > y) != (p[j].y > y) &&
                    x < (p[j].x - p[i].x) * (y - p[i].y) / (p[j].y - p[i].y) + p[i].x)
                    inside = !inside;
            }
            return inside;
        }
    }

    /// <summary>Piksel tuvali; maskeleri konturlu boyar, sprite olarak kaydeder.</summary>
    public class PixelCanvas
    {
        readonly int w, h;
        readonly Color32[] px;

        public PixelCanvas(int width, int height)
        {
            w = width; h = height;
            px = new Color32[w * h];
        }

        public void Fill(Color32 col) { for (int i = 0; i < px.Length; i++) px[i] = col; }

        public void Blit(PixelCanvas src, int ox, int oy)
        {
            for (int y = 0; y < src.h; y++)
                for (int x = 0; x < src.w; x++)
                {
                    int tx = x + ox, ty = y + oy;
                    if (tx < 0 || ty < 0 || tx >= w || ty >= h) continue;
                    var s = src.px[y * src.w + x];
                    if (s.a > 0) px[ty * w + tx] = s;
                }
        }

        public void WritePng(string path)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        /// <summary>Maskeyi boyar: kenara yakin pikseller kontur rengi, icerisi dolgu rengi.</summary>
        public void Paint(Mask m, Color32 fill, Color32 outline, int outlineWidth)
        {
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (!m.Get(x, y)) continue;
                    bool edge = false;
                    for (int dy = -outlineWidth; dy <= outlineWidth && !edge; dy++)
                        for (int dx = -outlineWidth; dx <= outlineWidth; dx++)
                        {
                            if (dx * dx + dy * dy > outlineWidth * outlineWidth) continue;
                            if (!m.Get(x + dx, y + dy)) { edge = true; break; }
                        }
                    px[y * w + x] = edge ? outline : fill;
                }
        }

        public void Rect(int x0, int y0, int x1, int y1, Color32 col)
        {
            for (int y = Mathf.Max(0, y0); y < Mathf.Min(h, y1); y++)
                for (int x = Mathf.Max(0, x0); x < Mathf.Min(w, x1); x++) px[y * w + x] = col;
        }

        public void Circle(float cx, float cy, float r, Color32 col)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - r) - 1), x1 = Mathf.Min(w - 1, Mathf.CeilToInt(cx + r) + 1);
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - r) - 1), y1 = Mathf.Min(h - 1, Mathf.CeilToInt(cy + r) + 1);
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                    if (dx * dx + dy * dy <= r * r) px[y * w + x] = col;
                }
        }

        public void Line(float x0, float y0, float x1, float y1, float thickness, Color32 col)
        {
            float len = Vector2.Distance(new Vector2(x0, y0), new Vector2(x1, y1));
            int steps = Mathf.Max(1, Mathf.CeilToInt(len * 2f));
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Circle(Mathf.Lerp(x0, x1, t), Mathf.Lerp(y0, y1, t), thickness * 0.5f, col);
            }
        }

        public Sprite Save(string path, int ppu, Vector2 pivot)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
