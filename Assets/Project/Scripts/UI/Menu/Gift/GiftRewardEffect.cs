using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GiftRewardEffect : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // CÀI ĐẶT
    // ═══════════════════════════════════════════
    [Header("── Sprites ──")]
    [SerializeField] private Sprite appleSprite;
    [SerializeField] private Canvas parentCanvas;

    [Header("── Text ──")]
    [SerializeField] private float textDuration = 1.6f;
    [SerializeField] private float textRiseHeight = 60f;
    [SerializeField] private int fontSize = 90;

    [Header("── Apple Icons ──")]
    [SerializeField] private int appleCount = 35;      // Số táo spawn
    [SerializeField] private float appleIconSize = 52f; // Kích thước nhỏ
    [SerializeField] private float minSpeed = 600f;     // Tốc độ tối thiểu
    [SerializeField] private float maxSpeed = 1100f;    // Tốc độ tối đa
    [SerializeField] private float gravity = 1800f;     // Trọng lực kéo xuống
    [SerializeField] private float appleDuration = 1.4f;

    // ═══════════════════════════════════════════
    // PUBLIC
    // ═══════════════════════════════════════════
    public void Play(int amount, Vector2 spawnPos)
    {
        StartCoroutine(SpawnText(amount, spawnPos));

        for (int i = 0; i < appleCount; i++)
            StartCoroutine(FlyOneApple(spawnPos, i));
    }

    // ═══════════════════════════════════════════
    // TEXT "+N 🍎"
    // ═══════════════════════════════════════════
    private IEnumerator SpawnText(int amount, Vector2 pos)
    {
        GameObject obj = new GameObject("RewardText");
        obj.transform.SetParent(parentCanvas.transform, false);
        obj.transform.SetAsLastSibling();

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(500f, 160f);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = $"+{amount}"; // 🍎
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.yellow;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = new Color32(0, 0, 0, 200);

        float elapsed = 0f;
        Vector2 startPos = pos;
        Vector2 endPos = pos + Vector2.up * textRiseHeight;

        while (elapsed < textDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textDuration;

            // Scale pop: 0 → 1.2 → 1.0 trong 0.35s đầu
            float scaleT = Mathf.Clamp01(elapsed / 0.35f);
            float scale;
            if (scaleT < 0.6f)
                scale = Mathf.Lerp(0f, 1.2f, scaleT / 0.6f);
            else
                scale = Mathf.Lerp(1.2f, 1.0f,
                    (scaleT - 0.6f) / 0.4f);
            obj.transform.localScale = Vector3.one * scale;

            // Bay lên nhẹ
            float moveT = 1f - Mathf.Pow(1f - Mathf.Min(t, 1f), 3f);
            rt.anchoredPosition = Vector2.Lerp(
                startPos, endPos, moveT);

            // Mờ dần từ 55%
            float alpha = t < 0.55f ? 1f
                : Mathf.Lerp(1f, 0f, (t - 0.55f) / 0.45f);
            tmp.color = new Color(1f, 1f, 1f, alpha);

            yield return null;
        }

        Destroy(obj);
    }

    // ═══════════════════════════════════════════
    // MỖI ICON TÁO
    // ═══════════════════════════════════════════
    private IEnumerator FlyOneApple(Vector2 startPos, int index)
    {
        // Delay nhỏ ngẫu nhiên để không spawn cùng 1 frame
        yield return new WaitForSeconds(Random.Range(0f, 0.08f));

        GameObject obj = new GameObject("AppleIcon");
        obj.transform.SetParent(parentCanvas.transform, false);
        obj.transform.SetAsLastSibling();

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = startPos;
        rt.sizeDelta = Vector2.one * appleIconSize
                       * Random.Range(0.75f, 1.25f); // Size ngẫu nhiên nhẹ

        Image img = obj.AddComponent<Image>();
        img.sprite = appleSprite;
        img.preserveAspect = true;

        // Hướng bay: trải đều 360° + random lệch
        float baseAngle = (360f / appleCount) * index;
        float angle = baseAngle + Random.Range(-25f, 25f);
        float rad = angle * Mathf.Deg2Rad;

        // Vận tốc ban đầu
        float speed = Random.Range(minSpeed, maxSpeed);
        Vector2 velocity = new Vector2(
            Mathf.Cos(rad), Mathf.Sin(rad)) * speed;

        float elapsed = 0f;
        Vector2 currentPos = startPos;
        float rotSpeed = Random.Range(-300f, 300f);

        while (elapsed < appleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / appleDuration;

            // Vật lý: vị trí = v*t + 0.5*g*t²
            velocity.y -= gravity * Time.deltaTime;
            currentPos += velocity * Time.deltaTime;
            rt.anchoredPosition = currentPos;

            // Xoay
            obj.transform.localEulerAngles = new Vector3(
                0f, 0f, rotSpeed * elapsed);

            // Scale: pop nhỏ ở đầu
            float scaleT = Mathf.Clamp01(elapsed / 0.12f);
            float scale = Mathf.Lerp(0f, 1f, scaleT);
            obj.transform.localScale = Vector3.one * scale;

            // Alpha: mờ dần từ 50%
            float alpha = t < 0.5f ? 1f
                : Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f);
            img.color = new Color(1f, 1f, 1f, alpha);

            yield return null;
        }

        Destroy(obj);
    }
}