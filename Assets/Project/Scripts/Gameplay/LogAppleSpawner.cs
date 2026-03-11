using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Random spawn táo trên mép Log
/// </summary>
public class LogAppleSpawner : MonoBehaviour
{
    [Header("── References ──")]
    [SerializeField] private GameObject applePrefab;

    [Header("── Cấu hình ──")]
    [SerializeField] private int appleCount = 2;
    [SerializeField] private float logRadius = 0.95f;

    private List<GameObject> _apples = new();
    private CircleCollider2D _collider;

    public System.Action<int> OnAppleCollected;

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        SpawnApples();
    }

    // ── PUBLIC ──────────────────────────────
    public void Setup(int count)
    {
        appleCount = count;
        ClearApples();
        SpawnApples();
    }

    public void ClearApples()
    {
        foreach (var a in _apples)
            if (a != null) Destroy(a);
        _apples.Clear();
    }

    // ── PRIVATE ─────────────────────────────
    private void SpawnApples()
    {
        if (appleCount <= 0) return;
        if (applePrefab == null)
        {
            Debug.LogError("LogAppleSpawner: Chưa gắn applePrefab!");
            return;
        }

        float actualRadius = _collider != null
            ? _collider.radius * transform.localScale.x
            : logRadius * transform.localScale.x;

        List<float> usedAngles = new List<float>();

        for (int i = 0; i < appleCount; i++)
        {
            float angle = GetRandomAngle(usedAngles);
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            Vector3 pos = transform.position
                        + (Vector3)(dir * (actualRadius + 0.1f));

            GameObject apple = Instantiate(applePrefab, pos,
                                           Quaternion.identity);

            apple.transform.SetParent(transform);
            apple.tag = "Apple";

            Apple appleScript = apple.GetComponent<Apple>();
            if (appleScript != null)
            {
                appleScript.OnCollected += (score) =>
                {
                    // Báo ScoreManager cộng điểm táo
                    if (ScoreManager.Instance != null)
                        ScoreManager.Instance.AddAppleScore(score);

                    OnAppleCollected?.Invoke(score);
                };
            }

            _apples.Add(apple);
        }

        Debug.Log($"LogAppleSpawner: Spawned {appleCount} apples");
    }

    private float GetRandomAngle(List<float> usedAngles)
    {
        float angle;
        int maxTry = 20;

        do
        {
            angle = Random.Range(0f, 360f);
            maxTry--;
        }
        while (!IsAngleValid(angle, usedAngles) && maxTry > 0);

        usedAngles.Add(angle);
        return angle;
    }

    private bool IsAngleValid(float angle, List<float> usedAngles)
    {
        foreach (float used in usedAngles)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(angle, used)) < 30f)
                return false;
        }
        return true;
    }
}