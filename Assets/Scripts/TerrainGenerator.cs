using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oyuncunun onunde arazi uretir. Perlin gurultusu + rastgele tumsek engeller.
/// Seviye ayarlari (LevelConfig) zorlugu belirler; bitis cizgisinden sonra arazi duzlesir.
/// Her parca: EdgeCollider2D (fizik) + siyah dolgu mesh (gorunum).
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform target;
    public Material groundMaterial;
    public PhysicsMaterial2D groundPhysics;

    [Header("Parca ayarlari")]
    public float chunkLength = 40f;
    public float pointSpacing = 0.5f;
    public float bottomY = -30f;
    public int chunksAhead = 3;
    public int chunksBehind = 1;
    public float flatStartLength = 18f;

    [Header("Seviye (Start'ta uygulanir)")]
    public LevelConfig config;

    readonly Dictionary<int, GameObject> chunks = new Dictionary<int, GameObject>();
    // Ozellikler: pozitif yukseklik = tumsek, negatif = cukur
    readonly List<float> featureX = new List<float>();
    readonly List<float> featureH = new List<float>();
    readonly List<float> featureW = new List<float>();
    System.Random rng;
    float nextFeatureX;
    float noiseOffset;
    bool initialized;

    public float FinishX => config.length;

    void Start()
    {
        if (!initialized) ApplyLevel(Levels.Get(LevelSession.Current));
        UpdateChunks();
    }

    void Update()
    {
        UpdateChunks();
    }

    public void ApplyLevel(LevelConfig cfg)
    {
        config = cfg;
        rng = new System.Random(cfg.seed);
        noiseOffset = (float)rng.NextDouble() * 1000f;
        featureX.Clear(); featureH.Clear(); featureW.Clear();
        nextFeatureX = flatStartLength + Range(cfg.featureMinGap, cfg.featureMaxGap);
        GenerateFeatures();
        foreach (var kv in chunks) Destroy(kv.Value);
        chunks.Clear();
        initialized = true;
    }

    float Range(float min, float max) => min + (float)rng.NextDouble() * (max - min);

    void UpdateChunks()
    {
        if (target == null || !initialized) return;
        int current = Mathf.FloorToInt(target.position.x / chunkLength);
        int min = current - chunksBehind;
        int max = current + chunksAhead;

        for (int i = min; i <= max; i++)
            if (!chunks.ContainsKey(i)) chunks[i] = BuildChunk(i);

        var remove = new List<int>();
        foreach (var kv in chunks)
            if (kv.Key < min || kv.Key > max) remove.Add(kv.Key);
        foreach (int i in remove)
        {
            Destroy(chunks[i]);
            chunks.Remove(i);
        }
    }

    void GenerateFeatures()
    {
        // Engeller yalnizca bitis cizgisinden ~15 m oncesine kadar
        float limit = config.length - 15f;
        bool lastWasPit = false;
        while (nextFeatureX < limit)
        {
            bool pit = !lastWasPit && rng.NextDouble() < config.pitChance;
            featureX.Add(nextFeatureX);
            if (pit)
            {
                featureH.Add(-Range(config.pitMinDepth, config.pitMaxDepth));
                featureW.Add(config.pitWidth);
            }
            else
            {
                featureH.Add(Range(config.bumpMinHeight, config.bumpMaxHeight));
                featureW.Add(config.bumpWidth);
            }
            lastWasPit = pit;
            nextFeatureX += Range(config.featureMinGap, config.featureMaxGap) + (pit ? 4f : 0f);
        }
    }

    public float HeightAt(float x)
    {
        if (x < 0f) x = 0f;

        // Baslangictaki duz alan yumusak gecisle biter; bitisten sonra tekrar duzlesir.
        float rampIn = Smooth(Mathf.Clamp01((x - flatStartLength) / 10f));
        float rampOut = 1f - Smooth(Mathf.Clamp01((x - (config.length - 12f)) / 12f));
        float ramp = rampIn * rampOut;

        float amp = Mathf.Min(config.maxAmplitude, config.baseAmplitude + config.amplitudeGrowth * (x / 10f));
        float h = (Mathf.PerlinNoise(x * config.noiseScale + noiseOffset, 0.37f) - 0.5f) * 2f * amp;
        h += (Mathf.PerlinNoise(x * config.noiseScale * 6f + noiseOffset, 4.21f) - 0.5f) * 2f * config.detailAmplitude;
        h *= ramp;

        // Tumsekler ve cukurlar (gauss profili)
        for (int i = 0; i < featureX.Count; i++)
        {
            float d = x - featureX[i];
            float w = featureW[i];
            if (Mathf.Abs(d) > w * 2f) continue;
            float g = Mathf.Exp(-(d * d) / (2f * w * w * 0.18f));
            h += featureH[i] * g;
        }
        return h;
    }

    static float Smooth(float t) => t * t * (3f - 2f * t);

    GameObject BuildChunk(int index)
    {
        float startX = index * chunkLength;
        float endX = startX + chunkLength;

        int count = Mathf.CeilToInt(chunkLength / pointSpacing) + 1;
        var top = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            float x = startX + i * pointSpacing;
            top[i] = new Vector2(x, HeightAt(x));
        }

        var go = new GameObject("Chunk_" + index);
        go.transform.SetParent(transform, false);
        var chunk = go.AddComponent<TerrainChunk>();
        chunk.startX = startX;
        chunk.endX = endX;

        var edge = go.AddComponent<EdgeCollider2D>();
        edge.points = top;
        edge.sharedMaterial = groundPhysics;

        var mesh = new Mesh { name = go.name };
        var verts = new Vector3[count * 2];
        var uvs = new Vector2[count * 2];
        for (int i = 0; i < count; i++)
        {
            verts[i * 2] = new Vector3(top[i].x, top[i].y, 0f);
            verts[i * 2 + 1] = new Vector3(top[i].x, bottomY, 0f);
            uvs[i * 2] = new Vector2(top[i].x * 0.1f, 1f);
            uvs[i * 2 + 1] = new Vector2(top[i].x * 0.1f, 0f);
        }
        var tris = new int[(count - 1) * 6];
        for (int i = 0; i < count - 1; i++)
        {
            int v = i * 2, t = i * 6;
            tris[t] = v; tris[t + 1] = v + 2; tris[t + 2] = v + 1;
            tris[t + 3] = v + 1; tris[t + 4] = v + 2; tris[t + 5] = v + 3;
        }
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateBounds();

        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = groundMaterial;
        mr.sortingOrder = -10;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        return go;
    }
}
