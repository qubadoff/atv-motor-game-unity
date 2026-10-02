using UnityEngine;

/// <summary>Surucunun kafasi araziye degerse oyun biter.</summary>
public class HeadTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<TerrainChunk>() == null) return;
        if (GameManager.Instance != null) GameManager.Instance.GameOver();
    }
}
