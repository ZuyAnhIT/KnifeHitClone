using UnityEngine;
using System.Collections;

public class BreakPiece : MonoBehaviour
{
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    public void Launch(Vector2 direction,
                       float speed,
                       float duration)
    {
        StartCoroutine(FlyRoutine(
            direction, speed, duration));
    }

    private IEnumerator FlyRoutine(Vector2 dir,
                                    float speed,
                                    float duration)
    {
        float rotSpeed = Random.Range(-240f, 240f);
        float elapsed = 0f;
        float gravity = Random.Range(1.5f, 3f);
        Color startColor = _sr != null
            ? _sr.color : Color.white;

        // Random speed nhỏ cho tự nhiên
        speed += Random.Range(-1f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Bay ra ngoài
            transform.position +=
                (Vector3)(dir * speed * Time.deltaTime);

            // Rơi xuống dần
            transform.position += Vector3.down
                * gravity * t * Time.deltaTime;

            // Xoay
            transform.Rotate(0f, 0f,
                rotSpeed * Time.deltaTime);

            // Mờ dần từ 60% thời gian
            if (t > 0.6f && _sr != null)
            {
                float fadeT = (t - 0.6f) / 0.4f;
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, fadeT);
                _sr.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}