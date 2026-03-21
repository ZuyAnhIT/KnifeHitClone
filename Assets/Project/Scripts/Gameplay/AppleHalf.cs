using UnityEngine;
using System.Collections;

public class AppleHalf : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 velocity)
    {
        StartCoroutine(FlyRoutine(velocity));
    }

    private IEnumerator FlyRoutine(Vector2 velocity)
    {
        // Set vận tốc ban đầu
        _rb.gravityScale = 2f;      // Rơi nhanh
        _rb.velocity = velocity;
        _rb.angularVelocity = Random.Range(-300f, 300f);

        // Chờ 0.3s rồi bắt đầu mờ dần
        yield return new WaitForSeconds(0.3f);

        float elapsed = 0f;
        float fadetime = 0.3f;
        Color start = _sr.color;

        while (elapsed < fadetime)
        {
            elapsed += Time.deltaTime;
            Color c = start;
            c.a = Mathf.Lerp(1f, 0f,
                              elapsed / fadetime);
            _sr.color = c;
            yield return null;
        }

        Destroy(gameObject);
    }
}
