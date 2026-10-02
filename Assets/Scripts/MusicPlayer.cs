using UnityEngine;

/// <summary>Sahneler arasi yasayan muzik calar: parcalari karisik sirayla surekli calar.</summary>
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    public AudioClip[] tracks;
    [Range(0f, 1f)] public float volume = 0.45f;

    AudioSource source;
    int[] order;
    int index;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        source = gameObject.AddComponent<AudioSource>();
        source.loop = false;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        Shuffle();
    }

    void Update()
    {
        if (tracks == null || tracks.Length == 0) return;
        source.mute = !Settings.MusicOn;
        source.volume = volume;
        if (!source.isPlaying && Settings.MusicOn) PlayNext();
    }

    void Shuffle()
    {
        order = new int[tracks != null ? tracks.Length : 0];
        for (int i = 0; i < order.Length; i++) order[i] = i;
        for (int i = order.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }
        index = 0;
    }

    void PlayNext()
    {
        if (index >= order.Length) Shuffle();
        source.clip = tracks[order[index++]];
        if (source.clip != null) source.Play();
    }
}
