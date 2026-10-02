using UnityEngine;

/// <summary>Bitis cizgisi: ATV govdesi gecince seviye tamamlanir.</summary>
public class FinishLine : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody == null) return;
        if (other.attachedRigidbody.GetComponent<AtvController>() == null) return;
        if (GameManager.Instance != null) GameManager.Instance.LevelComplete();
    }
}
