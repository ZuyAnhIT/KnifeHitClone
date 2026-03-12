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
        if (knifePrefab == null) return;

        float actualRadius = _collider != null
            ? _collider.radius * transform.localScale.x
            : logRadius * transform.localScale.x;

        float angleStep = 360f / preplacedCount;

        for (int i = 0; i < preplacedCount; i++)
        {
            float angle = i * angleStep;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(
                                Mathf.Cos(rad),
                                Mathf.Sin(rad));

            // Vị trí giống KnifeController.StickToLog
            Vector3 pos = transform.position
                         + (Vector3)(dir * (actualRadius + 1.05f));

            GameObject knife = Instantiate(knifePrefab, pos,
                                           Quaternion.identity);

            // Xoay ĐÚNG như dao cắm thật
            // Mũi dao hướng vào tâm Log
            float rotAngle = Mathf.Atan2(dir.y, dir.x)
                             * Mathf.Rad2Deg;
            knife.transform.eulerAngles =
                new Vector3(0f, 0f, rotAngle + 90f); // ← +90f giống StickToLog

            knife.transform.SetParent(transform);
            knife.tag = "StuckKnife";

            Rigidbody2D rb = knife.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector2.zero;
            }

            KnifeController kc = knife.GetComponent<KnifeController>();
            if (kc != null) kc.enabled = false;

            _preplacedKnives.Add(knife);
        }
    }
}