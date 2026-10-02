using System.Collections.Generic;
using UnityEngine;

/// <summary>Arka planda gecen bulutlar ve ara sira ucan kus suruleri (paralaks).</summary>
public class SkyDecor : MonoBehaviour
{
    public Camera cam;
    public Sprite cloudSprite;
    public Sprite[] birdFrames;

    [Header("Bulutlar")]
    public float cloudMinInterval = 5f;
    public float cloudMaxInterval = 11f;
    public float cloudParallax = 0.55f;   // 1 = kameraya yapisik, 0 = dunyada sabit
    public float cloudWind = 0.4f;

    [Header("Kuslar")]
    public float birdMinInterval = 14f;
    public float birdMaxInterval = 30f;
    public float birdParallax = 0.35f;
    public float birdSpeed = 3.5f;
    public float flapInterval = 0.13f;

    class Item
    {
        public Transform t;
        public float parallax;
        public Vector2 velocity;
        public SpriteRenderer[] birds;
        public float flapTimer;
        public int frame;
    }

    readonly List<Item> items = new List<Item>();
    float nextCloud, nextBirds;
    Vector3 lastCam;

    float HalfWidth => cam.orthographicSize * cam.aspect;

    void Start()
    {
        lastCam = cam.transform.position;
        nextCloud = 0.5f;
        nextBirds = Random.Range(6f, 14f);
        // Baslangicta birkac bulut zaten gokte olsun
        for (int i = 0; i < 3; i++) SpawnCloud(Random.Range(-HalfWidth, HalfWidth));
    }

    void Update()
    {
        Vector3 c = cam.transform.position;
        Vector3 delta = c - lastCam;
        lastCam = c;
        float dt = Time.deltaTime;

        nextCloud -= dt;
        if (nextCloud <= 0f) { SpawnCloud(HalfWidth + 3f); nextCloud = Random.Range(cloudMinInterval, cloudMaxInterval); }
        nextBirds -= dt;
        if (nextBirds <= 0f) { SpawnBirds(); nextBirds = Random.Range(birdMinInterval, birdMaxInterval); }

        for (int i = items.Count - 1; i >= 0; i--)
        {
            var it = items[i];
            it.t.position += new Vector3(delta.x * it.parallax + it.velocity.x * dt, delta.y * it.parallax + it.velocity.y * dt, 0f);

            if (it.birds != null)
            {
                it.flapTimer += dt;
                if (it.flapTimer >= flapInterval)
                {
                    it.flapTimer = 0f;
                    it.frame = (it.frame + 1) % birdFrames.Length;
                    foreach (var b in it.birds) b.sprite = birdFrames[it.frame];
                }
            }

            float x = it.t.position.x;
            if (x < c.x - HalfWidth - 8f || x > c.x + HalfWidth + 40f)
            {
                Destroy(it.t.gameObject);
                items.RemoveAt(i);
            }
        }
    }

    void SpawnCloud(float offsetX)
    {
        Vector3 c = cam.transform.position;
        var go = new GameObject("Cloud");
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(c.x + offsetX, c.y + Random.Range(-0.5f, 3.2f), 0f);
        float scale = Random.Range(0.7f, 1.4f);
        go.transform.localScale = new Vector3(scale, scale, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = cloudSprite;
        sr.sortingOrder = -20;
        sr.color = new Color(1f, 1f, 1f, 1f);
        items.Add(new Item { t = go.transform, parallax = cloudParallax + Random.Range(-0.1f, 0.15f), velocity = new Vector2(-cloudWind * Random.Range(0.6f, 1.4f), 0f) });
    }

    void SpawnBirds()
    {
        Vector3 c = cam.transform.position;
        bool fromRight = Random.value < 0.6f;
        int count = Random.Range(3, 6);
        var flock = new GameObject("Birds");
        flock.transform.SetParent(transform, false);
        flock.transform.position = new Vector3(c.x + (fromRight ? HalfWidth + 3f : -HalfWidth - 3f), c.y + Random.Range(1.5f, 3.4f), 0f);
        float dir = fromRight ? -1f : 1f;
        var birds = new SpriteRenderer[count];
        for (int i = 0; i < count; i++)
        {
            var b = new GameObject("Bird");
            b.transform.SetParent(flock.transform, false);
            int row = (i + 1) / 2;
            float side = i % 2 == 0 ? 1f : -1f;
            b.transform.localPosition = new Vector3(-dir * row * 0.8f, -row * 0.45f * (i == 0 ? 0 : 1) * side, 0f);
            float s = Random.Range(0.55f, 0.8f);
            b.transform.localScale = new Vector3(s * dir, s, 1f);
            var sr = b.AddComponent<SpriteRenderer>();
            sr.sprite = birdFrames[0];
            sr.sortingOrder = -15;
            birds[i] = sr;
        }
        items.Add(new Item { t = flock.transform, parallax = birdParallax, velocity = new Vector2(dir * birdSpeed, Random.Range(-0.2f, 0.3f)), birds = birds, flapTimer = Random.value * 0.1f });
    }
}
