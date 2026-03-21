using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Hiệu ứng khi dao phi chạm dao cắm:
/// 1. Flash màn hình trắng nhẹ rất nhanh
/// 2. Đốm trắng nở to tại điểm va chạm
/// </summary>
public class KnifeHitEffect : MonoBehaviour
{
    public static KnifeHitEffect Instance { get; private set; }

    [Header("── Flash màn hình ──")]
    [SerializeField] private Image flashScreen;
    [SerializeField] private float flashMaxAlpha = 0.25f; // Mờ nhẹ thôi
    [SerializeField] private int flashFrames = 2;     // Chỉ 2 frames

    [Header("── Đốm trắng ──")]
    [SerializeField] private Sprite dotSprite;    // shop_particles_woodoo_knives_0
    [SerializeField] private float dotStartScale = 0.2f;
    [SerializeField] private float dotEndScale = 2.5f;
    [SerializeField] private int dotFrames = 4;    // 4 frames nở to

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Đảm bảo flash trong suốt ban đầu
        if (flashScreen != null)
        {
            Color c = flashScreen.color;
            c.a = 0f;
            flashScreen.color = c;
        }
    }

    // ═══════════════════════════════════════════
    // PUBLIC — Gọi từ KnifeController
    // ═══════════════════════════════════════════
    public void PlayHitEffect(Vector3 contactPoint)
    {
        StartCoroutine(FlashRoutine());
        StartCoroutine(DotRoutine(contactPoint));
    }

    // ═══════════════════════════════════════════
    // FLASH MÀN HÌNH
    // ═══════════════════════════════════════════
    private IEnumerator FlashRoutine()
    {
        if (flashScreen == null) yield break;

        // Frame 1: Hiện flash
        Color flash = new Color(1f, 1f, 1f, flashMaxAlpha);
        flashScreen.color = flash;
        yield return null;

        // Frame 2: Mờ 50%
        flash.a = flashMaxAlpha * 0.5f;
        flashScreen.color = flash;
        yield return null;

        // Frame 3: Biến mất
        flash.a = 0f;
        flashScreen.color = flash;
    }

    // ═══════════════════════════════════════════
    // ĐỐM TRẮNG NỞ TO
    // ═══════════════════════════════════════════
    private IEnumerator DotRoutine(Vector3 pos)
    {
        if (dotSprite == null) yield break;

        // Tạo object đốm trắng
        GameObject dot = new GameObject("KnifeHitDot");
        dot.transform.position = pos;

        SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
        sr.sprite = dotSprite;
        sr.color = Color.white;
        sr.sortingOrder = 15; // Trên tất cả

        // Frame 1-4: Nở to dần
        for (int i = 0; i <= dotFrames; i++)
        {
            float t = (float)i / dotFrames;
            float scale = Mathf.Lerp(
                dotStartScale, dotEndScale, t);
            float alpha = Mathf.Lerp(1f, 0f, t);

            dot.transform.localScale = Vector3.one * scale;

            Color c = sr.color;
            c.a = alpha;
            sr.color = c;

            yield return null;
        }

        Destroy(dot);
    }
}