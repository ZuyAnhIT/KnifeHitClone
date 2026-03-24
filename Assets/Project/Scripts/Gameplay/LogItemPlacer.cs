using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý đặt DAO SẴN và TÁO lên Log
/// Đảm bảo không trùng vị trí nhau
/// </summary>
public class LogItemPlacer : MonoBehaviour
{
    [Header("── Prefabs ──")]
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private GameObject applePrefab;

    [Header("── Cấu hình ──")]
    [SerializeField] private float minAngleBetween = 35f; // Góc tối thiểu giữa 2 item
    [SerializeField] private float knifeOffset = 0.8f;
    [SerializeField] private float appleOffset = 1.03f;

    private CircleCollider2D _collider;
    private List<GameObject> _items = new List<GameObject>();
    private List<float> _usedAngles = new List<float>();

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
    }

    // ── PUBLIC ──────────────────────────────────
    public void Setup(int knifeCount, int appleCount)
    {
        ClearAll();

        float actualRadius = _collider != null
            ? _collider.radius * transform.localScale.x
            : 0.95f * transform.localScale.x;

        // Đặt dao sẵn trước
        for (int i = 0; i < knifeCount; i++)
        {
            float angle = GetUniqueAngle();
            if (angle < 0) break; // Không tìm được góc trống

            PlaceKnife(angle, actualRadius);
        }

        // Đặt táo sau (tránh góc đã dùng)
        for (int i = 0; i < appleCount; i++)
        {
            float angle = GetUniqueAngle();
            if (angle < 0) break;

            PlaceApple(angle, actualRadius);
        }

        Debug.Log($"LogItemPlacer: {knifeCount} knives, " +
                  $"{appleCount} apples placed");
    }

    public void ClearAll()
    {
        foreach (var item in _items)
            if (item != null) Destroy(item);
        _items.Clear();
        _usedAngles.Clear();
    }

    // ── PRIVATE ─────────────────────────────────
    private void PlaceKnife(float angle, float radius)
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        Vector3 pos = transform.position
                     + (Vector3)(dir * (radius + knifeOffset));

        GameObject knife = Instantiate(knifePrefab, pos,
                                       Quaternion.identity);

        // Xoay đúng tư thế cắm
        float rotAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        knife.transform.eulerAngles = new Vector3(0f, 0f, rotAngle + 90f);

        knife.transform.SetParent(transform);
        knife.tag = "StuckKnife";

        // Tắt physics và controller
        var rb = knife.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.isKinematic = true; rb.velocity = Vector2.zero; }

        var kc = knife.GetComponent<KnifeController>();
        if (kc != null) kc.enabled = false;

        _items.Add(knife);
    }

    private void PlaceApple(float angle, float radius)
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // KHÔNG nhân scale vào radius
        // Dùng collider.radius gốc + offset
        float rawRadius = _collider != null
            ? _collider.radius
            : 0.95f;

        Vector3 pos = transform.position
                     + (Vector3)(dir * (rawRadius + 0.5f));

        GameObject apple = Instantiate(applePrefab, pos,
                                       Quaternion.identity);

        // Xoay đúng hướng dít táo vào Log
        float rotAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        apple.transform.eulerAngles = new Vector3(0f, 0f, rotAngle - 90f);

        apple.transform.SetParent(transform);
        apple.tag = "Apple";

        // Đăng ký event điểm
        var appleScript = apple.GetComponent<Apple>();
        if (appleScript != null)
        {
            appleScript.OnCollected += (score) =>
            {
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.AddAppleScore(score);
            };
        }

        _items.Add(apple);
    }

    /// <summary>
    /// Tìm góc ngẫu nhiên không trùng với góc đã dùng
    /// Cách nhau ít nhất minAngleBetween độ
    /// </summary>
    private float GetUniqueAngle()
    {
        int maxTry = 30;

        for (int t = 0; t < maxTry; t++)
        {
            float angle = Random.Range(0f, 360f);

            if (IsAngleValid(angle))
            {
                _usedAngles.Add(angle);
                return angle;
            }
        }

        Debug.LogWarning("LogItemPlacer: Không tìm được góc trống!");
        return -1f; // Không tìm được
    }

    private bool IsAngleValid(float angle)
    {
        foreach (float used in _usedAngles)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(angle, used)) < minAngleBetween)
                return false;
        }
        return true;
    }
}