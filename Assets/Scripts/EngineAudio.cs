using UnityEngine;

/// <summary>
/// Kodla uretilen motor sesi. Ses dosyasi gerekmez: 1 saniyelik dongu sentezlenir,
/// gaz ve tekerlek hizina gore perde (pitch) ve ses seviyesi degisir.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class EngineAudio : MonoBehaviour
{
    public AtvController atv;

    [Header("Ses")]
    public float idlePitch = 0.7f;
    public float maxPitch = 2.1f;
    public float idleVolume = 0.25f;
    public float throttleVolume = 0.7f;
    public float revResponse = 3.5f;    // gaza tepki hizi
    public float dropResponse = 1.8f;   // gaz kesilince dusme hizi

    AudioSource source;
    float rpm;   // 0..1

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.clip = BuildClip();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = idleVolume;
        source.pitch = idlePitch;
        source.Play();
    }

    void Update()
    {
        if (atv == null) return;
        source.mute = !Settings.SoundOn || Time.timeScale == 0f;
        float wheel = atv.WheelSpeedNormalized;                 // 0..1
        bool gas = atv.Throttle > 0.01f;
        bool brake = atv.Throttle < -0.01f;

        // Gaz basiliyken motor tekerlekten bagimsiz da yukselir (bosta/havada vinlama)
        float target = gas ? Mathf.Max(wheel, 0.55f) : brake ? Mathf.Max(wheel * 0.6f, 0.3f) : wheel * 0.5f;
        float rate = target > rpm ? revResponse : dropResponse;
        rpm = Mathf.MoveTowards(rpm, target, rate * Time.unscaledDeltaTime);

        source.pitch = Mathf.Lerp(idlePitch, maxPitch, rpm) * Mathf.Lerp(0.6f, 1f, Time.timeScale);
        float vol = Mathf.Lerp(idleVolume, throttleVolume, gas ? 1f : rpm * 0.6f);
        source.volume = Mathf.MoveTowards(source.volume, vol, 2f * Time.unscaledDeltaTime);
    }

    /// <summary>Tek silindirli motor "pat-pat" dongusu sentezler (55 Hz, 1 sn, kesintisiz dongu).</summary>
    static AudioClip BuildClip()
    {
        const int rate = 44100;
        const float f0 = 55f;                 // 1 saniyede tam 55 dongu -> dikissiz loop
        var data = new float[rate];
        var rnd = new System.Random(42);
        float lowpass = 0f;

        for (int i = 0; i < rate; i++)
        {
            float t = i / (float)rate;
            float phase = (t * f0) % 1f;

            // Patlama darbesi: her dongude hizla sonen enerji
            float burst = Mathf.Exp(-phase * 7f);
            float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
            lowpass += (noise - lowpass) * 0.25f;     // yumusatilmis gurultu

            // Harmonikler (testere dalgasi benzeri gövde sesi)
            float tone = 0f;
            for (int k = 1; k <= 6; k++)
                tone += Mathf.Sin(2f * Mathf.PI * f0 * k * t) / k;
            float sub = Mathf.Sin(2f * Mathf.PI * f0 * 0.5f * t);   // alt oktav titresimi

            float s = tone * 0.35f + sub * 0.25f + lowpass * burst * 0.9f + burst * 0.15f;
            data[i] = Mathf.Clamp(s * 0.55f, -1f, 1f);
        }

        // Dongu baslangic/bitis uyumu icin kisa capraz gecis
        const int fade = 400;
        for (int i = 0; i < fade; i++)
        {
            float a = i / (float)fade;
            data[i] = data[i] * a + data[rate - fade + i] * (1f - a);
        }

        var clip = AudioClip.Create("EngineLoop", rate, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
