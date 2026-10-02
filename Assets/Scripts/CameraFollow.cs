using UnityEngine;

/// <summary>Kamerayi hedefe yumusak takip ettirir, hareket yonunde biraz ileri bakar.</summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Rigidbody2D targetBody;
    public Vector2 offset = new Vector2(3f, 1.6f);
    public float lookAheadPerSpeed = 0.25f;
    public float smoothTime = 0.25f;

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;
        float lookAhead = targetBody != null ? Mathf.Clamp(targetBody.linearVelocity.x * lookAheadPerSpeed, -3f, 4f) : 0f;
        Vector3 desired = new Vector3(target.position.x + offset.x + lookAhead, target.position.y + offset.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
