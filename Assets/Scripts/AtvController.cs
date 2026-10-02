using UnityEngine;

/// <summary>
/// ATV surus kontrolu. Gaz: ileri, fren: geri. Havada gaz/fren govdeyi egdirir.
/// Klavye (sag/sol ok, W/S, A/D), fare ve dokunmatik (ekranin sag yarisi gaz, sol yarisi fren) destekler.
/// </summary>
public class AtvController : MonoBehaviour
{
    [Header("Referanslar")]
    public WheelJoint2D rearWheel;
    public WheelJoint2D frontWheel;
    public Rigidbody2D body;

    [Header("Motor")]
    public float maxWheelSpeed = 1100f;   // derece/saniye (~9.6 m/s, teker yaricapi 0.5)
    public float motorTorque = 12f;
    public float reverseFactor = 0.6f;
    public float maxSpeed = 9.5f;         // birim/saniye, hiz siniri

    [Header("Egilme")]
    public float groundTorque = 2f;       // yerdeyken hafif kalkis hissi
    public float antiFlipAngle = 40f;     // burun bu aciyi gecince motor torku kisilir
    public float airTorque = 42f;         // havada govdeyi cevirme gucu (gaz: burun kalkar, birakmazsan sirt ustu duser)
    public float maxAirSpin = 380f;       // derece/saniye, havada donus siniri

    public bool IsGrounded { get; private set; }

    /// <summary>Arka tekerlek donus hizi / azami hiz (0..1), motor sesi icin.</summary>
    public float WheelSpeedNormalized =>
        rearWheel != null && rearWheel.connectedBody != null
            ? Mathf.Clamp01(Mathf.Abs(rearWheel.connectedBody.angularVelocity) / maxWheelSpeed) : 0f;

    public float Throttle { get; private set; } // -1 fren, 0 bos, +1 gaz
    public bool ControlsEnabled = true;

    [Header("Test (otomatik surus)")]
    public bool useAutoThrottle;
    public float autoThrottle;

    [Header("Denge")]
    public Vector2 centerOfMass = new Vector2(0f, -0.35f);

    JointMotor2D motor;

    void Start()
    {
        body.centerOfMass = centerOfMass;
    }

    void Reset()
    {
        body = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!ControlsEnabled) Throttle = 0f;
        else Throttle = useAutoThrottle ? Mathf.Clamp(autoThrottle, -1f, 1f) : ReadInput();
    }

    void FixedUpdate()
    {
        IsGrounded = rearWheel.connectedBody.IsTouchingLayers() || frontWheel.connectedBody.IsTouchingLayers();
        bool useMotor = Mathf.Abs(Throttle) > 0.01f;

        // Ileri yon = saga. Saat yonunde donus negatif oldugu icin gaz negatif hiz verir.
        float speed = Throttle > 0 ? -maxWheelSpeed : maxWheelSpeed * reverseFactor;

        // Hiz siniri: ileri giderken sinira ulasinca motoru serbest birak.
        if (Throttle > 0 && body.linearVelocity.x > maxSpeed) useMotor = false;
        if (Throttle < 0 && body.linearVelocity.x < -maxSpeed * reverseFactor) useMotor = false;

        // Anti-takla: yerdeyken burun cok kalkmissa motor torkunu kis.
        float tilt = Mathf.DeltaAngle(0f, body.rotation);
        float torqueScale = 1f;
        if (IsGrounded && Throttle > 0 && tilt > antiFlipAngle) torqueScale = 0.2f;
        if (IsGrounded && Throttle < 0 && tilt < -antiFlipAngle) torqueScale = 0.2f;

        motor.motorSpeed = speed;
        motor.maxMotorTorque = motorTorque * torqueScale;

        rearWheel.useMotor = useMotor;
        frontWheel.useMotor = useMotor;
        if (useMotor)
        {
            rearWheel.motor = motor;
            frontWheel.motor = motor;
        }

        // Govde egilme: gaz burnu kaldirir, fren burnu indirir. Havada cok daha etkili.
        if (Mathf.Abs(Throttle) > 0.01f)
        {
            float torque = IsGrounded ? groundTorque : airTorque;
            bool spinningTooFast = !IsGrounded && Mathf.Abs(body.angularVelocity) > maxAirSpin
                                   && Mathf.Sign(body.angularVelocity) == Mathf.Sign(Throttle);
            if (!spinningTooFast) body.AddTorque(Throttle * torque);
        }
    }

    static float ReadInput()
    {
        float t = 0f;

        // Klavye
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            t += 1f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            t -= 1f;

        // Dokunmatik: ekranin sag yarisi gaz, sol yarisi fren
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) continue;
            t += touch.position.x > Screen.width * 0.5f ? 1f : -1f;
        }

        // Fare (masaustunde test icin)
        if (Input.touchCount == 0 && Input.GetMouseButton(0))
            t += Input.mousePosition.x > Screen.width * 0.5f ? 1f : -1f;

        return Mathf.Clamp(t, -1f, 1f);
    }
}
