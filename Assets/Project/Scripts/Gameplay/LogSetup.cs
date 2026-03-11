using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Đặt dao sẵn trên Log khi bắt đầu màn
/// Số lượng và vị trí theo LevelPatternData
/// </summary>
public class LogSetup : MonoBehaviour
{
    [Header("── References ──")]
    [SerializeField] private GameObject knifePrefab;

    [Header("── Cấu hình ──")]
    [SerializeField] private int preplacedCount = 1;
    [SerializeField] private float logRadius = 0.95f;

    private List<GameObject> _preplacedKnives = new();
    private CircleCollider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        if (preplacedCount > 0)
            PlaceKnives();
    }

    // ── PUBLIC ──────────────────────────────
    public void Setup(int count)
    {
        preplacedCount = count;
        ClearKnives();
        PlaceKnives();
    }

    public void ClearKnives()
    {
        foreach (var k in _preplacedKnives)
            if (k != null) Destroy(k);
        _preplacedKnives.Clear();
    }

    // ── PRIVATE ─────────────────────────────
    private void PlaceKnives()
    {
        if (preplacedCount <= 0) return;
        if (knifePrefab == null)
        {
            Debug.LogError("LogSetup: Chưa gắn knifePrefab!");
            return;
        }

        // Tính radius thực tế theo scale
        float actualRadius = _collider != null
            ? _collider.radius * transform.localScale.x
            : logRadius * transform.localScale.x;

        // Chia đều góc
        float angleStep = 360f / preplacedCount;

        for (int i = 0; i < preplacedCount; i++)
        {
            float angle = i * angleStep;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(
                                 Mathf.Cos(rad),
                                 Mathf.Sin(rad));

            // Vị trí mép Log
            Vector3 pos = transform.position
                         + (Vector3)(dir * (actualRadius + 0.3f));

            // Tạo dao
            GameObject knife = Instantiate(knifePrefab, pos,
                                           Quaternion.identity);

            // Xoay hướng vào tâm
            float rotAngle = Mathf.Atan2(dir.y, dir.x)
                             * Mathf.Rad2Deg;
            knife.transform.eulerAngles =
                new Vector3(0f, 0f, rotAngle - 90f);

            // Đặt làm con của Log → xoay cùng Log
            knife.transform.SetParent(transform);

            // Gắn tag StuckKnife → Dao mới trúng = Game Over
            knife.tag = "StuckKnife";

            // Tắt Rigidbody → Không cần vật lý
            Rigidbody2D rb = knife.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector2.zero;
            }

            // Tắt KnifeController → Không cần logic bay
            KnifeController kc =
                knife.GetComponent<KnifeController>();
            if (kc != null) kc.enabled = false;

            _preplacedKnives.Add(knife);
        }

        Debug.Log($"LogSetup: Placed {preplacedCount} knives");
    }
}