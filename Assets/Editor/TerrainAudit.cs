using UnityEditor;
using UnityEngine;

/// <summary>Her seviyenin arazi egimini olcer: -executeMethod TerrainAudit.Run</summary>
public static class TerrainAudit
{
    [MenuItem("BurnGetter/Terrain Audit")]
    public static void Run()
    {
        var go = new GameObject("audit");
        var gen = go.AddComponent<TerrainGenerator>();
        var sb = new System.Text.StringBuilder("TERRAIN AUDIT\n");
        foreach (int level in new[] { 1, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 })
        {
            var cfg = Levels.Get(level);
            gen.ApplyLevel(cfg);
            float maxSlope = 0f, worstX = 0f; int steep = 0; float climb = 0f;
            const float step = 0.5f;
            float prev = gen.HeightAt(0f);
            for (float x = step; x <= cfg.length; x += step)
            {
                float h = gen.HeightAt(x);
                float slope = Mathf.Atan2(h - prev, step) * Mathf.Rad2Deg;
                if (h > prev) climb += h - prev;
                if (slope > maxSlope) { maxSlope = slope; worstX = x; }
                if (slope > 40f) steep++;
                prev = h;
            }
            sb.AppendLine($"L{level,2}: uzunluk={cfg.length:F0} maxEgim={maxSlope:F0}° @x={worstX:F0} dik(>40°)={steep} toplamTirmanis={climb:F0}m");
        }
        Object.DestroyImmediate(go);
        Debug.Log(sb.ToString());
    }
}
