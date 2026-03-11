using UnityEngine;

/// <summary>
/// Táo gắn trên Log
/// Dao cắm trúng → Cộng điểm + Destroy
/// </summary>
public class Apple : MonoBehaviour
{
    [Header("── Điểm thưởng ──")]
    [SerializeField] private int scoreValue = 10;

    // Event báo cho ScoreManager
    public System.Action<int> OnCollected;

    private bool _collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected) return;

        // Dao đang bay trúng táo
        if (other.CompareTag("Knife") ||
            other.gameObject.tag == "Knife")
        {
            Collect();
        }
    }

    private void Collect()
    {
        _collected = true;

        Debug.Log($"Apple: Collected! +{scoreValue}");

        // Báo điểm
        OnCollected?.Invoke(scoreValue);

        // TODO: Thêm particle effect sau
        Destroy(gameObject);
    }

    public int ScoreValue => scoreValue;
}