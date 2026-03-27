using UnityEngine;
using System.Collections;

/// <summary>
/// Hiệu ứng vòng tròn khi log vỡ
/// Dùng cho TẤT CẢ các màn kể cả boss
/// (Đã bỏ Glow chấm tâm theo yêu cầu)
/// </summary>
public class LogBreakRingEffect : MonoBehaviour
{
    [Header("── Sprite ──")]
    [SerializeField] private Sprite ringSprite;

    [Header("── Ring ──")]
    [SerializeField] private float ringDuration = 0.5f;

    private CircleCollider2D _col;

    private void Awake()
    {
        _col = GetComponent<CircleCollider2D>();
    }

    public void Play(Vector3 position)
    {
        float logRadius = _col != null
            ? _col.radius * transform.localScale.x
            : 0.5f;

        Debug.Log($"RingEffect: logRadius = {logRadius}");
        StartCoroutine(RingRoutine(position, logRadius));
    }

    private IEnumerator RingRoutine(
        Vector3 pos, float logRadius)
    {
        if (ringSprite == null)
        {
            Debug.LogError("LogBreakRingEffect: " +
                           "ringSprite chưa gắn!");
            yield break;
        }

        GameObject ring = new GameObject("BreakRing");
        ring.transform.position = pos;

        SpriteRenderer sr =
            ring.AddComponent<SpriteRenderer>();
        sr.sprite = ringSprite;
        sr.color = Color.white;
        sr.sortingOrder = 8;

        // Bắt đầu bằng đường kính Log
        // To ra vừa phải rồi mờ biến mất
        float startScale = logRadius * 0.8f;
        float endScale = logRadius * 1.5f;

        float elapsed = 0f;
        while (elapsed < ringDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ringDuration;

            // To dần ra
            ring.transform.localScale =
                Vector3.one * Mathf.Lerp(
                    startScale, endScale, t);

            // Mờ dần
            Color c = sr.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            sr.color = c;

            yield return null;
        }

        Destroy(ring);
    }
}