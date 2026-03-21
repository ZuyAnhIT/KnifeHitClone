using UnityEngine;

public class Apple : MonoBehaviour
{
    [Header("── Điểm ──")]
    [SerializeField] private int scoreValue = 10;

    public System.Action<int> OnCollected;

    private bool _collected = false;
    private AppleHitEffect _hitEffect;
    private AppleBreakEffect _breakEffect;

    private void Awake()
    {
        _hitEffect = GetComponent<AppleHitEffect>();
        _breakEffect = GetComponent<AppleBreakEffect>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected) return;
        if (other.gameObject.tag != "Knife") return;

        Collect();
    }

    private void Collect()
    {
        _collected = true;

        // Tắt collider ngay
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Vector3 pos = transform.position;

        // Ẩn táo gốc
        SpriteRenderer sr =
            GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // 1. Hiệu ứng flash + ngôi sao
        if (_hitEffect != null)
            _hitEffect.Play(pos);

        // 2. Táo vỡ 2 mảnh bay ra
        if (_breakEffect != null)
            _breakEffect.PlayBreak(pos);

        // 3. Cộng điểm
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddAppleScore(scoreValue);

        OnCollected?.Invoke(scoreValue);

        Destroy(gameObject, 1f);
    }
}