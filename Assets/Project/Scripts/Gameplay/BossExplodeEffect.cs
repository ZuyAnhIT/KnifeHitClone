using UnityEngine;
using System.Collections;

public class BossExplodeEffect : MonoBehaviour
{
    [Header("── Sprite ──")]
    [SerializeField] private Sprite dotSprite;

    [Header("── Cấu hình ──")]
    [SerializeField] private int dotCount = 80;
    [SerializeField] private float dotMinSize = 0.4f;
    [SerializeField] private float dotMaxSize = 1.4f;
    [SerializeField] private float dotMinSpeed = 4f;
    [SerializeField] private float dotMaxSpeed = 10f;
    [SerializeField] private float dotDuration = 1.2f;

    private CircleCollider2D _col;

    private void Awake()
    {
        _col = GetComponent<CircleCollider2D>();
    }

    public void Play(Vector3 position, Color color)
    {
        Debug.Log($"BossExplode: Playing! " +
                  $"Color={color} Dots={dotCount}");

        if (dotSprite == null)
        {
            Debug.LogError("BossExplodeEffect: " +
                           "dotSprite chưa gắn!");
            return;
        }

        StartCoroutine(SpawnAllDots(position, color));
    }

    private IEnumerator SpawnAllDots(
        Vector3 pos, Color color)
    {
        float logRadius = _col != null
            ? _col.radius * transform.localScale.x
            : 0.5f;

        for (int i = 0; i < dotCount; i++)
        {
            SpawnOneDot(pos, color, logRadius);

            // Spawn 8 chấm mỗi frame
            if (i % 8 == 0)
                yield return null;
        }
    }

    private void SpawnOneDot(
        Vector3 pos, Color color, float logRadius)
    {
        GameObject dot = new GameObject("BossDot");

        // Spawn từ vị trí random trong log
        // Không phải tất cả từ tâm
        Vector2 randomOffset =
            Random.insideUnitCircle * logRadius;
        dot.transform.position =
            pos + new Vector3(
                randomOffset.x, randomOffset.y, 0f);

        SpriteRenderer sr =
            dot.AddComponent<SpriteRenderer>();
        sr.sprite = dotSprite;
        sr.color = color;
        sr.sortingOrder = 10;

        // Size to hơn
        float size = Random.Range(
            logRadius * dotMinSize,
            logRadius * dotMaxSize);
        dot.transform.localScale = Vector3.one * size;

        // Hướng bay random
        Vector2 dir = Random.insideUnitCircle.normalized;
        float speed = Random.Range(dotMinSpeed, dotMaxSpeed);

        StartCoroutine(FlyRoutine(
            dot, sr, dir, speed, dotDuration, color));
    }

    private IEnumerator FlyRoutine(
        GameObject dot,
        SpriteRenderer sr,
        Vector2 dir,
        float speed,
        float duration,
        Color startColor)
    {
        float elapsed = 0f;
        float gravity = Random.Range(2f, 5f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Bay ra
            dot.transform.position +=
                (Vector3)(dir * speed * Time.deltaTime);

            // Rơi xuống
            dot.transform.position +=
                Vector3.down * gravity * t * Time.deltaTime;

            // Mờ dần từ 40%
            if (t > 0.4f)
            {
                float fadeT = (t - 0.4f) / 0.6f;
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, fadeT);
                sr.color = c;
            }

            yield return null;
        }

        Destroy(dot);
    }
}